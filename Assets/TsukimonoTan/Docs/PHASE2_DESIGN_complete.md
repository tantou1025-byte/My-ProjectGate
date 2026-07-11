# Phase 2: バトル用デフォルメ立ち絵 + 進化システム + UI/BGM 設計書
## クラウド側が作成。ローカル側の実装の基盤となる設計ドキュメント

---

## Phase 2-A: バトル用デフォルメ立ち絵150枚の生成・統合方式

### 概要
図鑑用立ち絵150枚（既に完成・Assets配置）とは別に、バトル画面用の「デフォルメ版」立ち絵150枚を新規生成する。
- 要件：背景透過PNG、パーティクルや効果が重ねられるよう透過必須
- 生成パイプライン：NovelAI txt2img → 自動背景除去 → 保存

### 前提条件（既存ナレッジから確定）
- **Anlas消費ゼロ条件：**①1枚ずつ生成②1024×1024以下③Steps 28以下
- 認証：永続APIトークン（`NOVELAI_API_TOKEN` 環境変数）
- プロンプトソース：既存のCSV「生成プロンプト設定_全150体.csv」の「元プロンプトタグ」カラムを流用

### 仕様：詳細ステップ

#### Step 2A-1: プロンプトテンプレート確定
```
base_prompt_format = "{元プロンプト}, chibi character, deformed, simple background"
```
例：
- 灰狼（灰狼）→「black wolf with ash coloring, chibi character, deformed, simple background」
- 業火灰狼（進化形態）→「evolved black ash wolf, flame pattern, chibi character, deformed, simple background」

#### Step 2A-2: ローカル操作（Python スクリプト生成）
ローカルで実行するPythonスクリプト（既存の `copy_selected.py` と同じ運用）を作成。以下の流れで自動化：

```python
# pseudo-code
import requests, json, subprocess, re
from PIL import Image
from io import BytesIO

NOVELAI_URL = "https://api.novelai.net/ai/generate-image"
NOVELAI_TOKEN = os.environ["NOVELAI_API_TOKEN"]
CSV_PATH = "generated_images/生成プロンプト設定_全150体.csv"
OUTPUT_DIR = "Assets/TsukimonoTan/Resources/CreatureSprites/Battle"

creatures = load_csv(CSV_PATH)  # id, name, 元プロンプト_タグ 列

for creature in creatures:
    prompt = f"{creature['元プロンプト']}, chibi character, deformed, simple background"
    
    # ① NovelAI へ txt2img リクエスト（Anlas消費ゼロ条件）
    response = requests.post(
        NOVELAI_URL,
        headers={"Authorization": f"Bearer {NOVELAI_TOKEN}"},
        json={
            "input": prompt,
            "model": "nai-diffusion",
            "quality_toggle": False,
            "width": 512,
            "height": 512,
            "steps": 28,
            "sampler": "ddim",
            "seed": -1,
            "scale": 7.0
        }
    )
    
    # ② 生成画像（base64）をPIL.Image へ
    image_bytes = base64.b64decode(response.json()["output"])
    image = Image.open(BytesIO(image_bytes))
    
    # ③ 背景除去：rembg コマンドで実行
    temp_path = f"/tmp/creature_{creature['id']:03d}_temp.png"
    image.save(temp_path)
    
    output_path = f"{OUTPUT_DIR}/creature_{creature['id']:03d}_battle.png"
    subprocess.run([
        "python", "-m", "rembg", "i",
        temp_path, output_path
    ])
    
    # ④ メタデータ記録：生成成功ログ
    log_entry = {
        "id": creature['id'],
        "name": creature['name'],
        "prompt": prompt,
        "output_file": f"creature_{creature['id']:03d}_battle.png",
        "status": "success"
    }
    save_log(log_entry)
    
    # 過負荷防止：1秒待機（NovelAI は rate-limit あり）
    time.sleep(1.0)
```

**ローカル側の実装ポイント：**
- `rembg` のインストール：`pip install rembg[gpu]` （GPU推奨、CPUでも可）
- 既存の `novelai-api` ラッパー（`Aedial版`か別版か）がローカルにあるなら、それを流用
- 失敗ハンドリング：ネットワーク/API エラー時は部分再試行可能な形に（全150枚を1回で走らせ切るのはリスク）
- 生成ログ出力：`generation_log_deformed.json` として保存（後で検証用）

#### Step 2A-3: 背景除去スタイルの確認
- `rembg` デフォルト（u2net）で大半のキャラ背景は除去可能
- 問題例：キャラと背景の色が似ている場合、カット線が歪む可能性
- 対応：生成1〜3体で テスト実行 → スクリーンショット報告 → 問題あれば調整（`rembg` モデル変更等）

#### Step 2A-4: 出力・整理
- 保存先：`Assets/TsukimonoTan/Resources/CreatureSprites/Battle/`
- ファイル名：`creature_{id:D3}_battle.png`（例：`creature_001_battle.png`）
- メタデータ：`generation_log_deformed.json` に id/name/prompt/status をJSON出力
- Unity取り込み：Texture Importer は図鑑版と同じ設定（`Compression: BC7 High Quality`, `PPU: 64` 等、タイル設定とは異なる）

### 品質チェック
1. **透過確認**：Photoshop/GIMP で開いて背景がチェッカー柄（透明）で表示される
2. **エッジ確認**：キャラ輪郭にハロー（半透明の色縁）が無いか
3. **テクスチャ確認**：デフォルメが機能しているか（単なる縮小ではなく、プロポーション変更がされているか）

---

## Phase 2-B: 進化システムの詳細実装仕様

### 背景・要件
- 全150体が進化を持つ（契約変質）
- 基本：1回進化（2形態）
- 主要種（約20体未定）：2回進化（3形態）
- 進化トリガー：絆度閾値

### データ仕様

#### 2B-1: 絆度閾値の設定値（確定）
```csharp
// EvolutionStage.bondThreshold の値
public class EvolutionStageThresholds
{
    public const int FirstEvolution = 20;   // 初期形態→第1進化形態
    public const int SecondEvolution = 50;  // 第1進化形態→第2進化形態（主要種のみ）
}
```

#### 2B-2: ローカルの CreatureData 構造との統合（仕様確認）
v6 の設計では以下を提示：
```csharp
public EvolutionStage[] evolutionChain;  // [0]=初期形態, [1]=第1進化, [2]=第2進化（主要種のみ）
```

各形態：
```csharp
[System.Serializable]
public class EvolutionStage
{
    public string stageName;              // 例：「灰狼」→「業火灰狼」→「獄炎灰狼」
    public int bondThreshold;             // [0]=0, [1]=20, [2]=50
    public Sprite dexSprite;              // 図鑑立ち絵（既存150体から選定済み）
    public Sprite battleSprite;           // バトル用デフォルメ（Phase 2A で生成）
    public float statMultiplier = 1f;     // 全ステータスに対する倍率（例：1.5 = 50%アップ）
}
```

#### 2B-3: ローカルでの設定方法（エディタツール推奨）
ScriptableObject の evolutionChain[] を手動でぽちぽち設定するのは、150体×最大3形態＝450セットの数値入力で現実的でない。

2つの方式のいずれか採用：
1. **CSV から自動インポート**（既存の「150体ScriptableObject自動インポート機能」と同運用）
   - CSV フォーマット：id, name, stage0_name, stage0_threshold, stage0_sprite, stage1_name, stage1_threshold, stage1_sprite, ...
   - ローカル側のエディタツール「TsukimonoTan → 進化データ一括インポート」で一気にロード
2. **プリセット・デフォルト方式**
   - 非人型115体：全て「2形態」のみ（第1進化で止まる）
   - 人型35体：「3形態」を持つ（主要種判定は人型のみで有効）
   - デフォルト spritePicker UI から各形態の図鑑・バトル立ち絵を選択
   - ローカルが「主要20体」を決め、その20体だけ [2] を埋める

**推奨：方式2（プリセット+手動）** → 手数は若干増えるが透明性があり、調整漏れが起きにくい

#### 2B-4: 進化トリガーと演出の流れ
```
BondSystem.AddBond() で絆度増加
  ↓
bondLevel >= evolutionChain[next].bondThreshold ?
  ↓ YES
CreatureInstance.evolutionStageIndex をインクリメント
OnEvolutionTriggered イベント発火
  ↓ 
EvolveEffectUI（新規コンポーネント）が購読
  ├─ 進化演出再生（画面振動、パーティクル、フラッシュ等）
  ├─ 新形態のバトル立ち絵に切り替え
  └─ 図鑑にも自動登録（CreatureInstance.seenBondEventIds に推移）
```

#### 2B-5: 進化形態の命名規則（和風ダークファンタジー維持）
例：
- 灰狼（基本） → 業火灰狼（進化） → 獄炎灰狼（さらに進化）
- 白蛇（基本） → 霊白蛇（進化） → 幽玄白蛇（さらに進化）

パターン：
- 非人型：行為・属性を示す漢字追加（業火、獄炎、霊、幽玄、蒼炎 等）
- 人型：武具・装備の豪華さで表現（初期 → 鎧装備 → 最高位鎧装備）

**命名はローカル側が初期設計済みと想定。変更が必要なら新チャットで相談。**

### 実装上の注意
- StatMultiplier は統一倍率（全ステータス同じ比率）のため、個別微調整が必要なら将来 StatBlock（HP/攻/防/速の個別倍率）へ拡張
- 進化後は元の形態へ「戻す」ことはできない（実装リスク・UI複雑化のため。仕様確定済み）
- 絆度リセットは無し（人型の絆イベント105個（35体×3）との整合性のため）

---

## Phase 2-C: UIモックアップから uGUI への移植計画

### 既存モックアップの構成（確認済み）
- `field_exploration_mockup_v3.html` — フィールド探索画面（スマホ対応、十字キーパッド、カメラ追従）
- `battle_scene_mockup_v3.html` — バトル画面（派手演出フル装備）

### 移植戦略

#### 2C-1: フィールドUI移植（高優先度）
```
既存HTML      →   uGUI 対応物
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
地形タイル    →   Tilemap + SpriteRenderer
プレイヤー    →   Sprite + Movement Controller
ステータス表示 →   Canvas Panel（左上 HP/絆度ゲージ 等）
十字操作パッド →   UI Buttons（仮想パッド）or キーボード直接受け
エンカウント  →   BattleSceneLoader トリガー
```

**実装の注意：**
- Canvasはワールドスペースか Screen Space Overlayか：トップビュー固定のため **Screen Space UI** で単純化（カメラズームの影響を受けない）
- ステータス表示パネル：基本的にHTML版から CSS をそのまま uGUI の VerticalLayoutGroup / HorizontalLayoutGroup でレイアウト
- エンカウント演出：画面フラッシュ（Image.color を赤くして即座に戻す）+ メッセージ表示

#### 2C-2: バトルUI移植（中優先度）
```
HTML 要素               uGUI コンポーネント
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
敵情報カード           Panel + Layout（上部）
敵HP/プレイヤーHP      Slider + Text（中央）
絆度ゲージ              Slider（固有色）
技選択ボタン4個        Button ×4
契約ボタン              Button（条件：敵HP減少 → アルファ有効化）
演出（画面振動、爆発）  Coroutine で Image.color / transform 操作
```

**実装の注意：**
- 演出の「画面振動」：RectTransform.anchoredPosition を揺らす（簡易版）
- 「爆発パーティクル」：UI Image ベース（ParticleSystem ではなく）で再現（実装済みの InkParticles 同様）
- ターン表示：Text + グロー効果（Outline コンポーネント）
- HP ゲージのアニメーション：DOTween 等のアニメーションライブラリ or Coroutine でスムーズに減少

#### 2C-3: フォント・配色の統一（重要）
- 使用フォント：日本語 NotoSansJP SDF（既にセットアップ）
- 色パレット：深紫黒 / 金 / 紅のみ使用（HTML版の CSS variables を uGUI Color に置き換え）
  ```csharp
  static class UIColors
  {
      public static readonly Color DarkPurpleBlack = new Color32(0x0a, 0x0e, 0x27, 0xff);
      public static readonly Color Gold = new Color32(0xd4, 0xaf, 0x37, 0xff);
      public static readonly Color Crimson = new Color32(0xc4, 0x1e, 0x3a, 0xff);
      public static readonly Color PaperWhite = new Color32(0xf5, 0xf0, 0xe6, 0xff);
  }
  ```
- レイアウト：HTML版のスクリーンショットを参照しながら RectTransform をセッティング

#### 2C-4: 実装順序と検証
1. **フィールド基本UI**（ステータスパネル + 十字パッド）→ フィールドシーンで play して動作確認
2. **バトルUI**（敵情報 + HP ゲージ + 技ボタン） → バトルシーンで play して動作確認
3. **演出の統合**（振動・爆発・フラッシュ） → 各演出を個別に Coroutine でテスト
4. **遷移フロー**（フィールド → エンカウント → バトル → フィールド戻り） → end-to-end 確認

---

## Phase 2-D: BGM・SE 生成手順

### 概要
Suno（またはMusicLM等の生成AI）を使い、オリジナル曲を生成。
- オープニング曲：1曲（約30秒、既存演出に合わせた壮大な和風曲想定）
- フィールド BGM：3曲（平和・昼間・夜間用に分けるか、1ループで統一するか → ローカルの判断）
- バトル BGM：2曲（通常バトル / ボス戦用）
- SE（効果音）：10種程度（決定音、エラー音、攻撃音、進化音等）

### 生成パイプライン

#### 2D-1: 曲生成（Suno 使用想定）
Suno の Web UI から手動で生成：
```
【オープニング曲】
プロンプト例：
"Japanese dark fantasy opening theme, 30 seconds, traditional instruments with modern synth, 
mysterious and ominous atmosphere, slow build-up to climax"

ジャンル：Dark Ambient / Traditional Japanese
テンポ：Slow (60-80 BPM)
```

```
【フィールド BGM】
プロンプト例：
"Japanese countryside exploration BGM, loopable, peaceful atmosphere, traditional flute and koto sounds,
around 2 minutes, suitable for 8-bit style game"

ジャンル：Lo-fi / Ambient / Traditional
テンポ：Moderate (90-110 BPM)
```

```
【バトル BGM】
プロンプト例：
"Japanese RPG battle theme, intense and epic, 2 minutes loop, traditional percussion with modern synth,
exciting gameplay energy"

ジャンル：Video Game Music / Japanese Rock
テンポ：Fast (140-160 BPM)
```

#### 2D-2: SE生成（Web Audio API or Suno か選択）
- **決定音**：Web Audio API で生成（Beep, 周波数 800Hz, duration 100ms）
- **エラー音**：Web Audio API（周波数 400Hz, down-sweep, 200ms）
- **攻撃音**：Suno SE 機能 or SFXR（レトロゲーム音生成ツール）
- **進化音**：キラキラ系SE（Web Audio API で周波数スイープ）

Suno のプロンプト例（SE生成）：
```
"8-bit video game sound effect, spell cast sound, magical sparkle, 1 second, bright and energetic"
```

#### 2D-3: ライセンス確認（重要）
- Suno 生成物：商用利用の可否を確認（Premium プラン or Free ユーザーか）
- ローカル側に委譲：「生成したファイルのライセンス表記が必要か」を Suno 側で確認
- 必要な場合：生成物メタデータに Suno クレジットを記載

#### 2D-4: Unity 統合
```csharp
// AudioManager (既存と想定) に曲登録
public enum BGMType { Opening, FieldDay, FieldNight, BattleNormal, BattleBoss }
public enum SEType { Decision, Error, Attack, Evolution, Encounter }

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] bgmClips; // 配列で登録
    [SerializeField] private AudioClip[] seClips;
    
    public void PlayBGM(BGMType type) { /* 再生ロジック */ }
    public void PlaySE(SEType type) { /* 再生ロジック */ }
}
```

- 保存先：`Assets/TsukimonoTan/Resources/Audio/BGM/` , `Assets/TsukimonoTan/Resources/Audio/SE/`
- フォーマット：.wav or .ogg（.mp3 は Unity のサブスクリプション推奨のため回避）
- 品質設定：BGM は 128kbps, SE は 32kbps で十分

---

## Phase 2-E: 全体テスト・デバッグ計画

### テストレベル

#### Level 1: ユニットテスト（各システム個別）
| 対象 | テスト内容 |
|---|---|
| BondSystem | AddBond() → 進化トリガー発火確認 |
| EvolutionSystem | 形態切り替え → sprites 正常反映 |
| GroundEffectController | GetTile(cell) → 足音パーティクル再生 |
| UIElements | ボタンクリック → 正常な入力検知 |

**実装者（ローカル側）が自動テスト or 手動テストで担当。**

#### Level 2: シーン間統合テスト
```
Opening → (TitleScene) → FieldScene
          ↓
      BattleScene ← (エンカウント)
          ↓
        FieldScene
          ↓
       (ボス戦) → EndingScene
```

**チェック項目：**
1. Opening ナレーション + タイトル表示 + TitleScene遷移
2. フィールド操作 → 移動・エンカウント・絆度表示
3. バトル操作 → ターン制・技選択・契約（HP低下で成立率UP）
4. 進化演出 → 絆度UP → 閾値到達 → 進化画面表示 → 形態切り替え
5. 8種エンディング分岐 → 条件達成 → EndingScene 遷移

#### Level 3: シナリオルート全走破テスト
- **ルート数**: 8 エンディング × 複数パターン（人型35体での絆イベント組み合わせ）
- **最小限テスト**: 代表エンディング 1〜2 ルートだけは必ず走破（UI や遷移のバグを洗い出す）

**現実的スケジュール**：1 ヶ月リリース目標のため、全 8 ルートは「リリース後の修正」に回す判定も認める。

#### Level 4: パフォーマンス確認
- フレームレート：安定して 60fps（フルHD環境）
- メモリ：150体 × 複数形態のスプライト読み込みで OOM の危険は低い（ファイルサイズ削減済み）
- ロード時間：シーン遷移時に 2 秒以上待たないこと

### バグ報告・修正フロー

```
テスト実行 → バグ発見
  ↓
Issue化（新チャットで報告）
  ↓
ローカルClaude Code が修正実装 → git push
  ↓
再テスト
  ↓
OK → 次へ
NG → 修正に戻る
```

### リリース前チェックリスト

- [ ] Opening シーン 再生確認
- [ ] フィールド UI 動作確認
- [ ] バトル UI + 演出確認
- [ ] 進化トリガー + 演出確認
- [ ] ナレーション全言語（日本語）表示確認
- [ ] 足音 + パーティクル 確認
- [ ] BGM / SE 再生確認
- [ ] 代表エンディング 1 ルート走破確認
- [ ] ビルド成功確認（Windows .exe 生成）
- [ ] メモリ / FPS 安定性確認

---

## 全 Phase の工程表（参考）

| Phase | 内容 | 担当 | 想定期間 |
|---|---|---|---|
| 1 | Opening 確認 / タイル処理 / データ整合 | ローカル | 2-3 日 |
| 2A | バトル立ち絵 150 枚生成 | ローカル + NovelAI | 2-3 日（リクエスト待機含） |
| 2B | 進化システム実装 + データ設定 | ローカル | 3-5 日 |
| 2C | UI uGUI 移植 + 演出統合 | ローカル | 5-7 日 |
| 2D | BGM / SE 生成 + 統合 | ローカル + Suno | 2-3 日 |
| 3 | 全体テスト + 最終デバッグ | ローカル + 相談 | 3-5 日 |
| **合計** | | | **約 3 週間** |

→ 1 ヶ月リリース目標のため、工程表通りなら目標達成可能。ただし OpenAI API / Suno ネットワーク遅延は想定リスク。
