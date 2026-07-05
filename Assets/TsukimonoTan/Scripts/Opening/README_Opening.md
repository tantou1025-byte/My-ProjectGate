# オープニングムービー実装 — 導入手順

## 構成ファイル
| ファイル | 役割 |
|---|---|
| `Scripts/Opening/OpeningDirector.cs` | メインシーケンス制御（約30秒のタイムライン、UI実行時構築、スキップ対応） |
| `Scripts/Opening/OpeningInkParticles.cs` | 墨パーティクル演出（uGUIベース、外部アセット不要） |
| `Scripts/Editor/OpeningSceneBuilder.cs` | シーン自動生成エディタツール |

## ローカルのTsukimonoTanプロジェクトへの導入手順

1. `Assets/TsukimonoTan/Scripts/Opening/` と `Scripts/Editor/OpeningSceneBuilder.cs` をプロジェクトの同じパスへコピー
2. Unityメニュー **TsukimonoTan > オープニングシーン生成** を実行
   - `Assets/TsukimonoTan/Scenes/OpeningScene.unity` が生成され、Build Settingsの先頭に登録される
3. シーン内の `OpeningDirector` に **日本語フォント（TMP_FontAsset）** を割り当てる
   - 未割り当ての場合は `Resources/Fonts/NotoSansJP SDF` を自動で探す
   - どちらも無い場合、日本語が□（豆腐）表示になる（FontSetupToolのバグ修正が前提）
4. 再生して確認。任意のキー/クリック/タップでスキップ可能

## 遷移先について
- 演出終了後 `TitleScene` へ遷移する（シーン名はInspectorで変更可）
- `TitleScene` がBuild Settingsに無い場合は警告ログを出して停止する（エラーにはしない）

## 設計メモ
- UIを全てコードで生成しているのは、シーンYAMLのGUID参照切れを避け、フォルダコピーだけで移植可能にするため
- 入力は Input System（新方式）対応。`ENABLE_INPUT_SYSTEM` が無効なプロジェクトでは旧Inputに自動フォールバック
- パーティクルはCanvas内のImageベース。Screen Space OverlayとParticleSystemの描画順競合を回避している
