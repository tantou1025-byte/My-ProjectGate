using System.Collections.Generic;

namespace ProjectGate.Core
{
    public enum BasicEndingResult { TrueEnd, GoodEnd, BadEndA_Closure, BadEndB_Missed }

    /// <summary>
    /// design_doc.md セクション5の確定条件を実装。
    /// 基本4種(Trust + 最終局面での名無し判定)はここで判定可能。
    /// 個別キャラ6種(双子/老婆/逃亡者/妊婦/蘇りの母/名無し専用End)は
    /// キャラルートのフラグ管理(Phase2でのシナリオ実装待ち)に依存するため、
    /// characterRouteFlagsを受け取れる形だけ用意してある。
    /// </summary>
    public class EndingResolver
    {
        /// <summary>
        /// Trust=0による即時ゲームオーバー(Bad End A)は、ゲーム進行中に
        /// GaugeManager.OnTrustDepleted で別途処理する想定。
        /// この関数は「14夜目まで生き残った」前提で、最終局面の結果から基本エンドを決める。
        /// </summary>
        public BasicEndingResult ResolveBasicEnding(
            float finalTrust,
            bool finalStageJudgmentCorrect,
            bool fullyIdentified)
        {
            if (finalTrust <= 0f) return BasicEndingResult.BadEndA_Closure;

            if (!finalStageJudgmentCorrect)
                return BasicEndingResult.BadEndB_Missed; // Trustは保てたが見逃した

            if (finalTrust >= 80f && fullyIdentified)
                return BasicEndingResult.TrueEnd;

            if (finalTrust >= 50f)
                return BasicEndingResult.GoodEnd;

            // ⚠️ design_doc.mdに明記がないグレーゾーン:
            // 「Trust 1〜49 かつ 最終局面で正しい判定」のケース。
            // 一旦Bad End Bにフォールバックしているが、要決定。
            return BasicEndingResult.BadEndB_Missed;
        }

        /// <summary>
        /// 個別キャラ6種の判定。Phase2でキャラルートのフラグ管理ができてから
        /// characterRouteFlagsの中身(例: "twins_route_clear"→true)を充実させる。
        /// </summary>
        public List<string> ResolveCharacterEndings(Dictionary<string, bool> characterRouteFlags)
        {
            var unlocked = new List<string>();
            foreach (var kv in characterRouteFlags)
                if (kv.Value) unlocked.Add(kv.Key);
            return unlocked;
        }
    }
}
