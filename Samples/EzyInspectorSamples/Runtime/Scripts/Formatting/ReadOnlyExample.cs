using UnityEngine;
using UV.EzyInspector;

public class ReadOnlyExample : MonoBehaviour
{
    [SerializeField, ReadOnly] private int _readOnlyInt = 10;
    [SerializeField, ReadOnly] private bool _readOnlyBool = false;
    [SerializeField, ReadOnly] private GameObject _readOnlyGameObject;
    [SerializeField, ReadOnly] private Transform[] _readOnlyTransforms;
}
