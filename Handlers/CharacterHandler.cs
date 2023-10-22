using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Reptile;

namespace DripRemix.Handlers {

    public class CharacterHandler : DripHandler {

        override public void GetAssets() {
            // Get Index
            GetIndex();

            // Clean
            FOLDERS.Clear();

            // Search & Add
            AssetFolder = Main.FolderCharacter.CreateSubdirectory(Main.CURRENTCHARACTER);
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

                FOLDERS.Add(new AssetFolder(folder, meshes, textures, emissions, parameters));
            }

            // Log
            LoadDetails("CHARACTER", Main.CURRENTCHARACTER);
        }

        override public void SetTexture(int add) {
            // Check if there is at least something to change
            if (FOLDERS.Count > 0) {
                // Add value to the Index
                INDEX_TEXTURE = Mathf.Clamp(INDEX_TEXTURE + add, 0, FOLDERS[0].textures.Count - 1);

                // Change every references by the new texture
                foreach (GameObject _ref in REFERENCES) {
                    SkinnedMeshRenderer smr = _ref.GetComponent<SkinnedMeshRenderer>();

                    try {
                        if (FOLDERS[0].parameters["forceTextureOverride"].ToLower() == "true") {
                            smr.material.mainTexture = FOLDERS[0].textures[INDEX_TEXTURE];
                            smr.material.SetTexture("_Emission", FOLDERS[0].emissions[INDEX_TEXTURE]);
                        } else {
                            Main.Log.LogError("Custom Character Model detected by vertex comparison ! Texture have not been changed !\n" +
                            $"Ignore this error if it's intended, or you can set [forceTextureOverride] to [True] in {FOLDERS[INDEX_MESH].directory.Parent.Name}\\{FOLDERS[INDEX_MESH].directory.Name}\\info.txt\n");
                        }
                    } catch {
                        Main.Log.LogError($"Missing info or wrong parameters : {FOLDERS[0].directory.Parent.Name}\\{FOLDERS[0].directory.Name}\\info.txt\n");
                    }
                }
            }
        }

        void GetIndex() {
            INDEX_MESH = Main.SAVE.SaveLines[Main.CURRENTCHARACTER].charMesh;
            INDEX_TEXTURE = Main.SAVE.SaveLines[Main.CURRENTCHARACTER].charTex;
        }
    }
}
