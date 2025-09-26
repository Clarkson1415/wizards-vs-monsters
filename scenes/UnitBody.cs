using Godot;
using System;
using WizardsVsMonster.scripts;

/// <summary>
/// Represents the units area2d.
/// Kind of like the units body. has armour and health.
/// Deals with passive things. like taking damage.
/// </summary>
public partial class UnitBody : Area2D
{
	[Export] private HealthComponent healthComponent;

    [Export] private AnimationComponent animComponent;

    private int armour;

	private float currentHealth;

	public void Setup(GameUnitResource data)
	{
		this.armour = data.GetArmour();

		currentHealth = data.GetHealth();
        healthComponent.Setup(data.GetHealth());
	}

	public void TakeDamage(int damage)
	{
		// TODO calculate armour and shit.
		currentHealth -= damage;
        healthComponent.UpdateHealthBar(currentHealth);

        if (currentHealth <= 0) 
        {
            animComponent.UpdateAnimation("die");
        }
        else
        {
            animComponent.UpdateAnimation("hurt");
        }
    }

    public void UpdateAnimation(string animName)
    {
        animComponent.UpdateAnimation(animName);
    }

    public void UpdateAnimation(Vector2 velocity)
    {
        animComponent.UpdateAnimation(velocity);
    }
}
