using UnityEngine;

/// <summary>
///Helper GUI functions for the prefab swatch
/// </summary>
public static class SwatchEditorHelper
{
    private static readonly Texture2D backgroundTexture = Texture2D.whiteTexture;
    private static readonly GUIStyle textureStyle = new GUIStyle { normal = new GUIStyleState { background = backgroundTexture } };

    /// <summary>
    ///Draw a rect
    /// </summary>
    public static void DrawRect(Rect position, Color color, GUIContent content = null)
    {
        Color backgroundColor = GUI.backgroundColor;
        GUI.backgroundColor = color;
        GUI.Box(position, content ?? GUIContent.none, textureStyle);
        GUI.backgroundColor = backgroundColor;
    }

    /// <summary>
    ///Draw a lyout box
    /// </summary>
    public static void LayoutBox(Color color, GUIContent content = null)
    {
        Color backgroundColor = GUI.backgroundColor;
        GUI.backgroundColor = color;
        GUILayout.Box(content ?? GUIContent.none, textureStyle);
        GUI.backgroundColor = backgroundColor;
    }

    /// <summary>
    ///Draw a sprite
    /// </summary>
    public static void DrawSprite(Rect rect, Sprite sprite)
    {
        Rect spriteRect = sprite.rect;
        Texture2D tex = sprite.texture;
        GUI.DrawTextureWithTexCoords(rect, tex, new Rect(spriteRect.x / tex.width, spriteRect.y / tex.height, spriteRect.width / tex.width, spriteRect.height / tex.height));
    }

}

