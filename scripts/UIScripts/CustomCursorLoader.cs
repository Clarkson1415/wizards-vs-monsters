using Godot;
using System;

public partial class CustomCursorLoader : Node
{
    [Export] Resource mouseTexture;

    public override void _Ready()
    {
        var sword = ResourceLoader.Load(mouseTexture.ResourcePath);

        // Changes only the arrow shape of the cursor.
        // This is similar to changing it in the project settings.
        Input.SetCustomMouseCursor(sword);


        // Changes a specific shape of the cursor (here, the I-beam shape).
        // like the typeing I
        // Input.SetCustomMouseCursor(beam, Input.CursorShape.Ibeam);
    }

}
