using Godot;
using System;
using WizardsVsMonster.scripts;

/// <summary>
/// Represents the units area2d.
/// Kind of like the units body. has armour and health.
/// Deals with passive things. like taking damage. or effects and stuff.
/// </summary>
public partial class UnitBody : Area2D
{
    [Export] private AnimationComponent animComponent;

    private int armour;

	private float currentHealth;

    private float maxHealth;

    private StatusComponent statusComponent;

    public void Setup(GameUnitResource data, StatusComponent statComponent)
	{
        this.statusComponent = statComponent;
		this.armour = data.GetArmour();
		currentHealth = data.GetHealth();
        maxHealth = data.GetHealth();
	}

    private bool isDead;

    public void TakeDamage(int damage)
	{
        if (isDead)
        {
            return;
        }

		// TODO calculate armour and shit.
		currentHealth -= damage;
        statusComponent.UpdateHealthPercentage(currentHealth / maxHealth);

        if (currentHealth <= 0) 
        {
            isDead = true;
            Monitorable = false;
            animComponent.UpdateAnimation("die");
        }
        else
        {
            animComponent.UpdateAnimation("hurt");
        }
    }

    public void UpdateAnimation(string animName)
    {
        if (isDead)
        {
            return;
        }

        animComponent.UpdateAnimation(animName);
    }

    public void UpdateAnimation(Vector2 velocity)
    {
        if (isDead)
        {
            return;
        }

        animComponent.UpdateAnimation(velocity);
    }
}
