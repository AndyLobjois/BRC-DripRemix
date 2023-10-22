using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using Reptile;

namespace DripRemix.Handlers {

    public class PhoneHandler : DripHandler {

        override public void GetAssets() {
            GetIndex();
            AssetFolder = Main.FolderPhone;
            base.GetAssets();
            LoadDetails("Phone", null);

            try {
                SetCameras();
            } catch {
                Main.Log.LogError($"Can't set Phone Cameras. Please, verify the parameters of {FOLDERS[INDEX_MESH].directory.Parent.Name}\\{FOLDERS[INDEX_MESH].directory.Name}\\info.txt");
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
                        if (_ref.GetComponent<MeshFilter>()) {
                            // Load Meshes
                            Mesh meshBuffer = FOLDERS[INDEX_MESH].meshes[_ref.name];

                            // Assign
                            _ref.GetComponent<MeshFilter>().mesh = meshBuffer;

                            // Reload Texture (It'll avoid out of range textures[])
                            SetTexture(0);

                            // Mirror the phone
                            _ref.transform.localScale = new Vector3(1, 1, -1);
                        }
                    } catch {
                        Main.Log.LogError($"Missing mesh : {FOLDERS[INDEX_MESH].directory.Parent.Name}\\{FOLDERS[INDEX_MESH].directory.Name}\\{ _ref.name}");
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
                        if (_ref.name == "PhoneOpen") {
                            Texture2D tex = FOLDERS[INDEX_MESH].sprites1[INDEX_TEXTURE] as Texture2D;
                            _ref.GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                        } else if (_ref.name == "PhoneClosed") {
                            Texture2D tex = FOLDERS[INDEX_MESH].sprites2[INDEX_TEXTURE] as Texture2D;
                            _ref.GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                        } else {
                            _ref.GetComponent<MeshRenderer>().material.mainTexture = FOLDERS[INDEX_MESH].textures[INDEX_TEXTURE];
                            _ref.GetComponent<MeshRenderer>().material.SetTexture("_Emission", FOLDERS[INDEX_MESH].emissions[INDEX_TEXTURE]);
                        }
                    } catch {
                        Main.Log.LogError($"Missing texture : {FOLDERS[INDEX_MESH].directory.Parent.Name}\\{FOLDERS[INDEX_MESH].directory.Name}\\{ _ref.name}");
                    }
                }
            }
        }

        public void SetCameras() {
            CharacterVisual visual = WorldHandler.instance.currentPlayer.characterVisual;
            GameObject screen = visual.handL.Find("propl/phoneInHand(Clone)/Screen").gameObject;
            GameObject cameraFront = visual.handL.Find("propl/phoneInHand(Clone)/phoneCameras/frontCamera").gameObject;
            GameObject cameraRear = visual.handL.Find("propl/phoneInHand(Clone)/phoneCameras/rearCamera").gameObject;
            string[] split;

            // Screen
            screen.SetActive(false);

            // Set Camera Front Position/Rotation/FOV
            cameraFront.GetComponent<Camera>().fieldOfView = float.Parse(FOLDERS[INDEX_MESH].parameters["cameraFront_fov"]);
            split = FOLDERS[INDEX_MESH].parameters["cameraFront_position"].Split(',');
            cameraFront.transform.localPosition = new Vector3(
                float.Parse(split[0], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(split[1], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(split[2], CultureInfo.InvariantCulture.NumberFormat)
            );
            split = FOLDERS[INDEX_MESH].parameters["cameraFront_rotation"].Split(',');
            cameraFront.transform.localEulerAngles = new Vector3(
                float.Parse(split[0], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(split[1], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(split[2], CultureInfo.InvariantCulture.NumberFormat)
            );

            // Set Camera Rear Position/Rotation/FOV
            cameraRear.GetComponent<Camera>().fieldOfView = float.Parse(FOLDERS[INDEX_MESH].parameters["cameraRear_fov"]);
            split = FOLDERS[INDEX_MESH].parameters["cameraRear_position"].Split(',');
            cameraRear.transform.localPosition = new Vector3(
                float.Parse(split[0], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(split[1], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(split[2], CultureInfo.InvariantCulture.NumberFormat)
            );
            split = FOLDERS[INDEX_MESH].parameters["cameraRear_rotation"].Split(',');
            cameraRear.transform.localEulerAngles = new Vector3(
                float.Parse(split[0], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(split[1], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(split[2], CultureInfo.InvariantCulture.NumberFormat)
            );
        }

        void GetIndex() {
            INDEX_MESH = Main.SAVE.SaveLines[Main.CURRENTCHARACTER].phoneMesh;
            INDEX_TEXTURE = Main.SAVE.SaveLines[Main.CURRENTCHARACTER].phoneTex;
        }
    }
}
