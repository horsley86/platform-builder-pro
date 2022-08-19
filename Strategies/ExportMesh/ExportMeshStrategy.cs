using System;
using System.Linq;
using UnityEngine;
using UnityEditor.Formats.Fbx.Exporter;

namespace PlatformBuilderPro
{
    [Serializable]
    public class ExportMeshStrategy : PlatformBuilderStrategy
    {
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
            if (GUILayout.Button("Export as FBX"))
            {
                ExportWholeSelectionToSingle(_platform.name);
            }
            GUILayout.Label("Note: Will save .fbx file to Assets/meshes");
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
        //export mesh to FBX file
        private static string targetFolder = "Assets/meshes";

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

        public void ExportWholeSelectionToSingle(string filename)
        {
            if (!CreateTargetFolder())
                return;
#if UNITY_EDITOR
            Transform selection = UnityEditor.Selection.GetTransforms(UnityEditor.SelectionMode.Editable | UnityEditor.SelectionMode.ExcludePrefab).Select(x => x.root).FirstOrDefault();
            

            if (selection == null)
            {
                UnityEditor.EditorUtility.DisplayDialog("No source object selected!", "Please select one or more target objects", "");
                return;
            }

            //make a temporary clone of the platform
            var platformClone = Instantiate(_platform.gameObject);

            //cleanup all the child gameObjects
            var tempList = platformClone.transform.Cast<Transform>().ToList();
            foreach (var child in tempList)
            {
                DestroyImmediate(child.gameObject);
            }

            //export gameObject using FBX exporter API
            ModelExporter.ExportObject(targetFolder + "/" + filename + ".fbx", platformClone);

            //cleanup the temporary clone of the platform
            DestroyImmediate(platformClone);

            //refresh the asset database so the mesh folder and .fbx show up in the editor
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
    }
}