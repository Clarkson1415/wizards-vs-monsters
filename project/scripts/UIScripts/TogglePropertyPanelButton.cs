using Godot;
using System;

public partial class TogglePropertyPanelButton : Button
{
    /// <summary>
    /// If the panel is slid off-screen and the icon is '<' or if its true its on screen and the icon is '>'
    /// </summary>
    public bool PropertyPanelOpen { get; private set; } = true;

    [Export] public AnimationPlayer propertyPanelAnimator;

    public override void _Pressed()
    {
        PropertyPanelOpen = !PropertyPanelOpen;

        if (PropertyPanelOpen)
        {
            propertyPanelAnimator.Play("SlideIn");
        }
        else
        {
            propertyPanelAnimator.Play("SlideOut");
        }
    }
}
