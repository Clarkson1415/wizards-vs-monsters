using Godot;
using System;
using System.Reflection.Emit;
using WizardsVsMonster.scripts;

public partial class GaeaWrapper : Node
{
    [Export] private Node GaeaGenerator { get; set; }

    [Export] private TileMapLayer layerWithTrees;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        GaeaGenerator.Call("generate");

        // wait for that to finish?
        CallDeferred(nameof(PutArea2dsOnTrees));
    }

    private void PutArea2dsOnTrees()
    {
        // area2d on tiles that have a tile on them.
        foreach(var cell in layerWithTrees.GetUsedCells())
        {
            // add area2d of the tilemap cell size on the tile and put it on layer.
            var newArea2d = new Area2D();

            // tilemap coordinate to global position how?
            var cellCoordinates = new Vector2(cell.X, cell.Y);
            newArea2d.GlobalPosition = layerWithTrees.GlobalPosition + (cellCoordinates * layerWithTrees.TileSet.TileSize) + (layerWithTrees.TileSet.TileSize / 2);

            newArea2d.CollisionLayer = 0;
            newArea2d.SetCollisionLayerValue(GlobalGameVariables.ViewBlockingAreaCollisionLayer, true);

            var collisionShape = new CollisionShape2D();
            var rect = new RectangleShape2D();
            rect.Size = layerWithTrees.TileSet.TileSize + (layerWithTrees.TileSet.TileSize / 2);
            collisionShape.Shape = rect;

            newArea2d.AddChild(collisionShape);
            AddChild(newArea2d);
        }
    }
}
