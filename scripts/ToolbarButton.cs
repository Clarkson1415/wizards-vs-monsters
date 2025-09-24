using Godot;
using System;
using WizardsVsMonster.scripts;

public partial class ToolbarButton : Button
{
	private GameUnitResource gameUnit;
	
	public void SetItem(GameUnitResource gameObj)
	{
		gameUnit = gameObj;
    }

    public override void _Pressed()
    {
        base._Pressed();
        GlobalCurrentSelection.GetInstance().SelectedToolbarUnitsInfo = this.gameUnit;
    }
}
