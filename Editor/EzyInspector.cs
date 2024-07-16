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
            DrawSerializedMembers();

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

            GUILayout.BeginHorizontal();

            //Draw the open script button
            var openGUI = EditorGUIUtility.IconContent("d_boo Script Icon");
            openGUI.text = "Open Script";
            openGUI.tooltip = "Opens the script in the specified External Script Editor";
            DrawButton(openGUI,
            () =>
            {
                AssetDatabase.OpenAsset(TargetMono);
            },
            GUILayout.Height(EditorGUIUtility.singleLineHeight));

            //Draw the ping script button
            var pingGUI = EditorGUIUtility.IconContent("d_Folder Icon");
            pingGUI.tooltip = "Ping Script\nPings the script in the Project folder";
            DrawButton(pingGUI, () =>
            {
                EditorUtility.FocusProjectWindow();
                EditorGUIUtility.PingObject(TargetMono);
            },
            GUILayout.Height(EditorGUIUtility.singleLineHeight),
            GUILayout.Width(EditorGUIUtility.singleLineHeight * 2));

            //Draw the select script button
            var selectGUI = EditorGUIUtility.IconContent("d_Selectable Icon");
            selectGUI.tooltip = "Select Script\nSelect the given script";
            DrawButton(selectGUI, () =>
            {
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = TargetMono;
                EditorGUIUtility.PingObject(TargetMono);
            },
            GUILayout.Height(EditorGUIUtility.singleLineHeight),
            GUILayout.Width(EditorGUIUtility.singleLineHeight * 2));

            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }

        /// <summary>
        /// Draws a button on the inspector with the given gui content 
        /// </summary>
        /// <param name="buttonGUI">The gui content for the button</param>
        /// <param name="onButtonPressed">The action which is to be called when the button is pressed</param>
        /// <param name="layoutOptions">The GUILayoutOptions for the button</param>
        protected virtual void DrawButton(GUIContent buttonGUI, System.Action onButtonPressed = null, params GUILayoutOption[] layoutOptions)
        {
            if (GUILayout.Button(buttonGUI, layoutOptions))
                onButtonPressed?.Invoke();
        }

        /// <summary>
        /// Draws all the serialized memebers 
        /// </summary>
        protected virtual void DrawSerializedMembers()
        {
            if (DrawableMembers == null || DrawableMembers.Length == 0) return;

            var indent = EditorGUI.indentLevel;
            var guiState = GUI.enabled;

            for (int i = 0; i < DrawableMembers.Length; i++)
            {
                var member = DrawableMembers[i];

                //Change indent level and readonly based on the parent 
                EditorGUI.indentLevel = indent + member.Depth;
                GUI.enabled = guiState;

                var memberInfo = member.MemberInfo;
                if (!member.IsParentExpanded()) continue;

                //Try finding the property
                SerializedProperty property = member.MemberProperty;

                //Check if member has ShowIfAttribute attribute and draw it accordingly 
                if (member.TryGetAttribute(out ShowIfAttribute showIf))
                {
                    var canShow = CorrectShowIfValue(member, showIf);

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
                DrawMember(member);
            }
        }

        /// <summary>
        /// Draws the given member
        /// </summary>
        /// <param name="member">The member which is to be drawn</param>
        public virtual void DrawMember(InspectorMember member)
        {
            //Check whether the member has a SerializedProperty associated with it
            var property = member.MemberProperty;
            if (property == null) return;

            //Disable it if needed
            EditorGUI.BeginDisabledGroup(member.IsReadOnly || member.HasAttribute<ReadOnlyAttribute>());
            EditorGUILayout.PropertyField(property, property.isArray);
            EditorGUI.EndDisabledGroup();
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual bool CorrectShowIfValue(InspectorMember member, ShowIfAttribute showIf)
        {
            //Find finding the showIfMember
            var showIfMember = RootMember.FindMember<InspectorMember>(showIf.PropertyName, true);

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
            if (value == null) return showIf.TargetValues == null || showIf.TargetValues.Length == 0;

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
            DrawButton(new(buttonName), () =>
            {
                method?.Invoke(parent, null);
                EditorUtility.SetDirty(this);
            });
            GUILayout.Space(5);
        }
    }
}
