using UnityEngine;
using Verse;

namespace PathAvoid
{
    public class Designator_PathAvoid : Designator
    {
        private PathAvoidDef def;

        public override bool DragDrawMeasurements
        {
            get
            {
                return true;
            }
        }

        public override int DraggableDimensions
        {
            get
            {
                return 2;
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

        public override AcceptanceReport CanDesignateCell(IntVec3 loc)
        {
            return true;
        }

        public Designator_PathAvoid(PathAvoidDef def)
        {
            this.icon = ContentFinder<Texture2D>.Get("UI/Designators/PathAvoid", true);
            this.def = def;
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
            Log.Warning("DesignateSingleCell level " + this.def.level);
            if (this.def.level == 40)
            {
                Util.AddBlocked(c);
            }
            else
            {
                Util.RemoveBlocked(c);
            }
        }

        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
            PathAvoidGrid pathAvoidGrid = base.Map.GetComponent<PathAvoidGrid>();
            if (pathAvoidGrid == null)
            {
                pathAvoidGrid = new PathAvoidGrid(base.Map);
                base.Map.components.Add(pathAvoidGrid);
            }
            pathAvoidGrid.MarkForDraw();
        }
    }
}
