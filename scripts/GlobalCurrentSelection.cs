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
        private static List<ClickableUnitComponent> UnitsSelectedOnField = new List<ClickableUnitComponent>();

        private static GameUnitResource? _lastSelectedToolbarUnit;

        /// <summary>
        /// The current toolbar item selected. This will clear any selected units on the battlefield.
        /// </summary>
        public GameUnitResource? SelectedToolbarUnitsInfo
        {
            get => _lastSelectedToolbarUnit;
            set
            {
                _lastSelectedToolbarUnit = value;
                UnSelectAllUnits();
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

        public void AddUnit(ClickableUnitComponent unit)
        {
            if (unit.IsGoodGuys)
            {
                // deselect bad guys.
                var baddies = UnitsSelectedOnField.Where(u => !u.IsGoodGuys).ToList();
                foreach (var baddy in baddies)
                {
                    baddy.UnHighlight();
                    UnitsSelectedOnField.Remove(baddy);
                }
            }

            if (unit.IsSelected)
            {
                LastSelectedUnitsInfo = unit.GetInfo();
                if (!UnitsSelectedOnField.Contains(unit))
                {
                    UnitsSelectedOnField.Add(unit);
                }
            }
            else
            {
                LastSelectedUnitsInfo = null;
                UnitsSelectedOnField.Remove(unit);
            }
        }

        private void UnSelectAllUnits()
        {
            foreach (var unit in UnitsSelectedOnField)
            {
                unit.UnHighlight();
            }

            UnitsSelectedOnField = new List<ClickableUnitComponent>();
        }
    }
}
