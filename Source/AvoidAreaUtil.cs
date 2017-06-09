using System.Collections.Generic;
using Verse;

namespace PathAvoid
{
    class AvoidAreaUtil
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
}
