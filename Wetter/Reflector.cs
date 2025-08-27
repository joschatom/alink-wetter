using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Wetter.Reflection
{
    // TOOD: Possibly use builtin Reflection directly?!?

    /// <summary>
    /// Interface that signals a type to be reflectable, e.g. it can be stepped into during reflection
    /// using Reflector.
    /// </summary>
    public interface IReflectable
    {
        public static bool IsReflectable(Type ty) => typeof(IReflectable).IsAssignableFrom(ty);
    }

    /// <summary>
    /// Used to mark a field or property to be ignored during reflection 
    /// when using Reflector.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property)]
    public sealed class ReflectIgnore : System.Attribute
    {
        public ReflectIgnore() { }
    };


    /// <summary>
    /// Allows for the recursive reflection of any Type
    /// that implements IReflectable with a given context T.
    /// </summary>
    /// <typeparam name="T">Context used in Reflection</typeparam>
    public static class Reflector<T>
    {
        public delegate void Callback(T context, (Type type, object? value) reflected, string name);

 
        public static void Reflect(IReflectable obj, T ctx, Callback cb)
        {
            foreach (FieldInfo field in obj.GetType().GetFields())
            {
                if (field.GetCustomAttribute<ReflectIgnore>() is not null) continue;


                cb(ctx, (field.FieldType, field.GetValue(obj)), field.Name); 

            }

            foreach (PropertyInfo prop in obj.GetType().GetProperties())
            {
                if (prop.GetCustomAttribute<ReflectIgnore>() is not null) continue;

              
                cb(ctx, (prop.PropertyType, prop.GetValue(obj)), prop.Name); 

            }
        }
    }

    public class ReflectableList<T>: List<T>, IReflectable
    {

    }
}
