using System.Collections.Generic;

public class ObjectSwatch<T>
{
    /// <summary> The primary swatch to examine</summary>
    public string primarySwatch = "";

    /// <summary> Objects found in the primary swatch </summary>
    public List<SwatchData<T>> primarySwatchObjects = new List<SwatchData<T>>();

    /// <summary> The list of secondary swatch names </summary>
    public List<string> secondarySwatchNames = new List<string>();

    /// <summary> Objects of secondary swatches to the primary  </summary>
    public List<Dictionary<string, List<SwatchData<T>>>> secondarySwatchObjects = new List<Dictionary<string, List<SwatchData<T>>>>();

    /// <summary>
    /// Gets the list of sub swatches within the swatch listlist
    /// </summary>
    public string[] GetSubSwatchList() => this.secondarySwatchNames.ToArray();
}

