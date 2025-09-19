using Godot;
using System.Collections.Generic;

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

    private Dictionary<long, Vector2> _touches = new();
    private float _lastDistance = 0f;

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (controlSceme == CONTROL_SCHEME.MOBILE)
        {

            if (@event is InputEventScreenDrag dragEvent)
            {
                mobilePan(dragEvent);
            }

            // handle zoom 

            if (@event is InputEventScreenTouch touchEvent)
            {
                if (touchEvent.Pressed)
                {
                    // Finger down → store position
                    _touches[touchEvent.Index] = touchEvent.Position;
                }
                else
                {
                    // Finger up → remove
                    _touches.Remove(touchEvent.Index);
                    _lastDistance = 0f;
                }
            }
            else if (@event is InputEventScreenDrag anotherDRAG)
            {
                // Update finger position while dragging
                _touches[anotherDRAG.Index] = anotherDRAG.Position;

                if (_touches.Count == 2)
                {
                    // Get the two touches
                    var enumerator = _touches.Values.GetEnumerator();
                    enumerator.MoveNext();
                    Vector2 p1 = enumerator.Current;
                    enumerator.MoveNext();
                    Vector2 p2 = enumerator.Current;

                    float currentDistance = p1.DistanceTo(p2);

                    if (_lastDistance > 0f)
                    {
                        float zoomChange = currentDistance / _lastDistance;

                        Zoom *= zoomChange;

                        // Clamp zoom
                        Zoom = Zoom.Clamp(new Vector2(0.1f, 0.1f), new Vector2(20f, 20f));
                    }

                    _lastDistance = currentDistance;
                }
            }

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

    private void mobilePan(InputEventScreenDrag dragEvent)
    {
        // How much the finger moved in screen pixels
        Vector2 fingerDeltaScreen = -dragEvent.Relative;

        // Convert to world units and move camera
        GlobalPosition += fingerDeltaScreen / Zoom;
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
