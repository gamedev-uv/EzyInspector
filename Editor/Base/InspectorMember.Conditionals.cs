namespace UV.EzyInspector.Editors
{
    public partial class InspectorMember
    {
        /// <summary>
        /// Whether the member is dependent on a show if member 
        /// </summary>
        public bool IsShowIfDependent { get; private set; }

        /// <summary>
        /// The show if attribute on the member
        /// </summary>
        public ShowIfAttribute ShowIfInstance { get; private set; }

        /// <summary>
        /// The member whose value is to be compared 
        /// </summary>
        public InspectorMember ShowIfMember { get; private set; }

        /// <summary>
        /// Initialized the info about the show if attribute if present on the member
        /// </summary>
        /// <param name="rootMember">The root of the member</param>
        public void InitializeShowIfMember(InspectorMember rootMember)
        {
            if (!TryGetAttribute(out ShowIfAttribute showIf)) return;
            ShowIfInstance = showIf;
            ShowIfMember = rootMember.FindMember<InspectorMember>(showIf.PropertyName, true);
            IsShowIfDependent = ShowIfMember != null;
        }
    }
}
