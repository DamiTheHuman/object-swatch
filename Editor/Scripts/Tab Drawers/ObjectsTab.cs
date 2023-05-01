using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ObjectsTab<T> : SwatchTab
{
    protected string tag = "Object";

    protected ObjectAssetsService<T> objectAssetsService;

    /// <summary> The genericObject selected by the user being hovered around the scene view about to be placed</summary>
    protected GameObject objectToPlaceInScene;

    /// <summary> The sorting layer for the genericObject being placed</summary>
    protected int objectSortingLayerIndex = 3;

    /// <summary> The genericObject selected by the user</summary>
    protected SwatchData<T> selectedGenericObject = new SwatchData<T>();

    /// <summary> The genericObject to bed drawn on scene selected by the user </summary>
    protected SwatchData<T> objectToDraw = new SwatchData<T>();

    /// <summary> The scroll view for the list of sprites used for displaying a lengthy list</summary>
    protected Vector2 objectsScrollView = new Vector2(0, 0);

    /// <summary> The zoom of active sprites within the scene</summary>
    protected float listPreviewZoom = 4f;

    /// <summary> Padding for the scroll bar to make it fit nicely</summary>
    protected float scrollBarPadding = 70;

    /// <summary> The margin between each button item</summary>
    protected Vector2 margin = new Vector2(4, 4);

    /// <summary> Show Secondary swatch</summary>
    private bool squashSecondarySwatches = true;

    /// <summary> The current values in in the tab</summary>
    private List<SwatchData<T>> values = new List<SwatchData<T>>();

    public ObjectsTab(SwatchWindow spriteSwatchWindow) => this.swatchWindow = spriteSwatchWindow;

    /// <inheritdoc>
    /// <see cref="SwatchTab"/>
    /// </inheritdoc>
    public override void Reset()
    {
        this.objectAssetsService.SetSwatchIndex(0);
        this.objectAssetsService.SetSwatchSubIndex(0);
        this.objectAssetsService.FetchTabData();
        this.UpdateValues();
    }

    /// <summary>
    /// Sets the object asset service
    /// <param name="objectAssetsService"> The new object assets service </param>
    /// </summary>
    public void SetObjectAssetsService(ObjectAssetsService<T> objectAssetsService) => this.objectAssetsService = objectAssetsService;

    /// <inheritdoc>
    /// <see cref="SwatchTab"/>
    /// </inheritdoc>
    public override void DrawTabTemplate(Vector2 mousePosition)
    {
        if (this.swatchWindow.GetLoading())
        {
            EditorGUILayout.HelpBox("Attempting to load " + this.tag + "s...", MessageType.Info);

            return;
        }
        else if (this.objectAssetsService.GetSwatchList().Count == 0 || this.objectAssetsService.HasNoObjects())
        {
            this.DrawUpdateButton();
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("The recommended structure to place " + this.tag + "(s) is Assets/Resources/[SUB_DIRECTORIES]/[PRIMARY_SWATCH]/[SECONDARY_SWATCH]/" + this.tag + "s", MessageType.Info);

            this.swatchWindow.SetAssetsDirectory(EditorGUILayout.TextField("Assets Directory", this.swatchWindow.GetAssetsDirectory()));
            this.UpdateAndDrawSubDirectoryNames();
            string error = "The Project has no " + this.tag + "(s) following the schema  Assets/Resources/[SUB_DIRECTORIES]/[PRIMARY_SWATCH]/[SECONDARY_SWATCH]/!";
            EditorGUILayout.HelpBox(error, MessageType.Error);

            return;
        }

        base.DrawTabTemplate(mousePosition);

        if (AssetPreview.IsLoadingAssetPreviews())
        {
            this.swatchWindow.Repaint();
        }
    }

    /// <inheritdoc>
    /// <see cref="SwatchTab"/>
    /// </inheritdoc>
    protected override void DrawTabHeader(Vector2 mousePosition)
    {
        GUI.enabled = this.objectToDraw != null;

        if (GUILayout.Button(new GUIContent("Start Placing " + this.tag, " Start Placing " + this.tag)))
        {
            this.OnSelectObject();
        }

        GUI.enabled = true;

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("The recommended structure to place" + this.tag + " is Assets/GameObjects/Resources/[GameObjectName]/" + this.tag + "s", MessageType.Info);

        this.swatchWindow.SetAssetsDirectory(EditorGUILayout.TextField("Assets Directory", this.swatchWindow.GetAssetsDirectory()));
        this.UpdateAndDrawSubDirectoryNames();

        EditorGUILayout.Space();
        GUILayout.Box("", this.swatchWindow.RenderSeperator(), GUILayout.ExpandWidth(true), GUILayout.Height(1));

        if (this.swatchWindow.GetPreviousAssetsDirectory() != this.swatchWindow.GetAssetsDirectory())
        {
            this.swatchWindow.SetPreviousAssetsDirectory(this.swatchWindow.GetAssetsDirectory());
            this.objectAssetsService.FetchTabData();
            this.UpdateValues();
        }
    }

    /// <inheritdoc>
    /// <see cref="SwatchTab"/>
    /// </inheritdoc>
    protected override void DrawTabFooter(Vector2 mousePosition)
    {
        GUILayout.Box("", this.swatchWindow.RenderSeperator(), GUILayout.ExpandWidth(true), GUILayout.Height(1));
        EditorGUILayout.BeginHorizontal();
        GUI.enabled = this.objectToDraw != null;

        if (GUILayout.Button(new GUIContent("Stop Editing", "Stop placing" + this.tag + "(ESC Key)")))
        {
            this.swatchWindow.GetDrawObjectService().RemoveObjectToPlace();
        }

        GUI.enabled = true;

        this.listPreviewZoom = EditorGUILayout.Slider(this.listPreviewZoom, 1f, 7f); // Set the zoom of the app
        EditorGUILayout.EndHorizontal();
        this.swatchWindow.DrawBreakPoint();
    }

    /// <inheritdoc>
    /// <see cref="SwatchTab"/>
    /// </inheritdoc>
    protected override void DrawTabFilter(Vector2 mousePosition)
    {
        GUI.enabled = true;
        this.objectAssetsService.InitializeSpriteSwatchData();
        this.UpdateAndDrawSwatchNames();
        this.ClampSwatchIndexes();
        this.objectAssetsService.SetSearchFilter(EditorGUILayout.TextField(this.tag + " List Filter", this.objectAssetsService.GetSearchFilter().Trim()));

        if (this.values.Count == 0)
        {
            this.UpdateValues();
        }

        this.DrawUpdateButton();
        this.DrawToggleSecondarySwatchButton();
        this.swatchWindow.DrawBreakPoint(1);
        GUILayout.Box("", this.swatchWindow.RenderSeperator(), GUILayout.ExpandWidth(true), GUILayout.Height(1));

        this.swatchWindow.DrawBreakPoint(2);
    }

    /// <inheritdoc>
    /// <see cref="SwatchTab"/>
    /// </inheritdoc>
    protected override void DrawTabBody(Vector2 mousePosition)
    {
        if (this.values.Count <= 0)
        {
            string errMessage = this.objectAssetsService.GetSearchFilter() == "" ? "The selected Swatch doesn't contain any Sprites " : "The selected Swatch doesn't contain any Sprites with the name '" + this.objectAssetsService.GetSearchFilter() + "'";
            EditorGUILayout.HelpBox(errMessage, MessageType.Warning);

            return;
        }

        Vector2 itemPosition = new Vector2(0, 0);
        int columnsPerRow = Mathf.RoundToInt(this.swatchWindow.position.width / (this.GetThumbnailSize() + (this.margin.x * 2)));
        int numRows = (this.values.Count / columnsPerRow) + 1;
        this.objectsScrollView = EditorGUILayout.BeginScrollView(this.objectsScrollView, false, false, GUILayout.Width(this.swatchWindow.position.width), GUILayout.MinHeight(0), GUILayout.MaxHeight(800), GUILayout.ExpandHeight(true));

        Vector2 mousePositionWithinTab = mousePosition;
        mousePositionWithinTab.x -= this.lastHeaderRect.xMin - this.objectsScrollView.x;
        mousePositionWithinTab.y -= this.lastHeaderRect.yMax - this.objectsScrollView.y;
        bool horizontalLayoutIsClosed = false;

        EditorGUILayout.BeginHorizontal();
        GUILayout.Box("", GUILayout.Width(this.margin.x));

        foreach (SwatchData<T> genericObject in this.values)
        {
            if (genericObject.genericObject == null)
            {
                continue;
            }

            if (itemPosition.x == 0 && horizontalLayoutIsClosed)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Box("", GUILayout.Width(this.margin.x));

                horizontalLayoutIsClosed = false;
            }

            this.DrawObjectPreview(genericObject, mousePositionWithinTab, itemPosition);

            itemPosition.x++;

            if (itemPosition.x >= columnsPerRow)
            {
                numRows++;
                itemPosition.y++;
                itemPosition.x = 0;
                GUILayout.Box("", GUILayout.Width(this.margin.x));

                EditorGUILayout.EndHorizontal();

                horizontalLayoutIsClosed = true;
            }
        }

        if (horizontalLayoutIsClosed == false)
        {
            GUILayout.Box("", GUILayout.Width(this.margin.x));
            EditorGUILayout.EndHorizontal();
        }

        this.swatchWindow.DrawBreakPoint();
        EditorGUILayout.EndScrollView();
        this.swatchWindow.DrawBreakPoint();
        GUI.enabled = true;
    }

    /// <summary>
    /// Draw the thumbnail preview of a genericObject and handle its interaction with the cursor/mous
    /// <param name="genericObject"> The genericObject to be drawn</param>
    /// <param name="mousePositionWithinTab"> The position of the mouse within the tab</param>
    /// <param name="itemPosition"> The current position to draw the genericObject within the window starting from (0,0)</param>
    /// </summary>
    protected Rect DrawObjectPreview(SwatchData<T> genericObject, Vector2 mousePositionWithinTab, Vector2 itemPosition)
    {
        Rect rect = new Rect();
        rect.x += (this.GetThumbnailSize() * itemPosition.x) + (this.margin.x * itemPosition.x);
        rect.y = (this.GetThumbnailSize() * itemPosition.y) + (this.margin.x * itemPosition.y);
        rect.width = this.GetThumbnailSize();
        rect.height = this.GetThumbnailSize();

        Rect buttonRect = this.DrawObjectButton(rect, genericObject);
        this.DrawObjectThumbnail(buttonRect, genericObject);
        this.DrawObjectHighlight(buttonRect, genericObject, buttonRect.Contains(mousePositionWithinTab));

        return rect;
    }

    /// <summary>
    /// Draw the update button and refreshes the genericObject swatch on update
    /// </summary>
    public void DrawUpdateButton()
    {
        if (GUILayout.Button(new GUIContent("Update " + this.tag + " List", "Updates the list of " + this.tag + "s from the resource folder")))
        {
            this.objectAssetsService.FetchTabData();
        }
    }


    /// <summary>
    /// Draw the toggle secondary swatch button which allows users to show the secondary swatch option or squash them
    /// </summary>
    private void DrawToggleSecondarySwatchButton()
    {
        if (GUILayout.Button(new GUIContent((this.squashSecondarySwatches ? "Show" : "Squash") + " Secondary Swatches", "Toggle Secondary Swatches")))
        {
            this.squashSecondarySwatches = !this.squashSecondarySwatches;
        }
    }

    /// <summary>
    /// Clamps the current swatch index and sub index based on the current swatch being watched
    /// </summary/>
    protected void ClampSwatchIndexes()
    {
        if (this.objectAssetsService.GetSwatchIndex() > 0)
        {
            this.objectAssetsService.SetSwatchIndex(Mathf.Clamp(this.objectAssetsService.GetSwatchIndex(), 0, this.objectAssetsService.GetSwatchList().Count - 1));
        }
        else
        {
            this.objectAssetsService.SetSwatchIndex(0);
        }

        if (this.objectAssetsService.GetSwatchSubNames().Length > 0)
        {
            this.objectAssetsService.SetSwatchSubIndex(Mathf.Clamp(this.objectAssetsService.GetSwatchSubIndex(), 0, this.objectAssetsService.GetSwatchSubNames().Length - 1));
        }
        else
        {
            this.objectAssetsService.SetSwatchSubIndex(0);
        }
    }

    /// <summary>
    /// Update the swatch names of the project ensuring its in sync with the SwatchList
    /// </summary/>
    protected void UpdateAndDrawSwatchNames()
    {
        string[] swatchNames = this.objectAssetsService.GetCurrentSwatchNames();

        this.objectAssetsService.SetSwatchIndex(EditorGUILayout.Popup("Primary Swatch", this.objectAssetsService.GetSwatchIndex(), swatchNames));

        if (this.objectAssetsService.GetSwatchList()[this.objectAssetsService.GetSwatchIndex()].GetSubSwatchList().Length != 0)
        {
            GUI.enabled = !this.squashSecondarySwatches;
            string[] swatchSubNames = this.objectAssetsService.GetCurrentSwatchSubNames();
            this.objectAssetsService.SetSwatchSubIndex(EditorGUILayout.Popup("Secondary Swatch", this.objectAssetsService.GetSwatchSubIndex(), swatchSubNames));
            this.UpdateValues();
            GUI.enabled = true;
        }

        GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Enabling this option will allow the secondary swatch be split"));
    }

    /// <summary>
    /// Update and the draw directory names
    /// </summary/>
    protected void UpdateAndDrawSubDirectoryNames()
    {
        if (this.GetSubDirectoryIndex() != this.GetPreviousSubDirectoryIndex())
        {
            this.objectAssetsService.StartUpdateSwatchCoroutine();
            this.SetPreviousSubDirectoryIndex(this.GetSubDirectoryIndex());
            this.objectAssetsService.SetSwatchIndex(0);
            this.objectAssetsService.SetSwatchSubIndex(0);
            this.UpdateValues();
        }

        if (this.objectAssetsService.GetSubDirectoryNames() != null && this.objectAssetsService.GetSubDirectoryNames().Length > 0)
        {
            this.SetSubDirectoryIndex(EditorGUILayout.Popup("Sub Directory", this.GetSubDirectoryIndex(), this.objectAssetsService.GetSubDirectoryNames()));
        }
    }

    /// <inheritdoc>
    /// <see cref="SwatchTab"/>
    /// </inheritdoc>
    public override void OnSelectObject() => this.ObjectToPlaceSelected();

    /// <inheritdoc>
    /// <see cref="SwatchTab"/>
    /// </inheritdoc>
    public override void OnDeselectObject() => this.ObjectToPlaceDeselected();

    /// <summary>
    /// Gets the button height in relative to the zoom
    /// </summary>
    public float GetThumbnailSize() => EditorGUIUtility.singleLineHeight * this.listPreviewZoom;

    /// <summary>
    /// Gets a list of every single swatch in a sub swatch
    /// </summary>
    protected virtual List<SwatchData<T>> LoadAllObjectValues() => throw new NotImplementedException();

    /// <summary>
    /// Gets a list of swatches
    /// </summary>
    protected virtual List<SwatchData<T>> LoadSecondarySwatchObjectValues() => throw new NotImplementedException();

    /// <summary>
    /// Inheritable event for when an object to place is selected
    /// </summary>
    protected virtual void ObjectToPlaceSelected() => throw new NotImplementedException();

    /// <summary>
    /// Inheritable event for when an object to place is deselected
    /// </summary>
    protected virtual void ObjectToPlaceDeselected() => throw new NotImplementedException();

    /// <summary>
    /// Render the button for our generic object and actions that are applied when the button is selected
    /// <param name="iconRect"> The rect of the icon to be rendered</param>
    /// <param name="genericObjectSwatchData"> The genericObject to be rendered</param>
    /// </summary>
    protected virtual Rect DrawObjectButton(Rect iconRect, SwatchData<T> genericObjectSwatchData)
    {
        bool button = GUILayout.Button(new GUIContent("", genericObjectSwatchData.genericObject.ToString()), GUILayout.Width(iconRect.width), GUILayout.Height(iconRect.height));
        Rect buttonRect = GUILayoutUtility.GetLastRect();

        if (button)
        {
            if (this.objectToDraw == null || !genericObjectSwatchData.genericObject.Equals(this.objectToDraw.genericObject))
            {
                this.OnObjectButtonClick(genericObjectSwatchData);
            }
        }

        return buttonRect;
    }

    /// <summary>
    /// Actions that are performed when a button is clicked
    /// <param name="genericObjectSwatchData"> The genericObject to be rendered</param>
    /// </summary>
    protected virtual void OnObjectButtonClick(SwatchData<T> genericObjectSwatchData) => throw new NotImplementedException();

    /// <summary>
    /// Render the thumbnail for the current genericObject
    /// <param name="buttonRect"> The rect of the icon to be rendered</param>
    /// <param name="genericObjectSwatchData"> The genericObject to be rendered</param>
    /// </summary>
    protected virtual void DrawObjectThumbnail(Rect buttonRect, SwatchData<T> genericObjectSwatchData) => throw new NotImplementedException();

    /// <summary>
    /// Draws a highlight over the genericObject when hovered or selected
    /// <param name="buttonRect"> The rect of the button to be highlight</param>
    /// <param name="genericObjectSwatchData"> The genericObject to be highlight </param>
    /// </summary>
    protected virtual void DrawObjectHighlight(Rect buttonRect, SwatchData<T> genericObjectSwatchData, bool highlighted = false)
    {
        Color color = highlighted ? new Color32(0x42, 0x80, 0xe4, 0x40) : new Color(0, 0, 0, 0);

        if (this.objectToDraw != null && this.objectToDraw.genericObject != null)
        {
            color = this.objectToDraw.genericObject.Equals(genericObjectSwatchData.genericObject) ? new Color32(0x42, 0x80, 0xe4, 0x80) : color;
        }

        EditorGUI.DrawRect(buttonRect, color);
    }

    public void UpdateValues() => this.values = this.squashSecondarySwatches ? this.LoadAllObjectValues() : this.LoadSecondarySwatchObjectValues();
}
