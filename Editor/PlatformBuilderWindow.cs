using UnityEngine;
using UnityEditor;
using PlatformBuilderPro;

public class PlatformBuilderWindow : EditorWindow
{
    Material material;
    int activeStrategyIndex;

	[MenuItem("Window/Platform Builder")]
	public static void ShowWindow()
	{
		GetWindow<PlatformBuilderWindow> ();
	}

	void OnGUI()
	{
        if (GUILayout.Button("Open Documentation"))
        {
            Application.OpenURL("https://github.com/horsley86/platform-builder-pro/wiki");
        }
        GUILayout.Space(20f);

        material = (Material)EditorGUILayout.ObjectField("Material", material, typeof(Material), false);
        if (GUILayout.Button("Create New"))
        {
            var sceneCameraTransform = SceneView.lastActiveSceneView.camera.transform;
            PlatformHelper.CreateNewPlatform(material, sceneCameraTransform.position + sceneCameraTransform.forward * 10f);
        }

		if (Selection.activeTransform != null)
        {
            var platform = Selection.activeTransform.root.GetComponentInChildren<Platform>();
            
            if (platform != null)
            {
                GUILayout.Space(20f);
                GUILayout.Label("Platform Operations");
                GUILayout.Space(5f);

                if (platform.strategies == null || platform.strategies[0] == null) platform.strategies = PlatformBuilder.GetStrategies();

                //iterate through the platform's strategies and paint the active gui
                for (var i = 0; i < platform.strategies.Length; i++)
                {
                    if (GUILayout.Button(platform.strategies[i].GuiTitle))
                    {
                        //if (activeStrategyIndex != i)
                        //{
                        //    platform.SetStrategy(platform.strategies[i]);
                        //}
                        platform.SetStrategy(platform.strategies[i]);
                        activeStrategyIndex = i;
                    }

                    if (activeStrategyIndex == i)
                    {
                        GUILayout.BeginVertical("box");
                        platform.strategies[i].DrawGui();
                        GUILayout.EndVertical();
                    }
                }
            }
		}
	}

    //when the selection changes trigger a repaint of this window
    void OnSelectionChange()
    {
        Repaint();
    }
}