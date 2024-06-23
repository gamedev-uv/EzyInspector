using System;
using System.Linq;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace UV.EzyInspector.Editors
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
            return HasSerializableAttributes(member) || IsPublic(member);
        }

        /// <summary>
        /// Whether the given member has serializable attributes or not i.e. SerializeField or SerializeReference
        /// </summary>
        /// <param name="member">The member to be checked</param>
        /// <returns>Return true or false based on if it i has serializable attributes or not</returns>
        public static bool HasSerializableAttributes(this MemberInfo member)
        {
            return member.HasAttribute<SerializeField>() || member.HasAttribute<SerializeReference>();
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
        public static (MemberInfo, object, string, Attribute[])[] GetSerializedMembers(this Object obj)
        {
            List<(MemberInfo, object, string, Attribute[])> serializedMembers = new();
            var members = obj.GetMembers(true);

            foreach (var (member, memberObj, path, attributes) in members)
            {
                if (member == null) continue;
                if (!member.IsSerialized()) continue;
                serializedMembers.Add((member, memberObj, path, attributes));
            }

            return serializedMembers.ToArray();
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

        /// <summary>
        /// Checks whether the given member has the attribute on it and returns the attribute if found
        /// </summary>
        /// <typeparam name="T">The type of attribute to find</typeparam>
        /// <param name="memberTuple">The member tuple in which the attribute is to be found</param>
        /// <param name="attribute">The attribute to be returned if found</param>
        /// <returns>Returns true or false based on if the attribute was found or not</returns>
        public static bool TryGetAttribute<T>(this (MemberInfo, object, string, Attribute[]) memberTuple, out T attribute) where T : Attribute
        {
            //Access all the attributes of the member
            attribute = null;
            var attributes = memberTuple.Item4;
            if (attributes == null || attributes.Length == 0) return false;

            //Try to find the typed attribute 
            T foundAttribute = null;
            for (int i = 0; i < attributes.Length; i++)
            {
                foundAttribute = attributes[i] as T;
                if (foundAttribute != null) break;
            }

            //If the attribute was found return true else false
            attribute = foundAttribute ?? null;
            return attribute != null;
        }

        /// <summary>
        /// Checks whether the given member has the attribute on it 
        /// </summary>
        /// <typeparam name="T">The type of attribute to find</typeparam>
        /// <param name="memberTuple">The member tuple in which the attribute is to be found</param>
        /// <returns>Returns true or false based on if the attribute was found or not</returns>
        public static bool HasAttribute<T>(this (MemberInfo, object, string, Attribute[]) memberTuple) where T : Attribute
        {
            //Access all the attributes of the member
            var attributes = memberTuple.Item4;
            if (attributes == null || attributes.Length == 0) return false;

            //Try to find the typed attribute 
            T foundAttribute = null;
            for (int i = 0; i < attributes.Length; i++)
            {
                foundAttribute = attributes[i] as T;
                if (foundAttribute != null) break;
            }

            //If the attribute was found return true else false
            return foundAttribute != null;
        }
    }
}
