using Godot;
using System;
using System.Collections.Generic;
using WizardsVsMonster.scripts;

public partial class ClearCursor : Button
{
    public override void _Pressed()
    {
        base._Pressed();

        GlobalCurrentSelection.GetInstance().ClearCursor();
    }
}
