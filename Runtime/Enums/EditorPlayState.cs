namespace UV.BetterInspector.Enums
{
    /// <summary>
    /// The state of the game in the editor
    /// </summary>
    public enum EditorPlayState 
    {
        NotPlaying = 0,
        Playing = 2,
        Always = NotPlaying | Playing,
    }
}
