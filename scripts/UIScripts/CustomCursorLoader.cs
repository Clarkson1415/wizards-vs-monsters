using Godot;
using System;

public partial class CustomCursorLoader : Node
{
    [Export] Resource defaultTexture;

    [Export] Resource swordTexture;

    /// <summary>
    /// Appears when one or more unit groups are selected. and the cursor is NOT over an enemy target. is like a movement icon.
    /// </summary>
    [Export] Resource moveSelectedTexture;

    private void ChangeToSword()
    {
        var sword = ResourceLoader.Load(swordTexture.ResourcePath);

        // Changes only the arrow shape of the cursor.
        // This is similar to changing it in the project settings.
        Input.SetCustomMouseCursor(sword);
    }

    public override void _Ready()
    {
        var sword = ResourceLoader.Load(defaultTexture.ResourcePath);
        Input.SetCustomMouseCursor(sword);

        // Changes a specific shape of the cursor (here, the I-beam shape).
        // like the typeing I
        // Input.SetCustomMouseCursor(beam, Input.CursorShape.Ibeam);
    }
}
