using Godot;
using System;

[GlobalClass]
public partial class GameUnitResource : Resource
{
	[Export] private int health;
	public int GetHealth() { return health; }

	[Export] private int armour;
	public int GetArmour() { return armour; }

	[Export] private string name;
	public string GetUnitName() { return name; }

	[Export] private int DPS;
	public int GetDPS() { return DPS; }

	[Export] private string description;
	public string GetDescription() { return this.description; }

	[Export] private PackedScene gameUnit;
	public PackedScene GetUnitScene() { return gameUnit; }

	[Export] private Texture2D toolbarImage;
	public Texture2D GetToolbarImage() { return toolbarImage; }
}
