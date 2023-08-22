using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using Reptile;
using BRCML;

/*
Models Mod for Bomb Rush Cyberfunk by Andy Hellgrim (Aru)
*/

namespace MeshRemix {

    [BepInPlugin(MeshRemixInfos.PLUGIN_ID, MeshRemixInfos.PLUGIN_NAME, MeshRemixInfos.PLUGIN_VERSION)]
    [BepInDependency(BRCML.PluginInfos.PLUGIN_ID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInProcess("Bomb Rush Cyberfunk.exe")]
    public class MeshRemix : BaseUnityPlugin {

        public static MeshRemix instance;
        internal static DirectoryInfo ModdingFolder { get; private set; } = null;
        //private static DirectoryInfo PluginDir => BRCML.Utils.ModdingFolder.GetModSubFolder(instance.Info);
        internal static ManualLogSource Log { get; private set; }

        // Core
        public GameObject PLAYER;
        bool CHECK = false;
        public string CHAR_STATE = "Rave"; // Not used yet
        public int CHAR_CUSTOM_INDEX;
        public string GEAR_STATE = "Skateboard"; // Not used yet
        public int GEAR_CUSTOM_INDEX;

        // BUNDLES LISTS
        public List<AssetBundle> raveBundles = new List<AssetBundle>();
        public List<AssetBundle> inlineBundles = new List<AssetBundle>();
        public List<AssetBundle> skateboardBundles = new List<AssetBundle>();
        public List<AssetBundle> bmxBundles = new List<AssetBundle>();

        // GAMEOBJECTS LISTS
        public GameObject raveReference;
        public List<GameObject> inlineReferences = new List<GameObject>();
        public List<GameObject> skateboardReferences = new List<GameObject>();
        public List<GameObject> bmxReferences = new List<GameObject>();

        static public void log(string message) {
            Debug.Log($"<color=orange>[MeshRemix] {message}</color>");
        }

        void Awake() {
            instance = this;
            Log = this.Logger;
            //Log.LogInfo(PluginDir);
            //var harmony = new Harmony(MeshRemixInfos.PLUGIN_NAME);
            //harmony.PatchAll();

            log("MeshRemix is loaded !");

            // Init
            GEAR_CUSTOM_INDEX = 0;

            // Get All Bundles
            GetBUNDLES("Characters", "Rave", raveBundles);
            GetBUNDLES("Gears", "Inline", inlineBundles);
            GetBUNDLES("Gears", "Skateboard", skateboardBundles);
            GetBUNDLES("Gears", "BMX", bmxBundles);
        }

        void LateUpdate() {
            // Checker
            var worldHandler = WorldHandler.instance?.enabled;
            if (worldHandler != null && !CHECK) {
                GetPLAYER();
                GetOBJECTS(PLAYER.transform.GetChild(0));
                log("References have been collected !");
                StartCoroutine("SetASSETS");

                CHECK = true;
            }

            if (worldHandler == null) {
                inlineReferences.Clear();
                skateboardReferences.Clear();
                bmxReferences.Clear();
                CHECK = false;
            }
        }

        void Update() {
            // Inputs
            if (Input.GetKeyDown(KeyCode.PageUp)) {
                GEAR_CUSTOM_INDEX = Mathf.Clamp(GEAR_CUSTOM_INDEX + 1, 0, 2);
                SetGear(skateboardBundles, skateboardReferences, GEAR_CUSTOM_INDEX); // NOTE: I need to swap on the current Movestyle used !
            }

            if (Input.GetKeyDown(KeyCode.PageDown)) {
                GEAR_CUSTOM_INDEX = Mathf.Clamp(GEAR_CUSTOM_INDEX - 1, 0, 2);
                SetGear(skateboardBundles, skateboardReferences, GEAR_CUSTOM_INDEX);
            }
        }

        void GetBUNDLES(string parentFolder, string childFolder, List<AssetBundle> bundlesList) {
            string[] pathsList = Directory.GetFiles(Application.dataPath + "/../" + $"/ModdingFolder/{parentFolder}/{childFolder}", "*", SearchOption.AllDirectories);

            foreach (string path in pathsList) {
                AssetBundle bundle = AssetBundle.LoadFromFile(path);
                bundlesList.Add(bundle);
                log($"<b>{bundle.name}</b> bundle loaded !");
            }
        }

        void GetPLAYER() {
            PLAYER = GameObject.Find("Player_HUMAN0");
            log("Player_HUMAN0 is referenced !");
        }

        void GetOBJECTS(Transform parent, int level = 0) { // Recursive Search Function
            foreach (Transform child in parent) {
                // Characters
                if (child.name == "angel(Clone)")
                    raveReference = child.Find("mesh").gameObject;

                // Movestyles
                if (child.name == "skateLeft(Clone)")
                    inlineReferences.Add(child.gameObject);

                if (child.name == "skateRight(Clone)")
                    inlineReferences.Add(child.gameObject);

                if (child.name == "skateboard(Clone)")
                    skateboardReferences.Add(child.gameObject);

                if (child.name == "bmxFrame(Clone)")
                    bmxReferences.Add(child.gameObject);

                // Process next deeper level
                GetOBJECTS(child, level + 1);
            }
        }

        IEnumerator SetASSETS() {
            yield return new WaitForSeconds(0.1f); // Workaround, I'm waiting for lists to be complete

            // Characters
            if (raveBundles.Count > 0)
                SetCharacter(raveBundles, raveReference, 0);

            // Gears
            if (inlineBundles.Count > 0)
                SetGear(inlineBundles, inlineReferences, 0);

            if (skateboardBundles.Count > 0)
                SetGear(skateboardBundles, skateboardReferences, 0);

            if (bmxBundles.Count > 0)
                SetGear(bmxBundles, bmxReferences, 0);
        }

        GameObject plop = new GameObject();
        void SetCharacter(List<AssetBundle> bundlesList, GameObject reference, int index) {
            // Replace Character with the new Character
            
            Instantiate(plop);
            plop.transform.parent = PLAYER.transform;
            plop.AddComponent<SkinnedMeshRenderer>();
            plop.GetComponent<SkinnedMeshRenderer>().sharedMesh = bundlesList[index].LoadAsset<Mesh>("rave.fbx");

            reference.transform.GetComponent<SkinnedMeshRenderer>().sharedMesh = plop.GetComponent<SkinnedMeshRenderer>().sharedMesh;

            log($"Load: {bundlesList[index].name}");
        }

        void SetGear(List<AssetBundle> bundlesList, List<GameObject> references, int index) {
            // Load the first Asset (Mesh Type for now)
            Mesh _mesh = bundlesList[index].LoadAsset<Mesh>(bundlesList[index].GetAllAssetNames()[0]); 

            // Replace MoveStyle asset by the new MoveStyle Asset
            for (int i = 0; i < references.Count; i++) {
                // Mesh
                references[i].transform.GetComponent<MeshFilter>().mesh = _mesh;

                // White Particle Spawning Mesh (IMPORTANT: Mesh need to have Read/Write enable in the Import Settings of Unity)
                if (references[i].transform.childCount > 0) {
                    references[i].transform.GetChild(0).GetComponent<ParticleSystemRenderer>().mesh = _mesh;
                }
            }

            log($"Load: {bundlesList[index].name}");
        }
    }
}
