using Godot;
using System;
using System.Diagnostics;

public partial class CameraController : Camera2D
{
    private static int zoomAmount = 10;

    private static float moveSpeed = 50;

    private enum CONTROL_SCHEME
    {
        KEYBOARD_WASD,
        KEYBOARD_CLICKDRAG,
        MOBILE
    }

    [Export] private CONTROL_SCHEME controlSceme;

    private bool dragging = false;

    private Vector2 dragStart;

    public override void _Process(double delta)
    {
        var floatDelta = (float)delta;

        if (controlSceme == CONTROL_SCHEME.KEYBOARD_WASD)
        {
            processWasdMovement(floatDelta);
        }
        
    }

    private Vector2 mouse_start_pos;
    private Vector2 screen_start_position;

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (controlSceme == CONTROL_SCHEME.MOBILE)
        {
            // TODO
            return;
        }

        if (controlSceme == CONTROL_SCHEME.KEYBOARD_CLICKDRAG)
        {
            if (@event.IsAction("drag"))
            {
                if (@event.IsPressed())
                {
                    mouse_start_pos = GetGlobalMousePosition();
                    screen_start_position = GlobalPosition;
                    dragging = true;
                }
                else
                {
                    dragging = false;
                }
            }
            else if (@event is InputEventMouseMotion && dragging)
            {
                pan(@event);
            }
        }

        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.WheelUp)
            {
                zoomIn();
            }
            else if (mouseEvent.ButtonIndex == MouseButton.WheelDown)
            {
                zoomOut();
            }
        }
    }

    private void pan(InputEvent @event)
    {
        // How much the mouse moved in screen pixels
        Vector2 mouseDeltaScreen = -((InputEventMouseMotion)@event).Relative;

        // Convert to world units and move camera
        GlobalPosition += mouseDeltaScreen / Zoom;
    }

    private void zoomIn()
	{
        this.Zoom = this.Zoom + (Vector2.One / zoomAmount);
    }


    private void processWasdMovement(float floatDelta)
    {
        var w = Input.IsKeyPressed(Key.W);
        var a = Input.IsKeyPressed(Key.A);
        var s = Input.IsKeyPressed(Key.S);
        var d = Input.IsKeyPressed(Key.D);

        if (w)
        {
            moveUp(floatDelta);
        }
        if (a)
        {
            moveLeft(floatDelta);
        }
        if (d)
        {
            moveRight(floatDelta);
        }
        if (s)
        {
            moveDown(floatDelta);
        }
    }


    #region WASD movement
    private void zoomOut()
	{
        this.Zoom = this.Zoom - (Vector2.One / zoomAmount);
    }

    private void moveUp(float delta)
    {
        GlobalPosition += Vector2.Up * moveSpeed * delta;
    }

    private void moveDown(float delta)
    {
        GlobalPosition += Vector2.Down * moveSpeed * delta;
    }

    private void moveLeft(float delta)
    {
        GlobalPosition += Vector2.Left * moveSpeed * delta;
    }

    private void moveRight(float delta)
    {
        GlobalPosition += Vector2.Right * moveSpeed * delta;
    }
    #endregion
}
