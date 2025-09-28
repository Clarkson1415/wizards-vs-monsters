using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WizardsVsMonster.scripts;
using static Godot.HttpRequest;

public partial class UnitGroup : Node2D
{
    /// <summary>
    /// shows new pos - not needed for enemy units tho i guess o sidk
    /// </summary>
    private List<TriangleScene> trianglesToShowNewPos = new();

    [Export] PackedScene triangleScene;

    /// <summary>
    /// Units that are alive rn.
    /// </summary>
    public List<GameUnit> UnitsRemaining => allUnits.Where(x => !x.IsDead).ToList();

    private List<GameUnit> allUnits = new List<GameUnit>();

    public GameUnitResource UnitResource { get; private set; }

    public GlobalGameVariables.FACTION Faction => UnitResource.GetFaction();

    // Called when the node enters the scene tree for the first time.
    public void SpawnUnits(GameUnitResource unitResource, Vector2 centrePositionWhereMouseClicked)
    {
        this.UnitResource = unitResource;
        var number = unitResource.GetNumberOfUnitsInSquadron();

        for (int i = 0; i < number; i++)
        {
            var newUnit = unitResource.GetUnitScene().Instantiate<GameUnit>();
            newUnit.Setup(unitResource);
            AddChild(newUnit);
            allUnits.Add(newUnit);
            newUnit.ClickableUnitComponent.OnPressed += OnUnitClicked;
        }

        var positions = GetPositionsToPutUnitsInGrid(centrePositionWhereMouseClicked);
        for (int i = 0; i < positions.Count; i++)
        {
            allUnits[i].GlobalPosition = positions[i];
            allUnits[i].SetTargetPosition(positions[i], GetDefaultDirection());
        }

        foreach (var initial in initialStatuses) { TryAddStatus(initial); }

        // setup signals
        foreach (var unit in allUnits)
        {
            unit.OnAttacked += OnUnitAttacked;
        }
    }

    private Vector2 GetDefaultDirection()
    {
        // TODO
        var direction = Vector2.Right;
        Logger.Log("will have to make sure enemies are facing the same as units. TODO calculate based of whos side of the board your plaing on.");
        return direction;
    }

    /// <summary>
    /// Triggered when unit clicked. Also called from unit bar slot button.
    /// </summary>
    public void OnUnitClicked()
    {
        HighlightUnits();
        GlobalCurrentSelection.GetInstance().OnGroupClicked(this);
        var added = GlobalCurrentSelection.GetInstance().IsGroupInSelection(this);
        if (!added)
        {
            UnhighlightUnits();
        }
    }

    public void TellGroupItWasRemovedFromSelection()
    {
        this.UnhighlightUnits();
    }

    private void UnhighlightUnits()
    {
        this.allUnits.ForEach(x => x.ClickableUnitComponent.UnHighlight());
    }

    private void HighlightUnits()
    {
        this.allUnits.ForEach(x => x.ClickableUnitComponent.Highlight());
    }

    #region centre calc 
    private Vector2 GetCentre()
    {
        if (allUnits.Count == 0) return Vector2.Zero;

        float minX = allUnits.Min(u => u.GlobalPosition.X);
        float maxX = allUnits.Max(u => u.GlobalPosition.X);
        float minY = allUnits.Min(u => u.GlobalPosition.Y);
        float maxY = allUnits.Max(u => u.GlobalPosition.Y);

        // center of the bounding rectangle
        float centerX = (minX + maxX) / 2f;
        float centerY = (minY + maxY) / 2f;

        var centre = new Vector2(centerX, centerY);

        var unitSize = UnitResource.GetSizeInUnits();
        var cellSize = GlobalGameVariables.CELL_SIZE;
        var offset = new Vector2(columns * cellSize * unitSize - cellSize * unitSize, rows * cellSize * unitSize - cellSize * unitSize) / 2f;
        return centre - offset;
    }
    #endregion

    #region targeting locations or enemies

    private List<Vector2> GetPositionsToPutUnitsInGrid(Vector2 centreOfSquadron)
    {
        // 1. Calculate grid shape mostly square as non given.
        columns = Mathf.CeilToInt(Mathf.Sqrt(allUnits.Count));
        rows = Mathf.CeilToInt((float)allUnits.Count / columns);
        return GetPositionsToPutUnitsInGrid(centreOfSquadron, columns, rows);
    }

    private List<Vector2> GetPositionsToPutUnitsInGrid(Vector2 centreOfSquadron, int columns, int rows)
    {
        var cellSize = GlobalGameVariables.CELL_SIZE;
        var unitSize = UnitResource.GetSizeInUnits();

        int count = allUnits.Count;
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

            Vector2 position = new Vector2(col, row) * cellSize * unitSize + centreOfSquadron;
            positions.Add(position);
        }

        return positions;
    }

    private int columns;
    private int rows;

    public void SetNewTargetEnemy(UnitGroup enemyToTarget)
    {
        var allTargets = new List<GameUnit>(enemyToTarget.UnitsRemaining);

        if (allTargets.Count == 0)
        {
            Logger.Log("No targets available to attack.");
            return;
        }

        var availableTargets = new List<GameUnit>(allTargets);
        int targetCount = allTargets.Count;

        GameUnit chosenTarget;

        // assign closest pairs (player, enemy) units first.
        var attackers = new List<GameUnit>(this.UnitsRemaining);
        List<(GameUnit, GameUnit, float)> combinations = new();

        // Step 1: Compute all distances
        foreach (var a in attackers)
        {
            foreach (var b in allTargets)
            {
                float dist = a.GlobalPosition.DistanceSquaredTo(b.GlobalPosition);
                combinations.Add(new(a, b, dist));
            }
        }

        // Step 2: Sort by distance ascending
        combinations.OrderBy(p => p.Item3).ToList();

        List<GameUnit> usedA = new();
        List<GameUnit> usedB = new();
        List<(GameUnit, GameUnit, float)> result = new();

        // Step 3: Select closest non-used pairs
        foreach (var (a, b, distance) in combinations)
        {
            if (!usedA.Contains(a) && !usedB.Contains(b))
            {
                result.Add((a, b, distance));
                usedA.Add(a);
                usedB.Add(b);
            }

            // Stop if assigned all targets to at least 1 attacker.
            if (result.Count >= allTargets.Count)
            {
                break;
            }
        }

        // if more attackers than targets need to assign from fursthees
        // TODO: same as above but for largest distance.

        // Reuse targets: rotate through the full list using a wrap-around index
        int rotationIndex = 0;

        foreach (var attacker in this.UnitsRemaining)
        {
            if (availableTargets.Count == 0)
            {
                return;
            }

            chosenTarget = allTargets[rotationIndex % targetCount];
            rotationIndex++;
            attacker.SetTargetUnit(chosenTarget);
        }
    }

    /// <summary>
    /// Set new location without specifying row, cols, would just be a click.
    /// </summary>
    /// <param name="newCentre"></param>
    public void SetNewTargetLocation(Vector2 newCentre)
    {
        ClearTriangles();

        var positionsArray = GetPositionsToPutUnitsInGrid(newCentre);
        for (int i = 0; i < positionsArray.Count; i++)
        {
            allUnits[i].SetTargetPosition(positionsArray[i], GetDefaultDirection());
        }

        SpawnTriangleIndicators(positionsArray, GetDefaultDirection());
    }

    private void SpawnTriangleIndicators(List<Vector2> positions, Vector2 directionUnitsFacingUnitVector)
    {
        for (int i = 0; i < allUnits.Count; i++)
        {
            var tri = triangleScene.Instantiate<TriangleScene>();
            AddChild(tri);
            tri.GlobalPosition = positions[i];
            tri.LoadTriangle(UnitResource.GetSizeInUnits() * GlobalGameVariables.CELL_SIZE, UnitResource.GetFaction());
            trianglesToShowNewPos.Add(tri);

            float angleRadians = Mathf.Atan2(directionUnitsFacingUnitVector.Y, directionUnitsFacingUnitVector.X);
            tri.GlobalRotation = angleRadians;
        }
    }

    /// <summary>
    /// Set new positions when clicked and dragged to set new position and a specific rotation.
    /// </summary>
    /// <param name="newPosition"></param>
    /// <param name="unitFacingDir"></param>
    /// <param name="cols"></param>
    /// <param name="rows"></param>
    public void SetNewTargetLocation(Vector2 newPosition, Vector2 unitFacingDir, int cols, int rows)
    {
        ClearTriangles();
        var positionsArray = GetPositionsToPutUnitsInGrid(newPosition, cols, rows);
        for (int i = 0; i < positionsArray.Count; i++)
        {
            allUnits[i].SetTargetPosition(positionsArray[i], unitFacingDir);
        }

        SpawnTriangleIndicators(positionsArray, GetDefaultDirection());
    }

    private void ClearTriangles()
    {
        foreach (var triangle in trianglesToShowNewPos)
        {
            triangle.QueueFree();
        }

        trianglesToShowNewPos.Clear();
    }
    #endregion

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (allUnits.Count == 0) { return; }

        var centre = GetCentre();

        if (allUnits.All(x => x.AtTargetLocation))
        {
            ClearTriangles();
        }

        CheckStatuses();
    }

    private void OnUnitAttacked(UnitGroup attackers)
    {
        Logger.Log("unit attacked, will retaliate if not busy.");
        // on unit attacked if no other command in place. then attack the attacker.
        if (UnitsRemaining.FirstOrDefault().CurrentCommand == GameUnit.COMMAND.Nothing)
        {
            Logger.Log("attacking back.");
            SetNewTargetEnemy(attackers);
        }
    }

    public float GetHealthPercentage()
    {
        var squadHealth = allUnits.Sum(u => u.UnitBody.GetCurrentHealth());
        var maxSquadHealth = UnitResource.GetHealth() * allUnits.Count;
        return squadHealth / maxSquadHealth;
    }

    #region status logic

    private void CheckStatuses()
    {
        if (this.GetHealthPercentage() <= 0.3)
        {
            TryAddStatus(StatusComponent.STATUS.dying);
        }
        else if (this.GetHealthPercentage() > 0.3)
        {
            RemoveStatus(StatusComponent.STATUS.dying);
        }

        // TODO other statuses
    }

    private Godot.Collections.Array<StatusComponent.STATUS> initialStatuses = new Godot.Collections.Array<StatusComponent.STATUS>() { StatusComponent.STATUS.fresh };


    private Godot.Collections.Array<StatusComponent.STATUS> activeStatuses = new();

    private void UpdateStatusEffectsOnUnits()
    {
        foreach (var unit in allUnits)
        {
            unit.UpdateStatusEffects(activeStatuses);
        }
    }

    public Godot.Collections.Array<StatusComponent.STATUS> GetActiveStatuses()
    {
        return this.activeStatuses;
    }

    private bool TryAddStatus(StatusComponent.STATUS status)
    {
        if (this.activeStatuses.Contains(status))
        {
            return false;
        }

        switch (status)
        {
            case StatusComponent.STATUS.fresh:
                AddStatusWithTimer(StatusComponent.STATUS.fresh, GlobalGameVariables.FRESH_STATUS_TIME);
                break;
            // TODO other statuses
            default:
                AddStatus(status);
                break;
        }

        return true;
    }

    private void RemoveStatus(StatusComponent.STATUS status)
    {
        this.activeStatuses.Remove(status);
        UpdateStatusEffectsOnUnits();
    }

    Timer freshStatusTimer;

    /// <summary>
    /// For ones like fresh and stuff.
    /// </summary>
    private void AddStatusWithTimer(StatusComponent.STATUS status, double time)
    {
        activeStatuses.Add(status);

        freshStatusTimer = new Timer();
        AddChild(freshStatusTimer);
        freshStatusTimer.WaitTime = time;

        freshStatusTimer.Start();
        freshStatusTimer.Timeout += () => RemoveStatus(status);
        freshStatusTimer.Timeout += freshStatusTimer.QueueFree;
        UpdateStatusEffectsOnUnits();
    }

    private void AddStatus(StatusComponent.STATUS status)
    {
        activeStatuses.Add(status);
        UpdateStatusEffectsOnUnits();
    }
    #endregion
}
