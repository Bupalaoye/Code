using System;

namespace FlatPhysics
{
    public static class Collisions
    {
        /// <summary>
        /// 判读两个圆是否相交,normal 为 a->b
        /// </summary>
        /// <param name="centerA"></param>
        /// <param name="radiusA"></param>
        /// <param name="centerB"></param>
        /// <param name="radiusB"></param>
        /// <param name="normal"> 方向为 a->b </param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public static bool IntersectCircles(
            FlatVector centerA, float radiusA,
            FlatVector centerB, float radiusB,
            out FlatVector normal, out float depth)
        {
            normal = FlatVector.Zero;
            depth = 0;

            float distance = FlatMath.Distance(centerA, centerB);
            float radiusii = radiusA + radiusB;
            if (distance >= radiusii)
            {
                return false;
            }
            else if (distance == 0f)
            {
                // Same Center
                normal = new FlatVector(1f, 0);
                depth = radiusA + radiusB;
                return true;
            }

            normal = FlatMath.Normalize(centerB - centerA);
            depth = radiusii - distance;

            return true;
        }


        public static bool InsertAABB(FlatAABB aabbA, FlatAABB aabbB)
        {
            if (aabbA.Min.X > aabbB.Max.X || aabbA.Max.X < aabbB.Min.X
                || aabbA.Min.Y > aabbB.Max.Y || aabbA.Max.Y < aabbB.Min.Y)
            {
                return false;
            }
            return true;
        }
        public static bool IntersectPolygons(FlatVector centerA, FlatVector[] verticesA,
            FlatVector centerB, FlatVector[] verticesB, out FlatVector normal, out float depth)
        {
            normal = FlatVector.Zero;
            depth = float.MaxValue;

            // 以多边形a的法线 作轴边来判断是否相交
            for (int i = 0; i < verticesA.Length; i++)
            {
                FlatVector va = verticesA[i];
                FlatVector vb = verticesA[(i + 1) % verticesA.Length];

                FlatVector edge = vb - va;
                FlatVector axis = new FlatVector(-edge.Y, edge.X);

                Collisions.ProjectVertices(verticesA, axis, out float minA, out float maxA);
                Collisions.ProjectVertices(verticesB, axis, out float minB, out float maxB);

                if (minA >= maxB || minB >= maxA)
                {
                    // 不相交
                    return false;
                }


                float axisDepth = MathF.Min(maxB - minA, maxA - minB);
                if (axisDepth < depth)
                {
                    depth = axisDepth;
                    normal = axis;
                }
            }

            // 再以多边形b的边的法线作轴 来判断是否相交
            for (int i = 0; i < verticesB.Length; i++)
            {
                FlatVector va = verticesB[i];
                FlatVector vb = verticesB[(i + 1) % verticesB.Length];

                FlatVector edge = vb - va;
                FlatVector axis = new FlatVector(-edge.Y, edge.X);

                Collisions.ProjectVertices(verticesA, axis, out float minA, out float maxA);
                Collisions.ProjectVertices(verticesB, axis, out float minB, out float maxB);

                if (minA >= maxB || minB >= maxA)
                {
                    // 不相交
                    return false;
                }

                float axisDepth = MathF.Min(maxB - minA, maxA - minB);
                if (axisDepth < depth)
                {
                    depth = axisDepth;
                    normal = axis;
                }
            }

            depth /= FlatMath.Length(normal);
            normal = FlatMath.Normalize(normal);

            // a->b的向量
            FlatVector direction = centerB - centerA;
            if (FlatMath.Dot(direction, normal) < 0f)
            {
                normal = -normal;
            }

            // 两次都检测出来相交 那就是相交了
            return true;
        }


        public static bool IntersectCirclePolygon(FlatVector polygonCenter, FlatVector[] vertices,
            FlatVector circleCenter, float radius,
            out FlatVector normal, out float depth)
        {
            normal = FlatVector.Zero;
            depth = float.MaxValue;

            FlatVector axis = FlatVector.Zero;

            float minA, maxA, minB, maxB, axisDepth;

            // 以多边形a的法线 作轴边来判断是否相交
            for (int i = 0; i < vertices.Length; i++)
            {
                FlatVector va = vertices[i];
                FlatVector vb = vertices[(i + 1) % vertices.Length];

                FlatVector edge = vb - va;
                axis = new FlatVector(-edge.Y, edge.X);
                // 因为min,max的值会拿到for循环的外面一层来比较,所以要归一化,不然的话标准不一样,比较结果也没有意义了
                axis = FlatMath.Normalize(axis);

                Collisions.ProjectVertices(vertices, axis, out minA, out maxA);
                Collisions.ProjectCircle(circleCenter, radius, axis, out minB, out maxB);

                if (minA >= maxB || minB >= maxA)
                {
                    // 不相交
                    return false;
                }

                axisDepth = MathF.Min(maxB - minA, maxA - minB);
                if (axisDepth < depth)
                {
                    depth = axisDepth;
                    normal = axis;
                }
            }

            #region Circle Axis Detect
            int cpIndex = Collisions.FindClosestPointOnPolygon(circleCenter, vertices);
            FlatVector cp = vertices[cpIndex];

            axis = cp - circleCenter;
            axis = FlatMath.Normalize(axis);
            Collisions.ProjectVertices(vertices, axis, out minA, out maxA);
            Collisions.ProjectCircle(circleCenter, radius, axis, out minB, out maxB);

            if (minA >= maxB || minB >= maxA)
            {
                // 不相交
                return false;
            }

            axisDepth = MathF.Min(maxB - minA, maxA - minB);
            if (axisDepth < depth)
            {
                depth = axisDepth;
                normal = axis;
            }
            #endregion

            depth /= FlatMath.Length(normal);
            normal = FlatMath.Normalize(normal);

            // a->b的向量
            FlatVector direction = circleCenter - polygonCenter;
            if (FlatMath.Dot(direction, normal) < 0f)
            {
                normal = -normal;
            }

            // 两次都检测出来相交 那就是相交了
            return true;
        }
        private static int FindClosestPointOnPolygon(FlatVector circleCenter, FlatVector[] vertices)
        {
            int result = -1;
            float minDistance = float.MaxValue;

            for (int i = 0; i < vertices.Length; i++)
            {
                float distance = FlatMath.Distance(circleCenter, vertices[i]);
                if (minDistance > distance)
                {
                    minDistance = distance;
                    result = i;
                }
            }
            return result;
        }

        private static void ProjectVertices(FlatVector[] vertices, FlatVector axis, out float min, out float max)
        {
            min = float.MaxValue;
            max = float.MinValue;

            for (int i = 0; i < vertices.Length; i++)
            {
                FlatVector v = vertices[i];
                float proj = FlatMath.Dot(v, axis);

                if (proj < min) { min = proj; }
                if (proj > max) { max = proj; }
            }
        }


        private static void ProjectCircle(FlatVector center, float radius, FlatVector axis, out float min, out float max)
        {
            FlatVector direction = FlatMath.Normalize(axis);
            FlatVector directionAndRadius = direction * radius;
            min = FlatMath.Dot(center + directionAndRadius, axis);
            max = FlatMath.Dot(center - directionAndRadius, axis);

            if (min > max)
            {
                // swap the min and max values.
                float t = min;
                min = max;
                max = t;
            }
        }


        public static bool Collide(FlatBody bodyA, FlatBody bodyB, out FlatVector normal, out float depth)
        {
            normal = FlatVector.Zero;
            depth = 0f;

            ShapeType shapeTypeA = bodyA.shapeType;
            ShapeType shapeTypeB = bodyB.shapeType;

            if (shapeTypeA is ShapeType.Circle)
            {
                if (shapeTypeB is ShapeType.Circle)
                {
                    return Collisions.IntersectCircles(bodyA.Position, bodyA.Radius, bodyB.Position, bodyB.Radius,
                        out normal, out depth);
                }
                else if (shapeTypeB is ShapeType.Box)
                {
                    bool result = Collisions.IntersectCirclePolygon(bodyB.Position, bodyB.GetTransformedVertices(),
                        bodyA.Position, bodyA.Radius,
                        out normal, out depth);
                    // 因为得到的方向和参数有关 要注意方向!(不懂的话再多想想!)
                    normal = -normal;
                    return result;
                }
            }
            else if (shapeTypeA is ShapeType.Box)
            {
                if (shapeTypeB is ShapeType.Box)
                {
                    return Collisions.IntersectPolygons(bodyA.Position, bodyA.GetTransformedVertices(),
                        bodyB.Position, bodyB.GetTransformedVertices(),
                        out normal, out depth);
                }
                else if (shapeTypeB is ShapeType.Circle)
                {
                    return Collisions.IntersectCirclePolygon(bodyA.Position, bodyA.GetTransformedVertices(),
                        bodyB.Position, bodyB.Radius,
                        out normal, out depth);
                }
            }

            return false;
        }


        public static void FindContactPoints(
            FlatBody bodyA, FlatBody bodyB,
            out FlatVector contact1, out FlatVector contact2,
            out int contactCount)
        {

            contact1 = FlatVector.Zero;
            contact2 = FlatVector.Zero;
            contactCount = 0;


            ShapeType shapeTypeA = bodyA.shapeType;
            ShapeType shapeTypeB = bodyB.shapeType;

            if (shapeTypeA is ShapeType.Circle)
            {
                if (shapeTypeB is ShapeType.Circle)
                {
                    Collisions.FindCirclesContactPoint(bodyA.Position, bodyA.Radius, bodyB.Position, out contact1);
                    contactCount = 1;
                }
                else if (shapeTypeB is ShapeType.Box)
                {
                    Collisions.FindCirclePolygonContactPoint(bodyA.Position, bodyA.Radius, bodyB.Position,
                        bodyB.GetTransformedVertices(), out contact1);
                    contactCount = 1;
                }
            }
            else if (shapeTypeA is ShapeType.Box)
            {
                if (shapeTypeB is ShapeType.Box)
                {
                    Collisions.FindPolygonsContactPoints(bodyA.GetTransformedVertices(), bodyB.GetTransformedVertices(),
                        out contact1, out contact2, out contactCount);
                }
                else if (shapeTypeB is ShapeType.Circle)
                {
                    Collisions.FindCirclePolygonContactPoint(bodyB.Position, bodyB.Radius, bodyA.Position,
                        bodyA.GetTransformedVertices(), out contact1);
                    contactCount = 1;
                }
            }
        }

        public static void FindPolygonsContactPoints(FlatVector[] verticesA, FlatVector[] verticesB,
            out FlatVector contact1, out FlatVector contact2, out int contactCount)
        {
            contact1 = FlatVector.Zero;
            contact2 = FlatVector.Zero;
            contactCount = 0;

            float minDistSq = float.MaxValue;
            // 先遍历A的边
            for (int i = 0; i < verticesA.Length; i++)
            {
                FlatVector p = verticesA[i];

                for (int j = 0; j < verticesB.Length; j++)
                {
                    FlatVector va = verticesB[j];
                    FlatVector vb = verticesB[(j + 1) % verticesB.Length];

                    Collisions.PointSegmentDistance(p, va, vb, out float distSq, out FlatVector cp);

                    if (FlatMath.NearlyEqual(distSq, minDistSq))
                    {
                        // 找到两个距离相同的点
                        if (!FlatMath.NearlyEqual(cp, contact1))
                        {
                            contact2 = cp;
                            contactCount = 2;
                        }
                    }
                    else if (distSq < minDistSq)
                    {
                        // 只找到一个
                        minDistSq = distSq;
                        contactCount = 1;
                        contact1 = cp;
                    }
                }
            }

            // 再遍历b的边
            for (int i = 0; i < verticesB.Length; i++)
            {
                FlatVector p = verticesB[i];

                for (int j = 0; j < verticesA.Length; j++)
                {
                    FlatVector va = verticesA[j];
                    FlatVector vb = verticesA[(j + 1) % verticesA.Length];

                    Collisions.PointSegmentDistance(p, va, vb, out float distSq, out FlatVector cp);

                    if (FlatMath.NearlyEqual(distSq, minDistSq))
                    {
                        // 找到两个距离相同的点
                        if (!FlatMath.NearlyEqual(cp, contact1))
                        {
                            contact2 = cp;
                            contactCount = 2;
                        }
                    }
                    else if (distSq < minDistSq)
                    {
                        // 只找到一个
                        minDistSq = distSq;
                        contactCount = 1;
                        contact1 = cp;
                    }
                }
            }

        }

        private static void FindCirclesContactPoint(FlatVector CenterA, float RadiusA, FlatVector CenterB, out FlatVector cp)
        {
            FlatVector ab = CenterB - CenterA;
            FlatVector dir = FlatMath.Normalize(ab);
            cp = CenterA + dir * RadiusA;
        }


        public static void PointSegmentDistance(
            FlatVector p, FlatVector a, FlatVector b,
            out float distanceSquared, out FlatVector cp)
        {
            cp = FlatVector.Zero;

            FlatVector ab = b - a;
            FlatVector ap = p - a;

            float proj = FlatMath.Dot(ab, ap);
            float abLengthSq = FlatMath.LengthSquared(ab);

            // 这行代码需要理解一下 ab的模 是一样的可以除掉,然后就是ap到ab上面的投影到ab的长度的比值
            float d = proj / abLengthSq;
            // 注意这个是线段ab 所以 只能取[a,b] 的点
            if (d <= 0.0f)
            {
                cp = a;
            }
            else if (d >= 1.0f)
            {
                cp = b;
            }
            else
            {
                cp = a + d * ab;
            }
            distanceSquared = FlatMath.DistanceSquared(cp, p);
        }

        private static void FindCirclePolygonContactPoint(
            FlatVector circleCenter, float circleRaius,
            FlatVector polygonCenter, FlatVector[] polygonVertices,
            out FlatVector cp)
        {
            cp = FlatVector.Zero;
            float minDistSq = float.MaxValue;

            // 循环遍历每条边,再取最短点
            for (int i = 0; i < polygonVertices.Length; i++)
            {
                FlatVector va = polygonVertices[i];
                FlatVector vb = polygonVertices[(i + 1) % polygonVertices.Length];

                Collisions.PointSegmentDistance(circleCenter, va, vb, out float distSq, out FlatVector contact);
                if (minDistSq > distSq)
                {
                    minDistSq = distSq;
                    cp = contact;
                }
            }
        }

    }
}
