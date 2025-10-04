using Godot;
using System.Collections.Generic;
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

    /// <summary>
    /// If this unit is under at least 1 tree or other view obstruction.
    /// </summary>
    public bool IsOnTreeCell => viewBlockingAreasThatTheBodyIsUnder.Count > 0;

    public void Setup(GameUnitResource data)
	{
        this.faction = data.GetFaction();
        this.armour = data.GetArmour();
		currentHealth = data.GetHealth();
        maxHealth = data.GetHealth();
        AreaEntered += OnAreaEntered;
        AreaExited += OnAreaExited;
    }

    private readonly List<Area2D> viewBlockingAreasThatTheBodyIsUnder = new List<Area2D>();

    private void OnAreaEntered(Area2D area)
    {
        if (area.GetCollisionLayerValue(GlobalGameVariables.ViewBlockingAreaCollisionLayer))
        {
            if (viewBlockingAreasThatTheBodyIsUnder.Contains(area))
            {
                Logger.Log("in tree");
                return;
            }

            viewBlockingAreasThatTheBodyIsUnder.Add(area);
        }
    }

    private void OnAreaExited(Area2D area)
    {
        if (area.GetCollisionLayerValue(GlobalGameVariables.ViewBlockingAreaCollisionLayer))
        {
            if (!viewBlockingAreasThatTheBodyIsUnder.Contains(area))
            {
                return;
            }

            viewBlockingAreasThatTheBodyIsUnder.Remove(area);
        }
    }

    public void TakeDamage(int damage, GameUnit hitBy)
	{
		// TODO calculate armour and shit.
		currentHealth -= damage;
        EmitSignal(SignalName.OnThisHit, hitBy);
    }
}
