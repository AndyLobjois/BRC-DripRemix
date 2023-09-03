using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using Reptile;
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
        public GameObject PLAYER;
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
            //[Characters.headManNoJetpack] = "Faux (Prelude)", // Necessary ?
            //[Characters.eightBallBoss] = "DOT EXE (Boss)", // Necessary ?
            //[Characters.legendMetalHead] = "Red Felix (Dream)", // Necessary ?
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
            }
        }

        void Update() {
            // Inputs
            CheckInput(characterKey.Value, Check.Character);
            CheckInput(gearKey.Value, Check.Gear);
            CheckInput(phoneKey.Value, Check.Phone);
            CheckInput(spraycanKey.Value, Check.Spraycan);

            if (Input.GetKeyDown(reloadKey.Value))
                Reload();
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
            CharacterVisual visual = WorldHandler.instance.currentPlayer.characterVisual;

            // Character
            foreach (KeyValuePair<Characters, string> entry in CHARACTERMAPS) {
                CHARACTERS[entry.Key].REFERENCES.Add((visual.characterObject.transform.Find("mesh").gameObject));
            }

            // Gears
            GEARS[MoveStyle.SKATEBOARD].REFERENCES.Add((visual.moveStyleProps.skateboard));

            GEARS[MoveStyle.INLINE].REFERENCES.Add((visual.moveStyleProps.skateL));
            GEARS[MoveStyle.INLINE].REFERENCES.Add((visual.moveStyleProps.skateR));

            GEARS[MoveStyle.BMX].REFERENCES.Add((visual.moveStyleProps.bmxFrame));
            GEARS[MoveStyle.BMX].REFERENCES.Add((visual.moveStyleProps.bmxGear));
            GEARS[MoveStyle.BMX].REFERENCES.Add((visual.moveStyleProps.bmxHandlebars));
            GEARS[MoveStyle.BMX].REFERENCES.Add((visual.moveStyleProps.bmxPedalL));
            GEARS[MoveStyle.BMX].REFERENCES.Add((visual.moveStyleProps.bmxPedalR));
            GEARS[MoveStyle.BMX].REFERENCES.Add((visual.moveStyleProps.bmxWheelF));
            GEARS[MoveStyle.BMX].REFERENCES.Add((visual.moveStyleProps.bmxWheelR));

            // Phone
            PHONES.REFERENCES.Add((visual.handL.Find("propl/phoneInHand(Clone)").gameObject));
            GameObject phone = GameObject.Find("Phone(Clone)");
            PHONES.REFERENCES.Add(phone.transform.Find("OpenCanvas/PhoneContainerOpen/PhoneOpen").gameObject);
            PHONES.REFERENCES.Add(phone.transform.Find("ClosedCanvas/PhoneContainerClosed/PhoneClosed").gameObject);

            // Spraycan
            SPRAYCANS.REFERENCES.Add((visual.handR.Find("propr/spraycan(Clone)").gameObject));

            // Spraycan Caps
            GameObject[] RootGameObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject go in RootGameObjects) {
                if (go.name == "spraycanCapJunk(Clone)") {
                    SPRAYCANS.REFERENCES.Add(go);
                }
            }
        }

        void Reload() {
            foreach (CharacterHandler handler in CHARACTERS.Values)
                handler.GetAssets();

            foreach (GearHandler handler in GEARS.Values)
                handler.GetAssets();

            PHONES.GetAssets();
            SPRAYCANS.GetAssets();

            if (WorldHandler.instance?.currentPlayer != null)
            {
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
            //Debug.Log($"<color=orange>[DripRemix] {message}</color>");
        }
    }
}
