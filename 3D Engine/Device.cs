using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    /// <summary>
    /// Class responsible for drawing, projecting 3D scene onto 2D plane and rendering
    /// </summary>
    class Device
    {
        public Bitmap viewport;
        Bitmap viewportWorking;
        int viewportWidth = 600;
        int viewportHeight = 600;

        RectangleF viewportRect; // used for line clipping


        /// <summary>
        /// Constructor initializes viewports needed to render the scene
        /// </summary>
        public Device()
        {
            viewport = new Bitmap(viewportWidth, viewportHeight);
            viewportWorking = new Bitmap(viewportWidth, viewportHeight);

            viewportRect = new RectangleF(0, 0, viewportWidth, viewportHeight);
        }


        /// <summary>
        /// Draws a point at a specified location
        /// </summary>
        /// <param name="point"> Location of the point</param>
        void DrawPoint(Vector2 point)
        {
            if (point.X >= 0 && point.Y >= 0 && point.X < viewportWidth && point.Y < viewportHeight)
            {
                viewportWorking.SetPixel((int)point.X, (int)point.Y, Color.Black);
            }
        }


        /// <summary>
        /// Projects a point in 3D space onto 2D plane
        /// </summary>
        /// <param name="vertex"> Coordinates of the point in 3D space</param>
        /// <param name="transformMatrix"> Transformation matrix</param>
        public Vector2 Project(Vector3 vertex, Matrix4x4 transformMatrix)
        {
            // transforming the coordinates into 2D space
            var point2D = Vector3.Transform(vertex, transformMatrix);

            if(point2D.Z <= 0)
                return new Vector2 { X = -10, Y = -10 };

            point2D.X = point2D.X / point2D.Z;
            point2D.Y = point2D.Y / point2D.Z;

            // The transformed coordinates will be based on coordinate system
            // starting on the center of the screen. But drawing on screen normally starts
            // from top left. We then need to transform them again to have x:0, y:0 on top left.
            var x = point2D.X * viewportWidth + viewportWidth / 2.0f;
            var y = -point2D.Y * viewportHeight + viewportHeight / 2.0f;

            return new Vector2 { X = x, Y = y };
        }


        /// <summary>
        /// Draws a line between two given points
        /// </summary>
        /// <param name="point0"> First point of the line</param>
        /// <param name="point1"> Last point of the line</param>
        public void DrawLine(Vector2 point0, Vector2 point1)
        {
            int x0 = (int)point0.X;
            int y0 = (int)point0.Y;
            int x1 = (int)point1.X;
            int y1 = (int)point1.Y;

            var dx = Math.Abs(x1 - x0);
            var dy = Math.Abs(y1 - y0);
            var sx = (x0 < x1) ? 1 : -1;
            var sy = (y0 < y1) ? 1 : -1;
            var err = dx - dy;

            while (true)
            {
                DrawPoint(new Vector2(x0, y0));

                if ((x0 == x1) && (y0 == y1)) break;
                var e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }
        }


        /// <summary>
        /// Renders the scene
        /// A list of meshes and a camera are needed to render the scene
        /// </summary>
        /// <param name="camera"> Camera object</param>
        /// <param name="meshes"> List of meshes to be rendered</param>
        public void Render(Camera camera, List<Mesh> meshes)
        {
            //clear the bitmap with white color
            using (Graphics graph = Graphics.FromImage(viewportWorking))
            {
                Rectangle imageSize = new Rectangle(0, 0, viewportWidth, viewportHeight);
                graph.FillRectangle(Brushes.White, imageSize);
            }

            Matrix4x4 viewMatrix = Matrix4x4.CreateLookAt(camera.Position, camera.Target, camera.UpVector);

            float fovAngle = (float)(Math.PI * 90 / 180.0);
            float aspectRatio = viewportWidth / (float)viewportHeight;
            float nearPlaneDistance = 0.01f;
            float farPlaneDistance = 1.0f;
            Matrix4x4 projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fovAngle, aspectRatio, nearPlaneDistance, farPlaneDistance);

            foreach (Mesh mesh in meshes)
            {
                // Beware to apply rotation before translation 
                // Matrix4x4 worldMatrix = Matrix4x4.CreateFromYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z) *
                //                        Matrix4x4.CreateTranslation(mesh.Position);

                Matrix4x4 transformMatrix = mesh.WorldMatrix * viewMatrix * projectionMatrix;

                foreach (var face in mesh.Faces)
                {
                    var vertexA = mesh.Vertices[face.A];
                    var vertexB = mesh.Vertices[face.B];
                    var vertexC = mesh.Vertices[face.C];

                    var pixelA = Project(vertexA, transformMatrix);
                    var pixelB = Project(vertexB, transformMatrix);
                    var pixelC = Project(vertexC, transformMatrix);

                    var line1 = CohenSutherland.ClipSegment(viewportRect, new PointF(pixelA.X, pixelA.Y), new PointF(pixelB.X, pixelB.Y));
                    if(line1 != null && !((pixelA.X == -10 && pixelA.Y == -10) || (pixelB.X == -10 && pixelB.Y == -10)))
                        DrawLine(new Vector2(line1.Item1.X, line1.Item1.Y), new Vector2(line1.Item2.X, line1.Item2.Y));

                    var line2 = CohenSutherland.ClipSegment(viewportRect, new PointF(pixelB.X, pixelB.Y), new PointF(pixelC.X, pixelC.Y));
                    if(line2 != null && !((pixelB.X == -10 && pixelB.Y == -10) || (pixelC.X == -10 && pixelC.Y == -10)))
                        DrawLine(new Vector2(line2.Item1.X, line2.Item1.Y), new Vector2(line2.Item2.X, line2.Item2.Y));

                    var line3 = CohenSutherland.ClipSegment(viewportRect, new PointF(pixelC.X, pixelC.Y), new PointF(pixelA.X, pixelA.Y));
                    if(line3 != null && !((pixelC.X == -10 && pixelC.Y == -10) || (pixelA.X == -10 && pixelA.Y == -10)))
                        DrawLine(new Vector2(line3.Item1.X, line3.Item1.Y), new Vector2(line3.Item2.X, line3.Item2.Y));
                }
            }

            if (viewport != null)
            {
                viewport.Dispose();
            }
            viewport = new Bitmap(viewportWorking);
        }
    }
}
