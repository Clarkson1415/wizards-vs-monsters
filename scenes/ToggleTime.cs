using Godot;
using System;

public partial class ToggleTime : Button
{
    private bool isPlaying = true;

    public override void _Pressed()
    {
        base._Pressed();

        Engine.TimeScale = isPlaying ? 0 : 1;
        isPlaying = !isPlaying;
    }
}
