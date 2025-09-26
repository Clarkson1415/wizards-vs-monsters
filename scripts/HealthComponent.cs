using Godot;
using System;
using System.Security;

public partial class HealthComponent : Control
{
	private float maxHealth;

	[Export] private TextureRect healthFill;

	[Export] private TextureRect healthBackground;

	private float maxHealthBarSize;

	public void Setup(int maxHealth)
	{
		this.maxHealth = maxHealth;

		maxHealthBarSize = healthFill.Size.X;
    }

	public void UpdateHealthBar(float currentHealth)
	{
		var fillAmount = (currentHealth / maxHealth) * maxHealthBarSize;
		var newSize = new Vector2(fillAmount, healthFill.Size.Y);
        healthFill.SetSize(newSize);
    }
}
