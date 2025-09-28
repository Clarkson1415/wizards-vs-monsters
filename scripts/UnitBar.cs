using Godot;
using System;


public partial class UnitBar : Control
{
	[Export] private PackedScene UnitBarSlotScene;

	[Export] private HBoxContainer slots;

	public void AddNewUnitGroup(UnitGroup unitGroup)
	{
		UnitBarSlot newSlot = UnitBarSlotScene.Instantiate<UnitBarSlot>();
		slots.AddChild(newSlot);
		newSlot.Setup(unitGroup);
    }
}
