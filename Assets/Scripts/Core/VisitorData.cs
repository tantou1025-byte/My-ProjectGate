using UnityEngine;

namespace ProjectGate.Core
{
    public enum VisitorCategory { Human, Fake, Gray, Composite }
    public enum GroundTruthHint { Human, Fake, Depends } // Dependsはグレーゾーン、シナリオ側で個別決定

    /// <summary>
    /// 来訪者1人分の定義データ。v3でvisitor_patterns.csv(28種)に対応するフィールドを整理。
    /// ProjectGateDataImporter.cs でCSVから一括生成される想定。
    /// </summary>
    [CreateAssetMenu(fileName = "NewVisitor", menuName = "ProjectGate/VisitorData")]
    public class VisitorData : ScriptableObject
    {
        [Header("識別(CSVのNo./Nameに対応)")]
        public int patternNo;
        public string displayName;
        public VisitorCategory category;
        public GroundTruthHint groundTruthHint;

        [Header("正解(プレイヤーには非公開。Dependsの場合はシナリオ側で個別に設定すること)")]
        public bool groundTruthIsHuman;

        [Header("見分け方の手がかり(design_doc.md記載のtellをそのまま格納)")]
        [TextArea] public string tell;

        [Header("初登場夜(No.15〜28は0=任意の夜に差し込み可能)")]
        public int debutNight;

        [Header("CG/Hシーンを持つか(攻略キャラ6体に該当するエントリのみtrue)")]
        public bool hasCG;

        [Header("「再来する個体(名無し)」専用")]
        public bool isRecurringCore; // No.4, No.14がtrue
        [Range(1, 4)] public int recurringStage = 1;

        [Header("適合率算出用(基礎バイアス、design_doc.mdに数値記載なし→仮値)")]
        [Range(0, 100)] public float baseMatchRate = 50f;

        [Header("台詞・選択肢(プレースホルダー、創作部分)")]
        [TextArea] public string openingLine;
        public ChoiceOption[] choices;

        [Header("異常variant表示")]
        public Sprite normalSprite;
        public Sprite anomalySprite;
    }

    [System.Serializable]
    public class ChoiceOption
    {
        public enum ChoiceType { LightObserve, Question, ToolUse }
        public ChoiceType type;
        [TextArea] public string label;
        public float doubtDelta; // 15〜30の範囲を想定(design_doc.md準拠)
        public bool revealsAnomaly;
    }
}
