
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class TimeManager {
    public static float timeScale = 1f;
    public static float deltaTime { get { return Time.deltaTime * timeScale; } }
}

public abstract class Enumeration : IComparable {
    public string Name { get; private set; }

    public int Id { get; private set; }

    protected Enumeration(int id, string name) {
        Id = id;
        Name = name;
    }

    public override int GetHashCode() => Id;

    public override string ToString() => Name;

    public static IEnumerable<T> GetAll<T>() where T : Enumeration {
        var fields = typeof(T).GetFields(BindingFlags.Public |
                                         BindingFlags.Static |
                                         BindingFlags.DeclaredOnly);

        return fields.Select(f => f.GetValue(null)).Cast<T>();
    }

    public override bool Equals(object obj) {

        if (!(obj is Enumeration otherValue))
            return false;

        var typeMatches = GetType().Equals(obj.GetType());
        var valueMatches = Id.Equals(otherValue.Id);

        return typeMatches && valueMatches;
    }

    public int CompareTo(object other) => Id.CompareTo(((Enumeration)other).Id);

}

public static class VectorUtil {
    public static Vector3 RotateAroundY(this ref Vector3 vector, float angleInDegree) {
        var angleInRadian = angleInDegree * Mathf.Deg2Rad;
        float x = vector.x, z = vector.z;
        float cos = Mathf.Cos(angleInRadian);
        float sin = Mathf.Sin(angleInRadian);
        vector.x = x * cos - z * sin;
        vector.z = x * sin + z * cos;
        return vector;
    }

    public static Vector3 RotateAroundZ(this ref Vector3 vector, float angleInDegree) {
        var angleInRadian = angleInDegree * Mathf.Deg2Rad;
        float x = vector.x;
        float y = vector.y;
        float cos = Mathf.Cos(angleInRadian);
        float sin = Mathf.Sin(angleInRadian);

        vector.x = x * cos - y * sin;
        vector.y = x * sin + y * cos;
        return vector;
    }

    public static Vector3 ProjectedXZPlane(this Vector3 vector) {
        vector.y = 0;
        return vector;
    }
    public static Vector3 ProjectedXYPlane(this Vector3 vector) {
        vector.z = 0;
        return vector;
    }
    public static Vector3 ProjectedYZPlane(this Vector3 vector) {
        vector.x = 0;
        return vector;
    }

    public static float DirectionToYawInDegrees(this Vector3 forward) {
        var xzForward = new Vector3(forward.x, 0, forward.z); xzForward.Normalize();
        //yaw == xz평면 북쪽 벡터와 xz평면 방향벡터 사이의 각 -> -180 ~ 180 사이의 값
        return Vector3.SignedAngle(Vector3.forward, xzForward, Vector3.up);
    }

    public static float Dot(this Vector3 vec, Vector3 other) {
        return Vector3.Dot(vec, other);
    }

    public static Vector3 MultiplyEach(this Vector3 vec, Vector3 other) {
        return new Vector3(vec.x * other.x, vec.y * other.y, vec.z * other.z);
    }

    public static float Distance(this Vector3 vec, Vector3 other) {
        return Vector3.Distance(vec, other);
    }
    public static float DistanceSquared(this Vector3 vec, Vector3 other) {
        return (other - vec).sqrMagnitude;
    }

}

public static class ColorUtil {
    const float epsilon = 0.001f;
    public static bool IsSimilar(this Color color, Color other) {
        return Mathf.Abs(color.r - other.r) <= epsilon
            && Mathf.Abs(color.g - other.g) <= epsilon
            && Mathf.Abs(color.b - other.b) <= epsilon
            && Mathf.Abs(color.a - other.a) <= epsilon;
    }

}

public static class VRUtil {
    public static bool IsTriggerPressed => // 디버그중에는 anyKeyDown, 실행환경에서는 카드보드 API 사용
#if UNITY_EDITOR
        Input.anyKeyDown;
#else
            Google.XR.Cardboard.Api.IsTriggerPressed;
#endif
    
    public static bool IsTriggerPressing {
        get {
#if UNITY_EDITOR
            return Input.anyKey;
#else
            if(Input.touchCount <= 0) return false;
            Touch touch = Input.GetTouch(0);
            return touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved;
#endif 
        }
    }
}

public static class Util {
    public static T GetOrAddComponent<T>(this GameObject from) where T : Component {
        var component = from.GetComponent<T>();
        return component ? component : from.AddComponent<T>();
    }
    public static T GetOrAddComponent<T>(this Component from) where T : Component {
        var component = from.GetComponent<T>();
        return component ? component : from.gameObject.AddComponent<T>();
    }
}