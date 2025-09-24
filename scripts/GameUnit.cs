using Godot;
using System;

/// <summary>
/// The Game unit on the board.
/// </summary>
public partial class GameUnit : Node2D
{
	[Export] public GameUnitResource resource;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (resource == null)
		{
			Logger.Log("resource on unit is null");
		}
		else
		{
			Logger.Log("resource valid");
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
