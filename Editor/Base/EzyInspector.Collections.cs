using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UV.EzyInspector.Editors
{
    using EzyReflection;

    public partial class EzyInspector
    {
        /// <summary>
        /// Draws the collection property in the current inspector 
        /// </summary>
        /// <param name="property">The collection property</param>
        /// <param name="member">The member of the collection property</param>
        /// <param name="disabled">Whether the collection gui controls are to be disabled</param>
        protected virtual bool DrawCollection(SerializedProperty property, InspectorMember member, bool disabled = false)
        {
            //Draw the default inspector if the type of collection isn't supported 
            var elementType = member.ElementType;
            if (elementType == null)
            {
                EditorGUILayout.PropertyField(property, true);
                return false;
            }

            //Draw the foldout header
            DrawFoldoutHeader(property, member, elementType, disabled);
            if (!property.isExpanded) return property.serializedObject.ApplyModifiedProperties();

            bool madeChanges = false;
            using (var backGroundBox = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                //Draw all the elements
                var drawFoldout = !elementType.IsSimpleType() && !elementType.IsSubclassOf(typeof(Object));
                DrawCollectionElements(property, member, drawFoldout, () => madeChanges = true);
                DrawAddAndRemoveButtons(property, member);
            }

            //Apply any changes that were made
            madeChanges = madeChanges || property.serializedObject.ApplyModifiedProperties();
            if (madeChanges)
                member.InitializeCollection(RootMember, serializedObject);

            GUILayout.Space(10);
            return madeChanges;
        }

        /// <summary>
        /// Draws the add and remove buttons for the given property and member
        /// </summary>
        /// <param name="property">The collection property itself</param>
        /// <param name="member">The member for the property</param>
        public virtual void DrawAddAndRemoveButtons(SerializedProperty property, InspectorMember member)
        {
            var addGUIContent = new GUIContent("Add", "Adds a new element to list");
            var removeGUIContent = new GUIContent("Remove", "Removes the last element from the list");

            //Draw the Add and Remove buttons
            using (new EditorGUILayout.HorizontalScope())
            {
                //Add a space to align buttons to the right
                GUILayout.FlexibleSpace();

                //Add button
                if (GUILayout.Button(addGUIContent))
                    property.arraySize++;

                //Remove button
                using (new EditorGUI.DisabledGroupScope(property.arraySize == 0))
                {
                    if (GUILayout.Button(removeGUIContent))
                        property.arraySize--;
                }

                return;
            }
        }

        /// <summary>
        /// Draws the foldout header for the collection, with options to clear the list, handle drag-and-drop, and resize the array
        /// </summary>
        /// <param name="property">The collection property to draw the header for</param>
        /// <param name="member">The member representing the collection property</param>
        /// <param name="elementType">The type of elements in the collection</param>
        /// <param name="disabled">Whether the controls for the collection are disabled</param>
        public virtual void DrawFoldoutHeader(SerializedProperty property, InspectorMember member, Type elementType, bool disabled)
        {
            //Draw collection header foldout 
            using (var horizontal = new EditorGUILayout.HorizontalScope(EditorStyles.inspectorFullWidthMargins))
            {
                GUILayout.Space(member.Depth * 15);

                //Collection foldout
                GUI.enabled = true;
                property.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(property.isExpanded, property.displayName);

                //Handle the drap and drop functionality if the member isn't disabled
                if (!disabled)
                {
                    var foldoutRect = horizontal.rect;
                    foldoutRect.width -= 70 + member.Depth * 15;
                    HandleDragAndDrop(foldoutRect, property, member, elementType);
                }

                //Collection Size
                GUI.enabled = !disabled;
                property.arraySize = EditorGUILayout.IntField(property.arraySize,
                                                              GUILayout.MinWidth(50), GUILayout.MaxWidth(70)
                                                              );

                //Clear list button
                using (new EditorGUI.DisabledGroupScope(property.arraySize == 0))
                {
                    GUIContent clearList = new(EditorGUIUtility.IconContent("d_winbtn_win_close@2x"))
                    {
                        tooltip = "Clear list"
                    };
                    if (GUILayout.Button(clearList,
                                         GUILayout.MinWidth(10), GUILayout.MaxWidth(20),
                                         GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                        property.ClearArray();
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }

        /// <summary>
        /// Handles the drag and drop functionality for the collection
        /// </summary>
        /// <param name="property">The SerializedProperty representing the collection</param>
        /// <param name="member">Whether to draw foldouts for each element</param>
        /// <param name="elementType">The type of elements stored in the collection</param>
        public virtual void HandleDragAndDrop(Rect foldoutRect, SerializedProperty property, InspectorMember member, Type elementType)
        {
            if (!elementType.IsSubclassOf(typeof(Object))) return;
            var objects = foldoutRect.CheckDragAndDrop(elementType);
            if (objects.Length == 0) return;

            //Add elements to the collection
            var index = property.arraySize - 1;
            for (int i = 0; i < objects.Length; i++)
            {
                property.InsertArrayElementAtIndex(index + i + 1);
                var element = property.GetArrayElementAtIndex(index + i + 1);
                element.objectReferenceValue = objects[i];
            }

            //Update the property, and reinitialize members
            property.serializedObject.ApplyModifiedProperties();
            member.InitializeCollection(RootMember, serializedObject);
        }

        /// <summary>
        /// Draws all the array elements for the given array property 
        /// </summary>
        /// <param name="arrayProperty">The array property for which the elements are to be drawn</param>
        /// <param name="drawFoldout">Whether a foldout is to be drawn for each element</param>
        /// <param name="arrayMember">The member for the property</param>
        /// <param name="propertyUpdated">The action to be called if the property was updated</param>
        protected virtual void DrawCollectionElements(SerializedProperty arrayProperty, InspectorMember arrayMember, bool drawFoldout, Action propertyUpdated)
        {
            //If it has zero elements 
            if (arrayProperty.arraySize == 0)
            {
                EditorGUILayout.LabelField("List is empty");
                return;
            }

            //Draw the array elements 
            int length = arrayProperty.arraySize;
            for (int i = 0; i < length; i++)
            {
                if (i < 0 || i > arrayProperty.arraySize - 1) break;

                var element = arrayProperty.GetArrayElementAtIndex(i);
                if (element == null) return;
                InspectorMember elementMember = null;
                try
                {
                    elementMember = arrayMember.ChildMembers[i] as InspectorMember;
                }
                catch { }

                if (elementMember == null) break;

                //Draw the collection element GUI
                DrawCollectionElement(i, drawFoldout, element, elementMember,
                arrayProperty, () =>
                {
                    //If any values were madeChanges; Reinitialize the children
                    arrayMember.InitializeCollection(RootMember, serializedObject);
                    propertyUpdated?.Invoke();
                    return;
                },
                () =>
                {
                    //If the element is to be deleted
                    arrayProperty.DeleteArrayElementAtIndex(i);
                    arrayProperty.serializedObject.ApplyModifiedProperties();
                    propertyUpdated?.Invoke();
                    return;
                },
                (targetIndex) =>
                {
                    //If the element is to be moved
                    arrayProperty.MoveArrayElement(i, targetIndex);
                    arrayProperty.serializedObject.ApplyModifiedProperties();
                    propertyUpdated?.Invoke();
                    return;
                });
            }

            GUILayout.Space(5);
        }

        /// <summary>
        /// Draws the inspector for the given array element 
        /// </summary>
        /// <param name="index">The index array element which is to be drawn</param>
        /// <param name="element">The array element for which the inspector is to be drawn</param>
        /// <param name="elementMember">The member for the element property</param>
        /// <param name="collectionProperty">The array property for which the elements are to be drawn</param>
        /// <param name="onPropertyUpdated">The action which is to be called when the element is updated</param>
        /// <param name="wantsToDelete">The action which is to be called if the element is to be removed</param>
        /// <param name="wantsToMoveElement">The action which is to be called if the element is to be moved</param>
        protected virtual void DrawCollectionElement(int index, bool drawFoldout,
                                                SerializedProperty element, InspectorMember elementMember, SerializedProperty collectionProperty,
                                                Action onPropertyUpdated, Action wantsToDelete, Action<int> wantsToMoveElement)
        {
            //Indent if foldout is to be drawn
            drawFoldout = drawFoldout && element.propertyType == SerializedPropertyType.Generic;
            var guiIndent = EditorGUI.indentLevel;
            if (drawFoldout)
                EditorGUI.indentLevel++;

            //Draw foldout area for the element 
            using (var horizontal = new EditorGUILayout.HorizontalScope())
            {
                GUILayoutOption[] guiLayoutOptions = new GUILayoutOption[] { GUILayout.Width(20), GUILayout.Height(18) };
                GUIStyle buttonStyle = new(EditorStyles.iconButton)
                {
                    fixedHeight = 20,
                    fixedWidth = 20,
                };

                //If a foldout is to be drawn 
                if (drawFoldout)
                {
                    element.isExpanded = EditorGUILayout.Foldout(element.isExpanded, element.displayName, true);
                    var deleted = false;

                    DrawElementReArrangeUI(index, collectionProperty.arraySize, buttonStyle, wantsToMoveElement, guiLayoutOptions);
                    DrawElementRemoveButton(buttonStyle, () =>
                    {
                        wantsToDelete?.Invoke();
                        deleted = true;
                        return;

                    }, guiLayoutOptions);

                    horizontal.Dispose();

                    if (deleted || !element.isExpanded)
                    {
                        EditorGUI.indentLevel = guiIndent;
                        return;
                    }
                }
                else
                {
                    //Draw re arrange buttons on left if there is no foldout 
                    DrawElementReArrangeUI(index, collectionProperty.arraySize, buttonStyle, wantsToMoveElement, guiLayoutOptions);
                }

                //Draw the members under the collection element
                if (DrawSerializedMembers(elementMember, true, !drawFoldout))
                    onPropertyUpdated?.Invoke();

                if (!drawFoldout)
                    DrawElementRemoveButton(buttonStyle, wantsToDelete, guiLayoutOptions);
            }

            EditorGUI.indentLevel = guiIndent;
        }

        /// <summary>
        /// Draws a button for removing an element from a list
        /// </summary>
        /// <param name="buttonStyle">The style to be applied to the remove button</param>
        /// <param name="wantsToDelete">The action to be invoked when the remove button is clicked</param>
        /// <param name="guiLayoutOptions">Optional layout parameters for the remove button</param>
        protected virtual void DrawElementRemoveButton(GUIStyle buttonStyle, Action wantsToDelete, params GUILayoutOption[] guiLayoutOptions)
        {
            GUIContent removeButton = new(EditorGUIUtility.IconContent("d_winbtn_win_close@2x"))
            {
                tooltip = "Removes the element from the list"
            };

            if (GUILayout.Button(removeButton, buttonStyle, guiLayoutOptions))
                wantsToDelete?.Invoke();
        }

        /// <summary>
        /// Draws the GUI for rearranging an element within a list.
        /// </summary>
        /// <param name="index">The index of the element which is to be drawn</param>
        /// <param name="arraySize">The total size of the collection to be drawn</param>
        /// <param name="buttonStyle">The style to be applied to the rearrange buttons</param>
        /// <param name="wantsToMoveElement">The action which is to be called if the element is to be moved</param>
        /// <param name="guiLayoutOptions">Optional layout parameters for the rearrange buttons</param>
        protected virtual void DrawElementReArrangeUI(int index, int arraySize, GUIStyle buttonStyle, Action<int> wantsToMoveElement, params GUILayoutOption[] guiLayoutOptions)
        {
            if (arraySize == 1) return;

            var firstElement = index == 0;
            var lastElement = index == arraySize - 1;

            if (GUILayout.Button(Resources.Load<Texture>("caret-up"), buttonStyle))
                wantsToMoveElement?.Invoke(firstElement ? arraySize - 1 : --index);

            if (GUILayout.Button(Resources.Load<Texture>("caret-down"), buttonStyle))
                wantsToMoveElement?.Invoke(lastElement ? 0 : ++index);
        }
    }
}
