using Godot;
using System;
using WizardsVsMonster.scripts;
#nullable enable

[GlobalClass]
public partial class PropertyPanel : Control
{
	[Export] private Label name;
	[Export] private Label description;
	[Export] private Label armour;
	[Export] private Label health;
	[Export] private Label dps;
	[Export] private Label range;
    [Export] private Label speed;
    [Export] private Label size;

    public override void _Ready()
	{
		Visible = false;
		GlobalCurrentSelection.GetInstance().OnItemLastSelectedChanged += OnItemChanged;
    }

    private void OnItemChanged()
	{
		Logger.Log("item changed");
		var unitData = GlobalCurrentSelection.GetInstance().LastSelectedUnitsInfo;
        ShowNewItem(unitData);
    }

	public void HidePanel()
	{
		Visible = false;
	}

	public void ShowNewItem(GameUnitResource? data)
	{
		if (data == null)
		{
			HidePanel();
			return;
        }

        Visible = true;
		name.Text = data.GetUnitName();
		description.Text = data.GetDescription();
		armour.Text = data.GetArmour().ToString();
		health.Text = data.GetHealth().ToString();
		dps.Text = data.GetDPS().ToString();
        range.Text = data.GetRange().ToString();
        speed.Text = data.GetSpeed().ToString();
		size.Text = data.GetSizeInUnits().ToString();
    }

}
