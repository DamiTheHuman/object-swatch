using UnityEngine;

/// <summary>
/// Parent class for all swatches that can be placed
/// </summary/>
public class SwatchTab
{
    public SwatchWindow swatchWindow;
    protected Vector2 windowSize;
    protected Rect lastHeaderRect;
    protected Rect lastBodyRect;
    protected Rect lastFooterRect;

    /// <summary> The pointer which defines the main directory</summary>
    [SerializeField]
    protected int subDirectoryIndex = 0;

    /// <summary> The previous value of directory index <see cref="subDirectoryIndex"/> </summary>
    protected int prevSubDirectoryIndex = 0;

    /// <summary> The original color of the prefab or sprite to be placed</summary>
    protected Color selectedObjectOriginalColor;

    /// <summary>
    /// The current selected object to to place in scene
    /// </summary/>
    public GameObject selectedObject;

    public virtual void Reset() { }

    /// <summary>
    /// Draw the template for the tab
    /// </summary/>
    public virtual void DrawTabTemplate(Vector2 mousePosition)
    {
        this.DrawTabHeader(mousePosition);
        this.DrawTabFilter(mousePosition);
        this.lastHeaderRect = GUILayoutUtility.GetLastRect();

        this.DrawTabBody(mousePosition);
        this.lastBodyRect = GUILayoutUtility.GetLastRect();

        this.DrawTabFooter(mousePosition);
        this.lastFooterRect = GUILayoutUtility.GetLastRect();
    }

    /// <summary>
    /// Draw the top section of the tab
    /// </summary/>
    protected virtual void DrawTabHeader(Vector2 mousePosition) { }

    protected virtual void DrawTabFilter(Vector2 mousePosition) { }

    /// <summary>
    /// Draws the body section of the tab
    /// </summary>
    protected virtual void DrawTabBody(Vector2 mousePosition) { }

    /// <summary>
    /// Draws the footer section for the prefab tab
    /// </summary>
    protected virtual void DrawTabFooter(Vector2 mousePosition) { }

    /// <summary>
    /// Actions performed when an object is placed in the scene
    /// </summary/>
    public virtual void OnSelectObject() { }

    /// <summary>
    /// Actions performed when the active object to place is deselected
    /// </summary/>
    public virtual void OnDeselectObject() { }

    public void SetWindowSize(Vector2 windowSize) => this.windowSize = windowSize;

    /// <summary>
    /// Get the current sub directory index
    /// </summary>
    public int GetSubDirectoryIndex() => this.subDirectoryIndex;

    /// <summary>
    /// Set the current sub directory index for assets
    /// <param name="subDirectoryIndex">The new value for <see cref="subDirectoryIndex"/></param>
    /// </summary>
    public void SetSubDirectoryIndex(int subDirectoryIndex) => this.subDirectoryIndex = subDirectoryIndex;

    /// <summary>
    /// Get the previous sub directory index
    /// </summary>
    public int GetPreviousSubDirectoryIndex() => this.prevSubDirectoryIndex;

    /// <summary>
    /// Set the previous sub directory index for assets
    /// <param name="prevSubDirectoryIndex">The new value for <see cref="prevSubDirectoryIndex"/></param>
    /// </summary>
    public void SetPreviousSubDirectoryIndex(int prevSubDirectoryIndex) => this.prevSubDirectoryIndex = prevSubDirectoryIndex;
}
