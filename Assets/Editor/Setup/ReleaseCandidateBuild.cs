using System;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace NeonReflexVR.Editor
{
    /// <summary>Temporary release compiler settings; development debugging preferences are restored afterwards.</summary>
    public static class ReleaseCandidateBuild
    {
        const string SessionKey = "NeonReflexVR.ReleaseCompilerArguments";
        [Serializable] sealed class Arguments { public string[] values; }

        public static void Prepare()
        {
            if (EditorApplication.isPlaying || EditorApplication.isCompiling ||
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Use a saved scene outside Play Mode.");
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64 ||
                PlayerSettings.GetScriptingBackend(NamedBuildTarget.Standalone) != ScriptingImplementation.Mono2x)
                throw new InvalidOperationException("Preserve the verified Windows x64 / Mono target.");
            if (!string.IsNullOrEmpty(SessionState.GetString(SessionKey, "")))
                throw new InvalidOperationException("Restore previous release arguments before preparing again.");
            var original = PlayerSettings.GetAdditionalCompilerArguments(NamedBuildTarget.Standalone);
            SessionState.SetString(SessionKey, JsonUtility.ToJson(new Arguments { values = original }));
            // Unity postprocessors require compiler symbols; remap their paths instead of disabling them.
            // This applies only while building the release; PDB/debug files are omitted from the ZIP.
            PlayerSettings.SetAdditionalCompilerArguments(NamedBuildTarget.Standalone,
                original.Concat(new[] { "-pathmap:" + Directory.GetCurrentDirectory() + "=/_/NeonReflexVR" }).ToArray());
            PlayerSettings.bundleVersion = "1.0.0-rc1";
            EditorUserBuildSettings.development = false;
            EditorUserBuildSettings.allowDebugging = false;
            EditorUserBuildSettings.connectProfiler = false;
            EditorUserBuildSettings.buildWithDeepProfilingSupport = false;
        }

        public static void RestoreCompilerArguments()
        {
            var saved = SessionState.GetString(SessionKey, "");
            if (string.IsNullOrEmpty(saved)) return;
            var original = JsonUtility.FromJson<Arguments>(saved);
            PlayerSettings.SetAdditionalCompilerArguments(NamedBuildTarget.Standalone, original.values);
            SessionState.EraseString(SessionKey);
        }
    }
}
