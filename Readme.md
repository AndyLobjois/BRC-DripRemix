# BRC-MeshRemix

> *Load multiple custom models and* s*witch inlines/skateboard/BMX directly in-game.*
> 

![meshremix_gif.gif](RESOURCES/meshremix_gif.gif)

# INSTALL

### R2ModManager

- Install R2ModManager and download BRC-MeshRemix and the dependencies.
- Launch the game at least once for auto-configuration

### Manually

- Install BepInEx
- Download **BRC-MeshRemix** (”Manually” button) from Thunderstore
- Unzip BRC-MeshRemix.zip into **BepInEx Plugins folder**
- Launch the game at least once for auto-configuration

# ADD BUNDLES

- Download Examples.zip
- Find this folder inside your BombRushCyberfunk application: `.../BombRushCyberfunk/ModdingFolder/BRC-MeshRemix/Gears/`
- Open the examples.zip and drop some bundles into corresponding folders
- Launch the game, all the bundles will be loaded
- When you are in game, enable your movestyle and press **PageUp / PageDown** of your keyboard for scrolling through your bundles

# CREATE BUNDLES

- Create a **New Project** with **Unity 2021.3.27f**
- Drop **MeshRemixUnityProject.unitypackage** into **Unity** and import all assets from the package
- Open a **.blend file** from **BlenderFiles.zip** and make your edits (for non-Blender users, there is .fbx files inside the archive)
- Export each mesh inside the Unity project, over the corresponding FBX files (keep the names untouched)
- **Rename** the bundle name from the assets
    - Suggestion: Put your nickname in the name like this *“mycustombmx_hellgrim.bmx”*, it’ll avoid same-name conflict
    - You can add the .inline/ .skateboard / .bmx at the end of the bundle, it’s just for orgagnization purpose
- **Build** the bundle(s) with **File > Build AssettBundles**. Congratulation, you have your bundle(s) !