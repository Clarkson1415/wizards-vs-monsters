using Godot.Collections;

namespace WizardsVsMonster.scripts
{
    public class GlobalGameVariables
    {
        public static int CELL_SIZE = 16;

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
    }
}
