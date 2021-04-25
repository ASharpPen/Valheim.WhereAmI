using BepInEx.Logging;
using System;

namespace Valheim.WhereAmI
{
    internal class Log
    {
        internal static ManualLogSource Logger;

        public static void LogDebug(string message)
        {
            Logger.LogInfo($"{message}");
        }

        public static void LogTrace(string message)
        {
            Logger.LogDebug($"{message}");
        }

        public static void LogInfo(string message) => Logger.LogMessage($"{message}");

        public static void LogWarning(string message) => Logger.LogWarning($"{message}");

        public static void LogError(string message, Exception e = null) => Logger.LogError($"{message}; {e?.Message ?? ""}");
    }
}
