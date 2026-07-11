# 憑物譚 最終ロードマップ引き継ぎ書 v7.0
## このドキュメントで全工程が設計確定。新Haikuチャットで実装担当

---

## 📋 概要：3つのドキュメント体系

このセッション（Fable 5）で以下3点を確定・作成しました：

1. **LOCAL_DIRECTIVE_phase1.md** 
   - ローカルClaude Code 向けの**実装指示書**
   - Openingシーン再生確認、タイル処理、GitHub版スクリプト削除チェック、データ整合確認
   - ローカルでの実行結果の報告フォーマット確定

2. **PHASE2_DESIGN_complete.md**
   - バトル用立ち絵生成、進化システム、UI移植、BGM/SE生成の**詳細設計書**
   - プロンプト例・コード断片・実装上の注意を含む完全版
   - ローカル実装の基盤となる仕様

3. **handoff_v7_complete_roadmap.md**（このファイル）
   - 全体統括・次チャットでのアクション定義

---

## 🎯 新Haikuチャットでやること（順序厳密）

### Step 1: 前置き（テンプレート）
新チャット開始時に、このテキストを貼り付けて伝える：
```
【引き継ぎ内容】
プロジェクト：憑物譚（つきものたん）
状態：Fable 5 セッションで Phase 1/2 の設計が完全確定
タスク：ローカル実装の確認・委譲・テスト
参照資料：
  - Assets/TsukimonoTan/Docs/LOCAL_DIRECTIVE_phase1.md（実装指示）
  - Assets/TsukimonoTan/Docs/PHASE2_DESIGN_complete.md（詳細設計）
  - Assets/TsukimonoTan/Docs/handoff_v6.md（背景・作業体制ルール）

作業体制：
  - ローカルUnity/Claude Code ← 実装・実行確認
  - このHaikuセッション ← 設計相談・新しい判断・結合
  - GitHub ← コード管理

最初のタスク：ローカル側が phase1 指示書を実行 → 結果報告
```

### Step 2: Phase 1 実行結果の収集
ローカル側（Claude Code）が `LOCAL_DIRECTIVE_phase1.md` に従い実施：
- ① Opening シーン再生確認 + Console ログ報告
- ② タイル4枚の正方形化・256縮小・Unityインポート + スクリーンショット報告
- ③ GitHub版Openingスクリプト重複チェック + 削除結果報告
- ④ データ設計（CreatureData/GroundTile）整合確認 + 結果報告

**Haikuセッションの対応**：
- 報告内容の妥当性を確認（「フォント□表示」なら対応方法を提示 等）
- ローカル側で詳細修正が必要な場合だけ指示を出す

### Step 3: Phase 2A 実行（バトル用立ち絵生成）
ローカル側が `PHASE2_DESIGN_complete.md` の「Phase 2-A」に従い：
- プロンプトテンプレート確定 + CSV読み込み
- Python スクリプト生成（NovelAI API + rembg 自動化）
- テスト実行（1〜3体で背景除去品質確認）
- 全150体生成実行 + ログ保存
- Assets/TsukimonoTan/Resources/CreatureSprites/Battle/ へ出力

**Haikuセッションの対応**：
- テスト実行時の背景除去品質のスクリーンショット確認
- 問題例：キャラと背景色が似ている場合の対応指示
- 生成進捗のモニタリング（API エラー対応）

### Step 4: Phase 2B 実行（進化システム実装）
ローカル側が `PHASE2_DESIGN_complete.md` の「Phase 2-B」に従い：
- ローカル既存 CreatureData との整合確認
- EvolutionStage[] の設定方法を決定（CSV自動 or 手動＋プリセット）
- 150体の進化形態・命名・ステータス倍率を設定
- BondSystem の進化トリガー実装・テスト

**Haikuセッションの対応**：
- 進化形態の命名が和風ダークファンタジー美学に合致しているか確認
- StatMultiplier（全体倍率）で問題ないか、個別値が必要か判断

### Step 5: Phase 2C 実行（UI uGUI 移植）
ローカル側が `PHASE2_DESIGN_complete.md` の「Phase 2-C」に従い：
- HTML モックアップ（field_exploration_v3.html / battle_scene_v3.html）参照
- フィールド UI（ステータスパネル+パッド）→ uGUI 実装
- バトル UI（敵情報+HP+技ボタン）→ uGUI 実装
- 演出（画面振動・爆発・フラッシュ）Coroutine 実装

**Haikuセッションの対応**：
- レイアウト・配色が確定パレット（深紫黒/金/紅）に統一できているか確認
- 演出のタイミング・ビジュアルが設計と合致しているか確認

### Step 6: Phase 2D 実行（BGM/SE 生成）
ローカル側が `PHASE2_DESIGN_complete.md` の「Phase 2-D」に従い：
- Suno で曲生成（Opening / フィールド / バトル）
- SE 生成（Web Audio API or SFXR）
- ライセンス確認
- AudioManager に統合

**Haikuセッションの対応**：
- 生成された曲がゲーム演出に合致しているか試聴確認
- 必要に応じて再生成指示

### Step 7: Phase 3 全体テスト
ローカル側が `PHASE2_DESIGN_complete.md` の「Phase 2-E」に従い：
- ユニットテスト各システム
- シーン間統合テスト
- 代表エンディング 1 ルート走破テスト
- パフォーマンス確認

**Haikuセッションの対応**：
- テスト結果のバグ報告 ↔ 修正の back-and-forth
- 最終的なリリース前チェックリスト確認

---

## 💬 役割分担（再掲：作業体制ルール）

| 環境 | 実施内容 |
|---|---|
| ローカルUnity + Claude Code | ファイル操作・実装・ローカル再生確認・git push |
| Haikuセッション（このチャット） | 設計判断・新しい仕様判定・品質確認・相談 |
| GitHub | コード履歴管理・ブランチ追跡 |

**超重要ルール**（前セッションのズレ問題から学んだ）：
- ローカルのファイルパスやUnityメニュー位置を断定して案内しない
- 不明なら「スクリーンショット見せて」or「実行結果を報告して」と求める
- 最小限のUndoで修正できる変更のみ指示（削除・上書きはローカルで確認後）

---

## 🔍 前回セッション（Fable 5）で作成・確定した成果物

### コード実装
- `Scripts/Opening/OpeningDirector.cs` — 30秒オープニングタイムライン（UI自動構築）
- `Scripts/Opening/OpeningInkParticles.cs` — uGUIベース墨パーティクル
- `Scripts/Editor/OpeningSceneBuilder.cs` — シーン自動生成エディタツール

### 設計書
- `Docs/handoff_v6.md` — 作業体制・状況整理（v6）
- `Docs/LOCAL_DIRECTIVE_phase1.md` — 即座実行タスク指示書
- `Docs/PHASE2_DESIGN_complete.md` — 詳細実装設計（バトル立ち絵〜BGM）
- `Docs/handoff_v7_complete_roadmap.md` — このファイル

### GitHub ブランチ
- `claude/tsukimono-demo-scene-j6seey` — 上記全ファイル収載

---

## ⚠️ 既知の未解決事項（リスク）

| リスク | 現状 | 対応予定 |
|---|---|---|
| 日本語フォント MissingReferenceException (41件) | 未確認・再発可能性 | Phase 1① で再実行してテスト |
| タイル4枚の縮小品質 | 256×256後の石畳ディテール不明 | Phase 1② でスクリーンショット確認 → 要あれば512再試行 |
| NovelAI API rate-limit / 認証 | Anlas消費ゼロ条件は確認済みが、長時間生成はタイムアウト可能性 | Phase 2A で1〜3体テスト後に全150体実行 |
| Suno 商用利用ライセンス | Free/Premium で異なる可能性 | Phase 2D で生成前に確認指示 |

---

## 📈 スケジュール見通し

| Phase | 想定期間 | 重要性 |
|---|---|---|
| Phase 1 | 2-3日 | 🔴 ブロッカー（合格まで先へ進めない） |
| Phase 2A | 2-3日 | 🟡 リソース多いが並行可能 |
| Phase 2B | 3-5日 | 🔴 ゲーム仕様に直結 |
| Phase 2C | 5-7日 | 🟡 バージョンアップ対応容易 |
| Phase 2D | 2-3日 | 🟢 無くても動く（後付け可能） |
| Phase 3 | 3-5日 | 🔴 リリース必須 |
| **合計** | **17-26日** | **1ヶ月目標達成可能** |

---

## 🚀 最後に

このロードマップで**仕様の未定部分は全て確定しました**。

あとはローカル側の実装 → 確認 → テストのサイクルです。

新Haikuチャットでは：
1. ローカル実行結果を受け取る
2. 品質・仕様チェック
3. 必要なら修正指示
4. 最終リリース判定

を繰り返すだけ。頑張ってください！🎮

---

**このドキュメント作成日時**：2026-07-05（Fable 5セッション）  
**対象プロジェクト**：`tantou1025-byte/my-projectgate` ブランチ `claude/tsukimono-demo-scene-j6seey`
