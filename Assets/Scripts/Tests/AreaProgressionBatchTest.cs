using NUnit.Framework;
using ProjectGate.Core;
using UnityEngine;

namespace ProjectGate.Tests
{
    /// <summary>
    /// SaveSystem のArea進行ロジックをテスト。
    /// v3: バグ修正（Area7への不正進行、上限チェック欠如）の確認テスト
    ///
    /// テスト対象:
    /// - SetCurrentAreaId() の下限/上限チェック
    /// - AdvanceArea() による正常進行
    /// - 最終Area(6)の判定
    /// - 判定履歴の記録
    /// </summary>
    [TestFixture]
    public class AreaProgressionBatchTest
    {
        private SaveSystem saveSystem;

        [SetUp]
        public void Setup()
        {
            GameObject go = new GameObject("SaveSystemTest");
            saveSystem = go.AddComponent<SaveSystem>();
            saveSystem.ResetGame();
        }

        [TearDown]
        public void Teardown()
        {
            Object.Destroy(saveSystem.gameObject);
        }

        // ===== SetCurrentAreaId() のテスト =====

        [Test]
        public void SetCurrentAreaId_ValidId_SetSuccessfully()
        {
            // 正常な Area ID (0-6) を設定できるか
            for (int area = 0; area <= 6; area++)
            {
                saveSystem.SetCurrentAreaId(area);
                Assert.AreEqual(area, saveSystem.GetCurrentAreaId(),
                    $"Area {area} への設定に失敗しました");
            }
        }

        [Test]
        public void SetCurrentAreaId_BelowMinimum_ClampsToZero()
        {
            // 下限未満 (負の値) を設定すると 0 にクランプされる
            saveSystem.SetCurrentAreaId(-5);
            Assert.AreEqual(0, saveSystem.GetCurrentAreaId(),
                "負の Area ID は 0 にクランプされるべきです");
        }

        [Test]
        public void SetCurrentAreaId_AboveMaximum_ClampsToSix()
        {
            // 上限超過 (Area7以上) を設定すると 6 にクランプされる
            // ← v3での重要な修正点（Area7不正進行バグの防止）
            saveSystem.SetCurrentAreaId(7);
            Assert.AreEqual(6, saveSystem.GetCurrentAreaId(),
                "Area 7 は MAX_AREA_ID(6) にクランプされるべきです");

            saveSystem.SetCurrentAreaId(999);
            Assert.AreEqual(6, saveSystem.GetCurrentAreaId(),
                "大幅な超過値でも 6 にクランプされるべきです");
        }

        [Test]
        public void SetCurrentAreaId_BoundaryValues()
        {
            // 境界値テスト
            saveSystem.SetCurrentAreaId(0);
            Assert.AreEqual(0, saveSystem.GetCurrentAreaId());

            saveSystem.SetCurrentAreaId(6);
            Assert.AreEqual(6, saveSystem.GetCurrentAreaId());

            saveSystem.SetCurrentAreaId(-1);
            Assert.AreEqual(0, saveSystem.GetCurrentAreaId());

            saveSystem.SetCurrentAreaId(7);
            Assert.AreEqual(6, saveSystem.GetCurrentAreaId());
        }

        // ===== AdvanceArea() のテスト =====

        [Test]
        public void AdvanceArea_FromStartToEnd()
        {
            // Area 0 から開始して、順序通り Area 6 まで進行できるか
            Assert.AreEqual(0, saveSystem.GetCurrentAreaId(), "初期値は Area 0");

            for (int expected = 1; expected <= 6; expected++)
            {
                saveSystem.AdvanceArea();
                Assert.AreEqual(expected, saveSystem.GetCurrentAreaId(),
                    $"AdvanceArea() で Area {expected} に進行するべき");
            }
        }

        [Test]
        public void AdvanceArea_FromFinalAreaDoesNotProgress()
        {
            // Area 6 (最終)から AdvanceArea() を呼ぶと進行しない
            saveSystem.SetCurrentAreaId(6);
            Assert.AreEqual(6, saveSystem.GetCurrentAreaId());

            saveSystem.AdvanceArea();
            Assert.AreEqual(6, saveSystem.GetCurrentAreaId(),
                "最終Area(6)からは進行しないべき（Area7への不正進行防止）");
        }

        [Test]
        public void AdvanceArea_IncrementNightNumber()
        {
            // AdvanceArea() は同時に Night Number もインクリメント
            Assert.AreEqual(1, saveSystem.GetCurrentNightNumber());

            saveSystem.AdvanceArea();
            Assert.AreEqual(2, saveSystem.GetCurrentNightNumber());

            saveSystem.AdvanceArea();
            Assert.AreEqual(3, saveSystem.GetCurrentNightNumber());
        }

        // ===== 最終Area判定のテスト =====

        [Test]
        public void IsInFinalArea_DetectsArea6()
        {
            // Area 6 を最終エリアとして正しく判定
            saveSystem.SetCurrentAreaId(5);
            Assert.IsFalse(saveSystem.IsInFinalArea(), "Area 5 は最終ではない");

            saveSystem.SetCurrentAreaId(6);
            Assert.IsTrue(saveSystem.IsInFinalArea(), "Area 6 は最終");
        }

        [Test]
        public void HasNextArea_AtMaxBoundary()
        {
            // Area 6 では次のエリアがない
            saveSystem.SetCurrentAreaId(6);
            Assert.IsFalse(saveSystem.HasNextArea(),
                "Area 6 では HasNextArea() は false");

            saveSystem.SetCurrentAreaId(5);
            Assert.IsTrue(saveSystem.HasNextArea(),
                "Area 5 では HasNextArea() は true");
        }

        // ===== 判定履歴のテスト =====

        [Test]
        public void RecordJudgment_TracksCorrectAndIncorrect()
        {
            // 判定履歴を記録して集計
            saveSystem.RecordJudgment(true);  // Night 1: 正判定
            saveSystem.RecordJudgment(false); // Night 2: 誤判定（不可能だが仮想テスト）

            SaveData data = saveSystem.GetSaveData();
            Assert.AreEqual(1, data.correctJudgmentCount);
            Assert.AreEqual(1, data.incorrectJudgmentCount);
        }

        [Test]
        public void RecordJudgment_AllNights()
        {
            // 全14夜の判定履歴を記録
            for (int i = 0; i < 14; i++)
            {
                bool isCorrect = (i % 2 == 0); // 交互に正/誤
                saveSystem.RecordJudgment(isCorrect);
                if (i < 13) saveSystem.AdvanceArea();
            }

            SaveData data = saveSystem.GetSaveData();
            Assert.AreEqual(7, data.correctJudgmentCount, "偶数夜が正判定");
            Assert.AreEqual(7, data.incorrectJudgmentCount, "奇数夜が誤判定");
        }

        // ===== Trust(施設信用度)のテスト =====

        [Test]
        public void Trust_SetAndClamp()
        {
            // Trust は 0-100 の範囲に正規化される
            saveSystem.SetTrust(100f);
            Assert.AreEqual(100f, saveSystem.GetTrust());

            saveSystem.SetTrust(50f);
            Assert.AreEqual(50f, saveSystem.GetTrust());

            saveSystem.SetTrust(-10f);
            Assert.AreEqual(0f, saveSystem.GetTrust(), "負の Trust は 0 に");

            saveSystem.SetTrust(150f);
            Assert.AreEqual(100f, saveSystem.GetTrust(), "100を超える Trust は 100 に");
        }

        // ===== リセット機能のテスト =====

        [Test]
        public void ResetGame_ClearsAllState()
        {
            // ゲーム進行中の状態をセット
            saveSystem.SetCurrentAreaId(3);
            saveSystem.SetTrust(50f);
            saveSystem.RecordJudgment(true);

            // リセット実行
            saveSystem.ResetGame();

            // 全て初期状態に戻っているか
            Assert.AreEqual(0, saveSystem.GetCurrentAreaId());
            Assert.AreEqual(1, saveSystem.GetCurrentNightNumber());
            Assert.AreEqual(100f, saveSystem.GetTrust());

            SaveData data = saveSystem.GetSaveData();
            Assert.AreEqual(0, data.correctJudgmentCount);
            Assert.AreEqual(0, data.incorrectJudgmentCount);
        }

        // ===== イベントコールバックのテスト =====

        [Test]
        public void OnAreaChanged_EventFires()
        {
            // OnAreaChanged イベントが発火するか
            int callbackAreaId = -1;
            saveSystem.OnAreaChanged += (area) => callbackAreaId = area;

            saveSystem.SetCurrentAreaId(3);
            Assert.AreEqual(3, callbackAreaId, "OnAreaChanged コールバックが発火すべき");
        }

        [Test]
        public void OnNightChanged_EventFires()
        {
            // OnNightChanged イベントが発火するか
            int callbackNightNumber = -1;
            saveSystem.OnNightChanged += (night) => callbackNightNumber = night;

            saveSystem.AdvanceArea();
            Assert.AreEqual(2, callbackNightNumber, "OnNightChanged コールバックが発火すべき");
        }

        // ===== エッジケース・統合テスト =====

        [Test]
        public void EdgeCase_RapidAreaChanges()
        {
            // 高速でArea変更 → 最終Areaに達する
            for (int i = 0; i < 100; i++)
            {
                saveSystem.AdvanceArea();
            }
            Assert.AreEqual(6, saveSystem.GetCurrentAreaId(),
                "どれだけ AdvanceArea() を呼んでも Area 7 にはならない");
        }

        [Test]
        public void EdgeCase_MixedSetAndAdvance()
        {
            // SetCurrentAreaId と AdvanceArea を混在
            saveSystem.SetCurrentAreaId(0);
            saveSystem.AdvanceArea(); // → Area 1
            saveSystem.SetCurrentAreaId(5);
            saveSystem.AdvanceArea(); // → Area 6
            saveSystem.AdvanceArea(); // → 変わらず Area 6

            Assert.AreEqual(6, saveSystem.GetCurrentAreaId());
            Assert.AreEqual(6, saveSystem.GetCurrentNightNumber());
        }
    }
}
