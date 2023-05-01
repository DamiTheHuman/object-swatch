using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

public class ObjectAssetsService<T>
{
    public SwatchWindow swatchWindow;

    protected SwatchTab swatchTab;
    [SerializeField]
    protected List<ObjectSwatch<T>> swatchList = new List<ObjectSwatch<T>>();

    /// <summary> The selected object to be placed within the scene</summary>
    protected T selectedObject;

    /// <summary> The scroll view for the list of objects used for displaying a lengthy list</summary>
    protected Vector2 objectsScrollView;

    /// <summary> The pointer which defines the primary field within the list of objects </summary>
    [SerializeField]
    protected int swatchIndex = 0;

    /// <summary> The previous value of swatch sub index<see cref="swatchIndex"/> </summary>
    protected int prevSwatchIndex = -999;

    /// <summary> The pointer which defines the secondary objects with a <see cref="swatchIndex"/> </summary>
    [SerializeField]
    protected int swatchSubIndex = 0;

    /// <summary> The previous value of swatch sub index<see cref="swatchSubIndex"/> </summary>
    protected int spriteSwatchSubIndex = -999;

    /// <summary> Current search filter </summary>
    protected string spriteSearchFilter = "";

    /// <summary> The names for all the resource directories</summary>
    protected string[] subDirectoryNames;

    /// <summary> The names for all the swatches</summary>
    protected string[] swatchNames;

    /// <summary> The sub names for all the swatches </summary>
    protected string[] swatchSubNames;

    /// <summary> A coroutine used to search for objects </summary>
    public IEnumerator updateSwatchCoroutine;

    /// <summary> The editor coroutine used to references the running coroutine </summary>
    public EditorCoroutine updateSwatchEditorCoroutine;

    public ObjectAssetsService(SwatchWindow SpriteSwatchWindow, SwatchTab swatchTab)
    {
        this.swatchWindow = SpriteSwatchWindow;
        this.swatchTab = swatchTab;
        this.FetchTabData();
    }

    /// <summary>
    /// Updates the swatch names based on the current swatch index
    /// </summary>
    /// 
    public void UpdateSwatchSubNames() => this.swatchSubNames = this.GetDirectoriesWithObjects(this.swatchWindow.GetAssetsDirectory() + "/" + this.subDirectoryNames[this.swatchTab.GetSubDirectoryIndex()] + "/" + this.swatchNames[this.swatchIndex], 4, this.swatchNames[this.swatchIndex]).ToArray();

    /// <summary>
    /// Initialize and set defaults for the object swatch data 
    /// Also manages clamping of object swatch data when changed
    /// </summary>
    public void InitializeSpriteSwatchData()
    {
        if (this.swatchList.Count == 0)
        {
            this.StartUpdateSwatchCoroutine();
        }

        if (this.swatchIndex != this.prevSwatchIndex)
        {
            this.UpdateSwatchSubNames();
            this.prevSwatchIndex = this.swatchIndex;
            this.swatchSubIndex = 0;//Back to the first index
        }

        if (this.swatchSubIndex != this.spriteSwatchSubIndex)
        {
            this.UpdateSwatchSubNames();
            this.spriteSwatchSubIndex = this.swatchSubIndex;
        }
    }

    /// <summary>
    /// Starts the update swatch coroutine to asynchronously search for objects
    /// </summary>
    public void StartUpdateSwatchCoroutine()
    {
        if (this.swatchWindow.GetLoading())
        {
            return;
        }

        this.FetchTabData();
    }


    /// <summary>
    /// Updates the current swatch data
    /// </summary>
    public IEnumerator UpdateSwatchDataCoroutine()
    {
        this.swatchList.Clear();
        this.subDirectoryNames = this.GetMainDirectoryHeaders(this.swatchWindow.GetAssetsDirectory()).ToArray();
        this.swatchNames = this.GetDirectoriesWithObjects(this.swatchWindow.GetAssetsDirectory() + "/" + this.subDirectoryNames[this.swatchTab.GetSubDirectoryIndex()]).ToArray();

        if (this.swatchNames.Length > 0)
        {
            this.UpdateSwatchSubNames();
            this.swatchTab.SetPreviousSubDirectoryIndex(this.swatchTab.GetSubDirectoryIndex());
            this.prevSwatchIndex = -999;
            this.spriteSwatchSubIndex = -999;

            //Save all the sub swatch data per primary swatch
            foreach (ObjectSwatch<T> hCSwatch in this.swatchList)
            {

                if (hCSwatch.primarySwatch != "")
                {
                    hCSwatch.secondarySwatchNames = this.GetDirectoriesWithObjects(this.swatchWindow.GetAssetsDirectory() + "/" + this.subDirectoryNames[this.swatchTab.GetSubDirectoryIndex()] + "/" + "/" + hCSwatch.primarySwatch, 4, hCSwatch.primarySwatch);
                    //List has no secondary swatches but may have primary swatches so fill the primary swatch list
                    if (hCSwatch.secondarySwatchNames.Count == 0)
                    {
                        hCSwatch.primarySwatchObjects = this.LoadAllObjects(hCSwatch.primarySwatch);
                    }
                    else
                    {
                        foreach (string secondarySwatchName in hCSwatch.secondarySwatchNames)
                        {
                            Dictionary<string, List<SwatchData<T>>> secondaryObjects = this.LoadSecondarySwatchObjects(secondarySwatchName, hCSwatch.primarySwatch + "/" + secondarySwatchName);
                            hCSwatch.secondarySwatchObjects.Add(secondaryObjects);
                        }
                    }
                }
            }
        }

        this.swatchWindow.SetLoading(false);

        switch (this.swatchWindow.GetCurrentTab())
        {
            case 0:
                this.swatchWindow.GetObjectsTab().UpdateValues();
                break;
            case 1:
                this.swatchWindow.GetSpritesTab().UpdateValues();
                break;
            default:
                break;

        }

        this.swatchWindow.Repaint();

        yield return null;
    }


    /// <summary>
    /// Get the directory headers of the resource path
    /// <param name="resourcePath"> The path to check for resources in</param>
    /// </summary>
    public List<string> GetMainDirectoryHeaders(string resourcePath)
    {
        int splitPath = 2;
        List<string> directoryHeader = new List<string>();

        foreach (string subFolderDirectory in AssetDatabase.GetSubFolders(resourcePath))
        {
            string resourceDirectoryName = subFolderDirectory.Split(new string[] { "/" }, StringSplitOptions.None)[splitPath];
            //Only add a resource to the list if it contains a directory name
            if (Resources.LoadAll(resourceDirectoryName, typeof(T)).Length != 0)
            {
                directoryHeader.Add(resourceDirectoryName);
            }
        }

        return directoryHeader;
    }

    /// <summary>
    /// Get the directory headers of the resource path
    /// <param name="resourcePath">The path to check for resources in</param>
    /// <param name="splitPath">How many paths down to check for </param>
    /// <param name="secondaryPath">The secondary path to gather data for </param>
    /// </summary>
    public List<string> GetDirectoriesWithObjects(string resourcePath, int splitPath = 3, string secondaryPath = "")
    {
        List<string> directoryHeader = new List<string>();

        foreach (string subFolderDirectory in AssetDatabase.GetSubFolders(resourcePath))
        {
            string resourceDirectoryName = subFolderDirectory.Split(new string[] { "/" }, StringSplitOptions.None)[splitPath];
            string directoryToLoad = this.subDirectoryNames[this.swatchTab.GetSubDirectoryIndex()] + "/" + (secondaryPath == "" ? resourceDirectoryName : secondaryPath + "/" + resourceDirectoryName);
            //Only add a resource to the list if it contains a directory name
            if (Resources.LoadAll(directoryToLoad, typeof(T)).Length != 0)
            {
                //Save the primary swatch data
                if (secondaryPath == "")
                {
                    ObjectSwatch<T> newSwatch = new ObjectSwatch<T>
                    {
                        primarySwatch = resourceDirectoryName
                    };

                    this.swatchList.Add(newSwatch);
                }

                directoryHeader.Add(resourceDirectoryName);
            }
        }

        return directoryHeader;
    }

    /// <summary>
    /// Fetch relevant tab data pertaining to the assets in our project
    /// </summary>
    public void FetchTabData()
    {
        this.updateSwatchCoroutine = this.UpdateSwatchDataCoroutine();
        this.updateSwatchEditorCoroutine = EditorCoroutineUtility.StartCoroutine(this.updateSwatchCoroutine, this);
    }

    /// <summary>
    /// Gets the Objects that belong to the secondary swatch
    /// <param name="parent"> The parent or key of the objects</param>
    /// <param name="dir"> The directory to search for the objects</param>
    /// </summary>
    public Dictionary<string, List<SwatchData<T>>> LoadSecondarySwatchObjects(string parent, string dir)
    {
        T[] objects = Resources.LoadAll(this.subDirectoryNames[this.swatchTab.GetSubDirectoryIndex()] + "/" + dir, typeof(T))
                        .Cast<T>()
                        .ToArray();

        List<SwatchData<T>> swatchDataObjects = new List<SwatchData<T>>();

        foreach (T genericObject in objects)
        {
            swatchDataObjects.Add(new SwatchData<T>(genericObject, parent));
        }

        Dictionary<string, List<SwatchData<T>>> result = new Dictionary<string, List<SwatchData<T>>>
        {
            { parent, swatchDataObjects.ToList() }
        };

        return result;
    }

    /// <summary>
    /// Load all the Objects within a directory
    /// <param name="dir"> The directory to search for the objects</param>
    /// </summary>
    public List<SwatchData<T>> LoadAllObjects(string dir)
    {
        List<T> objects = Resources.LoadAll(this.subDirectoryNames[this.swatchTab.GetSubDirectoryIndex()] + dir, typeof(T))
                       .Cast<T>()
                       .ToList();

        List<SwatchData<T>> swatchDataObjcets = null;

        foreach (T genericObject in objects)
        {
            swatchDataObjcets.Add(new SwatchData<T>(genericObject, this.subDirectoryNames[this.swatchTab.GetSubDirectoryIndex()]));
        }

        return swatchDataObjcets;
    }

    /// <summary>
    /// Gets the list of swatches
    /// </summary>
    public List<ObjectSwatch<T>> GetSwatchList() => this.swatchList;

    /// <summary>
    /// No Objects found
    /// </summary>
    public bool HasNoObjects() => this.swatchNames.Length == 0;

    /// <summary>
    /// Get Sub Directory Names
    /// </summary>
    public string[] GetSubDirectoryNames() => this.subDirectoryNames;

    /// <summary>
    /// Get Swatch Sub Names
    /// </summary>
    public string[] GetSwatchSubNames() => this.swatchSubNames;

    /// <summary>
    /// Get the current swatch index the user is looking into
    /// </summary>
    public int GetSwatchIndex() => this.swatchIndex;

    /// <summary>
    /// Set the current swatch index
    /// <param name="swatchIndex">The new value for <see cref="swatchIndex"/></param>
    /// </summary>
    public int SetSwatchIndex(int swatchIndex) => this.swatchIndex = swatchIndex;

    /// <summary>
    /// Get the current swatch sub/secondary index the user is looking tno
    /// </summary>
    public int GetSwatchSubIndex() => this.swatchSubIndex;

    /// <summary>
    /// Set the value for the swatch sub index
    /// <param name="swatchSubIndex">The new value for <see cref="swatchSubIndex"/></param>
    /// </summary>
    public int SetSwatchSubIndex(int swatchSubIndex) => this.swatchSubIndex = swatchSubIndex;

    /// <summary>
    /// Get the value for our search filter
    /// </summary>
    public string GetSearchFilter() => this.spriteSearchFilter;

    /// <summary>
    /// Set the value for the search filter
    /// <param name="searchFilter">The new value for <see cref="searchFilter"/></param>
    /// </summary>
    public void SetSearchFilter(string searchFilter)
    {
        if (searchFilter != this.spriteSearchFilter)
        {
            this.swatchWindow.GetSpritesTab().UpdateValues();
            this.swatchWindow.GetObjectsTab().UpdateValues();
        }

        this.spriteSearchFilter = searchFilter;
    }

    /// <summary>
    /// Get the current swatch names based on the swatch index
    /// </summary>
    public string[] GetCurrentSwatchNames() => this.GetSwatchList().Where(sw => sw.primarySwatch != null).Select(sw => sw.primarySwatch).ToArray();

    /// <summary>
    /// Get the current sub swatch names based on the current index
    /// </summary>
    public string[] GetCurrentSwatchSubNames() => this.GetSwatchList()[this.GetSwatchIndex()].GetSubSwatchList();
}
