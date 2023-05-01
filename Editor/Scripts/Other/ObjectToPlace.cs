using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data for the object to place in the scene
/// </summary>
public class ObjectToPlace
{
    /// <summary>
    /// The modifiable gameobject to place in scene
    /// </summary>
    public GameObject gameObject;
    /// <summary>
    /// The main act grid for the object
    /// </summary>
    public readonly string actGrid = "Act Grid";

    /// <summary>
    /// The hierarchy depicting structure for when the game object is placed in the scene,
    /// </summary>
    public string hierarchy = "Generic";

    /// <summary>
    /// The original prefab instance of the current gameobject
    /// </summary>
    public GameObject originalPrefabInstance;

    /// <summary>
    /// The minimum sorting layer the sprite can be on
    /// </summary>
    public int minSortingLayer;

    /// <summary>
    /// The maxmium sorting layer the sprite can be on
    /// </summary>
    public int maxSortingLayer;

    /// <summary>
    /// The list of sprite references
    /// </summary>
    public List<SpriteReferences> spriteReferences = new List<SpriteReferences>();

    public ObjectToPlace(GameObject gameObject, string objectToPlaceCategories, GameObject prefabInstance = null)
    {
        this.gameObject = gameObject;
        this.actGrid = "Act Grid";
        this.hierarchy = objectToPlaceCategories;
        this.originalPrefabInstance = prefabInstance;
        this.spriteReferences.Clear();
        this.UpdateSpriteReferences();
    }

    /// <summary>
    /// Update and gather all the sprite referneces attached to the gameobject
    /// </summary>
    private void UpdateSpriteReferences()
    {
        if (this.gameObject == null)
        {
            return;
        }

        SpriteRenderer mainSpriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();

        if (mainSpriteRenderer != null)
        {
            this.minSortingLayer = mainSpriteRenderer.sortingLayerID;
            this.maxSortingLayer = mainSpriteRenderer.sortingLayerID;
        }

        foreach (SpriteRenderer spriteRenderer in this.gameObject.GetComponentsInChildren<SpriteRenderer>())
        {
            this.spriteReferences.Add(new SpriteReferences(spriteRenderer));

            if (spriteRenderer.sortingLayerID <= this.minSortingLayer)
            {
                this.minSortingLayer = spriteRenderer.sortingLayerID;
            }

            if (spriteRenderer.sortingLayerID >= this.maxSortingLayer)
            {
                this.maxSortingLayer = spriteRenderer.sortingLayerID;
            }
        }
    }

    /// <summary>
    /// Set the color of all the sprites in our sprite reference
    /// <param name="color">The currrent color for the sprite references</param>
    /// </summary>
    public void SetSpriteReferencesColor(Color color)
    {
        foreach (SpriteReferences spriteReference in this.spriteReferences)
        {
            spriteReference.spriteRenderer.color = color;
        }
    }

    /// <summary>
    /// Set the color of all the sprites in our sprite reference to their original color
    /// </summary>
    public void ResetSpriteReferenceColors()
    {
        foreach (SpriteReferences spriteReference in this.spriteReferences)
        {
            spriteReference.spriteRenderer.color = spriteReference.originalColor;
        }
    }

}
