using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace PlatformBuilderPro
{
    [ExecuteInEditMode]
    public class PlatformSection : MonoBehaviour
    {
        #region properties
        [Serializable]
        public struct Child
        {
            public Vector3 position;
            public Vector3 offset;
            public Vector3[] positions;
        }

        public int OrderId;

        [HideInInspector]
        public List<PlatformPoint> platformPoints;

        [SerializeField]
        public List<Child> Children
        {
            get
            {
                if (_children == null) _children = new List<Child>();
                return _children;
            }
        }

        private Transform[] points;
        private Vector3 _lastPosition;

        [SerializeField]
        private Platform _platform;

        [SerializeField]
        private List<Child> _children;
        #endregion

        //set up properties when created
        void Start()
        {
            #if UNITY_EDITOR
                platformPoints = new List<PlatformPoint>(GetComponentsInChildren<PlatformPoint>());
                _platform = transform.parent.GetComponent<Platform>();
                UpdatePlatform();
            #endif
        }

        //set up a newly added point with this section
        void SetupPoint(PlatformPoint[] points, PlatformPoint point)
        {
            var _platformPoints = points.OrderBy(x => x.OrderId).ToArray();

            //if the point was duplicated at the end of the sections point list, then just add one to its orderId
            if (_platformPoints[_platformPoints.Length - 1].OrderId == point.OrderId)
            {
                point.OrderId = _platformPoints[_platformPoints.Length - 1].OrderId + 1;
            }
            //otherwise, we need to add one to its orderId and update all points after it (note: this is when a point between two points is added)
            else
            {
                point.OrderId++;
                var pointsToUpdate = platformPoints.Where(x => x.OrderId >= point.OrderId).OrderBy(x => x.OrderId).ToArray();
                for (var i = 0; i < pointsToUpdate.Length; i++)
                {
                    pointsToUpdate[i].OrderId++;
                    pointsToUpdate[i].name = "Point_" + pointsToUpdate[i].OrderId;
                }
            }
            
            point.name = "Point_" + point.OrderId;
            platformPoints.Add(point);
        }

        //TODO - improve performance here
        //gets all points in this section including all of the points children
        //Vector3[] GetChildPointsForSection()
        //{
        //    var allPoints = new List<Vector3>();
        //    var points = GetPoints();

        //    for (var i = 0; i < points.Length; i++)
        //    {
        //        allPoints.Add(points[i].transform.position);
        //        if (points[i].Children != null && points[i].Children.Count() > 0) allPoints.AddRange(points[i].Children.Select(x => x.point));
        //    }
        //    return allPoints.ToArray();
        //}

        //draws this section with a debug line
        public void DrawSection()
        {
            //var points = GetChildPointsForSection();
            //for (var i = 0; i < points.Length; i++)
            //{
            //    var nextIndex = 0;

            //    if (i < points.Length - 1)
            //        nextIndex = i + 1;
            //    Debug.DrawLine(points[i], points[nextIndex]);
            //}
        }

        //creates a new point at a specific point orderId and local position
        public void AddPointAtOrderId (int orderId, Vector3 position)
        {
            var point = platformPoints.Find(x => x.OrderId == orderId);
            var spawn = Instantiate(point);
            spawn.transform.parent = point.transform.parent;
            spawn.transform.localPosition = position;

            //check for new points to assign an id and name
            var _platformPoints = GetComponentsInChildren<PlatformPoint>();
            for (var i = 0; i < _platformPoints.Length; i++)
            {
                if (!platformPoints.Contains(_platformPoints[i]))
                {
                    SetupPoint(_platformPoints, _platformPoints[i]);
                }
            }
        }

        //update the platform every tenth of a second
        public void UpdatePlatform()
        {
            UpdatePlatform(false);
        }

        //update the platform, choosing whether or not to update at a regulated interval
        public void UpdatePlatform(bool updateConsistant)
        {
            var sections = _platform.GetSections();

            for (var i = 0; i < sections.Length; i++)
            {
                sections[i].UpdateChildren();
                var points = sections[i].GetPoints();
                for (var k = 0; k < points.Length; k++)
                {
                    points[k].UpdateChildren();
                }
            }

            if (updateConsistant) transform.parent.GetComponent<Platform>().UpdateConsistant();
            else transform.parent.GetComponent<Platform>().UpdatePlatform();
        }

        //get all points, not including child points
        public PlatformPoint[] GetPoints()
        {
            return gameObject.GetComponentsInChildren<PlatformPoint>().OrderBy(x => x.OrderId).ToArray();
        }

        //call the active strategy DrawGizmo method for any gizmos that should be drawn by it
        void OnDrawGizmos()
        {
            if (_platform == null) _platform = transform.parent.GetComponent<Platform>();
            if (_platform != null && _platform.activeStrategy != null) _platform.activeStrategy.DrawGizmo();
        }

        //get all point positions, parent and children included
        Vector3[] GetChildPointPositions(Vector3 position)
        {
            var points = GetPoints();
            var positionsList = new List<Vector3>();
            var offset = position - transform.position;

            foreach (var point in points)
            {
                if (point.Children.Count > 0)
                {
                    positionsList.AddRange(point.GetPointVects(offset));
                }
                else
                {
                    positionsList.Add(point.transform.position + offset);
                }
            }
            return positionsList.ToArray();
        }

        //add a new child to this section
        public void AddChild(Vector3 position)
        {
            var positions = GetChildPointPositions(position);
            Children.Add(new Child { position = position, positions = positions, offset = position - transform.position });
        }

        //move a specific child in this section
        public void MoveChild(int index, Vector3 position)
        {
            if ((Children.Count - 1) < index) return;

            var positions = GetChildPointPositions(position);
            Children[index] = new Child { position = position, positions = positions, offset = position - transform.position };
        }

        //update the positions of all children in this section
        public void UpdateChildren()
        {
            for (var i = 0; i < Children.Count; i++)
            {
                var positions = GetChildPointPositions(Children[i].position);
                Children[i] = new Child { position = transform.position + Children[i].offset, positions = positions, offset = Children[i].offset };
            }
        }

        //returns whether or not the section has moved (note: can be used to know when to allow the core to update)
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
}