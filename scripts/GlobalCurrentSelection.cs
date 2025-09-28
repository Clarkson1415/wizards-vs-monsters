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
        public static List<UnitGroup> UnitGroupsHighlighted { get; private set; } = [];

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

        public bool OnGroupClicked(UnitGroup group)
        {
            SelectedUnitToSpawn = null;
            LastSelectedUnitsInfo = group.GetUnitsInfo();

            if (!UnitGroupsHighlighted.Contains(group))
            {
                UnitGroupsHighlighted.Add(group);
                return true;
            }
            else
            {
                UnitGroupsHighlighted.Remove(group);
                return false;
            }
        }
    }
}
