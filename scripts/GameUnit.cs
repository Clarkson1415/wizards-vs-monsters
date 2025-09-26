using Godot;
using System;
using WizardsVsMonster.scripts;

/// <summary>
/// Represents the game unit on the board.
/// </summary>
[GlobalClass]
public partial class GameUnit : Node2D
{
	[Export] private ClickableUnitComponent clickableUnitComponent;

    [Export] protected UnitBody unitsAreaOrBodyAndArmour;

    public GameUnitResource GetInfo()
	{
		return this.clickableUnitComponent.GetInfo();
    }

    public override void _Ready()
    {
        base._Ready();

        unitsAreaOrBodyAndArmour.Setup(this.GetInfo());
    }
}
