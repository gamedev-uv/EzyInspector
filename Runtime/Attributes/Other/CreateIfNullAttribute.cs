using UnityEngine;

namespace UV.EzyInspector
{
    public class CreateIfNullAttribute : PropertyAttribute 
    {
        public CreateIfNullAttribute(string buttonDisplayName = "Create")
        {
            ButtonDisplayName = buttonDisplayName;
        }

        public string ButtonDisplayName { get; private set; }
    }
}