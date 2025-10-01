using Godot;
using System;
using static WizardsVsMonster.scripts.GlobalGameVariables;
using WizardsVsMonster.scripts;

public partial class GameUnitAnimatedSprite : AnimatedSprite2D
{
    public void SetOutlineColor(FACTION faction)
    {
        var color = GlobalGameVariables.FACTION_COLORS[faction];
        var material = (ShaderMaterial)Material;
        material.SetShaderParameter("outline_color", color);
    }

    public void FlashDamage()
	{
        var material = (ShaderMaterial)Material;
        // Flash white instantly
        material.SetShaderParameter("flash_strength", 1.0f);

        // Reset after a short time
        GetTree().CreateTimer(0.1).Timeout += () =>
        {
            material.SetShaderParameter("flash_strength", 0.0f);
        };
    }

	public void ToggleOutline(bool outlineOn)
	{
        var material = (ShaderMaterial)Material;
        material.SetShaderParameter("outline_on", outlineOn);
    }
}
