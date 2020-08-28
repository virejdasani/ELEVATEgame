using UnityEditor.Connect;

namespace Unity.Connect.Share.Editor
{
    public class UnityConnectSession
    {
        static UnityConnectSession _instance = new UnityConnectSession();

        public static UnityConnectSession instance
        {
            get => _instance;
        }

        public string GetAccessToken()
        {
            return UnityConnect.instance.GetAccessToken();
        }

        public string GetEnvironment()
        {
            return UnityConnect.instance.GetEnvironment();
        }

        public void ShowLogin()
        {
            UnityConnect.instance.ShowLogin();
        }

        /// <summary>
        /// NOTE no-op if user is not logged in
        /// </summary>
        /// <param name="url"></param>
        public static void OpenAuthorizedURLInWebBrowser(string url) =>
            UnityConnect.instance.OpenAuthorizedURLInWebBrowser(url);

        // TODO UnityConnect Bug, cannot be fully trusted currently, prefer using GetAccessToken() for checking the status.
        public static bool loggedIn => UnityConnect.instance.loggedIn;
    }
}
