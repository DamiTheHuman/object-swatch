using UnityEngine;

public class SpriteAssetsService : ObjectAssetsService<Sprite>
{
    public SpriteAssetsService(SwatchWindow swatchWindow, SwatchTab swatchTab) : base(swatchWindow, swatchTab)
    {
        this.swatchWindow = swatchWindow;
        this.swatchTab = swatchTab;
        this.FetchTabData();
    }

}
