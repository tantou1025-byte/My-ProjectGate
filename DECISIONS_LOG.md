# 意思決定ログ

## Session: Opening シーン素材準備（claude/opening-scene-assets-3n6cd6）
**Date:** 2026-07-11  
**Decision:** Option A — クラウド側でドキュメント・スクリプト準備 → ローカル Claude Code が実行

### 背景
Opening シーン用の以下タスクがある：
1. 和文フォント（GenJyuuGothic）の TextMesh Pro 導入
2. NovelAI で yokai スピリット画像生成
3. rembg で背景除去
4. Unity での TextMesh Pro テキスト＆イラスト設定

### 判断理由
- クラウド環境：Unity Editor GUI、NovelAI web UI にアクセス不可
- ローカルファイルシステム（Windows パス）にアクセス不可
- ダウンロード、スクリプト作成、ドキュメント化はクラウド側で対応可能

### アプローチ
| 作業 | 担当 | 形式 |
|---|---|---|
| フォント導入手順書 | クラウド | Markdown ドキュメント |
| rembg スクリプト整備 | クラウド | Python スクリプト |
| NovelAI プロンプト・設定 | クラウド | JSON/Markdown 設定ファイル |
| OpeningDirector.cs 更新準備 | クラウド | C# パッチ/提案 |
| ローカル実行（Font Manager、NovelAI、rembg、Unity 設定） | ローカル Claude Code | 実装 |

### 次ステップ
1. ✓ DECISIONS_LOG.md に記録（2026-07-11）
2. ✓ フォント導入手順書作成（`Assets/TsukimonoTan/Docs/01_FontSetup.md`）
3. ✓ rembg スクリプト作成（`Assets/TsukimonoTan/Scripts/Tools/02_RemoveBackground.py`）
4. ✓ NovelAI 生成設定ファイル作成（`Assets/TsukimonoTan/Docs/NovelAI_OpeningAssets.json`）
5. ✓ OpeningDirector.cs 修正ガイド作成（`Assets/TsukimonoTan/Docs/03_OpeningDirectorPatch.md`）
6. ✓ ローカル実行計画書作成（`Assets/TsukimonoTan/Docs/LOCAL_EXECUTION_PLAN.md`）

### クラウド側準備完了（2026-07-11）
- ✓ フォント導入手順書（01_FontSetup.md）
- ✓ 画像生成・背景除去手順書（02_AssetGeneration.md）
- ✓ rembg Python スクリプト（02_RemoveBackground.py）
- ✓ NovelAI 設定ファイル（NovelAI_OpeningAssets.json）
- ✓ OpeningDirector.cs パッチガイド（03_OpeningDirectorPatch.md）
- ✓ ローカル実行計画（LOCAL_EXECUTION_PLAN.md）

### 待機中：ローカル Claude Code / ユーザー実行
- [ ] Phase 1: GenJyuuGothic フォント導入
- [ ] Phase 2: NovelAI 画像生成 + rembg 背景除去
- [ ] Phase 3: Unity 統合（フォント割り当て、OpeningDirector.cs 修正）
- [ ] Phase 4: 再生確認＆Git コミット
