using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlatformBuilderPro
{
    [Serializable]
    public class ExportMeshStrategy : PlatformBuilderStrategy
    {
        private static int vertexOffset = 0;
        private static int normalOffset = 0;
        private static int uvOffset = 0;
        private Platform _platform;

        public override string GuiTitle
        {
            get
            {
                return "Export Mesh";
            }
        }

        public override void DrawGizmo(){}

        public override void DrawGui()
        {
            if (GUILayout.Button("Export as .OBJ"))
            {
                ExportWholeSelectionToSingle(_platform.name);
            }
            GUILayout.Label("Note: Will save .obj file to Assets/meshes");
        }

        public override void SetParent(Platform platform)
        {
            _platform = platform;
        }

        public override PlatformUpdateInfo UpdatePoints(PlatformUpdateInfo updateInfo)
        {
            return updateInfo;
        }

        //export mesh methods
        //export mesh to OBJ file
        private static string targetFolder = "Assets/meshes";

        private static string MeshToString(MeshFilter mf, Dictionary<string, ObjMaterial> materialList)
        {
            Mesh m = mf.sharedMesh;
            Material[] mats = mf.GetComponent<Renderer>().sharedMaterials;

            StringBuilder sb = new StringBuilder();

            sb.Append("g ").Append(mf.name).Append("\n");
            foreach (Vector3 lv in m.vertices)
            {
                Vector3 wv = mf.transform.TransformPoint(lv);

                //This is sort of ugly - inverting x-component since we're in
                //a different coordinate system than "everyone" is "used to".
                sb.Append(string.Format("v {0} {1} {2}\n", -wv.x, wv.y, wv.z));
            }
            sb.Append("\n");

            foreach (Vector3 lv in m.normals)
            {
                Vector3 wv = mf.transform.TransformDirection(lv);

                sb.Append(string.Format("vn {0} {1} {2}\n", -wv.x, wv.y, wv.z));
            }
            sb.Append("\n");

            foreach (Vector3 v in m.uv)
            {
                sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
            }

            for (int material = 0; material < m.subMeshCount; material++)
            {
                sb.Append("\n");
                sb.Append("usemtl ").Append(mats[material].name).Append("\n");
                sb.Append("usemap ").Append(mats[material].name).Append("\n");

                //See if this material is already in the materiallist.
                try
                {
                    ObjMaterial objMaterial = new ObjMaterial();

                    objMaterial.name = mats[material].name;

#if UNITY_EDITOR
                    if (mats[material].mainTexture)
                        objMaterial.textureName = UnityEditor.AssetDatabase.GetAssetPath(mats[material].mainTexture);
                    else
                        objMaterial.textureName = null;
#endif
                    materialList.Add(objMaterial.name, objMaterial);
                }
                catch (ArgumentException)
                {
                    //Already in the dictionary
                }


                int[] triangles = m.GetTriangles(material);
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    //Because we inverted the x-component, we also needed to alter the triangle winding.
                    sb.Append(string.Format("f {1}/{1}/{1} {0}/{0}/{0} {2}/{2}/{2}\n",
                        triangles[i] + 1 + vertexOffset, triangles[i + 1] + 1 + normalOffset, triangles[i + 2] + 1 + uvOffset));
                }
            }

            vertexOffset += m.vertices.Length;
            normalOffset += m.normals.Length;
            uvOffset += m.uv.Length;

            return sb.ToString();
        }

        private static void Clear()
        {
            vertexOffset = 0;
            normalOffset = 0;
            uvOffset = 0;
        }

        private static Dictionary<string, ObjMaterial> PrepareFileWrite()
        {
            Clear();

            return new Dictionary<string, ObjMaterial>();
        }

        private static void MeshesToFile(MeshFilter[] mf, string folder, string filename)
        {
            Dictionary<string, ObjMaterial> materialList = PrepareFileWrite();

            using (StreamWriter sw = new StreamWriter(folder + "/" + filename + ".obj"))
            {
                sw.Write("mtllib ./" + filename + ".mtl\n");

                for (int i = 0; i < mf.Length; i++)
                {
                    sw.Write(MeshToString(mf[i], materialList));
                }
            }
        }

        private static bool CreateTargetFolder()
        {
            try
            {
                System.IO.Directory.CreateDirectory(targetFolder);
            }
            catch
            {
#if UNITY_EDITOR
                UnityEditor.EditorUtility.DisplayDialog("Error!", "Failed to create target folder!", "");
#endif
                return false;
            }

            return true;
        }

        public static void ExportWholeSelectionToSingle(string filename)
        {
            if (!CreateTargetFolder())
                return;
#if UNITY_EDITOR
            Transform[] selection = UnityEditor.Selection.GetTransforms(UnityEditor.SelectionMode.Editable | UnityEditor.SelectionMode.ExcludePrefab);
            selection = selection.Select(x => x.root).ToArray();

            if (selection.Length == 0)
            {
                UnityEditor.EditorUtility.DisplayDialog("No source object selected!", "Please select one or more target objects", "");
                return;
            }

            int exportedObjects = 0;

            ArrayList mfList = new ArrayList();

            for (int i = 0; i < selection.Length; i++)
            {
                Component[] meshfilter = selection[i].GetComponentsInChildren(typeof(MeshFilter));

                for (int m = 0; m < meshfilter.Length; m++)
                {
                    exportedObjects++;
                    mfList.Add(meshfilter[m]);
                }
            }

            if (exportedObjects > 0)
            {
                MeshFilter[] mf = new MeshFilter[mfList.Count];

                for (int i = 0; i < mfList.Count; i++)
                {
                    mf[i] = (MeshFilter)mfList[i];
                }

                MeshesToFile(mf, targetFolder, filename);

                UnityEditor.AssetDatabase.Refresh();

            }
            else
                UnityEditor.EditorUtility.DisplayDialog("Objects not exported", "Make sure at least some of your selected objects have mesh filters!", "");
#endif
        }
    }
}