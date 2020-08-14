﻿using System;
using System.Collections.Generic;
using Verse;
using UnityEngine;
using RimWorld;

namespace PathAvoid
{
    public class PathAvoidGrid : MapComponent
    {
        private class PathAvoidLevel : Verse.ICellBoolGiver
        {
            private PathAvoidGrid grid;

            private Color color;

            private byte level;

            public Color Color
            {
                get
                {
                    return this.color;
                }
            }
            public PathAvoidLevel() { }

            public void Initialize(PathAvoidGrid grid, byte level, Color color)
            {
                this.grid = grid;
                this.level = level;
                this.color = color;
            }

            public bool GetCellBool(int index)
            {
                return this.grid.grid[index] == this.level;
            }

            public Color GetCellExtraColor(int index)
            {
                return Color.white;
            }
        }

        private ByteGrid grid;

        private List<CellBoolDrawer> LevelDrawers;

        private bool drawMarked = false;

        public PathAvoidGrid(Map map) : base(map)
        {
            this.grid = new ByteGrid(map);
            for (int i = 0; i < map.cellIndices.NumGridCells; i++)
            {
                this.grid[i] = SettingsController.GetBaseGridValue();
            }
        }

        public override void ExposeData()
        {
            this.grid.ExposeData();
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                List<byte> list = new List<byte>();
                foreach (PathAvoidDef current in DefDatabase<PathAvoidDef>.AllDefs)
                {
                    list.Add((byte)current.level);
                }
                for (int i = 0; i < this.map.cellIndices.NumGridCells; i++)
                {
                    if (!list.Contains(this.grid[i]))
                    {
                        byte b = list[0];
                        int num = Math.Abs((int)(b - this.grid[i]));
                        foreach (byte current2 in list)
                        {
                            if (Math.Abs((int)(current2 - this.grid[i])) < num)
                            {
                                b = current2;
                                num = Math.Abs((int)(b - this.grid[i]));
                            }
                        }
                        this.grid[i] = b;
                    }
                }
            }
        }

        public void MarkForDraw()
        {
            this.drawMarked = true;
        }

        public override void MapComponentUpdate()
        {
            if (this.drawMarked)
            {
                if (this.LevelDrawers == null)
                {
                    this.BuildLevelDrawers();
                }
                foreach (CellBoolDrawer current in this.LevelDrawers)
                {
                    current.MarkForDraw();
                    current.CellBoolDrawerUpdate();
                }
                this.drawMarked = false;
            }
        }

        public void BuildLevelDrawers()
        {
            this.LevelDrawers = new List<CellBoolDrawer>();
            foreach (PathAvoidDef current in DefDatabase<PathAvoidDef>.AllDefsListForReading)
            {
                if (current.isPrefer && !Settings.IsPreferredEnabled)
                    continue;

                if (current.display)
                {
                    var p = new PathAvoidGrid.PathAvoidLevel();
                    p.Initialize(this, (byte)current.level, current.color);
                    CellBoolDrawer item = new CellBoolDrawer(p, this.map.Size.x, this.map.Size.z);
                    this.LevelDrawers.Add(item);
                }
            }
        }

        public void SetValue(IntVec3 pos, byte val)
        {
            this.grid[pos] = val;
            bool flag = this.LevelDrawers != null;
            if (flag)
            {
                foreach (CellBoolDrawer current in this.LevelDrawers)
                {
                    current.SetDirty();
                }
            }
        }

        public void SetValue(int pos, byte val)
        {
            this.grid[pos] = val;
            bool flag = this.LevelDrawers != null;
            if (flag)
            {
                foreach (CellBoolDrawer current in this.LevelDrawers)
                {
                    current.SetDirty();
                }
            }
        }

        public static void ApplyAvoidGrid(Pawn p, ref ByteGrid result)
        {
            if (result == null && 
                p.Faction != null && 
                p.Faction.def.canUseAvoidGrid &&
                IsFactionFriendly(p.Faction))
            {
                PathAvoidGrid pathAvoidGrid = p.Map.GetComponent<PathAvoidGrid>();
                if (pathAvoidGrid == null)
                {
                    pathAvoidGrid = new PathAvoidGrid(p.Map);
                    p.Map.components.Add(pathAvoidGrid);
                }
                result = pathAvoidGrid.grid;
            }
        }

        private static bool IsFactionFriendly(Faction f)
        {
            if (f == Faction.OfPlayer)
                return true;

            FactionRelationKind kind = f.RelationWith(Faction.OfPlayer, false).kind;
            return kind != FactionRelationKind.Hostile;
        }
    }
}
