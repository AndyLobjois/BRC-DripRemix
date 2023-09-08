using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Reptile;

namespace DripRemix.Handlers {

    public class CharacterHandler : DripHandler {

        public Characters CURRENTCHARACTER;

        public CharacterHandler(/*Characters character*/) : base(HandlerTypes.Character) {
            //this.CURRENTCHARACTER = character;
            //AssetFolder = Main.CharactersFolder.CreateSubdirectory(CharacterToString(CURRENTCHARACTER));
        }

        override public void GetAssets() {
            // Clean
            FOLDERS.Clear();

            // Get Current Character
            CURRENTCHARACTER = WorldHandler.instance.currentPlayer.character;
            AssetFolder = Main.CharactersFolder.CreateSubdirectory(CharacterToString(CURRENTCHARACTER));

            // Search & Add
            DirectoryInfo[] folders = AssetFolder.GetDirectories();
            foreach (DirectoryInfo folder in folders) {
                FileInfo[] files = folder.GetFiles("*", SearchOption.TopDirectoryOnly);

                Dictionary<string, string> parameters = new Dictionary<string, string>();
                Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();
                List<Texture> textures = new List<Texture>();
                List<Texture> emissions = new List<Texture>();

                foreach (FileInfo file in files) {
                    // Info
                    if (file.Extension == ".txt") {
                        parameters = GetParameters(file);
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

                FOLDERS.Add(new AssetFolder(meshes, textures, emissions, parameters));
            }

            // Log
            if (FOLDERS[0].textures.Count > 0) {
                string _names = "";
                for (int i = 0; i < FOLDERS[0].textures.Count; i++)
                    _names += $"\n   • {FOLDERS[0].textures[i].name}";
                Main.log($"{FOLDERS[0].textures.Count} Skin(s) for {CharacterToString(CURRENTCHARACTER)} loaded ! {_names}\n");
            }
        }

        override public void SetTexture(int add) {
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

        public static string CharacterToString(Characters character) {
            if (Main.CHARACTERMAPS.ContainsKey(character)) {
                return Main.CHARACTERMAPS[character];
            }
            return character.ToString();
        }
    }
}
