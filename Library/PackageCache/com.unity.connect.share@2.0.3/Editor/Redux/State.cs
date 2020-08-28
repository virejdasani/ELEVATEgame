using System;

namespace Unity.Connect.Share.Editor
{
    [Serializable]
    public class AppState
    {
        public AppState(
            string title = null, string buildOutputDir = null, string buildGUID = null, string zipPath = null,
            ShareStep step = default, string errorMsg = null, string key = null, string url = null)
        {
            this.title = title;
            this.buildOutputDir = buildOutputDir;
            this.buildGUID = buildGUID;
            this.zipPath = zipPath;
            this.step = step;
            this.errorMsg = errorMsg;
            this.url = url;
            this.key = key;
        }

        public AppState CopyWith(
            string title = null, string buildOutputDir = null, string buildGUID = null, string zipPath = null,
            ShareStep? step = default, string errorMsg = null, string key = null, string url = null)
        {
            return new AppState(
                title: title ?? this.title,
                buildOutputDir: buildOutputDir ?? this.buildOutputDir,
                buildGUID: buildGUID ?? this.buildGUID,
                zipPath: zipPath ?? this.zipPath,
                step: step ?? this.step,
                errorMsg: errorMsg ?? this.errorMsg,
                key: key ?? this.key,
                url: url ?? this.url
            );
        }

        public string title;
        public string buildOutputDir;
        public string buildGUID;
        public string zipPath;
        public ShareStep step;
        public string key;
        public string errorMsg;
        public string url;
    }

    public enum ShareStep
    {
        Idle,
        Login,
        Zip,
        Upload,
        Process
    }
}
