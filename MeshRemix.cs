using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using Reptile;
using BRCML;
using BRCML.Utils;

namespace MeshRemix {

    [BepInPlugin(MeshRemixInfos.PLUGIN_ID, MeshRemixInfos.PLUGIN_NAME, MeshRemixInfos.PLUGIN_VERSION)]
    [BepInDependency(BRCML.PluginInfos.PLUGIN_ID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInProcess("Bomb Rush Cyberfunk.exe")]
    public class MeshRemix : BaseUnityPlugin {

        public static MeshRemix Instance;
        internal static DirectoryInfo ModdingFolder { get; private set; } = null;
        internal static DirectoryInfo CHARFOLDER => ModdingFolder.CreateSubdirectory("Characters");
        internal static DirectoryInfo GEARFOLDER => ModdingFolder.CreateSubdirectory("Gears");
        internal static ManualLogSource Log { get; private set; }

        // Core
        public GameObject PLAYER;
        public MoveStyle CURRENTMOVESTYLE;
        bool CHECK = false;
        public int INDEX_CHAR;
        public int INDEX_INLINE;
        public int INDEX_SKATEBOARD;
        public int INDEX_BMX;

        // BUNDLES LISTS
        public Dictionary<Characters, List<AssetBundle>> BUNDLES_CHARACTER = new Dictionary<Characters, List<AssetBundle>>();
        public List<AssetBundle> BUNDLES_INLINE = new List<AssetBundle>();
        public List<AssetBundle> BUNDLES_SKATEBOARD = new List<AssetBundle>();
        public List<AssetBundle> BUNDLES_BMX = new List<AssetBundle>();

        // GAMEOBJECTS LISTS
        public Dictionary<Characters, GameObject> REFS_CHARACTER;
        public List<GameObject> REFS_INLINE = new List<GameObject>();
        public List<GameObject> REFS_SKATEBOARD = new List<GameObject>();
        public List<GameObject> REFS_BMX = new List<GameObject>();

        static public void log(string message) {
            Debug.Log($"<color=orange>[MeshRemix] {message}</color>");
        }

        void Awake() {
            Instance = this;
            Log = this.Logger;
            //ModdingFolder = BRCML.Utils.ModdingFolder.GetModSubFolder(this.Info);
            //var harmony = new Harmony(MeshRemixInfos.PLUGIN_NAME);
            //harmony.PatchAll();

            log("MeshRemix is loaded !");

            // Init Index
            INDEX_CHAR = 0;
            INDEX_INLINE = 0;
            INDEX_SKATEBOARD = 0;
            INDEX_BMX = 0;

            // Get All Bundles
            //foreach (Characters character in characterNamesMap.Keys)
            //{
            //    BUNDLES_CHARACTER[character] = new List<AssetBundle>();
            //    //TODO Use the human readable name, not the machine name
            //    GetBUNDLES(CHARFOLDER.CreateSubdirectory(CharacterToString(character)), BUNDLES_CHARACTER[character]);

            //}

            GetBUNDLES("Gears", "Inline", BUNDLES_INLINE);
            GetBUNDLES("Gears", "Skateboard", BUNDLES_SKATEBOARD);
            GetBUNDLES("Gears", "BMX", BUNDLES_BMX);

            // GLOM GetBUNDLES
            //GetBUNDLES(GEARFOLDER.CreateSubdirectory("Gears/Inline"), BUNDLES_INLINE);
            //GetBUNDLES(GEARFOLDER.CreateSubdirectory("Gears/Skateboard"), BUNDLES_SKATEBOARD);
            //GetBUNDLES(GEARFOLDER.CreateSubdirectory("Gears/BMX"), BUNDLES_BMX);
        }

        void LateUpdate() {
            // Checker
            var worldHandler = WorldHandler.instance?.currentPlayer;
            if (worldHandler != null && !CHECK) {
                CHECK = true;

                // Get All References
                PLAYER = WorldHandler.instance?.currentPlayer.gameObject;
                
                GetREFERENCES(PLAYER.transform);
                log("References have been collected !");

                // Apply the new Assets
                StartCoroutine("InitAssets");
            }

            if (worldHandler == null) {
                CHECK = false;

                // Reset
                REFS_INLINE.Clear();
                REFS_SKATEBOARD.Clear();
                REFS_BMX.Clear();
            }
        }

        void Update() {
            var worldHandler = WorldHandler.instance?.currentPlayer;
            if (worldHandler) {
                CURRENTMOVESTYLE = WorldHandler.instance.currentPlayer.moveStyle;
            }

            // Inputs
            if (Input.GetKeyDown(KeyCode.PageUp))
                GearParser(+1);

            if (Input.GetKeyDown(KeyCode.PageDown))
                GearParser(-1);
        }

        void GearParser(int add) {
            if (CURRENTMOVESTYLE == MoveStyle.INLINE) {
                INDEX_INLINE = Mathf.Clamp(INDEX_INLINE + add, 0, BUNDLES_INLINE.Count - 1);
                if (BUNDLES_INLINE.Count > 0)
                    SetInline(INDEX_INLINE);
            }

            if (CURRENTMOVESTYLE == MoveStyle.SKATEBOARD) {
                INDEX_SKATEBOARD = Mathf.Clamp(INDEX_SKATEBOARD + add, 0, BUNDLES_SKATEBOARD.Count - 1);
                if (BUNDLES_SKATEBOARD.Count > 0)
                    SetSkateboard(INDEX_SKATEBOARD);
            }

            if (CURRENTMOVESTYLE == MoveStyle.BMX) {
                INDEX_BMX = Mathf.Clamp(INDEX_BMX + add, 0, BUNDLES_BMX.Count - 1);
                if (BUNDLES_BMX.Count > 0)
                    SetBMX(INDEX_BMX);
            }
        }

        void GetBUNDLES(string parentFolder, string childFolder, List<AssetBundle> bundlesList) {
            string[] pathsList = Directory.GetFiles(Application.dataPath + "/../" + $"/ModdingFolder/{parentFolder}/{childFolder}", "*", SearchOption.AllDirectories);

            foreach (string path in pathsList)
                bundlesList.Add(AssetBundle.LoadFromFile(path));

            // Log
            string _names = "";
            for (int i = 0; i < bundlesList.Count; i++) {
                _names += "\n" + bundlesList[i].name;
            }
            log($"{bundlesList.Count} {childFolder}(s) loaded ! <color=grey>{_names}</color>");
        }

        // GLOM GetBUNDLES()
        //void GetBUNDLES(DirectoryInfo bundleDir, List<AssetBundle> bundlesList) {
        //    FileInfo[] pathsList = bundleDir.GetFiles("*", SearchOption.AllDirectories);

        //    foreach (FileInfo path in pathsList) {
        //        AssetBundle bundle = AssetBundle.LoadFromFile(path.FullName);
        //        bundlesList.Add(bundle);
        //        log($"<b>{bundle.name}</b> bundle loaded !");
        //    }
        //}

        void GetREFERENCES(Transform parent, int level = 0) { // Recursive Search Function
            // Get Characters and Gears
            foreach (Transform child in parent) {
                // Characters
                //foreach (Characters character in characterNamesMap.Keys) {
                //    if (child.name == character + "(Clone)")
                //        REFS_CHARACTER[character] = child.Find("mesh").gameObject;
                //}

                // Gears
                if (child.name == "skateLeft(Clone)" || child.name == "skateRight(Clone)")
                    REFS_INLINE.Add(child.gameObject);

                if (child.name == "skateboard(Clone)")
                    REFS_SKATEBOARD.Add(child.gameObject);

                if (child.name == "BmxFrame(Clone)" ||
                    child.name == "BmxGear(Clone)" ||
                    child.name == "BmxHandlebars(Clone)" ||
                    child.name == "BmxPedalL(Clone)" ||
                    child.name == "BmxPedalR(Clone)" ||
                    child.name == "BmxWheelF(Clone)" ||
                    child.name == "BmxWheelR(Clone)")
                    REFS_BMX.Add(child.gameObject);

                // Process next deeper level
                GetREFERENCES(child, level + 1);
            }
        }

        IEnumerator InitAssets() {
            yield return new WaitForSeconds(0.1f); // Workaround, I'm waiting for lists to be complete

            //// Characters
            //foreach (Characters character in characterNamesMap.Keys) {
            //    if (BUNDLES_CHARACTER[character].Count > 0) {
            //        //SetCharacter(character, BUNDLES_CHARACTER[character], REFS_CHARACTER[character], 0);
            //    }
            //}

            // Gears
            if (BUNDLES_INLINE.Count > 0)
                SetInline(0);

            if (BUNDLES_SKATEBOARD.Count > 0)
                SetSkateboard(0);

            if (BUNDLES_BMX.Count > 0)
                SetBMX(0);
        }

        //GameObject plop = new GameObject();
        void SetCharacter(Characters character, List<AssetBundle> bundlesList, GameObject reference, int index) {
            // Replace Character with the new Character

            //// Test
            //Instantiate(plop);
            //plop.transform.parent = PLAYER.transform;
            //plop.AddComponent<SkinnedMeshRenderer>();
            //string fbxName = characterNamesMap[character].ToLower() + ".fbx";
            //plop.GetComponent<SkinnedMeshRenderer>().sharedMesh = bundlesList[index].LoadAsset<Mesh>(fbxName);

            //reference.transform.GetComponent<SkinnedMeshRenderer>().sharedMesh = plop.GetComponent<SkinnedMeshRenderer>().sharedMesh;

            log($"Load: {bundlesList[index].name}");
        }

        void SetInline(int index) {
            foreach (GameObject reference in REFS_INLINE)
                reference.GetComponent<MeshFilter>().mesh = BUNDLES_INLINE[index].LoadAsset<Mesh>(reference.name + ".fbx");

            log($"Load: {BUNDLES_INLINE[index].name}");

            //    //// White Particle Spawning Mesh (IMPORTANT: Mesh need to have Read/Write enable in the Import Settings of Unity)
            //    //if (REFS_INLINE[i].transform.childCount > 0) {
            //    //    REFS_INLINE[i].transform.GetChild(0).GetComponent<ParticleSystemRenderer>().mesh = _mesh;
            //    //}
        }

        void SetSkateboard(int index) {
            foreach (GameObject reference in REFS_SKATEBOARD)
                reference.GetComponent<MeshFilter>().mesh = BUNDLES_SKATEBOARD[index].LoadAsset<Mesh>(reference.name + ".fbx");

            log($"Load: {BUNDLES_SKATEBOARD[index].name}");
        }

        void SetBMX(int index) {
            foreach (GameObject reference in REFS_BMX)
                reference.GetComponent<MeshFilter>().mesh = BUNDLES_BMX[index].LoadAsset<Mesh>(reference.name + ".fbx");

            log($"Load: {BUNDLES_BMX[index].name}");
        }

        public Dictionary<Characters, string> characterNamesMap = new Dictionary<Characters, string>() {
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
            [Characters.boarder] = "DevilTheory",
            [Characters.headMan] = "Faux", // Necessary ?
            [Characters.prince] = "Flesh Prince",
            [Characters.jetpackBossPlayer] = "Irene Ritvield",
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

        public string CharacterToString(Characters character) {
            if (characterNamesMap.ContainsKey(character)) {
                return characterNamesMap[character];
            }
            return character.ToString();
        }
    }
}
