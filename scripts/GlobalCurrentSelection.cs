using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
#nullable enable

namespace WizardsVsMonster.scripts
{
    /// <summary>
    /// Represents the state of the players current select game item. Could be unit in game or item from the toolbar.
    /// </summary>
    public partial class GlobalCurrentSelection : Node
    {
        private static GlobalCurrentSelection instance = new GlobalCurrentSelection();

        public static GlobalCurrentSelection GetInstance()
        {
            return instance;
        }

        [Signal]
        public delegate void OnItemLastSelectedChangedEventHandler();

        /// <summary>
        /// Units selected on the battlefield.
        /// </summary>
        private static List<UnitGroup> UnitsSelectedOnField = new List<UnitGroup>();

        private static GameUnitResource? _lastSelectedToolbarUnit;

        /// <summary>
        /// The unit to spawn next.
        /// </summary>
        public GameUnitResource? SelectedUnitToSpawn
        {
            get => _lastSelectedToolbarUnit;
            set
            {
                _lastSelectedToolbarUnit = value;
                LastSelectedUnitsInfo = value;
            }
        }

        private static GameUnitResource? _lastSelectedUnitsInfo;

        /// <summary>
        /// Last selected unit whether from toolbar or the battlefield.
        /// </summary>
        public GameUnitResource? LastSelectedUnitsInfo
        {
            get => _lastSelectedUnitsInfo;
            set
            {
                _lastSelectedUnitsInfo = value;
                EmitSignal(SignalName.OnItemLastSelectedChanged);
            }
        }

        public void OnUnitGroupClicked(UnitGroup group)
        {
            LastSelectedUnitsInfo = group.GetGroupUnitType();

            // TODO group selection logic stuff.
            // IDK where the logic for targeting should go maybe here?
            // if group == enemy
            // set target for all units in players groups to that enemy group.
        }
    }
}
