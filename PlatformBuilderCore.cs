using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatformBuilderPro
{
    [Serializable]
    public class PlatformBuilderCore
    {
        #region properties
        [SerializeField]
        GameObject _gameObject;

        [SerializeField]
        Vert[][] vertMatrix;

        [Serializable]
        struct Vert
        {
            public Vector3 Vector;
            public Vector3[] Children;

            public Vert(Vector3 vector, Vector3[] children)
            {
                Vector = vector;
                Children = children;
            }
        }
        #endregion

        public PlatformBuilderCore(GameObject gameObject)
        {
            _gameObject = gameObject;
        }

        /*
         * core method to update the mesh
         * passes in the platformBuilder instance to run the active strategy
         */
        public CombineInstance[] UpdatePlatform(PlatformBuilder platformBuilder)
        {
            var platform = _gameObject.GetComponent<Platform>();
            var points = platform.GetPoints();
            var sections = platform.GetSections();

            //run current strategy before building mesh
            var updateInfo = platformBuilder.Update(points);

            //check if we should run an update
            if (!updateInfo.shouldUpdate) return null;

            points = updateInfo.points;

            //get verts from points
            vertMatrix = points.Select(x => x.Select(z => new Vert { Vector = z.transform.position, Children = z.Children.Select(y => y.point).ToArray() }).ToArray()).ToArray();

            //check for any verts with child points and set them up as their own verts
            for (var i = 0; i < vertMatrix.Length; i++)
            {
                var vertList = new List<Vert>();
                var section = vertMatrix[i];
                for (var k = 0; k < section.Length; k++)
                {
                    var point = section[k];
                    vertList.Add(point);
                    if (point.Children != null && point.Children.Length > 0)
                    {
                        vertList.AddRange(point.Children.Select(x => new Vert { Vector = x }));
                    }
                }
                vertMatrix[i] = vertList.ToArray();
            }

            //set up children from platformSections
            var sectionsWithChildren = sections.Where(x => x.Children.Count > 0).ToArray();
            if (sectionsWithChildren.Length > 0)
            {
                for (var i = 0; i < sectionsWithChildren.Length; i++)
                {
                    var section = sectionsWithChildren[i];
                    section.UpdateChildren();
                    var previousSectionsChildCount = sectionsWithChildren.Take(i).Select(x => x.Children.Count);
                    var previousSectionsChildCountIndex = 0;
                    if (previousSectionsChildCount.Count() > 0)
                    {
                        previousSectionsChildCountIndex = previousSectionsChildCount.Aggregate((previous, next) => previous + next);
                    }
                    var sectionIndex = Array.IndexOf(sections.ToArray(), section) + previousSectionsChildCountIndex;
                    
                    for (var k = 0; k < section.Children.Count; k++)
                    {
                        var child = section.Children[k];
                        var vertMatrixList = vertMatrix.ToList();
                        vertMatrixList.Insert(sectionIndex + k + 1, child.positions.Select(x => new Vert { Vector = x }).ToArray());
                        vertMatrix = vertMatrixList.ToArray();
                    }
                }
            }

            //generate individual sub meshes, one rectangle at a time, and add them to the meshList
            var meshList = new List<Mesh>();
            var firstSectionLengthDistance = 0f;
            var firstSectionWidthDistance = 0f;
            for (var i = 0; i < vertMatrix.Length - 1; i++)
            {
                var currentSection = vertMatrix[i];
                var nextSection = vertMatrix[i + 1];

                for (var k = 0; k < currentSection.Length; k++)
                {
                    var nextIndex = 0;

                    if (k < currentSection.Length - 1)
                        nextIndex = k + 1;

                    var verts = new Vector3[4];
                    verts[3] = currentSection[k].Vector;
                    verts[2] = currentSection[nextIndex].Vector;
                    verts[1] = nextSection[nextIndex].Vector;
                    verts[0] = nextSection[k].Vector;

                    //track the first section distance between first points
                    if (i == 0)
                    {
                        firstSectionLengthDistance = Mathf.Abs(Vector3.Distance(currentSection[k].Vector, nextSection[k].Vector));
                    }
                    if (k == 0)
                    {
                        firstSectionWidthDistance = Mathf.Abs(Vector3.Distance(currentSection[k].Vector, currentSection[nextIndex].Vector));
                    }

                    var previousLengthUvs = new Vector2[] { new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0f) };
                    var previousWidthUvs = new Vector2[] { new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0f) };

                    if (i > 0)
                    {
                        previousLengthUvs = meshList[meshList.Count - currentSection.Length].uv;
                    }
                    if (k > 0)
                    {
                        previousWidthUvs = meshList[meshList.Count - 1].uv;
                    }
                    meshList.Add(GenerateMeshFromPoints(verts, firstSectionLengthDistance, firstSectionWidthDistance, previousLengthUvs, previousWidthUvs));
                }
            }

            //combine mesh sides through entire platform
            var sideLength = vertMatrix[0].Length;
            var subMeshList = new List<Mesh>();
            for (var i = 0; i < sideLength; i++)
            {
                var index = 0;
                var combineSubMesh = new CombineInstance[vertMatrix.Length - 1];
                for (var k = i; k < meshList.Count; k = k + sideLength)
                {
                    combineSubMesh[index].mesh = meshList[k];
                    combineSubMesh[index].transform = Matrix4x4.identity;
                    index++;
                }
                var mesh = new Mesh();
                mesh.CombineMeshes(combineSubMesh);
                subMeshList.Add(mesh);
            }

            //combine child meshes with parent mesh (so they share the same material)
            var parentVerts = vertMatrix[0].Where(x => x.Children != null && x.Children.Length > 0).ToArray();
            for (var i = 0; i < parentVerts.Length; i++)
            {
                var parentSubmeshIndex = Array.IndexOf(vertMatrix[0], parentVerts[i]);
                var childSubmeshes = subMeshList.GetRange(parentSubmeshIndex + 1, parentVerts[i].Children.Count());
                var parentSubMesh = subMeshList[parentSubmeshIndex];

                var combineSubMesh = new CombineInstance[childSubmeshes.Count + 1];
                combineSubMesh[0].mesh = parentSubMesh;
                combineSubMesh[0].transform = Matrix4x4.identity;

                for (var k = 0; k < childSubmeshes.Count; k++)
                {
                    combineSubMesh[k + 1].mesh = childSubmeshes[k];
                    combineSubMesh[k + 1].transform = Matrix4x4.identity;
                }
                var mesh = new Mesh();
                mesh.CombineMeshes(combineSubMesh);
                subMeshList[parentSubmeshIndex] = mesh;
            }

            //since the child meshes were combined with the parent, remove them from the list of meshes
            var subMeshes = new List<Mesh>();
            for (var i = 0; i < subMeshList.Count(); i++)
            {
                if (vertMatrix[0][i].Children != null && vertMatrix[0][i].Children.Count() > 0)
                {
                    subMeshes.Add(subMeshList[i]);
                    i = i + Array.IndexOf(vertMatrix[0], vertMatrix[0][i]) + 1 + vertMatrix[0][i].Children.Count();
                }
                else
                {
                    subMeshes.Add(subMeshList[i]);
                }
            }

            //set up end caps
            subMeshes.Add(GenerateEndCaps(vertMatrix[0], true));
            subMeshes.Add(GenerateEndCaps(vertMatrix[vertMatrix.Length - 1], false));

            //combine all sub meshes
            var combine = new CombineInstance[subMeshes.Count];
            for (var i = 0; i < combine.Length; i++)
            {
                combine[i].mesh = subMeshes[i];
                combine[i].transform = _gameObject.transform.worldToLocalMatrix;
            }
            return combine;
        }

        //generates a rectangular mesh with verts, tris, and uvs.
        Mesh GenerateMeshFromPoints(Vector3[] vertices, float uvLengthDistance, float uvWidthDistance, Vector2[] previousUvs, Vector2[] previousWidthUvs)
        {
            var vertices2D = new Vector2[vertices.Length];
            var cross = Vector3.Cross(vertices[1] - vertices[0], vertices[2] - vertices[1]);

            if (cross.x != 0)
            {
                if (cross.x < 0)
                {
                    vertices2D = Array.ConvertAll(vertices, x => new Vector2(x.z, x.y));
                }
                else
                {
                    vertices2D = Array.ConvertAll(vertices, x => new Vector2(x.y, x.z));
                }
            }
            else if (cross.y != 0)
            {
                if (cross.y < 0)
                {
                    vertices2D = Array.ConvertAll(vertices, x => new Vector2(x.x, x.z));
                }
                else
                {
                    vertices2D = Array.ConvertAll(vertices, x => new Vector2(x.z, x.x));
                }
            }
            else if (cross.z != 0)
            {
                if (cross.z < 0)
                {
                    vertices2D = Array.ConvertAll(vertices, x => new Vector2(x.x, x.y));
                }
                else
                {
                    vertices2D = Array.ConvertAll(vertices, x => new Vector2(x.y, x.x));
                }
            }

            // Use the triangulator to get indices for creating triangles
            var tr = new PlatformHelper.Triangulator(vertices2D);
            int[] indices = tr.Triangulate();

            //uvs
            var currentUvLengthDistance = (Mathf.Abs(Vector3.Distance(vertices[0], vertices[3])) / uvLengthDistance);
            var currentUvWidthDistance = (Mathf.Abs(Vector3.Distance(vertices[0], vertices[1])) / uvWidthDistance);
            var uvs = new Vector2[4];

            uvs[3] = new Vector2(previousUvs[0].x, previousWidthUvs[2].y);
            uvs[2] = new Vector2(previousUvs[1].x, previousWidthUvs[1].y + currentUvWidthDistance);
            uvs[1] = new Vector2(previousUvs[0].x + currentUvLengthDistance, previousWidthUvs[2].y + currentUvWidthDistance);
            uvs[0] = new Vector2(previousUvs[1].x + currentUvLengthDistance, previousWidthUvs[1].y);

            // Create the mesh
            Mesh msh = new Mesh();
            msh.vertices = vertices;
            msh.triangles = indices;
            msh.uv = uvs;
            return msh;
        }

        //generates the ends of the platform
        Mesh GenerateEndCaps(Vert[] vertices, bool isStart)
        {
            var vertices2D = new Vector2[vertices.Length];
            if (isStart)
            {
                vertices2D = Array.ConvertAll(vertices, x => new Vector2(x.Vector.x, x.Vector.y));
            }
            else
            {
                vertices2D = Array.ConvertAll(vertices, x => new Vector2(x.Vector.y, x.Vector.x));
            }

            // Use the triangulator to get indices for creating triangles
            var tr = new PlatformHelper.Triangulator(vertices2D);
            int[] indices = tr.Triangulate();

            // Create the mesh
            Mesh msh = new Mesh();
            msh.vertices = vertices.Select(x => x.Vector).ToArray(); ;
            msh.triangles = indices;
            return msh;
        }
    }
}