using System;
using System.Linq;
using UnityEngine;
using BepInEx.Configuration;
using Reptile;

namespace DripRemix.Handlers
{
    [Serializable]
    public struct HandlersConfig
    {

        public int charCode;
        public TexMeshIndexes[] indexes;

        public Characters Character => (Characters)charCode;

        public HandlersConfig(Characters character)
        {
            this.charCode = (int)character;
            this.indexes = new TexMeshIndexes[Enum.GetValues(typeof(HandlerTypes)).Length];
            for (int i = 0; i < indexes.Length; i++) {
                indexes[i] = new TexMeshIndexes { Mesh = 0, Texture = 0 };
            };
        }
        public static readonly TypeConverter typeConverter = new TypeConverter
        {
            ConvertToString = (obj, type) => obj.ToString(),
            ConvertToObject = (str, type) => HandlersConfig.FromString(str),
        };
        override public string ToString()
        {
            string res = $"{charCode}";
            for (int i = 0; i < indexes.Length; i++)
            {
                res += $"|{indexes[i].Mesh}:{indexes[i].Texture}";
            };
            return res;
        }
        public static HandlersConfig FromString(string str)
        {
            HandlersConfig newConf = new HandlersConfig();
            var values = str.Split('|');
            newConf.charCode = int.Parse(values[0]);
            newConf.indexes = new TexMeshIndexes[values.Length - 1];
            for(int i = 1; i < values.Length; i++)
            {
                var texMeshIndexes = values[i].Split(':');
                newConf.indexes[i - 1] = new TexMeshIndexes { Mesh = int.Parse(texMeshIndexes[0]), Texture = int.Parse(texMeshIndexes[1]) };
            }
            return newConf;
        }
        public static void AddConverter()
        {
            TomlTypeConverter.AddConverter(typeof(HandlersConfig), typeConverter);
        }
    }
    [Serializable]
    public struct TexMeshIndexes
    {
        public int Mesh;
        public int Texture;
    }

    public enum HandlerTypes
    {
        Character,
        Gear,
        Phone,
        SprayCan,
        Grafiti
    }
}
