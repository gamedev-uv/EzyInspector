using System;

namespace UV.EzyInspector
{
    /// <summary>
    /// Draws a label with the value of member
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class Label : SerializeMemberAttribute
    {
        public Label(string formattedString = "{0} : {1}")
        {
            FormattedString = formattedString;
        }

        /// <summary>
        /// The formatted label text 
        /// </summary>
        public string FormattedString { get; private set; } = "{0} : {1}";
    }
}
