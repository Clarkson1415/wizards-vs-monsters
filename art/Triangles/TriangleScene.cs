using Godot;
using System;
using WizardsVsMonster.scripts;
using static WizardsVsMonster.scripts.GlobalGameVariables;

/// <summary>
/// To Show the player the new position of the units will be.
/// </summary>
[GlobalClass]
public partial class TriangleScene : Node2D
{
	[Export] Sprite2D size16_1Unit;
    [Export] Sprite2D size32_2Unit;

    public void LoadTriangle(int unitSizeInPixels, GlobalGameVariables.FACTION faction)
	{
		var img = unitSizeInPixels == 16 ? size16_1Unit : size32_2Unit;
		img.Visible = true;

		img.SelfModulate = GlobalGameVariables.FACTION_COLORS[faction];
	}
}
