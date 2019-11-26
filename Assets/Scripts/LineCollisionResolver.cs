using System;
using System.Collections;
using System.Collections.Generic;
using Domain;
using References;
using UnityEngine;

public class LineCollisionResolver : MonoBehaviour
{
    public float yOffset = 20;
    public float xOffset = 20;
    
    private IDictionary<string, LineHolder> _lines;

    private IDictionary<Vector2, string> _pointsToNodes;

    private IList<Vector2> _points;

    public void SetLines(IDictionary<string, LineHolder> lines)
    {
        _pointsToNodes = new Dictionary<Vector2, string>();
        _points = new List<Vector2>();
        _lines = lines;
        foreach (var line in _lines)
        {
            foreach (var pt in line.Value.GetPositions())
            {
                if (_points.Contains(pt))
                {
                    continue;
                }

                _points.Add(pt);
            }
        }
    }

    public void Resolve()
    {
        foreach (var line in _lines)
        {
            var linePoints = line.Value.GetPositions();
            var point1 = linePoints[0];
            var point2 = linePoints[1];

            var slope = Math.Abs(point1.x - point2.x) < 0.1f ? 0 : (point2.y - point1.y) / (point2.x - point1.x);
            var intercept = -(slope * point1.x) + point1.y;
            foreach (var pt in _points)
            {
                // Don't care about collisions on either end
                if (IsEqual(pt, point1) || IsEqual(pt, point2))
                {
                    continue;
                }
                
                var y = slope * pt.x + intercept;
                if ((Math.Abs(pt.y - y) < 0.1f || Math.Abs(slope) <= 0f) && IsBetween(pt, point1, point2))
                {
                    Debug.Log($"Found collision for node {line.Key} at ({pt.x}, {pt.y}): {GetCollisionType(point1, point2)}");
                    line.Value.SetPositionCount(3);
                    line.Value.SetPositions(new []
                    {
                        point1, 
                        GetCollisionResolverPoint(GetCollisionType(point1, point2), pt, point1, point2), 
                        point2
                    });
                }
            }
        }
    }

    private Vector3 GetCollisionResolverPoint(Directions direction, Vector2 collidedPoint, Vector2 point1, Vector2 point2)
    {
        var resolverPoint = new Vector3();
        resolverPoint.z = 90;
        switch (direction)
        {
            case Directions.Horizontal:
                resolverPoint.x = collidedPoint.x;
                resolverPoint.y = collidedPoint.y - yOffset;
                break;
            case Directions.Vertical:
                resolverPoint.x = collidedPoint.x + xOffset;
                resolverPoint.y = collidedPoint.y;
                break;
            case Directions.LeftTilt:
                resolverPoint.x = collidedPoint.x + xOffset / 1.5f;
                resolverPoint.y = collidedPoint.y + yOffset / 1.5f;
                break;
            case Directions.RightTilt:
                resolverPoint.x = collidedPoint.x + xOffset / 1.5f;
                resolverPoint.y = collidedPoint.y - yOffset / 1.5f;
                break;
            default:
                resolverPoint.x = point1.x;
                resolverPoint.y = point1.y;
                break;
        }

        return resolverPoint;
    }

    private static Directions GetCollisionType(Vector2 point1, Vector2 point2)
    {
        var y = point2.y - point1.y;
        var x = point2.x - point1.x;

        if (Math.Abs(y) < 0.1f)
        {
            return Directions.Horizontal;
        }

        if (Math.Abs(x) < 0.1f)
        {
            return Directions.Vertical;
        }

        if (y < 0)
        {
            return Directions.LeftTilt;
        }

        if (y > 0)
        {
            return Directions.RightTilt;
        }

        return Directions.Invalid;
    }

    private bool IsBetween(Vector2 point, Vector2 point1, Vector2 point2)
    {
        var smallerX = Math.Min(point1.x, point2.x);
        var biggerX = Math.Max(point1.x, point2.x);

        var smallerY = Math.Min(point1.y, point2.y);
        var biggerY = Math.Max(point1.y, point2.y);

        var x = point.x;
        var y = point.y;

        return x >= smallerX && x <= biggerX && y >= smallerY && y <= biggerY;
    }

    private bool IsEqual(Vector2 point1, Vector2 point2)
    {
        return Math.Abs(point1.x - point2.x) < 0.1f && Math.Abs(point1.y - point2.y) < 0.1f;
    } 
}
