using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ProjectGate.Core
{
    /// <summary>
    /// MVPの中核ループ。v3での変更点:
    /// - GaugeManagerのAPI変更(FocusActionType・ResetForNewNight)に追従
    /// - JudgeToolDataのグローバルアップグレード行(12夜目)を加味した実効値取得
    /// - 全体は14夜構成(design_doc.md)だが、MVP(Phase1)では1夜分のみ動かせばよい
    /// </summary>
    public class NightLoopController : MonoBehaviour
    {
        public const int TOTAL_NIGHTS = 14; // design_doc.md セクション2

        [SerializeField] private GaugeManager gaugeManager;
        [SerializeField] private List<VisitorData> tonightsVisitors;
        [SerializeField] private List<JudgeToolData> allTools; // global upgrade行も含めて全部入れる
        [SerializeField] private int currentNightNumber = 1;

        private int currentVisitorIndex = -1;
        private VisitorData CurrentVisitor =>
            currentVisitorIndex >= 0 && currentVisitorIndex < tonightsVisitors.Count
                ? tonightsVisitors[currentVisitorIndex]
                : null;

        public System.Action<VisitorData> OnVisitorEntered;
        public System.Action<bool> OnNightCleared;

        private void Start()
        {
            if (gaugeManager == null)
            {
                Debug.LogError("[ProjectGate] NightLoopController に GaugeManager が割り当てられていません。", this);
                enabled = false;
                return;
            }

            gaugeManager.OnTrustDepleted += HandleGameOver;
            gaugeManager.OnFocusDepleted += HandleFocusDepleted;
            gaugeManager.ResetForNewNight(currentNightNumber);
            AdvanceToNextVisitor();
        }

        public List<JudgeToolData> GetUnlockedTools() =>
            allTools.Where(t => !t.isGlobalUpgrade && t.IsUnlocked(currentNightNumber)).ToList();

        private bool IsGlobalUpgradeActive() =>
            allTools.Any(t => t.isGlobalUpgrade && t.IsUnlocked(currentNightNumber));

        public (float detectionRate, float matchRateBonus) GetEffectiveToolValues(JudgeToolData tool)
        {
            if (!IsGlobalUpgradeActive()) return (tool.detectionRate, tool.matchRateBonus);
            var globalUpgrade = allTools.First(t => t.isGlobalUpgrade);
            return (
                Mathf.Clamp01(tool.detectionRate + globalUpgrade.detectionRate),
                tool.matchRateBonus + globalUpgrade.matchRateBonus
            );
        }

        public void AdvanceToNextVisitor()
        {
            currentVisitorIndex++;
            if (CurrentVisitor == null)
            {
                OnNightCleared?.Invoke(true);
                return;
            }
            gaugeManager.ResetForNewVisitor();
            OnVisitorEntered?.Invoke(CurrentVisitor);
            // TODO(本制作): スプライト表示切替(表/裏/異常variant)のアニメーション
        }

        public void SelectChoice(ChoiceOption choice)
        {
            var actionType = choice.type switch
            {
                ChoiceOption.ChoiceType.LightObserve => FocusActionType.LightObserve,
                ChoiceOption.ChoiceType.Question => FocusActionType.Question,
                ChoiceOption.ChoiceType.ToolUse => FocusActionType.DeepInterrogationOrTool,
                _ => FocusActionType.Question
            };
            gaugeManager.SpendFocus(actionType);
            gaugeManager.AddDoubt(choice.doubtDelta);
        }

        public void UseTool(JudgeToolData tool)
        {
            if (!tool.IsUnlocked(currentNightNumber)) return;
            gaugeManager.SpendFocus(FocusActionType.DeepInterrogationOrTool);
            var (detectionRate, matchRateBonus) = GetEffectiveToolValues(tool);
            // TODO: detectionRate判定でanomalySprite解放、matchRateBonusを適合率リングに反映
        }

        public void MakeJudgment(bool playerChoseToPass)
        {
            // 夜がクリア済み(来訪者を全員さばいた後)に呼ばれてもNREにならないようにする
            var visitor = CurrentVisitor;
            if (visitor == null) return;

            bool wasCorrect = playerChoseToPass == visitor.groundTruthIsHuman;
            bool rejectedHuman = !wasCorrect && playerChoseToPass == false && visitor.groundTruthIsHuman;
            gaugeManager.ApplyJudgmentResult(wasCorrect, rejectedHuman);

            if (gaugeManager.trust > 0f)
                AdvanceToNextVisitor();
        }

        private void HandleFocusDepleted()
        {
            // TODO: 「思考停止」状態への移行(追加行動不可、手がかり不足のまま判定を迫られる)
        }

        private void HandleGameOver()
        {
            OnNightCleared?.Invoke(false);
        }
    }
}
