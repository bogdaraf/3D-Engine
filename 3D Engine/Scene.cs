using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Engine
{
    /// <summary>
    /// Class responsible for loading scene elements from XML file
    /// </summary>
    class Scene
    {
        SceneElements sceneElements;


        /// <summary>
        /// Constructor initializes scene loading
        /// </summary>
        /// <param name="path"> Path to XML file</param>
        public Scene(string path)
        {
            sceneElements = LoadSceneFromXml(path);
        }


        /// <summary>
        /// Loads and returns scene elements from XML file
        /// </summary>
        /// <param name="path"> Path to XML file</param>
        SceneElements LoadSceneFromXml(string path)
        {
            TextReader reader = new StreamReader(path);

            XmlSerializer deserializer = new XmlSerializer(typeof(SceneElements));
            sceneElements = (SceneElements)deserializer.Deserialize(reader);

            reader.Close();

            return sceneElements;
        }


        /// <summary>
        /// Returns a list of meshes from scene elements
        /// </summary>
        public List<Mesh> CreateMeshesFromSceneElements()
        {
            List<Mesh> meshes = new List<Mesh>();

            foreach(Cuboid cuboid in sceneElements.ArrayOfCuboids.cuboidList)
            {
                meshes.Add(CreateCuboidMesh(cuboid));
            }

            foreach (Sphere sphere in sceneElements.ArrayOfSpheres.sphereList)
            {
                meshes.Add(CreateSphereMesh(sphere));
            }

            foreach (Cylinder cylinder in sceneElements.ArrayOfCylinders.cylinderList)
            {
                meshes.Add(CreateCylinderMesh(cylinder));
            }

            foreach (Cone cone in sceneElements.ArrayOfCones.coneList)
            {
                meshes.Add(CreateConeMesh(cone));
            }

            return meshes;
        }


        /// <summary>
        /// Creates a cuboid mesh from a Cuboid object
        /// </summary>
        /// <param name="cuboid"> Cuboid object</param>
        Mesh CreateCuboidMesh(Cuboid cuboid)
        {
            Mesh mesh = new Mesh("Cuboid", 8, 12);

            float halfLengthX = cuboid.LengthX / 2;
            float halfLengthY = cuboid.LengthY / 2;
            float halfLengthZ = cuboid.LengthZ / 2;

            mesh.Vertices[0] = new Vector3(-halfLengthX, halfLengthY, halfLengthZ);
            mesh.Vertices[1] = new Vector3(halfLengthX, halfLengthY, halfLengthZ);
            mesh.Vertices[2] = new Vector3(-halfLengthX, -halfLengthY, halfLengthZ);
            mesh.Vertices[3] = new Vector3(halfLengthX, -halfLengthY, halfLengthZ);
            mesh.Vertices[4] = new Vector3(-halfLengthX, halfLengthY, -halfLengthZ);
            mesh.Vertices[5] = new Vector3(halfLengthX, halfLengthY, -halfLengthZ);
            mesh.Vertices[6] = new Vector3(halfLengthX, -halfLengthY, -halfLengthZ);
            mesh.Vertices[7] = new Vector3(-halfLengthX, -halfLengthY, -halfLengthZ);

            mesh.Faces[0] = new Face { A = 0, B = 1, C = 2 };
            mesh.Faces[1] = new Face { A = 1, B = 2, C = 3 };
            mesh.Faces[2] = new Face { A = 1, B = 3, C = 6 };
            mesh.Faces[3] = new Face { A = 1, B = 5, C = 6 };
            mesh.Faces[4] = new Face { A = 0, B = 1, C = 4 };
            mesh.Faces[5] = new Face { A = 1, B = 4, C = 5 };
            mesh.Faces[6] = new Face { A = 2, B = 3, C = 7 };
            mesh.Faces[7] = new Face { A = 3, B = 6, C = 7 };
            mesh.Faces[8] = new Face { A = 0, B = 2, C = 7 };
            mesh.Faces[9] = new Face { A = 0, B = 4, C = 7 };
            mesh.Faces[10] = new Face { A = 4, B = 5, C = 6 };
            mesh.Faces[11] = new Face { A = 4, B = 6, C = 7 };

            mesh.WorldMatrix = CreateWorldMatrix(cuboid.Row1, cuboid.Row2, cuboid.Row3, cuboid.Row4);

            return mesh;
        }

        /// <summary>
        /// Creates a sphere mesh from a Sphere object
        /// </summary>
        /// <param name="sphere"> Sphere object</param>
        Mesh CreateSphereMesh(Sphere sphere)
        {
            int n = sphere.Triangles;
            float radius = sphere.Radius;

            List<Vector3> vertices = new List<Vector3>();
            List<Face> faces = new List<Face>();

            //vertices of a UV sphere: https://analogfolk.com/news/10-steps-to-creating-an-unwrapped-sphere-in-blender
            for (int i=0; i<n; i++)
            {
                float y = (float)Math.Sin(i / (float)(n-1) * Math.PI - Math.PI / 2) * radius; //must be shifted by -pi/2

                for (int j=0; j<n; j++)
                {
                    float x = (float)Math.Cos(j / (float)n * 2 * Math.PI) * (float)Math.Sqrt(radius * radius - y * y);
                    float z = (float)Math.Sin(j / (float)n * 2 * Math.PI) * (float)Math.Sqrt(radius * radius - y * y);

                    vertices.Add(new Vector3(x, y, z));
                }
            }

            //faces
            for (int i = 0; i < n-1; i++)
            {
                int s = i * n; //starting index for vertices at current level

                for (int j = 0; j < n; j++)
                {
                    faces.Add(new Face() { A = s + j, B = s + (j + 1) % n, C = s + j + n });
                }
            }

            Mesh mesh = new Mesh("Sphere", vertices.Count, faces.Count);
            for (int i = 0; i < vertices.Count; i++)
            {
                mesh.Vertices[i] = vertices[i];
            }
            for (int i = 0; i < faces.Count; i++)
            {
                mesh.Faces[i] = faces[i];
            }

            mesh.WorldMatrix = CreateWorldMatrix(sphere.Row1, sphere.Row2, sphere.Row3, sphere.Row4);

            return mesh;
        }


        /// <summary>
        /// Creates a cylinder mesh from a Cylinder object
        /// </summary>
        /// <param name="cylinder"> Cylinder object</param>
        Mesh CreateCylinderMesh(Cylinder cylinder)
        {
            int verticesInBase = cylinder.Triangles / 4;
            int nOfVertices = 2 * (verticesInBase + 1);
            int nOfFaces = 4 * verticesInBase;

            float halfHeight = cylinder.Height / 2;

            Mesh mesh = new Mesh("Cylinder", nOfVertices, nOfFaces);

            // vertices in the first base
            mesh.Vertices[0] = new Vector3(0, halfHeight, 0);
            for (int i = 1; i < verticesInBase + 1; i++)
            {
                float x = (float)Math.Cos((i - 1) / (float)verticesInBase * 2 * Math.PI) * cylinder.Radius;
                float z = (float)Math.Sin((i - 1) / (float)verticesInBase * 2 * Math.PI) * cylinder.Radius;
                mesh.Vertices[i] = new Vector3(x, halfHeight, z);
            }

            // vertices in the second base
            mesh.Vertices[nOfVertices / 2] = new Vector3(0, -halfHeight, 0);
            for (int i = nOfVertices / 2 + 1; i < nOfVertices / 2 + verticesInBase + 1; i++)
            {
                float x = (float)Math.Cos((i - nOfVertices / 2 - 1) / (float)verticesInBase * 2 * Math.PI) * cylinder.Radius;
                float z = (float)Math.Sin((i - nOfVertices / 2 - 1) / (float)verticesInBase * 2 * Math.PI) * cylinder.Radius;
                mesh.Vertices[i] = new Vector3(x, -halfHeight, z);
            }

            // faces in the first base
            for (int i = 0; i < nOfFaces / 4 - 1; i++)
            {
                mesh.Faces[i] = new Face { A = 0, B = i + 1, C = i + 2 };
            }
            mesh.Faces[nOfFaces / 4 - 1] = new Face { A = 0, B = nOfFaces / 4, C = 1 };

            // faces in the second base
            for (int i = nOfFaces / 4; i < nOfFaces / 4 + nOfFaces / 4 - 1; i++)
            {
                mesh.Faces[i] = new Face { A = nOfVertices / 2, B = i + 2, C = i + 3 };
            }
            mesh.Faces[nOfFaces / 4 + nOfFaces / 4 - 1] = new Face { A = nOfVertices / 2, B = nOfVertices - 1, C = nOfVertices / 2 + 1 };

            // faces between the bases (triangles with bases in the first base)
            for (int i = nOfFaces / 2; i < nOfFaces / 2 + nOfFaces / 4 - 1; i++)
            {
                mesh.Faces[i] = new Face { A = i - nOfFaces / 2 + 1, B = i - nOfFaces / 2 + 2, C = i - nOfFaces / 2 + nOfVertices / 2 + 1 };
            }
            mesh.Faces[nOfFaces / 2 + nOfFaces / 4 - 1] = new Face { A = nOfVertices / 2 - 1, B = 1, C = nOfVertices - 1 };

            // faces between the bases (triangles with bases in the second base)
            for (int i = 3 * nOfFaces / 4; i < nOfFaces - 1; i++)
            {
                mesh.Faces[i] = new Face { A = i - 3 * nOfFaces / 4 + 2, B = i - 3 * nOfFaces / 4 + nOfVertices / 2 + 1, C = i - 3 * nOfFaces / 4 + nOfVertices / 2 + 2 };
            }
            mesh.Faces[nOfFaces - 1] = new Face { A = 1, B = nOfVertices - 1, C = nOfVertices / 2 + 1 };

            mesh.WorldMatrix = CreateWorldMatrix(cylinder.Row1, cylinder.Row2, cylinder.Row3, cylinder.Row4);

            return mesh;
        }


        /// <summary>
        /// Creates a cone mesh from a Cone object
        /// </summary>
        /// <param name="cone"> Cone object</param>
        Mesh CreateConeMesh(Cone cone)
        {
            int verticesInBase = cone.Triangles / 2;
            int nOfVertices = 2 + verticesInBase;
            int nOfFaces = 2 * verticesInBase;

            float halfHeight = cone.Height / 2;

            Mesh mesh = new Mesh("Cone", nOfVertices, nOfFaces);

            mesh.Vertices[0] = new Vector3(0, halfHeight, 0);
            for(int i=1; i<verticesInBase + 1; i++)
            {
                float x = (float)Math.Cos((i - 1) / (float)verticesInBase * 2 * Math.PI) * cone.Radius;
                float z = (float)Math.Sin((i - 1) / (float)verticesInBase * 2 * Math.PI) * cone.Radius;
                mesh.Vertices[i] = new Vector3(x, -halfHeight, z);
            }
            mesh.Vertices[nOfVertices - 1] = new Vector3(0, -halfHeight, 0);

            // faces from the top of the cone to the base
            for (int i=0; i<nOfFaces / 2 - 1; i++)
            {
                mesh.Faces[i] = new Face { A = 0, B = i + 1, C = i + 2 };
            }
            mesh.Faces[nOfFaces / 2 - 1] = new Face { A = 0, B = nOfFaces / 2, C = 1 };

            // faces on the base
            for(int i=nOfFaces / 2; i<nOfFaces - 1; i++)
            {
                mesh.Faces[i] = new Face { A = nOfVertices - 1, B =  i - nOfFaces / 2 + 1, C = i - nOfFaces / 2 + 2 };
            }
            mesh.Faces[nOfFaces - 1] = new Face { A = nOfVertices - 1, B = nOfFaces / 2, C = 1 };

            mesh.WorldMatrix = CreateWorldMatrix(cone.Row1, cone.Row2, cone.Row3, cone.Row4);

            return mesh;
        }


        /// <summary>
        /// Returns world matrix based on given strings
        /// </summary>
        /// <param name="row1"> Row 1 of the matrix</param>
        /// <param name="row2"> Row 2 of the matrix</param>
        /// <param name="row3"> Row 3 of the matrix</param>
        /// <param name="row4"> Row 4 of the matrix</param>
        Matrix4x4 CreateWorldMatrix(string row1, string row2, string row3, string row4)
        {
            string[] row1valuesString = row1.Split(' ');
            string[] row2valuesString = row2.Split(' ');
            string[] row3valuesString = row3.Split(' ');
            string[] row4valuesString = row4.Split(' ');

            Matrix4x4 worldMatrix = new Matrix4x4();
            worldMatrix.M11 = float.Parse(row1valuesString[0]);
            worldMatrix.M12 = float.Parse(row1valuesString[1]);
            worldMatrix.M13 = float.Parse(row1valuesString[2]);
            worldMatrix.M14 = float.Parse(row1valuesString[3]);
            worldMatrix.M21 = float.Parse(row2valuesString[0]);
            worldMatrix.M22 = float.Parse(row2valuesString[1]);
            worldMatrix.M23 = float.Parse(row2valuesString[2]);
            worldMatrix.M24 = float.Parse(row2valuesString[3]);
            worldMatrix.M31 = float.Parse(row3valuesString[0]);
            worldMatrix.M32 = float.Parse(row3valuesString[1]);
            worldMatrix.M33 = float.Parse(row3valuesString[2]);
            worldMatrix.M34 = float.Parse(row3valuesString[3]);
            worldMatrix.M41 = float.Parse(row4valuesString[0]);
            worldMatrix.M42 = float.Parse(row4valuesString[1]);
            worldMatrix.M43 = float.Parse(row4valuesString[2]);
            worldMatrix.M44 = float.Parse(row4valuesString[3]);

            return Matrix4x4.Transpose(worldMatrix);
        }
    }


    /// <summary>
    /// Class used to store elements of the scene
    /// </summary>
    public class SceneElements
    {
        [XmlElement("ArrayOfCuboids")]
        public ArrayOfCuboids ArrayOfCuboids { get; set; }

        [XmlElement("ArrayOfSpheres")]
        public ArrayOfSpheres ArrayOfSpheres { get; set; }

        [XmlElement("ArrayOfCylinders")]
        public ArrayOfCylinders ArrayOfCylinders { get; set; }

        [XmlElement("ArrayOfCones")]
        public ArrayOfCones ArrayOfCones { get; set; }
    }


    /// <summary>
    /// Auxiliary class needed for loading scene elements from XML file
    /// </summary>
    public class ArrayOfCuboids
    {
        [XmlElement("Cuboid")]
        public List<Cuboid> cuboidList = new List<Cuboid>();
    }


    /// <summary>
    /// Auxiliary class needed for loading scene elements from XML file
    /// </summary>
    public class Cuboid
    {
        [XmlElement("LengthX")]
        public float LengthX { get; set; }

        [XmlElement("LengthY")]
        public float LengthY { get; set; }

        [XmlElement("LengthZ")]
        public float LengthZ { get; set; }

        [XmlElement("Row1")]
        public string Row1 { get; set; }

        [XmlElement("Row2")]
        public string Row2 { get; set; }

        [XmlElement("Row3")]
        public string Row3 { get; set; }

        [XmlElement("Row4")]
        public string Row4 { get; set; }
    }


    /// <summary>
    /// Auxiliary class needed for loading scene elements from XML file
    /// </summary>
    public class ArrayOfSpheres
    {
        [XmlElement("Sphere")]
        public List<Sphere> sphereList = new List<Sphere>();
    }


    /// <summary>
    /// Class needed to store information about spheres
    /// </summary>
    public class Sphere
    {
        [XmlElement("Radius")]
        public float Radius { get; set; }

        [XmlElement("Triangles")]
        public int Triangles { get; set; }

        [XmlElement("Row1")]
        public string Row1 { get; set; }

        [XmlElement("Row2")]
        public string Row2 { get; set; }

        [XmlElement("Row3")]
        public string Row3 { get; set; }

        [XmlElement("Row4")]
        public string Row4 { get; set; }
    }


    /// <summary>
    /// Auxiliary class needed for loading scene elements from XML file
    /// </summary>
    public class ArrayOfCylinders
    {
        [XmlElement("Cylinder")]
        public List<Cylinder> cylinderList = new List<Cylinder>();
    }


    /// <summary>
    /// Class needed to store information about cylinders
    /// </summary>
    public class Cylinder
    {
        [XmlElement("Radius")]
        public float Radius { get; set; }

        [XmlElement("Height")]
        public float Height { get; set; }

        [XmlElement("Triangles")]
        public int Triangles { get; set; }

        [XmlElement("Row1")]
        public string Row1 { get; set; }

        [XmlElement("Row2")]
        public string Row2 { get; set; }

        [XmlElement("Row3")]
        public string Row3 { get; set; }

        [XmlElement("Row4")]
        public string Row4 { get; set; }
    }


    /// <summary>
    /// Auxiliary class needed for loading scene elements from XML file
    /// </summary>
    public class ArrayOfCones
    {
        [XmlElement("Cone")]
        public List<Cone> coneList = new List<Cone>();
    }


    /// <summary>
    /// Class needed to store information about cones
    /// </summary>
    public class Cone
    {
        [XmlElement("Radius")]
        public float Radius { get; set; }

        [XmlElement("Height")]
        public float Height { get; set; }

        [XmlElement("Triangles")]
        public int Triangles { get; set; }

        [XmlElement("Row1")]
        public string Row1 { get; set; }

        [XmlElement("Row2")]
        public string Row2 { get; set; }

        [XmlElement("Row3")]
        public string Row3 { get; set; }

        [XmlElement("Row4")]
        public string Row4 { get; set; }
    }
}
