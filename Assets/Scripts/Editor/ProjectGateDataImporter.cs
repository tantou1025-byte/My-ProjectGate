#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ProjectGate.EditorTools
{
    /// <summary>
    /// Data/*.csv (visitor_patterns / judge_tools / endings / recurring_visitor_stages)
    /// を読み込み、対応するScriptableObjectアセットを一括生成する。
    ///
    /// 使い方:
    /// 1. このファイルを Editor という名前のフォルダ配下に置く(現在は Assets/Scripts/Editor/)
    /// 2. 4つのCSVを SOURCE_DIR に置く
    ///    (visitor_patterns.csv, judge_tools.csv, endings.csv, recurring_visitor_stages.csv)
    /// 3. Unityメニュー「ProjectGate > データ一括インポート」を実行
    ///
    /// CSVは単純なカンマ区切り(フィールド内にASCIIカンマを含めないこと。日本語の「、」はOK)。
    /// </summary>
    public static class ProjectGateDataImporter
    {
        // CSVの実配置に合わせる。ここを変える場合はCSV4本ごと移動すること。
        private const string SOURCE_DIR = "Assets/Scripts/DataSource";
        private const string VISITOR_OUT_DIR = "Assets/Data/Visitors";
        private const string TOOL_OUT_DIR = "Assets/Data/Tools";
        private const string ENDING_OUT_DIR = "Assets/Data/Endings";

        [MenuItem("ProjectGate/データ一括インポート")]
        public static void ImportAll()
        {
            ImportVisitors();
            ImportTools();
            ImportEndings();
            ImportRecurringStages();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ProjectGate] データ一括インポート完了: VisitorData 28件+再訪2件 / JudgeToolData / EndingData を生成しました。");
        }

        private static string[][] ReadCsv(string fileName, int expectedColumns)
        {
            string path = Path.Combine(SOURCE_DIR, fileName);
            if (!File.Exists(path))
            {
                Debug.LogError($"[ProjectGate] CSVが見つかりません: {path}");
                return new string[0][];
            }

            var rows = File.ReadAllLines(path)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Skip(1) // ヘッダー行をスキップ
                .Select(l => l.Split(','))
                .ToArray();

            // 列数が足りない行をそのまま通すとIndexOutOfRangeでインポート全体が止まるため、
            // ここで弾いて何行目が悪いのかを報告する。
            var valid = new List<string[]>();
            for (int i = 0; i < rows.Length; i++)
            {
                if (rows[i].Length < expectedColumns)
                {
                    Debug.LogError(
                        $"[ProjectGate] {fileName} の{i + 2}行目は列数が足りません" +
                        $"(期待{expectedColumns}列 / 実際{rows[i].Length}列)。" +
                        "フィールド内にASCIIカンマが入っていないか確認してください。");
                    continue;
                }
                valid.Add(rows[i]);
            }
            return valid.ToArray();
        }

        private static void EnsureDir(string dir)
        {
            if (Directory.Exists(dir)) return;
            Directory.CreateDirectory(dir);
            // System.IOで作っただけのフォルダはAssetDatabaseがまだ知らないため、
            // 直後のCreateAssetが失敗する。ここでRefreshして認識させる。
            AssetDatabase.Refresh();
        }

        private static void ImportVisitors()
        {
            EnsureDir(VISITOR_OUT_DIR);
            var rows = ReadCsv("visitor_patterns.csv", 8);
            // No,Name,Category,GroundTruthHint,Tell,DebutNight,HasCG,IsRecurringCore
            var dependsPending = new List<string>();
            foreach (var r in rows)
            {
                int no = ParseInt(r[0]);
                string assetPath = $"{VISITOR_OUT_DIR}/Visitor_{no:D2}_{Sanitize(r[1])}.asset";
                var data = AssetDatabase.LoadAssetAtPath<ProjectGate.Core.VisitorData>(assetPath)
                           ?? ScriptableObject.CreateInstance<ProjectGate.Core.VisitorData>();

                data.patternNo = no;
                data.displayName = r[1];
                data.category = ParseEnum<ProjectGate.Core.VisitorCategory>(r[2]);
                data.groundTruthHint = ParseEnum<ProjectGate.Core.GroundTruthHint>(r[3]);
                data.tell = r[4];
                data.debutNight = ParseInt(r[5]);
                data.hasCG = ParseBool(r[6]);
                data.isRecurringCore = ParseBool(r[7]);
                data.recurringStage = no == 4 ? 1 : (no == 14 ? 4 : 1);

                // Depends(グレーゾーン)の正解はシナリオ側で個別に決める仕様なので、
                // CSVから機械的に決め打ちしない。既存アセットの手設定値も温存する。
                if (data.groundTruthHint == ProjectGate.Core.GroundTruthHint.Depends)
                    dependsPending.Add($"No.{no} {data.displayName}");
                else
                    data.groundTruthIsHuman =
                        data.groundTruthHint == ProjectGate.Core.GroundTruthHint.Human;

                if (!AssetDatabase.Contains(data))
                    AssetDatabase.CreateAsset(data, assetPath);
                else
                    EditorUtility.SetDirty(data);
            }

            if (dependsPending.Count > 0)
                Debug.LogWarning(
                    $"[ProjectGate] GroundTruthHint=Depends の来訪者{dependsPending.Count}件は " +
                    "groundTruthIsHuman を自動設定していません。シナリオ側で個別に設定してください: " +
                    string.Join(" / ", dependsPending));
        }

        private static void ImportTools()
        {
            EnsureDir(TOOL_OUT_DIR);
            var rows = ReadCsv("judge_tools.csv", 8);
            // ToolId,Name,Icon,UnlockNight,EffectDescription,FocusCost,DetectionRate,MatchRateBonus,Note
            foreach (var r in rows)
            {
                string assetPath = $"{TOOL_OUT_DIR}/Tool_{Sanitize(r[0])}.asset";
                var data = AssetDatabase.LoadAssetAtPath<ProjectGate.Core.JudgeToolData>(assetPath)
                           ?? ScriptableObject.CreateInstance<ProjectGate.Core.JudgeToolData>();

                data.toolId = r[0];
                data.toolName = r[1];
                data.icon = r[2];
                data.unlockAtNight = ParseInt(r[3]);
                data.effectDescription = r[4];
                data.focusCost = ParseFloat(r[5]);
                data.detectionRate = ParseFloat(r[6]);
                data.matchRateBonus = ParseFloat(r[7]);
                data.isGlobalUpgrade = r[0] == "global_upgrade";

                if (!AssetDatabase.Contains(data))
                    AssetDatabase.CreateAsset(data, assetPath);
                else
                    EditorUtility.SetDirty(data);
            }
        }

        private static void ImportEndings()
        {
            EnsureDir(ENDING_OUT_DIR);
            var rows = ReadCsv("endings.csv", 4);
            // EndingId,Name,Type,Condition
            foreach (var r in rows)
            {
                int id = ParseInt(r[0]);
                string assetPath = $"{ENDING_OUT_DIR}/Ending_{id:D2}_{Sanitize(r[1])}.asset";
                var data = AssetDatabase.LoadAssetAtPath<ProjectGate.Core.EndingData>(assetPath)
                           ?? ScriptableObject.CreateInstance<ProjectGate.Core.EndingData>();

                data.endingId = id;
                data.endingName = r[1];
                data.category = r[2] == "Basic" ? ProjectGate.Core.EndingCategory.Basic : ProjectGate.Core.EndingCategory.Character;
                data.conditionText = r[3];

                if (!AssetDatabase.Contains(data))
                    AssetDatabase.CreateAsset(data, assetPath);
                else
                    EditorUtility.SetDirty(data);
            }
        }

        private static T ParseEnum<T>(string value) where T : struct => System.Enum.Parse<T>(value.Trim());

        // OSのロケール次第で "0.8" が 8 と解釈されるのを防ぐため、必ずInvariantで読む。
        private static int ParseInt(string value) => int.Parse(value.Trim(), CultureInfo.InvariantCulture);

        private static float ParseFloat(string value) => float.Parse(value.Trim(), CultureInfo.InvariantCulture);

        private static bool ParseBool(string value) => bool.Parse(value.Trim());

        /// <summary>
        /// 名無しはvisitor_patterns.csvにNo.4(初登場)とNo.14(最終形態)しか無いが、
        /// design_doc.mdの4段階構成にはNo.が振られていない再訪1(7夜目)・再訪2(10夜目)もある。
        /// これらをrecurring_visitor_stages.csvから別途生成する。
        /// </summary>
        private static void ImportRecurringStages()
        {
            EnsureDir(VISITOR_OUT_DIR);
            var rows = ReadCsv("recurring_visitor_stages.csv", 3);
            // Stage,Night,Behavior
            foreach (var r in rows)
            {
                int stage = ParseInt(r[0]);
                if (stage == 1 || stage == 4) continue; // これらはNo.4/No.14として既に生成済み

                int night = ParseInt(r[1]);
                string assetPath = $"{VISITOR_OUT_DIR}/Visitor_Recurring_Stage{stage:D1}.asset";
                var data = AssetDatabase.LoadAssetAtPath<ProjectGate.Core.VisitorData>(assetPath)
                           ?? ScriptableObject.CreateInstance<ProjectGate.Core.VisitorData>();

                data.patternNo = 0; // 28パターン表には含まれないため0
                data.displayName = $"名無し(再訪{stage - 1})";
                data.category = ProjectGate.Core.VisitorCategory.Fake;
                data.groundTruthHint = ProjectGate.Core.GroundTruthHint.Fake;
                data.tell = r[2];
                data.debutNight = night;
                data.hasCG = true;
                data.isRecurringCore = true;
                data.recurringStage = stage;
                data.groundTruthIsHuman = false;

                if (!AssetDatabase.Contains(data))
                    AssetDatabase.CreateAsset(data, assetPath);
                else
                    EditorUtility.SetDirty(data);
            }
        }

        private static string Sanitize(string name) =>
            string.Concat(name.Where(c => !"\\/:*?\"<>|".Contains(c))).Replace(" ", "_");
    }
}
#endif
