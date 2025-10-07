using Godot;
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

                // position troops evenly spaced around the centre position
                // calculate centre positions based on troops BasicFormation size vector 2d
                // if odd or even the positions either include the centre or not.
                // if even = [troop] [centrepos] [troop]
                // if odd = [troop] [troop at center position] [troop]

                // in a straight line in the default direction if none given. 
                // also when troops are a certain amount in the col or row. then put in behind it.


                List<Vector2> troopHeightWidth = new();
                foreach (var troop in playersFactionsHighlighted)
                {
                    // troopHeightWidth.Add(troop.GetBasicFormation);
                    //GlobalGameVariables.GetDefaultDirection(troop.GetCentre());


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
