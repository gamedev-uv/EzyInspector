using UnityEngine;
using UV.EzyInspector;

public class ReadOnlyExample : MonoBehaviour
{
    [SerializeField, ReadOnly] private int _readOnlyInt = 10;
}
