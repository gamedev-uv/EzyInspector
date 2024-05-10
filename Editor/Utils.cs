using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UV.BetterInspector.Editors
{
    using System.Linq;
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
            if (member is FieldInfo field) return field.IsPublic;
            if (member is PropertyInfo property) return property.GetGetMethod() != null || property.GetSetMethod() != null;
            if (member is MethodInfo method) return method.IsPublic;
            return false;
        }

        /// <summary>
        /// Returns all the members which are to be serialized on the inspector
        /// </summary>
        /// <param name="obj">The object to be find the members in</param>
        /// <returns>Returns all the serialized members</returns>
        public static Dictionary<string, MemberInfo> GetSerializedMembers(this Object obj)
        {
            Dictionary<string, MemberInfo> serializedMembers = new();

            var members = obj.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.GetProperty);
            foreach (var member in members)
            {
                if (!member.IsSerialized() || member.MemberType == MemberTypes.Method || serializedMembers.ContainsKey(member.Name)) continue;
                serializedMembers.Add(member.Name, member);
            }

            return serializedMembers;
        }

        /// <summary>
        /// Returns all the foldoutMember groups which are to be drawn on the inspector
        /// </summary>
        /// <param name="obj">The object to be find the members in</param>
        /// <returns>Returns all the foldoutMembers</returns>
        public static Dictionary<string, MemberInfo[]> GetFoldoutsGroups(this Object obj)
        {
            //Fetch all members which have the foldout attribute on them
            var foldoutMembers = obj.GetMembersWithAttribute<FoldoutAttribute>();
            if (foldoutMembers == null || foldoutMembers.Count == 0) return null;

            var foldoutGroups = new Dictionary<string, MemberInfo[]>();
            foreach (var foldoutMember in foldoutMembers)
            {
                var foldout = foldoutMember.Value;
                var foldoutName = foldout.DisplayName;

                //Add a new foldout group if one does't exist with the same name
                if (!foldoutGroups.ContainsKey(foldoutName))
                {
                    foldoutGroups.Add(foldoutName, new MemberInfo[] { foldoutMember.Key });
                    continue;
                }

                //Add the member to the already existing group
                var members = foldoutGroups[foldoutName];
                members ??= new MemberInfo[0];
                members = members.Append(foldoutMember.Key).ToArray();
                foldoutGroups[foldoutName] = members;
            }

            return foldoutGroups;
        }
    }
}