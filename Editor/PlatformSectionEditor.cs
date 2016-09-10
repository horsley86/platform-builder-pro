using UnityEditor;
using UnityEngine;

namespace PlatformBuilderPro
{
    [CustomEditor(typeof(PlatformSection))]
    [CanEditMultipleObjects]
    public class PlatformSectionEditor : Editor
    {
        private Vector3 _lastPosition;
        void OnSceneGUI()
        {
            var section = (PlatformSection)target;

            if (section.transform.position != _lastPosition)
            {
                section.UpdatePlatform(true);
                _lastPosition = section.transform.position;
            }
            section.DrawSection();
        }
    }
}