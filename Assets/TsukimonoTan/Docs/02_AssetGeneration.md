# Opening シーン — ビジュアル素材生成手順

## 概要
Opening シーンの背景に配置する「妖怪スピリット」のキャラクター画像を、**NovelAI txt2img** で生成し、**rembg** で背景を除去します。

---

## Step 1: NovelAI で画像生成

### 1-1. NovelAI Web UI にアクセス

- **URL:** https://novelai.net
- **必須:** アカウント登録 & Anlas 残高確認（生成に消費）

### 1-2. txt2img 実行

メニュー: `Image Generation > Generate`

**プロンプト (英語):**

```
beautiful ethereal female yokai spirit, glowing eyes, elegant pose, 
flowing dark robes, muted colors, dark fantasy art, soft lighting,
high quality, smooth, detailed
```

**生成設定（Settings パネル）:**

| 項目 | 値 | 理由 |
|---|---|---|
| **Size** | `512 x 768` | Opening 背景縦長対応（または 400×600） |
| **Steps** | 28 | 品質と Anlas コスト のバランス |
| **Sampler** | Euler | 安定・確実な品質 |
| **Scale** | 5.0 | プロンプト忠実度（高すぎると歪む） |
| **Seed** | Random | 毎回異なる画像 |
| **Batch Count** | 1 | 複数案検討時は 3-4 に増やす |
| **Negative Prompt** | （未設定 / あれば watermark, blurry） | |

### 1-3. 生成と確認

- **生成時間:** 20-30秒
- **出力形式:** PNG
- **品質チェック:**
  - ✓ 妖怪らしい優雅さ
  - ✓ 目が光っている
  - ✓ ローブが流動的
  - ✓ 色が落ち着いている（深紫・黒・赤系）
  - ✓ アーティファクト（歪み）が最小限

**不満足な場合：**
- Seed を変更して再生成
- Prompt を微調整（例：「glowing red eyes」「black robes」追加）
- Steps を 36 に増やす（Anlas ×1.5）

### 1-4. ファイル保存

PNG をダウンロード → ローカル保存パス:

```
C:\Users\[ユーザー名]\UnityProjects\TsukimonoTan\Assets\TsukimonoTan\Resources\Sprites\Opening\
```

**ファイル名:** `spirit_yokai_opening.png`

---

## Step 2: rembg で背景除去

### 2-1. 環境確認

Python と rembg がインストール済みか確認：

```bash
# ターミナル/CMD で実行
python --version              # Python 3.8 以上推奨
pip show rembg                # インストール済みなら情報表示
```

**未インストール時:**

```bash
pip install rembg[gpu]        # GPU対応版（推奨。CUDA搭載PC）
# または
pip install rembg             # CPU版
```

### 2-2. rembg スクリプト実行

リポジトリに含まれるスクリプトを使用：

```bash
cd C:\Users\[ユーザー名]\UnityProjects\TsukimonoTan\

python Assets/TsukimonoTan/Scripts/Tools/02_RemoveBackground.py \
    --input "Assets/TsukimonoTan/Resources/Sprites/Opening/spirit_yokai_opening.png" \
    --output "Assets/TsukimonoTan/Resources/Sprites/Opening/spirit_yokai_opening_transparent.png"
```

**処理内容:**
- 入力 PNG から背景を自動検出・削除
- RGBA PNG（透過背景）として出力
- 処理時間：30秒～2分（モデルダウンロード初回は +5分）

### 2-3. 出力確認

出力ファイル:

```
Assets/TsukimonoTan/Resources/Sprites/Opening/spirit_yokai_opening_transparent.png
```

**確認項目:**
- ✓ ファイルが生成されている
- ✓ ファイルサイズ > 0 KB
- ✓ 画像ビューアで開いて、妖怪のシルエットのみ（背景透過）

**背景除去失敗時:**
- 入力画像を確認（破損、サイズ不適切なし）
- rembg をアップデート: `pip install --upgrade rembg`
- 再実行

---

## Step 3: Unity へのインポート

### 3-1. Sprite 設定

**Project ウィンドウ:**

1. `Assets/TsukimonoTan/Resources/Sprites/Opening/spirit_yokai_opening_transparent.png` を選択
2. **Inspector > Sprite (2D and UI):**
   - **Texture Type:** Sprite (2D and UI)
   - **Sprite Mode:** Single
   - **Filter Mode:** Point (no filter) ← **重要：ピクセルアート風**
   - **Compression:** None
   - **Alpha Is Transparency:** ON
   - **PPU (Pixels Per Unit):** 100 ← フィット時に調整

### 3-2. OpeningDirector.cs への統合

スクリプト更新は別ファイル `03_OpeningDirectorPatch.md` を参照。

---

## トラブルシューティング

### rembg が Python モジュールとして見つからない

```bash
# インストール確認
pip list | grep rembg

# 明示的インストール
pip install --user rembg
```

### 生成画像がボケている / 色が不自然

**NovelAI 設定を調整:**
- Steps: 36（費用増加）
- Scale: 4.0 → 6.0
- Sampler: DPM++ 2M Karras に変更

### 背景除去後、妖怪の輪郭がギザギザ

rembg の精度限界。手動で Photoshop / GIMP で調整、または：

```bash
# 別モデル試験（U2Net より精密）
pip install rembg
# rembg スクリプト内で model='u2netp' 指定
```

---

## ファイル構成

```
Assets/TsukimonoTan/
  Docs/
    01_FontSetup.md                     ← フォント導入
    02_AssetGeneration.md               ← このファイル
    03_OpeningDirectorPatch.md          ← コード更新
  Resources/Sprites/Opening/
    spirit_yokai_opening.png            ← NovelAI 出力（元画像）
    spirit_yokai_opening_transparent.png ← rembg 出力（使用ファイル）
  Scripts/
    Opening/
      OpeningDirector.cs                ← 修正対象
    Tools/
      02_RemoveBackground.py            ← rembg ラッパースクリプト
```

---

## 関連リンク

| リソース | URL |
|---|---|
| NovelAI Official | https://novelai.net |
| GenJyuuGothic Font | https://github.com/ButTaiwan/genjyuugothic |
| rembg GitHub | https://github.com/danielgatis/rembg |
| Unity Sprite Import | https://docs.unity3d.com/Manual/Sprites.html |

