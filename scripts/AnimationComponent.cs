using Godot;
using System;
using System.Reflection.Metadata.Ecma335;

public partial class AnimationComponent : AnimationPlayer
{
	/// <summary>
	/// For attacks
	/// </summary>
	/// <param name="animName"></param>
	public void UpdateAnimation(string animName)
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

        if (CurrentAnimation == "hurt" && IsPlaying())
        {
            // wait till hurt animation is finished.
            return;
        }

        Play(animName);
    }

    public void UpdateAnimation(Vector2 velocity)
	{
		if (velocity.X < 0 && velocity.Y == 0)
		{
            UpdateAnimation("walk_left");
		}
		else if (velocity == Vector2.Zero)
        {
            UpdateAnimation("idle");
		}

		// TODO more idle directions based on facing direction I guess.
	}

}
