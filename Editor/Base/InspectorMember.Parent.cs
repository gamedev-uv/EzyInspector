namespace UV.EzyInspector.Editors
{
    public partial class InspectorMember
    {
        /// <summary>
        /// The parent member of the member
        /// </summary>
        public InspectorMember ParentMember { get; private set; }

        /// <summary>
        /// Whether the current member has a parent member
        /// </summary>
        /// <returns>Returns true or false based on the member has a parent or not</returns>
        public bool HasParent()
        {
            return ParentMember != null;
        }

        /// <summary>
        /// Whether the parent is expanded in the inspector 
        /// </summary>
        /// <returns>Returns true or false based on if the parent is expanded in the inspector</returns>
        public bool IsParentExpanded()
        {
            if (!HasParent()) return true;
            if (ParentMember.MemberProperty == null) return true;
            return ParentMember.MemberProperty.isExpanded;
        }
    }
}
