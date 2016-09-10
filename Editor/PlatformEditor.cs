using UnityEditor;

namespace PlatformBuilderPro
{
    [CustomEditor(typeof(Platform))]
    public class PlatformEditor : Editor
    {
        void OnSceneGUI()
        {
            ((Platform)target).DrawSections();
        }
    }
}