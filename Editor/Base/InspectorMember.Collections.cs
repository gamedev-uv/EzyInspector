using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace UV.EzyInspector.Editors
{
    using EzyReflection;

    public partial class InspectorMember
    {
        /// <summary>
        /// Whether the member is a collection or not
        /// </summary>
        public bool IsCollection { get; private set; }

        /// <summary>
        /// Whether the member is a valid collection or not
        /// </summary>
        public bool IsValidCollection { get; private set; }

        /// <summary>
        /// The type of element which the member contains
        /// </summary>
        public Type ElementType { get; private set; }

        /// <summary>
        /// Initializes the member collection for the given targetObject
        /// </summary>
        /// <param name="rootMember">The root member for the member</param>
        /// <param name="serializedObject">The serializedObject for the member</param>
        public void InitializeCollection(InspectorMember rootMember, SerializedObject serializedObject)
        {
            IsCollection = true;
            IsValidCollection = true;
            ChildMembers = Array.Empty<Member>();
            var members = new List<Member>();

            //Find element's type 
            ElementType = MemberType.GetElementType();
            if (ElementType == null)
            {
                var typeArgs = MemberType.GenericTypeArguments;
                if (typeArgs == null || typeArgs.Length == 0) return;
                ElementType = typeArgs.First();
            }

            //Loop through and create a InspectorMember for each element
            for (int i = 0; i < MemberProperty.arraySize; i++)
            {
                var element = MemberProperty.GetArrayElementAtIndex(i);
                try
                {
                    object value = element.propertyType == SerializedPropertyType.ManagedReference ?
                                   element.managedReferenceValue :
                                   element.boxedValue;

                    var elementMember = new InspectorMember(value, element.propertyPath)
                    {
                        MemberProperty = serializedObject.FindProperty($"{Path}.Array.data[{i}]")
                    };

                    members.Add(elementMember);
                }
                catch (Exception exception)
                {
                    IsValidCollection = false;
                    Debug.LogWarning($"Error while fetching value for : {Name} : {exception}");
                }
            }

            //Find all the drawable members under the array elements
            ChildMembers = members.ToArray();
            _cachedDrawableMembers = GetDrawableMembers(rootMember, serializedObject, true);
        }
    }
}
