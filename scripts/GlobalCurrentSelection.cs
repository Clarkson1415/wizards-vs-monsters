using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WizardsVsMonster.scripts
{
    /// <summary>
    /// Represents the state of the players current select game item. Could be unit in game or item from the toolbar.
    /// </summary>
    public partial class GlobalCurrentSelection : Node
    {
        private static List<ClickableUnitComponent> _unitsSelected = new List<ClickableUnitComponent>();

        private static GameUnitResource _toolbarUnit;

        private static GlobalCurrentSelection instance = new GlobalCurrentSelection();

        public static GlobalCurrentSelection GetInstance()
        {
            return instance;
        }

        [Signal]
        public delegate void OnToolbarItemSelectedChangedEventHandler(GameUnitResource data);

        public void AddUnit(ClickableUnitComponent unit)
        {
            _unitsSelected.Add(unit);
            _toolbarUnit = null;
            EmitSignal(SignalName.OnToolbarItemSelectedChanged, unit.GetInfo());
        }

        public GameUnitResource SelectedToolbarUnit
        {
            get => _toolbarUnit;
            set
            {
                _toolbarUnit = value;
                UnHighlightUnits();
                _unitsSelected = new List<ClickableUnitComponent>();
                EmitSignal(SignalName.OnToolbarItemSelectedChanged, value);
            }
        }

        private void UnHighlightUnits()
        {
            foreach (var unit in _unitsSelected)
            {
                unit.UnHighlight();
            }
        }
    }
}
