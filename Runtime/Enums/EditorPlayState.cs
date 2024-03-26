namespace UV.BetterInspector
{
    /// <summary>
    /// The state of the game in the editor
    /// </summary>
    public enum EditorPlayState 
    {
        /// <summary>
        /// When the game is not running 
        /// </summary>
        NotPlaying = 0,

        /// <summary>
        /// When the game is running
        /// </summary>
        Playing = 1,

        /// <summary>
        /// Whenever the editor is focused
        /// </summary>
        Always = 2,
    }
}
