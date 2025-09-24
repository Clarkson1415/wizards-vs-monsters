using Godot;
using System;
using WizardsVsMonster.scripts;

[GlobalClass]
public partial class PropertyPanel : TextureRect
{
	[Export] private Label name;
	[Export] private Label description;
	[Export] private Label armour;
	[Export] private Label health;
	[Export] private Label dps;

    public override void _Ready()
	{
		Visible = false;

		GlobalCurrentSelection.GetInstance().OnToolbarItemSelectedChanged += OnItemChanged;
    }

    private void OnItemChanged(GameUnitResource unitData)
	{
		Logger.Log("item changed");
		ShowNewItem(unitData);
    }

	public void HidePanel()
	{
		Visible = false;
	}

	public void ShowNewItem(GameUnitResource data)
	{
		Visible = true;
		name.Text = data.GetUnitName();
		description.Text = data.GetDescription();
		armour.Text = data.GetArmour().ToString();
		health.Text = data.GetHealth().ToString();
		dps.Text = data.GetDPS().ToString();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
