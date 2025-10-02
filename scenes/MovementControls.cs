using Godot;
using System;
using System.Linq;
using WizardsVsMonster.scripts;
using WizardsVsMonster.scripts.UIScripts;

public partial class MovementControls : Control, IGameInputControlNode
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	// TODO group selection logic stuff.
	// IDK where the logic for targeting should go maybe here?
	// if group == enemy
	// set target for all units in players groups to that enemy group?
	public void InputTap(Vector2 positionTappedOrClicked)
	{
        TryMoveUnit(positionTappedOrClicked);
    }

    private void TryMoveUnit(Vector2 touchOrTapPosition)
    {
        if (GlobalCurrentSelection.GetInstance().LastSelectedUnitsInfo == null || GlobalCurrentSelection.PlayerGroupsSelected.Count == 0)
        {
            return;
        }

        // not sure if gui input will register if i click on another unit instead tho?

        // TODO logic like:
        // if last selected group was enemy group: move plays groups to fight enemy groups
        // if last selected group was a player, then i clicked on the ground. move players to that positoin.
        // What if i want to click and hold to create a formation rectangle of a size i want?

        var lastSelected = GlobalCurrentSelection.GetInstance().LastSelectedUnitsInfo.GetFaction();
        var playersFactionsHighlighted = GlobalCurrentSelection.PlayerGroupsSelected.Where(x => x.Faction == GlobalGameVariables.PlayerControlledFaction);

        if (lastSelected == GlobalGameVariables.PlayerControlledFaction)
        {
            Logger.Log("Moving unit");
            playersFactionsHighlighted.ToList().ForEach(x => x.SetNewTargetLocation(touchOrTapPosition));

            // TODO if clicked or dragged need to positions units at the new 'ghost' positions.
            // TODO if multiple groups calculate group layout size. then put them next to each other but still in formation.
        }
        else
        {
            Logger.Log("cant move an enemy unit.");
        }

    }
}
