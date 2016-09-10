using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PlatformBuilderPro
{
    [CustomEditor(typeof(PlatformPoint))]
    [CanEditMultipleObjects]
    public class PlatformPointEditor : Editor
    {
        private Vector3 _lastPosition;
        void OnSceneGUI()
        {
            var currentPoint = (PlatformPoint)target;
            var currentSection = currentPoint.transform.parent.GetComponent<PlatformSection>();

            if (currentPoint.transform.position != _lastPosition)
            {
                //check for new points to assign an id and name
                var _platformPoints = currentSection.GetComponentsInChildren<PlatformPoint>();
                for (var i = 0; i < _platformPoints.Length; i++)
                {
                    if (!currentSection.platformPoints.Contains(_platformPoints[i]))
                    {
                        SetupPoint(currentSection, _platformPoints, _platformPoints[i]);
                    }
                }

                currentPoint.UpdateChildren();
                currentSection.UpdateChildren();

                currentPoint.UpdatePlatform(true);
                _lastPosition = currentPoint.transform.position;
            }
            currentSection.DrawSection();
        }

        void SetupPoint(PlatformSection currentSection, PlatformPoint[] points, PlatformPoint point)
        {
            var _platformPoints = points.OrderBy(x => x.OrderId).ToArray();
            var orderId = point.OrderId;

            if (_platformPoints[_platformPoints.Length - 1].OrderId == point.OrderId)
            {
                point.OrderId = _platformPoints[_platformPoints.Length - 1].OrderId + 1;
            }
            else
            {
                point.OrderId++;
                var pointsToUpdate = currentSection.platformPoints.Where(x => x.OrderId >= point.OrderId).OrderBy(x => x.OrderId).ToArray();
                for (var i = 0; i < pointsToUpdate.Length; i++)
                {
                    pointsToUpdate[i].OrderId++;
                    pointsToUpdate[i].name = "Point_" + pointsToUpdate[i].OrderId;
                }
            }

            point.name = "Point_" + point.OrderId;
            currentSection.platformPoints.Add(point);

            var all = currentSection.transform.parent.GetComponentsInChildren<PlatformSection>();
            var sections = all.Where(x => x != currentSection).ToArray();

            foreach (var section in sections)
            {
                if (section.platformPoints.Count < currentSection.platformPoints.Count)
                {
                    section.AddPointAtOrderId(orderId, point.transform.localPosition);
                }
            }
        }
    }
}