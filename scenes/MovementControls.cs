using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using WizardsVsMonster.scripts;
using WizardsVsMonster.scripts.UIScripts;

public partial class MovementControls : Control, IGameInputControlNode
{
	// TODO group selection logic stuff.
	// IDK where the logic for targeting should go maybe here?
	// if group == enemy
	// set target for all units in players groups to that enemy group?
	public void OnTapInput(Vector2 positionTappedOrClicked)
	{
        TryMoveUnit(positionTappedOrClicked);
    }

    // TODO if clicked or dragged need to positions units at the new 'ghost' positions. Then set new target location on the groups on release of that button.


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
        var playersFactionsHighlighted = GlobalCurrentSelection.PlayerGroupsSelected;

        if (lastSelected == GlobalGameVariables.PlayerControlledFaction)
        {
            Logger.Log("Moving unit");

            // if multiple groups calculate group layout size. then put them next to each other but still in formation.
            if (playersFactionsHighlighted.Count > 1)
            {
                Logger.Log("doing this:");
                var centrePosition = touchOrTapPosition;
                // Check width and height of the units basic formation positions.
                // position each troops centre the 1/2 width of tropop plus 1/2 width of the other troop away.
                List<Vector2> troopHeightWidth = new();
                foreach (var troop in playersFactionsHighlighted)
                {
                    //troopHeightWidth.Add(troop.GetBasicFormation);
                }
            }
            else
            {
                playersFactionsHighlighted.ToList().ForEach(x => x.SetNewTargetLocation(touchOrTapPosition));
            }

        }
        else
        {
            Logger.Log("cant move an enemy unit.");
        }
    }
}
