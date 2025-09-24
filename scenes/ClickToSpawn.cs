using Godot;
using System;
using WizardsVsMonster.scripts;

/// <summary>
/// Things the player can do like clicking on the board to spawn items.
/// </summary>
public partial class ClickToSpawn : Control
{
	public override void _Input(InputEvent @event)
	{
        // TODO: instead of this position. Only fire an event if the board is tapped on.
        // then round to nearest centre of the squares.

        if (@event is InputEventScreenTouch touchEvent)
		{
            if (touchEvent.Pressed)
            {
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
        if (GlobalCurrentSelection.GetInstance().SelectedToolbarUnit == null)
        {
            return;
        }

        var newUnit = GlobalCurrentSelection.GetInstance().SelectedToolbarUnit.GetUnitScene().Instantiate<Node2D>();
        AddChild(newUnit);
        Logger.Log($"unit put at: {newUnit.GlobalPosition}");
        newUnit.GlobalPosition = tapPosition;
    }
}
