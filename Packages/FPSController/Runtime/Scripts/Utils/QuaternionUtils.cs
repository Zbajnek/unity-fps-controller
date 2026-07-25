using UnityEngine;

namespace Utils
{
    public static class QuaternionUtils
    {
        public static Quaternion SmoothDamp(Quaternion current, Quaternion target, ref Quaternion deriv, float smoothTime)
        {
            if (Time.deltaTime < Mathf.Epsilon) return current;

            var dot = Quaternion.Dot(current, target);
            var multi = dot > 0f ? 1f : -1f;
            target.x *= multi;
            target.y *= multi;
            target.z *= multi;
            target.w *= multi;
            
            var result = new Vector4(
                Mathf.SmoothDamp(current.x, target.x, ref deriv.x, smoothTime),
                Mathf.SmoothDamp(current.y, target.y, ref deriv.y, smoothTime),
                Mathf.SmoothDamp(current.z, target.z, ref deriv.z, smoothTime),
                Mathf.SmoothDamp(current.w, target.w, ref deriv.w, smoothTime)
            ).normalized;
            
            var derivError = Vector4.Project(new Vector4(deriv.x, deriv.y, deriv.z, deriv.w), result);
            deriv.x -= derivError.x;
            deriv.y -= derivError.y;
            deriv.z -= derivError.z;
            deriv.w -= derivError.w;
            
            return new Quaternion(result.x, result.y, result.z, result.w);
        }
    }
}