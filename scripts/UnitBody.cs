using Godot;
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

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    private float maxHealth;

    private GlobalGameVariables.FACTION faction;

    public GlobalGameVariables.FACTION GetFaction() { return faction; }

    public void Setup(GameUnitResource data)
	{
        this.faction = data.GetFaction();
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

        if (currentHealth <= 0) 
        {
            isDead = true;
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
