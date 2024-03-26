namespace UV.BetterInspector
{
    /// <summary>
    /// Used to define the draw sequence of the editor 
    /// </summary>
    public enum EditorDrawSequence 
    {
        /// <summary>
        /// Draws the custom editor before the default editor 
        /// </summary>
        BeforeDefaultEditor = 0,

        /// <summary>
        /// Draws the custom editor after the default editor 
        /// </summary>
        AfterDefaultEditor = 1, 
    }
}
