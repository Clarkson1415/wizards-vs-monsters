using Godot;

/// <summary>
/// Shines a square light on the board of the area this unit effects. Tower buff or unit attack area.
/// </summary>
[GlobalClass]
public partial class UnitIndicator : PointLight2D
{
	private int unitSquareSize;

    public void SetupLight(int unitSquareSize, bool isBlue)
    {
        base._Ready();

        this.unitSquareSize = unitSquareSize;

        var texture = this.Texture as GradientTexture2D;

        // set gradient to a white square
        texture.Gradient.RemovePoint(0);
        texture.Gradient.SetColor(0, Godot.Color.Color8(255, 255, 255, 255));

        // set size
        texture.Width = unitSquareSize;
        texture.Height = unitSquareSize;

        // set color blue else red
        var color = isBlue ? Godot.Color.Color8(0, 90, 233) : Godot.Color.Color8(165, 12, 20);
        this.Color = color;

        this.Energy = 0.5f;

        // set light to only target the tilemap layer
        this.RangeZMin = -5;
        this.RangeZMax = -5;

        this.Visible = false;
    }
}
