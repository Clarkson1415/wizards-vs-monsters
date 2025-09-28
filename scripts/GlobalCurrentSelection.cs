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

        public void OnGroupClicked(UnitGroup group)
        {
            SelectedUnitToSpawn = null;
            LastSelectedUnitsInfo = group.UnitResource;

            // remove or add group to highlighted groups.
            if (!UnitGroupsHighlighted.Contains(group))
            {
                UnitGroupsHighlighted.Add(group);
                OnGroupAddedToSelected(group);
            }
            else
            {
                UnitGroupsHighlighted.Remove(group);
                OnGroupAddedToSelected(group);
            }
        }

        private void OnGroupAddedToSelected(UnitGroup group)
        {
            if (!IsGroupEnemyOfPlayer(group)) // if unit group added is players group.
            {
                // deselect the enemy groups
                var enemyGroups = UnitGroupsHighlighted.Where(x => IsGroupEnemyOfPlayer(x)).ToList();
                UnitGroupsHighlighted.RemoveAll(x => IsGroupEnemyOfPlayer(x));
                enemyGroups.ForEach(x => x.TellGroupItWasRemovedFromSelection());
            }
            else if (UnitGroupsHighlighted.Any(x => !IsGroupEnemyOfPlayer(x))) // If enemy selected and player has groups selected. set group target to attack it.
            {
                var playersGroups = UnitGroupsHighlighted.Where(x => !IsGroupEnemyOfPlayer(x)).ToList();
                playersGroups.ForEach(x => x.SetNewTargetEnemy(group));
            }
        }

        private void OnGroupRemovedFromSelected(UnitGroup group)
        {
            group.TellGroupItWasRemovedFromSelection();
        }

        public bool IsGroupInSelection(UnitGroup group)
        {
            return UnitGroupsHighlighted.Contains(group);
        }

        private bool IsGroupEnemyOfPlayer(UnitGroup group)
        {
            return GlobalGameVariables.FactionEnemies[GlobalGameVariables.PlayerControlledFaction].Contains(group.Faction);
        }
    }
}
