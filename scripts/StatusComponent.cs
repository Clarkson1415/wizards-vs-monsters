using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Security;
using WizardsVsMonster.scripts;

public partial class StatusComponent : Control
{
	[Export] private TextureRect healthFill;

	[Export] private TextureRect healthBackground;

	private float maxHealthBarSize;

    [Export] private Godot.Collections.Dictionary<STATUS, Texture2D> statusTextures;
	[Export] private VBoxContainer colLeft;
	[Export] private VBoxContainer colRight;

	public System.Collections.Generic.List<STATUS> GetActiveStatuses()
    {
        return activeStatuses.Keys.ToList();
    }

    public void Setup()
	{
		maxHealthBarSize = healthFill.Size.X;
    }

	public void InitialiseInitialStatuses(Array<STATUS> statuses)
	{
		foreach (var stat in statuses)
		{
			switch (stat)
			{
				case STATUS.fresh:
					AddStatusWithTimer(stat, GlobalGameVariables.FRESH_STATUS_TIME);
					break;
				default:
					break;
			}
		}
	}

    /// <summary>
    /// For ones like fresh and stuff.
    /// </summary>
    private void AddStatusWithTimer(STATUS status, double time)
	{
		var added = TryAddStatus(status);

		if (!added) { return; }

		var newTimer = new Timer();
        AddChild(newTimer);
        newTimer.WaitTime = time;

		newTimer.Start();
        newTimer.Timeout += () => RemoveStatus(status);
        newTimer.Timeout += newTimer.QueueFree;
    }

    public void UpdateHealthPercentage(float healthPercentage)
	{
        if (healthPercentage < 0.3f)
        {
            TryAddStatus(StatusComponent.STATUS.dying);
        }
        else
        {
            RemoveStatus(StatusComponent.STATUS.dying);
        }

        var fillAmount = healthPercentage * maxHealthBarSize;
		var newSize = new Vector2(fillAmount, healthFill.Size.Y);
        healthFill.SetSize(newSize);
    }

	public enum STATUS { dying, bracing, determined, fresh }

	private Dictionary<STATUS, TextureRect> activeStatuses = [];

	private bool TryAddStatus(STATUS newStat)
	{
		if (activeStatuses.ContainsKey(newStat))
		{
			return false;
		}

		Logger.Log($"Status added: {newStat}");
		var newTexture = new TextureRect();
		newTexture.Texture = statusTextures[newStat];
		activeStatuses.Add(newStat, newTexture);

        var colToAddTo = colLeft.GetChildCount() <= colRight.GetChildCount() ? colLeft : colRight;
		colToAddTo.AddChild(newTexture);
		return true;
    }

    private void RemoveStatus(STATUS stat)
	{
		if (!activeStatuses.ContainsKey(stat))
		{
			Logger.Log($"did not contain key: {stat}");
			return;
		}

        var imageNode = activeStatuses[stat];

        Logger.Log($"Removed status: {stat}");
        imageNode.Free();
		activeStatuses.Remove(stat);
    }
}
