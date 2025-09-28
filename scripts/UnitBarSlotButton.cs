using Godot;
using System;

public partial class UnitBarSlotButton : Button
{
    private UnitGroup group;


    public void Setup(UnitGroup group)
    {
        this.group = group;
    }

    public override void _Pressed()
    {
        base._Pressed();
        this.group.OnUnitClicked();
    }
}
