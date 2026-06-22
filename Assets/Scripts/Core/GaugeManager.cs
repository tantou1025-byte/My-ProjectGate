using UnityEngine;
using System;

namespace ProjectGate.Core
{
    public enum FocusActionType { LightObserve, Question, DeepInterrogationOrTool }

    /// <summary>
    /// 集中力(Focus)・施設信用度(Trust)・疑念ゲージ(Doubt)を管理。
    /// v3: project_gate_design_doc.md セクション3の確定数値に合わせて再構成。
    /// - Focus消費は「軽い観察/質問/深い尋問・ツール使用」の3カテゴリ固定値(-5/-8/-15)
    /// - v2にあった時間経過のpassive drainは design_doc.md に記載がないため削除
    /// - 10夜目以降はFocus初期値が90に低下(疲労蓄積)
    /// </summary>
    public class GaugeManager : MonoBehaviour
    {
        [Header("集中力 Focus (0-100, 1夜単位でリセット)")]
        public float focus = 100f;
        public float focusInitDefault = 100f;
        public float focusInitFromNight10 = 90f;
        public const float COST_LIGHT_OBSERVE = 5f;
        public const float COST_QUESTION = 8f;
        public const float COST_DEEP_OR_TOOL = 15f;

        [Header("施設信用度 Trust (0-100, 全夜持続)")]
        public float trust = 100f;
        public const float TRUST_LOSS_REJECT_HUMAN = 15f;
        public const float TRUST_LOSS_PASS_FAKE = 25f;
        public const float TRUST_GAIN_CORRECT = 3f;

        [Header("疑念ゲージ Doubt (0-100, 来訪者ごとにリセット)")]
        public float doubt = 0f;
        public const float DOUBT_GAIN_MIN = 15f;
        public const float DOUBT_GAIN_MAX = 30f;
        public const float DOUBT_CORE_INSIGHT_THRESHOLD = 70f;

        public event Action<float> OnFocusChanged;
        public event Action<float> OnTrustChanged;
        public event Action<float> OnDoubtChanged;
        public event Action OnFocusDepleted;
        public event Action OnTrustDepleted;
        public event Action OnCoreInsightReached; // Doubt70%到達時、専用選択肢の解放トリガー

        private bool coreInsightFiredForCurrentVisitor = false;

        public void ResetForNewNight(int nightNumber)
        {
            focus = nightNumber >= 10 ? focusInitFromNight10 : focusInitDefault;
            OnFocusChanged?.Invoke(focus);
        }

        public void ResetForNewVisitor()
        {
            doubt = 0f;
            coreInsightFiredForCurrentVisitor = false;
            OnDoubtChanged?.Invoke(doubt);
        }

        public void SpendFocus(FocusActionType actionType)
        {
            float cost = actionType switch
            {
                FocusActionType.LightObserve => COST_LIGHT_OBSERVE,
                FocusActionType.Question => COST_QUESTION,
                FocusActionType.DeepInterrogationOrTool => COST_DEEP_OR_TOOL,
                _ => 0f
            };
            focus = Mathf.Clamp(focus - cost, 0f, 100f);
            OnFocusChanged?.Invoke(focus);
            if (focus <= 0f) OnFocusDepleted?.Invoke();
        }

        public void AddDoubt(float amount)
        {
            doubt = Mathf.Clamp(doubt + amount, 0f, 100f);
            OnDoubtChanged?.Invoke(doubt);
            if (!coreInsightFiredForCurrentVisitor && doubt >= DOUBT_CORE_INSIGHT_THRESHOLD)
            {
                coreInsightFiredForCurrentVisitor = true;
                OnCoreInsightReached?.Invoke();
            }
        }

        public void ApplyJudgmentResult(bool wasCorrect, bool rejectedHuman)
        {
            if (wasCorrect)
            {
                trust = Mathf.Clamp(trust + TRUST_GAIN_CORRECT, 0f, 100f);
            }
            else
            {
                float loss = rejectedHuman ? TRUST_LOSS_REJECT_HUMAN : TRUST_LOSS_PASS_FAKE;
                trust = Mathf.Clamp(trust - loss, 0f, 100f);
            }
            OnTrustChanged?.Invoke(trust);
            if (trust <= 0f) OnTrustDepleted?.Invoke();
        }
    }
}
