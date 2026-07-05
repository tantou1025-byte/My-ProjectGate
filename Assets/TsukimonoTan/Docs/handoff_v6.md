# 憑物譚（つきものたん）— チャット引き継ぎ書 v6.0

このドキュメントを新しいチャットに貼り付けて「このプロジェクトの続きです」と伝えてください。
前提となる詳細は `handoff_v3.md` / `handoff_v4.md` を参照（無くてもこの1枚で作業再開可能）。

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
1. ローカルのファイルパスやUnityメニューの位置を、確認せずに断定して案内しない（過去に案内と実物がズレるトラブルあり）。不明ならスクリーンショットか、ローカルClaude Codeでの確認結果を求めること
2. 実装作業はローカルClaude Codeに依頼文を書いて渡す形が確実。クラウド側は設計・コードレビュー・相談に徹する
3. Unityの「TsukimonoTan」メニューは**画面上部メニューバー**（Jobs と Window の間）。右クリック→Create→TsukimonoTan はScriptableObject作成用で別物

## 3. オープニングシーンの現状（今回セッションの主作業）

### 結論：ローカル版が正。GitHub版は使わない
- ローカルClaude Codeが独自にオープニングを実装済み。メニュー「**TsukimonoTan → オープニングシーン構築**」で、`Opening`シーン（Main Camera / CanvasRoot / OpeningDirector）が構築済み
- クラウド側も同仕様をGitHubリポジトリ`my-projectgate`のブランチ`claude/tsukimono-demo-scene-j6seey`（コミット`6fe0379`、`Assets/TsukimonoTan/Scripts/Opening/`配下）に実装したが、**ローカルへのコピーは中止と決定**（二重実装回避のため）

### 演出仕様（両実装共通、約30秒）
- 0-2秒 黒 → 2-5秒 白符フェードイン（揺らぎ）→ 5-10秒 墨パーティクル拡散 → 10-20秒 ナレーション6行 → 20-25秒 タイトル「憑物譚」→ 25-30秒 霧フェード→TitleSceneへ遷移
- ナレーション：「──想いは、死なない。／人が死んでも、／忘れても、／想いだけが夜に残る。／それを郷のひとは、こう呼んだ。／憑物、と。」

### 未完了タスク（次チャットの最初の確認事項）
1. **Openingシーンを▶Playして動作確認**（まだ結果未確認）。文字が□なら「TsukimonoTan → 日本語フォント自動セットアップ」実行後に再Play
2. 過去に`robocopy`でGitHub版をコピーした可能性がある。ローカルClaude Codeに「GitHubからコピーしたOpening系スクリプト（OpeningDirector.cs / OpeningInkParticles.cs / OpeningSceneBuilder.cs / README_Opening.md）がローカル版と重複していたら削除して」と依頼して一本化する
3. TitleSceneが未作成/Build Settings未登録なら遷移せず警告ログで止まる（仕様）

## 4. フィールドタイル素材の確定事項（今回セッションで決定）

### 素材の状態
- 4種の地形タイル完成：砂/土（黄）、雪/空色（青紫）、石畳（緑灰）、水（青緑）。SeamlessItでタイル化処理中〜完了
- **残作業①**：砂/土タイルだけ非正方形（約890×1024）→ 正方形にクロップ必要
- **残作業②**：4枚とも1024px級で画面表示（1マス100〜150px）に対し大きすぎる → **全タイルを256×256へ事前縮小**してからUnityに入れる（縮小はボックス/面積平均補間。バイキュービック滑らかは不可）。縮小後の石畳の見た目は要品質チェック
- **水タイルは進入不可**（歩行音プロファイル不要）

### Texture Importer 確定設定（タイル用）
- Filter Mode: `Point (no filter)` ／ Compression: `None` ／ Mip Maps: OFF ／ Wrap Mode: `Clamp`
- Alpha Is Transparency: ON ／ Read/Write: OFF ／ Physics Shape: OFF
- **PPU = 256、Max Size = 256**（256×256に縮小後の値）
- Grid: Rectangle、Cell Size (1,1,0) → 1マス=1ユニット
- `com.unity.2d.pixel-perfect` パッケージ導入推奨（カメラ追従時のちらつき防止、Assets PPU=64ではなく**256**に設定）
- 図鑑立ち絵150枚＋バトル用150枚は**この設定を適用しない**（BC7圧縮でよい）。タイル=None／イラスト=BC7をPresetで分けて運用

## 5. データ設計の確定方針（今回セッションで設計提示済み）

- **ScriptableObject（CreatureData）＝不変のテンプレート**、**実行時データ（CreatureInstance）＝プレーンクラスでJSONセーブ**、参照は`int creatureId`で張る。SOにミュータブル値を持たせるのは禁止（同種2体で値衝突・エディタ永続化バグ・セーブ不可の3重問題）
- `CreatureData`：id / 名前 / カテゴリ(人型・非人型) / 属性 / 基礎ステータス / `EvolutionStage[]`（絆度閾値`bondThreshold`で進化、`statMultiplier`で形態倍率）/ 絆イベントID配列
- `BondSystem.AddBond()`で絆度加算→進化閾値判定→`OnEvolutionTriggered`イベント発火（演出UIは購読側）
- **地面タイプ判定はカスタムTileクラス方式**：`GroundTile : Tile` に `GroundType groundType` と `bool walkable` を持たせる。水タイルのアセットだけ`walkable=false`。移動判定は物理コライダーではなく`TryMove()`内で`GetTile`（マス目移動なので移動側で止めるのが正）
- 足音・パーティクルは`GroundEffectController.OnPlayerStep(cell)`。**GetTileは1歩につき1回だけ**呼び、エンカウント判定と足音判定の両方へ同じtileを渡す
- ローカルには既に`CreatureData`/`BondEventData`のCreateAssetMenuが存在する（右クリックメニューで確認済み）。上記設計とローカル実装の整合はローカルClaude Codeに確認させること
- クラウド提示のコード全文はGitHubブランチには置いていない（チャット内提示のみ）。必要ならローカルClaude Codeに設計方針だけ伝えれば再現可能

## 6. ローカルに存在が確認できているエディタメニュー（スクショで確認済み）

TsukimonoTanメニュー配下：CG統合 / ビルド / Build Game UI / **オープニングシーン構築** / シーン作成＋システム配置 / デモシーン構築 / 日本語フォント自動セットアップ / 憑物データ一括インポート / 絆イベントプレースホルダー生成 / 絆イベントをCreatureDataに紐付け / 絆イベント会話文を流し込む

## 7. 既知の未解決事項（優先順）

1. Openingシーンの再生確認（§3）
2. 日本語フォントのMissingReferenceException（41件）が解消済みか未確認 — フォント自動セットアップ実行時に再発したら、壊れたフォントアセットを削除→再生成
3. タイル4枚の正方形化＋256縮小＋インポート（§4）
4. GitHub版Openingスクリプトの重複チェック・削除（§3）
5. 以降は従来ロードマップ：バトル用デフォルメ立ち絵150枚生成（txt2img・Anlas0条件：1枚ずつ/1024以下/Steps28以下）→ 進化システム実装 → モックアップuGUI移植 → BGM → 全体テスト
