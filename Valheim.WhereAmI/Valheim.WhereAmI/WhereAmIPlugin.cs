using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace Valheim.WhereAmI
{
    [BepInPlugin("asharppen.valheim.where_am_i", "Where Am I", "1.0.0")]
    public class WhereAmIPlugin : BaseUnityPlugin
    {
        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            Config = base.Config;

            Log.Logger = Logger;

            new Harmony("mod.where_am_i").PatchAll();
        }

        internal static new ConfigFile Config;
    }
}
