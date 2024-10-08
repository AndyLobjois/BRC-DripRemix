<p align="center"><img src="RESOURCES/img/icon.png"></p>

<p align="center"><img src="RESOURCES/img/DripRemix0.gif" width="200"><img src="RESOURCES/img/DripRemix1.gif" width="200"><img src="RESOURCES/img/DripRemix2.gif" width="200"><img src="RESOURCES/img/DripRemix3.gif" width="200"></p>

### Swap Skins and Models for your Character, Gears, Phone & Spraycan directly in-game ! (No Unity required)

- *`Unlimited Skins Swap per Character` → Character models can't be swap, use [CrewBoom](https://thunderstore.io/c/bomb-rush-cyberfunk/p/SoftGoat/CrewBoom/) instead !*
- *`Unlimited Skins/Models Swap for Movestyles/Gears, Phone and Spraycan`*
- *`Custom Color Spray` → Takes bottom left pixel from the texture*
- *`Advance Texture Options` → Emission maps for everything and Phone UI*
- *`Quick Reload In-game`*
- *`Phone Cameras Position/Rotation/FOV Editable`*
- *`Easy Skins/Models Installation & Edit` → Drop .obj and .png/.jpg inside a folder, **no Unity required !***
- *`Custom Binding`*
- *`Save System per Character` → Skin/Model of Character, Gears, Phone and Spraycan*

# HOW TO INSTALL / HOW TO USE

Check the [**Thunderstore Page**](https://lethal-league-blaze.thunderstore.io/c/bomb-rush-cyberfunk/p/AndyLobjois/DripRemix/) for more informations ★

# HOW TO MAKE YOUR OWN SKINS

### <ins>Basic Texture Editing</ins>
- Get your texture or extract the texture from a bundle (Custom Character) with [Asset Studio](https://github.com/Perfare/AssetStudio)
- Edit the texture with any 2D Software like Krita, GIMP, Paint.net or Paint !

### <ins>Custom Color Spray</ins>
- On your Spraycan texture, color the bottom left pixels (3x3)
- The color spray will change from it !

### <ins>Emission Map</ins>
- In BRC, Emission map only affect the shadows, which means you can control a part of the texture to never be (or slightly) affected by shadows
- If you have a texture named `MyTex.png`, just create a new one called `MyTex_Emission.png` and DripRemix will detect it

### <ins>Quick Reload with F5</ins>
- You don't need to restard the game everytime, just press `F5` and select your skin/model !

# HOW TO MAKE YOUR OWN MODELS

- No Unity required !
- Duplicate a folder from your `ModdingFolder` as a template (I recommand my [**Examples.zip**](https://github.com/AndyLobjois/BRC-DripRemix/blob/main/RESOURCES/Examples.zip))
- Download [**Models.zip**](https://github.com/AndyLobjois/BRC-DripRemix/blob/main/RESOURCES/Models.zip)
- Edit the `.blend` of your choice
- <ins>**IMPORTANT**</ins> : Before exporting, `reset location/rotation` of all **parts** ! (Location: 0,0,0, Rotation: 0,0,0)
  - OBJ format doesn't support custom transform, you have to put it "correctly" in place before exporting.
- Export each **parts** in `.obj` format and overwrite the corresponding files **(Don't rename it !)**
   - Export settings:
      - `Selected Only : True`
      - `Forward Axis : +Z`
- Remove the `.mtl` files
- Edit the `info.txt` and put the **name/author**
- Edit/Add as many textures as you want (You can rename them for giving a specific order)
- It's done ! You can launch the game and see from your own eyes !
   - Don't forget you can **reload** your mod by pressing `F5` without exiting the game !

# CREDITS

- **Andy Hellgrim** — Code & Design
- **Glomzubuk** — Code Structure & Save/Binding System

*Thanks LMR_1 for the previous logo !*

# CHANGELOG
- [0.1.0] Gears Swapping (MeshRemix was the old name of the tool)
- [1.0.0] Character Skins, Gears Skins/Models, Phone Skins/Models (with UI), Spraycan Skins/Models (with Color Spray)
- [1.0.1] Fix Missing Folders error, Fix PhoneClosed that doesn't show up, Color Spray added to GraffitiGame
- [1.0.2] Fix Felix missing SkinnedMeshRenderer reference
- [1.0.3] Fix Custom Char. Compatibility, Save System Fixed, Missing refs handlers (logError), Phone "rotation" fixed, New Shortcut for Spraycan (E)
- [1.0.4] Fix Phone Cameras + New feature: cameras pos/rot/fov can be changed from info.txt
- [1.0.5] Preventing errors-lock after wrong installation/updates from user with better logs and being more permissive. Also Custom Character Replacement are now detected and you can force the texture override from the info.txt of the Character Folder.
- [1.0.6] EVEN BETTER LOGS and preventing the plugin from stopping
- [1.0.7] Custom Character Compatible (Replacement/Additional), Auto Folder Structure, Hideout Gears display Custom Mesh/Tex, Skateboard UV/Baskitty UV/Spike BMX fixes, BMX Wheel Texture Correctly Set
- [1.0.8] Save File Location Fix (for R2MM and avoid access violation)
- [1.0.9] Preventing errors after wrong installation
