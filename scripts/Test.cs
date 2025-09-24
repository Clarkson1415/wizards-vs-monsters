using Godot;
using System;

public partial class Test : Node
{
	[Export] private GameUnitResource resource;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (resource == null)
		{
			Logger.LogError("null");
		}
		else
		{
			Logger.Log("Resource assigned.");
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
