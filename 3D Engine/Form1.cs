using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Engine
{
    /// <summary>
    /// Class responsible for managing the Form component and logic of the camera movement
    /// </summary>
    public partial class Form1 : Form
    {
        Device device = new Device();
        List<Mesh> meshes = new List<Mesh>();
        Camera camera = new Camera();

        delegate void InvokeDelegate();
        double fps;
        long frameTime;

        bool moveCameraW;
        bool moveCameraA;
        bool moveCameraS;
        bool moveCameraD;
        bool moveCameraUp;
        bool moveCameraDown;
        bool moveCameraLeft;
        bool moveCameraRight;


        /// <summary>
        /// Constructor initializes the Form component
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }


        /// <summary>
        /// This function is called once after the form is initially loaded
        /// Scene elements are loaded from XML file, camera setup is done, animation thread is started
        /// </summary>
        private void Form1_Load(object sender, EventArgs e)
        {
            Scene scene = new Scene(@"../../scene.xml");
            meshes = scene.CreateMeshesFromSceneElements();

            camera.Position = new Vector3(1.2f, 0, 8);
            camera.UpVector = Vector3.UnitY;
            camera.LookAt = new Vector3(0, 0, 1);//it's a vector; the camera is pointed at the direction of this vector
            camera.Target = camera.Position - camera.LookAt;

            Thread animationThread = new Thread(Animation);
            animationThread.Start();
        }


        /// <summary>
        /// This function is called every time the form needs to be refreshed
        /// Draws a bitmap containing the current frame on the screen
        /// </summary>
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            if (device.viewport != null)
            {
                g.DrawImage(device.viewport, 0, 0);
            }
        }


        /// <summary>
        /// Contains infinite loop where animation is processed
        /// </summary>
        private void Animation()
        {
            long lastFrameTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            while (true) //infinite loop; must be in another thread to avoid UI block
            {
                lastFrameTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                UpdateLogic();
                device.Render(camera, meshes);
                this.Invoke(new InvokeDelegate(this.Refresh)); // calls Refresh(); must be in this way coz this function is not in UI thread

                frameTime = DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastFrameTime;
                fps = (double)1000 / frameTime;
                Console.WriteLine("1 frame rendered in " + frameTime + " ms, fps = " + fps);
            }
        }


        /// <summary>
        /// Updates logic, calls a method which moves a camera
        /// </summary>
        private void UpdateLogic()
        {
            MoveCamera();
        }


        /// <summary>
        /// Moves camera properly based on the currently pressed keys
        /// </summary>
        private void MoveCamera()
        {
            if (moveCameraW)
            {
                camera.Position -= camera.LookAt / 50;
                camera.Target = camera.Position - camera.LookAt;
            }
            if (moveCameraS)
            {
                camera.Position += camera.LookAt / 50;
                camera.Target = camera.Position - camera.LookAt;
            }
            if(moveCameraA)
            {
                Vector3 cross = Vector3.Cross(camera.LookAt, camera.UpVector);
                camera.Position += cross / 50;
                camera.Target = camera.Position - camera.LookAt;
            }
            if (moveCameraD)
            {
                Vector3 cross = Vector3.Cross(camera.LookAt, camera.UpVector);
                camera.Position -= cross / 50;
                camera.Target = camera.Position - camera.LookAt;
            }
            if(moveCameraUp)
            {
                camera.LookAt -= camera.UpVector / 100;
                camera.Target = camera.Position - camera.LookAt;
            }
            if (moveCameraDown)
            {
                camera.LookAt += camera.UpVector / 100;
                camera.Target = camera.Position - camera.LookAt;
            }
            if (moveCameraLeft)
            {
                Vector3 cross = Vector3.Cross(camera.LookAt, camera.UpVector);
                camera.LookAt -= cross / 100;
                camera.Target = camera.Position - camera.LookAt;
            }
            if (moveCameraRight)
            {
                Vector3 cross = Vector3.Cross(camera.LookAt, camera.UpVector);
                camera.LookAt += cross / 100;
                camera.Target = camera.Position - camera.LookAt;
            }

            Console.WriteLine("cam pos: " + camera.Position.X + " " + camera.Position.Y + " " + camera.Position.Z);
            Console.WriteLine("cam tar: " + camera.Target.X + " " + camera.Target.Y + " " + camera.Target.Z);
        }


        /// <summary>
        /// Manages events happening after a key is pressed
        /// </summary>
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W)
            {
                moveCameraW = true;
            }
            if (e.KeyCode == Keys.A)
            {
                moveCameraA = true;
            }
            if (e.KeyCode == Keys.S)
            {
                moveCameraS = true;
            }
            if (e.KeyCode == Keys.D)
            {
                moveCameraD = true;
            }
            if (e.KeyCode == Keys.Up)
            {
                moveCameraUp = true;
            }
            if (e.KeyCode == Keys.Down)
            {
                moveCameraDown = true;
            }
            if (e.KeyCode == Keys.Left)
            {
                moveCameraLeft = true;
            }
            if (e.KeyCode == Keys.Right)
            {
                moveCameraRight = true;
            }
        }


        /// <summary>
        /// Manages events happening after a key is released
        /// </summary>
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W)
            {
                moveCameraW = false;
            }
            if (e.KeyCode == Keys.A)
            {
                moveCameraA = false;
            }
            if (e.KeyCode == Keys.S)
            {
                moveCameraS = false;
            }
            if (e.KeyCode == Keys.D)
            {
                moveCameraD = false;
            }
            if (e.KeyCode == Keys.Up)
            {
                moveCameraUp = false;
            }
            if (e.KeyCode == Keys.Down)
            {
                moveCameraDown = false;
            }
            if (e.KeyCode == Keys.Left)
            {
                moveCameraLeft = false;
            }
            if (e.KeyCode == Keys.Right)
            {
                moveCameraRight = false;
            }
        }
    }
}
