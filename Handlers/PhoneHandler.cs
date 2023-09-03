using UnityEngine;
using UnityEngine.UI;
using Reptile;

namespace DripRemix.Handlers {

    public class PhoneHandler : DripHandler {

        public PhoneHandler() : base(HandlerTypes.Phone) {
            AssetFolder = Main.PhonesFolder;
        }

        override public void GetAssets() {
            base.GetAssets();

            // Log
            if (FOLDERS.Count > 0) {
                string _names = "";
                for (int i = 0; i < FOLDERS.Count; i++)
                    _names += $"\n   • {FOLDERS[i].name} by {FOLDERS[i].author}";
                Main.log($"{FOLDERS.Count} Phone(s) loaded ! {_names}");
            }
        }

        override public void SetMesh(int indexMod) {
            // Check if there is at least something to change
            if (FOLDERS.Count > 0) {
                // Add value to the Index
                INDEX_MESH = Mathf.Clamp(INDEX_MESH + indexMod, 0, FOLDERS.Count - 1);

                // Change every reference by the new model
                foreach (GameObject _ref in REFERENCES) {
                    if (_ref.GetComponent<MeshFilter>()) {
                        // Load Meshes
                        Mesh meshBuffer;
                        FOLDERS[INDEX_MESH].meshes.TryGetValue(_ref.name, out meshBuffer);

                        // Assign
                        _ref.GetComponent<MeshFilter>().mesh = meshBuffer;
                    }
                }
            }
        }

        override public void SetTexture(int indexMod) {
            // Check if there is at least something to change
            if (FOLDERS.Count > 0) {
                // Add value to the Index
                INDEX_TEXTURE = Mathf.Clamp(INDEX_TEXTURE + indexMod, 0, FOLDERS[INDEX_MESH].textures.Count - 1);

                // Change every references by the new texture
                foreach (GameObject _ref in REFERENCES) {
                    if (_ref.name == "PhoneOpen") {
                        Texture2D tex = FOLDERS[INDEX_MESH].sprites1[INDEX_TEXTURE] as Texture2D;
                        _ref.GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                    }
                    else if (_ref.name == "PhoneClosed") {
                        Texture2D tex = FOLDERS[INDEX_MESH].sprites2[INDEX_TEXTURE] as Texture2D;
                        _ref.GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                    }
                    else {
                        _ref.GetComponent<MeshRenderer>().material.mainTexture = FOLDERS[INDEX_MESH].textures[INDEX_TEXTURE];
                        _ref.GetComponent<MeshRenderer>().material.SetTexture("_Emission", FOLDERS[INDEX_MESH].emissions[INDEX_TEXTURE]);
                    }
                }
            }
        }

    }
}
