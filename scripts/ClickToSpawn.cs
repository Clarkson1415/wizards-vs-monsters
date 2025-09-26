using Godot;
using System;
using WizardsVsMonster.scripts;

/// <summary>
/// Things the player can do like clicking on the board to spawn items.
/// Only present in the setup phase
/// </summary>
public partial class ClickToSpawn : Control
{
    private void SnapToGrid(GameUnit unit)
    {
        var snapSize = GlobalGameVariables.CELL_SIZE * unit.GetInfo().GetSizeInUnits();
        var newPos = (unit.GlobalPosition / snapSize).Floor() * snapSize;
        newPos += new Vector2(snapSize / 2, snapSize / 2); // centre it in tile.
        unit.GlobalPosition = newPos;

        // TODO store in positions dictionary of vectors 2s and units?
        // or use the physics system to move them?
    }

    public override void _GuiInput(InputEvent @event)
    {
        base._GuiInput(@event);

        if (@event is InputEventScreenTouch touchEvent)
        {
            if (touchEvent.Pressed)
            {
                // TODO instead of pressed check for a fast tap and release.
                // otherwise the player might be trying to scroll the screen.
                TrySpawnUnit(touchEvent.Position);
            }
        }

        // mouse click and release
        if (@event.IsAction("leftClick"))
        {
            if (@event.IsPressed())
            {
                var mouseClickPosition = GetGlobalMousePosition();
                TrySpawnUnit(mouseClickPosition);
            }
        }
    }

    private void TrySpawnUnit(Vector2 mouseClickPosition)
    {
        Logger.Log($"position: {mouseClickPosition}");

        if (GetAreaAtPoint(mouseClickPosition) != null)
        {
            Logger.Log("unit at position can't spawn item.");
            return;
        }

        SpawnGameUnitAt(mouseClickPosition);
    }

    private Area2D GetAreaAtPoint(Vector2 globalPos)
    {
        var spaceState = GetWorld2D().DirectSpaceState;

        var query = new PhysicsPointQueryParameters2D
        {
            Position = globalPos,
            CollideWithBodies = false,
            CollideWithAreas = true
        };

        query.CollideWithBodies = false;
        query.CollideWithAreas = true;

        var results = spaceState.IntersectPoint(query);

        foreach (var result in results)
        {
            var colliderObj = result["collider"];
            if (colliderObj.VariantType == Variant.Type.Object)
            {
                var area = colliderObj.As<Area2D>();
                if (area != null)
                {
                    GD.Print($"Clicked on area: {area.Name}");
                    return area;
                }
            }
        }

        return null;
    }

    public void SpawnGameUnitAt(Vector2 tapPosition)
	{
        if (GlobalCurrentSelection.GetInstance().SelectedToolbarUnitsInfo == null)
        {
            return;
        }

        var newUnit = GlobalCurrentSelection.GetInstance().SelectedToolbarUnitsInfo.GetUnitScene().Instantiate<GameUnit>();
        AddChild(newUnit);
        Logger.Log($"unit put at: {newUnit.GlobalPosition}");
        newUnit.GlobalPosition = tapPosition;
        SnapToGrid(newUnit);
    }
}
