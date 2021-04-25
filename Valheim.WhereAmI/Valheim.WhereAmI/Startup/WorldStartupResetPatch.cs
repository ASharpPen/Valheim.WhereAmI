using HarmonyLib;
using Valheim.WhereAmI.Location;

namespace Valheim.WhereAmI.Startup
{
    [HarmonyPatch(typeof(FejdStartup))]
    public static class WorldStartupResetPatch
    {
        /// <summary>
        /// Singleplayer
        /// </summary>
        [HarmonyPatch("OnWorldStart")]
        [HarmonyPrefix]
        private static void ResetState()
        {
            StateResetter.Reset();
        }

        /// <summary>
        /// Multiplayer
        /// </summary>
        [HarmonyPatch("JoinServer")]
        [HarmonyPrefix]
        private static void ResetStateMultiplayer()
        {
            StateResetter.Reset();

            LocationService.IsMultiplayer = true;
        }
    }
}
