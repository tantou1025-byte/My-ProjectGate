using UnityEngine;
using System;

namespace ProjectGate.Core
{
    /// <summary>
    /// ゲーム状態をメモリ上で保持・管理。セーブ/ロード機能は Phase2以降。
    /// v3: Area(夜)の進行状態、判定結果、ゲージ状態を一元管理。
    ///
    /// 仕様(design_doc.md):
    /// - Area0(序章) 〜 Area6(終章) = 7段階
    /// - Area7は存在しない（バグ防止のため上限チェック必須）
    /// </summary>
    [System.Serializable]
    public class SaveData
    {
        [Header("進行状態")]
        public int currentAreaId = 0; // 0-6 (Area7は不正)
        public int currentNightNumber = 1;

        [Header("ゲージ状態")]
        public float trust = 100f;
        public float focus = 100f;
        public float doubt = 0f;

        [Header("判定履歴")]
        public bool[] judgmentHistory = new bool[14]; // 14夜分、各夜の判定結果
        public int correctJudgmentCount = 0;
        public int incorrectJudgmentCount = 0;
    }

    public class SaveSystem : MonoBehaviour
    {
        private const int MIN_AREA_ID = 0;
        private const int MAX_AREA_ID = 6;
        private const int TOTAL_NIGHTS = 14;

        [SerializeField] private SaveData currentSaveData = new SaveData();

        public event Action<int> OnAreaChanged;
        public event Action<int> OnNightChanged;

        /// <summary>
        /// 現在のArea(夜)IDを取得
        /// </summary>
        public int GetCurrentAreaId() => currentSaveData.currentAreaId;

        /// <summary>
        /// 現在の夜の番号を取得（1-14）
        /// </summary>
        public int GetCurrentNightNumber() => currentSaveData.currentNightNumber;

        /// <summary>
        /// Area IDをセット。0-6の範囲内でのみ有効。
        /// v3修正: 上限チェック(area > 6)・下限チェック(area < 0)を実装
        /// </summary>
        public void SetCurrentAreaId(int area)
        {
            if (area < MIN_AREA_ID)
            {
                Debug.LogWarning($"[SaveSystem] Area ID が下限未満です: {area} < {MIN_AREA_ID}. 修正: {MIN_AREA_ID}に設定");
                area = MIN_AREA_ID;
            }

            if (area > MAX_AREA_ID)
            {
                Debug.LogWarning($"[SaveSystem] Area ID が上限を超えています: {area} > {MAX_AREA_ID}. 修正: {MAX_AREA_ID}に設定");
                area = MAX_AREA_ID;
            }

            if (currentSaveData.currentAreaId != area)
            {
                currentSaveData.currentAreaId = area;
                OnAreaChanged?.Invoke(area);
            }
        }

        /// <summary>
        /// 次のAreaに進行。末尾(Area6)の場合は Game Over 状態へ
        /// </summary>
        public void AdvanceArea()
        {
            if (currentSaveData.currentAreaId >= MAX_AREA_ID)
            {
                Debug.Log("[SaveSystem] 最終Area(6)に到達。ゲーム終了処理へ");
                return;
            }

            SetCurrentAreaId(currentSaveData.currentAreaId + 1);
            currentSaveData.currentNightNumber++;
            OnNightChanged?.Invoke(currentSaveData.currentNightNumber);
        }

        /// <summary>
        /// ゲームが最終段階(Area6)に達しているかチェック
        /// </summary>
        public bool IsInFinalArea() => currentSaveData.currentAreaId == MAX_AREA_ID;

        /// <summary>
        /// 次のAreaが存在するか（Area7への進行が可能か）チェック
        /// </summary>
        public bool HasNextArea() => currentSaveData.currentAreaId < MAX_AREA_ID;

        /// <summary>
        /// Trust(施設信用度)を取得
        /// </summary>
        public float GetTrust() => currentSaveData.trust;

        /// <summary>
        /// Trust をセット（0-100 の範囲に正規化）
        /// </summary>
        public void SetTrust(float value) => currentSaveData.trust = Mathf.Clamp(value, 0f, 100f);

        /// <summary>
        /// 判定履歴を記録
        /// </summary>
        public void RecordJudgment(bool wasCorrect)
        {
            if (currentSaveData.currentNightNumber > 0 && currentSaveData.currentNightNumber <= TOTAL_NIGHTS)
            {
                int idx = currentSaveData.currentNightNumber - 1;
                currentSaveData.judgmentHistory[idx] = wasCorrect;

                if (wasCorrect)
                    currentSaveData.correctJudgmentCount++;
                else
                    currentSaveData.incorrectJudgmentCount++;
            }
        }

        /// <summary>
        /// セーブデータをリセット（新規ゲーム開始時）
        /// </summary>
        public void ResetGame()
        {
            currentSaveData = new SaveData();
            OnAreaChanged?.Invoke(0);
            OnNightChanged?.Invoke(1);
        }

        /// <summary>
        /// 現在のセーブデータを取得（テスト・デバッグ用）
        /// </summary>
        public SaveData GetSaveData() => currentSaveData;
    }
}
