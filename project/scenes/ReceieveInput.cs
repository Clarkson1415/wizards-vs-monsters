using Godot;
using System.Collections.Generic;
using WizardsVsMonster.scripts;
using WizardsVsMonster.scripts.UIScripts;

/// <summary>
/// Main input control passes to movement and spawn control node. and draws the selection box.
/// </summary>
public partial class ReceieveInput : Control
{
    private Vector2 initialTouchPosition;
    private const float DragThreshold = 10f; // pixels
    [Export] UnitGroupSpawnerControl spawnUnitControl;
    [Export] MovementControls movementControl;

    /// <summary>
    /// If mouse drag selection button down or touch input valid.
    /// For the white rectangle used to mass select units.
    /// </summary>
    private bool wasSelectionButtonJustPressed;

    // TODO have a global option button for mobile to toggle map dragging / unit selection box dragging.

    Rect2 selectionRect;

    private bool isRectActive;

    private Vector2 rectDragStartPosition;

    public override void _GuiInput(InputEvent @event)
    {
        var thisInputsPosition = GetInputPosition(@event);
        var moveAndSpawnControls = new List<IGameInputControlNode>() { spawnUnitControl, movementControl };
        var wasInputTap = WasInputTap(@event);
        var wasInputDrag = IsInputADragMotion(@event);

        if (wasInputTap)
        {
            foreach (var inputNode in moveAndSpawnControls)
            {
                inputNode.OnTapInput(thisInputsPosition);
            }
        }

        if (@event is InputEventScreenTouch && !GlobalGameVariables.TouchScreenDragSetting_DragSelectOn)
        {
            return;
        }

        if (wasSelectionButtonJustPressed && !isRectActive) // Drag started
        {
            Logger.Log("drag started");
            isRectActive = true;
            rectDragStartPosition = thisInputsPosition;
        }
        else if (wasInputDrag && isRectActive)
        {
            Logger.Log($"dragging...pos = {GetInputPosition(@event)}");
            GlobalCurrentSelection.GetInstance().DragSelectionUpdate(selectionRect);
            float topLeftX = Mathf.Min(rectDragStartPosition.X, thisInputsPosition.X);
            float topLeftY = Mathf.Min(rectDragStartPosition.Y, thisInputsPosition.Y);

            // 2. Calculate the width and height (max - min)
            float width = Mathf.Max(rectDragStartPosition.X, thisInputsPosition.X) - topLeftX;
            float height = Mathf.Max(rectDragStartPosition.Y, thisInputsPosition.Y) - topLeftY;

            selectionRect = new Rect2(topLeftX, topLeftY, width, height);
            QueueRedraw();
        }

        if (IsDragButtonReleased(@event))
        {
            isRectActive = false;
            Logger.Log("drag released.");
            //GlobalCurrentSelection.OnDragReleased();
            //movementControl.OnDragReleased();
            wasSelectionButtonJustPressed = false;
            QueueRedraw();
        }
    }

    private Color selectionRectFill = new Color("#ffffff66");
    private Color selectionRectBoarder = new Color("#ffffff");
    private float boarderThickness = 2f;

    public override void _Draw()
    {
        if (!wasSelectionButtonJustPressed || !isRectActive)
        {
            return;
        }

        DrawRect(selectionRect, selectionRectFill);
        DrawRect(selectionRect, selectionRectBoarder, false, boarderThickness);
    }

    /// <summary>
    /// return true if drag button usually left click. is not held down. or if touch screen is not touched.
    /// </summary>
    /// <param name="event"></param>
    /// <returns></returns>
    private bool IsDragButtonReleased(InputEvent @event)
    {
        // Touch input
        if (@event is InputEventScreenTouch touchEvent)
        {
            return touchEvent.IsReleased();
        }

        // Mouse input
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.IsAction("leftClick"))
        {
            return mouseEvent.IsReleased();
        }

        return false;
    }

    private bool IsInputADragMotion(InputEvent @event)
    {
        var dist = 0f;

        if (@event is InputEventScreenTouch touchEvent)
        {
            dist = initialTouchPosition.DistanceTo(touchEvent.Position);
            
        }
        else if (@event is InputEventMouseMotion mouseEvent)
        {
            dist = initialTouchPosition.DistanceTo(mouseEvent.GlobalPosition);
        }

        return dist >= DragThreshold;
    }

    private Vector2 GetInputPosition(InputEvent @event)
    {
        if (@event is InputEventScreenTouch touchEvent)
        {
            return touchEvent.Position;
        }
        else if (@event is InputEventMouseButton mouseEvent && mouseEvent.IsAction("leftClick"))
        {
            return GetGlobalMousePosition();
        }
        else if (@event is InputEventMouseMotion mouseMotion)
        {
            return GetGlobalMousePosition();
        }

        return Vector2.Zero;
    }

    /// <summary>
    /// Also updates _pressPos and isSelctionButtonDown.
    /// </summary>
    /// <param name="event"></param>
    /// <returns></returns>
    private bool WasInputTap(InputEvent @event)
    {
        // Touch input
        if (@event is InputEventScreenTouch touchEvent)
        {
            if (touchEvent.Pressed)
            {
                initialTouchPosition = touchEvent.Position;
                wasSelectionButtonJustPressed = true;
            }
            else if (!touchEvent.Pressed) // release
            {
                float dist = initialTouchPosition.DistanceTo(touchEvent.Position);
                wasSelectionButtonJustPressed = false;
                return dist < DragThreshold;
            }
        }

        // Mouse input
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.IsAction("leftClick"))
        {
            if (mouseEvent.Pressed)
            {
                initialTouchPosition = mouseEvent.GlobalPosition;
                wasSelectionButtonJustPressed = true;
                Logger.Log("pressed");
            }
            else if (mouseEvent.IsReleased()) // release
            {
                float dist = initialTouchPosition.DistanceTo(mouseEvent.GlobalPosition);
                wasSelectionButtonJustPressed = false;
                var wasTap = dist < DragThreshold;
                Logger.Log($"released, was tap = {wasTap}");
                return wasTap;
            }
        }

        return false;
    }
}
