using Godot;
using Godot.Collections;

namespace WizardsVsMonster.scripts
{
    public class GlobalGameVariables
    {
        public static int CELL_SIZE = 16;

        /// <summary>
        /// The drag map setting. if true, touch drags draws the selection box. if false touch pans the map.
        /// </summary>
        public static bool TouchScreenDragSetting_DragSelectOn;

        /// <summary>
        /// Collision layer the the area2ds of the trees or other view obstructors are on.
        /// </summary>
        public static int ViewBlockingAreaCollisionLayer = 2;

        public enum FACTION { humans, monsters };

        public static FACTION PlayerControlledFaction = FACTION.humans;

        public static Dictionary<FACTION, Godot.Color> FACTION_COLORS = new()
        {
            { FACTION.humans, Godot.Color.Color8(0, 90, 233) },
            { FACTION.monsters, Godot.Color.Color8(165, 12, 20) }
        };

        public static Dictionary<FACTION, Array<FACTION>> FactionEnemies = new()
        {
            { FACTION.humans, [FACTION.monsters] },
            { FACTION.monsters, [FACTION.humans] }
        };

        /// <summary>
        /// How many seconds until 'fresh' status wears off.
        /// </summary>
        public static double FRESH_STATUS_TIME = 5;

        public static float FRESH_SPEED_MODIFIER = 20;

        /// <summary>
        /// Returns vector 2 left if on x < 0 on the let side of the board else  right if x > 0
        /// </summary>
        /// <param name="position">unit at</param>
        /// <returns></returns>
        public static Vector2 GetDefaultDirection(Vector2 position)
        {   
            // TODO use the tilemap size and calc half. could be north or south? instead of left right
            return position.X > 0 ? Vector2.Left : Vector2.Right;
        }
    }
}
