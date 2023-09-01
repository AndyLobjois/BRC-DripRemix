using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Reptile;
using BepInEx.Configuration;
using OBJImporter;

namespace DripRemix {

    public class GearHandler {

        public MoveStyle MOVESTYLE;
        public string movestyleName => MOVESTYLE.ToString().ToLower().FirstCharToUpper();
        public ConfigEntry<int> indexMeshConfig;
        public ConfigEntry<int> indexTextureConfig;
        public int INDEX_MESH { get { return indexMeshConfig.Value; } set { indexMeshConfig.Value = value; } }
        public int INDEX_TEXTURE = 0; /*{ get { return indexTextureConfig.Value; } set { indexTextureConfig.Value = value; } }*/
        public List<AssetFolder> FOLDERS = new List<AssetFolder>();
        public List<GameObject> REFERENCES = new List<GameObject>();

        public GearHandler(MoveStyle moveStyle) {
            indexMeshConfig = Main.Instance.Config.Bind<int>("General", $"{movestyleName}_IndexMesh", 0);
            indexTextureConfig = Main.Instance.Config.Bind<int>("General", $"{movestyleName}_IndexTexture", 0);
            this.MOVESTYLE = moveStyle;
        }

        public void GetAssets() {
            // Clean
            FOLDERS.Clear();

            // Search & Add
            DirectoryInfo[] folders = Main.GearsFolder.CreateSubdirectory(movestyleName).GetDirectories();
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

            // Index ???? I can't remember why it's needed
            //INDEX_MESH = Mathf.Clamp(INDEX_MESH, 0, FOLDERS.Count - 1);
            //INDEX_TEXTURE = Mathf.Clamp(INDEX_TEXTURE, 0, FOLDERS[INDEX_MESH].textures.Count - 1);

            // Log
            if (FOLDERS.Count > 0) {
                string _names = "";
                for (int i = 0; i < FOLDERS.Count; i++)
                    _names += $"\n   • {FOLDERS[i].name} by {FOLDERS[i].author}";
                Main.log($"{FOLDERS.Count} {movestyleName}(s) loaded ! {_names}");
            }    
        }

        public void SetMesh(int add) {
            // Check if there is at least something to change
            if (FOLDERS.Count > 0) {
                // Add value to the Index
                INDEX_MESH = Mathf.Clamp(INDEX_MESH + add, 0, FOLDERS.Count - 1);

                // Prepare the particle mesh before the foreach, if not it doesn't works
                Mesh particleBuffer;
                FOLDERS[INDEX_MESH].meshes.TryGetValue("particle", out particleBuffer);

                // Change every reference by the new model
                foreach (GameObject _ref in REFERENCES) {
                    // Load Meshes
                    Mesh meshBuffer;
                    FOLDERS[INDEX_MESH].meshes.TryGetValue(_ref.name, out meshBuffer);

                    // Assign
                    _ref.GetComponent<MeshFilter>().mesh = meshBuffer;

                    // Particle
                    if (_ref.name == "skateRight(Clone)" || _ref.name == "skateLeft(Clone)" || _ref.name == "skateboard(Clone)") {
                        if (_ref.transform.childCount > 0) { // Detect ParticleSystem
                            _ref.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().mesh = meshBuffer; //IMPORTANT: Mesh need to have Read/Write enable in the Import Settings of Unity
                            _ref.transform.GetChild(0).GetComponent<ParticleSystem>().Play();
                        }
                    } else if (_ref.name == "BmxFrame(Clone)") { // Because BMX is in multiple parts, the particle system use 1 specific merged mesh
                        if (_ref.transform.childCount > 0) { // Detect ParticleSystem
                            _ref.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().mesh = particleBuffer; //IMPORTANT: Mesh need to have Read/Write enable in the Import Settings of Unity
                            _ref.transform.GetChild(0).GetComponent<ParticleSystem>().Play();
                        }
                    }
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
