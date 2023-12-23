using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Flat1.Graphics
{
    public sealed class Camera
    {
        public readonly static float MinZ = 1f;
        public readonly static float MaxZ = 2048f;

        public readonly static int MinZoom = 1;
        public readonly static int MaxZoom = 20;
        // 摄像机的位置 (在视野中心)!
        private Vector2 position;
        private float z;
        private float basez;

        private int zoom;

        // 长宽比
        private float aspectRatio;
        // 视野
        private float fieldOfView;

        private Matrix view;
        private Matrix proj;

        public Vector2 Position
        {
            get { return this.position; }
        }

        public float Z
        {
            get { return this.z; }
        }
        public float BaseZ
        {
            get { return this.basez; }
        }

        public Matrix View
        { get { return this.view; } }

        public Matrix Projection
        {
            get
            {
                return this.proj;
            }
        }

        public Camera(Screen screen)
        {
            if (screen is null)
                throw new ArgumentNullException("screen");

            this.aspectRatio = (float)screen.Width / screen.Height;
            this.fieldOfView = MathHelper.PiOver2;

            this.position = new Vector2(0, 0);
            this.basez = GetZFromHeight(screen.Height);
            this.z = this.basez;

            this.UpdateMatrices();
            this.zoom = 1;
        }

        public void UpdateMatrices()
        {
            this.view = Matrix.CreateLookAt(new Vector3(0, 0, this.z), Vector3.Zero, Vector3.Up);
            this.proj = Matrix.CreatePerspectiveFieldOfView(this.fieldOfView, this.aspectRatio, Camera.MinZ, Camera.MaxZ);
        }

        // 这里的用法是为了获取初始的Z
        public float GetZFromHeight(float height)
        {
            return (0.5f * height) / MathF.Tan(0.5f * this.fieldOfView);
        }

        // 这里的用法是为了工具动态改变的Z值,得出Height的值
        public float GetHeightFromZ()
        {
            return this.z * MathF.Tan(0.5f * this.fieldOfView) * 2f;
        }
        public void MoveZ(float amount)
        {
            this.z += amount;
            this.z = Utils.Clamp(this.z, Camera.MinZ, Camera.MaxZ);
        }

        public void ResetZ()
        {
            this.z = this.basez;
        }

        public void Move(Vector2 amount)
        {
            this.position += amount;
        }

        public void MoveTo(Vector2 position)
        {
            this.position = position;
        }

        public void InZoom()
        {
            this.zoom++;
            this.zoom = Utils.Clamp(this.zoom, Camera.MinZoom, Camera.MaxZoom);
            this.z = this.basez / this.zoom;
        }

        public void DecZoom()
        {
            this.zoom--;
            this.zoom = Utils.Clamp(this.zoom, Camera.MinZoom, Camera.MaxZoom);
            this.z = this.basez / this.zoom;
        }

        public void SetZoom(int amount)
        {
            this.zoom = amount;
            this.zoom = Utils.Clamp(this.zoom, Camera.MinZoom, Camera.MaxZoom);
            this.z = this.basez / this.zoom;
        }

        // 获得可视范围
        public void GetExtents(out float width, out float height)
        {
            height = this.GetHeightFromZ();
            width = height * this.aspectRatio;
        }

        // 这其实是 AABB 包围盒,这是直线!!!
        public void GetExtents(out float left, out float right, out float bottom, out float top)
        {
            this.GetExtents(out float width, out float height);

            left = this.position.X - width * 0.5f;
            right = left + width;
            top = this.position.Y + height * 0.5f;
            bottom = top - height;
        }
        // 得到对角线的坐标(左下,右上)
        public void GetExtents(out Vector2 min, out Vector2 max)
        {
            this.GetExtents(out float left, out float right, out float bottom, out float top);
            min = new Vector2(left, bottom);
            max = new Vector2(right, top);
        }
    }
}
