using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Reptile;

namespace DripRemix.Handlers {

    public class CharacterHandler : DripHandler {

        public Characters CURRENTCHARACTER;

        public Dictionary<Characters, int> VertexCounts = new Dictionary<Characters, int>()
        {
            { Characters.girl1, 2993 },
            { Characters.frank, 2306 },
            { Characters.ringdude, 2262 },
            { Characters.metalHead, 2793 },
            { Characters.blockGuy, 3275 },
            { Characters.spaceGirl, 3376 },
            { Characters.angel, 3103 },
            { Characters.eightBall, 3103 },
            { Characters.dummy, 2829 },
            { Characters.dj, 3312 },
            { Characters.medusa, 3154 },
            { Characters.boarder, 3028 },
            { Characters.headMan, 3108 },
            { Characters.prince, 2675 },
            { Characters.jetpackBossPlayer, 4603 },
            { Characters.legendFace, 2657 },
            { Characters.oldheadPlayer, 1819 },
            { Characters.robot, 5750 },
            { Characters.skate, 4869 },
            { Characters.wideKid, 2069 },
            { Characters.futureGirl, 3154 },
            { Characters.pufferGirl, 2915 },
            { Characters.bunGirl, 2517 },
            { Characters.headManNoJetpack, 2903 },
            { Characters.eightBallBoss, 3103 },
            { Characters.legendMetalHead, 2436 },
        };

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

                FOLDERS.Add(new AssetFolder(folder, meshes, textures, emissions, parameters));
            }

            // Log
            if (FOLDERS[0].textures.Count > 0) {
                string _names = "";
                for (int i = 0; i < FOLDERS[0].textures.Count; i++)
                    _names += $"\n   • {FOLDERS[0].textures[i].name}";
                Main.Log.LogMessage($"{FOLDERS[0].textures.Count} Skin(s) for {CharacterToString(CURRENTCHARACTER)} loaded ! {_names}\n");
            }
        }

        override public void SetTexture(int add) {
            // Check if there is at least something to change
            if (FOLDERS.Count > 0) {

                // Add value to the Index
                INDEX_TEXTURE = Mathf.Clamp(INDEX_TEXTURE + add, 0, FOLDERS[0].textures.Count - 1);

                // Change every references by the new texture
                foreach (GameObject _ref in REFERENCES) {
                    SkinnedMeshRenderer smr = _ref.GetComponent<SkinnedMeshRenderer>();

                    // Check if it's a Custom Model by comparing Vertex Count
                    if (smr.sharedMesh.vertexCount == VertexCounts[CURRENTCHARACTER]) {
                        smr.material.mainTexture = FOLDERS[0].textures[INDEX_TEXTURE];
                        smr.material.SetTexture("_Emission", FOLDERS[0].emissions[INDEX_TEXTURE]);
                    } else {
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
        }

        public static string CharacterToString(Characters character) {
            if (Main.CHARACTERMAPS.ContainsKey(character)) {
                return Main.CHARACTERMAPS[character];
            }
            return character.ToString();
        }
    }
}
