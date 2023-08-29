using System;
using System.IO;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using Reptile;

// TODO:
// - Figure out Character swapping

namespace MeshRemix {

    [BepInPlugin(MeshRemixInfos.PLUGIN_ID, MeshRemixInfos.PLUGIN_NAME, MeshRemixInfos.PLUGIN_VERSION)]
    [BepInProcess("Bomb Rush Cyberfunk.exe")]
    public class MeshRemix : BaseUnityPlugin {

        public static MeshRemix Instance;
        internal static DirectoryInfo ModdingFolder = Directory.CreateDirectory(Path.Combine(Paths.GameRootPath, "ModdingFolder", "BRC-MeshRemix"));
        internal static DirectoryInfo GEARFOLDER = ModdingFolder.CreateSubdirectory("Gears");
        internal static ManualLogSource Log { get; private set; }

        internal ConfigEntry<KeyCode> switchGearUpKey;
        internal ConfigEntry<KeyCode> switchGearDownKey;
        internal ConfigEntry<KeyCode> reloadGearKey;

        // CORE
        public int HASH;
        public GameObject PLAYER;
        public MoveStyle CURRENTGEAR;
        public Dictionary<MoveStyle, GearHandler> GEARS = new Dictionary<MoveStyle, GearHandler>();


        void Awake() {
            Instance = this;
            Log = this.Logger;
            log("MeshRemix is loaded !");
            switchGearUpKey = Config.Bind("Keybinds", "SwitchModelUp", KeyCode.PageUp);
            switchGearDownKey = Config.Bind("Keybinds", "SwitchModelDown", KeyCode.PageDown);
            reloadGearKey = Config.Bind("Keybinds", "ReloadGear", KeyCode.F8);

            GEARS.Add(MoveStyle.INLINE, new GearHandler(MoveStyle.INLINE));
            GEARS.Add(MoveStyle.SKATEBOARD, new GearHandler(MoveStyle.SKATEBOARD));
            GEARS.Add(MoveStyle.BMX, new GearHandler(MoveStyle.BMX));


            // Get Bundles
            foreach (GearHandler gh in GEARS.Values)
            {
                gh.GetBundles();
            }
        }

        void LateUpdate() {
            if (WorldHandler.instance?.currentPlayer?.gameObject) {
                if (HASH != WorldHandler.instance.currentPlayer.characterVisual.GetHashCode()) {
                    HASH = WorldHandler.instance.currentPlayer.characterVisual.GetHashCode();

                    // Get All References
                    PLAYER = WorldHandler.instance?.currentPlayer.gameObject;
                    log("Player has been found!");

                    ReloadRefs();
                }
            }
        }

        void Update() {
            // Inputs
            if (Input.GetKeyDown(switchGearUpKey.Value))
                SetGear(-1);

            if (Input.GetKeyDown(switchGearDownKey.Value))
                SetGear(+1);

            if (Input.GetKeyDown(reloadGearKey.Value))
                ReloadGear();
        }

        void ReloadGear()
        {
            foreach (GearHandler gh in GEARS.Values)
            {
                gh.GetBundles();
            }
            ReloadRefs();
        }

        void ReloadRefs()
        {
            // Clear References
            foreach (GearHandler gh in GEARS.Values)
            {
                gh.ClearRefs();
            }
            GetReferences(PLAYER.transform);
            log("References have been collected !");

            // Apply the new Assets
            SetGear(0);
        }

        void GetReferences(Transform parent, int level = 0) { // Recursive Search Function
            foreach (Transform child in parent) {
                // Characters
                //...

                // Gears
                if (child.name == "skateLeft(Clone)" || child.name == "skateRight(Clone)") {
                    GEARS[MoveStyle.INLINE].AddReference(child.gameObject, child.GetComponent<MeshRenderer>().material.mainTexture);
                }

                if (child.name == "skateboard(Clone)")
                {
                    GEARS[MoveStyle.SKATEBOARD].AddReference(child.gameObject, child.GetComponent<MeshRenderer>().material.mainTexture);
                }

                if (child.name == "BmxFrame(Clone)" ||
                    child.name == "BmxGear(Clone)" ||
                    child.name == "BmxHandlebars(Clone)" ||
                    child.name == "BmxPedalL(Clone)" ||
                    child.name == "BmxPedalR(Clone)" ||
                    child.name == "BmxWheelF(Clone)" ||
                    child.name == "BmxWheelR(Clone)") {
                    GEARS[MoveStyle.BMX].AddReference(child.gameObject, child.GetComponent<MeshRenderer>().material.mainTexture);
                }

                // Process next deeper level
                GetReferences(child, level + 1);
            }
        }

        void SetGear(int add) {
            MoveStyle currentStyle = WorldHandler.instance.currentPlayer.moveStyleEquipped;
            GEARS[currentStyle].SetGear(add);
        }


        static public void log(string message) {
            Debug.Log($"<color=orange>[MeshRemix] {message}</color>");
        }
    }
}
