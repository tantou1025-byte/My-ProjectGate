using System.Collections.Generic;
using System.IO;
using TsukimonoTan.Opening;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TsukimonoTan.EditorTools
{
    /// <summary>
    /// オープニングシーンをワンクリックで生成するエディタツール。
    /// メニュー: TsukimonoTan > オープニングシーン生成
    /// UIはOpeningDirectorが実行時に自前で構築するため、シーンにはカメラとディレクターのみ配置する。
    /// </summary>
    public static class OpeningSceneBuilder
    {
        private const string ScenePath = "Assets/TsukimonoTan/Scenes/OpeningScene.unity";

        [MenuItem("TsukimonoTan/オープニングシーン生成")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color32(0x0a, 0x0e, 0x27, 0xff);
            camGo.AddComponent<AudioListener>();

            new GameObject("OpeningDirector").AddComponent<OpeningDirector>();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettingsAsFirst(ScenePath);

            EditorUtility.DisplayDialog(
                "TsukimonoTan",
                "オープニングシーンを生成しました。\n" + ScenePath +
                "\n\nBuild Settingsの先頭（起動シーン）にも登録済みです。" +
                "\n日本語フォント(TMP_FontAsset)をOpeningDirectorに割り当ててから再生してください。",
                "OK");
        }

        private static void AddToBuildSettingsAsFirst(string path)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (scenes.Exists(s => s.path == path)) return;
            // オープニングはゲーム起動時に再生されるため先頭に置く
            scenes.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
