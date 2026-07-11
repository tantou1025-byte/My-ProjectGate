# Opening シーン素材準備 — 完全ガイド

**Status:** クラウド側準備完了 ✓ | ローカル実行待機中  
**Branch:** `claude/opening-scene-assets-3n6cd6`  
**Last Updated:** 2026-07-11  

---

## クイックリンク

| ドキュメント | 対象 | 概要 |
|---|---|---|
| **LOCAL_EXECUTION_PLAN.md** | ローカル Claude Code | 実行手順＆チェックリスト（最初に読む） |
| **01_FontSetup.md** | ローカル | GenJyuuGothic フォント導入手順 |
| **02_AssetGeneration.md** | ローカル + ユーザー | NovelAI 画像生成・rembg 背順書 |
| **03_OpeningDirectorPatch.md** | ローカル | OpeningDirector.cs 修正ガイド |
| **NovelAI_OpeningAssets.json** | 参考資料 | NovelAI 生成設定（JSON） |
| **DECISIONS_LOG.md** | 記録 | 意思決定ログ（リポジトリルート） |

---

## 全体フロー図

```
┌──────────────────────────────────────────────────────────────┐
│                    Opening シーン素材準備                      │
│                  (claude/opening-scene-assets)               │
└──────────────────────────────────────────────────────────────┘

┌─ PHASE 1: フォント ─────────────────────────────────────────┐
│                                                              │
│  [Cloud] ✓ 手順書 (01_FontSetup.md)                         │
│    ↓                                                         │
│  [Local] GenJyuuGothic ダウンロード → TMP 生成             │
│    ↓                                                         │
│  [Local] OpeningDirector に割り当て                         │
│                                                              │
└────────────────────────────────────────────────────────────┘

┌─ PHASE 2: 画像生成・処理 ──────────────────────────────────┐
│                                                              │
│  [Cloud] ✓ 手順書 (02_AssetGeneration.md)                  │
│  [Cloud] ✓ NovelAI 設定 (NovelAI_OpeningAssets.json)       │
│  [Cloud] ✓ rembg スクリプト (02_RemoveBackground.py)      │
│    ↓                                                         │
│  [User] NovelAI で画像生成                                 │
│    ↓                                                         │
│  [Local] rembg 実行 → 背景除去                             │
│                                                              │
└────────────────────────────────────────────────────────────┘

┌─ PHASE 3: Unity 統合 ──────────────────────────────────────┐
│                                                              │
│  [Cloud] ✓ パッチガイド (03_OpeningDirectorPatch.md)       │
│    ↓                                                         │
│  [Local] OpeningDirector.cs 修正                           │
│    ↓                                                         │
│  [Local] Spirit Sprite 設定                                │
│    ↓                                                         │
│  [Local] 再生確認＆スクリーンショット                       │
│                                                              │
└────────────────────────────────────────────────────────────┘

┌─ PHASE 4: 最終チェック ────────────────────────────────────┐
│                                                              │
│  [Local] 全体テスト                                         │
│    ↓                                                         │
│  [Local] Git コミット                                       │
│                                                              │
└────────────────────────────────────────────────────────────┘
```

---

## ファイル構成

```
Assets/TsukimonoTan/
├── Docs/
│   ├── 01_FontSetup.md                      [フォント導入手順]
│   ├── 02_AssetGeneration.md                [画像生成手順]
│   ├── 03_OpeningDirectorPatch.md           [コード修正ガイド]
│   ├── NovelAI_OpeningAssets.json           [NovelAI 設定]
│   ├── LOCAL_EXECUTION_PLAN.md              [実行計画＆チェックリスト]
│   └── README_AssetPreparation.md           [このファイル]
├── Resources/
│   └── Sprites/
│       └── Opening/
│           ├── spirit_yokai_opening.png               [要生成：NovelAI]
│           └── spirit_yokai_opening_transparent.png   [要生成：rembg]
├── Scripts/
│   ├── Opening/
│   │   ├── OpeningDirector.cs               [修正対象]
│   │   ├── OpeningInkParticles.cs           [既存：変更なし]
│   │   └── README_Opening.md                [既存：参考]
│   └── Tools/
│       └── 02_RemoveBackground.py           [rembg ラッパースクリプト]
└── ...

Root/
└── DECISIONS_LOG.md                         [意思決定ログ]
```

---

## 準備状況

### ✓ クラウド側（完了）

- ✓ **手順書群**
  - 01_FontSetup.md — GenJyuuGothic 導入
  - 02_AssetGeneration.md — NovelAI + rembg
  - 03_OpeningDirectorPatch.md — コード修正
  - LOCAL_EXECUTION_PLAN.md — 実行計画

- ✓ **スクリプト**
  - 02_RemoveBackground.py — rembg ラッパー（実行可能）

- ✓ **設定ファイル**
  - NovelAI_OpeningAssets.json — JSON 形式の設定（参考用）

- ✓ **記録**
  - DECISIONS_LOG.md — 意思決定ログ
  - README_AssetPreparation.md — このファイル

### ⏳ ローカル側（実行待機）

- [ ] **Phase 1: フォント導入** 所要時間：5-10分
  - GenJyuuGothic ダウンロード → TMP 生成 → OpeningDirector 割り当て

- [ ] **Phase 2: 画像生成・処理** 所要時間：10-15分
  - NovelAI で yokai スピリット生成
  - rembg で背景除去

- [ ] **Phase 3: Unity 統合** 所要時間：20-30分
  - フォント割り当て確認
  - OpeningDirector.cs 修正
  - Sprite 設定
  - 再生確認

- [ ] **Phase 4: 最終チェック** 所要時間：5-10分
  - 全体テスト
  - Git コミット

**合計所要時間:** 40-65分

---

## スタートガイド

### ローカル Claude Code の場合：

```
1. LOCAL_EXECUTION_PLAN.md を開く
2. Phase 1 から順に実行
3. 各タスク内の参考資料を確認
4. チェックリストを埋める
5. 完了後、コミット
```

### ユーザーの場合（NovelAI 生成のみ）：

```
1. Assets/TsukimonoTan/Docs/02_AssetGeneration.md
   → "Step 1: NovelAI で画像生成" を実行
2. ファイルを保存：
   Assets/TsukimonoTan/Resources/Sprites/Opening/spirit_yokai_opening.png
3. ローカル Claude Code に通知
```

---

## よくある質問

### Q: フォント「源暎ゴシック」の代わりに別フォントは使える？

**A:** 可能ですが、推奨は GenJyuuGothic-Bold です。理由：
- 和風ダークファンタジーに適した Gothic 太字
- 無料
- テスト済み

別フォント使用時は、`01_FontSetup.md` の「フォント選択」セクションを参照。

---

### Q: NovelAI アカウントがない場合は？

**A:** 以下の代替案があります：
1. Anlas 残高を別途購入（クレジットカード / Steam ウォレット）
2. 別の txt2img サービス（Stable Diffusion など）を使用
   - ただし、Opening 統合コードは OpeningDirector.cs に spirit_yokai_opening_transparent.png を Resources.Load するため、ファイル形式・名前を統一してください

---

### Q: rembg がインストール失敗した場合は？

**A:** 以下を確認：
1. Python 3.8 以上がインストール済みか
2. `pip install rembg` の実行（GPU 版なら `pip install rembg[gpu]`）
3. ファイアウォール / プロキシ設定

詳細は `02_AssetGeneration.md` → "Step 2: rembg" セクション参照。

---

### Q: OpeningDirector.cs の修正で失敗した場合は？

**A:** 以下を確認：
1. `03_OpeningDirectorPatch.md` の全 Step を実行したか
2. コンパイルエラーログを確認
3. Resources パス `"Sprites/Opening/spirit_yokai_opening_transparent"` が正しいか

エラー内容をコピーして、ローカル Claude Code に相談。

---

## 連絡先 / サポート

問題発生時：
- **コンパイルエラー** → ローカル Claude Code に相談
- **Unity 再生エラー** → `03_OpeningDirectorPatch.md` のトラブルシューティング参照
- **NovelAI / rembg** → 各ドキュメントのトラブルシューティング参照

---

## 参考資料

| リソース | URL |
|---|---|
| NovelAI Official | https://novelai.net |
| GenJyuuGothic | https://github.com/ButTaiwan/genjyuugothic |
| rembg GitHub | https://github.com/danielgatis/rembg |
| Unity TextMesh Pro | https://docs.unity3d.com/Manual/TextMeshProUGUI.html |
| Unity Sprite | https://docs.unity3d.com/Manual/Sprites.html |

---

## ログ

- **2026-07-11（ローカル実行計画作成）**
  - Cloud 側ドキュメント・スクリプト準備完了
  - ローカル実行待機中

