using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Reptile;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/*
Models Mod for Bomb Rush Cyberfunk by Andy Hellgrim (Aru)

TODO:
    [X] Load the Plugin
    [X] Load the Bundles
    [X] Launch the main function only in Stage Scenes (update ?)
    [ ] Check the Bundle list for item types (characters, skateboard/skates/bmx) and swap them

SCENES:
    Bootstrap
    core
    DontDestroyOnLoad
    HideAndDontSave

    intro
    mainMenu
        Prelude
        DownHill
        Hideout
        Square
        Tower
        Mall
        Pyramid
        Osaka

 */

namespace MeshRemix {
    [BepInPlugin(MeshRemixInfos.PLUGIN_ID, MeshRemixInfos.PLUGIN_NAME, MeshRemixInfos.PLUGIN_VERSION)]
    //[BepInDependency(LLBML.PluginInfos.PLUGIN_ID, BepInDependency.DependencyFlags.HardDependency)]
    //[BepInDependency("no.mrgentle.plugins.llb.modmenu", BepInDependency.DependencyFlags.SoftDependency)]

    public class MeshRemixMain : BaseUnityPlugin {
        public static MeshRemixMain instance;
        //public static DirectoryInfo PluginDir => LLBML.Utils.ModdingFolder.GetModSubFolder(instance.Info);
        //internal static ManualLogSource Log { get; private set; }

        public Scene[] scenes;
        public Scene activeScene;
        public GameObject PLAYER;
        public AssetBundle BUNDLE;

        static public void log(string message) {
            Debug.Log($"[ModelsMod] {message}");
        }

        void Awake() {
            log("ModelsMod is loaded");
            activeScene = SceneManager.GetActiveScene();

            BUNDLE = AssetBundle.LoadFromFile(Paths.BepInExRootPath + "/plugins/BRC-ModelsMod/Assets/skateboard/skateboard");

            //instance = this;
            //Log = this.Logger;
            //Log.LogInfo(PluginDir);
            //var harmony = new Harmony(BundleFixInfos.PLUGIN_NAME);
            //harmony.PatchAll();
        }

        void Update() {
            Swap();
        }

        void Swap() {
            scenes = SceneManager.GetAllScenes();
            for (int i = 0; i < scenes.Length; i++) {
                if (scenes[i].name == "intro" || scenes[i].name == "mainMenu") {
                    return;
                } else if (!PLAYER) {
                    // Init PLAYER
                    PLAYER = GameObject.Find("Player_HUMAN0");
                    log("Player_HUMAN0 found !");

                    // Search for the Skateboard
                    RecursiveSearch(PLAYER.transform.GetChild(0).GetChild(PLAYER.transform.GetChild(0).childCount - 1), "skateboard");
                }
            }
        }


        void RecursiveSearch(Transform parent, string name, int level = 0) {
            foreach (Transform child in parent) {
                if (child.name == name) {
                    // Change the skateboard and particle
                    Mesh _mesh = BUNDLE.LoadAsset<Mesh>("skateboard");
                    child.GetChild(0).GetComponent<MeshFilter>().mesh = _mesh;
                    child.GetChild(0).GetChild(0).GetComponent<ParticleSystemRenderer>().mesh = _mesh; // IMPORTANT: Mesh need to have Read/Write enable in the Import Settings of Unity
                    log(child.GetChild(0).GetChild(0).name);

                    log($"SKATEBOARD FOUND → {child.parent.parent.name}/{child.parent.name}/{child.name}");
                }

                // Process next deeper level
                RecursiveSearch(child, name, level + 1);
            }
        }
    }
}
