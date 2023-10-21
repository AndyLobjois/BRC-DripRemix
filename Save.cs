using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using BepInEx.Configuration;
using Reptile;
using System.Security.Cryptography;

namespace DripRemix {
    public class Save {

        public Dictionary<string, SaveLine> SaveLines = new Dictionary<string, SaveLine>();
        
        public void GetSave() {
            // Clean
            SaveLines.Clear();

            // Check Save File
            if (File.Exists(BepInEx.Paths.PluginPath + "/BRC-DripRemix/SAVE")) {
                // Get Save
                FileInfo file = new FileInfo(BepInEx.Paths.PluginPath + "/BRC-DripRemix/SAVE");
                string[] allLines = File.ReadAllLines(file.FullName);

                // Get Data
                foreach (string line in allLines) {
                    string[] item = line.Split(',');

                    SaveLines.Add(item[0], new SaveLine(            // Name
                        int.Parse(item[1]), int.Parse(item[2]),     // Character Mesh/Tex
                        int.Parse(item[3]), int.Parse(item[4]),     // Inline Mesh/Tex
                        int.Parse(item[5]), int.Parse(item[6]),     // Skateboard Mesh/Tex
                        int.Parse(item[7]), int.Parse(item[8]),     // BMX Mesh/Tex
                        int.Parse(item[9]), int.Parse(item[10]),    // Phone Mesh/Tex
                        int.Parse(item[11]), int.Parse(item[12])    // Spraycan Mesh/Tex
                        ));
                }
            } else {
                File.Create(BepInEx.Paths.PluginPath + "/BRC-DripRemix/SAVE");
                Main.Log.LogError("Missing Save File. Empty Save File have been created !");
            }
        }

        public void SetSave() {
            //SaveLines[Main.CURRENTCHARACTER]
        }

    }

    public class SaveLine {
        public int charMesh = 0;
        public int charTex = 0;
        public int inlineMesh = 0;
        public int inlineTex = 0;
        public int skateboardMesh = 0;
        public int skateboardTex = 0;
        public int bmxMesh = 0;
        public int bmxTex = 0;
        public int phoneMesh = 0;
        public int phoneTex = 0;
        public int spraycanMesh = 0;
        public int spraycanTex = 0;

        public SaveLine(int charMesh, int charTex, int inlineMesh, int inlineTex, int skateboardMesh, int skateboardTex, int bmxMesh, int bmxTex, int phoneMesh, int phoneTex, int spraycanMesh, int spraycanTex) {
            this.charMesh = charMesh;
            this.charTex = charTex;
            this.inlineMesh = inlineMesh;
            this.inlineTex = inlineTex;
            this.skateboardMesh = skateboardMesh;
            this.skateboardTex = skateboardTex;
            this.bmxMesh = bmxMesh;
            this.bmxTex = bmxTex;
            this.phoneMesh = phoneMesh;
            this.phoneTex = phoneTex;
            this.spraycanMesh = spraycanMesh;
            this.spraycanTex = spraycanTex;
        }   
    }
}