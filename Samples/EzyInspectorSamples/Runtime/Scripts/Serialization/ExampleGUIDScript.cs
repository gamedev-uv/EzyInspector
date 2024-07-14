using UnityEngine;
using UV.EzyInspector;

[CreateAssetMenu(menuName = "UV/EzyInspector/Samples/ExampleSO", fileName = "Example So")]
public class ExampleGUIDScript : ScriptableObject
{
    [SerializeField, GUID] private string _objectGUID;
}