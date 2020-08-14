using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace PathAvoid
{
    class MapSettingsDialog : Window
    {
        private SortedDictionary<string, TerrainDef> terrain = new SortedDictionary<string, TerrainDef>();
        private SortedDictionary<int, PathAvoidDef> pathAvoidOptions = new SortedDictionary<int, PathAvoidDef>();

        private TerrainDef selectedTerrainDef = null;
        private PathAvoidDef selectedPathAvoidDef = null;

        private PathAvoidDef selectedPathAvoidDefForReset = null;
        public override Vector2 InitialSize => new Vector2(500f, 600f);

        public MapSettingsDialog()
        {
            base.doCloseButton = true;
            base.doCloseX = true;
            base.forcePause = true;
            base.absorbInputAroundWindow = true;

            foreach (var tpm in Current.Game.CurrentMap.Biome.terrainPatchMakers)
            {
                foreach (var t in tpm.thresholds)
                {
                    terrain[t.terrain.defName] = t.terrain; 
                }
            }

            foreach (var pad in DefDatabase<PathAvoidDef>.AllDefsListForReading)
            {
                pathAvoidOptions[pad.level] = pad;
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            float x = inRect.xMin + 10;
            float y = 0;

            Widgets.Label(new Rect(x, y, 300, 32), "PathAvoid.PathAvoidGlobalSettings".Translate());
            y += 60;
            Widgets.Label(new Rect(x, y, 300, 32), "PathAvoid.ApplyToTerrainType".Translate());
            y += 40;

            x += 10;
            if (Widgets.ButtonText(new Rect(x, y, 200, 32), (selectedTerrainDef == null) ? (string)"PathAvoid.TerrainType".Translate() : selectedTerrainDef.defName))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (var v in terrain.Values)
                {
                    list.Add(new FloatMenuOption(v.defName, delegate
                    {
                        selectedTerrainDef = v;
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }
            if (Widgets.ButtonText(new Rect(x + 220, y, 200, 32), (selectedPathAvoidDef == null) ? (string)"PathAvoid.PathAvoidType".Translate() : selectedPathAvoidDef.name))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (var v in pathAvoidOptions.Values)
                {
                    list.Add(new FloatMenuOption(v.name, delegate
                    {
                        selectedPathAvoidDef = v;
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }
            y += 40;

            if (selectedTerrainDef != null && selectedPathAvoidDef != null)
            {
                if (Widgets.ButtonText(new Rect(x, y, 200, 32), "Accept".Translate()))
                {
                    var map = Current.Game.CurrentMap;
                    var grid = map.GetComponent<PathAvoidGrid>();
                    if (grid != null)
                    {
                        for(int i = 0; i < map.terrainGrid.topGrid.Length; ++i)
                        {
                            if (map.terrainGrid.TerrainAt(i) == selectedTerrainDef)
                            {
                                grid.SetValue(i, (byte)selectedPathAvoidDef.level);
                            }
                        }
                    }
                    else
                    {
                        Log.Error("failed to get path avoid grid");
                    }
                }
            }
            x -= 10;
            y += 50;

            Widgets.DrawLineHorizontal(x, y, inRect.width - x * 2);
            y += 10;

            Widgets.Label(new Rect(x, y, 300, 32), "PathAvoid.ApplyToAllTiles".Translate());
            y += 40;

            x += 10;
            if (Widgets.ButtonText(new Rect(x, y, 200, 32), (selectedPathAvoidDefForReset == null) ? (string)"PathAvoid.PathAvoidType".Translate() : selectedPathAvoidDefForReset.name))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (var v in pathAvoidOptions.Values)
                {
                    list.Add(new FloatMenuOption(v.name, delegate
                    {
                        selectedPathAvoidDefForReset = v;
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }
            y += 40;
            if (selectedPathAvoidDefForReset != null)
            {
                if (Widgets.ButtonText(new Rect(x, y, 200, 32), "Accept".Translate()))
                {
                    var map = Current.Game.CurrentMap;
                    var grid = map.GetComponent<PathAvoidGrid>();
                    if (grid != null)
                    {
                        for (int i = 0; i < map.terrainGrid.topGrid.Length; ++i)
                        {
                            grid.SetValue(i, (byte)selectedPathAvoidDefForReset.level);
                        }
                    }
                    else
                    {
                        Log.Error("failed to get path avoid grid");
                    }
                }
            }
            x -= 10;
        }
    }
}
