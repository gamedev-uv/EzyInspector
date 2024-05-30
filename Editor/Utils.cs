using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UV.EzyInspector.Editors
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
            return member.HasAttribute<SerializeField>() || member.HasAttribute<SerializeReference>() || IsPublic(member);
        }

        /// <summary>
        /// Whether the given member is public or not
        /// </summary>
        /// <param name="member">The member to be checked</param>
        /// <returns>Return true or false based on if it is public or not</returns>
        public static bool IsPublic(this MemberInfo member)
        {
            if (member is FieldInfo field) return field.IsPublic;
            if (member is MethodInfo method) return method.IsPublic;

            if (member is PropertyInfo property)
                return (property.GetGetMethod(true) ?? property.GetSetMethod(true)).IsPublic;

            return false;
        }

        /// <summary>
        /// Returns all the members which are to be serialized on the inspector
        /// </summary>
        /// <param name="obj">The object to be find the members in</param>
        /// <returns>Returns all the serialized members</returns>
        public static (MemberInfo, object, string)[] GetSerializedMembers(this Object obj)
        {
            List<(MemberInfo, object, string)> serializedMembers = new();

            object currentObj = null;
            var members = obj.GetMembers(false);

            foreach (var (member, memberObj, path) in members)
            {
                if (!member.IsSerialized()) continue;
                if (currentObj != memberObj)
                    currentObj = memberObj;

                serializedMembers.Add((member, memberObj, path));
            }

            return serializedMembers.ToArray();
        }

        //public static (MemberInfo, object, string)[] GetSerializedMembers(this Object obj)
        //{
        //    List<(MemberInfo, object, string)> serializedMembers = new();

        //    var members = obj.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.GetProperty);
        //    foreach (var member in members)
        //    {
        //        if (!member.IsSerialized() || member.MemberType == MemberTypes.Method) continue;
        //        serializedMembers.Add((member, null, member.Name));
        //    }

        //    return serializedMembers.ToArray();
        //}

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
