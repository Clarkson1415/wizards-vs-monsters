using Godot;
using System;
using WizardsVsMonster.scripts;

public partial class DeselectAll : Button
{
    public override void _Pressed()
    {
        base._Pressed();

        GlobalCurrentSelection.GetInstance().DeselectAll();
    }
}
