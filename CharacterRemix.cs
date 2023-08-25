using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using Reptile;
using BRCML;
using BRCML.Utils;

// Character Swap WIP, not used yet
// Code from Glomzubuk

namespace MeshRemix {
    class CharacterRemix {
        internal static DirectoryInfo ModdingFolder = Directory.CreateDirectory(Path.Combine(BepInEx.Paths.GameRootPath, "ModdingFolder"));
        internal static DirectoryInfo CHARFOLDER => ModdingFolder.CreateSubdirectory("BRC-MeshRemix/Characters");
        public Dictionary<Characters, List<AssetBundle>> BUNDLES_CHARACTER = new Dictionary<Characters, List<AssetBundle>>();
        public Dictionary<Characters, GameObject> REFS_CHARACTER;

        public Dictionary<Characters, string> characterNamesMap = new Dictionary<Characters, string>() {
            // Added by Characters.list order
            [Characters.girl1] = "Vinyl",
            [Characters.frank] = "Frank",
            [Characters.ringdude] = "Coil",
            [Characters.metalHead] = "Red",
            [Characters.blockGuy] = "Tryce",
            [Characters.spaceGirl] = "Bel",
            [Characters.angel] = "Rave",
            [Characters.eightBall] = "DOT EXE",
            [Characters.dummy] = "Solace",
            [Characters.dj] = "DJ Cyber",
            [Characters.medusa] = "Eclipse",
            [Characters.boarder] = "DevilTheory",
            [Characters.headMan] = "Faux", // Necessary ?
            [Characters.prince] = "Flesh Prince",
            [Characters.jetpackBossPlayer] = "Irene Ritvield",
            [Characters.legendFace] = "Felix",
            [Characters.oldheadPlayer] = "Oldhead",
            [Characters.robot] = "Base",
            [Characters.skate] = "Jay",
            [Characters.wideKid] = "Mesh",
            [Characters.futureGirl] = "Futurism",
            [Characters.pufferGirl] = "Rise",
            [Characters.bunGirl] = "Shine",
            [Characters.headManNoJetpack] = "Faux (Prelude)", // Necessary ?
            [Characters.eightBallBoss] = "DOT EXE (Boss)", // Necessary ?
            [Characters.legendMetalHead] = "Red Felix (Dream)", // Necessary ?
        };

        public string CharacterToString(Characters character) {
            if (characterNamesMap.ContainsKey(character)) {
                return characterNamesMap[character];
            }
            return character.ToString();
        }
    }
}
