using UnityEngine;
using UV.EzyInspector;

public class ShowIfExample : MonoBehaviour
{
    [SerializeField]
    private bool _showProperty = true;

    [SerializeField]
    [ShowIf(nameof(_showProperty), true)]
    private int _conditionalProperty = 50;

    [SerializeField]
    [ShowIf(nameof(_complexCondition), HideMode.ReadOnly, true)]
    private string _readOnlyProperty = "Read Only when _complexCondition is true!";

    private bool _complexCondition => _showProperty && _conditionalProperty > 50;

    [SerializeField, ShowIf("_conditionalProperty", 1)]
    private int _intDependent;

    [SerializeField, ShowIf("_conditionalProperty", 2, 5)]
    private int _multipleIntDependent;
}