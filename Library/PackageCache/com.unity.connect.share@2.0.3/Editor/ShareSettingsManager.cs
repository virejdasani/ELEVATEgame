using UnityEditor;
using UnityEditor.SettingsManagement;

namespace Unity.Connect.Share.Editor
{
    static class ShareSettingsManager
    {
        internal const string k_PackageName = "com.unity.connect.share";

        static Settings s_Instance;

        internal static Settings instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new Settings(k_PackageName);
                }
                return s_Instance;
            }
        }

        // Register a new SettingsProvider that will scrape the owning assembly for [UserSetting] marked fields.
        [SettingsProvider]
        static SettingsProvider CreateSettingsProvider()
        {
            var provider = new UserSettingsProvider("Preferences/WebGL Publisher",
                instance,
                new[] { typeof(ShareSettingsManager).Assembly });

            return provider;
        }
    }
}
