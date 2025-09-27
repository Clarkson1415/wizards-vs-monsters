using Godot;
using System;
using System.Collections.Generic;
using WizardsVsMonster.scripts;

/// <summary>
/// Represents the game unit on the board.
/// </summary>
[GlobalClass]
public abstract partial class GameUnit : Node2D
{
    [Export] private ClickableUnitComponent clickableUnitComponent;

    [Export] protected UnitBody unitsAreaOrBodyAndArmour;

    [Export] private GameUnitResource resource;

    public float GetCurrentHealth()
    {
        return unitsAreaOrBodyAndArmour.GetCurrentHealth();
    }

    public ClickableUnitComponent GetClickableUnitComponent()
    {
        return clickableUnitComponent;
    }

    public GameUnitResource GetInfo()
    {
        return this.resource;
    }

    public override void _Ready()
    {
        base._Ready();

        if (resource == null)
        {
            Logger.LogError($"unit data resource not loaded for {this.Name}");
        }

        clickableUnitComponent.Setup(resource);
        unitsAreaOrBodyAndArmour.Setup(this.GetInfo());
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public void ApplyStatusEffects(Godot.Collections.Array<StatusComponent.STATUS> statuses)
    {
        foreach (var status in statuses)
        {
            // TODO the actual stat changes.
        }
    }
}
