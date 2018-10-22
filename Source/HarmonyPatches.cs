using Harmony;
using RimWorld;
using System.Reflection;
using Verse;

namespace PathAvoid
{
    [StaticConstructorOnStartup]
    class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = HarmonyInstance.Create("com.pathavoid.rimworld.mod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Log.Message("Path Avoid: Adding Harmony Postfix to PawnUtility.GetAvoidGrid()");
            //Log.Message("MapGenerator.GenerateMap")
            //Log.Message("MapGenerator.ExposeData")

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

    [HarmonyPatch(typeof(MapGenerator), "GenerateMap")]
    static class Patch_MapGenerator_GenerateMap
    {
        static void Prefix()
        {
            SettingsController.ApplyLevelSettings(DefDatabase<PathAvoidDef>.AllDefs);
        }
    }

    [HarmonyPatch(typeof(Map), "ExposeData")]
    static class Patch_Map_ExposeData
    {
        static void Prefix()
        {
            SettingsController.ApplyLevelSettings(DefDatabase<PathAvoidDef>.AllDefs);
        }
    }
}