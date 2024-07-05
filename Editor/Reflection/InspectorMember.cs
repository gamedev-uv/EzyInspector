using System;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace UV.EzyInspector
{
    using EzyReflection;

    /// <summary>
    /// Defines a member which appears in the Inspector
    /// </summary>
    public class InspectorMember : Member
    {
        public InspectorMember(object instance) : base(instance) { }
        public InspectorMember(MemberInfo memberInfo, object instance, object parentObject) : base(memberInfo, instance, parentObject) { }

        /// <summary>
        /// Whether the member is hidden or not
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Whether the member is readonly or not
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// The parent member of the member
        /// </summary>
        public InspectorMember ParentMember { get; private set; }

        /// <summary>
        /// The serialized property of the member
        /// </summary>
        public SerializedProperty MemberProperty { get; private set; }

        /// <summary>
        /// The depth of the member 
        /// </summary>
        public int Depth { get; private set; }

        /// <summary>
        /// Whether the member is serialized or not
        /// </summary>
        /// <returns>Return true or false based on if it is serialized or not</returns>
        public bool IsSerialized()
        {
            return HasAttribute<SerializeField>() || HasAttribute<SerializeReference>() || IsPublic();
        }

        /// <summary>
        /// Whether the member is public or not
        /// </summary>
        /// <returns>Return true or false based on if it is public or not</returns>
        public bool IsPublic()
        {
            if (MemberInfo is FieldInfo field) return field.IsPublic;
            if (MemberInfo is MethodInfo method) return method.IsPublic;

            if (MemberInfo is PropertyInfo property)
                return (property.GetGetMethod(true) ?? property.GetSetMethod(true)).IsPublic;

            return false;
        }

        /// <summary>
        /// Whether the given member is a valid member 
        /// </summary>
        /// <param name="memberInfo">The memberInfo which is to be checked</param>
        /// <returns>Returns true or false based on whether the given member is a valid member or not</returns>
        public override bool IsValidMember(MemberInfo memberInfo)
        {
            if (!base.IsValidMember(memberInfo)) return false;

            //If it a unity property
            string[] unityMembers = { "gameObject", "transform", "mesh", "destroyCancellationToken", "hideFlags" };
            if (unityMembers.Contains(memberInfo.Name)) return false;

            return true;
        }

        public override bool IsSearchableChild(Member child)
        {
            Type[] unityTypes = { typeof(Color), typeof(Vector2), typeof(Vector3) };
            return base.IsSearchableChild(child) && !unityTypes.Contains(child.MemberType);
        }

        /// <summary>
        /// Returns a member for the given memberInfo
        /// </summary>
        /// <param name="memberInfo">The memberInfo for which a member is to be returned</param>
        /// <returns>Returns the newly created member if handled else null</returns>
        public override Member GetMember(MemberInfo memberInfo)
        {
            var baseMember = base.GetMember(memberInfo);
            if (baseMember == null) return null;
            return new InspectorMember(baseMember.MemberInfo, baseMember.Instance, baseMember.ParentObject);
        }

        /// <summary>
        /// Initializes the member for the given targetObject
        /// </summary>
        /// <param name="parentMember">The parent member for this member</param>
        /// <param name="target">The target inspector for the member</param>
        /// <param name="serializedObject">The serializedObject for the member</param>
        public void InitializeMember(InspectorMember parentMember, Object target, SerializedObject serializedObject)
        {
            ParentMember = parentMember;
            Path = Path.Replace($"{target}.", "");
            Depth = Path.Count(x => x.Equals('.'));
            MemberProperty = serializedObject.FindProperty(Path);
        }

        /// <summary>
        /// The members which are to be drawn under on the inspector under this member
        /// </summary>
        /// <param name="target">The target for this member</param>
        /// <param name="serializedObject">The current serializedObject</param>
        /// <returns>Returns all the members which are to be drawn on the inspector</returns>
        public InspectorMember[] GetDrawableMembers(Object target, SerializedObject serializedObject)
        {
            FindChildren();
            var children = GetChildren<InspectorMember>();
            List<InspectorMember> drawableMembers = new();

            for (int i = 0; i < children.Length; i++)
            {
                var member = children[i];

                //If it is a method
                if (member.MemberInfo is MethodInfo)
                {
                    if (!member.HasAttribute<SerializeMemberAttribute>()) continue;
                    drawableMembers.Add(member);
                    continue;
                }

                //Skip if it is not serialized
                if (!member.IsSerialized()) continue;

                //Initialize the child member
                member.InitializeMember(this, target, serializedObject);
                if (member.MemberProperty == null && !member.HasAttribute<SerializeMemberAttribute>()) continue;

                //If found find all under its children
                drawableMembers.Add(member);
                drawableMembers.AddRange(member.GetDrawableMembers(target, serializedObject));
            }

            return drawableMembers.ToArray();
        }

        /// <summary>
        /// Whether the current member has a parent member
        /// </summary>
        /// <returns>Returns true or false based on the member has a parent or not</returns>
        public bool HasParent()
        {
            return ParentMember != null;
        }

        /// <summary>
        /// Whether the parent is expanded in the inspector 
        /// </summary>
        /// <returns>Returns true or false based on if the parent is expanded in the inspector</returns>
        public bool IsParentExpanded()
        {
            if (!HasParent()) return true;
            if (ParentMember.MemberProperty == null) return true;
            return ParentMember.MemberProperty.isExpanded;
        }

        /// <summary>
        /// Whether the member or its parent(s) are hidden
        /// </summary>
        /// <returns> Returns true or false based on if the member or its parent are hidden</returns>
        public bool IsMemberHidden()
        {
            if (!HasParent()) return IsHidden;
            return IsHidden || ParentMember.IsMemberHidden();
        }
    }
}
