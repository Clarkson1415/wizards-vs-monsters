using Godot;
using System;

// [Tool] enables the script to run in the editor for preview purposes.
public partial class changing_shadow : ColorRect
{
    [Export] AnimatedSprite2D animatedSprite;

    // Variables to track the sprite state for editor updates
    private string lastAnimation = "";
    private int lastFrame = -1;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (animatedSprite == null)
        {
            // Assuming Logger is defined elsewhere, or use GD.PrintErr
            GD.PrintErr("AnimatedSprite2D not assigned to changing_shadow script.");
            return;
        }

        // Only connect the signal if the game is running, not in the editor
        animatedSprite.FrameChanged += OnFrameChanged;

        // Initial call to set up the shadow
        OnFrameChanged();
    }

    private void OnFrameChanged()
    {
        if (Material is ShaderMaterial shadowMaterial)
        {
            SpriteFrames framesResource = animatedSprite.SpriteFrames;
            var animName = animatedSprite.Animation;
            var frameIndex = animatedSprite.Frame;

            // Note: GetFrameTexture returns a specific texture resource for that frame
            var frameTexture = framesResource.GetFrameTexture(animName, frameIndex);

            if (frameTexture == null)
            {
                // This can happen if the animation is empty or not set up yet
                return;
            }

            // Assume the frame's region is the full size initially
            Rect2 frameRect = new Rect2(Vector2.Zero, frameTexture.GetSize());
            Texture2D baseTexture = frameTexture; // Start with the frame texture as base

            // Check if it's an AtlasTexture (part of a spritesheet)
            if (frameTexture is AtlasTexture atlasTexture)
            {
                // If so, the Region holds the frame's coords on the sheet
                frameRect = atlasTexture.Region;

                // The Atlas property holds the full sprite sheet texture
                baseTexture = atlasTexture.Atlas;
            }

            // --- CRITICAL FIX: Calculate sheetSize from the baseTexture ---
            Vector2 sheetSize = baseTexture.GetSize();

            // --- 2. Calculate UV Coordinates ---
            // FRAME_UV_START: Frame's top-left position (pixels) / Sheet's total size
            Vector2 uvStart = frameRect.Position / sheetSize;

            // FRAME_UV_SIZE: Frame's pixel size / Sheet's total size
            Vector2 uvSize = frameRect.Size / sheetSize;

            // TEXEL_SIZE: 1 / Sheet's total size
            Vector2 texelSize = new Vector2(1.0f / sheetSize.X, 1.0f / sheetSize.Y);

            // Pass the full sprite sheet texture
            shadowMaterial.SetShaderParameter("sprite_Texture", baseTexture);

            // Pass the calculated UV parameters
            shadowMaterial.SetShaderParameter("FRAME_UV_START", uvStart);
            shadowMaterial.SetShaderParameter("FRAME_UV_SIZE", uvSize);
            shadowMaterial.SetShaderParameter("TEXEL_SIZE", texelSize);

            // This ensures the ColorRect bounding box matches the visible frame
            CustomMinimumSize = frameRect.Size;
            Size = frameRect.Size;
        }
    }
}