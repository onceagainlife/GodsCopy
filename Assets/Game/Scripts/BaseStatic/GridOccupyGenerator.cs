namespace BackPackLike.Editor
{
    using System.Collections.Generic;
    using UnityEngine;

    public static class GridOccupyGenerator
    {
        public static List<GridOffsetData> Generate(
            PolygonCollider2D collider,
            float cellSize)
        {
            List<GridOffsetData> result = new();

            Bounds b = collider.bounds;

            int minX = Mathf.FloorToInt(b.min.x / cellSize);
            int maxX = Mathf.FloorToInt(b.max.x / cellSize);

            int minY = Mathf.FloorToInt(b.min.y / cellSize);
            int maxY = Mathf.FloorToInt(b.max.y / cellSize);

            Vector2[] poly = GetWorldPoints(collider);

            Vector2 pivot = collider.transform.position;

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    Rect cellRect = new Rect(
                        x * cellSize,
                        y * cellSize,
                        cellSize,
                        cellSize
                    );

                    if (PolygonIntersectsRect(
                            poly,
                            cellRect))
                    {
                        Vector2 center =
                            cellRect.center;

                        result.Add(
                            new GridOffsetData
                            {
                                grid =
                                new Vector2Int(x, y),

                                localOffset =
                                center - pivot
                            });
                    }
                }
            }

            return result;
        }


        static Vector2[] GetWorldPoints(
            PolygonCollider2D col)
        {
            Vector2[] points = col.points;

            for (int i = 0; i < points.Length; i++)
            {
                points[i] =
                    col.transform.TransformPoint(
                        points[i]);
            }

            return points;
        }

        static bool PolygonIntersectsRect(
        Vector2[] poly,
        Rect rect)
        {
            Vector2[] rectPts =
            {
        rect.min,
        new(rect.xMax,rect.yMin),
        rect.max,
        new(rect.xMin,rect.yMax)
            };

            //1 多边形点在矩形内
            foreach (var p in poly)
            {
                if (rect.Contains(p))
                    return true;
            }

            //2 矩形点在多边形内
            foreach (var p in rectPts)
            {
                if (IsPointInPolygon(
                    p,
                    poly))
                    return true;
            }

            //3 边相交
            for (int i = 0; i < poly.Length; i++)
            {
                Vector2 a =
                    poly[i];

                Vector2 b =
                    poly[(i + 1) % poly.Length];

                for (int j = 0; j < 4; j++)
                {
                    Vector2 c =
                        rectPts[j];

                    Vector2 d =
                        rectPts[(j + 1) % 4];

                    if (LineCross(
                        a, b, c, d))
                        return true;
                }
            }

            return false;
        }
        static bool IsPointInPolygon(
    Vector2 p,
    Vector2[] poly)
        {
            bool inside = false;

            for (int i = 0, j = poly.Length - 1;
                i < poly.Length;
                j = i++)
            {
                if (((poly[i].y > p.y)
                != (poly[j].y > p.y))
                &&
                (p.x <
                (poly[j].x - poly[i].x)
                * (p.y - poly[i].y)
                / (poly[j].y - poly[i].y)
                + poly[i].x))
                {
                    inside = !inside;
                }
            }

            return inside;
        }
        static bool LineCross(
Vector2 a,
Vector2 b,
Vector2 c,
Vector2 d)
        {
            return
            CCW(a, c, d) != CCW(b, c, d)
            &&
            CCW(a, b, c) != CCW(a, b, d);
        }

        static bool CCW(
        Vector2 a,
        Vector2 b,
        Vector2 c)
        {
            return
            (c.y - a.y) * (b.x - a.x) >
            (b.y - a.y) * (c.x - a.x);
        }
    }

    [System.Serializable]
    public struct GridOffsetData
    {
        public Vector2Int grid;
        public Vector2 localOffset;
    }
}
