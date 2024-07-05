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
        public MonoScript TargetMonoScript { get; protected set; }

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
            HideMonoScript = RootMember.HasAttribute<HideMonoScriptAttribute>();
            if (HideMonoScript) return;

            //Find the mono script for the open script button
            if (target is MonoBehaviour mono)
                TargetMonoScript = MonoScript.FromMonoBehaviour(mono);
            else if (target is ScriptableObject so)
                TargetMonoScript = MonoScript.FromScriptableObject(so);
        }

        public override void OnInspectorGUI()
        {
            DrawOpenScriptUI();
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
        /// Draws a button to open the script in the specified editor
        /// </summary>
        protected virtual void DrawOpenScriptUI()
        {
            if (HideMonoScript) return;
            if (TargetMonoScript == null) return;

            if (GUILayout.Button("Open Script"))
                AssetDatabase.OpenAsset(TargetMonoScript);
            GUILayout.Space(5);
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

                //Draw a button for the method
                if (member.TryGetAttribute(out ButtonAttribute button))
                {
                    DrawButton(memberInfo as MethodInfo, button);
                    continue;
                }

                //Check whether it has the hide in inspector if it does hide it 
                if (member.IsMemberHidden() || member.HasAttribute<HideInInspector>())
                    continue;


                //If it is a label
                if (member.TryGetAttribute(out Label label))
                    GUILayout.Label(label.FormattedString
                                                        .Replace("{0}", member.Name)
                                                        .Replace("{1}", $"{member.GetValue()}"));

                //If the member has the EditMode Attribute draw it accordingly 
                if (member.TryGetAttribute(out EditModeOnly editMode) && Application.isPlaying)
                {
                    if (editMode.HideMode == HideMode.Hide) continue;
                    DrawReadOnly(property);
                    continue;
                }

                //Draw readonly members
                if (member.IsReadOnly || member.HasAttribute<ReadOnlyAttribute>())
                {
                    DrawReadOnly(property);
                    continue;
                }

                //Draw the member normally if it has a SerializedProperty for it
                if (property == null) continue;
                EditorGUILayout.PropertyField(property, property.isArray);
            }
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
        /// Draws the readonly property drawer for the property
        /// </summary>
        /// <param propertyPath="property">The property to be drawn</param>
        protected virtual void DrawReadOnly(SerializedProperty property)
        {
            if (property == null) return;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(property, property.isArray);
            EditorGUI.EndDisabledGroup();
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draws a button in the inspector for the given method styled by the button attribute
        /// </summary>
        /// <param propertyPath="method">The method to be drawn</param>
        /// <param propertyPath="button">The button used to style the ui</param>
        protected virtual void DrawButton(MethodInfo method, ButtonAttribute button)
        {
            string buttonName = button.DisplayName ?? method.Name;
            if (GUILayout.Button(buttonName))
            {
                method?.Invoke(target, null);
                EditorUtility.SetDirty(this);
            }

            GUILayout.Space(5);
        }
    }
}
