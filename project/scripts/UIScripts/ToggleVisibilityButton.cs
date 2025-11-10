using Godot;
using System;

/// <summary>
/// Used for toggling property panel description.
/// </summary>
public partial class ToggleVisibilityButton : Button
{
	[Export] private Control description;

	[Export] private Control stats;

	public override void _Ready()
	{
		stats.Visible = true;
		description.Visible = false;
    }

    public override void _Pressed()
	{
        stats.Visible = !stats.Visible;
        description.Visible = !description.Visible;

		Text = stats.Visible ? "About" : "Stats";
    }
}
