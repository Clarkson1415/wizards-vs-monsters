using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using WizardsVsMonster.scripts;

/// <summary>
/// Represents the game unit on the board.
/// </summary>
[GlobalClass]
public partial class GameUnit : Node2D
{
    [Export] public ClickableUnitComponent ClickableUnitComponent { get; private set; }

    [Export] public UnitBody UnitBody { get; private set; }

    [Export] private AnimationComponent animationPlayer;

    [Export] private AnimatedSprite2D animatedSprite2D;

    private int baseUnitDamagePerAnimation = 1;

    public GameUnitResource resource { get; private set; }

    protected float unitBaseSpeed;

    protected float speedModifier = 1;

    protected Vector2 velocity;

    public bool IsDead => UnitBody.GetCurrentHealth() <= 0;

    [Export] private Area2D range_left;
    [Export] private Area2D range_right;
    [Export] private Area2D range_down;
    [Export] private Area2D range_up;

    private List<UnitBody> targetsInRangeAndAlive = new List<UnitBody>();

    private void UpdateTargetsInRange()
    {
        targetsInRangeAndAlive.Clear();

        var areas = new List<Area2D> { range_left, range_right, range_down, range_up, this.UnitBody };
        areas.ForEach(x => UpdateEnemiesInArea(x));
    }

    private void UpdateEnemiesInArea(Area2D area)
    {
        if (!area.HasOverlappingAreas())
        {
            return;
        }

        var overlapping = area.GetOverlappingAreas();

        foreach (var overlap in overlapping)
        {
            if (overlap is UnitBody targetAsArea)
            {
                if (targetAsArea.GetCurrentHealth() <= 0)
                {
                    return;
                }
                else if (!GlobalGameVariables.FactionEnemies[this.resource.GetFaction()].Contains(targetAsArea.GetFaction()))
                {
                    return;
                }

                targetsInRangeAndAlive.Add(targetAsArea);
            }
        }        
    }

    /// <summary>
    /// When the animation frame that the attack animation should hit an enemy plays.
    /// </summary>
    private void OnAttackFrame()
    {
        if (targetsInRangeAndAlive.Count == 0) { return; }

        targetsInRangeAndAlive.First().TakeDamage(this.baseUnitDamagePerAnimation);

        //foreach (var tar in targetsInRange)
        //{
        //    tar.TakeDamage(this.baseUnitDamagePerAnimation);
        //}
    }

    public void Setup(GameUnitResource resource)
    {
        base._Ready();

        this.resource = resource;
        if (resource == null)
        {
            Logger.LogError($"unit data resource not loaded for {this.Name}");
        }

        unitBaseSpeed = this.resource.GetSpeed();
        baseUnitDamagePerAnimation = this.resource.GetDPS();
        // TODO scale damage to attack animation speed? or just make sure they match in the resource. OR attack at that dmg per second?

        ClickableUnitComponent.Setup(resource);
        UnitBody.Setup(this.resource);

        UnitBody.OnThisHit += OnHit;
    }

    private enum unitState
    {
        Idle,
        MoveToPosition,
        Attacking,
        Dead,
    }

    private unitState state = unitState.Idle;

    private Vector2 targetPosition;

    /// <summary>
    /// Normalised direction unit vector.
    /// </summary>
    private Vector2 targetDirectionUnitVector;

    /// <summary>
    /// Current direction facing. up, right, down, left etc.
    /// </summary>
    private Vector2 directionFacingUnitVector;

    private enum COMMAND { MoveToPosition, AttackTarget, Nothing };

    private COMMAND currentCommand = COMMAND.Nothing;

    public void SetTargetPosition(Vector2 pos, Vector2 dir)
    {
        currentCommand = COMMAND.MoveToPosition;
        targetPosition = pos;
        targetDirectionUnitVector = dir;
    }

    public void SetTargetUnit(GameUnit enemyToTarget)
    {
        currentCommand = COMMAND.AttackTarget;
        currentTarget = enemyToTarget.UnitBody;
    }

    private void OnHit()
    {
        Logger.Log($"this Hit {Name}. todo hurt animation? or just flash sprite as to not interrupt state machine.");
        // animationPlayer.UpdateAnimation(directionFacingUnitVector, "hurt");
        var material = (ShaderMaterial)animatedSprite2D.Material;
        // Flash white instantly
        material.SetShaderParameter("flash_strength", 1.0f);
        // Reset after a short time
        GetTree().CreateTimer(0.1).Timeout += () =>
        {
            material.SetShaderParameter("flash_strength", 0.0f);
        };
    }

    public bool AtTargetLocation => GlobalPosition.DistanceTo(targetPosition) < 1f;

    private bool AtTargetDirection()
    {
        return (targetDirectionUnitVector == directionFacingUnitVector);
    }

    private UnitBody currentTarget;

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (this.resource == null) { return; }

        UpdateTargetsInRange();
        if (UnitBody.GetCurrentHealth() <= 0)
        {
            animationPlayer.UpdateAnimation(directionFacingUnitVector, "die");
            state = unitState.Dead;
        }

        if (this.resource.GetFaction() == GlobalGameVariables.FACTION.humans)
        {
            Logger.Log($"state = {state.ToString()}");
        }

        float deltaf = (float)delta;
        switch (this.state)
        {
            case unitState.Idle:
                velocity = Vector2.Zero;

                if ((currentCommand == COMMAND.MoveToPosition) || (currentCommand == COMMAND.AttackTarget))
                {
                    state = unitState.MoveToPosition;
                }
                else if (currentCommand == COMMAND.Nothing)
                {
                    if (targetsInRangeAndAlive.Count > 0)
                    {
                        state = unitState.Attacking;
                    }
                }
                animationPlayer.UpdateAnimation(directionFacingUnitVector, "idle");
                break;
            case unitState.MoveToPosition:

                // update target positoin and orientation if following a target.
                if (currentCommand == COMMAND.AttackTarget)
                {
                    targetPosition = currentTarget.GlobalPosition;
                    targetDirectionUnitVector = (targetPosition - GlobalPosition).Normalized();

                    // TODO: this should be happening before this guy is right on top of the unit.
                    if (targetsInRangeAndAlive.Contains(currentTarget))
                    {
                        state = unitState.Attacking;
                    }
                    else if (currentTarget.GetCurrentHealth() <= 0)
                    {
                        currentCommand = COMMAND.Nothing;
                        state = unitState.Idle;
                    }
                }
                else // command.Move or Command.Nothing
                {
                    if (AtTargetLocation)
                    {
                        // TODO: rotate over time to be in target rotation
                        directionFacingUnitVector = targetDirectionUnitVector;
                        state = unitState.Idle;
                        currentCommand = COMMAND.Nothing;
                        return;
                    }
                }

                // Apply movement
                var directionVector = (targetPosition - GlobalPosition).Normalized();
                velocity = directionVector * unitBaseSpeed * speedModifier;
                GlobalPosition += velocity * deltaf;
                // face toward target position in cardinal direction.
                directionFacingUnitVector = RoundToNearestCardinalDirection(directionVector);

                animationPlayer.UpdateAnimation(directionFacingUnitVector, "walk");
                break;
            case unitState.Attacking:
                // if targetting a guy and not dead and it moved out of range.
                if (currentCommand == COMMAND.AttackTarget && !targetsInRangeAndAlive.Contains(currentTarget) && (currentTarget.GetCurrentHealth() >= 0))
                {
                    state = unitState.MoveToPosition;
                }
                else if (currentCommand == COMMAND.AttackTarget && currentTarget.GetCurrentHealth() <= 0)
                {
                    currentCommand = COMMAND.Nothing;
                    state = unitState.Idle;
                }
                else if ((currentCommand == COMMAND.Nothing) && targetsInRangeAndAlive.Count == 0)
                {
                    state = unitState.Idle;
                }
                else if (currentCommand == COMMAND.MoveToPosition)
                {
                    state = unitState.MoveToPosition;
                }
                

                // animation face the direction of the one your attacking.
                if (targetsInRangeAndAlive.Count != 0)
                {
                    var direction = (targetsInRangeAndAlive.First().GlobalPosition - GlobalPosition).Normalized();
                    directionFacingUnitVector = RoundToNearestCardinalDirection(direction);
                }

                animationPlayer.UpdateAnimation(directionFacingUnitVector, "attack_1");
                break;
            case unitState.Dead:
                break;
            default:
                break;
        }
    }


    private Vector2 RoundToNearestCardinalDirection(Vector2 vector)
    {
        // face toward target position in cardinal direction.
        var cardinalDirection = new Vector2(Math.Sign((int)Math.Round(vector.X)), Math.Sign((int)Math.Round(vector.Y)));

        // Favour Horizontal movement animations when on an angle.
        if (Math.Abs(cardinalDirection.X) == Math.Abs(cardinalDirection.Y))
        {
            cardinalDirection.Y = 0;
        }

        return cardinalDirection;
    }

    private Array<StatusComponent.STATUS> activeStatuses = [];

    public void UpdateStatusEffects(Godot.Collections.Array<StatusComponent.STATUS> updatedActiveStatuses)
    {
        foreach (var status in updatedActiveStatuses)
        {
            TryAddStatus(status);
        }

        // if there is a status on here that is not in active statuses remove it
        var oldStatusesToRemove = activeStatuses.Where(x => !updatedActiveStatuses.Contains(x));
        foreach (var oldStatus in oldStatusesToRemove)
        {
            RemoveStatus(oldStatus);
        }
    }

    private void RemoveStatus(StatusComponent.STATUS status)
    {
        switch (status)
        {
            case StatusComponent.STATUS.fresh:
                this.speedModifier -= GlobalGameVariables.FRESH_SPEED_MODIFIER;
                break;
            default:
                break;
        }

        activeStatuses.Remove(status);
    }

    private void TryAddStatus(StatusComponent.STATUS status)
    {
        if (activeStatuses.Contains(StatusComponent.STATUS.fresh))
        {
            return;
        }

        // TODO the actual stat changes.
        switch (status)
        {
            case StatusComponent.STATUS.fresh:
                this.speedModifier += GlobalGameVariables.FRESH_SPEED_MODIFIER;
                break;
            default:
                break;
        }

        activeStatuses.Add(status);
    }
}
