using System.Collections.Generic;
using UnityEngine;

namespace DripRemix {
    public class AssetFolder {
        public string name;
        public string author;
        public Dictionary<string, Mesh> meshes;
        public List<Texture> textures;
        public List<Texture> emissions;

        public AssetFolder(string _name, string _author, Dictionary<string, Mesh> _meshes, List<Texture> _textures, List<Texture> _emissions) {
            name = _name;
            author = _author;
            meshes = _meshes;
            textures = _textures;
            emissions = _emissions;
        }
    }
}
