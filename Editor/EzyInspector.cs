using System;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using Object = UnityEngine.Object;

namespace UV.EzyInspector.Editors
{
    using EzyReflection;
    using System.Linq;

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
            if (property.isArray && property.name.StartsWith("_"))
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

            //Apply any changes that were made
            arrayProperty.serializedObject.ApplyModifiedProperties();
            if (arrayProperty.arraySize != member.ChildMembers.Length)
                member.InitializeArray(target, serializedObject);

            //If the array is expanded 
            if (!arrayProperty.isExpanded)
            {
                EditorGUILayout.EndFoldoutHeaderGroup();
                return;
            }

            //Styles and contents for buttons 
            var style = new GUIStyle(EditorStyles.iconButton);
            GUIContent addButton = new(EditorGUIUtility.IconContent("Toolbar Plus"))
            {
                tooltip = "Adds a new element to list"
            };
            GUIContent removeButton = new(EditorGUIUtility.IconContent("Toolbar Minus"))
            {
                tooltip = "Removes the last element from the list"
            };

            using (var backGroundBox = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                //Draw all the elements
                DrawArrayElements(arrayProperty, member);
            }

            //Draw the array transformation buttons
            var buttonRect = EditorGUILayout.GetControlRect();
            buttonRect.x = buttonRect.width - 20 * 2.5f;
            buttonRect.width = 20 * 1.5f;
            buttonRect.width /= 2;

            //Add button
            if (GUI.Button(buttonRect, addButton, style))
            {
                arrayProperty.arraySize++;
                arrayProperty.serializedObject.ApplyModifiedProperties();
            }

            //Remove button
            using (new EditorGUI.DisabledGroupScope(arrayProperty.arraySize == 0))
            {
                buttonRect.x += buttonRect.width * 1.4f;
                if (GUI.Button(buttonRect, removeButton, style))
                {
                    arrayProperty.arraySize--;
                    arrayProperty.serializedObject.ApplyModifiedProperties();
                }
            }
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

            EditorGUI.indentLevel++;
            var guiIndent = EditorGUI.indentLevel;

            var elementType = arrayMember.MemberType.GetElementType();
            var drawFoldout = !elementType.IsSimpleType() && !elementType.IsSubclassOf(typeof(Object));

            //Draw the array elements 
            for (int i = 0; i < arrayProperty.arraySize; i++)
            {
                EditorGUI.indentLevel = guiIndent;
                var element = arrayProperty.GetArrayElementAtIndex(i);
                var elementMember = arrayMember.ChildMembers[i] as InspectorMember;

                //Draw the array element GUI
                DrawArrayElement(element, elementMember,
                arrayProperty, arrayMember,
                drawFoldout,
                () =>
                {
                    //If the element is to be deleted
                    arrayProperty.DeleteArrayElementAtIndex(i);
                    arrayProperty.serializedObject.ApplyModifiedProperties();
                    return;
                });
            }
        }

        /// <summary>
        /// Draws the inspector for the given array element 
        /// </summary>
        /// <param name="element">The array element for which the inspector is to be drawn</param>
        /// <param name="elementMember">The member for the element property</param>
        /// <param name="arrayProperty">The array property for which the elements are to be drawn</param>
        /// <param name="arrayMember">The member for the array property</param>
        /// <param name="wantsToDelete">The action which is to be called if the element is to be removed</param>
        protected virtual void DrawArrayElement(SerializedProperty element, InspectorMember elementMember, SerializedProperty arrayProperty, InspectorMember arrayMember, bool drawFoldout, Action wantsToDelete)
        {
            drawFoldout = drawFoldout && element.propertyType == SerializedPropertyType.Generic;

            //Draw foldout area for the element 
            using (var horizontal = new EditorGUILayout.HorizontalScope())
            {
                GUILayoutOption[] guiLayoutOptions = new GUILayoutOption[] { GUILayout.Width(20), GUILayout.Height(18) };
                GUIContent removeButton = new(EditorGUIUtility.IconContent("d_winbtn_win_close@2x"))
                {
                    tooltip = "Removes the element from the list"
                };

                GUILayout.Button("∧", EditorStyles.miniButton, guiLayoutOptions);
                GUILayout.Button("∨", EditorStyles.miniButton, guiLayoutOptions);

                //If a foldout is to be drawn 
                if (drawFoldout)
                {
                    element.isExpanded = EditorGUILayout.Foldout(element.isExpanded, element.displayName, true);

                    if (GUILayout.Button(removeButton, guiLayoutOptions))
                    {
                        wantsToDelete?.Invoke();
                        horizontal.Dispose();
                        return;
                    }

                    horizontal.Dispose();
                    if (!element.isExpanded)
                        return;
                }

                //If any values were updated; Reinitialize the children
                if (DrawSerializedMembers(elementMember, true, !drawFoldout))
                    arrayMember.InitializeArray(target, serializedObject);

                if (!drawFoldout)
                {
                    if (GUILayout.Button(removeButton, guiLayoutOptions))
                    {
                        wantsToDelete?.Invoke();
                        horizontal.Dispose();
                    }
                }
            }
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
