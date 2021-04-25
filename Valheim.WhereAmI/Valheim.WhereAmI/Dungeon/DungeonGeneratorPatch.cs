using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Valheim.WhereAmI.Dungeon
{
    [HarmonyPatch(typeof(DungeonGenerator))]
    public static class DungeonGeneratorPatch
    {
        private static MethodInfo Anchor = AccessTools.Method(typeof(GameObject), "GetComponent", generics: new[] { typeof(Room) });
        private static MethodInfo Detour = AccessTools.Method(typeof(DungeonGeneratorPatch), nameof(CacheRoom), new[] { typeof(Room) });

        [HarmonyPatch("PlaceRoom", new[] { typeof(DungeonDB.RoomData), typeof(Vector3), typeof(Quaternion), typeof(RoomConnection), typeof(ZoneSystem.SpawnMode) })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> HookRoomObject(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(true, new CodeMatch(OpCodes.Callvirt, Anchor))
                .Advance(2)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_1))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, Detour))
                .InstructionEnumeration();
        }

        private static void CacheRoom(Room component)
        {
            RoomCache.AddRoom(component);
        }
    }
}
