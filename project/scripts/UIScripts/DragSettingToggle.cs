using Godot;
using System;
using WizardsVsMonster.scripts;

/// <summary>
/// The toggle button for mobile to switch between dragging map screen or drawing the selction rectangle
/// </summary>
public partial class DragSettingToggle : CheckButton
{
    public override void _Toggled(bool toggledOn)
    {
        GlobalGameVariables.TouchScreenDragSetting_DragSelectOn = toggledOn;
    }
}
