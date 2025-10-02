using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
#nullable enable

namespace WizardsVsMonster.scripts
{
    /// <summary>
    /// Represents the state of the players current select game item. Could be unit in game or item from the toolbar.
    /// </summary>
    public partial class GlobalCurrentSelection : Node
    {
        private static GlobalCurrentSelection? instance;

        /// <summary>
        /// Units selected on the battlefield.
        /// </summary>
        public static List<UnitGroup> PlayerGroupsSelected { get; private set; } = [];

        private static GameUnitResource? _lastSelectedToolbarUnit;

        [Signal]
        public delegate void OnItemLastSelectedChangedEventHandler();

        private bool IsGroupEnemyOfPlayer(UnitGroup group)
        {
            return GlobalGameVariables.FactionEnemies[GlobalGameVariables.PlayerControlledFaction].Contains(group.Faction);
        }

        private static GameUnitResource? _lastSelectedUnitsInfo;

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

        public static GlobalCurrentSelection GetInstance()
        {
            if (instance == null)
            {
                instance = new GlobalCurrentSelection();
            }

            return instance;
        }

        /// <summary>
        /// Deselcts all highlighted unit groups.
        /// </summary>
        public void DeselectAll()
        {
            foreach (var group in PlayerGroupsSelected)
            {
                group.ToggleOutlineAllUnits(false);
            }

            PlayerGroupsSelected.Clear();
        }

        public void ClearCursor()
        {
            this.DeselectAll();

            SelectedUnitToSpawn = null;
            LastSelectedUnitsInfo = null;
        }

        private UnitGroup? EnemyGroupHighlighted;

        public void OnGroupClicked(UnitGroup group)
        {
            SelectedUnitToSpawn = null;
            LastSelectedUnitsInfo = group.UnitResource;

            // if enemy then send players units to attack it. If no player units are highlighted though return.
            if (IsGroupEnemyOfPlayer(group) && PlayerGroupsSelected.Count > 0)
            {
                if (EnemyGroupHighlighted != null)
                {
                    EnemyGroupHighlighted.ToggleOutlineAllUnits(false);
                }

                EnemyGroupHighlighted = group;
                group.ToggleOutlineAllUnits(true);
                PlayerGroupsSelected.ForEach(x => x.SetNewTargetEnemy(group));
                return;
            }

            // if only the clicked on is highlighted -> toggle it off.
            if (PlayerGroupsSelected.Count == 1 && PlayerGroupsSelected.Contains(group))
            {
                RemoveGroup(group);
                return;
            }

            TryAddGroup(group);

            if (Input.IsActionPressed("multi select"))
            {
                // if multi select on then can unhilight a single group from collection while maintaining the rest highlighted
                if (PlayerGroupsSelected.Contains(group))
                {
                    RemoveGroup(group);
                }

                return;
            }

            // if shift not held, deseelect all and just add the one clicked on.
            DeselectAll();

            TryAddGroup(group);
        }

        private void TryAddGroup(UnitGroup group)
        {
            if (!PlayerGroupsSelected.Contains(group))
            {
                PlayerGroupsSelected.Add(group);
                group.ToggleOutlineAllUnits(true);
            }
        }

        private void RemoveGroup(UnitGroup group)
        {
            group.ToggleOutlineAllUnits(false);
            PlayerGroupsSelected.Remove(group);
        }

        public void DragSelectionUpdate(Rect2 selectionArea)
        {
            foreach (var troop in UnitGroupSpawnerControl.SpawnedUnitGroups)
            {
                if (troop.UnitsRemaining.Any(x => selectionArea.HasPoint(x.GlobalPosition)))
                {
                    SelectedUnitToSpawn = null;
                    LastSelectedUnitsInfo = troop.UnitResource;

                    TryAddGroup(troop);
                }
                else
                {
                    RemoveGroup(troop);
                }
            }
        }
    }
}
