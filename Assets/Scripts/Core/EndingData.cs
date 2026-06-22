using UnityEngine;

namespace ProjectGate.Core
{
    public enum EndingCategory { Basic, Character }

    /// <summary>
    /// endings.csv(10パターン)から ProjectGateDataImporter.cs で一括生成されるデータ。
    /// 条件文(condition)は表示・確認用のテキストで、実際の判定ロジックはEndingResolver.csに書く。
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnding", menuName = "ProjectGate/EndingData")]
    public class EndingData : ScriptableObject
    {
        public int endingId;
        public string endingName;
        public EndingCategory category;
        [TextArea] public string conditionText;
    }
}
