using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Units movement and attacking. Active behaviours.
/// Unit passive things like taking damage go in unitarea.
/// </summary>
public partial class OgreOfKersh : MovingGameUnit
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();

        // TODO other ready stuff.
        // Ogre Stinky status?
    }

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
                targetsInRange.Add(targetAsArea);
            }
        }
    }

    private int damage = 5;

    /// <summary>
    /// When the animation frame that the attack animation should hit an enemy plays.
    /// </summary>
    private void OnAttackFrame()
    {
        foreach (var tar in targetsInRange)
        {
            tar.TakeDamage(this.damage);
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        base._Process(delta);

        // TODO this will go in a parent class. called FightingUnit.
        // TODO: will be able to manually assign targets. via clicking unit clicking enemy. and then will need to add if assignedTargets ignore targets and chase them.

        // TODO implement unit range? no need. will be done via the raycast 2d.
        UpdateTargetsInRange();

        if (targetsInRange.Count != 0)
		{
            unitsAreaOrBodyAndArmour.UpdateAnimation("attack");
        }
        else
		{
            MoveInDirectionFacing(delta);
            unitsAreaOrBodyAndArmour.UpdateAnimation(velocity);
        }
    }

	private void MoveInDirectionFacing(double delta)
	{
        float deltaf = (float)delta;

        // Movement vector
        velocity = directionFacing * unitBaseSpeed * speedModifier;

        // Apply movement
        GlobalPosition += velocity * deltaf;
    }
}
