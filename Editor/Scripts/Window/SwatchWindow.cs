using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

/// <summary>
///A Modified version of Alxendervrs prefab swatch
///This servers as a level editor which allows the users to add prefabs from directories to the scene while considering things like snap etc
/// </summary>
public class SwatchWindow : EditorWindow
{
    /// <summary> The main directory to look into </summary>
    private string assetsDirectory = "Assets/Resources";

    /// <summary> The previous value of <see cref="assetsDirectory"/> to help identify changes </summary>
    private string prevAssetsDirectory = "Assets/Resources";


    [MenuItem("Window/Object Swatch", priority = 10001)]
    private static void CreateWindow()
    {
        SwatchWindow window = GetWindow<SwatchWindow>("Object Swatch");
        window.titleContent = new GUIContent("Object Swatch");
        window.Show();
    }

    /// <summary> Handle interactions of the objects tab </summary>
    private PrefabsTab prefabTab;
    /// <summary> Handle interactions of the sprites tab </summary>
    private SpritesTab spritesTab;

    /// <summary> A Service to get a list of layers in project </summary>
    private ObjectLayersService objectLayersService;

    /// <summary> A service to get a list of prefabs in our assets directory</summary>
    private PrefabAssetsService prefabAssetsService;

    /// <summary> A service to get a list of sprites in our sprites directory</summary>
    private SpriteAssetsService spriteAssetsService;

    /// <summary> A service that draws the selected object from the active tab</summary>
    private DrawObjectService drawObjectService;

    /// <summary> Manages the toolbar that is seen while the prefab swatch is active</summary>
    private ToolbarDrawer toolbarDrawer;

    /// <summary> The scroll view for the entire editor window</summary>
    private Vector2 windowScrollView;

    /// <summary> The current position of the mouse</summary>
    private Vector2 mousePosition;

    /// <summary> The Current tab the user is view which either Prefab/Swatch usually </summary>
    private int currentTab;

    /// <summary> Used to determine if the swatch is trying to load data </summary>
    private bool loading = false;

    private void Awake()
    {
        this.prefabTab = new PrefabsTab(this);
        this.spritesTab = new SpritesTab(this);
        this.prefabAssetsService = new PrefabAssetsService(this, this.prefabTab);
        this.spriteAssetsService = new SpriteAssetsService(this, this.spritesTab);
        this.prefabTab.SetObjectAssetsService(this.prefabAssetsService);
        this.spritesTab.SetObjectAssetsService(this.spriteAssetsService);
        this.objectLayersService = new ObjectLayersService(this);
        this.drawObjectService = new DrawObjectService(this);
        this.toolbarDrawer = new ToolbarDrawer(this);
        this.prevAssetsDirectory = this.assetsDirectory;
    }

    private void OnEnable()
    {
        this.prefabTab = new PrefabsTab(this);
        this.spritesTab = new SpritesTab(this);
        this.prefabAssetsService = new PrefabAssetsService(this, this.prefabTab);
        this.spriteAssetsService = new SpriteAssetsService(this, this.spritesTab);
        this.prefabTab.SetObjectAssetsService(this.prefabAssetsService);
        this.spritesTab.SetObjectAssetsService(this.spriteAssetsService);
        this.objectLayersService = new ObjectLayersService(this);
        this.drawObjectService = new DrawObjectService(this);
        this.toolbarDrawer = new ToolbarDrawer(this);
        SceneView.duringSceneGui -= this.OnSceneGUI;
        SceneView.duringSceneGui += this.OnSceneGUI;
        Undo.undoRedoPerformed -= this.Repaint;
        Undo.undoRedoPerformed += this.Repaint;
        this.wantsMouseMove = true;
        this.wantsMouseEnterLeaveWindow = true;
        this.GetDrawObjectService().SetSnapCellSize(new Vector2(EditorPrefs.GetFloat("Editor_PrefabSwatch_SnapX"), EditorPrefs.GetFloat("Editor_PrefabSwatch_SnapY")));
    }

    private void Update()
    {
        if (Application.isPlaying && this.GetDrawObjectService().GetObjectToPlace() != null)
        {
            this.GetDrawObjectService().RemoveObjectToPlace();
        }
    }

    /// <summary>
    /// Refresh the active windows within the scene
    /// </summary>
    public void RefreshAllViews() => InternalEditorUtility.RepaintAllViews();

    /** DRAW UI */
    /// <summary>
    /// Manages the interaction between the user and the scene
    /// </summary>
    private void ManageUISceneInteraction()
    {
        if (Event.current.isMouse)
        {
            this.mousePosition = Event.current.mousePosition;
            this.Repaint();
        }

        if (Event.current.type == EventType.KeyDown)
        {
            foreach (ToolbarButton toolbarButton in this.toolbarDrawer.GetToolbarButtons())
            {
                if (Event.current.keyCode == toolbarButton.keyCode)
                {
                    toolbarButton.action();
                }
            }
        }
    }

    /// <summary>
    /// Get the GUI Seperator between objects
    /// </summary>
    public GUIStyle RenderSeperator()
    {
        GUIStyle guiSeparator = new GUIStyle("box");
        guiSeparator.border.top = guiSeparator.border.bottom = 1;
        guiSeparator.margin.top = guiSeparator.margin.bottom = 5;
        guiSeparator.margin.bottom = guiSeparator.margin.top = 5;
        guiSeparator.padding.top = guiSeparator.padding.bottom = 1;

        return guiSeparator;
    }

    /// <summary>
    /// Get the current tab
    /// </summary>
    public int GetCurrentTab() => this.currentTab;

    /// <summary>
    /// Draw an empty space of the specified length 
    /// <paramref name="amount"/> The amount of break tags to be drawn <paramref name="amount"/>
    /// </summary>
    public void DrawBreakPoint(int amount = 1)
    {
        for (int x = 0; x < amount; x++)
        {
            EditorGUILayout.Space();
        }
    }

    public void RepaintAll() => SceneView.RepaintAll();


    private void OnGUI()
    {
        this.ManageUISceneInteraction();

        EditorGUILayout.Space();
        this.windowScrollView = EditorGUILayout.BeginScrollView(this.windowScrollView, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        this.DrawCurrentTabInfo();
        GUILayout.Box("", this.RenderSeperator(), GUILayout.ExpandWidth(true), GUILayout.Height(1));

        this.GetDrawObjectService().DrawSnapSettings();
        GUILayout.Box("", this.RenderSeperator(), GUILayout.ExpandWidth(true), GUILayout.Height(1));
        this.DrawBreakPoint();

        EditorPrefs.SetFloat("Editor_PrefabSwatch_SnapX", this.GetDrawObjectService().GetSnapCellSize().x);
        EditorPrefs.SetFloat("Editor_PrefabSwatch_SnapY", this.GetDrawObjectService().GetSnapCellSize().y);

        switch (this.currentTab)
        {
            case 0:
                this.prefabTab.SetWindowSize(new Vector2(this.position.width, this.position.height));
                this.prefabTab.DrawTabTemplate(this.mousePosition);
                break;
            case 1:
                this.spritesTab.SetWindowSize(new Vector2(this.position.width, this.position.height));
                this.spritesTab.DrawTabTemplate(this.mousePosition);
                break;
            default:
                break;

        }

        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// Updates the current tab info
    /// </summary>
    private void DrawCurrentTabInfo()
    {
        int previousTab = currentTab;
        this.currentTab = GUILayout.Toolbar(this.currentTab, new string[] { "Prefab", "Sprite" });

        if (previousTab != this.currentTab)
        {
            this.GetDrawObjectService().RemoveObjectToPlace();
        }
    }

    /// <summary>
    /// Get the sprite tab info
    /// </summary>
    public SpritesTab GetSpritesTab() => this.spritesTab;

    /// <summary>
    /// Get the object tab info
    /// </summary>
    public PrefabsTab GetObjectsTab() => this.prefabTab;

    public PrefabAssetsService GetPrefabAssetsService() => this.prefabAssetsService;

    public SpriteAssetsService GetSpriteAssetsService() => this.spriteAssetsService;

    /// <summary>
    /// Get the current object layers service
    /// </summary>
    public ObjectLayersService GetObjectLayersService() => this.objectLayersService;

    /// <summary>
    /// Get the object placement service
    /// </summary>
    public DrawObjectService GetDrawObjectService() => this.drawObjectService;

    /// <summary>
    /// Get the object placement service
    /// </summary>
    public ToolbarDrawer GetToolbarDrawer() => this.toolbarDrawer;

    /// <summary>
    /// Get the loading state of the prefab swatch window
    /// </summary>
    public bool GetLoading() => this.loading;

    /// <summary>
    /// Set the loading state of the prefab swatch window
    /// <param name="loading">The new value for <see cref="loading"/></param>
    /// </summary>
    public void SetLoading(bool loading) => this.loading = loading;

    /// <summary>
    /// Get the assets directory
    /// </summary>
    public string GetAssetsDirectory() => this.assetsDirectory;

    /// <summary>
    /// Set the assets directory
    /// <param name="assetsDirectory">The new value for <see cref="assetsDirectory"/></param>
    /// </summary>
    public string SetAssetsDirectory(string assetsDirectory) => this.assetsDirectory = assetsDirectory;

    /// <summary>
    /// Get the previous assets directory
    /// </summary>
    public string GetPreviousAssetsDirectory() => this.prevAssetsDirectory;

    /// <summary>
    /// Set the previous assets directory
    /// <param name="prevAssetsDirectory">The new value for <see cref="prevAssetsDirectory"/></param>
    /// </summary>
    public string SetPreviousAssetsDirectory(string prevAssetsDirectory) => this.prevAssetsDirectory = prevAssetsDirectory;


    /// <summary>
    /// Handles the interaction with the window and selected object in the scene
    /// <param name="view">The current scene view</param>
    /// </summary>
    private void OnSceneGUI(SceneView view)

    {
        view.wantsMouseMove = true;
        view.wantsMouseEnterLeaveWindow = true;

        HandleUtility.Repaint();
        Handles.color = Color.yellow;

        if (this.drawObjectService.GetObjectToPlace() != null)
        {
            int control = GUIUtility.GetControlID(FocusType.Passive);
            Handles.CircleHandleCap(control, this.drawObjectService.GetObjectToPlace().gameObject.transform.position, Quaternion.identity, 0.05f, EventType.Repaint);
            this.GetDrawObjectService().DebugDrawBbox(this.drawObjectService.GetObjectToPlace().gameObject, Color.yellow);
        }

        Font defaultFont = GUI.skin.font;

        if (Screen.width >= 396)
        {
            this.GetToolbarDrawer().DrawToolbar(view);
        }

        this.ManageUserInputSceneEvents();
    }

    /// <summary>
    /// Manages the users input when interacting with the scene
    /// </summary/>
    private void ManageUserInputSceneEvents()
    {
        if (this.GetDrawObjectService().GetObjectToPlace() == null)
        {
            return;
        }

        int control = GUIUtility.GetControlID(FocusType.Passive);

        switch (Event.current.type)
        {
            case EventType.KeyDown:
                if (Event.current.keyCode == KeyCode.Escape)
                {
                    this.drawObjectService.RemoveObjectToPlace();
                }

                break;
            case EventType.Layout:
                HandleUtility.AddDefaultControl(control);


                break;
            case EventType.MouseDown:
                if (Event.current.button == 0)
                {
                    Tools.current = Tool.None;
                    this.GetDrawObjectService().SaveObjectInScene();
                    Event.current.Use();
                }

                break;
            case EventType.MouseUp:
                Tools.current = Tool.None;

                break;
            default:
                break;
        }

        if (Event.current.isMouse || Event.current.type == EventType.MouseEnterWindow)
        {
            this.mousePosition = Event.current.mousePosition;
            this.GetDrawObjectService().UpdatePlacingPrefab(this.mousePosition);
        }
    }

    /// <summary>
    /// Determines whether the <see cref="GUILayoutUtility.GetLastRect()"/> is valid based on the current event
    /// </summary>
    public bool IsGetLastRectValid() => Event.current.type is not EventType.Layout and not EventType.Repaint;
}
