using Godot;
using System;
using System.Reflection.Emit;

public partial class GaeaWrapper : Node
{
    [Export] private Node GaeaGenerator { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        GaeaGenerator.Call("generate");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
