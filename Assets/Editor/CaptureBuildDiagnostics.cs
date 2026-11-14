using System.IO;
using UnityEditor;
using UnityEngine;

// Preserve editor errors without publishing the full log or authentication output.
[InitializeOnLoad]
public static class CaptureBuildDiagnostics
{
    static CaptureBuildDiagnostics()
    {
        Application.logMessageReceived += Capture;
    }
    static void Capture(string message, string stackTrace, LogType type)
    {
        if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
        try
        {
            string directory = Path.GetFullPath("build");
            Directory.CreateDirectory(directory);
            File.AppendAllText(Path.Combine(directory,"unity-errors.log"), message + "\n" + stackTrace + "\n");
        }
        catch { /* Diagnostics must not replace the original error. */ }
    }
}
