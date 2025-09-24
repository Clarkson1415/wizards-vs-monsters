using Godot;
using System.Collections.Generic;

public partial class CameraController : Camera2D
{
    private static int zoomAmount = 10;

    private static float moveSpeed = 50;

    private bool mouseCurrentlyDragging = false;
    private Vector2 mouse_start_pos;
    private Vector2 screen_start_position;
    private Dictionary<long, Vector2> _touches = new();
    private float _lastDistance = 0f;

    public override void _Process(double delta)
    {
        var floatDelta = (float)delta;
        processWasdMovement(floatDelta);
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        // mouse drag
        handleMousePan(@event);
        handleMouseScroolWheelZoom(@event);
        handleTouchInput(@event);
    }

    private void handleTouchInput(InputEvent @event)
    {
        // dragging
        if (@event is InputEventScreenDrag fingerDrag) // mobile drag
        {
            Vector2 fingerDeltaScreen = -fingerDrag.Relative;
            panAny(fingerDeltaScreen);
        }

        // Zooming
        // mobile
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
    }

    private void handleMousePan(InputEvent @event)
    {
        if (@event.IsAction("drag"))
        {
            if (@event.IsPressed())
            {
                mouse_start_pos = GetGlobalMousePosition();
                screen_start_position = GlobalPosition;
                mouseCurrentlyDragging = true;
            }
            else
            {
                mouseCurrentlyDragging = false;
            }
        }
        else if (@event is InputEventMouseMotion mouseMotion && mouseCurrentlyDragging)
        {
            Vector2 mouseDeltaScreen = -mouseMotion.Relative;
            panAny(mouseDeltaScreen);
        }
    }

    private void handleMouseScroolWheelZoom(InputEvent @event)
    {
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

    private void panAny(Vector2 screenChange)
    {
        GlobalPosition += screenChange / Zoom;
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
