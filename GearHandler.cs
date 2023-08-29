﻿using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Reptile;
using BepInEx.Configuration;

namespace MeshRemix
{
    public class GearHandler
    {
        public MoveStyle moveStyle;
        public string MoveStyleName => moveStyle.ToString().ToLower().FirstCharToUpper();
        public ConfigEntry<int> indexConfig;
        public int Index { get { return indexConfig.Value; } set { indexConfig.Value = value; } }
        public List<AssetBundle> bundles = new List<AssetBundle>();
        public List<GameObject> refs = new List<GameObject>();
        public Texture texRef;


        
        public GearHandler(MoveStyle moveStyle)
        {
            indexConfig = MeshRemix.Instance.Config.Bind<int>("General", $"{MoveStyleName}_Index", 0);
            this.moveStyle = moveStyle;
        }


        public void GetBundles()
        {
            foreach(AssetBundle b in bundles)
            {
                b.Unload(true);
            }
            this.bundles.Clear();
            FileInfo[] pathsList = MeshRemix.GEARFOLDER.CreateSubdirectory(MoveStyleName).GetFiles("*", SearchOption.AllDirectories);
            foreach (FileInfo path in pathsList)
                bundles.Add(AssetBundle.LoadFromFile(path.FullName));


            Index = Mathf.Clamp(Index, 0, bundles.Count - 1);

            // Log
            string _names = "";
            for (int i = 0; i < bundles.Count; i++)
            {
                _names += "\n" + bundles[i].name;
            }
            MeshRemix.log($"{bundles.Count} {MoveStyleName}(s) loaded ! <color=grey>{_names}</color>");
        }

        public void ClearRefs()
        {
            refs.Clear();
            texRef = null;
        }

        public void AddReference(GameObject go, Texture texRef = null) {
            refs.Add(go);
            if (texRef != null)
            {
                this.texRef = texRef;
            }
        }

        public void SetGear(int add)
        {
            if (bundles.Count > 0)
            {
                Index = Mathf.Clamp(Index + add, 0, bundles.Count - 1);
                Swap();
            }
        }

        void Swap()
        {
            foreach (GameObject _ref in refs)
            {
                // Mesh
                Mesh _mesh = bundles[Index].LoadAsset<Mesh>(_ref.name + ".fbx");
                _ref.GetComponent<MeshFilter>().mesh = _mesh;

                // Particle
                Mesh _particle = bundles[Index].LoadAsset<Mesh>("particle.fbx");
                if (_ref.name == "skateRight(Clone)" || _ref.name == "skateLeft(Clone)" || _ref.name == "skateboard(Clone)")
                {
                    if (_ref.transform.childCount > 0)
                    { // Detect ParticleSystem
                        _ref.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().mesh = _mesh; //IMPORTANT: Mesh need to have Read/Write enable in the Import Settings of Unity
                        _ref.transform.GetChild(0).GetComponent<ParticleSystem>().Play();
                    }
                }
                else if (_ref.name == "BmxFrame(Clone)")
                { // Because BMX is in multiple parts, the particle system use 1 specific merged mesh
                    if (_ref.transform.childCount > 0)
                    { // Detect ParticleSystem
                        _ref.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().mesh = _particle; //IMPORTANT: Mesh need to have Read/Write enable in the Import Settings of Unity
                        _ref.transform.GetChild(0).GetComponent<ParticleSystem>().Play();
                    }
                }

                // Texture (Could be better !)
                MeshRenderer _renderer = _ref.GetComponent<MeshRenderer>();
                Texture _tex = bundles[Index].LoadAsset<Texture2D>("tex.png");
                if (_tex != null)
                {
                    _renderer.material.mainTexture = _tex;
                }
                else
                {
                    if (moveStyle == MoveStyle.INLINE)
                        _renderer.material.mainTexture = WorldHandler.instance.currentPlayer.MoveStylePropsPrefabs.skateL.GetComponent<MeshRenderer>().material.mainTexture;
                    if (moveStyle == MoveStyle.SKATEBOARD)
                        _renderer.material.mainTexture = WorldHandler.instance.currentPlayer.MoveStylePropsPrefabs.skateboard.GetComponent<MeshRenderer>().material.mainTexture;
                    if (moveStyle == MoveStyle.BMX)
                        _renderer.material.mainTexture = WorldHandler.instance.currentPlayer.MoveStylePropsPrefabs.bmxFrame.GetComponent<MeshRenderer>().material.mainTexture;
                }

            }

            MeshRemix.log($"Load: {bundles[Index].name}");
        }
    }
}