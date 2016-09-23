using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace PlatformBuilderPro
{
    [ExecuteInEditMode]
    public class Platform : MonoBehaviour
    {
        #region properties
        #region private
        [SerializeField]
        PlatformBuilder _platformBuilder;

        [SerializeField]
        PlatformBuilderCore _platformBuilderCore;

        [SerializeField]
        List<PlatformSection> _platformSections;
        float _currentSeconds;
        #endregion
        #region public
        [SerializeField]
        public PlatformBuilderStrategy[] strategies;

        [SerializeField]
        public PlatformBuilderStrategy activeStrategy;

        [SerializeField]
        public MeshFilter meshFilter;
        [SerializeField]
        public MeshCollider meshCollider;
        [SerializeField]
        public MeshRenderer meshRenderer;
        [SerializeField]
        public Material sharedMaterial;
        #endregion
        #endregion

        public void Awake()
        {
            #if UNITY_EDITOR
            //if this is a prefab, then we need to break the connection so it will update on its own.
            if (_platformSections != null && _platformSections.Count > 0)
            {
                UnityEditor.PrefabUtility.DisconnectPrefabInstance(this);
                meshFilter.sharedMesh = null;
                UpdateConsistant();
            }
            #endif
        }

        //set up platform specific items for this instance. Called once during creation.
        public void Setup()
        {
            //set locals
            _platformBuilder = new PlatformBuilder();
            _platformBuilderCore = new PlatformBuilderCore(gameObject);
            _platformSections = new List<PlatformSection>();
            strategies = PlatformBuilder.GetStrategies();

            //add components
            meshFilter = gameObject.AddComponent<MeshFilter> ();
            meshCollider = gameObject.AddComponent<MeshCollider>();
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        //keep track of the active strategy (note: a strategy is like a plugin and adds additional functionality/operations)
        public void SetStrategy(PlatformBuilderStrategy strategy)
        {
            activeStrategy = strategy;
            _platformBuilder.SetStrategy(strategy, this);
        }

        /*
         * call this method to update a platform 
         * (warning: calling this too frequently can be heavy on performance and should rarely be used. 
         * A platform is updated automatically already)
         */
        public void UpdateConsistant()
        {
            var combine = _platformBuilderCore.UpdatePlatform(_platformBuilder);
            if (combine == null) return;
            PlatformHelper.SetupInstanceMesh(meshFilter, meshRenderer, sharedMaterial, combine);
            meshCollider.sharedMesh = meshFilter.sharedMesh;
            meshFilter.sharedMesh.RecalculateNormals();
            meshFilter.sharedMesh.RecalculateBounds();
            meshFilter.sharedMesh.Optimize();
        }

        //a controlled update to the mesh and other components (collider, renderer, etc)
        public void UpdatePlatform()
        {
            if (Time.realtimeSinceStartup > _currentSeconds)
            {
                UpdateConsistant();
                WaitForSeconds(0.1f);
            }
        }

        //called when a platform is selected and will draw all the sections with a debug line
        public void DrawSections()
        {
            for (var k = 0; k < _platformSections.Count; k++)
            {
                var section = _platformSections[k];
                if (section != null)
                    section.DrawSection();
            }
        }

        //gets all sections in a platform and orders them
        public PlatformSection[] GetSections()
        {
            return GetComponentsInChildren<PlatformSection>().OrderBy(x => x.OrderId).ToArray();
        }

        //gets a multi-dimensional array of ordered platform points throughout the mesh
        public PlatformPoint[][] GetPoints()
        {
            return GetSections().Select(x => x.GetPoints()).ToArray();
        }

        //custom waitForSeconds, because the built in one doesn't work with custom classes :)
        void WaitForSeconds(float seconds)
        {
            _currentSeconds = Time.realtimeSinceStartup + seconds;
        }

        //sets up a new section that's just been added to the platform
        void SetupSection(PlatformSection[] sections, PlatformSection section)
        {
            var platformSections = sections.OrderBy(x => x.OrderId).ToArray();
            section.OrderId = platformSections[platformSections.Length - 1].OrderId + 1;
            section.name = "Section_" + section.OrderId;
            _platformSections.Add(section);
        }

        void Update()
        {
            //check for new sections to assign an id and name
            var platformSections = GetComponentsInChildren<PlatformSection>();
            for (var i = 0; i < platformSections.Length; i++)
            {
                if (!_platformSections.Contains(platformSections[i]))
                {
                    SetupSection(platformSections, platformSections[i]);
                }
            }
        }
    }
}