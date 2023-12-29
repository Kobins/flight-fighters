
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
    public static Vector3 RotateAroundY(this ref Vector3 vector, float angle) {
        angle = angle * Mathf.Deg2Rad;
        float x = vector.x, z = vector.z;
        vector.x = x * Mathf.Cos(angle) - z * Mathf.Sin(angle);
        vector.z = x * Mathf.Sin(angle) + z * Mathf.Cos(angle);
        return vector;
    }

    public static Vector3 ProjectedXZPlane(this Vector3 vector) {
        vector.y = 0;
        return vector;
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
