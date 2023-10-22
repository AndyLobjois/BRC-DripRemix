using System;
using System.IO;
using System.Collections.Generic;
using Reptile;

namespace DripRemix {
    public class Save {

        public Main main;
        public Dictionary<string, SaveLine> SaveLines = new Dictionary<string, SaveLine>();
        
        public void GetSave() {
            // Clean
            SaveLines.Clear();

            // Check Save File if Exist/Corrupted
            try {
                // Get Save
                FileInfo file = new FileInfo(BepInEx.Paths.PluginPath + "/BRC-DripRemix/save");
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

                // New Save Slot
                if (!SaveLines.ContainsKey(Main.CURRENTCHARACTER)) {
                    // Add Line
                    SaveLines.Add(Main.CURRENTCHARACTER, new SaveLine(0,0,0,0,0,0,0,0,0,0,0,0));

                    // Clean
                    main.CHARACTER.INDEX_MESH = 0;
                    main.CHARACTER.INDEX_TEXTURE = 0;
                    main.GEARS[MoveStyle.INLINE].INDEX_MESH = 0;
                    main.GEARS[MoveStyle.INLINE].INDEX_TEXTURE = 0;
                    main.GEARS[MoveStyle.SKATEBOARD].INDEX_MESH = 0;
                    main.GEARS[MoveStyle.SKATEBOARD].INDEX_TEXTURE = 0;
                    main.GEARS[MoveStyle.BMX].INDEX_MESH = 0;
                    main.GEARS[MoveStyle.BMX].INDEX_TEXTURE = 0;
                    main.PHONES.INDEX_MESH = 0;
                    main.PHONES.INDEX_TEXTURE = 0;
                    main.SPRAYCANS.INDEX_MESH = 0;
                    main.SPRAYCANS.INDEX_TEXTURE = 0;

                    // Force Save
                    SetSave();
                }
            } catch {
                Main.Log.LogError("Missing or Corrupted Save File.\nIf you want a New Save File, delete the Save File from BRC-DripRemix plugin folder and restart the game.");

                if (!File.Exists(BepInEx.Paths.PluginPath + "/BRC-DripRemix/save")) {
                    File.Create(BepInEx.Paths.PluginPath + "/BRC-DripRemix/save");
                }
            }
        }

        public void SetSave() {
            // Update the Current Character Line
            SaveLines[Main.CURRENTCHARACTER] = new SaveLine(
                main.CHARACTER.INDEX_MESH,
                main.CHARACTER.INDEX_TEXTURE,
                main.GEARS[MoveStyle.INLINE].INDEX_MESH,
                main.GEARS[MoveStyle.INLINE].INDEX_TEXTURE,
                main.GEARS[MoveStyle.SKATEBOARD].INDEX_MESH,
                main.GEARS[MoveStyle.SKATEBOARD].INDEX_TEXTURE,
                main.GEARS[MoveStyle.BMX].INDEX_MESH,
                main.GEARS[MoveStyle.BMX].INDEX_TEXTURE,
                main.PHONES.INDEX_MESH,
                main.PHONES.INDEX_TEXTURE,
                main.SPRAYCANS.INDEX_MESH,
                main.SPRAYCANS.INDEX_TEXTURE
                );

            // Write Save
            string text = "";
            foreach (var item in SaveLines) {
                text += $"{item.Key}," +
                    $"{item.Value.charMesh}," +
                    $"{item.Value.charTex}," +
                    $"{item.Value.inlineMesh}," +
                    $"{item.Value.inlineTex}," +
                    $"{item.Value.skateboardMesh}," +
                    $"{item.Value.skateboardTex}," +
                    $"{item.Value.bmxMesh}," +
                    $"{item.Value.bmxTex}," +
                    $"{item.Value.phoneMesh}," +
                    $"{item.Value.phoneTex}," +
                    $"{item.Value.spraycanMesh}," +
                    $"{item.Value.spraycanTex}" +
                    "\n";
            }
            File.WriteAllText(BepInEx.Paths.PluginPath + "/BRC-DripRemix/save", text);
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