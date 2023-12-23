using Microsoft.Xna.Framework;
using System;


namespace FlatPhysics
{
    public enum ShapeType
    {
        Circle = 0,
        Box = 1,
    };
    public sealed class FlatBody
    {
        private FlatVector position;
        private FlatVector linearVelocity;
        private float angle;
        private float angularVelocity;
        private FlatVector force;

        private FlatAABB aabb;
        private bool aabbUpdateRequired;

        public FlatVector Position
        {
            get { return position; }
        }

        public float Angle
        {
            get { return this.angle; }
        }
        public float AngularVelocity
        {
            get 
            { 
                return this.angularVelocity; 
            }
            internal set 
            {
                this.angularVelocity = value; 
            }
        }


        public FlatVector LinearVelocity
        {
            get
            {
                return this.linearVelocity;
            }
            internal set
            {
                this.linearVelocity = value;
            }
        }

        public ShapeType shapeType;
        public readonly float Density;  // 密度
        public readonly float Mass;     // 质量
        public readonly float InvMass;
        public readonly float Inertia;  // 惯性
        public readonly float InvInertia;
        public readonly float Restitution; // 可恢复程度
        public readonly float Area;
        public readonly bool IsStatic;  // 是否是静态的 如果是static 那么position就是固定的
        public readonly float Radius;
        public readonly float Width;
        public readonly float Height;
        // 摩擦力
        public readonly float StaticFriction;
        public readonly float DynamicFriction;


        private readonly FlatVector[] vertices;

        // 变换后的顶点坐标,缓存下来,这样就不用重新转化他们
        private FlatVector[] transformedVertices;
        // 因为不是需要transformedVertices 每次都更新,所以要一个变量来标志是否需要更新
        private bool transformUpdateRequired;


        private FlatBody(float density, float mass, float inertia, float restitution, float area,
            bool isStatic, float radius, float width, float heigth, FlatVector[] vertices, ShapeType shapeType)
        {
            this.position = FlatVector.Zero;
            this.linearVelocity = FlatVector.Zero;
            this.angle = 0f;
            this.angularVelocity = 0f;
            this.Density = density;
            this.Restitution = restitution;
            this.Area = area;
            this.IsStatic = isStatic;
            this.Mass = mass;
            this.Inertia = inertia;
            this.Radius = radius;
            this.Width = width;
            this.Height = heigth;
            this.shapeType = shapeType;
            this.StaticFriction = 0.6f;
            this.DynamicFriction = 0.4f;

            this.InvMass = 0;
            this.InvInertia = 0;

            if (!this.IsStatic)
            {
                this.InvMass = 1 / mass;
                this.InvInertia = 1 / this.Inertia;
            }

            if (this.shapeType is ShapeType.Box)
            {
                this.vertices = vertices;
                this.transformedVertices = new FlatVector[vertices.Length];
            }
            else
            {
                this.vertices = null;
                this.transformedVertices = null;
            }

            this.transformUpdateRequired = true;
            this.aabbUpdateRequired = true;
        }

        private static FlatVector[] CreateBoxVertices(float width, float height)
        {
            float left = -width / 2f;
            float right = left + width;
            float bottom = -height / 2f;
            float top = bottom + height;

            FlatVector[] vertices = new FlatVector[4];
            vertices[0] = new FlatVector(left, top);
            vertices[1] = new FlatVector(right, top);
            vertices[2] = new FlatVector(right, bottom);
            vertices[3] = new FlatVector(left, bottom);

            return vertices;
        }

        public static int[] CreateBoxTriangles()
        {
            int[] triangles = new int[6];
            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 2;
            triangles[3] = 0;
            triangles[4] = 2;
            triangles[5] = 3;
            return triangles;
        }

        public FlatAABB GetAABB()
        {
            if (this.aabbUpdateRequired)
            {
                float minX = float.MaxValue;
                float minY = float.MaxValue;
                float maxX = float.MinValue;
                float maxY = float.MinValue;

                if (this.shapeType is ShapeType.Circle)
                {
                    minX = this.position.X - this.Radius;
                    maxX = this.position.X + this.Radius;
                    minY = this.position.Y - this.Radius;
                    maxY = this.position.Y + this.Radius;
                }
                else if (this.shapeType is ShapeType.Box)
                {
                    FlatVector[] vertices = this.GetTransformedVertices(); ;
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        FlatVector v = vertices[i];
                        if (v.X < minX) { minX = v.X; }
                        if (v.X > maxX) { maxX = v.X; }
                        if (v.Y < minY) { minY = v.Y; }
                        if (v.Y > maxY) { maxY = v.Y; }
                    }
                }
                else
                {
                    throw new Exception("Unknow ShapeType");
                }
                this.aabb = new FlatAABB(minX, minY, maxX, maxY);
            }

            this.aabbUpdateRequired = false;

            return this.aabb;
        }

        public void Step(float time, FlatVector gravity, int iteration)
        {
            //FlatVector acceleration = this.force * this.InvMass;
            //this.linearVelocity += acceleration * time;
            //// 瞬间的一个力
            //this.force = FlatVector.Zero;

            if (this.IsStatic)
            {
                return;
            }
            time /= (float)iteration;
            this.linearVelocity += gravity * time;
            this.position += this.linearVelocity * time;

            this.angle += this.angularVelocity * time;
            this.transformUpdateRequired = true;
            this.aabbUpdateRequired = true;
        }

        public FlatVector[] GetTransformedVertices()
        {
            if (this.transformUpdateRequired)
            {
                FlatTransform transform = new FlatTransform(this.position, this.angle);

                for (int i = 0; i < this.transformedVertices.Length; i++)
                {
                    FlatVector v = this.vertices[i];
                    this.transformedVertices[i] = FlatVector.Transform(v, transform);
                }
            }
            this.transformUpdateRequired = false;
            return this.transformedVertices;
        }

        public void Move(FlatVector amoount)
        {
            this.position += amoount;
            this.transformUpdateRequired = true;
            this.aabbUpdateRequired = true;
        }

        public void MoveTo(FlatVector position)
        {
            this.position = position;
            this.transformUpdateRequired = true;
            this.aabbUpdateRequired = true;
        }

        public void Rotate(float amount)
        {
            this.angle += amount;
            this.transformUpdateRequired = true;
            this.aabbUpdateRequired = true;
        }

        public void RotateTo(float amount)
        {
            this.angle = amount;
            this.transformUpdateRequired = true;
            this.aabbUpdateRequired = true;
        }

        public void AddForce(FlatVector force)
        {
            this.force = force;
        }

        public static bool CreateCircleBody(float radius, float density,
            bool isStatic, float restitution, out FlatBody body, out string errorMsg)
        {
            body = null;
            errorMsg = string.Empty;
            float area = radius * radius * MathHelper.Pi;

            #region some Check
            if (area < FlatWorld.MinBodySize)
            {
                errorMsg = $"Circle radius is too small. Min Circle area is {FlatWorld.MinBodySize}";
                return false;
            }

            if (area > FlatWorld.MaxBodySize)
            {
                errorMsg = $"Circle radius is too big. Min Circle area is {FlatWorld.MaxBodySize}";
                return false;
            }

            if (density < FlatWorld.MinDensity)
            {
                errorMsg = $"Density is too small. Min density is {FlatWorld.MinDensity}";
                return false;
            }
            if (density > FlatWorld.MaxDensity)
            {
                errorMsg = $"Density is too big. Max density is {FlatWorld.MaxDensity}";
                return false;
            }
            #endregion

            restitution = FlatMath.Clamp(restitution, 0, 1);

            float mass = area * density;
            float inertia = 0.5f * mass * radius * radius;
            body = new FlatBody(density, mass, inertia, restitution, area, isStatic, radius, 0, 0, null, ShapeType.Circle);
            return true;
        }


        public static bool CreateBoxBody(float width, float height, float density,
            bool isStatic, float restitution, out FlatBody body, out string errorMsg)
        {
            body = null;
            errorMsg = string.Empty;
            float area = height * width;

            #region some Check
            if (area < FlatWorld.MinBodySize)
            {
                errorMsg = $"Area is too small. Min area is {FlatWorld.MinBodySize}";
                return false;
            }

            if (area > FlatWorld.MaxBodySize)
            {
                errorMsg = $"Area is too big. Min area is {FlatWorld.MaxBodySize}";
                return false;
            }

            if (density < FlatWorld.MinDensity)
            {
                errorMsg = $"Density is too small. Min density is {FlatWorld.MinDensity}";
                return false;
            }
            if (density > FlatWorld.MaxDensity)
            {
                errorMsg = $"Density is too big. Max density is {FlatWorld.MaxDensity}";
                return false;
            }
            #endregion

            restitution = FlatMath.Clamp(restitution, 0, 1);

            float mass = area * density;
            float inertia = (1f / 12) * mass * (width * width + height * height);

            FlatVector[] vertices = FlatBody.CreateBoxVertices(width, height);
            body = new FlatBody(density, mass, inertia, restitution, area, isStatic, 0f, width, height, vertices, ShapeType.Box);
            return true;
        }
    }
}
