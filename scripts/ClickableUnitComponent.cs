using Godot;
using System;
using WizardsVsMonster.scripts;

/// <summary>
/// A component to go on the game unit scenes that fight.
/// </summary>
[GlobalClass]
public partial class ClickableUnitComponent : Button
{
    [Export] private GameUnitAnimatedSprite animatedSprite;

    [Export] private Area2D area2d;

    private GameUnitResource resouce;

    public GameUnitResource GetInfo()
    {
        return resouce;
    }

    public void Setup(GameUnitResource resource)
    {
        this.resouce = resource;
        var unitSquareSize = resource.GetSizeInUnits() * GlobalGameVariables.CELL_SIZE;

        // setup this button
        CustomMinimumSize = new Vector2(unitSquareSize, unitSquareSize);
        Flat = true;

        // unit indicator light
        

        // area
        var areaShape = area2d.GetChild<CollisionShape2D>(0).Shape as RectangleShape2D;
        areaShape.Size = new Vector2(unitSquareSize, unitSquareSize);
    }

    [Signal]
    public delegate void OnPressedEventHandler();

    public override void _Pressed()
    {
        EmitSignal(SignalName.OnPressed);
    }

    public void ToggleOutline(bool outlineOn)
    {
        this.animatedSprite.ToggleOutline(outlineOn);
    }
}
