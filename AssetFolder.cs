using System.Collections.Generic;
using UnityEngine;

namespace DripRemix {
    public class AssetFolder {
        public string name;
        public string author;
        public Dictionary<string, Mesh> meshes;
        public List<Texture> textures;
        public List<Texture> emissions;
        public List<Texture> sprites1;
        public List<Texture> sprites2;

        public AssetFolder(string _name, string _author, Dictionary<string, Mesh> _meshes, List<Texture> _textures, List<Texture> _emissions) {
            name = _name;
            author = _author;
            meshes = _meshes;
            textures = _textures;
            emissions = _emissions;
            sprites1 = new List<Texture>();
            sprites2 = new List<Texture>();
        }

        public AssetFolder(string _name, string _author, Dictionary<string, Mesh> _meshes, List<Texture> _textures, List<Texture> _emissions, List<Texture> _sprites1, List<Texture> _sprites2) {
            name = _name;
            author = _author;
            meshes = _meshes;
            textures = _textures;
            emissions = _emissions;
            sprites1 = _sprites1;
            sprites2 = _sprites2;
        }
    }
}
