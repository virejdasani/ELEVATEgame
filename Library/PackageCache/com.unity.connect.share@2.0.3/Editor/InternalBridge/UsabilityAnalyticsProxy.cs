using UnityEditor;

namespace Unity.Connect.Share.Editor
{
    static public class UsabilityAnalyticsProxy
    {
        public static void SendEvent(string eventType, System.DateTime startTime, System.TimeSpan duration, bool isBlocking, object parameters)
        {
            UsabilityAnalytics.SendEvent(eventType, startTime, duration, isBlocking, parameters);
        }
    }
}
