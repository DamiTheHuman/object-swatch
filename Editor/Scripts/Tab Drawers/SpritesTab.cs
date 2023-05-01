using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SpritesTab : ObjectsTab<Sprite>
{
    public override void Reset()
    {
        this.objectAssetsService.SetSwatchIndex(0);
        this.objectAssetsService.SetSwatchSubIndex(0);
        this.objectAssetsService.FetchTabData();
    }

    public SpritesTab(SwatchWindow spriteSwatchWindow) : base(spriteSwatchWindow)
    {
        this.swatchWindow = spriteSwatchWindow;
        this.tag = "Sprite";
    }

    /// <inheritdoc>
    /// <see cref="ObjectAssetsService{T}"/>
    /// </inheritdoc>
    protected override void DrawTabHeader(Vector2 mousePosition)
    {
        EditorGUILayout.Space();

        this.objectSortingLayerIndex = EditorGUILayout.Popup("Sorting Layer", this.objectSortingLayerIndex, this.swatchWindow.GetObjectLayersService().GetLayerNames());

        Sprite selectedSprite =  (Sprite)EditorGUILayout.ObjectField("Sprite To Draw", this.objectToDraw != null ? this.objectToDraw.genericObject : null, typeof(Sprite), true);

        if (selectedSprite)
        {
            this.objectToDraw = new SwatchData<Sprite>(selectedSprite, this.objectToDraw != null ? this.objectToDraw.parentDirectory : this.swatchWindow.GetObjectLayersService().GetLayerNames()[this.objectSortingLayerIndex]);
        }

        base.DrawTabHeader(mousePosition);
    }

    /// <inheritdoc>
    /// <see cref="ObjectAssetsService{T}"/>
    /// </inheritdoc>
    protected override List<SwatchData<Sprite>> LoadAllObjectValues()
    {
        List<SwatchData<Sprite>> values = new List<SwatchData<Sprite>>();

        if (this.objectAssetsService.GetSwatchList().Count == 0)
        {
            return new List<SwatchData<Sprite>>();
        }

        foreach (Dictionary<string, List<SwatchData<Sprite>>> swatches in this.objectAssetsService.GetSwatchList()[this.objectAssetsService.GetSwatchIndex()].secondarySwatchObjects)
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

            return new List<SwatchData<Sprite>>();
        }

        return values;
    }

    /// <inheritdoc>
    /// <see cref="ObjectAssetsService"/>
    /// </inheritdoc>
    protected override List<SwatchData<Sprite>> LoadSecondarySwatchObjectValues()
    {
        List<SwatchData<Sprite>> values = this.objectAssetsService.GetSwatchList()[this.objectAssetsService.GetSwatchIndex()].secondarySwatchObjects.Count == 0 ? this.objectAssetsService.GetSwatchList()[this.objectAssetsService.GetSwatchIndex()].primarySwatchObjects : this.objectAssetsService.GetSwatchList()[this.objectAssetsService.GetSwatchIndex()].secondarySwatchObjects[this.objectAssetsService.GetSwatchSubIndex()].First().Value;

        try
        {
            values = values.Where(x => x.genericObject.name.IndexOf(this.objectAssetsService.GetSearchFilter(), StringComparison.OrdinalIgnoreCase) != -1).ToList();//Filter out results
        }
        catch (MissingReferenceException)
        {
            this.objectAssetsService.FetchTabData();

            return new List<SwatchData<Sprite>>();
        }

        return values;
    }

    /// <inheritdoc>
    /// <see cref="ObjectsTab{T}"/>
    /// </inheritdoc>
    protected override Rect DrawObjectButton(Rect iconRect, SwatchData<Sprite> genericObjectSwatchData)
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
    /// <param name="spriteSwatchData"> The sprite sprite to render</param>
    /// </inheritdoc>
    protected override void DrawObjectThumbnail(Rect buttonRect, SwatchData<Sprite> spriteSwatchData)
    {
        Texture2D icon = spriteSwatchData.genericObject.texture;

        if (icon != null)
        {
            Texture2D tex = spriteSwatchData.genericObject.texture;
            GUI.DrawTextureWithTexCoords(buttonRect, tex, new Rect(spriteSwatchData.genericObject.rect.x / tex.width, spriteSwatchData.genericObject.rect.y / tex.height, spriteSwatchData.genericObject.bounds.size.x / tex.width, spriteSwatchData.genericObject.bounds.size.y / tex.height));
        }
        else
        {
            EditorGUI.DrawRect(buttonRect, EditorStyles.label.normal.textColor * 0.25f);
        }
    }

    /// <inheritdoc>
    /// <see cref="ObjectAssetsService{T}"/>
    /// <param name="spriteSwatchData"> The sprite sprite to render</param>
    /// </inheritdoc>
    protected override void OnObjectButtonClick(SwatchData<Sprite> spriteSwatchData)
    {
        this.objectToDraw = new SwatchData<Sprite>(spriteSwatchData.genericObject, spriteSwatchData.parentDirectory);
        this.OnSelectObject();
        Tools.current = Tool.None;
        SceneView.RepaintAll();
    }

    /// <inheritdoc>
    /// <see cref="ObjectAssetsService{T}"/>
    /// </inheritdoc>
    protected override void ObjectToPlaceSelected()
    {
        if (this.objectToDraw == null)
        {
            return;
        }

        this.selectedObject = new GameObject(this.objectToDraw.genericObject.name);
        this.selectedObject.AddComponent<SpriteRenderer>();
        this.selectedObject.GetComponent<SpriteRenderer>().sprite = this.objectToDraw.genericObject;
        this.selectedObject.GetComponent<SpriteRenderer>().sortingLayerID = this.swatchWindow.GetObjectLayersService().GetLayerDictionary()[this.swatchWindow.GetObjectLayersService().GetLayerNames()[this.objectSortingLayerIndex]];
        string hierarchy = this.tag + "s/";
        hierarchy += this.objectToDraw.parentDirectory + "/";
        this.swatchWindow.GetDrawObjectService().SetObjectToPlace(new ObjectToPlace(this.selectedObject, hierarchy));
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
