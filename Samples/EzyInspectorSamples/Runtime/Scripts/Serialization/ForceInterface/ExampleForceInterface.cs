using UnityEngine;
using UV.EzyInspector;

public interface IMyInterface { }

public class ExampleForceInterface : MonoBehaviour
{
    [SerializeField]
    [ForceInterface(typeof(IMyInterface))]
    private Object _obj;
}