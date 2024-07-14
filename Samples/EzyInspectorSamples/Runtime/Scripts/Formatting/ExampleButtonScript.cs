using UnityEngine;
using UV.EzyInspector;

public class ExampleButtonScript : MonoBehaviour
{
    [Button("Custom Button")]
    private void CustomButton()
    {
        Debug.Log("Custom button clicked!");
    }
}

