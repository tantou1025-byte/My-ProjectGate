using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using System.Diagnostics;

public class BuildAndServeMenu
{
    [MenuItem("Tools/Project Gate/Build & Serve for Testing")]
    public static void BuildAndServeWebGL()
    {
        UnityEngine.Debug.Log("WebGL ビルド開始...");

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = new[] { EditorSceneManager.GetActiveScene().path },
            locationPathName = "Builds/WebGL",
            target = BuildTarget.WebGL,
            options = BuildOptions.None,
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);

        if (report.summary.result == BuildResult.Succeeded)
        {
            UnityEngine.Debug.Log("ビルド成功！");
            UnityEngine.Debug.Log("サーバー起動中...");

            StartWebServer();

            UnityEngine.Debug.Log(new string('=', 50));
            UnityEngine.Debug.Log("スマホでアクセス：http://192.168.11.6:8080");
            UnityEngine.Debug.Log(new string('=', 50));
        }
        else
        {
            UnityEngine.Debug.LogError("ビルド失敗");
        }
    }

    private static void StartWebServer()
    {
        string projectPath = System.IO.Path.GetDirectoryName(Application.dataPath);
        string webglPath = System.IO.Path.Combine(projectPath, "Builds", "WebGL");

        string command = $"cd '{webglPath}' && python -m http.server 8080";

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoExit -Command \"{command}\"",
            UseShellExecute = true,
            CreateNoWindow = false,
        };

        try
        {
            Process.Start(psi);
            UnityEngine.Debug.Log("PowerShell でサーバーを起動しました");
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError($"サーバー起動失敗: {ex.Message}");
        }
    }
}
