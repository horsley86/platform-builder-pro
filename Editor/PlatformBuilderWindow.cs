using UnityEngine;
using UnityEditor;
using PlatformBuilderPro;

public class PlatformBuilderWindow : EditorWindow
{
    Material material;

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
            if (material == null) material = new Material(Shader.Find("Diffuse"));
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

                if (platform.strategies == null || platform.strategies[0] == null || platform.strategies.Length != PlatformBuilder.GetStrategies().Length) platform.strategies = PlatformBuilder.GetStrategies();

                var strategyMetaInfoArray = PlatformBuilder.GetAllStrategyMetaInfo();

                //iterate through the platform's strategies and paint the active gui
                for (var i = 0; i < strategyMetaInfoArray.Length; i++)
                {
                    var strategy = PlatformBuilder.GetStrategyFromMetaName(platform.strategies, strategyMetaInfoArray[i].Name);

                    if (strategy == null)
                    {
                        if (GUILayout.Button(strategyMetaInfoArray[i].Name))
                        {
                            //platform.SetStrategy(strategy);
                            platform.activeStrategyIndex = i;
                        }

                        if (platform.activeStrategyIndex == i)
                        {
                            var bytes = System.IO.File.ReadAllBytes(strategyMetaInfoArray[i].ImgDir);
                            Texture2D tex = new Texture2D(2, 2);
                            tex.LoadImage(bytes);

                            GUILayout.BeginHorizontal("box");
                            GUILayout.Box(tex, GUILayout.Width(150), GUILayout.Height(100));
                            var style = new GUIStyle();
                            style.fontSize = 18;
                            style.normal.textColor = Color.white;
                            style.wordWrap = true;
                            style.padding = new RectOffset(5, 0, 5, 10);

                            var descStyle = new GUIStyle();
                            descStyle.wordWrap = true;
                            descStyle.normal.textColor = Color.white;
                            descStyle.wordWrap = true;
                            descStyle.padding = new RectOffset(5, 0, 0, 5);

                            GUILayout.BeginVertical();

                            GUILayout.Label(strategyMetaInfoArray[i].Name, style);

                            GUILayout.Label(strategyMetaInfoArray[i].Description, descStyle);

                            if (GUILayout.Button("More info here!"))
                            {
                                Application.OpenURL(strategyMetaInfoArray[i].StoreUrl);
                            }

                            GUILayout.EndVertical();

                            GUILayout.EndHorizontal();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(strategy.GuiTitle))
                        {
                            platform.SetStrategy(strategy);
                            platform.activeStrategyIndex = i;
                        }

                        if (platform.activeStrategyIndex == i)
                        {
                            GUILayout.BeginVertical("box");
                            strategy.DrawGui();
                            GUILayout.EndVertical();
                        }
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