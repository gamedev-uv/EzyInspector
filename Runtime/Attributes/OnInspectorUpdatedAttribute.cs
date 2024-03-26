using System;

namespace UV.BetterInspector
{
    using UnityEngine;

    /// <summary>
    /// Calls the method when the inspector of the object is updated
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class OnInspectorUpdatedAttribute : Attribute
    {
        /// <summary>
        /// The state when the method is called if the inspector is updated
        /// </summary>
        private EditorPlayState _updateEditorPlayState;

        /// <summary>
        /// Invokes the method when the inspector is updated
        /// </summary>
        public OnInspectorUpdatedAttribute()
        {
            _updateEditorPlayState = EditorPlayState.Always;
        }

        /// <summary>
        /// Invokes the method when the inspector is updated
        /// </summary>
        /// <param name="editorGameState">The game state to invoke the methods in</param>
        public OnInspectorUpdatedAttribute(EditorPlayState editorGameState = EditorPlayState.Always)
        {
            _updateEditorPlayState = editorGameState;
        }

        /// <summary>
        /// Whether the editor game state is in the correct state for the method to be called 
        /// </summary>
        public bool IsCorrectEditorPlayerState()
        {
            if (_updateEditorPlayState.Equals(EditorPlayState.Always)) return true;
            if(Application.isPlaying && _updateEditorPlayState.Equals(EditorPlayState.Playing)) return true;
            if(!Application.isPlaying && _updateEditorPlayState.Equals(EditorPlayState.NotPlaying)) return true;
            return false;
        }
    }
}
