# OpeningDirector.cs 更新 — Yokai スピリット画像統合

## 概要
Opening シーンに **妖怪スピリット画像** を背景として表示し、適切なフェードアニメーションを加えます。

---

## 変更内容サマリー

| 項目 | 詳細 |
|---|---|
| **新フィールド** | Spirit Image (Image コンポーネント)、フェード設定 |
| **BuildUI()** | Spirit Image GameObject を生成・配置 |
| **Run()** | タイムラインに Spirit フェードアニメーションを統合 |
| **Timeline** | 既存：0-30秒。新：Spirit フェード制御を追加 |

---

## Step 1: フィールド追加（line 24-60 周辺）

**追加コード:**

```csharp
// 既存：
private Image _blackout;
private bool _transitionStarted;
private float _swayTime;

// ↓ 以下を追加 ↓
private Image _spiritImage;  // 妖怪スピリット画像
```

**位置:** `_swayTime` の下に追加（line 60 あたり）

---

## Step 2: BuildUI() に Spirit 画像生成ロジック追加

**既存コード（line 155 前後）:**

```csharp
CreateImage(root, "Background", BgColor, stretch: true);

// 白符（中央の縦長の札）
var talismanImg = CreateImage(root, "Talisman", WashiWhite, stretch: false);
```

**↓ 修正：Background の直後に Spirit Image を挿入**

```csharp
CreateImage(root, "Background", BgColor, stretch: true);

// 妖怪スピリット画像（背景より手前、符より奥）
_spiritImage = CreateImage(root, "SpiritImage", new Color(1f, 1f, 1f, 0f), stretch: false);
_spiritImage.sprite = Resources.Load<Sprite>("Sprites/Opening/spirit_yokai_opening_transparent");
_spiritImage.rectTransform.sizeDelta = new Vector2(400f, 600f);  // フィット値
_spiritImage.rectTransform.anchoredPosition = new Vector2(0f, -50f);  // 下配置
_spiritImage.raycastTarget = false;

// 白符（中央の縦長の札）
var talismanImg = CreateImage(root, "Talisman", WashiWhite, stretch: false);
```

**重要なポイント:**
- `sprite = Resources.Load<Sprite>(...)` で透過 PNG をロード
- `sizeDelta` は画像アスペクトに合わせて調整（512×768 → 400×600 推奨）
- `anchoredPosition` は下部配置（-50 は微調整値）
- 初期 alpha = 0（非表示）

---

## Step 3: Run() でフェードアニメーション統合

**現在のタイムライン:**

```csharp
private IEnumerator Run()
{
    yield return new WaitForSeconds(2f);                    // 0-2s  黒・静寂
    yield return FadeGroup(_talisman, 0f, 1f, 3f);          // 2-5s  符フェードイン
    _ink.Play(5f);                                          // 5-10s 墨拡散
    // ... 以降省略
}
```

**↓ 修正：Spirit フェード制御を追加**

```csharp
private IEnumerator Run()
{
    // Spirit 初期表示（黒の背後から浮かび上がり）
    yield return new WaitForSeconds(1f);  // 1s遅延
    StartCoroutine(FadeImageAlpha(_spiritImage, 0f, 0.75f, 2f));  // 1-3s Spirit フェードイン

    yield return new WaitForSeconds(1f);  // 2s時点
    
    yield return FadeGroup(_talisman, 0f, 1f, 3f);          // 2-5s  符フェードイン（Spirit 上に重ねる）
    
    _ink.Play(5f);                                          // 5-10s 墨拡散
    yield return new WaitForSeconds(5f);

    // ... 既存の Run 処理を続行
}
```

**タイムライン修正版:**

```
0-1s   完全な黒（Spirit も非表示）
1-3s   Spirit フェードイン（alpha 0→0.75）
2-5s   白符フェードイン（Spirit を覆う）
5-10s  墨パーティクル拡散
10-20s ナレーション
20-25s タイトル浮かび上がり
25-30s 霧フェード→遷移
```

---

## Step 4: スキップ時の Spirit フェード

**Transition() メソッド（line 127 前後）:**

スキップされた時の Spirit フェードアウトを追加（オプション）

```csharp
private IEnumerator Transition(bool fast)
{
    _transitionStarted = true;

    // Spirit をフェードアウト
    yield return FadeImageAlpha(_spiritImage, 0.75f, 0f, fast ? 0.6f : 2.5f);

    yield return FadeImageAlpha(_fog, 0f, 0.55f, fast ? 0.6f : 2.5f);
    yield return FadeImageAlpha(_blackout, 0f, 1f, fast ? 0.4f : 1.5f);

    // ... 既存のシーン遷移処理
}
```

---

## Step 5: ResourcePath 確保

Spirit 画像ロード時の パス確認：

```csharp
// 以下が正しくロードできるよう、ファイルが以下パスにあることを確認
Assets/TsukimonoTan/Resources/Sprites/Opening/spirit_yokai_opening_transparent.png
// → Resources.Load<Sprite>("Sprites/Opening/spirit_yokai_opening_transparent")
```

**ロード失敗時の fallback（オプション）:**

```csharp
_spiritImage.sprite = Resources.Load<Sprite>("Sprites/Opening/spirit_yokai_opening_transparent");
if (_spiritImage.sprite == null)
{
    Debug.LogWarning("[Opening] Spirit 画像が見つかりません。Sprite 表示をスキップします。");
    _spiritImage.gameObject.SetActive(false);
}
```

---

## 完成イメージ

**Opening タイムライン（修正版）:**

```
┌─────────────────────────────────────────────────────────┐
│ 時間: 0s                                          30s   │
├─────────────────────────────────────────────────────────┤
│ BG（深紫黒）: ████████████████████████████████████████  │
│                                                         │
│ Spirit Image:       ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓                 │
│ (0.75 opacity)      ↑(1-3s fade in)    ↑(25-30s out)  │
│                                                         │
│ White Talisman:     ■■■■■■■■■■■■■■■■■              │
│ (0.25 opacity)        ↑(2-5s fade)     ↑(20-25s)      │
│                                                         │
│ Ink Particles:          [▪▪▪▪▪▪▪▪]                    │
│                         ↑(5-10s)                       │
│                                                         │
│ Narration Text:            [テキスト]                  │
│                            ↑(10-20s)                   │
│                                                         │
│ Title「憑物譚」:                 ▲                      │
│                                ↑(20-25s)               │
│                                                         │
│ Fog（霧 overlay）:                   ▒▒▒▒▒▒▒▒▒▒      │
│                                      ↑(25-30s)         │
│                                                         │
│ Blackout（暗転）:                       ███████████     │
│                                      ↑(25-30s)         │
└─────────────────────────────────────────────────────────┘
```

---

## テストポイント

修正後の確認項目：

- [ ] Unity Editor で Opening シーン再生
- [ ] 1-3秒で Spirit が徐々に表示される
- [ ] 2-5秒で白符が Spirit を覆う（自然な重なり）
- [ ] 5-10秒で墨が拡散
- [ ] 10-20秒でナレーション表示
- [ ] 20-25秒でタイトル浮かび上がり
- [ ] 25-30秒で全てフェードアウト→遷移
- [ ] キー/クリック入力でスキップ可能
- [ ] 日本語テキスト表示（豆腐なし）

---

## ローカル実装チェックリスト

- [ ] Step 1: `_spiritImage` フィールド追加
- [ ] Step 2: BuildUI() に Spirit Image 生成ロジック追加
- [ ] Step 3: Run() コルーチンに Spirit フェード制御追加
- [ ] Step 4: Transition() に Spirit フェードアウト追加（オプション）
- [ ] Step 5: Resources パス確認・エラーハンドリング追加
- [ ] Unity 再生確認
- [ ] スクリーンショット取得 → 検収

---

## 関連ファイル

| ファイル | 役割 |
|---|---|
| `Assets/TsukimonoTan/Scripts/Opening/OpeningDirector.cs` | 本ファイル（修正対象） |
| `Assets/TsukimonoTan/Resources/Sprites/Opening/spirit_yokai_opening_transparent.png` | Spirit 画像（ロード対象） |
| `Assets/TsukimonoTan/Docs/02_AssetGeneration.md` | 画像生成手順 |

