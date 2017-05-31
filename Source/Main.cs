using Harmony;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using Verse.AI;

namespace PathAvoid
{
    [StaticConstructorOnStartup]
    class Main
    {
        static Main()
        {
            var harmony = HarmonyInstance.Create("com.pathavoid.rimworld.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Log.Message("Path Avoid: Adding Harmony Postfix to PawnUtility.GetAvoidGrid()");
        }
    }

    [HarmonyPatch(typeof(PawnUtility), "GetAvoidGrid")]
    static class Patch_PawnUtility_GetAvoidGrid
    {
        static void Postfix(Pawn p, ref ByteGrid __result)
        {
            PathAvoidGrid.ApplyAvoidGrid(p, ref __result);
        }
    }

    internal static class Util
    {
        internal static AreaManager AreaManager = null;
        internal static Dictionary<IntVec3, bool> BlockedTiles = new Dictionary<IntVec3, bool>();

        public static void AddBlocked(IntVec3 pos)
        {
            BlockedTiles[pos] = true;
        }

        public static void RemoveBlocked(IntVec3 pos)
        {
            BlockedTiles.Remove(pos);
        }

        public static void Init(Map map)
        {
            if (AreaManager == null)
            {
                AreaManager = new AreaManager(map);
                BlockedTiles.Clear();
            }
        }
    }

    [HarmonyPatch(typeof(PathFinder), "GetAllowedArea")]
    static class Patch_PathFinder_GetAllowedArea
    {
        static void PostFix(Pawn pawn, Area __result)
        {
            Log.Warning("Patch_PathFinder_GetAllowedArea");
            Util.Init(pawn.Map);

            Area_Allowed rv = new Area_Allowed(Util.AreaManager, AllowedAreaMode.Any, "temp");

            int count = 0;
            if (__result != null)
            {
                foreach (IntVec3 vec in __result.ActiveCells)
                {
                    if (count < 10)
                    {
                        Log.Warning(count + " __result: " + vec.x + ", " + vec.z + " = " + __result[vec]);
                    }
                    rv[vec] = true;
                }
            }
            count = 0;
            foreach (IntVec3 vec in Util.BlockedTiles.Keys)
            {
                if (count < 10)
                {
                    Log.Warning(count + " area: " + vec.x + ", " + vec.z + " = " + Util.BlockedTiles[vec]);
                }
                rv[vec] = true;
            }
            __result = rv;
        }
    }
}