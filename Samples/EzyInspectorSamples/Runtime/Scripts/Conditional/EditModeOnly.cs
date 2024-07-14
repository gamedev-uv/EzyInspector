using UnityEngine;
using UV.EzyInspector;

public class ExampleEditModeScript : MonoBehaviour
{
    [SerializeField]
    private int _normalVariable = 10;

    [SerializeField, EditModeOnly]
    private int _hiddenInPlayMode = 20;

    [SerializeField, EditModeOnly(HideMode.ReadOnly)]
    private string _readOnlyInPlayMode = "You can't edit this in play mode!";
}