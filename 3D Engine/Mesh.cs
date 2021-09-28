using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    /// <summary>
    /// Structure needed to store information about faces
    /// </summary>
    public struct Face
    {
        public int A;
        public int B;
        public int C;
    }


    /// <summary>
    /// Class needed to store information about meshes
    /// </summary>
    public class Mesh
    {
        public string Name { get; set; }
        public Vector3[] Vertices { get; private set; }
        public Face[] Faces { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Matrix4x4 WorldMatrix { get; set; } // transformation matrix


        /// <summary>
        /// Constructor initializes initial values of three fields
        /// </summary>
        /// <param name="name"> Name of the mesh</param>
        /// <param name="verticesCount"> Number of vertices of the mesh</param>
        /// <param name="facesCount"> Number of faces of the mesh</param>
        public Mesh(string name, int verticesCount, int facesCount)
        {
            Name = name;
            Vertices = new Vector3[verticesCount];
            Faces = new Face[facesCount];
        }
    }
}
