using UnityEngine;
using UV.EzyInspector;

public class CallBacks : MonoBehaviour
{
    [OnInspectorUpdated]
    private void OnInspectorUpdate()
    {
        Debug.Log("Inspector updated!");
    }

    [OnInspectorUpdated(EditorPlayState.Playing)]
    private void OnInspectorUpdatePlaying()
    {
        Debug.Log("Inspector updated during play mode!");
    }

    [OnInspectorUpdated(EditorPlayState.NotPlaying)]
    private void OnInspectorUpdateNotPlaying()
    {
        Debug.Log("Inspector updated during edit mode!");
    }
}
