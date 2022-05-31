using RasterLib.LinearMath;
using System.Collections.Generic;

namespace RasterLib.Core;

public static class Clipping
{
    /// <summary>
    ///     Stores the result of clipping a triangle with a plane.
    ///     Could contain 0, 1 or 2 triangles.
    /// </summary>
    public struct ClippingResult<VertexData> where VertexData : IInterpolatable<VertexData>
    {
        public int NumTriangles { get; private set; }
        public Triangle<VertexData>? Triangle1 { get; private set; }
        public Triangle<VertexData>? Triangle2 { get; private set; }

        public ClippingResult()
        {
            NumTriangles = 0;
            Triangle1 = null;
            Triangle2 = null;
        }

        public ClippingResult(Triangle<VertexData> t)
        {
            NumTriangles = 1;
            Triangle1 = t;
            Triangle2 = null;
        }

        public ClippingResult(Triangle<VertexData> t1, Triangle<VertexData> t2)
        {
            NumTriangles = 2;
            Triangle1 = t1;
            Triangle2 = t2;
        }

        public delegate void MapCallback(Triangle<VertexData> triangle);
        
        /// <summary>
        ///     Perform a function over the resulting triangles
        /// </summary>
        public void Map(MapCallback cb)
        {
            if (NumTriangles > 0) cb(Triangle1.Value);
            if (NumTriangles > 1) cb(Triangle2.Value);
        }
    };

    /// <summary>
    ///     Calculates the factor a component is infront of a plane in homogenous coordinates. 
    /// </summary>
    public static float CalcAlpha(float xa, float xb, float wa, float wb)
    {
        return (wb - xb) / (xa - wa + (wb - xb));
    }

    /// <summary>
    ///     Clip a triangle which described using homogenous coordinates against one of the viewport planes
    /// </summary>
    public static ClippingResult<VertexData> ClipHomogenous<VertexData>(Triangle<VertexData> tri, int compIndex, bool left) where VertexData : IInterpolatable<VertexData>
    {
        List<int> InsidePoints = new();
        List<int> OutsidePoints = new();

        float sign = left ? -1.0f : 1.0f;

        // Check which vertices are infront or behind the plane
        if ((tri.VertexA.Position[compIndex] > (sign * tri.VertexA.Position.w)) ^ !left)
        {
            InsidePoints.Add(0);
        }
        else
        {
            OutsidePoints.Add(0);
        }

        if ((tri.VertexB.Position[compIndex] > (sign * tri.VertexB.Position.w)) ^ !left)
        {
            InsidePoints.Add(1);
        }
        else
        {
            OutsidePoints.Add(1);
        }

        if ((tri.VertexC.Position[compIndex] > (sign * tri.VertexC.Position.w)) ^ !left)
        {
            InsidePoints.Add(2);
        }
        else
        {
            OutsidePoints.Add(2);
        }

        if (InsidePoints.Count == 1)
        {
            // Clip the two outside points so they are now inside the plane 
            var InsidePoint0 = tri[InsidePoints[0]];
            var OutsidePoint0 = tri[OutsidePoints[0]];
            var OutsidePoint1 = tri[OutsidePoints[1]];

            float t1 = CalcAlpha(InsidePoint0.Position[compIndex], OutsidePoint0.Position[compIndex],
                                 sign * InsidePoint0.Position.w, sign * OutsidePoint0.Position.w);
            Vector4 other1 = Vector4.Lerp(OutsidePoint0.Position, InsidePoint0.Position, t1);

            float t2 = CalcAlpha(InsidePoint0.Position[compIndex], OutsidePoint1.Position[compIndex],
                                 sign * InsidePoint0.Position.w, sign * OutsidePoint1.Position.w);
            Vector4 other2 = Vector4.Lerp(OutsidePoint1.Position, InsidePoint0.Position, t2);

            return new ClippingResult<VertexData>(new Triangle<VertexData>(
                (InsidePoint0.Position, InsidePoint0.Attribs),
                (other1, OutsidePoint0.Attribs.Interpolate(InsidePoint0.Attribs, t1)),
                (other2, OutsidePoint1.Attribs.Interpolate(InsidePoint0.Attribs, t2))
            ));
        }
        else if (InsidePoints.Count == 2)
        {
            // Clip the two inside points with the outside point to create to new points that are inside the triangle
            // Use these points to output two new triangles which are inside the plane.
            var InsidePoint0 = tri[InsidePoints[0]];
            var InsidePoint1 = tri[InsidePoints[1]];
            var OutsidePoint0 = tri[OutsidePoints[0]];

            float t1 = CalcAlpha(InsidePoint0.Position[compIndex], OutsidePoint0.Position[compIndex],
                                 sign * InsidePoint0.Position.w, sign * OutsidePoint0.Position.w);
            Vector4 other1 = Vector4.Lerp(OutsidePoint0.Position, InsidePoint0.Position, t1);

            float t2 = CalcAlpha(InsidePoint1.Position[compIndex], OutsidePoint0.Position[compIndex],
                                 sign * InsidePoint1.Position.w, sign * OutsidePoint0.Position.w);
            Vector4 other2 = Vector4.Lerp(OutsidePoint0.Position, InsidePoint1.Position, t2);

            return new ClippingResult<VertexData>(
                new Triangle<VertexData>(
                    (InsidePoint0.Position, InsidePoint0.Attribs),
                    (InsidePoint1.Position, InsidePoint1.Attribs),
                    (other1, OutsidePoint0.Attribs.Interpolate(InsidePoint0.Attribs, t1))),
                new Triangle<VertexData>(
                    (InsidePoint1.Position, InsidePoint1.Attribs),
                    (other1, OutsidePoint0.Attribs.Interpolate(InsidePoint0.Attribs, t1)),
                    (other2, OutsidePoint0.Attribs.Interpolate(InsidePoint1.Attribs, t2)))
            );
        }
        else if (InsidePoints.Count == 3)
        {
            return new ClippingResult<VertexData>(tri);
        }

        return new ClippingResult<VertexData>();
    }

    /// <summary>
    ///     Clip a triangle against all 6 viewport planes and apply a function to the resulting triangles
    /// </summary>
    public static void ClipHomogenousAllMap<VertexData>(Triangle<VertexData> tri, ClippingResult<VertexData>.MapCallback cb) where VertexData : IInterpolatable<VertexData>
    {
        ClipHomogenous(tri, 0, false).Map(
            tri2 => ClipHomogenous(tri2, 0, true).Map(
                tri3 => ClipHomogenous(tri3, 1, false).Map(
                    tri4 => ClipHomogenous(tri4, 1, true).Map(
                        tri5 => ClipHomogenous(tri5, 2, false).Map(
                            tri6 => ClipHomogenous(tri6, 2, true).Map(cb))))));
    }

    public static bool InViewport(Vector4 v)
    {
        return (v.x > -v.w) && (v.x < v.w) && (v.y > -v.w) && (v.y < v.w) && (v.z > -v.w) && (v.z < v.w);
    }

    /// <summary>
    ///     Clip a line in homogenous coordinates against all 6 viewport planes
    /// </summary>
    public static (Vector4, Vector4)? ClipLine(Vector4 start, Vector4 end)
    {
        bool startInside = InViewport(start);
        bool endInside = InViewport(end);

        if (!startInside && !endInside)
        {
            return null;
        }
        else if (!startInside && endInside)
        {
            // clip start
            if (start.x < -start.w)
            {
                float alpha1 = CalcAlpha(start.x, end.x, -start.w, -end.w);
                start = Vector4.Lerp(end, start, alpha1);
            }

            if (start.y < -start.w)
            {
                float alpha2 = CalcAlpha(start.y, end.y, -start.w, -end.w);
                start = Vector4.Lerp(end, start, alpha2);
            }


            if (start.z < -start.w)
            {
                float alpha3 = CalcAlpha(start.z, end.z, -start.w, -end.w);
                start = Vector4.Lerp(end, start, alpha3);
            }


            if (start.x > start.w)
            {
                float alpha1 = CalcAlpha(start.x, end.x, start.w, end.w);
                start = Vector4.Lerp(end, start, alpha1);
            }

            if (start.y > start.w)
            {
                float alpha2 = CalcAlpha(start.y, end.y, start.w, end.w);
                start = Vector4.Lerp(end, start, alpha2);
            }


            if (start.z > start.w)
            {
                float alpha3 = CalcAlpha(start.z, end.z, start.w, end.w);
                start = Vector4.Lerp(end, start, alpha3);
            }
        }
        else if (startInside && !endInside)
        {
            // clip end
            if (end.x < -end.w)
            {
                float alpha1 = CalcAlpha(end.x, start.x, -end.w, -start.w);
                end = Vector4.Lerp(start, end, alpha1);
            }

            if (end.y < -end.w)
            {
                float alpha2 = CalcAlpha(end.y, start.y, -end.w, -start.w);
                end = Vector4.Lerp(start, end, alpha2);
            }

            if (end.z < -end.w)
            {
                float alpha3 = CalcAlpha(end.z, start.z, -end.w, -start.w);
                end = Vector4.Lerp(start, end, alpha3);
            }


            if (end.x > end.w)
            {
                float alpha1 = CalcAlpha(end.x, start.x, end.w, start.w);
                end = Vector4.Lerp(start, end, alpha1);
            }

            if (end.y > end.w)
            {
                float alpha2 = CalcAlpha(end.y, start.y, end.w, start.w);
                end = Vector4.Lerp(start, end, alpha2);
            }

            if (end.z > end.w)
            {
                float alpha3 = CalcAlpha(end.z, start.z, end.w, start.w);
                end = Vector4.Lerp(start, end, alpha3);
            }
        }

        return (start, end);
    }

    public static ClippingResult<VertexData> Clip<VertexData>(Triangle<VertexData> tri, Plane plane) where VertexData : IInterpolatable<VertexData>
    {
        float aDist = plane.DistanceToPoint(tri.VertexA.Position.xyz);
        float bDist = plane.DistanceToPoint(tri.VertexB.Position.xyz);
        float cDist = plane.DistanceToPoint(tri.VertexC.Position.xyz);

        List<int> InsidePoints = new();
        List<int> OutsidePoints = new();

        if (aDist >= 0)
        {
            InsidePoints.Add(0);
        }
        else
        {
            OutsidePoints.Add(0);
        }

        if (bDist >= 0)
        {
            InsidePoints.Add(1);
        }
        else
        {
            OutsidePoints.Add(1);
        }

        if (cDist >= 0)
        {
            InsidePoints.Add(2);
        }
        else
        {
            OutsidePoints.Add(2);
        }

        if (InsidePoints.Count == 1)
        {
            var InsidePoint0 = tri[InsidePoints[0]];
            var OutsidePoint0 = tri[OutsidePoints[0]];
            var OutsidePoint1 = tri[OutsidePoints[1]];

            (Vector3 other1, float t1) = plane.GetPointOnPlaneFromLine(InsidePoint0.Position.xyz, OutsidePoint0.Position.xyz);
            (Vector3 other2, float t2) = plane.GetPointOnPlaneFromLine(InsidePoint0.Position.xyz, OutsidePoint1.Position.xyz);

            return new ClippingResult<VertexData>(new Triangle<VertexData>(
                (InsidePoint0.Position, InsidePoint0.Attribs),
                (new Vector4(other1, MathUtil.Lerp(InsidePoint0.Position.w, OutsidePoint0.Position.w, t1)), InsidePoint0.Attribs.Interpolate(OutsidePoint0.Attribs, t1)),
                (new Vector4(other2, MathUtil.Lerp(InsidePoint0.Position.w, OutsidePoint1.Position.w, t2)), InsidePoint0.Attribs.Interpolate(OutsidePoint1.Attribs, t2))
            ));
        }
        else if (InsidePoints.Count == 2)
        {
            var InsidePoint0 = tri[InsidePoints[0]];
            var InsidePoint1 = tri[InsidePoints[1]];
            var OutsidePoint0 = tri[OutsidePoints[0]];

            (Vector3 other1, float t1) = plane.GetPointOnPlaneFromLine(InsidePoint0.Position.xyz, OutsidePoint0.Position.xyz);
            (Vector3 other2, float t2) = plane.GetPointOnPlaneFromLine(InsidePoint1.Position.xyz, OutsidePoint0.Position.xyz);

            return new ClippingResult<VertexData>(
                new Triangle<VertexData>(
                    (InsidePoint0.Position, InsidePoint0.Attribs),
                    (InsidePoint1.Position, InsidePoint1.Attribs),
                    (new Vector4(other1, MathUtil.Lerp(InsidePoint0.Position.w, OutsidePoint0.Position.w, t1)), InsidePoint0.Attribs.Interpolate(OutsidePoint0.Attribs, t1))),
                new Triangle<VertexData>(
                    (InsidePoint1.Position, InsidePoint1.Attribs),
                    (new Vector4(other1, MathUtil.Lerp(InsidePoint0.Position.w, OutsidePoint0.Position.w, t1)), InsidePoint0.Attribs.Interpolate(OutsidePoint0.Attribs, t1)),
                    (new Vector4(other2, MathUtil.Lerp(InsidePoint1.Position.w, OutsidePoint0.Position.w, t2)), InsidePoint1.Attribs.Interpolate(OutsidePoint0.Attribs, t2)))
            );
        }
        else if (InsidePoints.Count == 3)
        {
            return new ClippingResult<VertexData>(tri);
        }

        return new ClippingResult<VertexData>();
    }

    private static void ClipAndMap_Recursive<VertexData>(Triangle<VertexData> triangle, Plane[] planes, int numPlanes, ClippingResult<VertexData>.MapCallback cb) where VertexData : IInterpolatable<VertexData>
    {
        if (numPlanes == 0)
        {
            cb(triangle);
        }
        else
        {
            Clip(triangle, planes[numPlanes - 1]).Map(
                tri => ClipAndMap_Recursive(tri, planes, numPlanes - 1, cb));
        }
    }

    public static void ClipAndMap<VertexData>(Triangle<VertexData> triangle, Plane[] planes, ClippingResult<VertexData>.MapCallback cb) where VertexData : IInterpolatable<VertexData>
    {
        ClipAndMap_Recursive(triangle, planes, planes.Length, cb);
    }
}
