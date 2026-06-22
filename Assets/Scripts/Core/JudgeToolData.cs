using UnityEngine;

namespace ProjectGate.Core
{
    /// <summary>
    /// 判定ツールの定義。judge_tools.csv から ProjectGateDataImporter.cs で一括生成。
    /// v3: 12夜目の「全道具精度向上」は個別ツールのLvではなく、
    /// NightLoopController側でグローバルに加算する仕様(isGlobalUpgrade=trueの行がそれ)。
    /// </summary>
    [CreateAssetMenu(fileName = "NewTool", menuName = "ProjectGate/JudgeToolData")]
    public class JudgeToolData : ScriptableObject
    {
        public string toolId;
        public string toolName;
        public string icon;
        [TextArea] public string effectDescription;

        public int unlockAtNight = 1;
        public float focusCost;          // 0ならFocus消費なし(チェックリスト等の参照系を想定する場合に調整)
        public float detectionRate;      // 0-1(仮値。design_doc.mdに数値記載なし)
        public float matchRateBonus;     // 仮値

        [Header("グローバルアップグレード行(12夜目の全道具精度向上)はこれをtrueにする")]
        public bool isGlobalUpgrade = false;

        public bool IsUnlocked(int currentNightNumber) => currentNightNumber >= unlockAtNight;
    }
}
