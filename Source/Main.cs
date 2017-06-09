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

    [HarmonyPatch(typeof(PathFinder), "GetAllowedArea")]
    static class Patch_PathFinder_GetAllowedArea
    {
        static void PostFix(Pawn pawn, Area __result)
        {
            Log.Warning("Patch_PathFinder_GetAllowedArea");
            AvoidAreaUtil.Init(pawn.Map);

            Area_Allowed rv = new Area_Allowed(AvoidAreaUtil.AreaManager, AllowedAreaMode.Any, "temp");

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
            foreach (IntVec3 vec in AvoidAreaUtil.BlockedTiles.Keys)
            {
                if (count < 10)
                {
                    Log.Warning(count + " area: " + vec.x + ", " + vec.z + " = " + AvoidAreaUtil.BlockedTiles[vec]);
                }
                rv[vec] = true;
            }
            __result = rv;
        }
    }
}