using Godot;
using System;
using System.Linq;
using WizardsVsMonster.scripts.UIScripts;

public partial class ReceieveInput : Control
{
    private Vector2 _pressPos;
    private bool _pressed = false;
    private const float DragThreshold = 10f; // pixels

    public override void _GuiInput(InputEvent @event)
    {
        if (WasInputTap(@event))
        {
            var position = GetInputPosition(@event);
            var inputs = GetChildren();
            foreach (var child in inputs.Cast<IGameInputControlNode>())
            {
                if (child == null) { return; }

                child.InputTap(position);
            }
        }
    }

    private Vector2 GetInputPosition(InputEvent @event)
    {
        if (@event is InputEventScreenTouch touchEvent)
        {
            return touchEvent.Position;
        }

        if (@event is InputEventMouseButton mouseEvent && mouseEvent.IsAction("leftClick"))
        {
            return GetGlobalMousePosition();
        }

        return Vector2.Zero;
    }

    private bool WasInputTap(InputEvent @event)
    {
        // Touch input
        if (@event is InputEventScreenTouch touchEvent)
        {
            if (touchEvent.Pressed)
            {
                _pressPos = touchEvent.Position;
                _pressed = true;
            }
            else if (_pressed) // release
            {
                float dist = _pressPos.DistanceTo(touchEvent.Position);
                _pressed = false;
                return dist < DragThreshold;
            }
        }

        // Mouse input
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.IsAction("leftClick"))
        {
            if (mouseEvent.Pressed)
            {
                _pressPos = mouseEvent.GlobalPosition;
                _pressed = true;
            }
            else if (_pressed) // release
            {
                float dist = _pressPos.DistanceTo(mouseEvent.GlobalPosition);
                _pressed = false;
                return dist < DragThreshold;
            }
        }

        return false;
    }
}
