using UnityEngine;

public class PrefabAssetsService : ObjectAssetsService<GameObject>
{
    public PrefabAssetsService(SwatchWindow swatchWindow, SwatchTab swatchTab): base(swatchWindow, swatchTab)
    {
        this.swatchWindow = swatchWindow;
        this.swatchTab = swatchTab;
        this.FetchTabData();
    }

}
