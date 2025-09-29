using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using WizardsVsMonster.scripts;

/// <summary>
/// Represents the game unit on the board.
/// </summary>
[GlobalClass]
public partial class GameUnit : Node2D
{
    [Export] public NavigationAgent2D navAgent;

    [Export] public ClickableUnitComponent ClickableUnitComponent { get; private set; }

    [Export] public UnitBody UnitBody { get; private set; }

    [Export] private AnimationComponent animationPlayer;

    [Export] private AnimatedSprite2D animatedSprite2D;

    private int baseUnitDamagePerAnimation = 1;

    public GameUnitResource resource { get; private set; }

    protected float unitBaseSpeed;

    protected float speedModifier = 1;

    protected Vector2 velocity;

    private float rangeInPixels;

    public bool IsDead => UnitBody.GetCurrentHealth() <= 0;

    [Export] private Area2D inRangeArea;

    private List<UnitBody> targetsInNeabyChunkAndAlive = new List<UnitBody>();
    private List<UnitBody> targetsInRange = new List<UnitBody>();

    [Signal] public delegate void OnAttackedEventHandler(UnitGroup attackers);

    /// <summary>
    /// Emitted when target moved out of range or died while being targeted.
    /// </summary>
    /// <param name="attackers"></param>
    [Signal] public delegate void OnTargetsMovedAwayWhileAttackingEventHandler(UnitGroup attackers);

    public void OnUnitAreaEnteredChunk(Area2D area)
    {
        var unit = area as UnitBody;
        if (unit == null)
        {
            return;
        }

        if (IsUnitAnEnemy(unit) && (unit.GetCurrentHealth() > 0) && (!targetsInNeabyChunkAndAlive.Contains(unit)))
        {
            targetsInNeabyChunkAndAlive.Add(unit);
        }
    }

    public void OnUnitAreaExitedChunk(Area2D area)
    {
        var unit = area as UnitBody;
        if (unit == null)
        {
            return;
        }

        if (IsUnitAnEnemy(unit) && targetsInNeabyChunkAndAlive.Contains(unit))
        {
            targetsInNeabyChunkAndAlive.Remove(unit);
        }
    }

    private bool IsUnitAnEnemy(UnitBody targetAsArea)
    {
        return GlobalGameVariables.FactionEnemies[this.resource.GetFaction()].Contains(targetAsArea.GetFaction());
    }

    /// <summary>
    /// When the animation frame that the attack animation should hit an enemy plays.
    /// </summary>
    private void OnAttackFrame()
    {
        if (targetsInRange.Count == 0) { return; }

        targetsInRange.First().TakeDamage(this.baseUnitDamagePerAnimation, this);

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

        rangeInPixels = ((this.resource.GetSizeInUnits() * GlobalGameVariables.CELL_SIZE) / 2) + (this.resource.GetRange() * GlobalGameVariables.CELL_SIZE);

        UnitBody.OnThisHit += OnHit;

        inRangeArea.AreaEntered += OnUnitAreaEnteredChunk;
        inRangeArea.AreaEntered += OnUnitAreaEnteredChunk;

        if (navAgent != null)
        {
            navAgent.VelocityComputed += OnVelocityComputed;
        }
    }

    public void SetInitialDirectionFacing(Vector2 direction)
    {
        directionFacingUnitVector = direction;
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
    private Vector2 directionFacingUnitVector = Vector2.Right;

    public enum COMMAND { MoveToPosition, AttackTarget, Nothing };

    public COMMAND CurrentCommand { get; private set; } = COMMAND.Nothing;

    public void SetTargetPosition(Vector2 pos, Vector2 dir)
    {
        CurrentCommand = COMMAND.MoveToPosition;
        targetPosition = pos;
        targetDirectionUnitVector = dir;
    }

    public void SetTargetUnit(GameUnit enemyToTarget)
    {
        CurrentCommand = COMMAND.AttackTarget;
        currentTarget = enemyToTarget.UnitBody;
        targetPosition = enemyToTarget.GlobalPosition;
        targetDirectionUnitVector = RoundToNearestCardinalDirection((targetPosition - GlobalPosition).Normalized());
    }

    private void OnHit(GameUnit attacker)
    {
        var material = (ShaderMaterial)animatedSprite2D.Material;
        // Flash white instantly
        material.SetShaderParameter("flash_strength", 1.0f);
        // Reset after a short time
        GetTree().CreateTimer(0.1).Timeout += () =>
        {
            material.SetShaderParameter("flash_strength", 0.0f);
        };

        // tell group were being attacked.
        var group = attacker.GetParent<UnitGroup>();
        if (group == null) { Logger.LogError("NO GROUP WHY"); }
        EmitSignal(SignalName.OnAttacked, group);
    }

    public bool AtTargetLocation => GlobalPosition.DistanceTo(targetPosition) < 1f;

    private bool AtTargetDirection()
    {
        return (targetDirectionUnitVector == directionFacingUnitVector);
    }

    private UnitBody currentTarget;

    /// <summary>
    /// Calculates if an area is within this units range in pixels.
    /// </summary>
    /// <param name="targetArea"></param>
    /// <returns></returns>
    private bool IsAreaWithinRange(UnitBody targetArea)
    {
        // 1. Get the current physics space ID
        var space = GetWorld2D().DirectSpaceState;

        // 2. Define the search parameters (the circular range)
        var intersectionQuery = new PhysicsShapeQueryParameters2D(); // Correct instantiation

        // Create the circle shape
        var shape = new CircleShape2D();
        shape.Radius = rangeInPixels;
        intersectionQuery.Shape = shape;

        // Set the circle's position
        // Transform2D(rotation, position)
        intersectionQuery.Transform = new Transform2D(0, GlobalPosition);

        // Ensure the query checks against Area2Ds
        intersectionQuery.CollideWithAreas = true;

        // Set the collision mask to only check layers the targetArea is on
        intersectionQuery.CollisionMask = targetArea.CollisionLayer;

        // 3. Perform the shape intersection query
        // This returns a list of objects that the circle shape intersects.
        Godot.Collections.Array<Godot.Collections.Dictionary> results = space.IntersectShape(intersectionQuery);

        // 4. Check if the targetArea is in the results
        foreach (Godot.Collections.Dictionary result in results)
        {
            // Check if the 'collider' key exists and its value is an Object (which Area2D is).
            if (result.TryGetValue("collider", out Variant colliderValue) && colliderValue.Obj is Area2D colliderArea)
            {
                // Check if the hit Area2D is the one we're looking for
                if (colliderArea == targetArea)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (this.resource == null) { return; }

        // remove dead enemy targets
        if (targetsInNeabyChunkAndAlive.Any(x => x.GetCurrentHealth() <= 0))
        {
            targetsInNeabyChunkAndAlive.RemoveAll(x => x.GetCurrentHealth() <= 0);
        }

        // update targets in range
        foreach (var unitBody in targetsInNeabyChunkAndAlive)
        {
            if (targetsInRange.Contains(unitBody))
            {
                if (!IsAreaWithinRange(unitBody))
                {
                    targetsInRange.Remove(unitBody);
                }

                continue;
            }

            // calculate boolean return true if area is witihin the distance between this.GlobalPosition and the float rangeInPixels
            if (IsAreaWithinRange(unitBody))
            {
                targetsInRange.Add(unitBody);
            }
        }

        if (UnitBody.GetCurrentHealth() <= 0)
        {
            animationPlayer.UpdateAnimation(directionFacingUnitVector, "die");
            state = unitState.Dead;
        }

        if (animationPlayer.CurrentAnimation.Contains("attack"))
        {
            // wait for attack animation to finish.
            return;
        }

        navAgent.TargetPosition = this.targetPosition;

        float deltaf = (float)GetPhysicsProcessDeltaTime();
        switch (this.state)
        {
            case unitState.Idle:
                velocity = Vector2.Zero;

                if ((CurrentCommand == COMMAND.MoveToPosition && !AtTargetLocation) || (CurrentCommand == COMMAND.AttackTarget))
                {
                    state = unitState.MoveToPosition;
                }
                else if (CurrentCommand == COMMAND.Nothing)
                {
                    if (targetsInRange.Count > 0)
                    {
                        state = unitState.Attacking;
                    }
                }
                animationPlayer.UpdateAnimation(directionFacingUnitVector, "idle");
                break;
            case unitState.MoveToPosition:
                // update target positoin and orientation if following a target.
                if (CurrentCommand == COMMAND.AttackTarget)
                {
                    targetPosition = currentTarget.GlobalPosition;
                    targetDirectionUnitVector = RoundToNearestCardinalDirection((targetPosition - GlobalPosition).Normalized());

                    // TODO: this should be happening before this guy is right on top of the unit.
                    if (targetsInRange.Contains(currentTarget))
                    {
                        state = unitState.Attacking;
                        if (resource.GetFaction() == GlobalGameVariables.FACTION.monsters)
                        {
                            Logger.Log($"{this.Name} found target = {state}");
                        }
                        return;
                    }
                    else if (currentTarget.GetCurrentHealth() <= 0)
                    {
                        CurrentCommand = COMMAND.Nothing;
                        state = unitState.Idle;
                        return;
                    }
                }
                else // command.Move or Command.Nothing
                {
                    if (AtTargetLocation)
                    {
                        // TODO: rotate over time to be in target rotation
                        directionFacingUnitVector = targetDirectionUnitVector;
                        state = unitState.Idle;
                        CurrentCommand = COMMAND.Nothing;
                        return;
                    }
                }

                // Apply movement
                var nextPosition = navAgent.GetNextPathPosition();
                var directionVector = (nextPosition - GlobalPosition).Normalized();
                var desiredVelocity = directionVector * unitBaseSpeed * speedModifier;
                navAgent.SetVelocity(desiredVelocity);

                // face toward target position in cardinal direction.
                directionFacingUnitVector = RoundToNearestCardinalDirection(directionVector);
                animationPlayer.UpdateAnimation(directionFacingUnitVector, "walk");

                if (resource.GetFaction() == GlobalGameVariables.FACTION.monsters)
                {
                    Logger.Log($"{this.Name} moved = {state}");
                }

                break;
            case unitState.Attacking:
                // animation face the direction of the one your attacking.
                if (targetsInRange.Count != 0)
                {
                    var direction = (targetsInRange.First().GlobalPosition - GlobalPosition).Normalized();
                    directionFacingUnitVector = RoundToNearestCardinalDirection(direction);
                }
                animationPlayer.UpdateAnimation(directionFacingUnitVector, "attack_1");

                // check changes
                // if targetting a guy and not dead and it moved out of range or died.
                if (CurrentCommand == COMMAND.AttackTarget && (!targetsInRange.Contains(currentTarget) || currentTarget.GetCurrentHealth() <= 0))
                {
                    state = unitState.Idle;
                    CurrentCommand = COMMAND.Nothing;
                    var group = currentTarget.GetParent<GameUnit>().GetParent<UnitGroup>();
                    EmitSignal(SignalName.OnTargetsMovedAwayWhileAttacking, group);

                    if (resource.GetFaction() == GlobalGameVariables.FACTION.humans)
                    {
                        Logger.Log($"Blue {this.Name} emmitted signal that target left. retargeting. state: {state}");
                    }
                }
                else if ((CurrentCommand == COMMAND.Nothing) && targetsInRange.Count == 0)
                {
                    state = unitState.Idle;
                }
                else if (CurrentCommand == COMMAND.MoveToPosition)
                {
                    state = unitState.MoveToPosition;
                }
                break;
            case unitState.Dead:
                break;
            default:
                break;
        }

        //if (resource.GetFaction() == GlobalGameVariables.FACTION.monsters) 
        //{
        //    Logger.Log($"state = {state}");
        //}

    }


    private Vector2 RoundToNearestCardinalDirection(Vector2 vector)
    {
        // face toward target position in cardinal direction.
        var cardinalDirection = new Vector2(Math.Sign((int)Math.Round(vector.X)), Math.Sign((int)Math.Round(vector.Y)));

        if (cardinalDirection.Y == 0 && cardinalDirection.X == 0)
        {
            return directionFacingUnitVector; // keep same.
        }

        // Favour Horizontal movement animations when on an angle.
        else if (Math.Abs(cardinalDirection.X) == Math.Abs(cardinalDirection.Y))
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

    // Add this method to your GameUnit class
    private void OnVelocityComputed(Vector2 safeVelocity)
    {
        // The safeVelocity is the final, adjusted velocity provided by RVO.
        // We must use this to move the unit.
        if (state == unitState.MoveToPosition)
        {
            // Use delta from _PhysicsProcess/GetPhysicsProcessDeltaTime() for frame-rate independence.
            float delta = (float)GetPhysicsProcessDeltaTime();

            // This is the line that actually applies the RVO-corrected movement
            GlobalPosition += safeVelocity * delta;

            // The rest of the movement logic (like animation updates) should probably 
            // stay in _PhysicsProcess, or at least the animation part could stay there.
            // For simplicity, let's keep the movement part here.
        }
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
