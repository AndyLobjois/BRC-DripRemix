/*
 * Base code from Dummiesman OBJImporter package (https://assetstore.unity.com/packages/tools/modeling/runtime-obj-importer-49547)
*/

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using OBJImporter;

namespace OBJImporter
{
    public enum SplitMode {
        None,
        Object,
        Material
    }
    
    public class OBJLoader {
        // Determines how objects will be created
        public SplitMode SplitMode = SplitMode.Object;

        //global lists, accessed by objobjectbuilder
        internal List<Vector3> Vertices = new List<Vector3>();
        internal List<Vector3> Normals = new List<Vector3>();
        internal List<Vector2> UVs = new List<Vector2>();

        //file info for files loaded from file path, used for GameObject naming and MTL finding
        private FileInfo _objInfo;

        public Mesh Load(Stream input) {
            var reader = new StreamReader(input);

            Dictionary<string, OBJObjectBuilder> builderDict = new Dictionary<string, OBJObjectBuilder>();
            OBJObjectBuilder currentBuilder = null;
            string currentMaterial = "default";

            //lists for face data
            //prevents excess GC
            List<int> vertexIndices = new List<int>();
            List<int> normalIndices = new List<int>();
            List<int> uvIndices = new List<int>();

            //helper func
            Action<string> setCurrentObjectFunc = (string objectName) =>
            {
                if (!builderDict.TryGetValue(objectName, out currentBuilder))
                {
                    currentBuilder = new OBJObjectBuilder(objectName, this);
                    builderDict[objectName] = currentBuilder;
                }
            };

            //create default object
            setCurrentObjectFunc.Invoke("default");

			//var buffer = new DoubleBuffer(reader, 256 * 1024);
			var buffer = new OBJCharWordReader(reader, 4 * 1024);

			//do the reading
			while (true)
            {
				buffer.SkipWhitespaces();

				if (buffer.endReached == true) {
					break;
				}

				buffer.ReadUntilWhiteSpace();
				
                //comment or blank
                if (buffer.Is("#"))
                {
					buffer.SkipUntilNewLine();
                    continue;
                }
				
				if (buffer.Is("v")) {
					Vertices.Add(buffer.ReadVector());
					continue;
				}

				//normal
				if (buffer.Is("vn")) {
                    Normals.Add(buffer.ReadVector());
                    continue;
                }

                //uv
				if (buffer.Is("vt")) {
                    UVs.Add(buffer.ReadVector());
                    continue;
                }

                //new material
				if (buffer.Is("usemtl")) {
					buffer.SkipWhitespaces();
					buffer.ReadUntilNewLine();
					string materialName = buffer.GetString();
                    currentMaterial = materialName;

                    if(SplitMode == SplitMode.Material)
                    {
                        setCurrentObjectFunc.Invoke(materialName);
                    }
                    continue;
                }

                //new object
                if ((buffer.Is("o") || buffer.Is("g")) && SplitMode == SplitMode.Object) {
                    buffer.ReadUntilNewLine();
                    string objectName = buffer.GetString(1);
                    setCurrentObjectFunc.Invoke(objectName);
                    continue;
                }

                //face data (the fun part)
                if (buffer.Is("f"))
                {
                    //loop through indices
                    while (true)
                    {
						bool newLinePassed;
						buffer.SkipWhitespaces(out newLinePassed);
						if (newLinePassed == true) {
							break;
						}

                        int vertexIndex = int.MinValue;
                        int normalIndex = int.MinValue;
                        int uvIndex = int.MinValue;

						vertexIndex = buffer.ReadInt();
						if (buffer.currentChar == '/') {
							buffer.MoveNext();
							if (buffer.currentChar != '/') {
								uvIndex = buffer.ReadInt();
							}
							if (buffer.currentChar == '/') {
								buffer.MoveNext();
								normalIndex = buffer.ReadInt();
							}
						}

                        //"postprocess" indices
                        if (vertexIndex > int.MinValue)
                        {
                            if (vertexIndex < 0)
                                vertexIndex = Vertices.Count - vertexIndex;
                            vertexIndex--;
                        }
                        if (normalIndex > int.MinValue)
                        {
                            if (normalIndex < 0)
                                normalIndex = Normals.Count - normalIndex;
                            normalIndex--;
                        }
                        if (uvIndex > int.MinValue)
                        {
                            if (uvIndex < 0)
                                uvIndex = UVs.Count - uvIndex;
                            uvIndex--;
                        }

                        //set array values
                        vertexIndices.Add(vertexIndex);
                        normalIndices.Add(normalIndex);
                        uvIndices.Add(uvIndex);
                    }

                    //push to builder
                    currentBuilder.PushFace(currentMaterial, vertexIndices, normalIndices, uvIndices);

                    //clear lists
                    vertexIndices.Clear();
                    normalIndices.Clear();
                    uvIndices.Clear();

					continue;
                }

				buffer.SkipUntilNewLine();
            }

            Mesh msh = new Mesh();

            foreach (var builder in builderDict)
            {
                //empty object
                if (builder.Value.PushedFaceCount == 0)
                    continue;

                msh = builder.Value.Build();
            }

            return msh;
        }

        // Load an OBJ and MTL file from a file path.
        // Returns a GameObject represeting the OBJ file, with each imported object as a child.
        public Mesh Load(string path) {
            string mtlPath = null;
            _objInfo = new FileInfo(path);
            if (!string.IsNullOrEmpty(mtlPath) && File.Exists(mtlPath))
            {
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    return Load(fs);
                }
            }
            else
            {
                using (var fs = new FileStream(path, FileMode.Open))
                {
                    return Load(fs);
                }
            }
        }
    }
}