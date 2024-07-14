using UnityEngine;
using UV.EzyInspector;

public class LabelExample : MonoBehaviour
{
    [SerializeField] private int _age = 18;

    [DisplayAsLabel("Qualified : {1}")] private bool _isQualified => _age >= 18;
}