using UnityEngine;
using Reptile;

namespace DripRemix.Handlers {

    public class SpraycanHandler : DripHandler {

        public SpraycanHandler() : base(HandlerTypes.SprayCan) {
            AssetFolder = Main.SpraycansFolder;
        }

        override public void GetAssets() {
            base.GetAssets();

            // Log
            if (FOLDERS.Count > 0) {
                string _names = "";
                for (int i = 0; i < FOLDERS.Count; i++)
                    _names += $"\n   • {FOLDERS[i].parameters["name"]} by {FOLDERS[i].parameters["author"]}";
                Main.Log.LogMessage($"{FOLDERS.Count} Spraycan(s) loaded ! {_names}\n");
            }
        }

        override public void SetMesh(int indexMod) {
            // Check if there is at least something to change
            if (FOLDERS.Count > 0) {
                // Add value to the Index
                INDEX_MESH = Mathf.Clamp(INDEX_MESH + indexMod, 0, FOLDERS.Count - 1);

                // Change every reference by the new model
                foreach (GameObject _ref in REFERENCES) {
                    try {
                        // Load Meshes
                        Mesh meshBuffer = FOLDERS[INDEX_MESH].meshes[_ref.name];

                        // Assign
                        _ref.GetComponent<MeshFilter>().mesh = meshBuffer;

                        // Reload Texture (It'll avoid out of range textures[])
                        SetTexture(0);
                    } catch {
                        Main.Log.LogError($"Missing mesh : {FOLDERS[INDEX_MESH].directory.Parent.Name}\\{FOLDERS[INDEX_MESH].directory.Name}/{ _ref.name}");
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
                    try {
                        _ref.GetComponent<MeshRenderer>().material.mainTexture = FOLDERS[INDEX_MESH].textures[INDEX_TEXTURE];
                        _ref.GetComponent<MeshRenderer>().material.SetTexture("_Emission", FOLDERS[INDEX_MESH].emissions[INDEX_TEXTURE]);

                        // Change Color Spray
                        if (_ref.name == "spraycan(Clone)") {
                            Texture2D _tex = FOLDERS[INDEX_MESH].textures[INDEX_TEXTURE] as Texture2D;
                            _ref.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material.color = _tex.GetPixel(0, 0);
                        }
                    } catch {
                        Main.Log.LogError($"Missing texture : {FOLDERS[INDEX_MESH].directory.Parent.Name}\\{FOLDERS[INDEX_MESH].directory.Name}/{ _ref.name}");
                    }
                }
            }
        }

        public void SetGraffitiEffect() {
            try {
                // Graffiti Effects
                GraffitiEffect[] effects = GameObject.FindObjectsOfType<GraffitiEffect>();

                foreach (GraffitiEffect effect in effects) {
                    // This effect is a mix of 2D Unlit textures and mesh "clouds"
                    // It's hard to color edit so I just disable it :D
                    if (effect.name == "grafExplosionEffectBig(Clone)") {
                        effect.gameObject.SetActive(false);
                    }

                    // Splat, Splashes and Strokes
                    if (effect.splat) {
                        effect.splat.startColor = Color.white;
                        Texture2D _tex = FOLDERS[INDEX_MESH].textures[INDEX_TEXTURE] as Texture2D;
                        effect.splat.GetComponent<ParticleSystemRenderer>().material.color = _tex.GetPixel(1, 1);
                    }

                    if (effect.splashes) {
                        effect.splashes.startColor = Color.white;
                        Texture2D _tex = FOLDERS[INDEX_MESH].textures[INDEX_TEXTURE] as Texture2D;
                        effect.splashes.GetComponent<ParticleSystemRenderer>().material.color = _tex.GetPixel(1, 1);
                    }

                    if (effect.strokes) {
                        effect.strokes.startColor = Color.white;
                        Texture2D _tex = FOLDERS[INDEX_MESH].textures[INDEX_TEXTURE] as Texture2D;
                        effect.strokes.GetComponent<ParticleSystemRenderer>().material.color = _tex.GetPixel(1, 1);
                    }
                }
            } catch {
                Main.Log.LogError($"GraffitiEffect issues !");
            }
        }

    }
}
