using System;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using UnityEngine.Events;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace UV.EzyInspector.Editors
{
    using EzyReflection;

    /// <summary>
    /// Defines a member which appears in the Inspector
    /// </summary>
    public partial class InspectorMember : Member
    {
        public InspectorMember(object instance, string pathPrefix = null) : base(instance)
        {
            pathPrefix ??= Name;
            Path = pathPrefix;
        }

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
        /// The serialized property of the member
        /// </summary>
        public SerializedProperty MemberProperty { get; private set; }

        /// <summary>
        /// The depth of the member
        /// </summary>
        public int Depth
        {
            get
            {
                if (MemberProperty == null)
                    return EditorGUI.indentLevel;

                try
                {
                    return MemberProperty.depth;
                }
                catch (InvalidOperationException)
                {
                    // SerializedProperty iterator is invalid (playmode exit / rebuild)
                    return EditorGUI.indentLevel;
                }
            }
        }


        /// <summary>
        /// The cached drawable members 
        /// </summary>
        private InspectorMember[] _cachedDrawableMembers;

        /// <summary>
        /// The unity types which are to be not searched
        /// </summary>
        private readonly Type[] _excludedUnityTypes =
        {
             typeof(Rect),
             typeof(RectInt),

             typeof(Color),
             typeof(Gradient),

             typeof(Vector2),
             typeof(Vector2Int),

             typeof(Vector3),
             typeof(Vector3Int),

             typeof(Vector4),
             typeof(Quaternion),
             typeof(AnimationCurve),

             typeof(Bounds),
             typeof(BoundsInt),

             typeof(AssetImporter),
        };

        /// <summary>
        /// Whether the member has serializer attributes or not
        /// </summary>
        /// <returns>Return true or false based on if it has serializer attributes or not</returns>
        public bool HasSerializerAttributes()
        {
            return HasAttribute<SerializeField>() || HasAttribute<SerializeReference>() || HasAttribute<SerializeMemberAttribute>();
        }

        /// <summary>
        /// Whether the member is serialized or not
        /// </summary>
        /// <returns>Return true or false based on if it is serialized or not</returns>
        public bool IsSerialized()
        {
            if (MemberInfo is PropertyInfo)
                return HasSerializerAttributes();

            return IsPublic() || HasSerializerAttributes();
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
            return base.IsSearchableChild(child) && !HasAttribute<HideInInspector>();
        }

        public override bool IsSearchableType(Type type, bool unityTypes = false)
        {
            return base.IsSearchableType(type, unityTypes) && !IsUnityType(type);
        }

        /// <summary>
        /// Whether the given memberType is a unity type or not
        /// </summary>
        /// <param name="memberType">The member type which is to be checked</param>
        public virtual bool IsUnityType(Type memberType)
        {
            if (memberType != null && memberType.IsSubclassOf(typeof(UnityEventBase))) return true;
            return _excludedUnityTypes.Contains(MemberType);
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
        /// <param name="rootMember">The root member for the member</param>
        /// <param name="serializedObject">The serializedObject for the member</param>
        public void InitializeMember(InspectorMember parentMember, InspectorMember rootMember, SerializedObject serializedObject)
        {
            var target = rootMember.Instance as Object;
            ParentMember = parentMember;

            //Find the serialized property for the member
            Path = Path.Replace($"{target}.", "");

            //Initialize show if attribute
            InitializeShowIfMember(rootMember);

            MemberProperty = serializedObject.FindProperty(Path);
            if (MemberProperty == null) return;

            //If the member is an collection;
            if (!MemberProperty.isArray || HasAttribute<HideInInspector>()) return;
            InitializeCollection(rootMember, serializedObject);
        }

        /// <summary>
        /// The members which are to be drawn under on the inspector under this member
        /// </summary>
        /// <param name="rootObject">The rootMember for this member</param>
        /// <param name="serializedObject">The current serializedObject</param>
        /// <param name="includeMethods">Whether methods are to be included or not</param>
        /// <returns>Returns all the members which are to be drawn on the inspector</returns>
        public InspectorMember[] GetDrawableMembers(InspectorMember rootObject, SerializedObject serializedObject, bool includeMethods = true)
        {
            // clear cache
            if (Event.current.type == EventType.Layout)
                _cachedDrawableMembers = null;
            
            if (!IsSearchableType(MemberType))
            {
                if (Instance != null && !Instance.Equals(rootObject.Instance))
                    return Array.Empty<InspectorMember>();
            }

            //If it needs to use the property drawer 
            if (HasAttribute<UsePropertyDrawer>())
                return Array.Empty<InspectorMember>();

            //If the members have already been found
            if (_cachedDrawableMembers != null && _cachedDrawableMembers.Length > 0) return _cachedDrawableMembers;

            if (ChildMembers == null || ChildMembers.Length == 0) FindChildren();
            var children = GetChildren<InspectorMember>();
            List<InspectorMember> drawableMembers = new();

            for (int i = 0; i < children.Length; i++)
            {
                var member = children[i];

                //If it is a method
                if (member.MemberInfo is MethodInfo)
                {
                    if (!includeMethods) continue;
                    if (!member.HasAttribute<SerializeMemberAttribute>()) continue;
                    member.InitializeShowIfMember(rootObject);
                    drawableMembers.Add(member);
                    continue;
                }

                //Skip if it is not serialized
                if (!member.IsSerialized())
                    continue;

                //Initialize the child member
                member.InitializeMember(this, rootObject, serializedObject);
                if (member.MemberProperty == null && !member.HasAttribute<SerializeMemberAttribute>())
                    continue;

                //If found find all under its children
                drawableMembers.Add(member);
                drawableMembers.AddRange(member.GetDrawableMembers(rootObject, serializedObject, includeMethods));
            }

            _cachedDrawableMembers = drawableMembers.ToArray();
            return _cachedDrawableMembers;
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

