using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Units movement and attacking. Active behaviours.
/// Unit passive things like taking damage go in unitarea.
/// </summary>
public partial class OgreOfKersh : GameUnit
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();

        // TODO other ready stuff.
        // Ogre Stinky status?
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        base._Process(delta);
    }

}
