using UnityEngine;

namespace Helper
{
    public static class Calculator
    {
        public static bool IsNearObject(Vector3 source, Vector3 destination, float distance)
        {
            return SqrDistance(source, destination) < distance * distance;
        }

        public static float SqrDistance(Vector3 source, Vector3 destination)
        {
            var dx = source.x - destination.x;
            var dy = source.y - destination.y;
            var dz = source.z - destination.z;

            return dx * dx + dy * dy + dz * dz;
        }
    }
}