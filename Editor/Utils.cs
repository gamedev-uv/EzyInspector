using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UV.BetterInspector.Editors
{
    using UV.Utils;

    /// <summary>
    /// Contains helper methods 
    /// </summary>
    public static class Utils 
    {
        /// <summary>
        /// Whether the given member is serialized or not
        /// </summary>
        /// <param name="member">The member to be checked</param>
        /// <returns>Return true or false based on if it is serialized or not</returns>
        public static bool IsSerialized(this MemberInfo member)
        {
            return member.HasAttribute<SerializeField>() || member.HasAttribute<SerializeReference>() || member.IsPublic();
        }

        /// <summary>
        /// Whether the given member is public or not
        /// </summary>
        /// <param name="member">The member to be checked</param>
        /// <returns>Return true or false based on if it is public or not</returns>
        public static bool IsPublic(this MemberInfo member)
        {
            if (member is FieldInfo) return (member as FieldInfo).IsPublic;
            if (member is PropertyInfo) return (member as PropertyInfo).GetGetMethod() != null || (member as PropertyInfo).GetSetMethod() != null;
            if (member is MethodInfo) return (member as FieldInfo).IsPublic;
            return false;
        }

        /// <summary>
        /// Returns all the members which are to be serialized in the inspector
        /// </summary>
        /// <param name="obj">The object to be find the members in</param>
        /// <returns>Returns all the serialized members</returns>
        public static Dictionary<string, MemberInfo> GetSerializedMembers(this Object obj)
        {
            Dictionary<string, MemberInfo> serializedMembers = new();

            var members = obj.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.GetProperty);
            foreach (var member in members)
            {
                if (member.MemberType == MemberTypes.Method || serializedMembers.ContainsKey(member.Name)) continue;
                serializedMembers.Add(member.Name, member);
            }

            return serializedMembers;
        }
    }
}
