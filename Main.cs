using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        internal static DirectoryInfo ModdingFolder = Directory.CreateDirectory(Path.Combine(Paths.GameRootPath, "ModdingFolder", "BRC-DripRemix"));
        internal static DirectoryInfo CharactersFolder = ModdingFolder.CreateSubdirectory("Characters");
        internal static DirectoryInfo GearsFolder = ModdingFolder.CreateSubdirectory("Gears");
        internal static DirectoryInfo BMXFolder = GearsFolder.CreateSubdirectory("Bmx");
        internal static DirectoryInfo InlineFolder = GearsFolder.CreateSubdirectory("Inline");
        internal static DirectoryInfo SkateboardFolder = GearsFolder.CreateSubdirectory("Skateboard");
        internal static DirectoryInfo PhonesFolder = GearsFolder.CreateSubdirectory("Phones");
        internal static DirectoryInfo SpraycansFolder = GearsFolder.CreateSubdirectory("Spraycans");
        //internal static DirectoryInfo GraffitiFolder = ModdingFolder.CreateSubdirectory("Graffiti");

        // Inputs
        internal ConfigEntry<KeyCode> characterKey;
        internal ConfigEntry<KeyCode> gearKey;
        internal ConfigEntry<KeyCode> phoneKey;
        internal ConfigEntry<KeyCode> spraycanKey;
        internal ConfigEntry<KeyCode> reloadKey;
        internal ConfigEntry<KeyCode> meshUpKey;
        internal ConfigEntry<KeyCode> meshDownKey;
        internal ConfigEntry<KeyCode> textureUpKey;
        internal ConfigEntry<KeyCode> textureDownKey;

        // References
        public int HASH;
        public bool GRAFFITIGAME_EDITED = false;
        public GameObject PLAYER;
        public CharacterVisual PLAYER_VISUAL;
        public Phone PLAYER_PHONE;
        public enum Check { Character, Gear, Phone, Spraycan }

        public CharacterHandler CHARACTER;
        public Dictionary<MoveStyle, GearHandler> GEARS = new Dictionary<MoveStyle, GearHandler>();
        public PhoneHandler PHONES;
        public SpraycanHandler SPRAYCANS;
        //public GraffitiHandler GRAFFITI = new GraffitiHandler();

        public Dictionary<string, Material> MATERIALS = new Dictionary<string, Material>();
        public Dictionary<Characters, ConfigEntry<HandlersConfig>> SavedIndexes = new Dictionary<Characters, ConfigEntry<HandlersConfig>>();
        /*public static Dictionary<Characters, string> CHARACTERMAPS = new Dictionary<Characters, string>() {
            // Added by Characters.list order
            [Characters.girl1] = "Vinyl",
            [Characters.frank] = "Frank",
            [Characters.ringdude] = "Coil",
            [Characters.metalHead] = "Red",
            [Characters.blockGuy] = "Tryce",
            [Characters.spaceGirl] = "Bel",
            [Characters.angel] = "Rave",
            [Characters.eightBall] = "DOT EXE",
            [Characters.dummy] = "Solace",
            [Characters.dj] = "DJ Cyber",
            [Characters.medusa] = "Eclipse",
            [Characters.boarder] = "Devil Theory",
            [Characters.headMan] = "Faux",
            [Characters.prince] = "Flesh Prince",
            [Characters.jetpackBossPlayer] = "Irene Rietveld",
            [Characters.legendFace] = "Felix",
            [Characters.oldheadPlayer] = "Oldhead",
            [Characters.robot] = "Base",
            [Characters.skate] = "Jay",
            [Characters.wideKid] = "Mesh",
            [Characters.futureGirl] = "Futurism",
            [Characters.pufferGirl] = "Rise",
            [Characters.bunGirl] = "Shine",
            [Characters.headManNoJetpack] = "Faux (Prelude)",
            [Characters.eightBallBoss] = "DOT EXE (Boss)",
            [Characters.legendMetalHead] = "Red Felix (Dream)",
        };*/

        void Awake() {
            Instance = this;
            Log = this.Logger;
            HandlersConfig.AddConverter();

            // Init Key Inputs
            reloadKey = Config.Bind("Keybinds", "Reload", KeyCode.F5); // Is there a way to display AcceptableValues only for the first input ?
            characterKey = Config.Bind("Keybinds", "CharacterKey", KeyCode.C);
            gearKey = Config.Bind("Keybinds", "GearKey", KeyCode.G);
            phoneKey = Config.Bind("Keybinds", "PhoneKey", KeyCode.P);
            spraycanKey = Config.Bind("Keybinds", "SpraycanKey", KeyCode.E);
            meshUpKey = Config.Bind("Keybinds", "MeshUP", KeyCode.Home);
            meshDownKey = Config.Bind("Keybinds", "MeshDOWN", KeyCode.End);
            textureUpKey = Config.Bind("Keybinds", "TextureUP", KeyCode.PageUp);
            textureDownKey = Config.Bind("Keybinds", "TextureDOWN", KeyCode.PageDown);

            // Init Folders, Lists and Saved Indexes
            SavedIndexes.Add(Characters.NONE, Config.Bind("Saved Indexes", $"Default Indexes", new HandlersConfig(Characters.NONE), $"Default saved indexes"));
            Config.SaveOnConfigSet = true;
            //foreach (KeyValuePair<Characters, string> entry in CHARACTERMAPS) {
            //    CharactersFolder.CreateSubdirectory(Path.Combine(entry.Value, ".Default"));
            //    SavedIndexes.Add(entry.Key, Config.Bind("Saved Indexes", $"Indexes for {entry.Value}", new HandlersConfig(entry.Key), $"Saved indexes for {entry.Value}"));
            //}
            CHARACTER = new CharacterHandler();
            GEARS.Add(MoveStyle.INLINE, new GearHandler(MoveStyle.INLINE));
            GEARS.Add(MoveStyle.SKATEBOARD, new GearHandler(MoveStyle.SKATEBOARD));
            GEARS.Add(MoveStyle.BMX, new GearHandler(MoveStyle.BMX));
            PHONES = new PhoneHandler();
            SPRAYCANS = new SpraycanHandler();
        }

        void LateUpdate() {
            if (WorldHandler.instance?.currentPlayer?.gameObject) {
                if (HASH != WorldHandler.instance.currentPlayer.characterVisual.GetHashCode()) {
                    HASH = WorldHandler.instance.currentPlayer.characterVisual.GetHashCode();

                    // Get All References
                    try {
                        PLAYER = WorldHandler.instance?.currentPlayer.gameObject;
                    } catch {
                        Log.LogError("Player can't be referenced !");
                    }

                    ReloadAssets(); // It'll reload references too

                    // Init Materials
                    GetMaterials();
                }

                if (WorldHandler.instance.currentPlayer.inGraffitiGame) {
                    SPRAYCANS.SetGraffitiEffect();

                    // Trying to Remove Flash during GraffitiGame
                    //GraffitiGame graffitiGame = FindObjectOfType<GraffitiGame>();
                    //graffitiGame.flashDuration = 0;
                }
            }
        }

        void Update() {
            // Inputs
            CheckInput(characterKey.Value, Check.Character);
            CheckInput(gearKey.Value, Check.Gear);
            CheckInput(phoneKey.Value, Check.Phone);
            CheckInput(spraycanKey.Value, Check.Spraycan);

            if (Input.GetKeyDown(reloadKey.Value))
                ReloadAssets();
        }

        void CheckInput(KeyCode key, Check check) {
            if (Input.GetKey(key)) {
                if (Input.GetKeyDown(meshUpKey.Value)) { // Mesh Swap -
                    SetMesh(check, -1);
                }
                if (Input.GetKeyDown(meshDownKey.Value)) { // Mesh Swap +
                    SetMesh(check, +1);
                }
                if (Input.GetKeyDown(textureUpKey.Value)) { // Texture Swap -
                    SetTexture(check, -1);
                }
                if (Input.GetKeyDown(textureDownKey.Value)) {// Texture Swap +
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
        }

        void GetReferences() {
            // Character Visual Slot
            try {
                PLAYER_VISUAL = WorldHandler.instance.currentPlayer.characterVisual;
            } catch {
                Log.LogError("Player.CharacterVisual can't be referenced !");
            }

            // Phone Slot
            try {
                PLAYER_PHONE = WorldHandler.instance.currentPlayer.phone;
            } catch {
                Log.LogError("Player.Phone can't be referenced !");
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

            // Gears
            NPC[] GearSpotsInHideout = GameObject.FindObjectsOfType<NPC>();
            try {
                // Hideout Skateboard Spot
                foreach (var spot in GearSpotsInHideout) {
                    if (spot.name == "NPC_MovestyleChangerSkateboard") {
                        GameObject go;
                        // Skateboard 1
                        go = spot.transform.Find("PreviewSkateboard/skateboard").gameObject;
                        go.name = "skateboard(Clone)";
                        GEARS[MoveStyle.SKATEBOARD].REFERENCES.Add(go);
                        // Skateboard 2
                        go = spot.transform.Find("PreviewSkateboard/skateboard (1)").gameObject;
                        go.name = "skateboard(Clone)";
                        GEARS[MoveStyle.SKATEBOARD].REFERENCES.Add(go);
                    }
                }

                GEARS[MoveStyle.SKATEBOARD].REFERENCES.Add(PLAYER_VISUAL.moveStyleProps.skateboard);
            } catch {
                Log.LogError("Skateboard Gameobject can't be referenced !");
            }

            try {
                // Hideout Inlines Spot
                foreach (var spot in GearSpotsInHideout) {
                    if (spot.name == "NPC_MovestyleChangerInline") {
                        GameObject go;
                        // Skate Left
                        go = spot.transform.Find("SkatesPreview/skateLeft").gameObject;
                        go.name = "skateLeft(Clone)";
                        GEARS[MoveStyle.INLINE].REFERENCES.Add(go);
                        // Skate Right
                        go = spot.transform.Find("SkatesPreview/skateRight").gameObject;
                        go.name = "skateRight(Clone)";
                        GEARS[MoveStyle.INLINE].REFERENCES.Add(go);
                    }
                }

                GEARS[MoveStyle.INLINE].REFERENCES.Add(PLAYER_VISUAL.moveStyleProps.skateL);
                GEARS[MoveStyle.INLINE].REFERENCES.Add(PLAYER_VISUAL.moveStyleProps.skateR);
            } catch {
                Log.LogError("Inline Gameobjects can't be referenced !");
            }

            try {
                // Hideout BMX Spot
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

                GEARS[MoveStyle.BMX].REFERENCES.Add(PLAYER_VISUAL.moveStyleProps.bmxFrame);
                GEARS[MoveStyle.BMX].REFERENCES.Add(PLAYER_VISUAL.moveStyleProps.bmxGear);
                GEARS[MoveStyle.BMX].REFERENCES.Add(PLAYER_VISUAL.moveStyleProps.bmxHandlebars);
                GEARS[MoveStyle.BMX].REFERENCES.Add(PLAYER_VISUAL.moveStyleProps.bmxPedalL);
                GEARS[MoveStyle.BMX].REFERENCES.Add(PLAYER_VISUAL.moveStyleProps.bmxPedalR);
                GEARS[MoveStyle.BMX].REFERENCES.Add(PLAYER_VISUAL.moveStyleProps.bmxWheelF);
                GEARS[MoveStyle.BMX].REFERENCES.Add(PLAYER_VISUAL.moveStyleProps.bmxWheelR);
            } catch {
                Log.LogError("BMX Gameobjects can't be referenced !");
            }
            

            // Phone
            try {
                PHONES.REFERENCES.Add(PLAYER_VISUAL.handL.Find("propl/phoneInHand(Clone)").gameObject);
                PHONES.REFERENCES.Add(PLAYER_PHONE.openPhoneCanvas.transform.Find("PhoneContainerOpen/PhoneOpen").gameObject);
                PHONES.REFERENCES.Add(PLAYER_PHONE.closedPhoneCanvas.transform.Find("PhoneContainerClosed/PhoneClosed").gameObject);
            } catch {
                Log.LogError("Phone Gameobjects can't be referenced !");
            }


            // Spraycan
            try {
                SPRAYCANS.REFERENCES.Add(PLAYER_VISUAL.handR.Find("propr/spraycan(Clone)").gameObject);
            } catch {
                Log.LogError("Spraycan Gameobject can't be referenced !");
            }

            // Spraycan Caps
            GameObject[] RootGameObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject go in RootGameObjects) {
                if (go.name == "spraycanCapJunk(Clone)") {
                    try {
                        SPRAYCANS.REFERENCES.Add(go);
                    } catch {
                        Log.LogError("Spraycan Caps Gameobjects can't be referenced !");
                    }
                }
            }

            Log.LogMessage($"————————————————————————————————————————— {RandomLine()}");
        }

        void ReloadAssets() {
            // Get Assets
            CHARACTER.GetAssets();
            foreach (GearHandler handler in GEARS.Values)
                handler.GetAssets();
            PHONES.GetAssets();
            SPRAYCANS.GetAssets();

            // Reload References
            //if (WorldHandler.instance?.currentPlayer != null) {
                ReloadReferences();
            //}
        }

        void ReloadReferences() {
            // Clear References
            CHARACTER.REFERENCES.Clear();
            foreach (GearHandler handler in GEARS.Values)
                handler.REFERENCES.Clear();
            PHONES.REFERENCES.Clear();
            SPRAYCANS.REFERENCES.Clear();
            MATERIALS.Clear();

            // Get References
            GetReferences();

            // Apply the new Assets
            CHARACTER.Reapply();
            GEARS[WorldHandler.instance.currentPlayer.moveStyleEquipped].Reapply();
            PHONES.Reapply();
            SPRAYCANS.Reapply();
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
