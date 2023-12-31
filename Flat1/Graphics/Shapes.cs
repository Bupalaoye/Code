﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;


namespace Flat1.Graphics
{
    public sealed class Shapes : IDisposable
    {
        private Game game;
        private bool isDisposed;
        private BasicEffect effect;
        private Camera camera;

        private VertexPositionColor[] vertices;
        private int[] indices;

        private int shapeCount;
        private int vertexCount;
        private int indexCount;

        private bool isStarted;
        public static readonly float MinLineThickness = 1f;
        public static readonly float MaxLineThickness = 10f;

        public Shapes(Game game)
        {
            this.game = game ?? throw new ArgumentNullException("game");

            this.isDisposed = false;
            this.effect = new BasicEffect(this.game.GraphicsDevice);
            this.effect.VertexColorEnabled = true;
            this.effect.TextureEnabled = false;
            this.effect.FogEnabled = false;
            this.effect.LightingEnabled = false;
            this.effect.World = Matrix.Identity;
            this.effect.View = Matrix.Identity;
            this.effect.Projection = Matrix.Identity;


            const int MaxVertexCount = 1024;
            const int MaxIndexCount = MaxVertexCount * 3;

            this.vertices = new VertexPositionColor[MaxVertexCount];
            this.indices = new int[MaxIndexCount];

            this.shapeCount = 0;
            this.vertexCount = 0;
            this.indexCount = 0;

            this.isStarted = false;

            this.camera = null;
        }

        // 用于GC垃圾回收使用
        public void Dispose()
        {
            if (this.isDisposed)
                return;

            this.effect?.Dispose();
            this.isDisposed = true;
        }

        public void Begin(Camera camera)
        {
            if (this.isStarted)
            {
                throw new Exception("batching is already started.");
            }

            if (camera is null)
            {
                Viewport vp = this.game.GraphicsDevice.Viewport;
                this.effect.Projection = Matrix.CreateOrthographicOffCenter(0, vp.Width, 0, vp.Height, 0f, 1f);
                this.effect.View = Matrix.Identity;
            }
            else
            {
                camera.UpdateMatrices();
                this.effect.View = camera.View;
                this.effect.Projection = camera.Projection;
            }

            this.camera = camera;

            this.isStarted = true;
        }

        public void End()
        {
            EnsureStarted();
            this.Flush();
            this.isStarted = false;
        }

        public void Flush()
        {
            if (this.shapeCount == 0)
            {
                return;
            }
            EnsureStarted();

            foreach (EffectPass pass in this.effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                this.game.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                    PrimitiveType.TriangleList,
                    this.vertices,
                    0,
                    this.vertexCount,
                    this.indices,
                    0,
                    this.indexCount / 3);
            }


            this.shapeCount = 0;
            this.vertexCount = 0;
            this.indexCount = 0;
        }

        public void EnsureStarted()
        {
            if (!this.isStarted)
            {
                throw new Exception("batching is never started.");
            }
        }

        public void EnsureSpace(int shapeVertexCount, int shapeIndexCount)
        {
            // 如果它超出了最大容量 肯定也是不行的
            if (shapeVertexCount > this.vertices.Length)
            {
                throw new Exception("Maximum shape vertex count is : " + this.vertices.Length);
            }

            if (shapeIndexCount > this.indices.Length)
            {
                throw new Exception("Maximum shape indices count is : " + this.indices.Length);
            }


            if (this.vertexCount + shapeVertexCount > this.vertices.Length ||
                this.indexCount + shapeIndexCount > this.indices.Length)
            {
                Flush();
            }
        }

        public void DrawRectangleFill(float x, float y, float width, float height, Color color)
        {
            this.EnsureStarted();

            const int shapeVertexCount = 4;
            const int shapeIndexCount = 6;

            this.EnsureSpace(shapeVertexCount, shapeIndexCount);

            float left = x;
            float right = x + width;
            float bottom = y;
            float top = y + height;


            Vector2 a = new Vector2(left, top);
            Vector2 b = new Vector2(right, top);
            Vector2 c = new Vector2(right, bottom);
            Vector2 d = new Vector2(left, bottom);

            //  + this.vertexCount 是为了确保顶点取的是对的
            this.indices[this.indexCount++] = 0 + this.vertexCount;
            this.indices[this.indexCount++] = 1 + this.vertexCount;
            this.indices[this.indexCount++] = 2 + this.vertexCount;
            this.indices[this.indexCount++] = 0 + this.vertexCount;
            this.indices[this.indexCount++] = 2 + this.vertexCount;
            this.indices[this.indexCount++] = 3 + this.vertexCount;

            this.vertices[this.vertexCount++] = new VertexPositionColor(new Vector3(a, 0f), color);
            this.vertices[this.vertexCount++] = new VertexPositionColor(new Vector3(b, 0f), color);
            this.vertices[this.vertexCount++] = new VertexPositionColor(new Vector3(c, 0f), color);
            this.vertices[this.vertexCount++] = new VertexPositionColor(new Vector3(d, 0f), color);

            this.shapeCount++;
        }


        public void DrawLineSlow(Vector2 a, Vector2 b, float thickness, Color color)
        {
            this.EnsureStarted();

            const int shapeVectexCount = 4;
            const int shapeIndexCount = 6;

            this.EnsureSpace(shapeVectexCount, shapeIndexCount);

            thickness = Utils.Clamp(thickness, Shapes.MinLineThickness, Shapes.MaxLineThickness);
            thickness++;    // 确保thickness最少为2
            float halfThickness = thickness / 2f;

            Vector2 e1 = b - a;
            e1.Normalize();
            e1 *= halfThickness;
            Vector2 e2 = -e1;

            // 利用dot点乘为0 得到发现
            Vector2 n1 = new Vector2(-e1.Y, e1.X);
            Vector2 n2 = -n1;

            Vector2 q1 = a + n1 + e2;
            Vector2 q2 = b + n1 + e1;
            Vector2 q3 = b + n2 + e1;
            Vector2 q4 = a + n2 + e2;

            //  + this.vertexCount 是为了确保顶点取的是对的
            this.indices[this.indexCount++] = 0 + this.vertexCount;
            this.indices[this.indexCount++] = 1 + this.vertexCount;
            this.indices[this.indexCount++] = 2 + this.vertexCount;
            this.indices[this.indexCount++] = 0 + this.vertexCount;
            this.indices[this.indexCount++] = 2 + this.vertexCount;
            this.indices[this.indexCount++] = 3 + this.vertexCount;

            this.vertices[this.vertexCount++] = new VertexPositionColor(new Vector3(q1, 0f), color);
            this.vertices[this.vertexCount++] = new VertexPositionColor(new Vector3(q2, 0f), color);
            this.vertices[this.vertexCount++] = new VertexPositionColor(new Vector3(q3, 0f), color);
            this.vertices[this.vertexCount++] = new VertexPositionColor(new Vector3(q4, 0f), color);

            this.shapeCount++;
        }

        public void DrawLine(Vector2 a, Vector2 b, float thickness, Color color)
        {
            this.DrawLine(a.X, a.Y, b.X, b.Y, thickness, color);
        }

        public void DrawLine(Vector2 a, Vector2 b, Color color)
        {
            this.DrawLine(a.X, a.Y, b.X, b.Y, 1f, color);
        }
        public void DrawLine(float ax, float ay, float bx, float by, Color color)
        {
            this.DrawLine(ax, ay, bx, by, 1f, color);
        }

        public void DrawLine(float ax, float ay, float bx, float by, float thickness, Color color)
        {
            this.EnsureStarted();

            const int shapeVectexCount = 4;
            const int shapeIndexCount = 6;

            this.EnsureSpace(shapeVectexCount, shapeIndexCount);

            thickness = Utils.Clamp(thickness, Shapes.MinLineThickness, Shapes.MaxLineThickness);
            thickness++;    // 确保thickness最少为2


            // 当前相机的z 与 基础z 位置的比率 , 我们希望该线看起来具有相同的厚度
            // 这两者的关系最好还是用数学公式计算一下 对比一下!
            // 等价于 / camrea.zoom
            if (this.camera != null)
            {
                // 这意味者我们在camera 空间绘画line
                thickness *= (this.camera.Z / this.camera.BaseZ);
            }

            float halfThickness = thickness / 2f;

            float e1x = bx - ax;
            float e1y = by - ay;
            Utils.Normalize(ref e1x, ref e1y);
            e1x *= halfThickness;
            e1y *= halfThickness;

            // 因为e1x 和 e1y 都是normalize后的 !注意要归一化
            float e2x = -e1x;
            float e2y = -e1y;

            // 利用dot点乘为0 得到发现
            float n1x = -e1y;
            float n1y = e1x;

            float n2x = e1y;
            float n2y = -e1x;


            float q1x = ax + n1x + e2x;
            float q1y = ay + n1y + e2y;

            float q2x = bx + n1x + e1x;
            float q2y = by + n1y + e1y;

            float q3x = bx + n2x + e1x;
            float q3y = by + n2y + e1y;

            float q4x = ax + n2x + e2x;
            float q4y = ay + n2y + e2y;

            Vector2 q1 = new Vector2(q1x, q1y);
            Vector2 q2 = new Vector2(q2x, q2y);
            Vector2 q3 = new Vector2(q3x, q3y);
            Vector2 q4 = new Vector2(q4x, q4y);

            Vector2 e1 = new Vector2(e1x, e1y);
            Vector2 e2 = new Vector2(e2x, e2y);

            //  + this.vertexCount 是为了确保顶点取的是对的
            this.indices[this.indexCount++] = 0 + this.vertexCount;
            this.indices[this.indexCount++] = 1 + this.vertexCount;
            this.indices[this.indexCount++] = 2 + this.vertexCount;
            this.indices[this.indexCount++] = 0 + this.vertexCount;
            this.indices[this.indexCount++] = 2 + this.vertexCount;
            this.indices[this.indexCount++] = 3 + this.vertexCount;

            this.vertices[this.vertexCount++] = new VertexPositionColor(new Vector3(q1x, q1y, 0f), color);
            this.vertices[this.vertexCount++] = new VertexPositionColor(new Vector3(q2x, q2y, 0f), color);
            this.vertices[this.vertexCount++] = new VertexPositionColor(new Vector3(q3x, q3y, 0f), color);
            this.vertices[this.vertexCount++] = new VertexPositionColor(new Vector3(q4x, q4y, 0f), color);

            this.shapeCount++;
        }

        public void DrawRectangle(float x, float y, float width, float height, float thickness, Color color)
        {
            float left = x;
            float right = x + width;
            float bottom = y;
            float top = y + height;

            this.DrawLine(left, top, right, top, thickness, color);
            this.DrawLine(right, top, right, bottom, thickness, color);
            this.DrawLine(right, bottom, left, bottom, thickness, color);
            this.DrawLine(left, bottom, left, top, thickness, color);
        }


        public void DrawCircleSlow(float x, float y, float radius, int points, float thickness, Color color)
        {
            const int minPoints = 3;
            const int maxPoints = 256;

            points = MathHelper.Clamp(points, minPoints, maxPoints);

            float deltaAngle = MathHelper.TwoPi / (float)points;
            float angle = 0f;   // 开始的角度

            for (int i = 0; i < points; i++)
            {
                float ax = MathF.Sin(angle) * radius + x;
                float ay = MathF.Cos(angle) * radius + y;

                angle += deltaAngle;

                float bx = MathF.Sin(angle) * radius + x;
                float by = MathF.Cos(angle) * radius + y;

                this.DrawLine(ax, ay, bx, by, thickness, color);
            }
        }

        public void DrawCircle(Vector2 center, float radius, int points, float thickness, Color color)
        {
            this.DrawCircle(center.X, center.Y, radius, points, thickness, color);
        }

        public void DrawCircle(float x, float y, float radius, int points, float thickness, Color color)
        {
            const int minPoints = 3;
            const int maxPoints = 256;

            points = MathHelper.Clamp(points, minPoints, maxPoints);

            float rotation = MathHelper.TwoPi / (float)points;
            // 然后用数学知识 避免了sin 和 cos 的重复调用 '耗时'
            float sin = MathF.Sin(rotation);
            float cos = MathF.Cos(rotation);

            // 这里代表的是起点
            float ax = radius;
            float ay = 0;
            // 声明
            float bx = 0;
            float by = 0;

            for (int i = 0; i < points; i++)
            {
                /*
                 * x2 = cosβx1−sinβy1
                 * y2 = sinβx1+cosβy1
                 */

                bx = cos * ax - sin * ay;
                by = sin * ax + cos * ay;
                this.DrawLine(ax + x, ay + y, bx + x, by + y, thickness, color);

                // 这样就避免一个点坐标算两次的情况
                ax = bx;
                ay = by;
            }
        }

        public void DrawCircleFill(Vector2 center, float radius, int points, Color color)
        {
            this.DrawCircleFill(center.X, center.Y, radius, points, color);
        }
        public void DrawCircleFill(float x, float y, float radius, int points, Color color)
        {
            this.EnsureStarted();

            const int minPoints = 3;
            const int maxPoints = 256;

            int shapeVertexCount = Utils.Clamp(points, minPoints, maxPoints);
            // 'Triangle number : ' 其实是吧多边形以某点为'原点' ,和其他两个点组成三角形的数量 (不是圆心!)
            int shapeTriangleCount = shapeVertexCount - 2;
            int shapeIndexCount = shapeTriangleCount * 3;

            this.EnsureSpace(shapeVertexCount, shapeIndexCount);

            // 索引数组
            int index = 1;

            for (int i = 0; i < shapeTriangleCount; i++)
            {
                this.indices[this.indexCount++] = 0 + this.vertexCount;
                this.indices[this.indexCount++] = index + this.vertexCount;
                this.indices[this.indexCount++] = index + 1 + this.vertexCount;

                index++;
            }

            float rotation = MathHelper.TwoPi / (float)points;

            float sin = MathF.Sin(rotation);
            float cos = MathF.Cos(rotation);

            float ax = radius;
            float ay = 0;

            // 顶点数组
            for (int i = 0; i < shapeVertexCount; i++)
            {
                float x1 = ax;
                float y1 = ay;

                this.vertices[this.vertexCount++] = new VertexPositionColor(new Vector3(x1 + x, y1 + y, 0f), color);

                ax = cos * x1 - sin * y1;
                ay = sin * x1 + cos * y1;
            }

            this.shapeCount++;
        }

        public void DrawPolygon(Vector2[] vertices, FlatTransform transform, float thickness, Color color)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 a = vertices[i];
                // 注意不要越界就行
                int j = i + 1;
                if (j > vertices.Length - 1)
                    j = 0;
                Vector2 b = vertices[j];

                a = Utils.Transform(a, transform);
                b = Utils.Transform(b, transform);

                this.DrawLine(a, b, thickness, color);
            }
        }

        public void DrawPolygon(Vector2[] vertices, float thickness, Color color)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 a = vertices[i];
                // 注意不要越界就行
                int j = i + 1;
                if (j > vertices.Length - 1)
                    j = 0;
                Vector2 b = vertices[j];
                this.DrawLine(a, b, thickness, color);
            }
        }


        public void DrawPolygonFill(Vector2[] vertices, int[] triangleIndices, FlatTransform transform, Color color)
        {
#if DEBUG
            if (vertices is null || triangleIndices is null)
            {
                throw new ArgumentNullException("vertices or triangleIndices");
            }
            if (vertices.Length == 3 || triangleIndices.Length < 3)
            {
                throw new ArgumentOutOfRangeException("vertices or triangleIndices");
            }
#endif

            this.EnsureStarted();
            this.EnsureSpace(vertices.Length, triangleIndices.Length);

            for (int i = 0; i < triangleIndices.Length; i++)
            {
                this.indices[this.indexCount++] = triangleIndices[i] + this.vertexCount;
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 vertex = vertices[i];
                vertex = Utils.Transform(vertex, transform);
                this.vertices[this.vertexCount++] = new VertexPositionColor(new Vector3(vertex.X, vertex.Y, 0f), color);
            }

            this.shapeCount++;
        }

        public void DrawBoxFill(float x, float y, float width, float height, Color[] colors)
        {
            Vector2 min = new Vector2(x, y);
            Vector2 max = new Vector2(x + width, y + height);

            this.DrawBoxFill(min, max, colors);
        }
        public void DrawBoxFill(Vector2 center, float width, float height, float angle, Color color)
        {
            this.DrawBoxFill(center,width,height,angle,Vector2.One,color);
        }
        public void DrawBoxFill(Vector2 min,Vector2 max, float angle, Color color)
        {
            this.EnsureStarted();

            int shapeVertexCount = 4;
            int shapeIndexCount = 6;

            this.EnsureSpace(shapeVertexCount, shapeIndexCount);

            Utils.Transform(min, new FlatTransform(Vector2.Zero, angle, Vector2.One));
            Utils.Transform(max, new FlatTransform(Vector2.Zero, angle, Vector2.One));

            Vector3 a = new Vector3(min.X, max.Y, 0f);
            Vector3 b = new Vector3(max.X, max.Y, 0f);
            Vector3 c = new Vector3(max.X, min.Y, 0f);
            Vector3 d = new Vector3(min.X, min.Y, 0f);

            this.indices[this.indexCount++] = 0 + this.vertexCount;
            this.indices[this.indexCount++] = 1 + this.vertexCount;
            this.indices[this.indexCount++] = 2 + this.vertexCount;
            this.indices[this.indexCount++] = 0 + this.vertexCount;
            this.indices[this.indexCount++] = 2 + this.vertexCount;
            this.indices[this.indexCount++] = 3 + this.vertexCount;

            this.vertices[this.vertexCount++] = new VertexPositionColor(a, color);
            this.vertices[this.vertexCount++] = new VertexPositionColor(b, color);
            this.vertices[this.vertexCount++] = new VertexPositionColor(c, color);
            this.vertices[this.vertexCount++] = new VertexPositionColor(d, color);

            this.shapeCount++;
        }
        public void DrawBoxFill(Vector2 center, float width, float height, float rotation, Vector2 scale, Color color)
        {
            this.EnsureStarted();

            int shapeVertexCount = 4;
            int shapeIndexCount = 6;

            this.EnsureSpace(shapeVertexCount, shapeIndexCount);

            float left = -width * 0.5f;
            float right = left + width;
            float bottom = -height * 0.5f;
            float top = bottom + height;

            // Precompute the trig. functions.
            float sin = MathF.Sin(rotation);
            float cos = MathF.Cos(rotation);

            // Vector components:

            float ax = left;
            float ay = top;
            float bx = right;
            float by = top;
            float cx = right;
            float cy = bottom;
            float dx = left;
            float dy = bottom;

            // Scale transform:

            float sx1 = ax * scale.X;
            float sy1 = ay * scale.Y;
            float sx2 = bx * scale.X;
            float sy2 = by * scale.Y;
            float sx3 = cx * scale.X;
            float sy3 = cy * scale.Y;
            float sx4 = dx * scale.X;
            float sy4 = dy * scale.Y;

            // Rotation transform:

            float rx1 = sx1 * cos - sy1 * sin;
            float ry1 = sx1 * sin + sy1 * cos;
            float rx2 = sx2 * cos - sy2 * sin;
            float ry2 = sx2 * sin + sy2 * cos;
            float rx3 = sx3 * cos - sy3 * sin;
            float ry3 = sx3 * sin + sy3 * cos;
            float rx4 = sx4 * cos - sy4 * sin;
            float ry4 = sx4 * sin + sy4 * cos;

            // Translation transform:

            ax = rx1 + center.X;
            ay = ry1 + center.Y;
            bx = rx2 + center.X;
            by = ry2 + center.Y;
            cx = rx3 + center.X;
            cy = ry3 + center.Y;
            dx = rx4 + center.X;
            dy = ry4 + center.Y;

            this.indices[this.indexCount++] = 0 + this.vertexCount;
            this.indices[this.indexCount++] = 1 + this.vertexCount;
            this.indices[this.indexCount++] = 2 + this.vertexCount;
            this.indices[this.indexCount++] = 0 + this.vertexCount;
            this.indices[this.indexCount++] = 2 + this.vertexCount;
            this.indices[this.indexCount++] = 3 + this.vertexCount;

            this.vertices[this.vertexCount++] = new VertexPositionColor(new Vector3(ax, ay, 0f), color);
            this.vertices[this.vertexCount++] = new VertexPositionColor(new Vector3(bx, by, 0f), color);
            this.vertices[this.vertexCount++] = new VertexPositionColor(new Vector3(cx, cy, 0f), color);
            this.vertices[this.vertexCount++] = new VertexPositionColor(new Vector3(dx, dy, 0f), color);

            this.shapeCount++;
        }

        public void DrawBoxFill(Vector2 min, Vector2 max, Color[] colors)
        {
            if (colors is null || colors.Length != 4)
            {
                throw new ArgumentOutOfRangeException("colors array must have exactly 4 items.");
            }

            this.EnsureStarted();

            int shapeVertexCount = 4;
            int shapeIndexCount = 6;

            this.EnsureSpace(shapeVertexCount, shapeIndexCount);

            Vector3 a = new Vector3(min.X, max.Y, 0f);
            Vector3 b = new Vector3(max.X, max.Y, 0f);
            Vector3 c = new Vector3(max.X, min.Y, 0f);
            Vector3 d = new Vector3(min.X, min.Y, 0f);

            this.indices[this.indexCount++] = 0 + this.vertexCount;
            this.indices[this.indexCount++] = 1 + this.vertexCount;
            this.indices[this.indexCount++] = 2 + this.vertexCount;
            this.indices[this.indexCount++] = 0 + this.vertexCount;
            this.indices[this.indexCount++] = 2 + this.vertexCount;
            this.indices[this.indexCount++] = 3 + this.vertexCount;

            this.vertices[this.vertexCount++] = new VertexPositionColor(a, colors[0]);
            this.vertices[this.vertexCount++] = new VertexPositionColor(b, colors[1]);
            this.vertices[this.vertexCount++] = new VertexPositionColor(c, colors[2]);
            this.vertices[this.vertexCount++] = new VertexPositionColor(d, colors[3]);

            this.shapeCount++;
        }

        public void DrawBoxFill(Vector2 min, Vector2 max, Color color)
        {
            this.EnsureStarted();

            int shapeVertexCount = 4;
            int shapeIndexCount = 6;

            this.EnsureSpace(shapeVertexCount, shapeIndexCount);

            Vector3 a = new Vector3(min.X, max.Y, 0f);
            Vector3 b = new Vector3(max.X, max.Y, 0f);
            Vector3 c = new Vector3(max.X, min.Y, 0f);
            Vector3 d = new Vector3(min.X, min.Y, 0f);

            this.indices[this.indexCount++] = 0 + this.vertexCount;
            this.indices[this.indexCount++] = 1 + this.vertexCount;
            this.indices[this.indexCount++] = 2 + this.vertexCount;
            this.indices[this.indexCount++] = 0 + this.vertexCount;
            this.indices[this.indexCount++] = 2 + this.vertexCount;
            this.indices[this.indexCount++] = 3 + this.vertexCount;

            this.vertices[this.vertexCount++] = new VertexPositionColor(a, color);
            this.vertices[this.vertexCount++] = new VertexPositionColor(b, color);
            this.vertices[this.vertexCount++] = new VertexPositionColor(c, color);
            this.vertices[this.vertexCount++] = new VertexPositionColor(d, color);

            this.shapeCount++;
        }

        public void DrawPolygonFill(Vector2[] vertices, int[] triangleIndices, Color color)
        {
#if DEBUG
            if (vertices is null || triangleIndices is null)
            {
                throw new ArgumentNullException("vertices or triangleIndices");
            }
            if (vertices.Length == 3 || triangleIndices.Length < 3)
            {
                throw new ArgumentOutOfRangeException("vertices or triangleIndices");
            }
#endif
            this.EnsureStarted();
            this.EnsureSpace(vertices.Length, triangleIndices.Length);

            for (int i = 0; i < triangleIndices.Length; i++)
            {
                this.indices[this.indexCount++] = triangleIndices[i] + this.vertexCount;
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 vertex = vertices[i];
                this.vertices[this.vertexCount++] = new VertexPositionColor(new Vector3(vertex.X, vertex.Y, 0f), color);
            }

            this.shapeCount++;
        }


        public void DrawPolygonTriangles(Vector2[] vecties, int[] triangles, FlatTransform transform, Color color)
        {
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int a = triangles[i];
                int b = triangles[i + 1];
                int c = triangles[i + 2];

                Vector2 va = vecties[a];
                Vector2 vb = vecties[b];
                Vector2 vc = vecties[c];

                va = Utils.Transform(va, transform);
                vb = Utils.Transform(vb, transform);
                vc = Utils.Transform(vc, transform);

                this.DrawLine(va, vb, 1f, color);
                this.DrawLine(vb, vc, 1f, color);
                this.DrawLine(vc, va, 1f, color);
            }
        }

        public void DrawBox(Vector2 min, Vector2 max, Color color)
        {
            this.DrawLine(min.X, max.Y, max.X, max.Y, color);
            this.DrawLine(max.X, max.Y, max.X, min.Y, color);
            this.DrawLine(max.X, min.Y, min.X, min.Y, color);
            this.DrawLine(min.X, min.Y, min.X, max.Y, color);
        }

        public void DrawBox(float x, float y, float width, float height, Color color)
        {
            Vector2 min = new Vector2(x, y);
            Vector2 max = new Vector2(x + width, y + height);

            this.DrawBox(min, max, color);
        }

        public void DrawBox(Vector2 center, float width, float height, Color color)
        {
            Vector2 min = new Vector2(center.X - width * 0.5f, center.Y - height * 0.5f);
            Vector2 max = new Vector2(min.X + width, min.Y + height);

            this.DrawBox(min, max, color);
        }

        public void DrawBox(Vector2 center, float width, float height, float angle, Color color)
        {
            float left = -width * 0.5f;
            float right = left + width;
            float bottom = -height * 0.5f;
            float top = bottom + height;

            // Precompute the trig. functions.
            float sin = MathF.Sin(angle);
            float cos = MathF.Cos(angle);

            // Vector components:

            float ax = left;
            float ay = top;
            float bx = right;
            float by = top;
            float cx = right;
            float cy = bottom;
            float dx = left;
            float dy = bottom;

            // Rotation transform:

            float rx1 = ax * cos - ay * sin;
            float ry1 = ax * sin + ay * cos;
            float rx2 = bx * cos - by * sin;
            float ry2 = bx * sin + by * cos;
            float rx3 = cx * cos - cy * sin;
            float ry3 = cx * sin + cy * cos;
            float rx4 = dx * cos - dy * sin;
            float ry4 = dx * sin + dy * cos;

            // Translation transform:

            ax = rx1 + center.X;
            ay = ry1 + center.Y;
            bx = rx2 + center.X;
            by = ry2 + center.Y;
            cx = rx3 + center.X;
            cy = ry3 + center.Y;
            dx = rx4 + center.X;
            dy = ry4 + center.Y;
            this.DrawLine(ax, ay, bx, by, color);
            this.DrawLine(bx, by, cx, cy, color);
            this.DrawLine(cx, cy, dx, dy, color);
            this.DrawLine(dx, dy, ax, ay, color);
        }

    }
}
