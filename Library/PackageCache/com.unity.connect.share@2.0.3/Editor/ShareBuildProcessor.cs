using System;
using System.Collections;
using System.IO;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Unity.Connect.Share.Editor
{
    class ShareBuildProcessor : IPostprocessBuildWithReport, IPreprocessBuildWithReport
    {
        static bool buildStartedFromTool = false;
        public int callbackOrder { get { return 0; } }

        public void OnPostprocessBuild(BuildReport report)
        {
            BuildSummary summary = report.summary;
            if (summary.platform != BuildTarget.WebGL) { return; }

            string buildOutputDir = summary.outputPath;
            string buildGUID = summary.guid.ToString();

            ShareUtils.AddBuildDirectory(buildOutputDir);

            ShareWindow.FindInstance()?.Store.Dispatch(new BuildFinishAction
            {
                outputDir = buildOutputDir,
                buildGUID = buildGUID
            });

            WriteMetadataFile(summary.outputPath, buildGUID);
        }

        IEnumerator WaitUntilBuildFinishes(BuildReport report)
        {
            /* [NOTE] You might want to use a frame wait instead of a time based one:
             * Building is main thread, and we won't get a frame update until the build is complete.
             * So that would almost certainly wait until right after the build is done and next frame tick,
             * reducing the likely hood of data being unloaded / unavaliable due to
             * cleanup operations which could happen to the build report as variables on the stack are not counted as "in use" for the GC system
             */
            EditorWaitForSeconds waitForSeconds = new EditorWaitForSeconds(1f);
            while (BuildPipeline.isBuildingPlayer)
            {
                yield return waitForSeconds;
            }

            AnalyticsHelper.BuildCompleted(report.summary.result, report.summary.totalTime);
            switch (report.summary.result)
            {
                case BuildResult.Cancelled: Debug.LogWarning("[Version and Build] Build cancelled! " + report.summary.totalTime); break;
                case BuildResult.Failed: Debug.LogError("[Version and Build] Build failed! " + report.summary.totalTime); break;
                case BuildResult.Succeeded: Debug.Log("[Version and Build] Build succeeded! " + report.summary.totalTime); break;
                case BuildResult.Unknown: Debug.Log("[Version and Build] Unknown build result! " + report.summary.totalTime); break;
            }
        }

        /// <summary>
        /// Write metadata files into the build directory
        /// </summary>
        /// <param name="outputPath"></param>
        void WriteMetadataFile(string outputPath, string buildGUID)
        {
            try
            {
                // dependencies.txt: list of "depepedency@version"
                string dependenciesFilePath = $"{outputPath}/dependencies.txt";

                using (StreamWriter streamWriter = new StreamWriter(dependenciesFilePath, false))
                {
                    PackageManagerProxy.GetAllVisiblePackages()
                        .Select(pkg => $"{pkg.name}@{pkg.version}")
                        // We probably don't have the package.json of the used Microgame available,
                        // so add the information manually
                        .Concat(new[] { $"{PackageManagerProxy.GetApplicationIdentifier() ?? Application.productName}@{Application.version}" })
                        .Distinct()
                        .ToList()
                        .ForEach(streamWriter.WriteLine);
                }

                // The Unity version used
                string versionFilePath = $"{outputPath}/ProjectVersion.txt";
                File.Copy("ProjectSettings/ProjectVersion.txt", versionFilePath, true);

                string guidFilePath = $"{outputPath}/GUID.txt";
                File.WriteAllText(guidFilePath, buildGUID);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Triggers the "Build Game" dialog
        /// </summary>
        /// <returns>true if everything goes well and the build is done, false otherwise</returns>
        public static bool OpenBuildGameDialog(BuildTarget activeBuildTarget)
        {
            try
            {
                string path = EditorUtility.SaveFolderPanel("Choose Location of Built Application", "Builds", "");
                if (string.IsNullOrEmpty(path)) { return false; }

                BuildPlayerOptions buildOptions = new BuildPlayerOptions();
                buildOptions.scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);
                buildOptions.locationPathName = path;
                buildOptions.options = BuildOptions.None;
                buildOptions.targetGroup = BuildPipeline.GetBuildTargetGroup(activeBuildTarget);
                buildOptions.target = activeBuildTarget;

                buildStartedFromTool = true; //Debug.Log("building " + buildOptions.locationPathName);
                BuildReport report = BuildPipeline.BuildPlayer(buildOptions); //Debug.LogError("OnPostprocessBuild custom");
                buildStartedFromTool = false;

                AnalyticsHelper.BuildCompleted(report.summary.result, report.summary.totalTime);
                switch (report.summary.result)
                {
                    case BuildResult.Cancelled: //Debug.LogWarning("[Version and Build] Build cancelled! " + report.summary.totalTime);
                    case BuildResult.Failed: //Debug.LogError("[Version and Build] Build failed! " + report.summary.totalTime);
                        return false;

                    case BuildResult.Succeeded: //Debug.Log("[Version and Build] Build succeeded! " + report.summary.totalTime);
                    case BuildResult.Unknown: //Debug.Log("[Version and Build] Unknown build result! " + report.summary.totalTime);
                        break;
                }
            }
            catch (BuildPlayerWindow.BuildMethodException /*e*/)
            {
                //Debug.LogError(e.Message);
                return false;
            }
            return true;
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.WebGL) { return; }
            AnalyticsHelper.BuildStarted(buildStartedFromTool);

            if (buildStartedFromTool) { return; }
            //then we need to wait until the build process finishes, in order to get the proper BuildReport
            EditorCoroutineUtility.StartCoroutineOwnerless(WaitUntilBuildFinishes(report));
        }
    }
}
