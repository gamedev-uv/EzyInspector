using UnityEngine;
using UV.EzyInspector;

public class ExampleTagSelector : MonoBehaviour
{
    [SerializeField, TagSelector] private string _exampleTag;
}
