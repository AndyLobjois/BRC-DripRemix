using UnityEngine;
using Reptile;

namespace DripRemix.Handlers {

    public class GearHandler : DripHandler {

        public MoveStyle MOVESTYLE;
        public string movestyleName => MOVESTYLE.ToString().ToLower().FirstCharToUpper();

        public GearHandler(MoveStyle moveStyle) : base ($"{WorldHandler.instance?.currentPlayer?.character}_{moveStyle}") {
            this.MOVESTYLE = moveStyle;
            AssetFolder = Main.GearsFolder.CreateSubdirectory(movestyleName);
        }

        override public void GetAssets() {
            base.GetAssets();

            // Log
            if (FOLDERS.Count > 0) {
                string _names = "";
                for (int i = 0; i < FOLDERS.Count; i++)
                    _names += $"\n   • {FOLDERS[i].name} by {FOLDERS[i].author}";
                Main.log($"{FOLDERS.Count} {movestyleName}(s) loaded ! {_names}");
            }    
        }

        override public void SetMesh(int indexMod) {
            // Check if there is at least something to change
            if (FOLDERS.Count > 0) {
                // Add value to the Index
                INDEX_MESH = Mathf.Clamp(INDEX_MESH + indexMod, 0, FOLDERS.Count - 1);

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

        override public void SetTexture(int indexMod) {
            // Check if there is at least something to change
            if (FOLDERS.Count > 0) {

                // Add value to the Index
                INDEX_TEXTURE = Mathf.Clamp(INDEX_TEXTURE + indexMod, 0, FOLDERS[INDEX_MESH].textures.Count - 1);

                // Change every references by the new texture
                foreach (GameObject _ref in REFERENCES) {
                    _ref.GetComponent<MeshRenderer>().material.mainTexture = FOLDERS[INDEX_MESH].textures[INDEX_TEXTURE];
                    _ref.GetComponent<MeshRenderer>().material.SetTexture("_Emission", FOLDERS[INDEX_MESH].emissions[INDEX_TEXTURE]);
                }
            }
        }
    }
}
