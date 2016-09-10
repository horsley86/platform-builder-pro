using System;
using UnityEngine;

namespace PlatformBuilderPro
{
    [Serializable]
    public abstract class PlatformBuilderStrategy : ScriptableObject
    {
        public abstract string GuiTitle { get; }
        public abstract void SetParent(Platform platform);
        public abstract PlatformUpdateInfo UpdatePoints(PlatformUpdateInfo updateInfo);
        public abstract void DrawGui();
        public abstract void DrawGizmo();
    }
}