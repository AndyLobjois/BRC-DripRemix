using UnityEngine;
using Reptile;

namespace DripRemix.Handlers {

    public class SpraycanHandler : DripHandler {

        public SpraycanHandler() : base($"{WorldHandler.instance?.currentPlayer?.character}_SPRAYCAN") {
            AssetFolder = Main.SpraycansFolder;
        }

        override public void GetAssets() {
            base.GetAssets();

            // Log
            if (FOLDERS.Count > 0) {
                string _names = "";
                for (int i = 0; i < FOLDERS.Count; i++)
                    _names += $"\n   • {FOLDERS[i].name} by {FOLDERS[i].author}";
                Main.log($"{FOLDERS.Count} Spraycan(s) loaded ! {_names}");
            }
        }

        override public void SetMesh(int indexMod) {
            // Check if there is at least something to change
            if (FOLDERS.Count > 0) {
                // Add value to the Index
                INDEX_MESH = Mathf.Clamp(INDEX_MESH + indexMod, 0, FOLDERS.Count - 1);

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

        override public void SetTexture(int indexMod) {
            // Check if there is at least something to change
            if (FOLDERS.Count > 0) {

                // Add value to the Index
                INDEX_TEXTURE = Mathf.Clamp(INDEX_TEXTURE + indexMod, 0, FOLDERS[INDEX_MESH].textures.Count - 1);

                // Change every references by the new texture
                foreach (GameObject _ref in REFERENCES) {
                    _ref.GetComponent<MeshRenderer>().material.mainTexture = FOLDERS[INDEX_MESH].textures[INDEX_TEXTURE];
                    _ref.GetComponent<MeshRenderer>().material.SetTexture("_Emission", FOLDERS[INDEX_MESH].emissions[INDEX_TEXTURE]);

                    // Change Color Spray
                    if (_ref.name == "spraycan(Clone)") {
                        Texture2D _tex = FOLDERS[INDEX_MESH].textures[INDEX_TEXTURE] as Texture2D;
                        _ref.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material.color = _tex.GetPixel(1, 1);
                    }
                }
            }
        }

    }
}
