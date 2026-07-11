# Opening シーン — 和文フォント導入手順

## 概要
Opening シーンのテキストを正しく表示するために、**GenJyuuGothic（源暎ゴシック）** を TextMesh Pro フォントアセットとして導入します。

---

## 1. フォント ファイルのダウンロード

### 1-1. 源暎ゴシック（GenJyuuGothic）をダウンロード

- **URL:** https://github.com/ButTaiwan/genjyuugothic/releases
- **推奨版:** 最新の `GenJyuuGothic-*.zip`
- **推奨フォント:** `GenJyuuGothic-Bold.ttf`（太字、Openingの和風デザインに適合）

### 1-2. ファイル配置

ダウンロード後、ローカル Unity プロジェクトの以下にコピー：

```
C:\Users\[ユーザー名]\UnityProjects\TsukimonoTan\Assets\Fonts\
```

**ファイル構造例：**
```
Assets/
  Fonts/
    GenJyuuGothic-Bold.ttf
    NotoSansJP-Regular.ttf   （既存：別タスクで使用）
    ...
```

---

## 2. Unity での TextMesh Pro フォント生成

### 2-1. Essential Resources のインポート（初回のみ）

既に実施済みの場合はスキップしてください。

```
Menu: Assets > TextMesh Pro > Import TMP Essential Resources
```

これにより `Assets/TextMesh Pro Essential Resources/` が生成されます。

### 2-2. フォント アセット生成

1. **Project ウィンドウで** `Assets/Fonts/GenJyuuGothic-Bold.ttf` を右クリック
2. **メニュー:** `TextMesh Pro > Create Font Asset`
3. **ダイアログ:**
   - 出力先: `Assets/Fonts/` （または `Assets/TextMesh Pro/Fonts/`）
   - ファイル名: `GenJyuuGothic-Bold_TMP` （自動生成されるため確認のみ）
4. **生成完了後:**
   - `GenJyuuGothic-Bold_TMP.asset`
   - `GenJyuuGothic-Bold_TMP.png`（フォント テクスチャ）

---

## 3. Opening シーンへの割り当て

### 3-1. Scene を開く

```
Assets/TsukimonoTan/Scenes/OpeningScene.unity
```

### 3-2. OpeningDirector コンポーネントへ割り当て

1. **Hierarchy** で `OpeningDirector` GameObject を選択
2. **Inspector** の `OpeningDirector (Script)` コンポーネントを表示
3. **Japanese Font** フィールドに `GenJyuuGothic-Bold_TMP.asset` をドラッグ＆ドロップ
   - または Project から検索して割り当て

### 3-3. 再生して確認

```
Menu: ▶ Play（または Ctrl+P）
```

**期待される表示:**
- ナレーション 6 行が正しく日本語で表示される（□豆腐なし）
- 「憑物譚」タイトルも和文で表示

---

## 4. トラブルシューティング

### 問題：日本語が □（豆腐）で表示される

**原因①：フォント未割り当て**
- 解決：§3-2 の割り当てを確認

**原因②：フォント生成失敗**
- 解決：
  - `GenJyuuGothic-Bold_TMP.asset` を削除
  - Unity を再起動
  - §2-2 から再実行

**原因③：MissingReferenceException**
- TMP Essential Resources の破損の可能性
- 解決：
  1. `Assets/TextMesh Pro Essential Resources/` フォルダを削除
  2. Unity を再起動
  3. §2-1 から再実行

### 問題：フォント ファイルが Unity に認識されない

- **確認項目:**
  - ファイル拡張子が `.ttf` か `.otf` か
  - ファイル名に特殊文字なし
  - Assets フォルダ配下か確認

---

## 5. 関連ファイル・参考

| ファイル | 役割 |
|---|---|
| `Assets/TsukimonoTan/Scripts/Opening/OpeningDirector.cs` | Opening シーン制御（フォント割り当て先） |
| `Assets/TsukimonoTan/Scripts/Opening/README_Opening.md` | Opening 実装概要 |
| `Assets/TsukimonoTan/Scenes/OpeningScene.unity` | Opening シーン本体 |

---

## 6. 次ステップ

フォント導入後：

1. ✓ テキスト表示確認
2. [ ] Yokai スピリット画像生成・合成（`02_AssetGeneration.md` 参照）
3. [ ] OpeningDirector.cs へ画像統合
4. [ ] 全体再生確認

