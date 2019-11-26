using System.Numerics;

namespace Domain
{
    public class Segment
    {
        public Vector2 point1;
        public Vector2 point2;
        public float length;

        public Segment(Vector2 p1, Vector2 p2, float len)
        {
            point1 = p1;
            point2 = p2;
            length = len;
        }
    }
}