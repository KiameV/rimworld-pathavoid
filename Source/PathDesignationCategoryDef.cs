using System.Collections.Generic;
using Verse;

namespace PathAvoid
{
    internal class PathDesignationCategoryDef : DesignationCategoryDef
    {
        public override void ResolveReferences()
        {
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                List<Designator> list = base.AllResolvedDesignators;
                foreach (PathAvoidDef current in DefDatabase<PathAvoidDef>.AllDefs)
                {
                    if (!Settings.IsPreferredEnabled && current.name.Equals("Prefer"))
                    {
                        continue;
                    }
                    var d = new Designator_PathAvoid();
                    d.Initialize(current);
                    list.Add(d);
                }
            });
        }
    }
}