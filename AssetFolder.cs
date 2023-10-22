using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace DripRemix {
    public class AssetFolder {

        public DirectoryInfo directory;
        public Dictionary<string, Mesh> meshes;
        public List<Texture> textures;
        public List<Texture> emissions;
        public Dictionary<string, string> parameters;
        public List<Texture> sprites1;
        public List<Texture> sprites2;

        public AssetFolder(DirectoryInfo _directory, Dictionary<string, Mesh> _meshes, List<Texture> _textures, List<Texture> _emissions, Dictionary<string, string> _parameters) {
            directory = _directory;
            meshes = _meshes;
            textures = _textures;
            emissions = _emissions;
            parameters = _parameters;
            sprites1 = new List<Texture>();
            sprites2 = new List<Texture>();
        }

        public AssetFolder(DirectoryInfo _directory, Dictionary<string, Mesh> _meshes, List<Texture> _textures, List<Texture> _emissions, Dictionary<string, string> _parameters, List<Texture> _sprites1, List<Texture> _sprites2) {
            directory = _directory;
            meshes = _meshes;
            textures = _textures;
            emissions = _emissions;
            parameters = _parameters;
            sprites1 = _sprites1;
            sprites2 = _sprites2;
        }

        public string description() {
            string name = "MISSING_NAME";
            string author = "MISSING_AUTHOR";

            name = parameters["name"];
            author = parameters["author"];

            return $"\n   • {name} by {author}";
        }
    }
}
