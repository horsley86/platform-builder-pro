using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace PlatformBuilderPro
{
    [ExecuteInEditMode]
    public class PlatformPoint : MonoBehaviour
    {
        #region properties
        private Vector3 _lastPosition;

        [SerializeField]
        private List<PointChild> _children;

        
        //public struct Child
        //{
        //    public Vector3 point;
        //    public Vector3 offset;
        //}

        [HideInInspector]
        public int OrderId;

        [SerializeField]
        public List<PointChild> Children
        {
            get
            {
                if (_children == null) _children = new List<PointChild>();
                return _children;
            }
        }
        #endregion

        void Start()
        {
        #if UNITY_EDITOR
            
        #endif
        }

        //add a child point to this point
        public void AddChild(Vector3 position)
        {
            var child = new GameObject("child_" + Children.Count, new Type[] { typeof(PointChild) }).GetComponent<PointChild>();
            child.transform.parent = transform;
            child.transform.position = position;
            child.transform.localRotation = Quaternion.identity;
            child.Index = Children.Count;
            Children.Add(child);
        }

        //move a child (given an index in the Children list)
        public void MoveChild(int index, Vector3 position)
        {
            if ((Children.Count - 1) < index) return;
            Children[index].transform.position = position;
        }

        //since the children are in world coordinates, make sure we keep their positions updated relative to the parent point
        //public void UpdateChildren()
        //{
        //    for (var i = 0; i < Children.Count; i++)
        //    {
        //        Children[i] = new Child { point = transform.position + Children[i].offset, offset = Children[i].offset };
        //    }
        //}

        //update the platform every tenth of a second
        public void UpdatePlatform()
        {
            UpdatePlatform(false);
        }

        //update the platform, choosing whether or not to update at a regulated interval
        public void UpdatePlatform(bool updateConsistant)
        {
            if (updateConsistant) transform.root.GetComponentInChildren<Platform>().UpdateConsistant();
            else transform.root.GetComponentInChildren<Platform>().UpdatePlatform();
        }

        //gets the vertices for the point and its children
        public Vector3[] GetPointVects(Vector3 offset)
        {
            var vectList = new List<Vector3>() { transform.position + offset };
            vectList.AddRange(Children.Select(x => x.transform.position + offset));
            return vectList.ToArray();
        }

        //returns whether or not the point has moved (note: can be used to know when to allow the core to update)
        public bool HasMoved()
        {
            var hasMoved = false;
            if (transform.position != _lastPosition)
            {
                hasMoved = true;
                _lastPosition = transform.position;
            }
            return hasMoved;
        }
    }

    [ExecuteInEditMode]
    public class PointChild : MonoBehaviour
    {
        public int Index { get; set; }
    }
}