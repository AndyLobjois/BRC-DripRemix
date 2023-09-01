﻿using System;
using System.IO;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using Reptile;
using OBJImporter;

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

        // References
        public int HASH;
        public GameObject PLAYER;
        public enum Check { Character, Gear, Phone, Spraycan }

        public Dictionary<Characters, CharacterHandler> CHARACTERS = new Dictionary<Characters, CharacterHandler>();
        public Dictionary<MoveStyle, GearHandler> GEARS = new Dictionary<MoveStyle, GearHandler>();
        public PhoneHandler PHONES = new PhoneHandler();
        public SpraycanHandler SPRAYCANS = new SpraycanHandler();
        public GraffitiHandler GRAFFITI = new GraffitiHandler();

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

            // Init Key Inputs
            characterKey = Config.Bind("Keybinds", "CharacterKey", KeyCode.C);
            gearKey = Config.Bind("Keybinds", "GearKey", KeyCode.G);
            phoneKey = Config.Bind("Keybinds", "PhoneKey", KeyCode.P);
            spraycanKey = Config.Bind("Keybinds", "SpraycanKey", KeyCode.B);
            reloadKey = Config.Bind("Keybinds", "Reload", KeyCode.F5);

            // Init Folders and Lists
            foreach (KeyValuePair<Characters, string> entry in CHARACTERMAPS) {
                CharactersFolder.CreateSubdirectory(entry.Value + "/.Default/");
                CHARACTERS.Add(entry.Key, new CharacterHandler(entry.Key));
            }
            GEARS.Add(MoveStyle.INLINE, new GearHandler(MoveStyle.INLINE));
            GEARS.Add(MoveStyle.SKATEBOARD, new GearHandler(MoveStyle.SKATEBOARD));
            GEARS.Add(MoveStyle.BMX, new GearHandler(MoveStyle.BMX));

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
                if (Input.GetKeyDown(KeyCode.PageUp)) { // Mesh Swap -
                    SetMesh(check, -1);
                }
                if (Input.GetKeyDown(KeyCode.PageDown)) { // Mesh Swap +
                    SetMesh(check, +1);
                }
                if (Input.GetKeyDown(KeyCode.Home)) { // Texture Swap -
                    SetTexture(check, -1);
                }
                if (Input.GetKeyDown(KeyCode.End)) {// Texture Swap +
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
            GEARS[MoveStyle.SKATEBOARD].REFERENCES.Add((visual.skateboardBone.Find("skateboard(Clone)").gameObject));

            GEARS[MoveStyle.INLINE].REFERENCES.Add((visual.footL.Find("skateLeft(Clone)").gameObject));
            GEARS[MoveStyle.INLINE].REFERENCES.Add((visual.footR.Find("skateRight(Clone)").gameObject));

            GEARS[MoveStyle.BMX].REFERENCES.Add((visual.bmxFrameBone.Find("BmxFrame(Clone)").gameObject));
            GEARS[MoveStyle.BMX].REFERENCES.Add((visual.bmxGearBone.Find("BmxGear(Clone)").gameObject));
            GEARS[MoveStyle.BMX].REFERENCES.Add((visual.bmxHandlebarsBone.Find("BmxHandlebars(Clone)").gameObject));
            GEARS[MoveStyle.BMX].REFERENCES.Add((visual.bmxPedalLBone.Find("BmxPedalL(Clone)").gameObject));
            GEARS[MoveStyle.BMX].REFERENCES.Add((visual.bmxPedalRBone.Find("BmxPedalR(Clone)").gameObject));
            GEARS[MoveStyle.BMX].REFERENCES.Add((visual.bmxWheelFBone.Find("BmxWheelF(Clone)").gameObject));
            GEARS[MoveStyle.BMX].REFERENCES.Add((visual.bmxWheelRBone.Find("BmxWheelR(Clone)").gameObject));

            // Phone
            PHONES.REFERENCES.Add((visual.handL.Find("propl/phoneInHand(Clone)").gameObject));

            // Spraycan
            SPRAYCANS.REFERENCES.Add((visual.handR.Find("propr/spraycan(Clone)").gameObject));
        }

        void Reload() {
            foreach (CharacterHandler handler in CHARACTERS.Values)
                handler.GetAssets();

            foreach (GearHandler handler in GEARS.Values)
                handler.GetAssets();

            PHONES.GetAssets();
            SPRAYCANS.GetAssets();

            ReloadReferences();
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
            CHARACTERS[WorldHandler.instance.currentPlayer.character].SetTexture(0);
            GEARS[WorldHandler.instance.currentPlayer.moveStyleEquipped].SetMesh(0);
            GEARS[WorldHandler.instance.currentPlayer.moveStyleEquipped].SetTexture(0);
            PHONES.SetMesh(0);
            PHONES.SetTexture(0);
            SPRAYCANS.SetMesh(0);
            SPRAYCANS.SetTexture(0);
        }

        static public void log(string message) {
            Log.LogMessage($"{message}");
            //Debug.Log($"<color=orange>[DripRemix] {message}</color>");
        }
    }
}
