using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.Connect.Share.Editor
{
    /// <summary>
    /// A collection of utility methods used by the Share Package
    /// </summary>
    public static class ShareUtils
    {
        public const int MAX_DISPLAYED_BUILDS = 10;
        const string PROJECT_VERSION_REGEX = "^\\d{4}\\.\\d{1}\\Z";

        public const string DefaultGameName = "Untitled";

        public static List<string> GetAllBuildsDirectories()
        {
            List<string> result = Enumerable.Repeat(string.Empty, MAX_DISPLAYED_BUILDS).ToList();
            string path = GetEditorPreference("buildOutputDirList");

            if (string.IsNullOrEmpty(path)) { return result; }

            List<string> existingPaths = path.Split(';').ToList();
            for (int i = 0; i < existingPaths.Count; i++)
            {
                result[i] = existingPaths[i];
            }
            return result;
        }

        public static void AddBuildDirectory(string buildPath)
        {
            string path = GetEditorPreference("buildOutputDirList");
            List<string> buildPaths = path.Split(';').ToList();
            if (buildPaths.Contains(buildPath)) { return; }

            while (buildPaths.Count < MAX_DISPLAYED_BUILDS)
            {
                buildPaths.Add(string.Empty);
            }

            //Left Shift
            for (int i = MAX_DISPLAYED_BUILDS - 1; i > 0; i--)
            {
                buildPaths[i] = buildPaths[i - 1];
            }

            buildPaths[0] = buildPath;
            SetEditorPreference("buildOutputDirList", string.Join(";", buildPaths));
        }

        public static void RemoveBuildDirectory(string buildPath)
        {
            List<string> buildPaths = GetEditorPreference("buildOutputDirList").Split(';').ToList();

            buildPaths.Remove(buildPath);

            while (buildPaths.Count < MAX_DISPLAYED_BUILDS)
            {
                buildPaths.Add(string.Empty);
            }

            SetEditorPreference("buildOutputDirList", string.Join(";", buildPaths));
        }

        public static bool ValidBuildExists() => !string.IsNullOrEmpty(GetFirstValidBuildPath());

        public static string GetFirstValidBuildPath() => GetAllBuildsDirectories().FirstOrDefault(BuildIsValid);

        public static bool BuildIsValid(string buildPath)
        {
            if (string.IsNullOrEmpty(buildPath)) { return false; }

            string unityVersionOfBuild = GetUnityVersionOfBuild(buildPath); //UnityEngine.Debug.Log("unity version: " + unityVersionOfBuild);
            if (string.IsNullOrEmpty(unityVersionOfBuild)) { return false; }

            string descriptorFileName = buildPath.Split('/').Last();

            switch (unityVersionOfBuild)
            {
                case "2019.3": return BuildIsCompatibleFor2019_3(buildPath, descriptorFileName);
                case "2020.2": return BuildIsCompatibleFor2020_2(buildPath, descriptorFileName);
                default: return true; //if we don't know the exact build structure for other unity versions, we assume the build is valid
            }
        }

        public static bool BuildIsCompatibleFor2019_3(string buildPath, string descriptorFileName)
        {
            return File.Exists(Path.Combine(buildPath, string.Format("Build/{0}.data.unityweb", descriptorFileName)))
                && File.Exists(Path.Combine(buildPath, string.Format("Build/{0}.wasm.code.unityweb", descriptorFileName)))
                && File.Exists(Path.Combine(buildPath, string.Format("Build/{0}.wasm.framework.unityweb", descriptorFileName)))
                && File.Exists(Path.Combine(buildPath, string.Format("Build/{0}.json", descriptorFileName)))
                && File.Exists(Path.Combine(buildPath, string.Format("Build/UnityLoader.js", descriptorFileName)));
        }

        public static bool BuildIsCompatibleFor2020_2(string buildPath, string descriptorFileName)
        {
            string buildFilesPath = Path.Combine(buildPath, "Build/");
            return Directory.GetFiles(buildFilesPath, string.Format("{0}.data.*", descriptorFileName)).Length > 0
                && Directory.GetFiles(buildFilesPath, string.Format("{0}.framework.js.*", descriptorFileName)).Length > 0
                && File.Exists(Path.Combine(buildPath, string.Format("Build/{0}.loader.js", descriptorFileName)))
                && Directory.GetFiles(buildFilesPath, string.Format("{0}.wasm.*", descriptorFileName)).Length > 0;
        }

        public static string GetUnityVersionOfBuild(string buildPath)
        {
            if (string.IsNullOrEmpty(buildPath)) { return string.Empty; }

            string versionFile = Path.Combine(buildPath, "ProjectVersion.txt");
            if (!File.Exists(versionFile)) { return string.Empty; }

            string version = File.ReadAllLines(versionFile)[0].Split(' ')[1].Substring(0, 6); //The row is something like: m_EditorVersion: 2019.3.4f1, so it will return 2019.3
            return Regex.IsMatch(version, PROJECT_VERSION_REGEX) ?  version : string.Empty;
        }

        public static string GetThumbnailPath() { return GetEditorPreference("thumbnailPath"); }
        public static void SetThumbnailPath(string path) { SetEditorPreference("thumbnailPath", path); }

        public static void SetEditorPreference(string key, string value) { ShareSettingsManager.instance.Set(key, value, SettingsScope.Project); }
        public static string GetEditorPreference(string key)
        {
            string result = ShareSettingsManager.instance.Get<string>(key, SettingsScope.Project);
            if (result == null)
            {
                result = string.Empty;
                SetEditorPreference(key, result);
            }
            return result;
        }

        public static string GetFilteredGameTitle(string currentGameTitle)
        {
            if (string.IsNullOrEmpty(currentGameTitle?.Trim())) { return DefaultGameName; }

            return currentGameTitle;
        }

        /// <summary>
        /// Supports GB, MB, KB, or B
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns>xB with two decimals, B with zero decimals</returns>
        public static string FormatBytes(ulong bytes)
        {
            double gb = bytes / (1024.0 * 1024.0 * 1024.0);
            double mb = bytes / (1024.0 * 1024.0);
            double kb = bytes / 1024.0;
            // Use :#.000 to specify further precision if wanted
            if (mb >= 1000) return $"{gb:#.00} GB";
            if (kb >= 1000) return $"{mb:#.00} MB";
            if (kb >= 1) return $"{kb:#.00} KB";
            return $"{bytes} B";
        }

        public static ulong GetSizeFolderSize(string folder)
        {
            ulong size = 0;
            DirectoryInfo directoryInfo = new DirectoryInfo(folder);
            foreach (FileInfo fileInfo in directoryInfo.GetFiles("*", SearchOption.AllDirectories))
            {
                size += (ulong)fileInfo.Length;
            }
            return size;
        }

        /// <summary>
        /// Allows a visual element to react on left click
        /// </summary>
        public class LeftClickManipulator : MouseManipulator
        {
            Action<VisualElement> OnClick;
            bool active;

            public LeftClickManipulator(Action<VisualElement> OnClick)
            {
                activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
                this.OnClick = OnClick;
            }

            protected override void RegisterCallbacksOnTarget()
            {
                target.RegisterCallback<MouseDownEvent>(OnMouseDown);
                target.RegisterCallback<MouseUpEvent>(OnMouseUp);
            }

            protected override void UnregisterCallbacksFromTarget()
            {
                target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
                target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            }

            protected void OnMouseDown(MouseDownEvent e)
            {
                if (active)
                {
                    e.StopImmediatePropagation();
                    return;
                }

                if (CanStartManipulation(e))
                {
                    active = true;
                    target.CaptureMouse();
                    e.StopPropagation();
                }
            }

            protected void OnMouseUp(MouseUpEvent e)
            {
                if (!active || !target.HasMouseCapture() || !CanStopManipulation(e)) { return; }

                active = false;
                target.ReleaseMouse();
                e.StopPropagation();

                if (OnClick == null) { return; }
                OnClick.Invoke(target);
            }
        }
    }
}
