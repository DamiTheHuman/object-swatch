using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// A class that handles the drawing and placement of prefabs on the editor scene
/// </summary>
public class DrawObjectService
{
    /// <summary>
    /// The prefab swatch window
    /// </summary>
    public SwatchWindow swatchWindow;

    /// <summary> An override that forces the drawer to snap the object to be placed position </summary>
    private bool snap = true;

    /// <summary> The size of the grid to be snapped to</summary>
    private Vector2 snapCellSize = new Vector2(16, 16);

    /// <summary>
    /// An external action called when the current object is deselected
    /// </summary>
    private Action onDeselectAction;

    /// <summary>
    /// The object to place in the scene
    /// </summary>
    private ObjectToPlace objectToPlace;

    /// <summary>
    /// The current rotation;
    /// </summary>
    public Quaternion currentRotation;

    /// <summary>
    /// The current local scale
    /// </summary>
    private Vector3 currentLocalScale = new Vector3(1, 1, 1);

    /// <summary>
    /// Image Offset
    /// </summary>
    private int layerOffset = 0;

    public DrawObjectService(SwatchWindow prefabSwatchWindow) => this.swatchWindow = prefabSwatchWindow;

    public void Reset()
    {
        if (this.objectToPlace != null)
        {
            UnityEngine.Object.DestroyImmediate(this.objectToPlace.gameObject);
        }

        this.currentRotation = Quaternion.Euler(0, 0, 0);
        this.currentLocalScale = new Vector3(1, 1, 1);
        this.layerOffset = 0;
        this.objectToPlace = null;
    }

    /// <summary>
    /// Set the value for the object to place in scene
    /// <param name="snap">The new value for <see cref="objectToPlace"/></param>
    /// </summary>
    public void SetObjectToPlace(ObjectToPlace objectToPlace, bool skipReset = false)
    {
        if (this.objectToPlace != null)
        {
            this.RemoveObjectToPlace();

            return;
        }

        if (!skipReset)
        {
            this.Reset();
        }

        this.objectToPlace = objectToPlace;

        if(this.objectToPlace.gameObject == null)
        {
            return;
        }

        this.objectToPlace.SetSpriteReferencesColor(new Color(1, 1, 1, 0.5f));
        this.objectToPlace.gameObject.transform.localScale = this.currentLocalScale;
        this.objectToPlace.gameObject.transform.rotation = this.currentRotation;
    }


    /// <summary>
    /// Update the prefab position based on the current mouse position when a prefab is selected
    /// <param name="mousePosition">The current position of the mouse </param>
    /// </summary>
    public void UpdatePlacingPrefab(Vector2 mousePosition)
    {
        if (this.objectToPlace == null || Event.current.control)
        {
            return;
        }

        Vector2 positionToPlaceAt = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
        positionToPlaceAt = this.snap ? this.SnapPositionToPlaceAt(positionToPlaceAt) : positionToPlaceAt;

        this.SetObjectToPlacePosition(new Vector2(positionToPlaceAt.x, positionToPlaceAt.y));
        this.objectToPlace.gameObject.transform.localScale = this.currentLocalScale;
        this.objectToPlace.gameObject.transform.rotation = this.currentRotation;
    }

    /// <summary>
    /// Save the current object to place in scene and create a clone for continous placement if possible
    /// </summary>
    public void SaveObjectInScene()
    {
        if (this.objectToPlace == null)
        {
            return;
        }

        this.ConfigureObjectToPlaceHeirachy(this.objectToPlace);
        this.objectToPlace.ResetSpriteReferenceColors();
        string objectToPlaceInSuffix = " (" + this.GetSiblingCloneIndex(this.objectToPlace) + ")";

        Undo.RegisterCreatedObjectUndo(this.objectToPlace.gameObject, "Place Object: " + this.objectToPlace.gameObject.name + "In Scene via Object Swatch");
        GameObject clonedGameObject = this.objectToPlace.originalPrefabInstance != null ? (GameObject)PrefabUtility.InstantiatePrefab(this.objectToPlace.originalPrefabInstance, SceneManager.GetActiveScene()) : UnityEngine.Object.Instantiate(this.objectToPlace.gameObject);
        ObjectToPlace cloneObjectToPlace = new ObjectToPlace(clonedGameObject, this.objectToPlace.hierarchy, this.objectToPlace.originalPrefabInstance);
        cloneObjectToPlace.gameObject.name = this.objectToPlace.gameObject.name;
        cloneObjectToPlace.gameObject.transform.position = this.objectToPlace.gameObject.transform.position;

        this.objectToPlace.gameObject.name += objectToPlaceInSuffix;
        this.objectToPlace = null;
        this.SetObjectToPlace(cloneObjectToPlace, true);
        this.MoveRenderLayer(0);
    }

    /// <summary>
    /// Configure the heirarchy of the object to place by creating a tree of objects seperated by a '/'
    /// Example: Input: Parent1/Parent2/Parent3
    ///         Output: Parent1 (GameObject)
    ///                     Parent2 (GameObject)
    ///                         Parent3 (GameObject)
    ///                             <see cref="objectToPlace"/>
    ///     
    /// <param name="objectToPlace">The the object to place according to the heirachy</param>
    /// </summary>
    private void ConfigureObjectToPlaceHeirachy(ObjectToPlace objectToPlace)
    {
        string[] parentComponents = objectToPlace.hierarchy.Split('/');
        GameObject parentTo = this.GetOrCreateParentGameObjectWithName(objectToPlace.actGrid);

        if (parentTo.GetComponent<Grid>() == null)
        {
            parentTo.AddComponent<Grid>().GetComponent<Grid>().cellSize = this.snapCellSize;
        }

        for (int x = 0; x < parentComponents.Length - 1; x++)
        {
            GameObject nextParentTo = this.GetOrCreateParentGameObjectWithName(parentComponents[x], parentTo);

            if (parentTo != null)
            {
                nextParentTo.transform.parent = parentTo.transform;
            }

            parentTo = nextParentTo;
        }

        objectToPlace.gameObject.transform.parent = parentTo.transform;
    }

    /// <summary>
    /// Get the parent in the scene with the set <see cref="name"/> and return it if it exists but if not create it
    /// <param name="name">The new of the parent to look for or create</param>
    /// <param name="parent">The parent to to parent the gameobject with if set</param>
    /// </summary>
    private GameObject GetOrCreateParentGameObjectWithName(string name, GameObject parent = null)
    {
        if (parent != null)
        {
            Transform[] allParentChildren = parent.GetComponentsInChildren<Transform>();
            Transform childWithName = allParentChildren.Where(k => k.gameObject.name == name && k.transform.parent == parent.transform).FirstOrDefault();

            if (childWithName != null)
            {
                return childWithName.gameObject;
            }

            GameObject targetParent = new GameObject(name);
            targetParent.transform.parent = parent.transform;

            return targetParent;
        }

        GameObject targetGameObject = GameObject.Find(name);

        if (targetGameObject != null)
        {
            return targetGameObject;
        }

        return new GameObject(name);
    }

    /// <summary>
    /// Finds the number of siblings of the same type/name
    /// </summary>
    private int GetSiblingCloneIndex(ObjectToPlace objectToPlaceInScene)
    {
        int count = 0;

        foreach (Transform gameObject in this.objectToPlace.gameObject.transform.parent.GetComponentsInChildren<Transform>())
        {
            if (gameObject.name.Split('(')[0].Trim() == objectToPlaceInScene.gameObject.name.Split('(')[0].Trim())
            {
                count++;
            }
        }

        return count - 1;
    }

    /// <summary>
    /// Remove the current object
    /// </summary>
    public void RemoveObjectToPlace()
    {
        if (this.objectToPlace != null)
        {
            UnityEngine.Object.DestroyImmediate(this.objectToPlace.gameObject);
            this.objectToPlace = null;
        }

        if (this.onDeselectAction != null)
        {
            this.onDeselectAction();
            this.onDeselectAction = null;
        }
    }

    /// <summary>
    /// Draw the debug box around the rect of the to be placed object
    /// <param name="gameObject">The object to draw debug info around</param>
    /// <param name="color">The color for the debug info</param>
    /// </summary>
    public void DebugDrawBbox(GameObject gameObject, Color color)
    {
        SpriteRenderer sprRenderer = gameObject.GetComponent<SpriteRenderer>() ?? gameObject.GetComponentInChildren<SpriteRenderer>();

        if (sprRenderer == null)
        {
            return;
        }

        // top left
        float x1 = sprRenderer.bounds.min.x;
        float y1 = sprRenderer.bounds.max.y;
        Vector2 topLeft = new Vector2(x1, y1);
        // top right
        float x2 = sprRenderer.bounds.max.x;
        float y2 = sprRenderer.bounds.max.y;
        Vector2 topRight = new Vector2(x2, y2);
        // bottom right
        float x3 = sprRenderer.bounds.max.x;
        float y3 = sprRenderer.bounds.min.y;
        Vector2 bottomRight = new Vector2(x3, y3);
        // bottom left
        float x4 = sprRenderer.bounds.min.x;
        float y4 = sprRenderer.bounds.min.y;
        Vector2 bottomLeft = new Vector2(x4, y4);
        Handles.DrawLine(topLeft, topRight);
        Handles.DrawLine(topRight, bottomRight);
        Handles.DrawLine(bottomRight, bottomLeft);
        Handles.DrawLine(bottomLeft, topLeft);
    }


    /// <summary>
    /// Set the position for the current selected object
    /// <param name="position">The new value for the object position </param>
    /// </summary>
    public void SetObjectToPlacePosition(Vector2 position)
    {
        if (this.objectToPlace.gameObject == null)
        {
            return;
        }

        this.objectToPlace.gameObject.transform.position = position;
    }

    /// <summary>
    /// Set the rotation for the current selected object
    /// <param name="snap">The new rotation value for the selected object</param>
    /// </summary>
    public void SetObjectToPlaceRotation(Quaternion rotation)
    {
        if (this.objectToPlace == null)
        {
            return;
        }

        this.currentRotation = rotation;
        this.objectToPlace.gameObject.transform.rotation = this.currentRotation;
        this.swatchWindow.RepaintAll();
    }

    /// <summary>
    /// Update the object to place rotation by the angle passed
    /// <param name="angle">The angle of the object to place</param>
    /// </summary
    public void AddRotation(float angle)
    {
        if (this.objectToPlace == null)
        {
            return;
        }

        this.SetObjectToPlaceRotation(Quaternion.Euler(0, 0, this.objectToPlace.gameObject.transform.eulerAngles.z + angle));
    }

    /// <summary>
    /// Move the render layer by the set amount in either direction
    /// <param name="directionToMoveBy">The direcetiont o move the render layer by </param>
    /// </summary>
    public void MoveRenderLayer(int directionToMoveBy = 1)
    {
        if (this.objectToPlace == null)
        {
            return;
        }

        int[] sortingLayerUniqueIds = this.swatchWindow.GetObjectLayersService().GetSortingLayerUniqueIDs();

        foreach (SpriteReferences spriteReference in this.objectToPlace.spriteReferences)
        {
            int originalIndex = this.swatchWindow.GetObjectLayersService().GetSortingLayerIndex(spriteReference.originalLayer);

            //If the index doesnt exist in our array undo the change and return
            if (this.layerOffset + originalIndex + directionToMoveBy < 0 || this.layerOffset + originalIndex + directionToMoveBy > sortingLayerUniqueIds.Length - 1)
            {
                this.swatchWindow.RepaintAll();

                return;
            }
        }

        this.layerOffset += directionToMoveBy;

        foreach (SpriteReferences spriteReference in this.objectToPlace.spriteReferences)
        {
            int originalIndex = this.swatchWindow.GetObjectLayersService().GetSortingLayerIndex(spriteReference.originalLayer);
            spriteReference.spriteRenderer.sortingLayerID = sortingLayerUniqueIds[originalIndex + this.layerOffset];
        }

        this.swatchWindow.RepaintAll();
    }

    /// <summary>
    /// Flip the selected prefab horizontally or vertically
    /// <param name="flipHorizontal">Whether to flip the object on the horizontal or vertical axis</param>
    /// </summary>
    public void FlipSelectedObject(bool flipHorizontal = true)
    {
        if (this.objectToPlace == null)
        {
            return;
        }

        Vector3 localScale = this.objectToPlace.gameObject.transform.localScale;

        if (flipHorizontal)
        {
            localScale.x *= -1;
        }
        else
        {
            localScale.y *= -1;
        }

        this.currentLocalScale = localScale;
        this.objectToPlace.gameObject.transform.localScale = this.currentLocalScale;
        this.swatchWindow.RepaintAll();
    }

    /// <summary>
    /// Draw the snap settings
    /// </summary>
    public void DrawSnapSettings()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Snap");
        GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Enabling this option will snap the Objects to the specified Cell size (in Units)"));
        this.snap = EditorGUILayout.Toggle(this.snap, GUILayout.Width(15f));
        EditorGUILayout.EndHorizontal();

        if (this.snap)
        {
            this.snapCellSize = EditorGUILayout.Vector2Field("", this.snapCellSize);
            this.snapCellSize = Vector2.Max(this.snapCellSize, new Vector2(8, 8));
        }

        EditorGUILayout.Space();
        GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "Enabling this option will continuously place Prefabs when you click and drag the mouse"));
    }

    public ObjectToPlace GetObjectToPlace() => this.objectToPlace;

    /// <summary>
    /// Snap the position to place at
    /// <param name="currentPosition">Snaps the objects current position to the set <see cref="snapCellSize"/></param>
    /// </summary>
    private Vector3 SnapPositionToPlaceAt(Vector2 currentPosition)
    {
        if (this.snapCellSize.x > 0f && this.snapCellSize.y > 0f)
        {
            currentPosition.x = Mathf.Round(currentPosition.x / this.snapCellSize.x) * this.snapCellSize.x;
            currentPosition.y = Mathf.Round(currentPosition.y / this.snapCellSize.y) * this.snapCellSize.y;
        }

        return currentPosition;
    }

    /// <summary>
    /// Get the snap cell size
    /// </summary>
    public Vector2 GetSnapCellSize() => this.snapCellSize;

    /// <summary>
    /// Set the snap cell size
    /// <param name="snapCellSize">The new value for <see cref="snapCellSize"/></param>
    /// </summary>
    public void SetSnapCellSize(Vector2 snapCellSize) => this.snapCellSize = snapCellSize;

    /// <summary>
    /// Get the value determining whether snap is active
    /// </summary>
    public bool GetSnap() => this.snap;

    /// <summary>
    /// Swet the value of snap
    /// <param name="snap">The new value for <see cref="snap"/></param>
    /// </summary>
    public bool SetSnap(bool snap) => this.snap = snap;

    /// <summary>
    /// Set extra actions to be performed when the current selected object is deselected
    /// <param name="onDeselectAction">The new value for <see cref="onDeselectAction"/></param>
    /// </summary>
    public void SetOnDeselectAction(Action onDeselectAction) => this.onDeselectAction = onDeselectAction;
}
