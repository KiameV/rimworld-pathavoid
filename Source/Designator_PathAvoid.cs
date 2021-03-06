﻿using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace PathAvoid
{
    public abstract class ADesignator_PathAvoid : Designator
    {
        public override int DraggableDimensions
        {
            get
            {
                return 2;
            }
        }

        public override bool DragDrawMeasurements
        {
            get
            {
                return true;
            }
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(base.Map))
            {
                return false;
            }
            return true;
        }
        public ADesignator_PathAvoid() { }

        protected virtual void Initialize(PathAvoidDef def)
        {
            base.icon = ContentFinder<Texture2D>.Get("UI/Designators/PathAvoid", true);
            base.hotKey = def.hotKey;
            base.useMouseIcon = true;
            base.soundDragSustain = SoundDefOf.Designate_DragStandard;
            base.soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
        }
    }

    public abstract class Designator_PathAvoid : ADesignator_PathAvoid
    {
        protected PathAvoidDef def;

        protected override void Initialize(PathAvoidDef def)
        {
            base.Initialize(def);
            this.def = def;
        }

        public override Color IconDrawColor => this.def.color;

        public override string Desc => this.def.desc;

        public override string Label => this.def.name;

        public override void DesignateSingleCell(IntVec3 c)
        {
            PathAvoidGrid pathAvoidGrid = base.Map.GetComponent<PathAvoidGrid>();
            if (pathAvoidGrid == null)
            {
                pathAvoidGrid = new PathAvoidGrid(base.Map);
                base.Map.components.Add(pathAvoidGrid);
            }
            pathAvoidGrid.SetValue(c, (byte)this.def.level);
        }

        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
            PathAvoidGrid pathAvoidGrid = base.Map.GetComponent<PathAvoidGrid>();
            bool flag = pathAvoidGrid == null;
            if (flag)
            {
                pathAvoidGrid = new PathAvoidGrid(base.Map);
                base.Map.components.Add(pathAvoidGrid);
            }
            pathAvoidGrid.MarkForDraw();
        }

        public override void RenderHighlight(List<IntVec3> dragCells)
        {
            DesignatorUtility.RenderHighlightOverSelectableCells(this, dragCells);
        }
    }

    public class Designator_PathAvoid_Prefer : Designator_PathAvoid
    {
        public Designator_PathAvoid_Prefer()
        {
            Initialize(DefDatabase<PathAvoidDef>.GetNamed("PathAvoidPrefer"));
        }
    }

    public class Designator_PathAvoid_Normal : Designator_PathAvoid
    {
        public Designator_PathAvoid_Normal()
        {
            Initialize(DefDatabase<PathAvoidDef>.GetNamed("PathAvoidNormal"));
        }
    }

    public class Designator_PathAvoid_Dislike : Designator_PathAvoid
    {
        public Designator_PathAvoid_Dislike()
        {
            Initialize(DefDatabase<PathAvoidDef>.GetNamed("PathAvoidDislike"));
        }
    }

    public class Designator_PathAvoid_Hate : Designator_PathAvoid
    {
        public Designator_PathAvoid_Hate()
        {
            Initialize(DefDatabase<PathAvoidDef>.GetNamed("PathAvoidHate"));
        }
    }

    public class Designator_PathAvoid_Strong : Designator_PathAvoid
    {
        public Designator_PathAvoid_Strong()
        {
            Initialize(DefDatabase<PathAvoidDef>.GetNamed("PathAvoidStrong"));
        }
    }

    public class Designator_MapSettings : ADesignator_PathAvoid
    {
        private MapSettingsDef def;

        public override Color IconDrawColor => Color.white;

        public override string Label => this.def.name;

        public Designator_MapSettings()
        {
            this.def = DefDatabase<MapSettingsDef>.GetNamed("PathAvoidMapSettings");
            base.icon = ContentFinder<Texture2D>.Get("UI/Designators/PathAvoid", true);
            base.hotKey = def.hotKey;
            base.useMouseIcon = true;
            base.soundDragSustain = SoundDefOf.Designate_DragStandard;
            base.soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
        }

        public override void Selected()
        {
            base.Selected();
            Find.WindowStack.Add(new MapSettingsDialog());
        }
    }
}
