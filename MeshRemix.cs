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

// TODO:
// - Figure out Character swapping
// - Configurable shortcuts (PageUp, PageDown)
// - Save File for current indexes (Inside the config files ?)

namespace MeshRemix {

    [BepInPlugin(MeshRemixInfos.PLUGIN_ID, MeshRemixInfos.PLUGIN_NAME, MeshRemixInfos.PLUGIN_VERSION)]
    [BepInDependency(BRCML.PluginInfos.PLUGIN_ID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInProcess("Bomb Rush Cyberfunk.exe")]
    public class MeshRemix : BaseUnityPlugin {

        public static MeshRemix Instance;
        internal static DirectoryInfo ModdingFolder = Directory.CreateDirectory(Path.Combine(BepInEx.Paths.GameRootPath, "ModdingFolder"));
        internal static DirectoryInfo GEARFOLDER => ModdingFolder.CreateSubdirectory("BRC-MeshRemix/Gears");
        internal static ManualLogSource Log { get; private set; }

        // CORE
        public int HASH;
        public GameObject PLAYER;
        public MoveStyle CURRENTGEAR;
        public int INDEX_INLINE;
        public int INDEX_SKATEBOARD;
        public int INDEX_BMX;

        // BUNDLES
        public List<AssetBundle> BUNDLES_INLINE = new List<AssetBundle>();
        public List<AssetBundle> BUNDLES_SKATEBOARD = new List<AssetBundle>();
        public List<AssetBundle> BUNDLES_BMX = new List<AssetBundle>();

        // REFERENCES
        public List<GameObject> REFS_INLINE = new List<GameObject>();
        public List<GameObject> REFS_SKATEBOARD = new List<GameObject>();
        public List<GameObject> REFS_BMX = new List<GameObject>();
        public Texture[] REFS_TEXTURES = new Texture[3]; // [BMX, SKATEBOARD, INLINE]

        void Awake() {
            Instance = this;
            Log = this.Logger;
            log("MeshRemix is loaded !");

            // Init Index
            INDEX_INLINE = 0;
            INDEX_SKATEBOARD = 0;
            INDEX_BMX = 0;

            // Get Bundles
            GetBundles(GEARFOLDER.CreateSubdirectory("Inline"), BUNDLES_INLINE);
            GetBundles(GEARFOLDER.CreateSubdirectory("Skateboard"), BUNDLES_SKATEBOARD);
            GetBundles(GEARFOLDER.CreateSubdirectory("BMX"), BUNDLES_BMX);
        }

        void LateUpdate() {
            if (WorldHandler.instance?.currentPlayer.gameObject) {
                if (HASH != WorldHandler.instance.currentPlayer.characterVisual.GetHashCode()) {
                    HASH = WorldHandler.instance.currentPlayer.characterVisual.GetHashCode();

                    // Get All References
                    PLAYER = WorldHandler.instance?.currentPlayer.gameObject;
                    log("Player have been found !");

                    // Clear References
                    REFS_INLINE.Clear();
                    REFS_SKATEBOARD.Clear();
                    REFS_BMX.Clear();
                    GetReferences(PLAYER.transform);
                    log("References have been collected !");

                    // Apply the new Assets
                    SetGear(0);
                }
            }
        }

        void Update() {
            if (WorldHandler.instance?.currentPlayer)
                CURRENTGEAR = WorldHandler.instance.currentPlayer.moveStyleEquipped;

            // Inputs
            if (Input.GetKeyDown(KeyCode.PageUp))
                SetGear(-1);

            if (Input.GetKeyDown(KeyCode.PageDown))
                SetGear(+1);
        }

        void GetBundles(DirectoryInfo bundleDir, List<AssetBundle> bundlesList) {
            FileInfo[] pathsList = bundleDir.GetFiles("*", SearchOption.AllDirectories);
            foreach (FileInfo path in pathsList)
                bundlesList.Add(AssetBundle.LoadFromFile(path.FullName));

            // Log
            string _names = "";
            for (int i = 0; i < bundlesList.Count; i++) {
                _names += "\n" + bundlesList[i].name;
            }
            log($"{bundlesList.Count} {bundleDir.Name}(s) loaded ! <color=grey>{_names}</color>");
        }

        void GetReferences(Transform parent, int level = 0) { // Recursive Search Function
            foreach (Transform child in parent) {
                // Characters
                //...

                // Gears
                if (child.name == "skateLeft(Clone)" || child.name == "skateRight(Clone)") {
                    REFS_INLINE.Add(child.gameObject);
                    REFS_TEXTURES[2] = child.GetComponent<MeshRenderer>().material.mainTexture;
                }

                if (child.name == "skateboard(Clone)") {
                    REFS_SKATEBOARD.Add(child.gameObject);
                    REFS_TEXTURES[1] = child.GetComponent<MeshRenderer>().material.mainTexture;
                }

                if (child.name == "BmxFrame(Clone)" ||
                    child.name == "BmxGear(Clone)" ||
                    child.name == "BmxHandlebars(Clone)" ||
                    child.name == "BmxPedalL(Clone)" ||
                    child.name == "BmxPedalR(Clone)" ||
                    child.name == "BmxWheelF(Clone)" ||
                    child.name == "BmxWheelR(Clone)") {
                    REFS_BMX.Add(child.gameObject);
                    REFS_TEXTURES[0] = child.GetComponent<MeshRenderer>().material.mainTexture;
                }

                // Process next deeper level
                GetReferences(child, level + 1);
            }
        }

        void SetGear(int add) {
            if (WorldHandler.instance.currentPlayer.moveStyleEquipped == MoveStyle.INLINE) {
                INDEX_INLINE = Mathf.Clamp(INDEX_INLINE + add, 0, BUNDLES_INLINE.Count - 1);
                if (BUNDLES_INLINE.Count > 0)
                    Swap(REFS_INLINE, BUNDLES_INLINE, INDEX_INLINE);
            }

            if (WorldHandler.instance.currentPlayer.moveStyleEquipped == MoveStyle.SKATEBOARD) {
                INDEX_SKATEBOARD = Mathf.Clamp(INDEX_SKATEBOARD + add, 0, BUNDLES_SKATEBOARD.Count - 1);
                if (BUNDLES_SKATEBOARD.Count > 0)
                    Swap(REFS_SKATEBOARD, BUNDLES_SKATEBOARD, INDEX_SKATEBOARD);
            }

            if (WorldHandler.instance.currentPlayer.moveStyleEquipped == MoveStyle.BMX) {
                INDEX_BMX = Mathf.Clamp(INDEX_BMX + add, 0, BUNDLES_BMX.Count - 1);
                if (BUNDLES_BMX.Count > 0)
                    Swap(REFS_BMX, BUNDLES_BMX, INDEX_BMX);
            }
        }

        void Swap(List<GameObject> _REFS, List<AssetBundle> _BUNDLES, int index) {
            foreach (GameObject _ref in _REFS) {
                // Mesh
                Mesh _mesh = _BUNDLES[index].LoadAsset<Mesh>(_ref.name + ".fbx");
                _ref.GetComponent<MeshFilter>().mesh = _mesh;

                // Particle
                Mesh _particle = _BUNDLES[index].LoadAsset<Mesh>("particle.fbx");
                if (_ref.name == "skateRight(Clone)" || _ref.name == "skateLeft(Clone)" || _ref.name == "skateboard(Clone)") {
                    if (_ref.transform.childCount > 0) { // Detect ParticleSystem
                        _ref.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().mesh = _mesh; //IMPORTANT: Mesh need to have Read/Write enable in the Import Settings of Unity
                        _ref.transform.GetChild(0).GetComponent<ParticleSystem>().Play();
                    }
                } else if (_ref.name == "BmxFrame(Clone)") { // Because BMX is in multiple parts, the particle system use 1 specific merged mesh
                    if (_ref.transform.childCount > 0) { // Detect ParticleSystem
                        _ref.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().mesh = _particle; //IMPORTANT: Mesh need to have Read/Write enable in the Import Settings of Unity
                        _ref.transform.GetChild(0).GetComponent<ParticleSystem>().Play();
                    }
                }

                // Texture (Could be better !)
                MeshRenderer _renderer = _ref.GetComponent<MeshRenderer>();
                Texture _tex = _BUNDLES[index].LoadAsset<Texture2D>("tex.png");
                if (_tex != null) {
                    _renderer.material.mainTexture = _tex;
                } else {
                    if (CURRENTGEAR == MoveStyle.INLINE)
                        _renderer.material.mainTexture = WorldHandler.instance.currentPlayer.MoveStylePropsPrefabs.skateL.GetComponent<MeshRenderer>().material.mainTexture;
                    if (CURRENTGEAR == MoveStyle.SKATEBOARD)
                        _renderer.material.mainTexture = WorldHandler.instance.currentPlayer.MoveStylePropsPrefabs.skateboard.GetComponent<MeshRenderer>().material.mainTexture;
                    if (CURRENTGEAR == MoveStyle.BMX)
                        _renderer.material.mainTexture = WorldHandler.instance.currentPlayer.MoveStylePropsPrefabs.bmxFrame.GetComponent<MeshRenderer>().material.mainTexture;
                }
            }

            log($"Load: {_BUNDLES[index].name}");
        }

        static public void log(string message) {
            Debug.Log($"<color=orange>[MeshRemix] {message}</color>");
        }
    }
}
