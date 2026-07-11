# Opening シーン素材準備 — ローカル実行計画

**Status:** クラウド側準備完了 → ローカル Claude Code で実行待ち  
**Target Branch:** `claude/opening-scene-assets-3n6cd6`  
**Prepared by:** Claude（Cloud）on 2026-07-11  

---

## 概要

Opening シーン用のビジュアル素材（和文フォント、妖怪スピリット画像、背景除去）を準備し、Unity で統合します。

**分担:**
- **クラウド側（完了）:** ドキュメント、スクリプト、パッチ提案 ✓
- **ローカル側（実行待ち）:** NovelAI 生成、rembg 実行、Unity 統合、テスト

---

## フェーズ 1: フォント導入

### Task 1-1: GenJyuuGothic フォント準備

**担当:** ローカル Claude Code  
**所要時間:** 5-10分

**手順:**

1. **フォントをダウンロード**
   - URL: https://github.com/ButTaiwan/genjyuugothic/releases
   - ファイル: `GenJyuuGothic-Bold.ttf` を最新版からダウンロード

2. **Assets フォルダに配置**
   ```
   C:\Users\あつき\UnityProjects\TsukimonoTan\Assets\Fonts\
   ```

3. **Unity Editor で TextMesh Pro フォント生成**
   - `Assets/Fonts/GenJyuuGothic-Bold.ttf` を右クリック
   - `TextMesh Pro > Create Font Asset`
   - 出力: `Assets/Fonts/GenJyuuGothic-Bold_TMP.asset`

**参考資料:** `Assets/TsukimonoTan/Docs/01_FontSetup.md`

---

## フェーズ 2: 妖怪スピリット画像生成・処理

### Task 2-1: NovelAI で画像生成

**担当:** ローカル（ユーザー）  
**所要時間:** 2-5分

**手順:**

1. **NovelAI にログイン** → https://novelai.net
2. **以下設定で txt2img 実行:**
   - **Prompt（英文）:**
     ```
     beautiful ethereal female yokai spirit, glowing eyes, elegant pose, 
     flowing dark robes, muted colors, dark fantasy art, soft lighting,
     high quality, smooth, detailed
     ```
   - **Size:** 512 × 768
   - **Steps:** 28
   - **Sampler:** Euler
   - **Scale:** 5.0
   - **Seed:** Random

3. **品質確認チェックリスト:**
   - ✓ 妖怪らしい優雅さ
   - ✓ 光る瞳
   - ✓ 流動的なローブ
   - ✓ 落ち着いた色合い（深紫・黒系）
   - ✓ アーティファクト最小限

4. **PNG をダウンロード**
   ```
   ファイル名: spirit_yokai_opening.png
   保存先: C:\Users\あつき\UnityProjects\TsukimonoTan\Assets\TsukimonoTan\Resources\Sprites\Opening\
   ```

**参考資料:** `Assets/TsukimonoTan/Docs/02_AssetGeneration.md`  
**設定ファイル:** `Assets/TsukimonoTan/Docs/NovelAI_OpeningAssets.json`

---

### Task 2-2: rembg で背景除去

**担当:** ローカル Claude Code  
**所要時間:** 1-3分

**前提条件:**

```bash
# Python インストール確認
python --version              # 3.8 以上推奨

# rembg をインストール
pip install rembg[gpu]        # GPU 対応版推奨
# または
pip install rembg             # CPU 版
```

**実行コマンド:**

```bash
cd C:\Users\あつき\UnityProjects\TsukimonoTan\

python Assets/TsukimonoTan/Scripts/Tools/02_RemoveBackground.py \
    --input "Assets/TsukimonoTan/Resources/Sprites/Opening/spirit_yokai_opening.png" \
    --output "Assets/TsukimonoTan/Resources/Sprites/Opening/spirit_yokai_opening_transparent.png"
```

**出力確認:**

```
✓ spirit_yokai_opening_transparent.png が生成される
✓ ファイルサイズ > 0 KB
✓ 画像ビューアで開いて、背景が透過（チェッカーパターン表示）
```

**参考資料:** `Assets/TsukimonoTan/Scripts/Tools/02_RemoveBackground.py`（スクリプト内 docstring）

---

## フェーズ 3: Unity での統合

### Task 3-1: TextMesh Pro フォント割り当て

**担当:** ローカル Claude Code  
**所要時間:** 2-3分

**手順:**

1. **Opening シーンを開く**
   ```
   Assets/TsukimonoTan/Scenes/OpeningScene.unity
   ```

2. **OpeningDirector を選択**
   - Hierarchy: `OpeningDirector`

3. **Inspector で Japanese Font 割り当て**
   - フィールド: `Japanese Font`
   - 値: `GenJyuuGothic-Bold_TMP.asset` をドラッグ＆ドロップ

4. **再生確認**
   ```
   ▶ Play（Ctrl+P）
   ```
   - ナレーション 6 行が正しく表示される（豆腐なし）

---

### Task 3-2: OpeningDirector.cs に Spirit 画像統合

**担当:** ローカル Claude Code  
**所要時間:** 10-15分

**修正内容:**

クラウド側で提案した パッチを適用：

```
ファイル: Assets/TsukimonoTan/Scripts/Opening/OpeningDirector.cs
参考資料: Assets/TsukimonoTan/Docs/03_OpeningDirectorPatch.md
```

**修正ポイント:**
- Step 1: `_spiritImage` フィールド追加
- Step 2: BuildUI() に Spirit Image 生成ロジック
- Step 3: Run() にフェードアニメーション統合
- Step 4: Transition() に Spirit フェードアウト（オプション）
- Step 5: Resources パス確認

**修正チェックリスト:**
- [ ] フィールド追加
- [ ] BuildUI() 修正
- [ ] Run() 修正
- [ ] Transition() 修正（オプション）
- [ ] コンパイルエラーなし
- [ ] Resources パス確認

---

### Task 3-3: Sprite 設定

**担当:** ローカル Claude Code  
**所要時間:** 3-5分

**手順:**

1. **Project ウィンドウで Sprite を選択**
   ```
   Assets/TsukimonoTan/Resources/Sprites/Opening/spirit_yokai_opening_transparent.png
   ```

2. **Inspector で設定**
   - **Texture Type:** Sprite (2D and UI)
   - **Sprite Mode:** Single
   - **Filter Mode:** Point (no filter)
   - **Compression:** None
   - **Alpha Is Transparency:** ON
   - **PPU:** 100

---

### Task 3-4: Opening シーン再生確認

**担当:** ローカル Claude Code  
**所要時間:** 5分

**再生手順:**

1. **シーンを開く**
   ```
   Assets/TsukimonoTan/Scenes/OpeningScene.unity
   ```

2. **▶ Play を実行**

3. **タイムラインをチェック:**

   | 時間 | イベント | ✓ |
   |---|---|---|
   | 0-1s | 黒・静寂 | [ ] |
   | 1-3s | Spirit フェードイン | [ ] |
   | 2-5s | 白符フェードイン | [ ] |
   | 5-10s | 墨パーティクル拡散 | [ ] |
   | 10-20s | ナレーション表示（日本語） | [ ] |
   | 20-25s | タイトル「憑物譚」 | [ ] |
   | 25-30s | 霧フェード→遷移 | [ ] |

4. **スキップテスト**
   - 任意のキー、マウス クリック、タッチで高速遷移 ✓

5. **問題なし → スクリーンショット取得**

---

## フェーズ 4: 最終チェック＆コミット

### Task 4-1: 全体確認

**担当:** ローカル Claude Code  
**所要時間:** 5分

**チェック項目:**

```
[ ] 和文フォント GenJyuuGothic 導入完了
[ ] Spirit 画像（透過 PNG）生成完了
[ ] Opening シーン再生：0-30秒 全て正常
[ ] 日本語テキスト：豆腐なし、レイアウト正常
[ ] Image 合成：Spirit が背景として表示、White Talisman と調和
[ ] スキップ機能：動作確認
```

---

### Task 4-2: Git コミット

**担当:** ローカル Claude Code  
**所要時間:** 2-3分

**コミット内容:**

```
Branch: claude/opening-scene-assets-3n6cd6

Changed Files:
  - Assets/TsukimonoTan/Scripts/Opening/OpeningDirector.cs
  - Assets/TsukimonoTan/Resources/Sprites/Opening/spirit_yokai_opening_transparent.png

Commit Message:
  "Opening シーン: 妖怪スピリット画像統合 + GenJyuuGothic フォント設定"
```

---

## 実行順序

```
┌─────────────────────────────────────────┐
│ Phase 1: フォント導入                    │
│  ├─ Task 1-1: GenJyuuGothic DL + TMP  │
│  └─ 時間: 5-10分                       │
├─────────────────────────────────────────┤
│ Phase 2: 画像生成・処理                  │
│  ├─ Task 2-1: NovelAI 生成             │
│  ├─ Task 2-2: rembg 実行               │
│  └─ 時間: 5-10分                       │
├─────────────────────────────────────────┤
│ Phase 3: Unity 統合                     │
│  ├─ Task 3-1: Font 割り当て             │
│  ├─ Task 3-2: OpeningDirector 修正    │
│  ├─ Task 3-3: Sprite 設定              │
│  ├─ Task 3-4: 再生確認                 │
│  └─ 時間: 20-30分                      │
├─────────────────────────────────────────┤
│ Phase 4: 最終チェック                   │
│  ├─ Task 4-1: 全体確認                 │
│  ├─ Task 4-2: Git コミット             │
│  └─ 時間: 5-10分                       │
├─────────────────────────────────────────┤
│ 合計所要時間: 35-60分                    │
└─────────────────────────────────────────┘
```

---

## トラブルシューティング

| 問題 | 原因 | 解決策 |
|---|---|---|
| 日本語が □ 表示 | フォント未割り当て | Task 3-1 を確認 |
| Spirit 画像が表示されない | Resources パスエラー | Task 3-2 のパス確認 |
| rembg が実行できない | Python/rembg 未インストール | 前提条件確認、pip install rembg |
| OpeningDirector.cs コンパイルエラー | 修正漏れ | Task 3-2 の全ステップ確認 |

---

## 提供ファイル一覧（クラウド側準備済）

```
Assets/TsukimonoTan/
  Docs/
    ✓ 01_FontSetup.md                     ← フォント手順書
    ✓ 02_AssetGeneration.md               ← 画像生成手順書
    ✓ 03_OpeningDirectorPatch.md          ← コード修正ガイド
    ✓ NovelAI_OpeningAssets.json          ← NovelAI 設定ファイル
    ✓ LOCAL_EXECUTION_PLAN.md             ← このファイル
  Scripts/
    Tools/
      ✓ 02_RemoveBackground.py            ← rembg スクリプト

Root/
  ✓ DECISIONS_LOG.md                      ← 意思決定ログ
```

---

## 完了後の通知

実行完了後、以下をローカルで確認・報告してください：

```markdown
## Opening シーン素材準備 — 完了報告

**Status:** ✓ 完了 / [ ] 進行中 / [ ] ブロック

### 実施内容
- [ ] フォント導入（GenJyuuGothic）
- [ ] Spirit 画像生成（NovelAI）
- [ ] 背景除去（rembg）
- [ ] OpeningDirector.cs 統合
- [ ] 再生確認（スクリーンショット）
- [ ] Git コミット

### 問題・気付き
（ある場合は記入）

### スクリーンショット
（Opening 再生画面）
```

---

## 関連リンク

- GitHub リポジトリ: https://github.com/tantou1025-byte/my-projectgate
- Branch: `claude/opening-scene-assets-3n6cd6`
- 本計画書の置き場: `Assets/TsukimonoTan/Docs/LOCAL_EXECUTION_PLAN.md`

