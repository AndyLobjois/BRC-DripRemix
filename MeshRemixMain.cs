﻿//using BRCML;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Reptile;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
Models Mod for Bomb Rush Cyberfunk by Andy Hellgrim (Aru)
*/

namespace MeshRemix {
    [BepInPlugin(MeshRemixInfos.PLUGIN_ID, MeshRemixInfos.PLUGIN_NAME, MeshRemixInfos.PLUGIN_VERSION)]
    //[BepInDependency(BRCML.PluginInfos.PLUGIN_ID, BepInDependency.DependencyFlags.HardDependency)]
    //[BepInDependency("fr.glomzubuk.plugins.brc.brcml", BepInDependency.DependencyFlags.HardDependency)]
    

    public class MeshRemixMain : BaseUnityPlugin {
        //public static MeshRemixMain instance;
        //public static DirectoryInfo PluginDir => BRCML.Utils.ModdingFolder.GetModSubFolder(instance.Info);
        //internal static ManualLogSource Log { get; private set; }

        // Core
        public GameObject PLAYER;
        public AssetBundle[] BUNDLE = new AssetBundle[1];
        bool CHECK = false;

        // Asset References
        public bool AssetsHaveBeenChecked = false;
        public List<GameObject> characters = new List<GameObject>();
        public List<GameObject> skates = new List<GameObject>(); //skateLeft(Clone) & skateRight(Clone)
        public List<GameObject> skateboards = new List<GameObject>(); //skateboard
        public List<GameObject> bmxs = new List<GameObject>(); //bmxFrame

        static public void log(string message) {
            Debug.Log($"[MeshRemix] {message}");
        }

        void Awake() {
            // BepInEx Stuff
            //instance = this;
            //Log = this.Logger;
            //Log.LogInfo(PluginDir);
            //var harmony = new Harmony(MeshRemixInfos.PLUGIN_NAME);
            //harmony.PatchAll();
            log("MeshRemix is loaded !");

            // Get Bundles
            BUNDLE[0] = AssetBundle.LoadFromFile(Paths.BepInExRootPath + $"/plugins/BRC-{MeshRemixInfos.PLUGIN_NAME}/Assets/skateboard/skateboard");
        }

        void LateUpdate() {
            var worldHandler = WorldHandler.instance?.enabled;
            if (worldHandler != null && !CHECK) {
                GetPLAYER();
                GetASSETS(PLAYER.transform.GetChild(0));
                log("Assets have been collected !");
                StartCoroutine("SetASSETS");

                CHECK = true;
            }

            if (worldHandler == null) {
                skates.Clear();
                skateboards.Clear();
                bmxs.Clear();
                CHECK = false;
            }
        }

        void GetPLAYER() {
            PLAYER = GameObject.Find("Player_HUMAN0");
            log("Player_HUMAN0 is referenced !");
        }

        
        void GetASSETS(Transform parent, int level = 0) { // Recursive Search Function
            foreach (Transform child in parent) {
                if (child.name == "skateLeft(Clone)")
                    skates.Add(child.gameObject);

                if (child.name == "skateRight(Clone)")
                    skates.Add(child.gameObject);

                if (child.name == "skateboard(Clone)")
                    skateboards.Add(child.gameObject);

                if (child.name == "bmxFrame(Clone)")
                    bmxs.Add(child.gameObject);

                // Process next deeper level
                GetASSETS(child, level + 1);
            }
        }

        IEnumerator SetASSETS() {
            yield return new WaitForSeconds(0.1f); // Workaround, I'm waiting for lists to be complete
            Mesh skateboardMesh = BUNDLE[0].LoadAsset<Mesh>("skateboard");

            for (int i = 0; i < skateboards.Count; i++) {
                // Mesh
                skateboards[i].transform.GetComponent<MeshFilter>().mesh = skateboardMesh;

                // White Particle Spawning Mesh (IMPORTANT: Mesh need to have Read/Write enable in the Import Settings of Unity)
                if (skateboards[i].transform.childCount > 0) {
                    skateboards[i].transform.GetChild(0).GetComponent<ParticleSystemRenderer>().mesh = skateboardMesh;
                }
            }

            log($"'skateboard' have been swapped !");
        }
    }
}
