using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class FantasyAndroidBuild
{
    public static void Build()
    {
        // Generate the campaign bootstrap scene from the repository's editor builder.
        DemoLevelBuilder.Build();

        const string scenePath = "Assets/Scenes/ForestStage01.unity";
        if (!File.Exists(scenePath))
            throw new FileNotFoundException("The campaign scene was not generated.", scenePath);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(scenePath, true)
        };

        PlayerSettings.companyName = "Fantasy Adventure Studio";
        PlayerSettings.productName = "Fantasy Adventure";
        PlayerSettings.bundleVersion = "0.3.0";
        PlayerSettings.Android.bundleVersionCode = 3;
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
        PlayerSettings.applicationIdentifier = "com.fantasyadventure.game";
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

        string outputDirectory = Path.GetFullPath("build/Android");
        Directory.CreateDirectory(outputDirectory);
        string outputPath = Path.Combine(outputDirectory, "FantasyAdventure.apk");

        var options = new BuildPlayerOptions
        {
            scenes = new[] { scenePath },
            locationPathName = outputPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
            throw new System.Exception("Android build failed: " + report.summary.result +
                                       " (" + report.summary.totalErrors + " errors).");

        Debug.Log("Android APK created: " + outputPath);
    }
}
