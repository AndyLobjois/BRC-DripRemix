using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Reptile;
using BepInEx.Configuration;


namespace DripRemix {

    public class CharacterHandler {

        public Characters CHARACTER;
        
        //public ConfigEntry<int> indexMeshConfig;
        public ConfigEntry<int> indexTextureConfig;
        //public int INDEX_MESH { get { return indexMeshConfig.Value; } set { indexMeshConfig.Value = value; } }
        public int INDEX_TEXTURE = 0; /*{ get { return indexTextureConfig.Value; } set { indexTextureConfig.Value = value; } }*/
        public List<AssetFolder> FOLDERS = new List<AssetFolder>();
        public List<GameObject> REFERENCES = new List<GameObject>();

        public CharacterHandler(Characters character) {
            //indexMeshConfig = Main.Instance.Config.Bind<int>("General", $"{CharacterToString(CHARACTER)}_IndexMesh", 0);
            indexTextureConfig = Main.Instance.Config.Bind<int>("General", $"{CharacterToString(CHARACTER)}_IndexTexture", 0);
            this.CHARACTER = character;
        }

        public void GetAssets() {
            // Clean
            FOLDERS.Clear();

            // Search & Add
            DirectoryInfo[] folders = Main.CharactersFolder.CreateSubdirectory(CharacterToString(CHARACTER)).GetDirectories();
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

                    // Meshes → No Mesh replacement for now, so it's useless
                    //if (file.Extension == ".obj") {
                    //    meshes.Add(file.Name.Replace(file.Extension, ""), new OBJLoader().Load(file.FullName)); // ERROR I've got a duplicate names in the Key dictionnary ??
                    //}

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
                //string _names = "";
                //for (int i = 0; i < FOLDERS.Count; i++)
                //    _names += $"\n   • {FOLDERS[i].name} by {FOLDERS[i].author}";
                Main.log($"{FOLDERS[0].textures.Count} Skin(s) for {CharacterToString(CHARACTER)} loaded !"); //{_names}
            }
        }

        public void SetTexture(int add) {
            // Check if there is at least something to change
            if (FOLDERS.Count > 0) {

                // Add value to the Index
                INDEX_TEXTURE = Mathf.Clamp(INDEX_TEXTURE + add, 0, FOLDERS[0].textures.Count - 1);

                // Change every references by the new texture
                foreach (GameObject _ref in REFERENCES) {
                    _ref.GetComponent<SkinnedMeshRenderer>().material.mainTexture = FOLDERS[0].textures[INDEX_TEXTURE];
                    _ref.GetComponent<SkinnedMeshRenderer>().material.SetTexture("_Emission", FOLDERS[0].emissions[INDEX_TEXTURE]);
                }
            }
        }

        public string CharacterToString(Characters character) {
            if (Main.CHARACTERMAPS.ContainsKey(character)) {
                return Main.CHARACTERMAPS[character];
            }
            return character.ToString();
        }
    }
}
