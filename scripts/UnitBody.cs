using Godot;
using WizardsVsMonster.scripts;

/// <summary>
/// Represents the units area2d.
/// </summary>
public partial class UnitBody : Area2D
{
    private int armour;

	private float currentHealth;

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    [Signal] public delegate void OnThisHitEventHandler(GameUnit unit);

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

    public void TakeDamage(int damage, GameUnit hitBy)
	{
		// TODO calculate armour and shit.
		currentHealth -= damage;
        EmitSignal(SignalName.OnThisHit, hitBy);
    }
}
