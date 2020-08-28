using System.Linq;
using UnityEngine;

namespace Unity.Connect.Share.Editor
{
    public class ShareAction {}
    public class ShareStartAction : ShareAction
    {
        public string title;
        public string buildPath;
    }
    public class BuildFinishAction : ShareAction
    {
        public string outputDir;
        public string buildGUID;
    }

    public class ZipPathChangeAction : ShareAction
    {
        public string zipPath;
    }

    public class UploadStartAction : ShareAction
    {
        public string buildGUID;
    }

    public class UploadProgressAction : ShareAction
    {
        public int progress;
    }

    public class QueryProgressAction : ShareAction
    {
        public string key;
    }

    public class QueryProgressResponseAction : ShareAction
    {
        public GetProgressResponse response;
    }

    public class TitleChangeAction : ShareAction
    {
        public string title;
    }

    public class DestroyAction : ShareAction {}

    public class OnErrorAction : ShareAction
    {
        public string errorMsg;
    }

    public class StopUploadAction : ShareAction {}

    public class NotLoginAction : ShareAction {}

    public class LoginAction : ShareAction {}

    public class ShareReducer
    {
        public static AppState reducer(AppState old, object action)
        {
            switch (action)
            {
                case BuildFinishAction build:
                    return old.CopyWith(
                        buildOutputDir: build.outputDir,
                        buildGUID: build.buildGUID
                    );

                case ZipPathChangeAction zip:
                    return old.CopyWith(
                        zipPath: zip.zipPath,
                        step: ShareStep.Zip
                    );

                case UploadStartAction upload:
                    AnalyticsHelper.UploadStarted();
                    return old.CopyWith(step: ShareStep.Upload);

                case QueryProgressAction query:

                    return old.CopyWith(
                        step: ShareStep.Process,
                        key: query.key
                    );

                case UploadProgressAction upload:
                    ShareWindow.FindInstance()?.OnUploadProgress(upload.progress);
                    return old;

                case QueryProgressResponseAction queryResponse:
                    ShareStep? step = null;
                    if (queryResponse.response.progress == 100)
                    {
                        step = ShareStep.Idle;
                    }

                    ShareWindow.FindInstance()?.OnProcessingProgress(queryResponse.response.progress);
                    return old.CopyWith(url: queryResponse.response.url, step: step);

                case TitleChangeAction titleChangeAction: return old.CopyWith(title: titleChangeAction.title);

                case DestroyAction destroyAction: return new AppState(buildOutputDir: old.buildOutputDir, buildGUID: old.buildGUID);

                case OnErrorAction errorAction: return old.CopyWith(errorMsg: errorAction.errorMsg);

                case StopUploadAction stopUploadAction: return new AppState(buildOutputDir: old.buildOutputDir, buildGUID: old.buildGUID);

                case NotLoginAction login: return old.CopyWith(step: ShareStep.Login);

                case LoginAction login: return old.CopyWith(step: ShareStep.Idle);
            }
            return old;
        }
    }
}
