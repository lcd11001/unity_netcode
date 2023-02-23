using Demo;
using System;
using System.Reflection;

public static class EnumExtension
{
    public static T GetAttribute<T>(this Enum value)
        where T : Attribute
    {
        // Get the type
        Type type = value.GetType();

        // Get fieldinfo for this type
        FieldInfo fieldInfo = type.GetField(value.ToString());

        //// Get the stringvalue attributes
        //var attribs = fieldInfo.GetCustomAttributes(typeof(T), false);

        //// Return the first if there was a match.
        //return attribs.Length > 0 ? (T)attribs[0] : null;
        return fieldInfo.GetCustomAttribute<T>(false);
    }
    /// <summary>
    /// Will get the string value for a given enums value, this will
    /// only work if you assign the StringValue attribute to
    /// the items in your enum.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string GetStringValue(this Enum value)
    {
        var attribute = value.GetAttribute<StringValueAttribute>();
        return attribute == null ? value.ToString() : attribute.StringValue;
    }

    /// <summary>
    /// Will convert string value to enum
    /// </summary>
    /// <typeparam name="T">destination Enum type</typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static T ToEnum<T>(this string value)
        where T : Enum
    {
        var type = typeof(T);
        foreach (var field in type.GetFields())
        {
            var attribute = field.GetCustomAttribute<StringValueAttribute>(false);
            //var attribute = Attribute.GetCustomAttribute(field, typeof(StringValueAttribute), false) as StringValueAttribute;
            if (attribute != null)
            {
                if (attribute.StringValue == value)
                {
                    return (T)field.GetValue(null);
                }
            }
            else
            {
                if (field.Name == value)
                {
                    return (T)field.GetValue(null);
                }
            }
        }
        throw new ArgumentException($"unknown value: {value}");
    }
}
