﻿using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using Object = UnityEngine.Object;

namespace UV.EzyInspector.Editors
{
    using EzyReflection;

    /// <summary>
    /// A overriden inspector 
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Object), true)]
    public class EzyInspector : Editor
    {
        /// <summary>
        /// The mono script for the current target
        /// </summary>
        public MonoScript TargetMono { get; protected set; }

        /// <summary>
        /// The root target member
        /// </summary>
        public InspectorMember RootMember { get; protected set; }

        /// <summary>
        /// All the members which are to be drawn 
        /// </summary>
        public InspectorMember[] DrawableMembers { get; protected set; }

        /// <summary>
        /// All the methods which are to be called when the inspector is updated 
        /// </summary>
        public (Member, OnInspectorUpdatedAttribute)[] OnInspectorUpdatedMethods { get; protected set; }

        /// <summary>
        /// Whether the default mono view is to be displaced
        /// </summary>
        public bool DrawDefaultMonoGUI { get; protected set; }

        /// <summary>
        /// Whether the open mono script button is to be hidden or not
        /// </summary>
        public bool HideMonoScript { get; protected set; }

        protected virtual void OnEnable() => Init();

        /// <summary>
        /// Initializes all the needed variables
        /// </summary>
        protected virtual void Init()
        {
            //Initialize the root member
            RootMember = new(target);

            //Find the drawable members 
            DrawableMembers = RootMember.GetDrawableMembers(target, serializedObject);

            //Find the OnInspectorUpdateMethods
            OnInspectorUpdatedMethods = RootMember.FindMembersWithAttribute<OnInspectorUpdatedAttribute>(true);

            //Whether the open script button is to be hidden
            HideMonoScript = RootMember.HasAttribute<HideMonoGUIAttribute>();
            if (HideMonoScript) return;

            //Check whether the default mono script gui is to be drawn
            DrawDefaultMonoGUI = RootMember.HasAttribute<DefaultMonoGUIAttribute>();

            //Find the mono script for the open script button
            if (target is MonoBehaviour mono)
                TargetMono = MonoScript.FromMonoBehaviour(mono);
            else if (target is ScriptableObject so)
                TargetMono = MonoScript.FromScriptableObject(so);
        }

        public override void OnInspectorGUI()
        {
            DrawMonoScriptUI();
            DrawSerializedMembers(RootMember);

            //Apply modified properties and check if anything was updated 
            if (!serializedObject.ApplyModifiedProperties()) return;

            if (OnInspectorUpdatedMethods == null || OnInspectorUpdatedMethods.Length == 0) return;
            for (int i = 0; i < OnInspectorUpdatedMethods.Length; i++)
            {
                var tuple = OnInspectorUpdatedMethods[i];
                var method = tuple.Item1;
                var attribute = tuple.Item2;
                if (!attribute.IsCorrectEditorPlayerState()) continue;
                try
                {
                    (method.MemberInfo as MethodInfo)?.Invoke(target, null);
                }
                catch { }
            }
        }

        /// <summary>
        /// Draws the ui for the MonoScript
        /// </summary>
        protected virtual void DrawMonoScriptUI()
        {
            if (HideMonoScript) return;
            if (TargetMono == null) return;

            //If the default gui is to be used 
            if (DrawDefaultMonoGUI)
            {
                GUI.enabled = false;
                EditorGUILayout.ObjectField("Script", TargetMono, typeof(MonoScript), false);
                GUI.enabled = true;
                return;
            }

            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                //Draw the open script button
                var openGUI = EditorGUIUtility.IconContent("d_boo Script Icon");
                openGUI.text = "Open Script";
                openGUI.tooltip = "Opens the script in the specified External Script Editor";
                this.DrawButton(openGUI,
                () =>
                {
                    AssetDatabase.OpenAsset(TargetMono);
                },
                GUILayout.Height(EditorGUIUtility.singleLineHeight));

                //Draw the ping script button
                var pingGUI = EditorGUIUtility.IconContent("d_Folder Icon");
                pingGUI.tooltip = "Ping Script\nPings the script in the Project folder";
                this.DrawButton(pingGUI, () =>
                {
                    EditorUtility.FocusProjectWindow();
                    EditorGUIUtility.PingObject(TargetMono);
                },
                GUILayout.Height(EditorGUIUtility.singleLineHeight),
                GUILayout.Width(EditorGUIUtility.singleLineHeight * 2));

                //Draw the select script button
                var selectGUI = EditorGUIUtility.IconContent("d_Selectable Icon");
                selectGUI.tooltip = "Select Script\nSelect the given script";
                this.DrawButton(selectGUI, () =>
                {
                    EditorUtility.FocusProjectWindow();
                    Selection.activeObject = TargetMono;
                    EditorGUIUtility.PingObject(TargetMono);
                },
                GUILayout.Height(EditorGUIUtility.singleLineHeight),
                GUILayout.Width(EditorGUIUtility.singleLineHeight * 2));
            }
            GUILayout.Space(10);
        }

        /// <summary>
        /// Draws the given member
        /// </summary>
        /// <param name="member">The member which is to be drawn</param>
        protected virtual bool DrawMember(InspectorMember member)
        {
            //Check whether the member has a SerializedProperty associated with it
            var property = member.MemberProperty;
            if (property == null) return false;

            //Disable it if needed
            EditorGUI.BeginDisabledGroup(member.IsReadOnly || member.HasAttribute<ReadOnlyAttribute>());
            if (property.isArray)
                DrawArray(property, member);
            else
                EditorGUILayout.PropertyField(property, property.isArray);

            EditorGUI.EndDisabledGroup();
            return property.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draws all serialized members under the given rootMember
        /// </summary>
        /// <param name="rootMember">The root member which contains the drawableMembers</param>
        /// <param name="includeMethods">Whether methods are to be drawn or not</param>
        /// <param name="includeSelf">Whether the root member is to be drawn or not</param>
        /// <returns>Returns true or false based on if a property was updated or not</returns>
        protected virtual bool DrawSerializedMembers(InspectorMember rootMember, bool includeMethods = true, bool includeSelf = false)
        {
            var members = rootMember.GetDrawableMembers(target, serializedObject, includeMethods);
            if (includeSelf)
                members = members.Append(rootMember).ToArray();
            return DrawSerializedMembers(rootMember, members);
        }

        /// <summary>
        /// Draws all given serialized members 
        /// </summary>
        /// <param name="rootMember">The root member which contains the drawableMembers</param>
        /// <param name="drawableMembers">The members which are to be drawn</param>
        /// <returns>Returns true or false based on if a property was updated or not</returns>
        protected virtual bool DrawSerializedMembers(Member rootMember, InspectorMember[] drawableMembers)
        {
            if (drawableMembers == null || drawableMembers.Length == 0) return false;

            var indent = EditorGUI.indentLevel;
            var guiState = GUI.enabled;

            for (int i = 0; i < drawableMembers.Length; i++)
            {
                var member = drawableMembers[i];

                //Change indent level and readonly based on the parent 
                EditorGUI.indentLevel = indent + member.Depth;
                GUI.enabled = guiState;

                var memberInfo = member.MemberInfo;
                if (!member.IsParentExpanded()) continue;

                //Check if member has ShowIfAttribute attribute and draw it accordingly 
                if (member.TryGetAttribute(out ShowIfAttribute showIf))
                {
                    var canShow = CorrectShowIfValue(rootMember, member, showIf);

                    if (showIf.HideMode == HideMode.Hide)
                        member.IsHidden = !canShow;

                    if (showIf.HideMode == HideMode.ReadOnly)
                        member.IsReadOnly = !canShow;
                }

                //If the member has the EditMode Attribute draw it accordingly 
                if (member.TryGetAttribute(out EditModeOnly editMode) && Application.isPlaying)
                {
                    if (editMode.HideMode == HideMode.Hide) member.IsHidden = true;
                    if (editMode.HideMode == HideMode.ReadOnly) member.IsReadOnly = true;
                }

                //Draw a button for the method
                if (member.TryGetAttribute(out ButtonAttribute button))
                {
                    //Skips methods if the object is an Object Reference 
                    var isNestedObjectMethod = member.ParentObject is Object @object && @object != target;
                    if (isNestedObjectMethod) continue;

                    DrawButton(member.ParentObject, memberInfo as MethodInfo, button);
                    continue;
                }

                //Check whether it has the hide in inspector if it does hide it 
                if (member.IsMemberHidden() || member.HasAttribute<HideInInspector>())
                    continue;

                //If it is a label
                if (member.TryGetAttribute(out DisplayAsLabel label))
                {
                    GUILayout.Label(label.FormattedString
                                                       .Replace("{0}", member.Name)
                                                       .Replace("{1}", $"{member.GetValue()}"));
                    continue;
                }

                //Draw the member
                if (DrawMember(member)) return true;
            }

            return serializedObject.ApplyModifiedProperties();
        }

        #region Array Drawing
        /// <summary>
        /// Draws the array property in the current inspector 
        /// </summary>
        /// <param name="arrayProperty">The array property</param>
        /// <param name="member">The member of the array property</param>
        protected virtual void DrawArray(SerializedProperty arrayProperty, InspectorMember member)
        {
            //Draw array header foldout 
            using (new EditorGUILayout.HorizontalScope())
            {
                //Array foldout and size
                arrayProperty.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(arrayProperty.isExpanded, arrayProperty.displayName);
                arrayProperty.arraySize = EditorGUILayout.IntField(arrayProperty.arraySize, GUILayout.Width(50));

                //Clear list button
                using (new EditorGUI.DisabledGroupScope(arrayProperty.arraySize == 0))
                {
                    GUIContent clearList = new(EditorGUIUtility.IconContent("d_winbtn_win_close@2x"))
                    {
                        tooltip = "Clear list"
                    };
                    if (GUILayout.Button(clearList, GUILayout.Width(20), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                    {
                        arrayProperty.ClearArray();
                        arrayProperty.serializedObject.ApplyModifiedProperties();
                    }
                }
            }

            //If the array is expanded 
            if (!arrayProperty.isExpanded)
            {
                EditorGUILayout.EndFoldoutHeaderGroup();
                return;
            }

            using (var backGroundBox = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                //Draw all the elements
                DrawArrayElements(arrayProperty, member);

                //Draw the Add and Remove buttons
                using (new EditorGUILayout.HorizontalScope())
                {
                    //Add a space to align buttons to the right
                    GUILayout.FlexibleSpace();

                    //Add button
                    if (GUILayout.Button(new GUIContent("Add", "Adds a new element to list")))
                    {
                        arrayProperty.arraySize++;
                        arrayProperty.serializedObject.ApplyModifiedProperties();
                    }

                    //Remove button
                    using (new EditorGUI.DisabledGroupScope(arrayProperty.arraySize == 0))
                    {
                        if (GUILayout.Button(new GUIContent("Remove", "Removes the last element from the list")))
                        {
                            arrayProperty.arraySize--;
                            arrayProperty.serializedObject.ApplyModifiedProperties();
                        }
                    }
                }
            }

            //Apply any changes that were made
            arrayProperty.serializedObject.ApplyModifiedProperties();
            if (arrayProperty.arraySize != member.ChildMembers.Length)
                member.InitializeArray(target, serializedObject);

            EditorGUILayout.EndFoldoutHeaderGroup();
            GUILayout.Space(10);
        }

        /// <summary>
        /// Draws all the array elements for the given array property 
        /// </summary>
        /// <param name="arrayProperty">The array property for which the elements are to be drawn</param>
        /// <param name="arrayMember">The member for the property</param>
        protected virtual void DrawArrayElements(SerializedProperty arrayProperty, InspectorMember arrayMember)
        {
            //If it has zero elements 
            if (arrayProperty.arraySize == 0)
            {
                EditorGUILayout.LabelField("List is empty");
                return;
            }

            var elementType = arrayMember.MemberType.GetElementType();
            var drawFoldout = !elementType.IsSimpleType() && !elementType.IsSubclassOf(typeof(Object));

            //Draw the array elements 
            int length = arrayProperty.arraySize;
            for (int i = 0; i < length; i++)
            {
                var element = arrayProperty.GetArrayElementAtIndex(i);
                var elementMember = arrayMember.ChildMembers[i] as InspectorMember;

                //Draw the array element GUI
                DrawArrayElement(i, drawFoldout, element, elementMember,
                arrayProperty, arrayMember,
                () =>
                {
                    //If the element is to be deleted
                    arrayProperty.DeleteArrayElementAtIndex(i);
                    arrayProperty.serializedObject.ApplyModifiedProperties();
                    return;
                },
                (targetIndex) =>
                {
                    arrayProperty.MoveArrayElement(i, targetIndex);
                    arrayProperty.serializedObject.ApplyModifiedProperties();
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
        /// <param name="arrayProperty">The array property for which the elements are to be drawn</param>
        /// <param name="arrayMember">The member for the array property</param>
        /// <param name="wantsToDelete">The action which is to be called if the element is to be removed</param>
        /// <param name="wantsToMoveElement">The action which is to be called if the element is to be moved</param>
        protected virtual void DrawArrayElement(int index, bool drawFoldout,
                                                SerializedProperty element, InspectorMember elementMember,
                                                SerializedProperty arrayProperty, InspectorMember arrayMember,
                                                Action wantsToDelete, Action<int> wantsToMoveElement)
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

                    DrawElementReArrangeUI(index, arrayProperty.arraySize, buttonStyle, wantsToMoveElement, guiLayoutOptions);
                    DrawElementRemoveButton(buttonStyle, wantsToDelete, guiLayoutOptions);

                    horizontal.Dispose();
                    if (!element.isExpanded)
                    {
                        EditorGUI.indentLevel = guiIndent;
                        return;
                    }
                }
                else
                {
                    //Draw re arrange buttons on left if there is no foldout 
                    DrawElementReArrangeUI(index, arrayProperty.arraySize, buttonStyle, wantsToMoveElement, guiLayoutOptions);
                }

                //If any values were updated; Reinitialize the children
                if (DrawSerializedMembers(elementMember, true, !drawFoldout))
                    arrayMember.InitializeArray(target, serializedObject);

                if (!drawFoldout)
                    DrawElementRemoveButton(buttonStyle, wantsToDelete, guiLayoutOptions);
            }

            EditorGUI.indentLevel = guiIndent;
        }

        /// <summary>
        /// Draws a button for removing an element from a list.
        /// </summary>
        /// <param name="buttonStyle">The style to be applied to the remove button.</param>
        /// <param name="wantsToDelete">The action to be invoked when the remove button is clicked.</param>
        /// <param name="guiLayoutOptions">Optional layout parameters for the remove button.</param>
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
        /// <param name="buttonStyle">The style to be applied to the rearrange buttons.</param>
        /// <param name="wantsToMoveElement">The action which is to be called if the element is to be moved</param>
        /// <param name="guiLayoutOptions">Optional layout parameters for the rearrange buttons.</param>
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
        #endregion

        /// <summary>
        /// Whether the member has the correct value as per as the ShowIf attribute
        /// </summary>
        /// <param name="rootMember">The root member that contains the member</param>
        /// <param name="member">The member itself</param>
        /// <param name="showIf">The show if attribute</param>
        /// <returns>returns true or false based on if the value is correct</returns>
        protected virtual bool CorrectShowIfValue(Member rootMember, InspectorMember member, ShowIfAttribute showIf)
        {
            //Find finding the showIfMember
            var showIfMember = rootMember.FindMember<InspectorMember>(showIf.PropertyName, true);

            //If the value is null display a help box accordingly 
            if (showIfMember == null)
            {
                //Extract just the name of the member 
                var memberName = member.Name;
                var startingIndex = memberName.IndexOf('<') + 1;
                var endingIndex = memberName.IndexOf('>');

                if (startingIndex > -1 && endingIndex > -1)
                    memberName = memberName[startingIndex..endingIndex];

                //Draw the help box 
                EditorGUILayout.HelpBox($"\"{showIf.PropertyName}\" not found!\nCan't draw : \"{memberName}\"", MessageType.Error);
                return false;
            }

            //If the current value is null
            var value = showIfMember.GetValue();
            if (showIf.TargetValues == null || showIf.TargetValues.Length == 0)
                return value == null;

            for (int i = 0; i < showIf.TargetValues.Length; i++)
            {
                var targetValue = showIf.TargetValues[i];
                var targetValueType = targetValue.GetType();

                //Casts the fetched value back into the type value
                var typedValue = ReflectionHelpers.ChangeType(value, targetValueType);
                if (typedValue == null) return false;

                //Checks whether the values are the same or not
                if (typedValue.Equals(targetValue))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Draws a button in the inspector for the given method styled by the button attribute
        /// </summary>
        /// <param propertyPath="method">The parent object which conatains the method to be drawn</param>
        /// <param propertyPath="method">The method to be drawn</param>
        /// <param propertyPath="button">The button used to style the ui</param>
        protected virtual void DrawButton(object parent, MethodInfo method, ButtonAttribute button)
        {
            string buttonName = button.DisplayName ?? method.Name;
            this.DrawButton(new(buttonName), () =>
            {
                method?.Invoke(parent, null);
                EditorUtility.SetDirty(this);
            });
            GUILayout.Space(5);
        }
    }
}
