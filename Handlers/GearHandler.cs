using System;
using UnityEngine;
using Reptile;

namespace DripRemix.Handlers {

    public class GearHandler : DripHandler {

        public MoveStyle MOVESTYLE;
        public string movestyleName => MOVESTYLE.ToString().ToLower().FirstCharToUpper();

        public GearHandler(MoveStyle moveStyle) {
            this.MOVESTYLE = moveStyle;
        }

        override public void GetAssets() {
            GetIndex();
            AssetFolder = Main.FolderGears.CreateSubdirectory(movestyleName);
            base.GetAssets();
            LoadDetails(MOVESTYLE.ToString().ToLower().FirstCharToUpper(), null);
        }

        override public void SetMesh(int add) {
            // Check if there is at least something to change
            if (FOLDERS.Count > 0) {
                // Add value to the Index
                INDEX_MESH = Mathf.Clamp(INDEX_MESH + add, 0, FOLDERS.Count - 1);

                // Change every reference by the new model
                foreach (GameObject _ref in REFERENCES) {
                    Mesh meshBuffer = null;
                    Mesh particleBuffer = null;

                    // Load Mesh
                    try {
                        meshBuffer = FOLDERS[INDEX_MESH].meshes[_ref.name];
                    } catch {
                        Main.Log.LogError($"Missing mesh : {FOLDERS[INDEX_MESH].directory.Parent.Name}\\{FOLDERS[INDEX_MESH].directory.Name}\\{_ref.name}");
                    }

                    // Load Particle Mesh
                    if (MOVESTYLE == MoveStyle.BMX) {
                        try {
                            particleBuffer = FOLDERS[INDEX_MESH].meshes["particle"];
                        } catch {
                            Main.Log.LogError($"Missing mesh : {FOLDERS[INDEX_MESH].directory.Parent.Name}\\{FOLDERS[INDEX_MESH].directory.Name}\\particle");
                        }
                    }

                    // Assign
                    _ref.GetComponent<MeshFilter>().mesh = meshBuffer;

                    // Reload Texture (It'll avoid out of range textures[])
                    SetTexture(0);

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

        override public void SetTexture(int add) {
            // Check if there is at least something to change
            if (FOLDERS.Count > 0) {
                // Add value to the Index
                INDEX_TEXTURE = Mathf.Clamp(INDEX_TEXTURE + add, 0, FOLDERS[INDEX_MESH].textures.Count - 1);

                // Change every references by the new texture
                foreach (GameObject _ref in REFERENCES) {
                    try {
                        foreach (Material mat in _ref.GetComponent<MeshRenderer>().materials) {
                            mat.mainTexture = FOLDERS[INDEX_MESH].textures[INDEX_TEXTURE];
                            mat.SetTexture("_Emission", FOLDERS[INDEX_MESH].emissions[INDEX_TEXTURE]);
                        }
                    } catch {
                        Main.Log.LogError($"Missing texture : {FOLDERS[INDEX_MESH].directory.Parent.Name}\\{FOLDERS[INDEX_MESH].directory.Name}\\{_ref.name}");
                    }
                }
            }
        }

        void GetIndex() {
            if (MOVESTYLE == MoveStyle.INLINE) {
                INDEX_MESH = Main.SAVE.SaveLines[Main.CURRENTCHARACTER].inlineMesh;
                INDEX_TEXTURE = Main.SAVE.SaveLines[Main.CURRENTCHARACTER].inlineTex;
            }
            if (MOVESTYLE == MoveStyle.SKATEBOARD) {
                INDEX_MESH = Main.SAVE.SaveLines[Main.CURRENTCHARACTER].skateboardMesh;
                INDEX_TEXTURE = Main.SAVE.SaveLines[Main.CURRENTCHARACTER].skateboardTex;
            }
            if (MOVESTYLE == MoveStyle.BMX) {
                INDEX_MESH = Main.SAVE.SaveLines[Main.CURRENTCHARACTER].bmxMesh;
                INDEX_TEXTURE = Main.SAVE.SaveLines[Main.CURRENTCHARACTER].bmxTex;
            }
        }
    }
}
