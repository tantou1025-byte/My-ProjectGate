# 憑物譚（つきものたん）— チャット引き継ぎ書 v7.0

このドキュメントを新しいチャットに貼り付けて「このプロジェクトの続きです」と伝えてください。
前提となる詳細は `handoff_v6.md`（無くてもこの1枚で作業再開可能。ただし §2 の環境ルールは必読）。

---

## 1. プロジェクト概要（最小限）

- FANZA/DLsite向け、和風ダークファンタジーのモンスター収集×絆育成×アダルトRPG
- 150体の「憑物」、契約（捕獲）、絆度育成、契約変質（進化）、8種エンディング
- 開発環境：Unity 6 LTS（6000.3.15f1）、URP、Input Systemは**新方式のみ**（旧Inputクラス使用不可）
- カラーパレット確定：深紫黒 `#0a0e27` / 金 `#d4af37` / 紅 `#c41e3a`（＋和紙白 `#f5f0e6`）
- リリース目標：1ヶ月以内、最小限＆無駄なし

## 2. 【最重要】作業体制のルール — 3つの環境がある

| 環境 | 役割 | できること・できないこと |
|---|---|---|
| ローカルUnityプロジェクト | 実体 | `C:\Users\あつき\UnityProjects\TsukimonoTan\`。全アセット・全スクリプトはここにある |
| ローカルのClaude Code CLI | **実装担当（正）** | ローカルファイルを直接編集できる。独自のエディタメニュー群を実装済み |
| クラウドのClaude（このチャット系） | 設計・相談担当 | **ローカルPCのファイルは一切見えない・触れない**。GitHubの`tantou1025-byte/my-projectgate`リポジトリのみ操作可能 |

**次のチャットが守るべきルール：**
1. ローカルのファイルパスやUnityメニューの位置を、確認せずに断定して案内しない。不明ならスクリーンショットか、ローカルClaude Codeでの確認結果を求めること
2. 実装作業はローカルClaude Codeに依頼文を書いて渡す形が確実。クラウド側は設計・コードレビュー・相談に徹する
3. Unityの「TsukimonoTan」メニューは**画面上部メニューバー**（Jobs と Window の間）。右クリック→Create→TsukimonoTan はScriptableObject作成用で別物
4. NovelAI txt2img・Unity Editor操作など、クラウド環境で実行不可能な作業は、**ドキュメント・スクリプトの準備までをクラウド側の役割とし、実行はローカル担当（Claude Code CLIまたはユーザー本人）に依頼する**分担が今回確立された（§3参照）

## 3. Opening シーン素材準備 — 今回セッションの主作業

### 3-1. 経緯・意思決定

- ブランチ `claude/opening-scene-assets-3n6cd6` で作業
- 依頼内容：①TextMesh Pro用和文フォント導入、②NovelAIでのイラスト生成、③rembgでの背景除去、④UnityでのTMP/イラスト設定、⑤OpeningDirector.cs更新
- **クラウド環境の制約**（Unity Editor GUI操作・NovelAI Web UI・ローカルファイルシステム不可）が判明したため、以下の分担案を採用し `DECISIONS_LOG.md` に記録：
  - **クラウド側：** ドキュメント・スクリプト・コード修正ガイドの準備のみ
  - **ローカル側（Claude Code CLIまたはユーザー）：** フォントDL、NovelAI生成、rembg実行、Unity実操作

### 3-2. クラウド側で完成した成果物（コミット `ace254e`、リポジトリにpush済み）

| ファイル | 内容 |
|---|---|
| `Assets/TsukimonoTan/Docs/README_AssetPreparation.md` | **全体ガイド（最初に読む）**。フロー図・ファイル構成・FAQ |
| `Assets/TsukimonoTan/Docs/LOCAL_EXECUTION_PLAN.md` | **ローカル実行計画（実務の起点）**。Phase1〜4のタスク分解・チェックリスト・所要時間目安（合計40〜65分） |
| `Assets/TsukimonoTan/Docs/01_FontSetup.md` | GenJyuuGothic（源暎ゴシック）フォント導入手順。DL先URL・配置パス・TMP Font Asset生成手順・トラブルシューティング |
| `Assets/TsukimonoTan/Docs/02_AssetGeneration.md` | NovelAI txt2img手順（プロンプト・設定値）＋rembg実行手順 |
| `Assets/TsukimonoTan/Docs/NovelAI_OpeningAssets.json` | NovelAI生成設定のJSON化（プロンプト、サイズ512×768、Steps28、Sampler Euler、Scale5.0等）＋再生成時の調整ガイド |
| `Assets/TsukimonoTan/Docs/03_OpeningDirectorPatch.md` | `OpeningDirector.cs`への妖怪スピリット画像統合パッチ手順（Step1〜5、コード差分つき） |
| `Assets/TsukimonoTan/Scripts/Tools/02_RemoveBackground.py` | rembgラッパースクリプト（実行可能、`--input`/`--output`/`--model`引数対応） |
| `DECISIONS_LOG.md`（リポジトリルート） | 分担方針の意思決定記録 |

### 3-3. 未実施（ローカル側の実行待ち）— 次チャットの最初の確認事項

`LOCAL_EXECUTION_PLAN.md` のPhase構成に対応：

1. **Phase 1（5-10分）：** GenJyuuGothic-Bold.ttf をDL → `Assets/Fonts/`に配置 → TMP Font Asset生成（`GenJyuuGothic-Bold_TMP`）
2. **Phase 2（10-15分）：** NovelAIで妖怪スピリット画像生成（`spirit_yokai_opening.png`）→ rembgで背景除去（`spirit_yokai_opening_transparent.png`）→ `Assets/TsukimonoTan/Resources/Sprites/Opening/`に配置
3. **Phase 3（20-30分）：** OpeningDirectorへのフォント割り当て／`OpeningDirector.cs`にSpirit画像表示ロジックを追加（`03_OpeningDirectorPatch.md`のパッチ適用）／Sprite Importer設定／シーン再生確認
4. **Phase 4（5-10分）：** 全体チェックリスト消化 → Git commit

**重要：** 3-2の成果物はあくまで「手順書・スクリプト」であり、実際のフォントファイル・画像ファイル・コード修正はまだ**何もローカルに反映されていない**。次チャット（ローカルClaude Code想定）はこの引き継ぎ書を読んだら、まず `LOCAL_EXECUTION_PLAN.md` を開いてPhase 1から着手すること。

### 3-4. §3以前（v6時点）のOpening状況との関係

- v6で言及されていた「ローカル版が正、GitHub版は使わない」「robocopyで重複コピーした可能性」の論点（v6 §3）とは**別レイヤーの作業**。今回はローカルに既に存在する`OpeningDirector.cs`（v6で正とされたもの）に対して、素材追加のパッチを当てる想定
- v6 §3の未完了タスク（Openingの▶Play動作確認、重複スクリプト削除）が済んでいない場合は、今回のPhase 3着手前に解消しておくこと

## 4. フィールドタイル素材の確定事項（v6から変更なし・据え置き）

### 素材の状態
- 4種の地形タイル完成：砂/土（黄）、雪/空色（青紫）、石畳（緑灰）、水（青緑）。SeamlessItでタイル化処理中〜完了
- **残作業①**：砂/土タイルだけ非正方形（約890×1024）→ 正方形にクロップ必要
- **残作業②**：4枚とも1024px級で画面表示（1マス100〜150px）に対し大きすぎる → **全タイルを256×256へ事前縮小**してからUnityに入れる（縮小はボックス/面積平均補間。バイキュービック滑らかは不可）
- **水タイルは進入不可**（歩行音プロファイル不要）

### Texture Importer 確定設定（タイル用）
- Filter Mode: `Point (no filter)` ／ Compression: `None` ／ Mip Maps: OFF ／ Wrap Mode: `Clamp`
- Alpha Is Transparency: ON ／ Read/Write: OFF ／ Physics Shape: OFF
- **PPU = 256、Max Size = 256**
- Grid: Rectangle、Cell Size (1,1,0) → 1マス=1ユニット
- `com.unity.2d.pixel-perfect` パッケージ導入推奨（Assets PPU=**256**）
- 図鑑立ち絵150枚＋バトル用150枚は**この設定を適用しない**（BC7圧縮）

## 5. データ設計の確定方針（v6から変更なし・据え置き）

- **ScriptableObject（CreatureData）＝不変のテンプレート**、**実行時データ（CreatureInstance）＝プレーンクラスでJSONセーブ**、参照は`int creatureId`で張る
- `CreatureData`：id / 名前 / カテゴリ(人型・非人型) / 属性 / 基礎ステータス / `EvolutionStage[]`（絆度閾値`bondThreshold`で進化、`statMultiplier`で形態倍率）/ 絆イベントID配列
- `BondSystem.AddBond()`で絆度加算→進化閾値判定→`OnEvolutionTriggered`イベント発火
- **地面タイプ判定はカスタムTileクラス方式**：`GroundTile : Tile` に `GroundType groundType` と `bool walkable` を持たせる。移動判定は`TryMove()`内で`GetTile`
- 足音・パーティクルは`GroundEffectController.OnPlayerStep(cell)`。**GetTileは1歩につき1回だけ**呼ぶ
- ローカルには既に`CreatureData`/`BondEventData`のCreateAssetMenuが存在する。上記設計とローカル実装の整合はローカルClaude Codeに確認させること

## 6. ローカルに存在が確認できているエディタメニュー（v6時点のスクショ確認。今回未確認）

TsukimonoTanメニュー配下：CG統合 / ビルド / Build Game UI / オープニングシーン構築 / シーン作成＋システム配置 / デモシーン構築 / 日本語フォント自動セットアップ / 憑物データ一括インポート / 絆イベントプレースホルダー生成 / 絆イベントをCreatureDataに紐付け / 絆イベント会話文を流し込む

## 7. 既知の未解決事項（優先順・v6から更新）

1. **Opening素材準備の実行**（§3-3）— 今回セッションの最優先タスク。`LOCAL_EXECUTION_PLAN.md`のPhase1〜4を消化
2. Openingシーンの再生確認（v6 §3で未確認のまま持ち越し。素材追加後にまとめて確認でも可）
3. 日本語フォントのMissingReferenceException（41件、v6言及）が解消済みか未確認
4. タイル4枚の正方形化＋256縮小＋インポート（§4）
5. GitHub版Openingスクリプトの重複チェック・削除（v6 §3、未対応なら要確認）
6. 以降は従来ロードマップ：バトル用デフォルメ立ち絵150枚生成（txt2img・Anlas0条件：1枚ずつ/1024以下/Steps28以下）→ 進化システム実装 → モックアップuGUI移植 → BGM → 全体テスト

## 8. リポジトリ状態

- ブランチ：`claude/opening-scene-assets-3n6cd6`（push済み、PR未作成）
- 最新コミット：`ace254e`「Opening シーン素材準備 — ドキュメント・スクリプト整備」
- PR作成URL（必要なら）：`https://github.com/tantou1025-byte/My-ProjectGate/pull/new/claude/opening-scene-assets-3n6cd6`
