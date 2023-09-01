using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Reptile;
using BepInEx.Configuration;
using OBJImporter;

namespace DripRemix {

    public class PhoneHandler {

        public ConfigEntry<int> indexMeshConfig;
        public ConfigEntry<int> indexTextureConfig;
        public int INDEX_MESH = 0; //{ get { return indexMeshConfig.Value; } set { indexMeshConfig.Value = value; } }
        public int INDEX_TEXTURE = 0; /*{ get { return indexTextureConfig.Value; } set { indexTextureConfig.Value = value; } }*/
        public List<AssetFolder> FOLDERS = new List<AssetFolder>();
        public List<GameObject> REFERENCES = new List<GameObject>();

        public PhoneHandler() {
            //indexMeshConfig = Main.Instance.Config.Bind<int>("General", $"Phone_IndexMesh", 0);
            //indexTextureConfig = Main.Instance.Config.Bind<int>("General", $"Phone_IndexTexture", 0);
        }

        public void GetAssets() {
            // Clean
            FOLDERS.Clear();

            // Search & Add
            DirectoryInfo[] folders = Main.PhonesFolder.GetDirectories();
            foreach (DirectoryInfo folder in folders) {
                FileInfo[] files = folder.GetFiles("*", SearchOption.TopDirectoryOnly);

                string name = "";
                string author = "";
                Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();
                List<Texture> textures = new List<Texture>();
                List<Texture> emissions = new List<Texture>();

                foreach (FileInfo file in files) {
                    // Info
                    if (file.Extension == ".txt") {
                        string[] lines = File.ReadAllLines(file.FullName);
                        name = lines[0].Split('=')[1];
                        author = lines[1].Split('=')[1];
                    }

                    // Meshes
                    if (file.Extension == ".obj") {
                        meshes.Add(file.Name.Replace(file.Extension, ""), new OBJLoader().Load(file.FullName));
                    }

                    // Textures
                    if (file.Extension == ".png" || file.Extension == ".jpg") {
                        byte[] bytes = File.ReadAllBytes(file.FullName);
                        Texture2D img = new Texture2D(2, 2);
                        img.LoadImage(bytes);
                        img.name = file.Name;

                        if (file.Name.Contains("_emission")) {
                            for (int i = 0; i < textures.Count; i++) {
                                if (textures[i].name == file.Name.Replace("_emission", "")) {
                                    emissions[i] = img;
                                }
                            }
                        } else {
                            textures.Add(img);
                            emissions.Add(Texture2D.blackTexture);
                        }
                    }
                }

                FOLDERS.Add(new AssetFolder(name, author, meshes, textures, emissions));
            }

            // Log
            if (FOLDERS.Count > 0) {
                string _names = "";
                for (int i = 0; i < FOLDERS.Count; i++)
                    _names += $"\n   • {FOLDERS[i].name} by {FOLDERS[i].author}";
                Main.log($"{FOLDERS.Count} Phone(s) loaded ! {_names}");
            }
        }

        public void SetMesh(int add) {
            // Check if there is at least something to change
            if (FOLDERS.Count > 0) {
                // Add value to the Index
                INDEX_MESH = Mathf.Clamp(INDEX_MESH + add, 0, FOLDERS.Count - 1);

                // Change every reference by the new model
                foreach (GameObject _ref in REFERENCES) {
                    // Load Meshes
                    Mesh meshBuffer;
                    FOLDERS[INDEX_MESH].meshes.TryGetValue(_ref.name, out meshBuffer);

                    // Assign
                    _ref.GetComponent<MeshFilter>().mesh = meshBuffer;
                }
            }
        }

        public void SetTexture(int add) {
            // Check if there is at least something to change
            if (FOLDERS.Count > 0) {

                // Add value to the Index
                INDEX_TEXTURE = Mathf.Clamp(INDEX_TEXTURE + add, 0, FOLDERS[INDEX_MESH].textures.Count - 1);

                // Change every references by the new texture
                foreach (GameObject _ref in REFERENCES) {
                    _ref.GetComponent<MeshRenderer>().material.mainTexture = FOLDERS[INDEX_MESH].textures[INDEX_TEXTURE];
                    _ref.GetComponent<MeshRenderer>().material.SetTexture("_Emission", FOLDERS[INDEX_MESH].emissions[INDEX_TEXTURE]);
                }
            }
        }

    }
}
