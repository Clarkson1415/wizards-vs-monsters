using Godot;
using System;
using WizardsVsMonster.scripts;

/// <summary>
/// Things the player can do like clicking on the board to spawn items.
/// Only present in the setup phase
/// </summary>
public partial class ClickToSpawn : Control
{
    [Export] private PackedScene unitGroupScene;

    private void SnapToGrid(UnitGroup unit, GameUnitResource info)
    {
        var snapSize = GlobalGameVariables.CELL_SIZE * info.GetSizeInUnits();
        var newPos = (unit.GlobalPosition / snapSize).Floor() * snapSize;
        newPos += new Vector2(snapSize / 2, snapSize / 2); // centre it in tile.
        unit.GlobalPosition = newPos;
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
        if (GlobalCurrentSelection.GetInstance().SelectedUnitToSpawn == null)
        {
            return;
        }

        var newUnitGroup = this.unitGroupScene.Instantiate<UnitGroup>();
        AddChild(newUnitGroup);
        Logger.Log($"unit put at: {newUnitGroup.GlobalPosition}");
        newUnitGroup.GlobalPosition = tapPosition;
        var unitData = GlobalCurrentSelection.GetInstance().SelectedUnitToSpawn;
        SnapToGrid(newUnitGroup, unitData);
        newUnitGroup.SpawnUnits(unitData, tapPosition);
    }
}
