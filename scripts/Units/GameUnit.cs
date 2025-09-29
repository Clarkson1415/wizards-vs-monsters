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
public partial class GameUnit : CharacterBody2D
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
            targetsInRange.Remove(unit);
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
        UpdateTargets();
        if (targetsInRange.Count == 0) { return; }

        if (currentTarget == null || (currentTarget.GetCurrentHealth() <= 0))
        {
            targetsInRange.First().TakeDamage(this.baseUnitDamagePerAnimation, this);
        }
        else if (targetsInRange.Contains(currentTarget))
        {
            currentTarget.TakeDamage(this.baseUnitDamagePerAnimation, this);
        }

        //foreach (var tar in targetsInRange)
        //{
        //    tar.TakeDamage(this.baseUnitDamagePerAnimation);
        //}
    }

    public void Setup(GameUnitResource resource)
    {
        base._Ready();

        var relevantSprite2d = resource.GetAnimatedSprite2D().Instantiate<AnimatedSprite2D>();
        AddChild(relevantSprite2d);
        relevantSprite2d.Visible = false;

        // set this sprites 2d frames to the aboves
        this.animatedSprite2D.SpriteFrames = relevantSprite2d.SpriteFrames;
        this.animatedSprite2D.Play();

        // make sure material is set as unique otherwise white flashes on all units.
        this.animatedSprite2D.Material = this.animatedSprite2D.Material.DuplicateDeep() as Material;

        var listbefore = this.animationPlayer.GetAnimationLibraryList();

        foreach (var lib in this.animationPlayer.GetAnimationLibraryList())
        {
            this.animationPlayer.RemoveAnimationLibrary(lib);
        }

        var list = this.animationPlayer.GetAnimationLibraryList();

        var libraryName = "added";
        this.animationPlayer.AddAnimationLibrary(libraryName, resource.GetAnimationLibrary());
        var aniamtoins = this.animationPlayer.GetAnimationList();

        this.animationPlayer.SetAnimationLibraryName(libraryName);

        this.resource = resource;
        if (resource == null)
        {
            Logger.LogError($"unit data resource not loaded for {this.Name}");
        }

        unitBaseSpeed = this.resource.GetSpeed();
        baseUnitDamagePerAnimation = this.resource.GetDPS();

        ClickableUnitComponent.Setup(resource);
        UnitBody.Setup(this.resource);

        // TODO setup programmaticalloy the unit range area2d
        // button, clickable unit, unitbodysize, agent navigion radius etc. anything unit size or resource dependent.

        rangeInPixels = ((this.resource.GetSizeInUnits() * GlobalGameVariables.CELL_SIZE) / 2) + (this.resource.GetRange() * GlobalGameVariables.CELL_SIZE);

        UnitBody.OnThisHit += OnHit;

        inRangeArea.AreaEntered += OnUnitAreaEnteredChunk;
        inRangeArea.AreaExited += OnUnitAreaExitedChunk;

        navAgent.VelocityComputed += OnVelocityComputed;
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
        navAgent.TargetPosition = pos;
        targetDirectionUnitVector = dir;
    }

    public void SetTargetUnit(GameUnit enemyToTarget)
    {
        CurrentCommand = COMMAND.AttackTarget;
        currentTarget = enemyToTarget.UnitBody;
        navAgent.TargetPosition = GetPositionWhereUnitIsJustInRange(currentTarget);
        targetDirectionUnitVector = RoundToNearestCardinalDirection((navAgent.TargetPosition - GlobalPosition).Normalized());
    }

    private void OnHit(GameUnit attacker)
    {
        Logger.Log($"this at {Position} hit by {attacker.Position}, attackers faction: {attacker.resource.GetFaction()} tihs faction: {this.resource.GetFaction()}");
        Logger.Log($"attackers targets {attacker.targetsInRange}");
        Logger.Log($"is this in attackers targets {attacker.targetsInRange.Contains(this.UnitBody)}");

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

    private bool AtTargetDirection()
    {
        return (targetDirectionUnitVector == directionFacingUnitVector);
    }

    private UnitBody currentTarget;

    private Vector2 GetPositionWhereUnitIsJustInRange(UnitBody areaToTarget)
    {
        return areaToTarget.GlobalPosition;

        // TODO: not working.

        Vector2 currentPosition = GlobalPosition;
        Vector2 targetCenter = areaToTarget.GlobalPosition;

        // 1. Calculate the vector from the target's center to the unit's current position.
        Vector2 directionToCurrent = currentPosition - targetCenter;

        // 2. Normalize the direction vector to get a unit vector (length of 1).
        // If the unit is exactly on top of the target, this will result in a zero vector.
        if (directionToCurrent.LengthSquared() < 0.0001f)
        {
            // Handle the case where the unit and target are at the same spot.
            // Choose an arbitrary direction (e.g., right) and move out by 'rangeInPixels'.
            return targetCenter + Vector2.Right * rangeInPixels;
        }

        Vector2 directionUnit = directionToCurrent.Normalized();

        // 3. Calculate the desired position.
        // Move away from the 'targetCenter' by *exactly* 'rangeInPixels'.
        Vector2 idealPosition = targetCenter + (directionUnit * rangeInPixels);

        return idealPosition;
    }

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

    private void UpdateTargets()
    {
        // remove dead enemy targets
        if (targetsInNeabyChunkAndAlive.Any(x => x.GetCurrentHealth() <= 0))
        {
            targetsInNeabyChunkAndAlive.RemoveAll(x => x.GetCurrentHealth() <= 0);
        }

        targetsInRange.Clear();
        // update targets in range
        foreach (var unitBody in targetsInNeabyChunkAndAlive)
        {
            // calculate boolean return true if area is witihin the distance between this.GlobalPosition and the float rangeInPixels
            if (IsAreaWithinRange(unitBody))
            {
                targetsInRange.Add(unitBody);
            }
        }
    }

    [Export] private CollisionShape2D collisionShape;

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (this.resource == null) { return; }

        if (UnitBody.GetCurrentHealth() <= 0 && state != unitState.Dead)
        {
            animationPlayer.UpdateAnimation(directionFacingUnitVector, "die");
            this.CollisionMask = 0;
            this.CollisionLayer= 0;
            this.collisionShape.Disabled = true;
            this.navAgent.QueueFree();
            state = unitState.Dead;
            return;
        }

        UpdateTargets();

        float deltaf = (float)GetPhysicsProcessDeltaTime();
        switch (this.state)
        {
            case unitState.Idle:
                navAgent.SetVelocity(Vector2.Zero);

                var atPos = (this.GlobalPosition - navAgent.TargetPosition).Length() < navAgent.TargetDesiredDistance;
                var hasBeenPushedFromPosition = navAgent.IsNavigationFinished() && !atPos;

                if ((CurrentCommand == COMMAND.MoveToPosition && (!navAgent.IsNavigationFinished() || hasBeenPushedFromPosition))
                    || (CurrentCommand == COMMAND.AttackTarget))
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
                    navAgent.TargetPosition = GetPositionWhereUnitIsJustInRange(currentTarget);
                    targetDirectionUnitVector = RoundToNearestCardinalDirection((navAgent.TargetPosition - GlobalPosition).Normalized());

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
                    if (navAgent.IsNavigationFinished())
                    {
                        // TODO: rotate over time to be in target rotation
                        directionFacingUnitVector = targetDirectionUnitVector;
                        state = unitState.Idle;
                        CurrentCommand = COMMAND.Nothing;
                        return;
                    }
                }

                //if (navAgent.GetCurrentNavigationResult().PathLength < 2)
                //{
                //    // Path is impossible (only contains the start position)
                //    return;
                //}

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
                navAgent.SetVelocity(Vector2.Zero);

                if (animationPlayer.CurrentAnimation.Contains("attack"))
                {
                    // wait for attack animation to finish.
                    return;
                }

                // animation face the direction of the one your attacking.
                if (targetsInRange.Count != 0)
                {
                    var direction = (targetsInRange.First().GlobalPosition - GlobalPosition).Normalized();
                    directionFacingUnitVector = RoundToNearestCardinalDirection(direction);
                }

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

                    return;
                }
                else if ((CurrentCommand == COMMAND.Nothing) && targetsInRange.Count == 0)
                {
                    state = unitState.Idle;
                    return;
                }
                else if (CurrentCommand == COMMAND.MoveToPosition)
                {
                    state = unitState.MoveToPosition;
                    return;
                }

                animationPlayer.UpdateAnimation(directionFacingUnitVector, "attack_1");
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

    private void OnVelocityComputed(Vector2 safeVelocity)
    {
        //if (safeVelocity.Round() == Vector2.Zero)
        //{
        //    return;
        //}
        //else
        //{
        //    Velocity = safeVelocity;
        //    MoveAndSlide();
        //}

        Velocity = safeVelocity;
        MoveAndSlide();
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
