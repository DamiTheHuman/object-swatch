using UnityEngine;

[System.Serializable]
/// <summary>
/// Contains References to the information around the sprite to place
/// </summary>
public class SpriteReferences
{
    /// <summary> The sprite rednerer for the current reference </summary>
    public SpriteRenderer spriteRenderer;
    /// <summary> The original color for the sprite references</summary>
    public Color originalColor;
    /// <summary> The original layer for the sprite reference</summary>
    public int originalLayer;

    public SpriteReferences(SpriteRenderer spriteRenderer)
    {
        this.spriteRenderer = spriteRenderer;
        this.originalColor = spriteRenderer.color;
        this.originalLayer = spriteRenderer.sortingLayerID;
    }
}
