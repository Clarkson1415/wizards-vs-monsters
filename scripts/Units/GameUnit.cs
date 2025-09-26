using Godot;
using System;
using WizardsVsMonster.scripts;

/// <summary>
/// Represents the game unit on the board.
/// </summary>
[GlobalClass]
public abstract partial class GameUnit : Node2D
{
	[Export] private ClickableUnitComponent clickableUnitComponent;

    [Export] protected UnitBody unitsAreaOrBodyAndArmour;

    [Export] private Godot.Collections.Array<StatusComponent.STATUS> initialStatuses;

    [Export] private StatusComponent statusComponent;

    public GameUnitResource GetInfo()
	{
		return this.clickableUnitComponent.GetInfo();
    }

    public override void _Ready()
    {
        base._Ready();

        unitsAreaOrBodyAndArmour.Setup(this.GetInfo(), statusComponent);
        statusComponent.InitialiseInitialStatuses(initialStatuses);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        foreach (var status in this.statusComponent.GetActiveStatuses())
        {
            // TODO: add functions to apply statuses.
            // Then in child classes I can override those status application functions to be different if i so want? idk
            // Switch(status)
        }
    }
}
