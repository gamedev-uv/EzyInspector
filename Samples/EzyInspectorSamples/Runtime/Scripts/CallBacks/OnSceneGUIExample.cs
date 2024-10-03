using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UV.EzyInspector;
#endif

public class OnSceneGUIExample : MonoBehaviour
{
#if UNITY_EDITOR
    [OnSceneGUI]
    private void DrawHandles()
    {
        // Draw a solid disc at the object's position
        Handles.color = Color.black;
        Handles.DrawSolidDisc(transform.position, transform.up, 2);

        // Display a label at the object's position
        Handles.Label(transform.position, "That's all it takes to draw handles!");
    }
#endif
}