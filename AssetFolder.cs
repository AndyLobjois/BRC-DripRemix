using System.Collections.Generic;
using UnityEngine;

namespace DripRemix {
    public class AssetFolder {
        public Dictionary<string, Mesh> meshes;
        public List<Texture> textures;
        public List<Texture> emissions;
        public Dictionary<string, string> parameters;
        public List<Texture> sprites1;
        public List<Texture> sprites2;

        public AssetFolder(Dictionary<string, Mesh> _meshes, List<Texture> _textures, List<Texture> _emissions, Dictionary<string, string> _parameters) {
            meshes = _meshes;
            textures = _textures;
            emissions = _emissions;
            parameters = _parameters;
            sprites1 = new List<Texture>();
            sprites2 = new List<Texture>();
        }

        public AssetFolder(Dictionary<string, Mesh> _meshes, List<Texture> _textures, List<Texture> _emissions, Dictionary<string, string> _parameters, List<Texture> _sprites1, List<Texture> _sprites2) {
            meshes = _meshes;
            textures = _textures;
            emissions = _emissions;
            parameters = _parameters;
            sprites1 = _sprites1;
            sprites2 = _sprites2;
        }
    }
}
