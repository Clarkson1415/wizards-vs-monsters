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
            newUnit.GetClickableUnitComponent().OnPressed += OnUnitClicked;
        }

        PlaceUnitsInCenteredGrid(units, centrePositionWhereMouseClicked);
        statusComponent.InitialiseInitialStatuses(initialStatuses);
    }

    private void OnUnitClicked()
    {
        foreach(var unit in units)
        {
            unit.GetClickableUnitComponent().Highlight();
        }

        GlobalCurrentSelection.GetInstance().OnUnitGroupClicked(this);
    }

    private void PlaceUnitsInCenteredGrid(List<GameUnit> units, Vector2 origin)
    {
        var cellSize = GlobalGameVariables.CELL_SIZE;
        var unitSize = unitResource.GetSizeInUnits();

        int count = units.Count;
        if (count == 0) return;

        // 1. Calculate optimal grid shape
        int columns = Mathf.CeilToInt(Mathf.Sqrt(count));
        int rows = Mathf.CeilToInt((float)count / columns);

        // 2. Calculate total grid size
        float gridWidth = columns * cellSize * unitSize;
        float gridHeight = rows * cellSize * unitSize;
        Vector2 gridOffset = origin - new Vector2(gridWidth, gridHeight) / 2f;

        // 3. Place each unit
        for (int i = 0; i < count; i++)
        {
            int row = i / columns;
            int col = i % columns;

            Vector2 position = new Vector2(col, row) * cellSize * unitSize + gridOffset;
            units[i].GlobalPosition = position;
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
            unit.ApplyStatusEffects(statusComponent.GetActiveStatuses());
        }

        // Update health skill indicator if dying or remove if not.
        var currentHealth = units.Sum(u => u.GetCurrentHealth());
        var maxHealth = unitResource.GetHealth() * units.Count;
        statusComponent.UpdateHealthPercentage(currentHealth / maxHealth);
    }
}
