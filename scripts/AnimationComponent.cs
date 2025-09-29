using Godot;
using System;
using System.Reflection.Metadata.Ecma335;

public partial class AnimationComponent : AnimationPlayer
{
	/// <summary>
	/// For attacks
	/// </summary>
	/// <param name="animName"></param>
	private void UpdateAnimation(string animName)
	{
        if (CurrentAnimation.Contains("die"))
        {
            return;
        }

        if (animName.Contains("die"))
        {
            Play(animName);
            return;
        }

        Play(animName);
    }

    public void UpdateAnimation(Vector2 directionFacing, string animName)
	{
        string direction = "";
        if (directionFacing.X < 0 && directionFacing.Y == 0)
        {
            direction = "left";
        }
        else if (directionFacing.X > 0 && directionFacing.Y == 0)
        {
            direction = "right";
        }
        else if (directionFacing.X == 0 && directionFacing.Y < 0)
        {
            direction = "up";
        }
        else if (directionFacing.X == 0 && directionFacing.Y > 0)
        {
            direction = "down";
        }
        else
        {
            Logger.Log($"No matching animation found {direction}, {animName}.");
            direction = "right";
            animName = "idle";
        }

        var animToPlay = $"{animName}_{direction}";

        if (!GetAnimationList().Contains(animToPlay))
        {
            Logger.LogError($"Missing animation: {animToPlay} on character {GetParent().Name}");
            return;
        }

        UpdateAnimation(animToPlay);
	}
}
