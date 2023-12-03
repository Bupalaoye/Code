using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Flat1.Physics
{
    public enum WindingOrder
    {
        Clockwise, CounterClockwise, Invalid
    };

    public static class PolygonHelper
    {
        // 简单的凹凸边形的运算
        public static float FindPolygonArea(Vector2[] vertices)
        {
            float totalArea = 0f;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 a = vertices[i];
                int j = i + 1;
                // 这样做避免 % 运算 : j = (i+1) % vertices.Length
                if (j >= vertices.Length)
                    j = 0;

                Vector2 b = vertices[j];
                float dy = (a.Y + b.Y) * 0.5f;
                float dx = b.X - a.X;

                float area = dy * dx;
                totalArea += area;
            }
            return MathF.Abs(totalArea);
        }

        /// <summary>
        /// 切分多边形 to 三角形
        /// </summary>
        /// <param name="vertices">多边形的顶点</param>
        /// <param name="triangles">三角形的index 数组</param>
        /// <param name="errorMessage">错误信息(如果error)</param>
        /// <returns></returns>
        public static bool Triangulate(Vector2[] vertices, out int[] triangles, out string errorMessage)
        {
            triangles = null;
            errorMessage = string.Empty;

            if (vertices is null)
            {
                errorMessage = " The vertices is empty";
                return false;
            }

            if (vertices.Length < 3)
            {
                errorMessage = "The vertices must have at least 3 vertices.";
                return false;
            }

            if (vertices.Length > 1024)
            {
                errorMessage = "The max vertices length is 1024";
                return false;
            }

            //if (!PolygonHelper.IsSimplePolygon(vertices))
            //{
            //    errorMessage = "the vertex list does not define a simple polygon.";
            //    return false;
            //}

            //if (PolygonHelper.ContainsColinearEdges(vertices))
            //{
            //    errorMessage = "the vertex list contains colinear edges.";
            //    return false;
            //}

            //PolygonHelper.ComputePolygonArea(vertices, out float area, out WindingOrder windingOrder);
            //if (windingOrder is WindingOrder.Invalid)
            //{
            //    errorMessage = "the vertex list does not contains a valid polygon";
            //    return false;
            //}

            //if (windingOrder is WindingOrder.CounterClockwise)
            //{
            //    Array.Reverse(vertices);
            //}

            List<int> indexList = new List<int>();
            for (int i = 0; i < vertices.Length; i++)
            {
                indexList.Add(i);
            }

            int totalTriangleCount = vertices.Length - 2;
            int totalTriangleIndexCount = totalTriangleCount * 3;
            triangles = new int[totalTriangleIndexCount];

            int triangleIndexCount = 0;

            while (indexList.Count > 3)
            {
                for (int i = 0; i < indexList.Count; i++)
                {
                    int a = indexList[i];
                    int b = Utils.GetItem(indexList, i - 1);
                    int c = Utils.GetItem(indexList, i + 1);

                    Vector2 va = vertices[a];
                    Vector2 vb = vertices[b];
                    Vector2 vc = vertices[c];

                    Vector2 va_to_vb = vb - va;
                    Vector2 va_to_vc = vc - va;

                    // Is ear test vertex convex? 是否是顶点凸
                    if (Utils.Cross(va_to_vb, va_to_vc) < 0f)
                    {
                        // 如果是钝角则马上跳过当前的点
                        continue;
                    }

                    bool IsEar = true;

                    // does test ear contain any polygon vertices?
                    for (int j = 0; j < vertices.Length; j++)
                    {
                        // 判断其他的点在不在 三角形abc 里面
                        if (j == a || j == b || j == c)
                        {
                            continue;
                        }

                        Vector2 p = vertices[j];

                        if (PolygonHelper.IsPointInTriangle(p, vb, va, vc))
                        {
                            // 如果找到了有其他点在三角形里面 则跳出循环,找另一个三角形
                            IsEar = false;
                            break;
                        }
                    }

                    // 如果通过了 ear的测试
                    if (IsEar)
                    {
                        triangles[triangleIndexCount++] = b;
                        triangles[triangleIndexCount++] = a;
                        triangles[triangleIndexCount++] = c;

                        // 当前点(即a点) 要除去 然后在找下一个三角形 如此循环
                        indexList.RemoveAt(i);
                        break;
                    }
                }
            }

            // 跳出循环 即 index == 3 则将这'最后一个'三角形放进去
            triangles[triangleIndexCount++] = indexList[0];
            triangles[triangleIndexCount++] = indexList[1];
            triangles[triangleIndexCount++] = indexList[2];

            return true;
        }

        public static bool IsSimplePolygon(Vector2[] vertices)
        {
            throw new NotImplementedException();
        }

        // 是否出现3个点共线的情况
        public static bool ContainsColinearEdges(Vector2[] vertices)
        {
            throw new NotImplementedException();
        }
        public static void ComputePolygonArea(Vector2[] vertices, out float area, out WindingOrder windingOrder)
        {
            throw new NotImplementedException();
        }

        public static bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            Vector2 ab = b - a;
            Vector2 bc = c - b;
            Vector2 ca = a - c;

            Vector2 ap = p - a;
            Vector2 bp = p - b;
            Vector2 cp = p - c;

            float cross1 = Utils.Cross(ab, ap);
            float cross2 = Utils.Cross(bc, bp);
            float cross3 = Utils.Cross(ca, cp);

            if (cross1 > 0f || cross2 > 0f || cross3 > 0f)
            {
                return false;
            }
            return true;
        }
    }
}
