using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    /// <summary>
    /// This class contains the definition of Camera object
    /// </summary>
    public class Camera
    {
        public Vector3 Position { get; set; }
        public Vector3 Target { get; set; }
        public Vector3 UpVector { get; set; }
        public Vector3 LookAt { get; set; }
    }
}
