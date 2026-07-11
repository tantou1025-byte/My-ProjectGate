# ローカルClaude Code 実装指示書 Phase 1
## ■ このドキュメントはローカル側でのみ有効です。クラウドClaude からの委譲指示

このファイルをローカルのUnityプロジェクト側で開いている Claude Code に読み込ませて実行してください。

---

## タスク①: Openingシーン再生確認 + フォント状態確認

### 手順
1. Unity: メニューバー「TsukimonoTan → オープニングシーン構築」実行（既に実行済みなら不要）
2. Scenes フォルダに `Opening.unity` が生成されていることを確認
3. シーンを開き、Hierarchy → `OpeningDirector` を選択
4. Inspector: フォント割り当ての有無を確認
   - 割り当て済み（NotoSansJP SDF等が表示）→ そのままPlay
   - 未割り当て → TsukimonoTan → 日本語フォント自動セットアップ実行 → 待機 → Play
5. 再生結果をスクリーンショット + Console ウィンドウのログ記録で報告
   - 期待：黒2秒 → 白符フェードイン → 墨拡散 → ナレーション6行 → 縦書き「憑物譚」 → 霧フェード（TitleScene未登録なら警告ログ）
   - ナレーションが □ 表示 → フォント問題、Consoleログを貼る
   - エラー → Consoleの赤ログをそのまま報告
6. **Play 結果の文字出力・エラーログを全てここに貼り付けて報告**

---

## タスク②: フィールドタイル4枚の処理パイプライン

### 前提条件
- 4つの地形画像ファイルがローカルのどこかにある状態
- SeamlessIt 処理は既に完了 or 今から実行

### 処理手順（仕様）

#### Step 1: 砂/土タイルの正方形クロップ
- 現状：約890×1024（非正方形）→ 正方形化が必須（Tilemapが正方形前提）
- 対処：1024×1024 の正方形キャンバスへクロップ。中央揃え
- ツール：Photoshop / GIMP / オンラインツール等。選択はローカルの判断
- 完了ファイル名：`dirt_v2_05_retrofit_1024sq.png`

#### Step 2: 4枚全てを 256×256 へ縮小
- 理由：表示解像度（1マス100〜150px）に対し、1024px は大きすぎて実行時に間引きされてちらつきが出る
- 縮小方法：**ボックス補間（面積平均）**を使用。バイキュービック滑らかは不可
  - Photoshop：Image → Scale Image → 補間「ニアレスト近傍（拡大用）」ではなく「自動」もしくは「バイキュービック」も避ける → 実験的に見ると「線形」が無難
  - GIMP：Scale Image → Interpolation「None」（ニアレスト）か「Linear」
  - ImageMagick（CLI）：`convert input.png -resize 256x256 -filter Box output.png`
  - オンライン：Pixlr / Compressor等
- 完了ファイル名形式：`water_v1_02_retrofit_256.png`, `stone_v1_04_retrofit_256.png`, `snow_v1_04_retrofit_256.png`, `dirt_v2_05_retrofit_256.png`

#### Step 3: 石畳（`stone_v1_04_retrofit_256.png`）の品質チェック
- 縮小後、細かいディテール（石の個別感）が潰れすぎていないか **スクリーンショットで確認**
- 大きく見づらくなった → 512×512で再試行 → そのスクリーンショットも報告
- OK → 次に進む

#### Step 4: 4枚を Unity にインポート
- 保存先：`Assets/TsukimonoTan/Resources/Tiles/`
- ファイル：256版4ファイル（`*_256.png`）を全てコピー
- Unity側での操作：4ファイルそれぞれに以下の Texture Importer 設定を適用
  ```
  - Texture Type: Sprite (2D and UI)
  - Sprite Mode: Single
  - Filter Mode: Point (no filter)
  - Compression: None
  - Max Size: 256
  - Generate Mip Maps: OFF
  - Wrap Mode: Clamp
  - Alpha Is Transparency: ON
  - Read/Write Enabled: OFF
  - Generate Physics Shape: OFF
  - Pixels Per Unit: 256
  ```
- Apply を押す
- **インポート後、シーンに配置して目視でぼやけ/エッジの汚れがないことを簡易確認**（Tilemap に配置して表示 or SpriteRenderer）

### 報告内容
- Step 1〜3 の完了スクリーンショット（石畳の品質チェック）
- Step 4 完了後：`Resources/Tiles/` フォルダの Tile 画像4ファイルのスクリーンショット（Project ウィンドウに表示）
- 目視確認結果：「ぼやけなし・エッジ境界クリーン」or「問題点の詳細」

---

## タスク③: GitHub版Openingスクリプトの重複チェック・削除

### 背景
今回のセッション（クラウド側）で、GitHub にオープニング実装（OpeningDirector.cs 等）を push した。ローカル側はすでに完成している。二重実装を避けるため、GitHub版がローカルにコピーされていないか確認する。

### 手順
1. `Assets/TsukimonoTan/Scripts/Opening/` フォルダを確認
   - 存在しない → 何もしない（OK）
   - 存在する → 中身をリスト表示（フォルダ内のファイル一覧スクリーンショット）
2. 存在する場合、以下ファイルがあるか確認
   - `OpeningDirector.cs`
   - `OpeningInkParticles.cs`
   - `OpeningSceneBuilder.cs`
   - `README_Opening.md`
3. **あれば全て削除**（ローカル側の実装がある前提）
4. 削除後：フォルダが空になるか、フォルダ自体が消えることを確認

### 報告内容
- 「チェック完了。ファイルなし」or「ファイル検出 → 削除完了」のどちらか

---

## タスク④: ローカルのデータ設計との整合確認

前回のセッション（クラウド側）で `CreatureData`/`BondSystem` の設計を提示した。ローカル側に既にスクリプトが存在するはずなので、以下を確認する。

### 確認内容

#### 4-1. `CreatureData` スクリプトの確認
- 場所：`Scripts/Core/` または `Scripts/DataSource/` 内に存在するはず
- 確認項目：以下の構造があるか
  ```csharp
  public class CreatureData : ScriptableObject
  {
      public int id;
      public string creatureName;
      public EvolutionStage[] evolutionChain; // ← 重要
      public int[] bondEventIds;
      // 他：カテゴリ、属性、ステータス等
  }
  
  [System.Serializable]
  public class EvolutionStage
  {
      public string stageName;
      public int bondThreshold;
      public Sprite dexSprite;
      public Sprite battleSprite;
      // stat補正（現在は statMultiplier があるか、将来個別値が必要か）
  }
  ```
- 存在しない or 大きく異なる → ローカルのデザインを尊重（新チャットへ報告）
- 存在・整合 → 次へ

#### 4-2. 地面タイプシステムの確認
- `GroundTile` カスタムクラス（Tile を継承）が存在するか
  ```csharp
  public class GroundTile : UnityEngine.Tilemaps.Tile
  {
      public GroundType groundType;
      public bool walkable = true;
  }
  ```
- `GroundEffectController` (足音・パーティクル制御)が存在するか
- 存在・整合 → OK
- 存在しない → 「クラウド側で提示した設計で実装が必要」と新チャットへ報告

### 報告内容
- 「CreatureData/EvolutionStage 存在・構造整合」or「不整合の詳細」
- 「GroundTile/GroundEffectController 存在」or「未実装」

---

## 完了後の報告フォーマット

本ドキュメント終了後、以下の形式で新しいチャットに報告してください（このクラウドセッションではなく、新Haikuセッションへ）：

```
【Phase 1 実装結果報告】

✅ タスク① Openingシーン再生確認
  - ナレーション表示：✓ 正常 / ✗ □表示 / ✗ エラー
  - Play ログ：[Consoleコピペ]

✅ タスク② タイル処理完了
  - Step 1 正方形クロップ：完了
  - Step 2 256縮小（ボックス補間）：完了
  - Step 3 石畳品質確認：OK / 512再試行
  - Step 4 Unityインポート：完了
  - 目視確認：クリーン✓

✅ タスク③ 重複チェック・削除
  - Opening関連ファイル：なし（または削除完了）

✅ タスク④ データ設計整合
  - CreatureData：整合
  - GroundTile：実装済み / 未実装
```

このレポートが新チャットに届いたら、Phase 2（バトル立ち絵設計・進化システム設計）へ進みます。
