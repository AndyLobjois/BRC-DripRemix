﻿using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using BepInEx.Configuration;
using OBJImporter;

namespace DripRemix.Handlers
{
    public class DripHandler
    {
        public ConfigEntry<int> indexMeshConfig;
        public ConfigEntry<int> indexTextureConfig;

        public List<AssetFolder> FOLDERS = new List<AssetFolder>();
        public List<GameObject> REFERENCES = new List<GameObject>();

        public int INDEX_MESH { get { return indexMeshConfig?.Value ?? 0; } set { if (indexMeshConfig != null) indexMeshConfig.Value = value; } }
        public int INDEX_TEXTURE { get { return indexTextureConfig?.Value ?? 0; } set { if (indexTextureConfig != null) indexTextureConfig.Value = value; } }


        public DirectoryInfo AssetFolder { get; protected set; }

        public DripHandler(string configPath) {
            if (configPath != null)
            {
                indexMeshConfig = Main.Instance?.Config.Bind<int>("Save Index", $"{configPath}_Mesh_Index", 0);
                indexTextureConfig = Main.Instance?.Config.Bind<int>("Save Index", $"{configPath}_Texture_Index", 0);
            }
        }

        virtual public void GetAssets() {
            // Clean
            FOLDERS.Clear();

            // Search & Add
            DirectoryInfo[] folders = AssetFolder.GetDirectories();
            foreach (DirectoryInfo folder in folders)
            {
                FileInfo[] files = folder.GetFiles("*", SearchOption.TopDirectoryOnly);

                string name = "";
                string author = "";
                Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();
                List<Texture> textures = new List<Texture>();
                List<Texture> emissions = new List<Texture>();

                foreach (FileInfo file in files)
                {
                    // Info
                    if (file.Extension == ".txt")
                    {
                        string[] lines = File.ReadAllLines(file.FullName);
                        name = lines[0].Split('=')[1];
                        author = lines[1].Split('=')[1];
                    }

                    // Meshes
                    if (file.Extension == ".obj")
                    {
                        meshes.Add(file.Name.Replace(file.Extension, ""), new OBJLoader().Load(file.FullName));
                    }

                    // Textures
                    if (file.Extension == ".png" || file.Extension == ".jpg")
                    {
                        byte[] bytes = File.ReadAllBytes(file.FullName);
                        Texture2D img = new Texture2D(2, 2);
                        img.LoadImage(bytes);
                        img.name = file.Name;

                        if (file.Name.Contains("_emission"))
                        {
                            for (int i = 0; i < textures.Count; i++)
                            {
                                if (textures[i].name == file.Name.Replace("_emission", ""))
                                {
                                    emissions[i] = img;
                                }
                            }
                        }
                        else
                        {
                            textures.Add(img);
                            emissions.Add(Texture2D.blackTexture);
                        }
                    }
                }

                FOLDERS.Add(new AssetFolder(name, author, meshes, textures, emissions));
            }

            // Index ???? I can't remember why it's needed 
            // It's needed for a reload to keep the index in-bounds in case the user removes one

            INDEX_MESH = Mathf.Clamp(INDEX_MESH, 0, FOLDERS.Count - 1);
            INDEX_TEXTURE = Mathf.Clamp(INDEX_TEXTURE, 0, FOLDERS[INDEX_MESH].textures.Count - 1);
        }

        virtual public void SetMesh(int indexMod) { }
        virtual public void SetTexture(int indexMod) { }

        virtual public void Reapply () {
            SetMesh(0);
            SetTexture(0);
        }
    }
}