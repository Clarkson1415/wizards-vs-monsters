using Godot;
using System.Collections.Generic;
using WizardsVsMonster.scripts;

/// <summary>
/// Represents the game unit on the board.
/// </summary>
[GlobalClass]
public partial class GameUnit : Node2D
{
    [Export] private ClickableUnitComponent clickableUnitComponent;

    [Export] protected UnitBody unitsAreaOrBodyAndArmour;

    private int baseUnitDamagePerAnimation = 10;

    private GameUnitResource resource;

    protected int unitBaseSpeed;

    protected int speedModifier = 1;

    protected Vector2 directionFacing = Vector2.Left;

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

    public float GetCurrentHealth()
    {
        return unitsAreaOrBodyAndArmour.GetCurrentHealth();
    }

    public ClickableUnitComponent GetClickableUnitComponent()
    {
        return clickableUnitComponent;
    }

    public GameUnitResource GetResourceReference()
    {
        return this.resource;
    }

    public void Setup(GameUnitResource resource)
    {
        base._Ready();

        this.resource = resource;
        if (resource == null)
        {
            Logger.LogError($"unit data resource not loaded for {this.Name}");
        }

        unitBaseSpeed = this.GetResourceReference().GetSpeed();
        baseUnitDamagePerAnimation = this.GetResourceReference().GetDPS();
        // TODO scale damage to attack animation speed? or just make sure they match in the resource. OR attack at that dmg per second?

        clickableUnitComponent.Setup(resource);
        unitsAreaOrBodyAndArmour.Setup(this.GetResourceReference());
    }

    private enum unitState
    {
        Idle,
        Moving,
        Attacking,
        Dead,
    }

    private void MoveInDirectionFacing(double delta)
    {
        float deltaf = (float)delta;

        // Movement vector
        velocity = directionFacing * unitBaseSpeed * speedModifier;

        // Apply movement
        GlobalPosition += velocity * deltaf;
    }

    private unitState state = unitState.Idle;

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (this.resource == null)
        {
            return;
        }

        // TODO: will be able to manually assign targets. via clicking unit clicking enemy. and then will need to add if assignedTargets ignore targets and chase them.
        // TODO implement unit range? no need. will be done via the raycast 2d.
        UpdateTargetsInRange();

        if (targetsInRange.Count != 0)
        {
            state = unitState.Attacking;
        }
        else
        {
            state = unitState.Idle;
        }

        switch (this.state)
        {
            case unitState.Idle:
                velocity = Vector2.Zero;
                unitsAreaOrBodyAndArmour.UpdateAnimation("idle");
                // if AI faction move towards nearest enemy.
                if (this.resource.GetFaction() != GlobalGameVariables.PlayerControlledFaction)
                {
                    this.state = unitState.Moving;
                }
                break;
            case unitState.Moving:
                MoveInDirectionFacing(delta);
                unitsAreaOrBodyAndArmour.UpdateAnimation(velocity);
                break;
            case unitState.Attacking:
                unitsAreaOrBodyAndArmour.UpdateAnimation("attack");
                break;
            default:
                break;
        }
    }

    public void ApplyStatusEffects(Godot.Collections.Array<StatusComponent.STATUS> statuses)
    {
        foreach (var status in statuses)
        {
            // TODO the actual stat changes.
        }
    }
}
