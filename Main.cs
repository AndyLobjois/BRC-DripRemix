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
        internal static DirectoryInfo ModdingFolder = Directory.CreateDirectory(Path.Combine(Paths.GameRootPath, "ModdingFolder", "BRC-DripRemix"));
        internal static DirectoryInfo CharactersFolder = ModdingFolder.CreateSubdirectory("Characters");
        internal static DirectoryInfo GearsFolder = ModdingFolder.CreateSubdirectory("Gears");
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

        public Dictionary<Characters, CharacterHandler> CHARACTERS = new Dictionary<Characters, CharacterHandler>();
        public Dictionary<MoveStyle, GearHandler> GEARS = new Dictionary<MoveStyle, GearHandler>();
        public PhoneHandler PHONES;
        public SpraycanHandler SPRAYCANS;
        //public GraffitiHandler GRAFFITI = new GraffitiHandler();

        public Dictionary<Characters, ConfigEntry<HandlersConfig>> SavedIndexes = new Dictionary<Characters, ConfigEntry<HandlersConfig>>();

        public static Dictionary<Characters, string> CHARACTERMAPS = new Dictionary<Characters, string>() {
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
            [Characters.headManNoJetpack] = "Faux (Prelude)", // Necessary ?
            [Characters.eightBallBoss] = "DOT EXE (Boss)", // Necessary ?
            [Characters.legendMetalHead] = "Red Felix (Dream)", // Necessary ?
        };

        void Awake() {
            Instance = this;
            Log = this.Logger;
            log($"{Infos.PLUGIN_NAME} {Infos.PLUGIN_VERSION} is loaded !");
            HandlersConfig.AddConverter();

            // Init Key Inputs
            reloadKey = Config.Bind("Keybinds", "Reload", KeyCode.F5); // Is there a way to display AcceptableValues only for the first input ?
            characterKey = Config.Bind("Keybinds", "CharacterKey", KeyCode.C);
            gearKey = Config.Bind("Keybinds", "GearKey", KeyCode.G);
            phoneKey = Config.Bind("Keybinds", "PhoneKey", KeyCode.P);
            spraycanKey = Config.Bind("Keybinds", "SpraycanKey", KeyCode.B);
            meshUpKey = Config.Bind("Keybinds", "MeshUP", KeyCode.Home);
            meshDownKey = Config.Bind("Keybinds", "MeshDOWN", KeyCode.End);
            textureUpKey = Config.Bind("Keybinds", "TextureUP", KeyCode.PageUp);
            textureDownKey = Config.Bind("Keybinds", "TextureDOWN", KeyCode.PageDown);

            // Init Folders and Lists
            SavedIndexes.Add(Characters.NONE, Config.Bind("Saved Indexes", $"Default Indexes", new HandlersConfig(Characters.NONE), $"Default saved indexes"));
            Config.SaveOnConfigSet = true;
            foreach (KeyValuePair<Characters, string> entry in CHARACTERMAPS) {
                CharactersFolder.CreateSubdirectory(Path.Combine(entry.Value, ".Default"));
                SavedIndexes.Add(entry.Key, Config.Bind("Saved Indexes", $"Indexes for {entry.Value}", new HandlersConfig(entry.Key), $"Saved indexes for {entry.Value}"));
                CHARACTERS.Add(entry.Key, new CharacterHandler(entry.Key));

            }
            GEARS.Add(MoveStyle.INLINE, new GearHandler(MoveStyle.INLINE));
            GEARS.Add(MoveStyle.SKATEBOARD, new GearHandler(MoveStyle.SKATEBOARD));
            GEARS.Add(MoveStyle.BMX, new GearHandler(MoveStyle.BMX));
            PHONES = new PhoneHandler();
            SPRAYCANS = new SpraycanHandler();

            // Get Assets
            foreach (CharacterHandler handler in CHARACTERS.Values)
                handler.GetAssets();

            foreach (GearHandler handler in GEARS.Values)
                handler.GetAssets();

            PHONES.GetAssets();
            SPRAYCANS.GetAssets();
            //GRAFFITI.GetAssets(); //WorldHandler.instance.graffitiArtInfo.graffitiArt[0].graffitiMaterial.mainTexture
        }

        void LateUpdate() {
            if (WorldHandler.instance?.currentPlayer?.gameObject) {
                if (HASH != WorldHandler.instance.currentPlayer.characterVisual.GetHashCode()) {
                    HASH = WorldHandler.instance.currentPlayer.characterVisual.GetHashCode();

                    // Get All References
                    PLAYER = WorldHandler.instance?.currentPlayer.gameObject;
                    log("Player has been found!");

                    ReloadReferences();
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
                CHARACTERS[WorldHandler.instance.currentPlayer.character].SetTexture(add);
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
            try {
                PLAYER_VISUAL = WorldHandler.instance.currentPlayer.characterVisual;
            } catch {
                logError("Player.CharacterVisual can't be referenced !");
            }

            try {
                PLAYER_PHONE = WorldHandler.instance.currentPlayer.phone;
            } catch {
                logError("Player.Phone can't be referenced !");
            }

            
            // Character
            foreach (KeyValuePair<Characters, string> entry in CHARACTERMAPS) {
                foreach (Transform child in PLAYER_VISUAL.characterObject.transform) {
                    try {
                        if (child.GetComponent<SkinnedMeshRenderer>()) {
                            CHARACTERS[entry.Key].REFERENCES.Add(child.gameObject);
                        }
                    } catch {
                        logError("Character SkinnedMeshRenderer can't be referenced !");
                    }
                }
            }

            // Gears
            try {
                GEARS[MoveStyle.SKATEBOARD].REFERENCES.Add(PLAYER_VISUAL.moveStyleProps.skateboard);
            } catch {
                logError("Skateboard Gameobject can't be referenced !");
            }

            try {
                GEARS[MoveStyle.INLINE].REFERENCES.Add(PLAYER_VISUAL.moveStyleProps.skateL);
                GEARS[MoveStyle.INLINE].REFERENCES.Add(PLAYER_VISUAL.moveStyleProps.skateR);
            } catch {
                logError("Inline Gameobjects can't be referenced !");
            }

            try {
                GEARS[MoveStyle.BMX].REFERENCES.Add(PLAYER_VISUAL.moveStyleProps.bmxFrame);
                GEARS[MoveStyle.BMX].REFERENCES.Add(PLAYER_VISUAL.moveStyleProps.bmxGear);
                GEARS[MoveStyle.BMX].REFERENCES.Add(PLAYER_VISUAL.moveStyleProps.bmxHandlebars);
                GEARS[MoveStyle.BMX].REFERENCES.Add(PLAYER_VISUAL.moveStyleProps.bmxPedalL);
                GEARS[MoveStyle.BMX].REFERENCES.Add(PLAYER_VISUAL.moveStyleProps.bmxPedalR);
                GEARS[MoveStyle.BMX].REFERENCES.Add(PLAYER_VISUAL.moveStyleProps.bmxWheelF);
                GEARS[MoveStyle.BMX].REFERENCES.Add(PLAYER_VISUAL.moveStyleProps.bmxWheelR);
            } catch {
                logError("BMX Gameobjects can't be referenced !");
            }
            

            // Phone
            try {
                PHONES.REFERENCES.Add(PLAYER_VISUAL.handL.Find("propl/phoneInHand(Clone)").gameObject);
                PHONES.REFERENCES.Add(PLAYER_PHONE.openPhoneCanvas.transform.Find("PhoneContainerOpen/PhoneOpen").gameObject);
                PHONES.REFERENCES.Add(PLAYER_PHONE.closedPhoneCanvas.transform.Find("PhoneContainerClosed/PhoneClosed").gameObject);
            } catch {
                logError("Phone Gameobjects can't be referenced !");
            }


            // Spraycan
            try {
                SPRAYCANS.REFERENCES.Add(PLAYER_VISUAL.handR.Find("propr/spraycan(Clone)").gameObject);
            } catch {
                logError("Spraycan Gameobject can't be referenced !");
            }

            // Spraycan Caps
            GameObject[] RootGameObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject go in RootGameObjects) {
                if (go.name == "spraycanCapJunk(Clone)") {
                    try {
                        SPRAYCANS.REFERENCES.Add(go);
                    } catch {
                        logError("Spraycan Caps Gameobjects can't be referenced !");
                    }
                }
            }
        }

        void ReloadAssets() {
            foreach (CharacterHandler handler in CHARACTERS.Values)
                handler.GetAssets();

            foreach (GearHandler handler in GEARS.Values)
                handler.GetAssets();

            PHONES.GetAssets();
            SPRAYCANS.GetAssets();

            if (WorldHandler.instance?.currentPlayer != null) {
                ReloadReferences();
            }
        }

        void ReloadReferences() {
            // Clear References
            foreach (CharacterHandler handler in CHARACTERS.Values) {
                handler.REFERENCES.Clear();
            }
            foreach (GearHandler handler in GEARS.Values) {
                handler.REFERENCES.Clear();
            }
            PHONES.REFERENCES.Clear();
            SPRAYCANS.REFERENCES.Clear();
            GetReferences();
            log("References have been collected !");

            // Apply the new Assets
            CHARACTERS[WorldHandler.instance.currentPlayer.character].Reapply();
            GEARS[WorldHandler.instance.currentPlayer.moveStyleEquipped].Reapply();
            PHONES.Reapply();
            SPRAYCANS.Reapply();
        }

        static public void log(string message) {
            Log.LogMessage($"{message}");
        }

        static public void logError(string message) {
            Log.LogError($"{message}");
        }
    }
}
