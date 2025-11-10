using Godot;
using System;

public partial class ToolbarSlot : TextureRect
{
	[Export] private GameUnitResource gameUnitData;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		var textureRect = GetChild<TextureRect>(0);
		textureRect.Texture = gameUnitData.GetUnitSmallImage();

		var button = GetChild<ToolbarButton>(1);
		button.SetItem(gameUnitData);
	}
}
