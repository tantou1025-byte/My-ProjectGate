#!/usr/bin/env python3
"""
バトル用デフォルメ立ち絵 一括生成スクリプト（Phase 2A 実行体）

生成プロンプト設定_全150体.csv の元プロンプトに「ちびキャラ・デフォルメ」タグを
付加して NovelAI txt2img で生成し、rembg で背景を除去して透過PNGとして保存する。

Anlas消費ゼロ条件（Opusプラン・厳守）:
  ① 1枚ずつ逐次生成（n_samples=1、バッチ不可）
  ② 解像度 1024x1024 以下
  ③ Steps 28 以下

使い方:
  # 事前準備
  pip install requests rembg pillow
  set NOVELAI_API_TOKEN=<永続APIトークン>   (Windows) / export ... (Unix)

  # まず3体だけテスト（背景除去品質の確認用）
  python generate_battle_sprites.py --csv 生成プロンプト設定_全150体.csv --limit 3

  # 品質OKなら全件実行（生成済みはスキップされるので再実行=続きから再開）
  python generate_battle_sprites.py --csv 生成プロンプト設定_全150体.csv

出力:
  Assets/TsukimonoTan/Resources/CreatureSprites/Battle/creature_{id:03d}_battle.png  (透過)
  Assets/TsukimonoTan/Resources/CreatureSprites/Battle/raw/creature_{id:03d}_raw.png (生成原本)
  generation_log_battle.jsonl  (1行1件の生成ログ)
"""

import argparse
import base64
import csv
import io
import json
import os
import random
import sys
import time
import zipfile
from pathlib import Path

import requests

# 生成エンドポイント（新ホスト優先、旧ホストへフォールバック）
NOVELAI_ENDPOINTS = [
    "https://image.novelai.net/ai/generate-image",
    "https://api.novelai.net/ai/generate-image",
]

DEFORM_TAGS = "chibi, deformed, full body, simple background, standing, game sprite"
DEFAULT_NEGATIVE = (
    "nsfw, lowres, bad anatomy, error, extra digits, fewer digits, cropped, "
    "worst quality, low quality, jpeg artifacts, signature, watermark, "
    "username, blurry, text"
)

# CSVカラム名の候補（実CSVの列名ゆらぎに対応。--id-col等で明示指定も可能）
ID_COL_CANDIDATES = ["id", "ID", "番号", "No", "no"]
NAME_COL_CANDIDATES = ["name", "名前", "憑物名", "creature_name"]
PROMPT_COL_CANDIDATES = ["prompt", "プロンプト", "元プロンプト", "元プロンプトタグ", "タグ", "tags"]


def pick_column(fieldnames, candidates, explicit, kind):
    if explicit:
        if explicit not in fieldnames:
            sys.exit(f"エラー: 指定カラム '{explicit}' がCSVにありません。実際の列: {fieldnames}")
        return explicit
    for c in candidates:
        if c in fieldnames:
            return c
    sys.exit(
        f"エラー: {kind} カラムを自動判定できません。実際の列: {fieldnames}\n"
        f"--{kind}-col で列名を明示指定してください。"
    )


def load_creatures(csv_path, id_col, name_col, prompt_col):
    for encoding in ("utf-8-sig", "cp932", "utf-8"):
        try:
            with open(csv_path, newline="", encoding=encoding) as f:
                reader = csv.DictReader(f)
                fields = reader.fieldnames or []
                cid = pick_column(fields, ID_COL_CANDIDATES, id_col, "id")
                cname = pick_column(fields, NAME_COL_CANDIDATES, name_col, "name")
                cprompt = pick_column(fields, PROMPT_COL_CANDIDATES, prompt_col, "prompt")
                rows = []
                for row in reader:
                    raw_id = (row.get(cid) or "").strip()
                    if not raw_id:
                        continue
                    rows.append({
                        "id": int(raw_id),
                        "name": (row.get(cname) or "").strip(),
                        "prompt": (row.get(cprompt) or "").strip(),
                    })
                return rows
        except UnicodeDecodeError:
            continue
    sys.exit(f"エラー: {csv_path} を utf-8 / cp932 のいずれでも読めませんでした。")


def request_image(token, prompt, negative, steps, size, seed, timeout=180):
    """NovelAI txt2img。応答はZIP(中にPNG)またはJSON(base64)の両形式に対応する。"""
    payload = {
        "input": prompt,
        "model": "nai-diffusion-3",
        "action": "generate",
        "parameters": {
            "width": size,
            "height": size,
            "scale": 5.5,
            "sampler": "k_euler_ancestral",
            "steps": steps,          # Anlas0条件: 28以下
            "n_samples": 1,          # Anlas0条件: 1枚ずつ
            "seed": seed,
            "ucPreset": 0,
            "qualityToggle": True,
            "negative_prompt": negative,
        },
    }
    headers = {"Authorization": f"Bearer {token}", "Content-Type": "application/json"}

    last_error = None
    for endpoint in NOVELAI_ENDPOINTS:
        try:
            res = requests.post(endpoint, json=payload, headers=headers, timeout=timeout)
        except requests.RequestException as e:
            last_error = f"{endpoint}: {e}"
            continue
        if res.status_code != 200:
            last_error = f"{endpoint}: HTTP {res.status_code} {res.text[:200]}"
            continue

        content_type = res.headers.get("Content-Type", "")
        if "zip" in content_type or res.content[:2] == b"PK":
            with zipfile.ZipFile(io.BytesIO(res.content)) as zf:
                names = zf.namelist()
                if not names:
                    last_error = f"{endpoint}: ZIPが空"
                    continue
                return zf.read(names[0])
        try:
            body = res.json()
            if "output" in body:
                return base64.b64decode(body["output"])
        except (ValueError, KeyError):
            pass
        last_error = f"{endpoint}: 未知の応答形式 (Content-Type={content_type})"
    raise RuntimeError(f"全エンドポイントで生成失敗: {last_error}")


def remove_background(raw_png_bytes):
    from rembg import remove  # 遅延import（未インストール時に生成前へ到達させない）
    return remove(raw_png_bytes)


def append_log(log_path, entry):
    with open(log_path, "a", encoding="utf-8") as f:
        f.write(json.dumps(entry, ensure_ascii=False) + "\n")


def main():
    parser = argparse.ArgumentParser(description="バトル用デフォルメ立ち絵の一括生成")
    parser.add_argument("--csv", required=True, help="生成プロンプト設定_全150体.csv のパス")
    parser.add_argument("--out", default="Assets/TsukimonoTan/Resources/CreatureSprites/Battle",
                        help="出力ディレクトリ")
    parser.add_argument("--limit", type=int, default=0, help="先頭からN体のみ生成（テスト用）")
    parser.add_argument("--start-id", type=int, default=0, help="このID以降のみ生成")
    parser.add_argument("--steps", type=int, default=28)
    parser.add_argument("--size", type=int, default=1024)
    parser.add_argument("--delay", type=float, default=1.5, help="リクエスト間の待機秒数")
    parser.add_argument("--negative", default=DEFAULT_NEGATIVE)
    parser.add_argument("--id-col", dest="id_col", default=None)
    parser.add_argument("--name-col", dest="name_col", default=None)
    parser.add_argument("--prompt-col", dest="prompt_col", default=None)
    parser.add_argument("--skip-rembg", action="store_true",
                        help="背景除去を行わず生成原本のみ保存（切り分け用）")
    args = parser.parse_args()

    # Anlas消費ゼロ条件のガード（外れた設定を実行前に止める）
    if args.steps > 28:
        sys.exit(f"エラー: steps={args.steps} はAnlas消費ゼロ条件(28以下)を超えています。")
    if args.size > 1024:
        sys.exit(f"エラー: size={args.size} はAnlas消費ゼロ条件(1024以下)を超えています。")

    token = os.environ.get("NOVELAI_API_TOKEN")
    if not token:
        sys.exit("エラー: 環境変数 NOVELAI_API_TOKEN が未設定です（永続APIトークン）。")

    out_dir = Path(args.out)
    raw_dir = out_dir / "raw"
    raw_dir.mkdir(parents=True, exist_ok=True)
    log_path = "generation_log_battle.jsonl"

    creatures = load_creatures(args.csv, args.id_col, args.name_col, args.prompt_col)
    if args.start_id:
        creatures = [c for c in creatures if c["id"] >= args.start_id]
    if args.limit:
        creatures = creatures[: args.limit]

    print(f"対象: {len(creatures)}体 / 出力先: {out_dir}")
    done = failed = skipped = 0

    for creature in creatures:
        cid, cname = creature["id"], creature["name"]
        out_path = out_dir / f"creature_{cid:03d}_battle.png"
        raw_path = raw_dir / f"creature_{cid:03d}_raw.png"

        if out_path.exists():
            skipped += 1
            continue
        if not creature["prompt"]:
            print(f"  [{cid:03d}] {cname}: プロンプト空のためスキップ")
            append_log(log_path, {"id": cid, "name": cname, "status": "empty_prompt"})
            failed += 1
            continue

        prompt = f"{creature['prompt']}, {DEFORM_TAGS}"
        seed = random.randint(0, 2**32 - 1)
        try:
            print(f"  [{cid:03d}] {cname}: 生成中...", flush=True)
            raw_bytes = request_image(token, prompt, args.negative, args.steps, args.size, seed)
            raw_path.write_bytes(raw_bytes)

            if args.skip_rembg:
                out_bytes = raw_bytes
            else:
                out_bytes = remove_background(raw_bytes)
            out_path.write_bytes(out_bytes)

            append_log(log_path, {
                "id": cid, "name": cname, "prompt": prompt, "seed": seed,
                "output": out_path.name, "status": "success",
            })
            done += 1
        except Exception as e:  # 1体の失敗で全体を止めない。再実行で続きから再開できる
            print(f"  [{cid:03d}] {cname}: 失敗 - {e}")
            append_log(log_path, {
                "id": cid, "name": cname, "prompt": prompt, "seed": seed,
                "status": "error", "error": str(e),
            })
            failed += 1

        time.sleep(args.delay)

    print(f"\n完了: 成功{done} / スキップ(既存){skipped} / 失敗{failed}")
    if failed:
        print("失敗分は同じコマンドの再実行で続きから再試行できます。"
              f"詳細は {log_path} を参照。")


if __name__ == "__main__":
    main()
