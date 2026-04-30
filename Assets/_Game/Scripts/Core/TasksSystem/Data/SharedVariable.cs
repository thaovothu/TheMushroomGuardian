// using System.Numerics;
using UnityEngine;

public class SharedVariable<T>
{
    public string Name { get; set; }
    public T Value { get; set; }

    public SharedVariable()
    {
    }
    public SharedVariable(T value)
    {
        Value = value;
    }

    public static implicit operator SharedVariable<T>(T value)
    {
        return new SharedVariable<T>(value);
    }
}
    public class ShareBool : SharedVariable<bool>
    {
        public static implicit operator ShareBool(bool value)
        {
            return new ShareBool { Value = value };
        }
    }

    public class ShareFloat : SharedVariable<float>
    {
        public static implicit operator ShareFloat(float value)
        {
            return new ShareFloat { Value = value };
        }
    }

    public class ShareInt : SharedVariable<int>
    {
        public static implicit operator ShareInt(int value)
        {
            return new ShareInt { Value = value };
        }
    }

    public class ShareString : SharedVariable<string>
    {
        public static implicit operator ShareString(string value)
        {
            return new ShareString { Value = value };
        }
    }

    public class ShareVector3 : SharedVariable<Vector3>
    {
        public static implicit operator ShareVector3(Vector3 value)
        {
            return new ShareVector3 { Value = value };
        }
    }

    public class ShareVector2 : SharedVariable<Vector2>
    {
        public static implicit operator ShareVector2(Vector2 value)
        {
            return new ShareVector2 { Value = value };
        }
    }

    public class ShareTransform : SharedVariable<Transform>
    {
        public static implicit operator ShareTransform(Transform value)
        {
            return new ShareTransform { Value = value };
        }
    }

    public class ShareGameObject : SharedVariable<GameObject>
    {
        public static implicit operator ShareGameObject(GameObject value)
        {
            return new ShareGameObject { Value = value };
        }
    }

    public class ShareColor : SharedVariable<Color>
    {
        public static implicit operator ShareColor(Color value)
        {
            return new ShareColor { Value = value };
        }
    }

