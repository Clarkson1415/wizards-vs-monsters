using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
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

    private int baseUnitDamagePerAnimation = 10;

    public GameUnitResource resource { get; private set; }

    protected int unitBaseSpeed;

    protected int speedModifier = 1;

    protected Vector2 velocity;

    // TODO other rays.
    [Export] private RayCast2D ray_left;

    private List<UnitBody> targetsInRange = new List<UnitBody>();

    private void UpdateTargetsInRange()
    {
        targetsInRange.Clear();

        // TODO: when implement 4 directions, check all raycasters not just the left one.
        if (ray_left.IsColliding())
        {
            var target = ray_left.GetCollider();
            if (target is UnitBody targetAsArea)
            {
                if (targetAsArea.GetCurrentHealth() <= 0)
                {
                    return;
                }
                else if (!GlobalGameVariables.FactionEnemies[this.resource.GetFaction()].Contains(targetAsArea.GetFaction()))
                {
                    return;
                }

                targetsInRange.Add(targetAsArea);
            }
        }
    }

    /// <summary>
    /// When the animation frame that the attack animation should hit an enemy plays.
    /// </summary>
    private void OnAttackFrame()
    {
        foreach (var tar in targetsInRange)
        {
            tar.TakeDamage(this.baseUnitDamagePerAnimation);
        }
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
        Moving,
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

    public void SetNewTargetPositionRotation(Vector2 pos, Vector2 dir)
    {
        targetPosition = pos;
        targetDirectionUnitVector = dir;
    }

    private void OnHit()
    {
        animationPlayer.UpdateAnimation(directionFacingUnitVector, "hurt");
        state = unitState.Dead;
    }

    private bool AtTargetLocation()
    {
        return (Math.Round(GlobalPosition.X, 1) == Math.Round(targetPosition.X, 1)) && (Math.Round(GlobalPosition.Y, 1) == Math.Round(targetPosition.Y, 1));
    }

    private bool AtTargetDirection()
    {
        return (targetDirectionUnitVector == directionFacingUnitVector);
    }

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
        else if (!AtTargetLocation() || !AtTargetDirection())
        {
            state = unitState.Moving;
        }

        float deltaf = (float)delta;
        switch (this.state)
        {
            case unitState.Idle:
                velocity = Vector2.Zero;
                if (targetsInRange.Count != 0)
                {
                    state = unitState.Attacking;
                }

                animationPlayer.UpdateAnimation(directionFacingUnitVector, "idle");
                break;
            case unitState.Moving:
                if (AtTargetLocation())
                {
                    // TODO: rotate over time to be in target rotation
                    directionFacingUnitVector = targetDirectionUnitVector;
                    state = unitState.Idle;
                    return;
                }

                // Apply movement
                float radians = this.GlobalRotationDegrees * (float)Math.PI / 180f;
                var directionVector = new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
                velocity = directionVector * unitBaseSpeed * speedModifier;
                GlobalPosition += velocity * deltaf;

                animationPlayer.UpdateAnimation(directionFacingUnitVector, "walk");
                break;
            case unitState.Attacking:
                animationPlayer.UpdateAnimation(directionFacingUnitVector, "attack_1");
                if (targetsInRange.Count == 0)
                {
                    state = unitState.Idle;
                }
                break;
            case unitState.Dead:
                break;
            default:
                break;
        }
    }

    private Array<StatusComponent.STATUS> activeStatuses = [];

    public void ApplyStatusEffects(System.Collections.Generic.List<StatusComponent.STATUS> statuses)
    {
        foreach (var status in statuses)
        {
            TryAddStatus(status);
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
                break;
            default:
                break;
        }

        activeStatuses.Add(status);
    }
}
