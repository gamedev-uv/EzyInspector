using System;
using UnityEngine;

namespace UV.EzyInspector
{
    /// <summary>
    /// Calls the method when the transform of the object is updated
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class OnTransformUpdatedAttribute : Attribute
    {
        /// <summary>
        /// The state when the method is called if the transform is updated
        /// </summary>
        private readonly EditorPlayState _updateEditorPlayState;

        /// <summary>
        /// Invokes the method when the inspector is updated
        /// </summary>
        public OnTransformUpdatedAttribute()
        {
            _updateEditorPlayState = EditorPlayState.Always;
        }

        /// <summary>
        /// Invokes the method when the transform is updated
        /// </summary>
        /// <param name="editorGameState">The game state to invoke the methods in</param>
        public OnTransformUpdatedAttribute(EditorPlayState editorGameState = EditorPlayState.Always)
        {
            _updateEditorPlayState = editorGameState;
        }

        /// <summary>
        /// Whether the editor game state is in the correct state for the method to be called 
        /// </summary>
        public bool IsCorrectEditorPlayerState()
        {
            if (_updateEditorPlayState.Equals(EditorPlayState.Always)) return true;
            if (Application.isPlaying && _updateEditorPlayState.Equals(EditorPlayState.Playing)) return true;
            if (!Application.isPlaying && _updateEditorPlayState.Equals(EditorPlayState.NotPlaying)) return true;
            return false;
        }
    }
}
