#!/usr/bin/env python3
"""
rembg ラッパースクリプト — Opening シーン用画像背景除去ツール

用途：
    NovelAI txt2img で生成した画像から背景を自動除去し、RGBA PNG として出力する

使用方法：
    python 02_RemoveBackground.py --input <入力画像> --output <出力画像>

例：
    python 02_RemoveBackground.py \
        --input "Assets/TsukimonoTan/Resources/Sprites/Opening/spirit_yokai_opening.png" \
        --output "Assets/TsukimonoTan/Resources/Sprites/Opening/spirit_yokai_opening_transparent.png"

前提条件：
    - Python 3.8 以上
    - rembg インストール済み: pip install rembg[gpu]  または pip install rembg
"""

import argparse
import sys
from pathlib import Path

try:
    from rembg import remove
    from PIL import Image
except ImportError as e:
    print(f"エラー: 必要なモジュールがインストールされていません。")
    print(f"実行: pip install rembg pillow")
    print(f"詳細: {e}")
    sys.exit(1)


def remove_background(input_path: str, output_path: str, model: str = "u2net") -> bool:
    """
    画像から背景を除去し、透過PNG として保存する

    Args:
        input_path (str): 入力画像パス
        output_path (str): 出力画像パス（RGBA PNG）
        model (str): rembg が使用するモデル
            - "u2net"（デフォルト）: バランス型、高速
            - "u2netp"：よりシャープ、処理重い
            - "cloth_seg", "isnet-general-use" など

    Returns:
        bool: 成功時 True、失敗時 False
    """

    input_p = Path(input_path)
    output_p = Path(output_path)

    # 入力ファイル確認
    if not input_p.exists():
        print(f"エラー: 入力ファイルが見つかりません: {input_p}")
        return False

    if not input_p.is_file():
        print(f"エラー: 入力がファイルではありません: {input_p}")
        return False

    # 出力ディレクトリ作成
    output_p.parent.mkdir(parents=True, exist_ok=True)

    print(f"処理中: {input_p}")
    print(f"モデル: {model}")

    try:
        # 入力画像を開く
        input_image = Image.open(input_p).convert("RGBA")
        print(f"入力サイズ: {input_image.size}")

        # 背景除去
        print("背景を除去中...")
        output_image = remove(input_image, model_name=model)

        # 出力画像を保存
        output_image.save(output_p, "PNG", optimize=True)
        print(f"成功: {output_p}")
        print(f"出力サイズ: {output_image.size}")
        print(f"出力ファイルサイズ: {output_p.stat().st_size / 1024:.1f} KB")

        return True

    except FileNotFoundError as e:
        print(f"エラー: ファイルが見つかりません: {e}")
        return False
    except IOError as e:
        print(f"エラー: ファイル I/O エラー: {e}")
        return False
    except Exception as e:
        print(f"エラー: 背景除去処理に失敗しました: {e}")
        print(f"rembg をアップデートしてください: pip install --upgrade rembg")
        return False


def main():
    parser = argparse.ArgumentParser(
        description="rembg ラッパー — Opening シーン用画像背景除去",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
例：
  python 02_RemoveBackground.py \\
    --input spirit_yokai_opening.png \\
    --output spirit_yokai_opening_transparent.png

  python 02_RemoveBackground.py \\
    --input input.png \\
    --output output.png \\
    --model u2netp
        """
    )

    parser.add_argument(
        "--input", "-i",
        required=True,
        help="入力画像ファイルパス（PNG/JPG など）"
    )
    parser.add_argument(
        "--output", "-o",
        required=True,
        help="出力画像ファイルパス（RGBA PNG として保存）"
    )
    parser.add_argument(
        "--model", "-m",
        default="u2net",
        choices=["u2net", "u2netp", "cloth_seg", "isnet-general-use"],
        help="rembg モデル選択（デフォルト: u2net）"
    )

    args = parser.parse_args()

    success = remove_background(args.input, args.output, args.model)
    sys.exit(0 if success else 1)


if __name__ == "__main__":
    main()
