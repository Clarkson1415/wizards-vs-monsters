using Godot;
using System;

public abstract partial class MovingGameUnit : GameUnit
{
    protected int unitBaseSpeed;

    protected int speedModifier = 1;

    protected Vector2 directionFacing = Vector2.Left;

    protected Vector2 velocity;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		base._Ready();
        unitBaseSpeed = this.GetInfo().GetSpeed();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        base._Process(delta);
        // TODO: update statuses?
	}
}
