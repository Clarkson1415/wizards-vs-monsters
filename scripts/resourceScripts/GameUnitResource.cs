using Godot;
using System;

[GlobalClass]
public partial class GameUnitResource : Resource
{
	[Export] private int health;
	public int GetHealth() { return health; }

	[Export] private int speed = 2;

	/// <summary>
	/// Tiles per second. 1 tile or meter is 16 pixels.
	/// </summary>
	/// <returns></returns>
	public int GetSpeed() { return speed; }

	[Export] private int armour;
	public int GetArmour() { return armour; }

	[Export] private string name;
	public string GetUnitName() { return name; }

	[Export] private int size;

	/// <summary>
	/// size of 1 takes up 1 16x16 unit.
	/// </summary>
	/// <returns></returns>
	public int GetSizeInUnits() { return size; }

    /// <summary>
    /// The distance in tiles this unit will start attacking. 1 means adjacent tile.
    /// </summary>
    [Export] private int range;
	public int GetRange() { return range; }

    [Export] private int DPS;
	public int GetDPS() { return DPS; }

	[Export] private string description;
	public string GetDescription() { return this.description; }

	[Export] private PackedScene gameUnit;
	public PackedScene GetUnitScene() { return gameUnit; }

	[Export] private Texture2D toolbarImage;
	public Texture2D GetToolbarImage() { return toolbarImage; }
}
