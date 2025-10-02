using Godot;
using WizardsVsMonster.scripts;
using WizardsVsMonster.scripts.UIScripts;

/// <summary>
/// Things the player can do like clicking on the board to spawn items.
/// Children of this are the unit groups in game. rn.
/// Only present in the setup phase
/// </summary>
public partial class ClickToSpawn : Control, IGameInputControlNode
{
    [Export] private PackedScene unitGroupScene;

    [Export] private UnitBar unitBar;

    private void SnapToGrid(UnitGroup unit, GameUnitResource info)
    {
        var snapSize = GlobalGameVariables.CELL_SIZE * info.GetSizeInUnits();
        var newPos = (unit.GlobalPosition / snapSize).Floor() * snapSize;
        newPos += new Vector2(snapSize / 2, snapSize / 2); // centre it in tile.
        unit.GlobalPosition = newPos;
    }

    public void InputTap(Vector2 tapPosition)
    {
        var spawned = TrySpawnUnit(tapPosition);
        if (spawned)
        {
            GlobalCurrentSelection.GetInstance().SelectedUnitToSpawn = null;
        }
    }

    private bool TrySpawnUnit(Vector2 mouseClickPosition)
    {
        Logger.Log($"position: {mouseClickPosition}");

        if (GetAreaAtPoint(mouseClickPosition) != null)
        {
            Logger.Log("unit at position can't spawn item.");
            return false;
        }

        var unitData = GlobalCurrentSelection.GetInstance().SelectedUnitToSpawn;
        if (unitData == null)
        {
            return false;
        }

        var newUnitGroup = this.unitGroupScene.Instantiate<UnitGroup>();
        AddChild(newUnitGroup);
        Logger.Log($"unit put at: {newUnitGroup.GlobalPosition}");
        newUnitGroup.GlobalPosition = mouseClickPosition;
        SnapToGrid(newUnitGroup, unitData);
        newUnitGroup.SpawnUnits(unitData, mouseClickPosition);

        // add to bar
        unitBar.AddNewUnitGroup(newUnitGroup);
        return true;
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
}
