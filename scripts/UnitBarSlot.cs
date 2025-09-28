using Godot;
using System;

/// <summary>
/// Bottom of screen unit bar with current units in it.
/// </summary>
public partial class UnitBarSlot : TextureRect
{
	[Export] private StatusComponent statusComponent;

    [Export] private Label NumberText;

    [Export] private TextureRect unitImage;
    
    /// <summary>
    /// Ranged or melee - TODO: this can change when clicked on if they have 2 different attacks.
    /// </summary>
    // [Export] private TextureRect unitTypeImage;

    [Export] private UnitBarSlotButton button;

    private UnitGroup group;

	public void Setup(UnitGroup group)
	{
		this.group = group;
        var data = group.UnitResource;
        this.NumberText.Text = data.GetNumberOfUnitsInSquadron().ToString();
        unitImage.Texture = data.GetUnitSmallImage();

        button.Setup(group);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (this.group == null) { return; }

        statusComponent.UpdateHealthPercentage(this.group.GetHealthPercentage());
        statusComponent.AddStatuses(this.group.GetActiveStatuses());
    }
}
