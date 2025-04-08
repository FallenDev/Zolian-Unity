namespace Assets.Scripts.Extensions
{
    public static class Vector3Extensions
    {
        /// <summary>
        /// Converts UnityEngine.Vector3 to System.Numerics.Vector3
        /// </summary>
        public static System.Numerics.Vector3 ToNumerics(this UnityEngine.Vector3 v)
        {
            return new System.Numerics.Vector3(v.x, v.y, v.z);
        }

        /// <summary>
        /// Converts System.Numerics.Vector3 to UnityEngine.Vector3
        /// </summary>
        public static UnityEngine.Vector3 ToUnity(this System.Numerics.Vector3 v)
        {
            return new UnityEngine.Vector3(v.X, v.Y, v.Z);
        }
    }
}
