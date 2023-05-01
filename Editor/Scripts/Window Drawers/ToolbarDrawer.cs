using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
/// <summary>
/// A class for drawing the toolbar while making use of the swatch object placer
/// </summary>
public class ToolbarDrawer
{
    /// <summary>
    /// The prefab swatch window
    /// </summary>
    public SwatchWindow prefabSwatchWindow;

    /// <summary> The current rect for the toolbar </summary>
    private Rect toolbarRect;

    /// <summary> The sprite to define the toolbar button UI </summary>
    private Sprite buttonUI;

    /// <summary> The color of the outter background </summary>
    private Color backgroundToolbarColor = new Color(0.2352941f, 0.2352941f, 0.2352941f, 1);

    /// <summary> The color for text related input </summary>
    private Color fontColor = new Color(0.6901961f, 0.6901961f, 0.6901961f, 1);

    public ToolbarDrawer(SwatchWindow prefabSwatchWindow) => this.prefabSwatchWindow = prefabSwatchWindow;

    /// <summary>
    /// Draw the toolbar on the scene
    /// </summary>
    public void DrawToolbar(SceneView view)
    {
        this.DrawToolbarBackground(view);
        this.DrawToolbarPrefabPosition(view);
        this.DrawToolbarButtons(view);
    }

    /// <summary>
    /// Draw the toolbar background
    /// <param name="view">The current scene view</param>
    /// </summary>
    private void DrawToolbarBackground(SceneView view)
    {
        Handles.BeginGUI();

        int toolBarBackground = 32;
        this.toolbarRect = new Rect(0, view.position.height - 21 - toolBarBackground, view.position.width, toolBarBackground);

        GUILayout.BeginArea(new Rect(0, 0, view.position.width, view.position.height));
        SwatchEditorHelper.DrawRect(this.toolbarRect, this.backgroundToolbarColor);
        SwatchEditorHelper.DrawRect(new Rect(this.toolbarRect.x, this.toolbarRect.y, this.toolbarRect.width, 1), this.backgroundToolbarColor);

        GUI.backgroundColor = this.backgroundToolbarColor;
        GUILayout.EndArea();

        Handles.EndGUI();
    }

    /// <summary>
    /// Draw the positions element on the tool bar
    /// <param name="view">The current scene view </param>
    /// </summary>
    private void DrawToolbarPrefabPosition(SceneView view)
    {
        Handles.BeginGUI();

        int fontSize = 16;
        string position = "<< 0, 0 >>";

        if (this.prefabSwatchWindow.GetDrawObjectService().GetObjectToPlace() != null)
        {
            Vector2 objectToPlacePos = this.prefabSwatchWindow.GetDrawObjectService().GetObjectToPlace().gameObject.transform.position;
            position = "<< " + objectToPlacePos.x + ", " + objectToPlacePos.y + " >>";
        }

        GUILayout.BeginArea(new Rect(4f, 4f + fontSize, view.position.width, view.position.height));
        GUIStyle positionLabelStyle = new GUIStyle
        {
            font = AssetDatabase.LoadAssetAtPath<Font>("../Assets/Fonts/Debug-Font.ttf"),
            fontSize = fontSize
        };

        positionLabelStyle.normal.textColor = this.fontColor;

        Rect rect = new Rect(4, view.position.height - (56 - 4) - fontSize, fontSize, fontSize);
        GUI.Label(new Rect(rect.x, rect.y, view.position.width, rect.height), position, positionLabelStyle);
        GUILayout.EndArea();

        Handles.EndGUI();
    }

    /// <summary>
    /// Draws the editable buttons
    /// <param name="view">The current scene view </param>
    /// </summary/>
    private void DrawToolbarButtons(SceneView view)
    {
        Handles.BeginGUI();

        int buttonWidth = 32;
        int buttonHeight = 24;

        GUILayout.BeginArea(new Rect(0f, 4f + buttonHeight, view.position.width, view.position.height));
        Rect rect = new Rect(view.position.width, view.position.height - 54 - buttonHeight, buttonWidth, buttonHeight);

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 15
        };

        buttonStyle.normal.textColor = this.fontColor;
        buttonStyle.alignment = TextAnchor.MiddleCenter;
        foreach (ToolbarButton toolbarButton in this.GetToolbarButtons())
        {
            GUI.backgroundColor = Color.white;
            rect.x -= buttonWidth + 4;

            if (GUI.Button(rect, new GUIContent(toolbarButton.text, "'" + toolbarButton.keyCode + "'- " + toolbarButton.toolTip), buttonStyle))
            {
                toolbarButton.action();
            }

            int control = GUIUtility.GetControlID(FocusType.Passive);
        }

        GUI.enabled = true;

        GUILayout.EndArea();

        Handles.EndGUI();
    }

    /// <summary>
    /// Fetch the buttons used in the toolbar
    /// </summary>
    public new List<ToolbarButton> GetToolbarButtons()
    {
        this.buttonUI = AssetDatabase.LoadAssetAtPath<Sprite>("../Assets/Buttons/Primary_Button.png");

        return new List<ToolbarButton>
        {
            new ToolbarButton("\u2716", () => this.prefabSwatchWindow.GetDrawObjectService().RemoveObjectToPlace(), "Ends the editing mode of the prefab swatch", KeyCode.Escape),
            new ToolbarButton("\u21BA", () => this.prefabSwatchWindow.GetDrawObjectService().AddRotation(45), "Rotates the selected object by 45 degrees", KeyCode.J),
            new ToolbarButton("\u21BB", () => this.prefabSwatchWindow.GetDrawObjectService().AddRotation(-45), "Rotates the selected object by -45 degrees", KeyCode.K),
            new ToolbarButton("\u2750", () => this.prefabSwatchWindow.GetDrawObjectService().MoveRenderLayer(1), "Increases the render layer of all sprites on the selected object by 1", KeyCode.L),
            new ToolbarButton("\u274F", () => this.prefabSwatchWindow.GetDrawObjectService().MoveRenderLayer(-1), "Reduces the render layer of all sprites on the selected object by 1", KeyCode.Semicolon),
            new ToolbarButton("\u2195", () => this.prefabSwatchWindow.GetDrawObjectService().FlipSelectedObject(false), "Flip the selected object horizontally", KeyCode.N),
            new ToolbarButton("\u2194", () => this.prefabSwatchWindow.GetDrawObjectService().FlipSelectedObject(true), "Flip the selected object verticaly", KeyCode.M)
        };
    }

}
