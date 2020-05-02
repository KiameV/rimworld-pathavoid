﻿using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace PathAvoid
{
    public abstract class Designator_PathAvoid : Designator
    {
        private PathAvoidDef def;

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

        public override Color IconDrawColor
        {
            get
            {
                return this.def.color;
            }
        }

        public override string Desc
        {
            get
            {
                return this.def.desc;
            }
        }

        public override string Label
        {
            get
            {
                return this.def.name;
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
        public Designator_PathAvoid() { }

        protected void Initialize(PathAvoidDef def)
        {
            this.icon = ContentFinder<Texture2D>.Get("UI/Designators/PathAvoid", true);
            this.def = def;
            this.useMouseIcon = true;
            this.soundDragSustain = SoundDefOf.Designate_DragStandard;
            this.soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
        }

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
}
