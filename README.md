# Object Swatch

Object Swatch is a Unity tool that adds a drag/drop tool to the Unity editor that helps place 2D prefabs and sprites on the scene in Unity.

## Usage
### Where the Magic Happens
- The project loads game objects that are placed using the following schema Assets/Resources/[SUB_DIRECTORIES]/[PRIMARY_SWATCH]/[SECONDARY_SWATCH]
#### Opening the Object Swatch Tool
- To begin using this package, visit Window > Prefab Swatch... in the menu bar
- This should open up the prefab swatch window
- The prefab swatch will then load all the prefabs that exist within the `Assets/Resources` directory of the project, but this can be changed to your liking
#### Placing Prefabs
- Once the prefabs within the set directory have been loaded in, you can click any of the prefabs displayed in the window to select them
- With a prefab, you can drop them unto the `Act Grid` by clicking on the scene
#### Placing Sprites
- Similar to prefabs, you can place sprites on the scene as well by selecting the sprite tab and clicking on them
- With  a sprite selected, you can place them unto the `Act Grid` by clicking on the scene
- You also have the option to place a sprite by simply selecting the `Sprite To Draw` option and  clicking the `Start Placing Sprite Option`

## Features
#### While Placing An Object
- Selecting object orientation by 45-degree increments
- Horizontal & Vertical Axis flipping
- Grid Snapping
#### Within the window
- Filtering
- Object Zooming

## Contributing

Pull requests are welcome. For major changes, please open an issue first
to discuss what you would like to change.

## TODO
- Make the magic more magical instead of being hardcoded

## License

[MIT](./LICENSE.md)
