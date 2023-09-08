using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using BepInEx.Configuration;
using Reptile;
using OBJImporter;

namespace DripRemix.Handlers
{
    public class DripHandler
    {

        public List<AssetFolder> FOLDERS = new List<AssetFolder>();
        public List<GameObject> REFERENCES = new List<GameObject>();

        protected HandlerTypes type;

        public int INDEX_MESH {
            get {
                var currentChar = WorldHandler.instance?.currentPlayer?.character;
                return Main.Instance?.SavedIndexes[currentChar ?? Characters.NONE].Value.indexes[(int)type].Mesh ?? 0;
            }
            set {
                var currentChar = WorldHandler.instance?.currentPlayer?.character;
                var config = Main.Instance?.SavedIndexes[currentChar ?? Characters.NONE];
                if (config != null) {
                    HandlersConfig hc = config.Value;
                    hc.indexes[(int)type].Mesh = value;
                    config.Value = hc;
                    Main.Instance.Config.Save();
                }
            }
        }
        public int INDEX_TEXTURE {
            get {
                var currentChar = WorldHandler.instance?.currentPlayer?.character;
                return Main.Instance?.SavedIndexes[currentChar ?? Characters.NONE].Value.indexes[(int)type].Texture ?? 0;
            }
            set {
                var currentChar = WorldHandler.instance?.currentPlayer?.character;
                var config = Main.Instance?.SavedIndexes[currentChar ?? Characters.NONE];
                if (config != null) {
                    HandlersConfig hc = config.Value;
                    hc.indexes[(int)type].Texture = value;
                    config.Value = hc;
                    Main.Instance.Config.Save();
                }
            }
        }

        public DirectoryInfo AssetFolder { get; protected set; }

        public DripHandler(HandlerTypes type) {
            this.type = type;
        }

        virtual public void GetAssets() {
            // Clean
            FOLDERS.Clear();

            // Search & Add
            DirectoryInfo[] folders = AssetFolder.GetDirectories();
            foreach (DirectoryInfo folder in folders)
            {
                // Check if there is Folder to open
                if (Directory.GetFileSystemEntries(folder.FullName).Length != 0)
                {
                    FileInfo[] files = folder.GetFiles("*", SearchOption.TopDirectoryOnly);

                    Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();
                    List<Texture> textures = new List<Texture>();
                    List<Texture> emissions = new List<Texture>();
                    Dictionary<string, string> parameters = new Dictionary<string, string>();
                    List<Texture> sprites1 = new List<Texture>();
                    List<Texture> sprites2 = new List<Texture>();

                    foreach (FileInfo file in files) {
                        // Info
                        if (file.Extension == ".txt") {
                            parameters = GetParameters(file);
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
                            } else if (file.Name.Contains("_PhoneOpen")) {
                                for (int i = 0; i < textures.Count; i++) {
                                    if (textures[i].name == file.Name.Replace("_PhoneOpen", "")) {
                                        sprites1[i] = img;
                                    }
                                }
                            } else if (file.Name.Contains("_PhoneClosed")) {
                                for (int i = 0; i < textures.Count; i++) {
                                    if (textures[i].name == file.Name.Replace("_PhoneClosed", "")) {
                                        sprites2[i] = img;
                                    }
                                }
                            } else {
                                textures.Add(img);
                                emissions.Add(Texture2D.blackTexture);
                                sprites1.Add(Texture2D.blackTexture);
                                sprites2.Add(Texture2D.blackTexture);
                            }
                        }
                    }

                    FOLDERS.Add(new AssetFolder(meshes, textures, emissions, parameters, sprites1, sprites2));
                }
            }   
        }

        public Dictionary<string, string> GetParameters(FileInfo file) {

            Dictionary<string, string> _parameters = new Dictionary<string, string>();
            string[] _lines = File.ReadAllLines(file.FullName);
            foreach (string _line in _lines) {
                string[] _split = _line.Split('=');
                _parameters.Add(_split[0], _split[1]);
            }

            return _parameters;
        }

        virtual public void SetMesh(int indexMod) { }
        virtual public void SetTexture(int indexMod) { }

        virtual public void Reapply () {
            SetMesh(0);
            SetTexture(0);
        }
    }
}
