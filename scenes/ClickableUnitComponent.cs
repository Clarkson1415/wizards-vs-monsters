using Godot;
using System;
using WizardsVsMonster.scripts;

/// <summary>
/// A component to go on the game unit scenes that fight.
/// </summary>
public partial class ClickableUnitComponent : Button
{
    [Export] private int unitSquareSize = 16;

    [Export] private UnitIndicator unitIndicatorLight;

    [Export] private Area2D area2d;

    // Cant store a resource in a csharp node in a subscene of the game scene.
    [Export] private string pathToData;

    private GameUnitResource resource;

    public GameUnitResource GetInfo()
    {
        return resource;
    }

    public override void _Ready()
    {
        resource = GD.Load<GameUnitResource>(pathToData);

        if (resource == null)
        {
            Logger.LogError($"unit data resource not loaded for {this.Name}");
        }

        CustomMinimumSize = new Vector2(unitSquareSize, unitSquareSize);
        unitIndicatorLight.SetupLight(unitSquareSize);
        var areaShape = area2d.GetChild<CollisionShape2D>(0).Shape as RectangleShape2D;
        areaShape.Size = new Vector2(unitSquareSize, unitSquareSize);
    }

    public override void _Pressed()
    {
        unitIndicatorLight.Visible = true;
        GlobalCurrentSelection.GetInstance().AddUnit(this);
    }

    public void UnHighlight()
    {
        unitIndicatorLight.Visible = false;
    }
}
