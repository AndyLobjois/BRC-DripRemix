using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using Reptile;
using Reptile.Phone;
using OBJImporter;
using DripRemix.Handlers;

namespace DripRemix {

    [BepInPlugin(Infos.PLUGIN_ID, Infos.PLUGIN_NAME, Infos.PLUGIN_VERSION)]
    [BepInProcess("Bomb Rush Cyberfunk.exe")]
    public class Main : BaseUnityPlugin {

        public static Main Instance;
        internal static ManualLogSource Log { get; private set; }

        // Folders
        internal static DirectoryInfo FolderModding = Directory.CreateDirectory(Path.Combine(Paths.GameRootPath, "ModdingFolder", "BRC-DripRemix"));
        internal static DirectoryInfo FolderCharacter = FolderModding.CreateSubdirectory("Characters");
        internal static DirectoryInfo FolderGears = FolderModding.CreateSubdirectory("Gears");
        internal static DirectoryInfo FolderBMX = FolderGears.CreateSubdirectory("Bmx");
        internal static DirectoryInfo FolderInline = FolderGears.CreateSubdirectory("Inline");
        internal static DirectoryInfo FolderSkateboard = FolderGears.CreateSubdirectory("Skateboard");
        internal static DirectoryInfo FolderPhone = FolderGears.CreateSubdirectory("Phones");
        internal static DirectoryInfo FolderSpraycan = FolderGears.CreateSubdirectory("Spraycans");
        //internal static DirectoryInfo FolderGraffiti = ModdingFolder.CreateSubdirectory("Graffiti");

        // Inputs
        internal ConfigEntry<KeyCode> keyCharacter;
        internal ConfigEntry<KeyCode> keyGear;
        internal ConfigEntry<KeyCode> keyPhone;
        internal ConfigEntry<KeyCode> keySpraycan;
        internal ConfigEntry<KeyCode> keyReload;
        internal ConfigEntry<KeyCode> keyMeshUp;
        internal ConfigEntry<KeyCode> keyMeshDown;
        internal ConfigEntry<KeyCode> keyTextureUp;
        internal ConfigEntry<KeyCode> keyTextureDown;

        // References
        public static Save SAVE = new Save();
        public int HASH;
        public bool GRAFFITIGAME_EDITED = false;
        public GameObject PLAYER;
        public CharacterVisual PLAYER_VISUAL;
        public Phone PLAYER_PHONE;
        public Dictionary<string, Material> MATERIALS = new Dictionary<string, Material>();

        public static string CURRENTCHARACTER;
        public CharacterHandler CHARACTER;
        public Dictionary<MoveStyle, GearHandler> GEARS = new Dictionary<MoveStyle, GearHandler>();
        public PhoneHandler PHONES;
        public SpraycanHandler SPRAYCANS;
        //public GraffitiHandler GRAFFITI = new GraffitiHandler();

        public enum Check { Character, Gear, Phone, Spraycan }
        public Dictionary<string, string> ConvertNames = new Dictionary<string, string>()
        {
            {"girl1" , "Vinyl"},
            {"frank" , "Frank" },
            {"ringdude" , "Coil"},
            {"metalHead" , "Red"},
            {"blockGuy" , "Tryce"},
            {"spaceGirl" , "Bel"},
            {"angel" , "Rave"},
            {"eightBall" , "DOT EXE"},
            {"dummy" , "Solace"},
            {"dj" , "DJ Cyber"},
            {"medusa" , "Eclipse"},
            {"boarder" , "Devil Theory"},
            {"headMan" , "Faux"},
            {"prince" , "Flesh Prince"},
            {"jetpackBossPlayer" , "Irene Rietveld"},
            {"legendFace" , "Felix"},
            {"oldheadPlayer" , "Oldhead"},
            {"robot" , "Base"},
            {"skate" , "Jay"},
            {"wideKid" , "Mesh"},
            {"futureGirl" , "Futurism"},
            {"pufferGirl" , "Rise"},
            {"bunGirl" , "Shine"},
            {"headManNoJetpack" , "Faux (Prelude)"},
            {"eightBallBoss" , "DOT EXE (Boss)"},
            {"legendMetalHead" , "Red Felix (Dream)"},
        };

        void Awake() {
            Instance = this;
            Log = this.Logger;

            // Init Key Inputs
            keyReload = Config.Bind("Keybinds", "Reload", KeyCode.F5); // Is there a way to display AcceptableValues only for the first input ?
            keyCharacter = Config.Bind("Keybinds", "CharacterKey", KeyCode.C);
            keyGear = Config.Bind("Keybinds", "GearKey", KeyCode.G);
            keyPhone = Config.Bind("Keybinds", "PhoneKey", KeyCode.P);
            keySpraycan = Config.Bind("Keybinds", "SpraycanKey", KeyCode.E);
            keyMeshUp = Config.Bind("Keybinds", "MeshUP", KeyCode.Home);
            keyMeshDown = Config.Bind("Keybinds", "MeshDOWN", KeyCode.End);
            keyTextureUp = Config.Bind("Keybinds", "TextureUP", KeyCode.PageUp);
            keyTextureDown = Config.Bind("Keybinds", "TextureDOWN", KeyCode.PageDown);
            Config.SaveOnConfigSet = true;

            // Init Folders, Lists and Saved Indexes
            CHARACTER = new CharacterHandler();
            GEARS.Add(MoveStyle.INLINE, new GearHandler(MoveStyle.INLINE));
            GEARS.Add(MoveStyle.SKATEBOARD, new GearHandler(MoveStyle.SKATEBOARD));
            GEARS.Add(MoveStyle.BMX, new GearHandler(MoveStyle.BMX));
            PHONES = new PhoneHandler();
            SPRAYCANS = new SpraycanHandler();

            // Link Main to the Save System
            SAVE.main = this;
        }

        void LateUpdate() {
            if (WorldHandler.instance?.currentPlayer?.gameObject) {
                if (HASH != WorldHandler.instance.currentPlayer.characterVisual.GetHashCode()) {
                    HASH = WorldHandler.instance.currentPlayer.characterVisual.GetHashCode();

                    // Get Player and Name Reference
                    try {
                        PLAYER = WorldHandler.instance?.currentPlayer.gameObject;

                        // Get Character Name/ID
                        string nameClean = WorldHandler.instance.currentPlayer.characterVisual.name.Replace(" ", "").Replace("Visuals(Clone)", "");
                        if (ConvertNames.ContainsKey(nameClean)) {
                            CURRENTCHARACTER = ConvertNames[nameClean];
                        } else {
                            CURRENTCHARACTER = nameClean;
                        }
                    } catch {
                        Log.LogError("Player can't be referenced !");
                    }

                    // Save
                    SAVE.GetSave();

                    // Get Everything
                    GetReferences();
                    GetAssets();

                    // Apply Assets
                    Apply();

                    // Init Materials
                    GetMaterials();

                    // End Mark
                    Log.LogMessage($"————————————————————————————————————————— {RandomLine()}");
                }

                if (WorldHandler.instance.currentPlayer.inGraffitiGame) {
                    SPRAYCANS.SetGraffitiEffect();
                }
            }
        }

        void Update() {
            // Inputs
            CheckInput(keyCharacter.Value, Check.Character);
            CheckInput(keyGear.Value, Check.Gear);
            CheckInput(keyPhone.Value, Check.Phone);
            CheckInput(keySpraycan.Value, Check.Spraycan);

            if (Input.GetKeyDown(keyReload.Value)) {
                GetReferences();
                GetAssets();
                Apply();

                // End Mark
                Log.LogMessage($"————————————————————————————————————————— {RandomLine()}");
            }
        }

        void CheckInput(KeyCode key, Check check) {
            if (Input.GetKey(key)) {
                if (Input.GetKeyDown(keyMeshUp.Value)) { // Mesh Swap -
                    SetMesh(check, -1);
                }
                if (Input.GetKeyDown(keyMeshDown.Value)) { // Mesh Swap +
                    SetMesh(check, +1);
                }
                if (Input.GetKeyDown(keyTextureUp.Value)) { // Texture Swap -
                    SetTexture(check, -1);
                }
                if (Input.GetKeyDown(keyTextureDown.Value)) {// Texture Swap +
                    SetTexture(check, +1);
                }
            }
        }

        void SetMesh(Check check, int add) {
            if (check == Check.Character) {
                //CHARACTERS[WorldHandler.instance.currentPlayer.character].SetGear(add);
                // One day ...
            }

            if (check == Check.Gear) {
                GEARS[WorldHandler.instance.currentPlayer.moveStyleEquipped].SetMesh(add);
            }

            if (check == Check.Phone) {
                PHONES.SetMesh(add);
            }

            if (check == Check.Spraycan) {
                SPRAYCANS.SetMesh(add);
            }

            // Save
            SAVE.SetSave();
        }

        void SetTexture(Check check, int add) {
            if (check == Check.Character) {
                CHARACTER.SetTexture(add);
                //CHARACTERS[WorldHandler.instance.currentPlayer.character].SetTexture(add);
            }

            if (check == Check.Gear) {
                GEARS[WorldHandler.instance.currentPlayer.moveStyleEquipped].SetTexture(add);
            }

            if (check == Check.Phone) {
                PHONES.SetTexture(add);
            }

            if (check == Check.Spraycan) {
                SPRAYCANS.SetTexture(add);
            }

            // Save
            SAVE.SetSave();
        }

        void GetReferences() {
            // Clear References
            CHARACTER.REFERENCES.Clear();
            foreach (GearHandler handler in GEARS.Values)
                handler.REFERENCES.Clear();
            PHONES.REFERENCES.Clear();
            SPRAYCANS.REFERENCES.Clear();
            MATERIALS.Clear();

            /* PLAYER --------------------------------------------------------------------------------------------- */
            // Character Visual Slot
            try {
                PLAYER_VISUAL = WorldHandler.instance.currentPlayer.characterVisual;
            } catch {
                Log.LogError("Player.CharacterVisual can't be referenced !");
            }

            // Character
            foreach (Transform child in PLAYER_VISUAL.characterObject.transform) {
                try {
                    if (child.GetComponent<SkinnedMeshRenderer>()) {
                        CHARACTER.REFERENCES.Add(child.gameObject);
                    }
                } catch {
                    Log.LogError("Character SkinnedMeshRenderer can't be referenced !");
                }
            }

            // Player Skateboard
            try {
                GEARS[MoveStyle.SKATEBOARD].REFERENCES.Add(PLAYER_VISUAL.moveStyleProps.skateboard);
            } catch {
                Log.LogError("Player.Skateboard Gameobject can't be referenced !");
            }

            // Player Inlines
            try {
                GEARS[MoveStyle.INLINE].REFERENCES.Add(PLAYER_VISUAL.moveStyleProps.skateL);
                GEARS[MoveStyle.INLINE].REFERENCES.Add(PLAYER_VISUAL.moveStyleProps.skateR);
            } catch {
                Log.LogError("Player Inline Gameobject(s) can't be referenced !");
            }

            // Player BMX
            try {
                GEARS[MoveStyle.BMX].REFERENCES.Add(PLAYER_VISUAL.moveStyleProps.bmxFrame);
                GEARS[MoveStyle.BMX].REFERENCES.Add(PLAYER_VISUAL.moveStyleProps.bmxGear);
                GEARS[MoveStyle.BMX].REFERENCES.Add(PLAYER_VISUAL.moveStyleProps.bmxHandlebars);
                GEARS[MoveStyle.BMX].REFERENCES.Add(PLAYER_VISUAL.moveStyleProps.bmxPedalL);
                GEARS[MoveStyle.BMX].REFERENCES.Add(PLAYER_VISUAL.moveStyleProps.bmxPedalR);
                GEARS[MoveStyle.BMX].REFERENCES.Add(PLAYER_VISUAL.moveStyleProps.bmxWheelF);
                GEARS[MoveStyle.BMX].REFERENCES.Add(PLAYER_VISUAL.moveStyleProps.bmxWheelR);
            } catch {
                Log.LogError("Player BMX Gameobject(s) can't be referenced !");
            }

            // Phone Slot
            try {
                PLAYER_PHONE = WorldHandler.instance.currentPlayer.phone;
            } catch {
                Log.LogError("Player.Phone can't be referenced !");
            }

            // Phone
            try {
                PHONES.REFERENCES.Add(PLAYER_VISUAL.handL.Find("propl/phoneInHand(Clone)").gameObject);
                PHONES.REFERENCES.Add(PLAYER_PHONE.openPhoneCanvas.transform.Find("PhoneContainerOpen/PhoneOpen").gameObject);
                PHONES.REFERENCES.Add(PLAYER_PHONE.closedPhoneCanvas.transform.Find("PhoneContainerClosed/PhoneClosed").gameObject);
            } catch {
                Log.LogError("Phone Gameobject(s) can't be referenced !");
            }

            // Spraycan
            try {
                SPRAYCANS.REFERENCES.Add(PLAYER_VISUAL.handR.Find("propr/spraycan(Clone)").gameObject);
            } catch {
                Log.LogError("Spraycan Gameobject(s) can't be referenced !");
            }

            // Spraycan Caps
            GameObject[] RootGameObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject go in RootGameObjects) {
                if (go.name == "spraycanCapJunk(Clone)") {
                    try {
                        SPRAYCANS.REFERENCES.Add(go);
                    } catch {
                        Log.LogError("Spraycan Cap Gameobject(s) can't be referenced !");
                    }
                }
            }

            
            /* HIDEOUT --------------------------------------------------------------------------------------------- */
            NPC[] GearSpotsInHideout = GameObject.FindObjectsOfType<NPC>();

            // Hideout Inlines Spot
            try {
                foreach (var spot in GearSpotsInHideout) {
                    if (spot.name == "NPC_MovestyleChangerInline") {

                        // Check if these gameobject exist and haven't been renamed yet
                        if (spot.transform.Find("SkatesPreview/skateLeft")) {
                            spot.transform.Find("SkatesPreview/skateLeft").name = "skateLeft(Clone)";
                            spot.transform.Find("SkatesPreview/skateRight").name = "skateRight(Clone)";
                        }

                        GEARS[MoveStyle.INLINE].REFERENCES.Add(spot.transform.Find("SkatesPreview").GetChild(0).gameObject);
                        GEARS[MoveStyle.INLINE].REFERENCES.Add(spot.transform.Find("SkatesPreview").GetChild(1).gameObject);
                    }
                }
            } catch {
                Log.LogError("Hideout Inline Gameobject(s) can't be referenced !");
            }

            // Hideout Skateboard Spot
            try {
                foreach (var spot in GearSpotsInHideout) {
                    if (spot.name == "NPC_MovestyleChangerSkateboard") {

                        // Check if these gameobject exist and haven't been renamed yet
                        if (spot.transform.Find("PreviewSkateboard/skateboard")) {
                            spot.transform.Find("PreviewSkateboard/skateboard").name = "skateboard(Clone)";
                            spot.transform.Find("PreviewSkateboard/skateboard (1)").name = "skateboard(Clone)";
                        }

                        GEARS[MoveStyle.SKATEBOARD].REFERENCES.Add(spot.transform.Find("PreviewSkateboard").GetChild(0).gameObject);
                        GEARS[MoveStyle.SKATEBOARD].REFERENCES.Add(spot.transform.Find("PreviewSkateboard").GetChild(1).gameObject);
                    }
                }
            } catch {
                Log.LogError("Hideout Skateboard Gameobject(s) can't be referenced !");
            }

            // Hideout BMX Spot
            try {
                foreach (var spot in GearSpotsInHideout) {
                    if (spot.name == "NPC_MovestyleChangerBMX") {
                        GEARS[MoveStyle.BMX].REFERENCES.Add(spot.transform.Find("BMXPreview/BmxFrame(Clone)").gameObject);
                        GEARS[MoveStyle.BMX].REFERENCES.Add(spot.transform.Find("BMXPreview/bmxGear/BmxGear(Clone)").gameObject);
                        GEARS[MoveStyle.BMX].REFERENCES.Add(spot.transform.Find("BMXPreview/bmxHandlebars/BmxHandlebars(Clone)").gameObject);
                        GEARS[MoveStyle.BMX].REFERENCES.Add(spot.transform.Find("BMXPreview/bmxGear/bmxPedalL/BmxPedalL(Clone)").gameObject);
                        GEARS[MoveStyle.BMX].REFERENCES.Add(spot.transform.Find("BMXPreview/bmxGear/bmxPedalR/BmxPedalR(Clone)").gameObject);
                        GEARS[MoveStyle.BMX].REFERENCES.Add(spot.transform.Find("BMXPreview/bmxHandlebars/bmxWheelF/BmxWheelF(Clone)").gameObject);
                        GEARS[MoveStyle.BMX].REFERENCES.Add(spot.transform.Find("BMXPreview/bmxWheelR/BmxWheelR(Clone)").gameObject);
                    }
                }  
            } catch {
                Log.LogError("Hideout BMX Gameobject(s) can't be referenced !");
            }
        }

        void GetAssets() {
            CHARACTER.GetAssets();
            foreach (GearHandler handler in GEARS.Values)
                handler.GetAssets();
            PHONES.GetAssets();
            SPRAYCANS.GetAssets();
        }

        void GetMaterials() {
            if (Core.instance.Assets) {
                Bundle bundle = Core.instance.Assets.availableBundles["common_assets"];

                MATERIALS.Add("Character", new Material(bundle.assetBundle.LoadAsset<Material>("pedestrianMat").shader));
                MATERIALS.Add("Environment", new Material(bundle.assetBundle.LoadAsset<Material>("Hideout_Buildings03AtlasMat").shader));
                MATERIALS.Add("TransparentCutout", new Material(bundle.assetBundle.LoadAsset<Material>("Hideout_PropsAtlasMat").shader));
                MATERIALS.Add("TransparentUnlit", new Material(bundle.assetBundle.LoadAsset<Material>("unlitTransparentRed").shader));
                MATERIALS.Add("SpriteAnimation", bundle.assetBundle.LoadAsset<Material>("AnimBillboardAtlas"));
            }
        }

        void Apply() {
            CHARACTER.Apply();
            // I apply on every movestyle instead for QuickSwap compatibility. Between stages, non-used gears will not be initialized.
            //GEARS[WorldHandler.instance.currentPlayer.moveStyleEquipped].Apply();
            GEARS[MoveStyle.INLINE].Apply();
            GEARS[MoveStyle.SKATEBOARD].Apply();
            GEARS[MoveStyle.BMX].Apply();
            PHONES.Apply();
            SPRAYCANS.Apply();
        }

        string RandomLine() {
            string[] lines = new string[] {
                "Dig it !",
                "Yo What's up ?",
                "OPERATEOPERATEOPERATE",
                "SHAKE DAT ASS ASS ASS",
                "You Degenerate",
                "Better Watch Ya Back !",
                "Get REP man",
                "All City King !",
                "Pretty Boy",
                "Easy Prey",
                "Back to the daily grind !",
                "How could I looose ?",
                "The critical error, was you.",
                "Have a Jawbreaker !",
                "Feel the Beat !",
                "Let's Roll !",
                "YAAAAAHH",
                "So sorry. It's just... business.",
                "Shiny Cuff !",
                "Let's Boogie !"
            };
            return lines[UnityEngine.Random.Range(0, lines.Length)];
        }
    }
}
