using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PrefabsTab : ObjectsTab<GameObject>
{
    public PrefabsTab(SwatchWindow spriteSwatchWindow) : base(spriteSwatchWindow)
    {
        this.swatchWindow = spriteSwatchWindow;
        this.tag = "Prefab";
    }

    public override void Reset()
    {
        this.objectAssetsService.SetSwatchIndex(0);
        this.objectAssetsService.SetSwatchSubIndex(0);
        this.objectAssetsService.FetchTabData();
    }

    /// <inheritdoc>
    /// <see cref="ObjectAssetsService{T}"/>
    /// </inheritdoc>
    protected override List<SwatchData<GameObject>> LoadAllObjectValues()
    {
        List<SwatchData<GameObject>> values = new List<SwatchData<GameObject>>();

        if (this.objectAssetsService.GetSwatchList().Count == 0)
        {
            return new List<SwatchData<GameObject>>();
        }

        foreach (Dictionary<string, List<SwatchData<GameObject>>> swatches in this.objectAssetsService.GetSwatchList()[this.objectAssetsService.GetSwatchIndex()].secondarySwatchObjects)
        {
            values.AddRange(swatches.First().Value);
        }

        try
        {
            values = values.Where(x => x.genericObject.name.IndexOf(this.objectAssetsService.GetSearchFilter(), StringComparison.OrdinalIgnoreCase) != -1).ToList();//Filter out results
        }
        catch (MissingReferenceException)
        {
            this.objectAssetsService.FetchTabData();

            return new List<SwatchData<GameObject>>();
        }

        return values;
    }

    /// <inheritdoc>
    /// <see cref="ObjectAssetsService{T}"/>
    /// </inheritdoc>
    protected override List<SwatchData<GameObject>> LoadSecondarySwatchObjectValues()
    {
        List<SwatchData<GameObject>> values = this.objectAssetsService.GetSwatchList()[this.objectAssetsService.GetSwatchIndex()].secondarySwatchObjects.Count == 0 ? this.objectAssetsService.GetSwatchList()[this.objectAssetsService.GetSwatchIndex()].primarySwatchObjects : this.objectAssetsService.GetSwatchList()[this.objectAssetsService.GetSwatchIndex()].secondarySwatchObjects[this.objectAssetsService.GetSwatchSubIndex()].First().Value;

        try
        {
            values = values.Where(x => x.genericObject.name.IndexOf(this.objectAssetsService.GetSearchFilter(), StringComparison.OrdinalIgnoreCase) != -1).ToList();//Filter out results
        }
        catch (MissingReferenceException)
        {
            this.objectAssetsService.FetchTabData();

            return new List<SwatchData<GameObject>>();
        }

        return values;
    }

    /// <inheritdoc>
    /// <see cref="ObjectsTab{T}"/>
    /// </inheritdoc>
    protected override Rect DrawObjectButton(Rect iconRect, SwatchData<GameObject> genericObjectSwatchData)
    {
        bool button = GUILayout.Button(new GUIContent("", genericObjectSwatchData.genericObject.name), GUILayout.Width(iconRect.width), GUILayout.Height(iconRect.height));
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

    /// <inheritdoc>
    /// <see cref="ObjectAssetsService{T}"/>
    /// <param name="buttonRect"> The rect of the icon to be rendered</param>
    /// <param name="prefabSwatchData"> The prefab of the gameobject being rendered</param>
    /// </inheritdoc>
    protected override void DrawObjectThumbnail(Rect buttonRect, SwatchData<GameObject> prefabSwatchData)
    {
        Texture2D icon = AssetPreview.GetAssetPreview(prefabSwatchData.genericObject);

        if (icon != null)
        {
            GUI.DrawTexture(buttonRect, icon, ScaleMode.ScaleToFit, true, 1f, Color.white, Vector4.zero, Vector4.one * 4f);
        }
        else
        {
            EditorGUI.DrawRect(buttonRect, EditorStyles.label.normal.textColor * 0.25f);
        }
    }

    /// <inheritdoc>
    /// <see cref="ObjectAssetsService{T}"/>
    /// <param name="prefabSwatchData"> The prefab to render</param>
    /// </inheritdoc>
    protected override void OnObjectButtonClick(SwatchData<GameObject> prefabSwatchData)
    {
        this.objectToDraw = new SwatchData<GameObject>(prefabSwatchData.genericObject, prefabSwatchData.parentDirectory);
        this.swatchWindow.GetObjectLayersService().SetLayerSwitchRestriction(0);
        this.swatchWindow.GetDrawObjectService().SetObjectToPlaceRotation(Quaternion.Euler(0, 0, 0));
        this.OnSelectObject();
        Tools.current = Tool.None;
        SceneView.RepaintAll();
    }

    /// <inheritdoc>
    /// <see cref="ObjectAssetsService{T}"/>
    /// </inheritdoc>
    protected override void ObjectToPlaceSelected()
    {
        //In this case we know the user has selected when prefab and then chosen another one
        if (this.selectedObject != null)
        {
            this.swatchWindow.GetDrawObjectService().SetOnDeselectAction(null);
            this.swatchWindow.GetDrawObjectService().RemoveObjectToPlace();
        }

        if (this.objectToDraw == null)
        {
            return;
        }
        else if (this.swatchWindow.GetDrawObjectService().GetObjectToPlace() != null)
        {
            this.swatchWindow.GetDrawObjectService().RemoveObjectToPlace();
        }

        this.selectedObject = (GameObject)PrefabUtility.InstantiatePrefab(this.objectToDraw.genericObject, SceneManager.GetActiveScene());

        string hierachy = this.tag + "s/";
        hierachy += this.objectAssetsService.GetSubDirectoryNames()[this.GetSubDirectoryIndex()] + "/";
        hierachy += this.objectAssetsService.GetCurrentSwatchNames()[this.objectAssetsService.GetSwatchIndex()] + "/";
        hierachy += this.objectToDraw.parentDirectory + "/";

        this.swatchWindow.GetDrawObjectService().SetObjectToPlace(new ObjectToPlace(this.selectedObject, hierachy, this.objectToDraw.genericObject));
        this.swatchWindow.GetDrawObjectService().SetOnDeselectAction(this.OnDeselectObject);
    }

    /// <inheritdoc>
    /// <see cref="ObjectAssetsService{T}"/>
    /// </inheritdoc>
    protected override void ObjectToPlaceDeselected()
    {
        this.selectedObject = null;
        this.objectToDraw = null;
        this.swatchWindow.Repaint();
    }
}
