using System;
using UnityEngine;

/// <summary>
///Contains important deta pertaining to the tol bar
/// </summary>
public class ToolbarButton
{
    /// <summary> The text placed in the middle of the button</summary>
    public string text;
    /// <summary> The action performed when the button is activated</summary>
    public Action action;
    /// <summary> The keycode of the action which also serves as a shortcut </summary>
    public KeyCode keyCode;
    /// <summary> The tooltip of the button </summary>
    public string toolTip;

    public ToolbarButton(string text, Action action, string toolTip = "", KeyCode shortcut = KeyCode.None)
    {
        this.text = text;
        this.action = action;
        this.keyCode = shortcut;
        this.toolTip = toolTip;
    }
}
