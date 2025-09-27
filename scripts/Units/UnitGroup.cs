using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using WizardsVsMonster.scripts;

public partial class UnitGroup : Node2D
{
    [Export] private StatusComponent statusComponent;

    [Export] private Godot.Collections.Array<StatusComponent.STATUS> initialStatuses = [];

    private List<GameUnit> units = new List<GameUnit>();

    private GameUnitResource unitResource;

    public GlobalGameVariables.FACTION Faction => unitResource.GetFaction();

    public GameUnitResource GetGroupUnitType()
    {
        return unitResource;
    }

    // Called when the node enters the scene tree for the first time.
    public void SpawnUnits(GameUnitResource unitResource, Vector2 centrePositionWhereMouseClicked)
    {
        this.unitResource = unitResource;
        var number = unitResource.GetNumberOfUnitsInSquadron();

        for (int i = 0; i < number; i++)
        {
            var newUnit = unitResource.GetUnitScene().Instantiate<GameUnit>();
            newUnit.Setup(unitResource);
            AddChild(newUnit);
            units.Add(newUnit);
            newUnit.ClickableUnitComponent.OnPressed += OnUnitClicked;
        }

        var positions = GetPositionsToPutUnitsInGrid(centrePositionWhereMouseClicked);
        for (int i = 0; i < positions.Count; i++)
        {
            units[i].GlobalPosition = positions[i];
            units[i].SetNewTargetPositionRotation(positions[i], GetDefaultDirection());
        }

        statusComponent.InitialiseInitialStatuses(initialStatuses);
    }

    private Vector2 GetDefaultDirection()
    {
        var direction = Vector2.Right;
        Logger.Log("will have to make sure enemies are facing the same as units. TODO calculate based of whos side of the board your plaing on.");
        return direction;
    }

    private void OnUnitClicked()
    {
        HighlightUnits();
        var added = GlobalCurrentSelection.GetInstance().OnGroupClicked(this);
        if (!added)
        {
            UnhighlightUnits();
        }
    }

    private void UnhighlightUnits()
    {
        this.units.ForEach(x => x.ClickableUnitComponent.UnHighlight());
    }

    private void HighlightUnits()
    {
        this.units.ForEach(x => x.ClickableUnitComponent.Highlight());
    }

    private List<Vector2> GetPositionsToPutUnitsInGrid(Vector2 centreOfSquadron)
    {
        // 1. Calculate grid shape mostly square as non given.
        int columns = Mathf.CeilToInt(Mathf.Sqrt(units.Count));
        int rows = Mathf.CeilToInt((float)units.Count / columns);

        return GetPositionsToPutUnitsInGrid(centreOfSquadron, columns, rows);
    }

    private List<Vector2> GetPositionsToPutUnitsInGrid(Vector2 centreOfSquadron, int columns, int rows)
    {
        var cellSize = GlobalGameVariables.CELL_SIZE;
        var unitSize = unitResource.GetSizeInUnits();

        int count = units.Count;
        if (count == 0) return [];

        // 2. Calculate total grid size
        float gridWidth = columns * cellSize * unitSize;
        float gridHeight = rows * cellSize * unitSize;
        Vector2 gridOffset = centreOfSquadron - new Vector2(gridWidth, gridHeight) / 2f;

        // get positions they should be
        List<Vector2> positions = new();
        for (int i = 0; i < count; i++)
        {
            int row = i / columns;
            int col = i % columns;

            Vector2 position = new Vector2(col, row) * cellSize * unitSize + gridOffset;
            positions.Add(position);
        }

        return positions;
    }

    private Vector2 getCentre()
    {
        // Average all unit positions? 
        var averageX = units.Average(x => x.GlobalPosition.X);
        var averageY = units.Average(x => x.GlobalPosition.Y);
        return new Vector2(averageX, averageY);
    }

    /// <summary>
    /// Set new location without specifying row, cols, would just be a click.
    /// </summary>
    /// <param name="newCentre"></param>
    public void SetNewTargetLocation(Vector2 newCentre)
    {
        float dx = getCentre().X - newCentre.X;
        float dy = getCentre().Y - newCentre.Y;

        // ^ thats for the whole unit. the centre position.
        // work out the target positions for all the units.
        // keep formation move to new position?
        // TODO: what if want to change formation tho? diff size rectangle?
        // TODO: Move centre of unit group to new position.
        // TODO calculate rows cols to use.

        var positionsArray = GetPositionsToPutUnitsInGrid(newCentre);
        for (int i = 0; i < positionsArray.Count; i++)
        {
            units[i].SetNewTargetPositionRotation(positionsArray[i], GetDefaultDirection());
        }
    }

    /// <summary>
    /// Set new positions when clicked and dragged to set new position.
    /// </summary>
    /// <param name="newPosition"></param>
    /// <param name="unitFacingDir"></param>
    /// <param name="cols"></param>
    /// <param name="rows"></param>
    public void SetNewTargetLocation(Vector2 newPosition, Vector2 unitFacingDir, int cols, int rows)
    {
        var positionsArray = GetPositionsToPutUnitsInGrid(newPosition, cols, rows);
        for (int i = 0; i < positionsArray.Count; i++)
        {
            units[i].SetNewTargetPositionRotation(positionsArray[i], unitFacingDir);
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (units.Count == 0)
        {
            return;
        }

        foreach (var unit in units)
        {
            //unit.ApplyStatusEffects(statusComponent.GetActiveStatuses());
        }

        // Update health skill indicator if dying or remove if not.
        //var squadHealth = units.Sum(u => u.UnitBody.GetCurrentHealth());
        //var maxHealth = unitResource.GetHealth() * units.Count;
        //statusComponent.UpdateHealthPercentage(squadHealth / maxHealth);
    }

}
