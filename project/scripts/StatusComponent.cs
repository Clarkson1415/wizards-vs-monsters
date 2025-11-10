 using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

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

    public void UpdateStatuses(Array<STATUS> currentStatuses)
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

        // idle is different
        if (!currentStatuses.Contains(STATUS.idle))
        {
            labelZZZAnimationPlayer.Play("blank");
        }
    }

    public void UpdateHealthPercentage(float healthPercentage)
    {
        var fillAmount = healthPercentage * maxHealthBarSize;
        var newSize = new Vector2(fillAmount, healthFill.Size.Y);
        healthFill.SetSize(newSize);
    }

    public enum STATUS 
    {
        /// <summary>
        /// When most units in a group are under a tree the unit group is hidden.
        /// </summary>
        Hidden,

        /// <summary>
        /// Health is very low <= 1/6th
        /// </summary>
        dying, 

        /// <summary>
        /// In formation state.
        /// </summary>
        bracing,

        /// <summary>
        /// Determined when close to winning or empowered by a leader. or low on health - and surrounded by allies.
        /// </summary>
        determined, 

        /// <summary>
        /// When newly born.
        /// </summary>
        fresh,

        /// <summary>
        /// in idle state. no cmd.
        /// </summary>
        idle,

        /// <summary>
        /// when units are in the fleeing state.
        /// </summary>
        fleeing,
    }

    private Godot.Collections.Dictionary<STATUS, TextureRect> displayedStatuses = [];

    private void TryAddStatus(STATUS newStat)
    {
        if (newStat == STATUS.idle)
        {
            labelZZZAnimationPlayer.Play("loop");
            return;
        }
        else if (displayedStatuses.ContainsKey(newStat))
        {
            return;
        }

        Logger.Log($"Status added: {newStat}");
        var newTexture = new TextureRect();

        if (!statusTextures.ContainsKey(newStat))
        {
            Logger.Log($"MISSIN KEY: {newStat}");
            return;
        }

        newTexture.Texture = statusTextures[newStat];
        displayedStatuses.Add(newStat, newTexture);

        var list = new List<VBoxContainer>() { colLeft, colMidle, colRight };
        var colToAddTo = list.OrderBy(x => x.GetChildCount()).First();
        colToAddTo.AddChild(newTexture);
        return;
    }

    private void RemoveStatus(STATUS stat)
    {
        if (stat == STATUS.idle)
        {
            labelZZZAnimationPlayer.Play("blank");
            return;
        }
        else if (!displayedStatuses.ContainsKey(stat))
        {
            Logger.Log($"did not contain key: {stat}");
            return;
        }

        var imageNode = displayedStatuses[stat];

        Logger.Log($"Removed status: {stat}");
        imageNode.Free();
        displayedStatuses.Remove(stat);
    }
}
