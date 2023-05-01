using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditorInternal;

public class ObjectLayersService
{
    public SwatchWindow prefabSwatchWindow;

    /// <summary> The List of layers within the project </summary>
    private string[] layerNames = new string[] { };

    /// <summary> The List of layer id's within the project</summary>
    private int[] layerIDs = new int[] { };

    /// <summary> A dictionary that uses  <see cref="layerNames"/>layerNames to find the layer id's <see cref="layerIDs"/> </summary>
    private Dictionary<string, int> layerDictionary = new Dictionary<string, int>();

    /// <summary> A restriction stopping the layer that can be set on a sprite </summary>
    private int layerSwitchRestriction = 0;

    public ObjectLayersService(SwatchWindow prefabSwatchWindow)
    {
        this.prefabSwatchWindow = prefabSwatchWindow;
        this.UpdateLayerData();
    }

    /// <summary>
    /// Get Layer names
    /// </summary>
    public string[] GetLayerNames() => this.layerNames;

    /// <summary>
    /// Get Layer names
    /// </summary>
    public void SetLayerNames(string[] layerNames) => this.layerNames = layerNames;

    /// <summary>
    /// Get Layer Ids
    /// </summary>
    public int[] GetLayerIDs() => this.layerIDs;

    /// <summary>
    /// Set Layer IDs
    /// </summary>
    public void SetLayerIDs(int[] layerIDs) => this.layerIDs = layerIDs;

    /// <summary>
    /// Get Layer Ids
    /// </summary>
    public Dictionary<string, int> GetLayerDictionary() => this.layerDictionary;

    /// <summary>
    /// Set Layer IDs
    /// </summary>
    public void SetLayerDictionary(Dictionary<string, int> layerDictionary) => this.layerDictionary = layerDictionary;

    /// <summary>
    /// Set the current layer switch restriction
    /// </summary>
    public void SetLayerSwitchRestriction(int layerSwitchRestriction) => this.layerSwitchRestriction = layerSwitchRestriction;

    /// <summary>
    /// Updates Relevant Layer Info
    /// </summary>
    public void UpdateLayerData()
    {
        this.layerSwitchRestriction = 0;
        this.layerNames = this.GetSortingLayerNames();
        this.layerIDs = this.GetSortingLayerUniqueIDs();
        this.layerDictionary.Clear();

        for (int x = 0; x < this.layerNames.Length; x++)
        {
            this.layerDictionary.Add(this.layerNames[x], this.layerIDs[x]);
        }
    }

    /// <summary>
    /// Gets a list of all the sorting layers within the project
    /// </summary>
    public string[] GetSortingLayerNames()
    {
        Type internalEditorUtilityType = typeof(InternalEditorUtility);
        PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);

        return (string[])sortingLayersProperty.GetValue(null, new object[0]);
    }

    /// <summary>
    /// Gets all he unique ID's of he sorting layers wihin he projec
    /// </summary>
    public int[] GetSortingLayerUniqueIDs()
    {
        Type internalEditorUtilityType = typeof(InternalEditorUtility);
        PropertyInfo sortingLayerUniqueIDsProperty = internalEditorUtilityType.GetProperty("sortingLayerUniqueIDs", BindingFlags.Static | BindingFlags.NonPublic);

        return (int[])sortingLayerUniqueIDsProperty.GetValue(null, new object[0]);
    }

    public int GetSortingLayerIndex(int id)
    {
        return this.layerIDs.Select((v, i) => new { sortingLayerId = v, index = i }).First(x => x.sortingLayerId == id ).index;
    }
}
