using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using WizardsVsMonster.scripts;

public partial class StatusComponent : Control
{
	[Export] private TextureRect healthFill;
	private float maxHealthBarSize;
    [Export] private Godot.Collections.Dictionary<STATUS, Texture2D> statusTextures;
	[Export] private VBoxContainer colLeft;
	[Export] private VBoxContainer colMidle;
	[Export] private VBoxContainer colRight;
	[Export] AnimationPlayer labelZZZAnimationPlayer;

    public override void _Ready()
    {
		maxHealthBarSize = healthFill.Size.X;
		labelZZZAnimationPlayer.Play("blank");
    }

	public void AddStatuses(Array<STATUS> currentStatuses)
	{
		foreach (var stat in currentStatuses)
		{
			TryAddStatus(stat);
		}

        // if there is a status on here that is not in active statuses remove it
        var oldStatusesToRemove = displayedStatuses.Keys.Where(x => !currentStatuses.Contains(x));
        foreach (var oldStatus in oldStatusesToRemove)
        {
            RemoveStatus(oldStatus);
        }
    }

    public void UpdateHealthPercentage(float healthPercentage)
	{
        var fillAmount = healthPercentage * maxHealthBarSize;
		var newSize = new Vector2(fillAmount, healthFill.Size.Y);
        healthFill.SetSize(newSize);
    }

	public enum STATUS { dying, bracing, determined, fresh, idle }

	private Godot.Collections.Dictionary<STATUS, TextureRect> displayedStatuses = [];

	private bool TryAddStatus(STATUS newStat)
	{
		if (displayedStatuses.ContainsKey(newStat))
		{
			return false;
		}

		if (newStat == STATUS.idle)
		{
			labelZZZAnimationPlayer.Play("loop");
		}

		Logger.Log($"Status added: {newStat}");
		var newTexture = new TextureRect();
		newTexture.Texture = statusTextures[newStat];
		displayedStatuses.Add(newStat, newTexture);

		var list = new List<VBoxContainer>() { colLeft, colMidle, colRight };
		var colToAddTo = list.OrderBy(x => x.GetChildCount()).First();
        colToAddTo.AddChild(newTexture);
		return true;
    }

    private void RemoveStatus(STATUS stat)
	{
		if (!displayedStatuses.ContainsKey(stat))
		{
			Logger.Log($"did not contain key: {stat}");
			return;
		}

        if (stat == STATUS.idle)
        {
            labelZZZAnimationPlayer.Play("blank");
        }

        var imageNode = displayedStatuses[stat];

        Logger.Log($"Removed status: {stat}");
        imageNode.Free();
		displayedStatuses.Remove(stat);
    }
}
