using UnityEngine;
using UV.EzyInspector;

public class ToggleButtonExample : MonoBehaviour
{
    [SerializeField, ToggleButton("[ON] Click to turn off", "[OFF] Click to turn on")]
    private bool _toggleExample;
}
