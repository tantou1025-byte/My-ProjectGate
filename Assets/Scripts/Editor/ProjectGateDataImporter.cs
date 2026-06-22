#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

namespace ProjectGate.EditorTools
{
    /// <summary>
    /// Data/*.csv (visitor_patterns / judge_tools / endings / recurring_visitor_stages)
    /// を読み込み、対応するScriptableObjectアセットを一括生成する。
    ///
    /// 使い方:
    /// 1. このファイルを Assets/Editor/ に配置
    /// 2. 4つのCSVを Assets/Data/Source/ に配置
    ///    (visitor_patterns.csv, judge_tools.csv, endings.csv, recurring_visitor_stages.csv)
    /// 3. Unityメニュー「ProjectGate > データ一括インポート」を実行
    ///
    /// CSVは単純なカンマ区切り(フィールド内にASCIIカンマを含めないこと。日本語の「、」はOK)。
    /// </summary>
    public static class ProjectGateDataImporter
    {
        private const string SOURCE_DIR = "Assets/Data/Source";
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

        private static string[][] ReadCsv(string fileName)
        {
            string path = Path.Combine(SOURCE_DIR, fileName);
            if (!File.Exists(path))
            {
                Debug.LogError($"[ProjectGate] CSVが見つかりません: {path}");
                return new string[0][];
            }
            var lines = File.ReadAllLines(path).Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
            return lines.Skip(1).Select(l => l.Split(',')).ToArray(); // ヘッダー行をスキップ
        }

        private static void EnsureDir(string dir)
        {
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        }

        private static void ImportVisitors()
        {
            EnsureDir(VISITOR_OUT_DIR);
            var rows = ReadCsv("visitor_patterns.csv");
            // No,Name,Category,GroundTruthHint,Tell,DebutNight,HasCG,IsRecurringCore
            foreach (var r in rows)
            {
                int no = int.Parse(r[0]);
                string assetPath = $"{VISITOR_OUT_DIR}/Visitor_{no:D2}_{Sanitize(r[1])}.asset";
                var data = AssetDatabase.LoadAssetAtPath<ProjectGate.Core.VisitorData>(assetPath)
                           ?? ScriptableObject.CreateInstance<ProjectGate.Core.VisitorData>();

                data.patternNo = no;
                data.displayName = r[1];
                data.category = ParseEnum<ProjectGate.Core.VisitorCategory>(r[2]);
                data.groundTruthHint = ParseEnum<ProjectGate.Core.GroundTruthHint>(r[3]);
                data.tell = r[4];
                data.debutNight = int.Parse(r[5]);
                data.hasCG = bool.Parse(r[6]);
                data.isRecurringCore = bool.Parse(r[7]);
                data.recurringStage = no == 4 ? 1 : (no == 14 ? 4 : 1);
                data.groundTruthIsHuman = data.groundTruthHint == ProjectGate.Core.GroundTruthHint.Human;

                if (!AssetDatabase.Contains(data))
                    AssetDatabase.CreateAsset(data, assetPath);
                else
                    EditorUtility.SetDirty(data);
            }
        }

        private static void ImportTools()
        {
            EnsureDir(TOOL_OUT_DIR);
            var rows = ReadCsv("judge_tools.csv");
            // ToolId,Name,Icon,UnlockNight,EffectDescription,FocusCost,DetectionRate,MatchRateBonus,Note
            foreach (var r in rows)
            {
                string assetPath = $"{TOOL_OUT_DIR}/Tool_{Sanitize(r[0])}.asset";
                var data = AssetDatabase.LoadAssetAtPath<ProjectGate.Core.JudgeToolData>(assetPath)
                           ?? ScriptableObject.CreateInstance<ProjectGate.Core.JudgeToolData>();

                data.toolId = r[0];
                data.toolName = r[1];
                data.icon = r[2];
                data.unlockAtNight = int.Parse(r[3]);
                data.effectDescription = r[4];
                data.focusCost = float.Parse(r[5]);
                data.detectionRate = float.Parse(r[6]);
                data.matchRateBonus = float.Parse(r[7]);
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
            var rows = ReadCsv("endings.csv");
            // EndingId,Name,Type,Condition
            foreach (var r in rows)
            {
                int id = int.Parse(r[0]);
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

        /// <summary>
        /// 名無しはvisitor_patterns.csvにNo.4(初登場)とNo.14(最終形態)しか無いが、
        /// design_doc.mdの4段階構成にはNo.が振られていない再訪1(7夜目)・再訪2(10夜目)もある。
        /// これらをrecurring_visitor_stages.csvから別途生成する。
        /// </summary>
        private static void ImportRecurringStages()
        {
            EnsureDir(VISITOR_OUT_DIR);
            var rows = ReadCsv("recurring_visitor_stages.csv");
            // Stage,Night,Behavior
            foreach (var r in rows)
            {
                int stage = int.Parse(r[0]);
                if (stage == 1 || stage == 4) continue; // これらはNo.4/No.14として既に生成済み

                int night = int.Parse(r[1]);
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
