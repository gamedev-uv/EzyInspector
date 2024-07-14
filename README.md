![](README/header.png)

# EzyInspector [![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)](https://github.com/gamedev-uv/EzyInspector/blob/main/LICENSE)

**Makes working and customizing the inspector easier!**

## Table of Contents

- [Installation](#installation)
- [Dependencies](#dependencies)
- [Attributes](#attributes)
  - [Formatting](#formatting)
    - [Button](#button)
    - [Label](#label)
    - [ReadOnly](#readonly)
    - [HideMonoScript](#hidemonoscript)
  - [Serialization](#serialization)
    - [GUID](#guid)
    - [ForceInterface](#forceinterface)
  - [Conditional](#conditional)
    - [EditModeOnly](#editmodeonly)
    - [ShowIf](#showif)
  - [Callback](#callback)
    - [OnInspectorUpdated](#oninspectorupdated)

# 💿 Installation
:warning: This package requires the [**EzyReflection**](https://github.com/gamedev-uv/EzyReflection) package in order to function. Make sure you install that package before installing this one.

Through the [**Unity Package Manager**](https://docs.unity3d.com/Manual/upm-ui-giturl.html) using the following Git URLs:
```
https://github.com/gamedev-uv/EzyReflection.git
```

```
https://github.com/gamedev-uv/EzyInspector.git
```

# Dependencies 
 - [**EzyReflection**](https://github.com/gamedev-uv/EzyReflection)


# Attributes

## Formatting

Attributes that affect how data is displayed or formatted in the inspector.

#### Button

Used to draw buttons in the Unity inspector. This attribute allows methods to be displayed as buttons in the Unity editor interface.

**`ButtonAttribute(string buttonName)`**
   Creates a button with a specified display name. By default, the button is drawn after the default editor elements.
   - **Parameters:**
     - `buttonName` (string): The display name of the button.

**`ButtonAttribute(EditorDrawSequence editorDrawSequence)`**
   Creates a button without a display name, specifying the sequence in which it should be drawn relative to other editor elements.
   - **Parameters:**
     - `editorDrawSequence` (EditorDrawSequence): The target draw sequence of the button.
       - `EditorDrawSequence.BeforeDefaultEditor`: Draws the button before the default editor elements.
       - `EditorDrawSequence.AfterDefaultEditor`: Draws the button after the default editor elements.

**`ButtonAttribute(string buttonName, EditorDrawSequence editorDrawSequence)`**
   Creates a button with a specified display name and draw sequence.
   - **Parameters:**
     - `buttonName` (string): The display name of the button.
     - `editorDrawSequence` (EditorDrawSequence): The target draw sequence of the button.

```cs
using UnityEngine;
using UV.EzyInspector;

public class ExampleButtonScript : MonoBehaviour
{
    [Button("Custom Button")]
    private void CustomButton()
    {
        Debug.Log("Custom button clicked!");
    }

    [Button(EditorDrawSequence.BeforeDefaultEditor)]
    private void ButtonBeforeOther()
    {
        Debug.Log("Button drawn before default editor elements!");
    }

    [Button("Sequence Button", EditorDrawSequence.AfterDefaultEditor)]
    private void ButtonWithSequence()
    {
        Debug.Log("Button drawn after default editor elements!");
    }
}
```

#### Label

Draws a label with the value of member in the Unity inspector.

**`Label(string formattedString = "{0} : {1}")`**
   Draws a label with a formatted string displaying the member's name and value.
   - **Parameters:**
     - `formattedString` (string): The format string for displaying the label text.

```cs
using UnityEngine;
using UV.EzyInspector;

public class LabelExample : MonoBehaviour
{
    [SerializeField]
    [Label("Current Health: {0}")]
    private int health = 100;

    [SerializeField]
    [Label("{0} Value: {1}")]
    private float floatValue = 3.5f;

    [SerializeField]
    [Label("{0} is {1}")]
    private string playerName = "Player1";
}
```

#### ReadOnly

Makes the member readonly in the inspector.

```cs
using UnityEngine;
using UV.EzyInspector;

public class ReadOnlyExample : MonoBehaviour
{
    [SerializeField, ReadOnly] private int readOnlyInt = 10;
}
```

#### HideMonoScript

Hides the open script UI from the inspector.

```cs
using UnityEngine;
using UV.EzyInspector;

[HideMonoScript]
public class ExampleHideMonoScript : MonoBehaviour
{
        
}
```

## Serialization

Attributes that affect how data is serialized or represented in Unity.

#### GUID

Saves the Unity GUID of the current target object to a given string.

```cs
using UnityEngine;
using UV.EzyInspector;

public class ExampleGUIDScript : MonoBehaviour
{
    [SerializeField, GUID] private string objectGUID; 
}
```

#### ForceInterface

Force draws a specified interface in the Unity inspector for a field.

**`ForceInterfaceAttribute(Type interfaceType)`**
   Specifies the type of interface to be drawn in the inspector for the field.
   - **Parameters:**
     - `interfaceType` (Type): The type of interface to draw.

```cs
using UnityEngine;
using UV.EzyInspector;

public interface IMyInterface {}

public class ExampleForceInterface : MonoBehaviour
{
    [SerializeField]
    [ForceInterface(typeof(IMyInterface))]
    private Object obj; 
}
```

## Conditional

Attributes that conditionally display or hide inspector elements.

#### EditModeOnly

Hides the member when not in edit mode.

**`EditModeOnly()`**
   Hides the member when not in edit mode.

**`EditModeOnly(HideMode hideMode)`**
   Hides or makes the member readonly when not in edit mode based on the `hideMode` parameter.
   - **Parameters:**
     - `hideMode` (HideMode): Specifies whether the member is to be hidden or made readonly.

```cs
using UnityEngine;
using UV.EzyInspector;

public class ExampleEditModeScript : MonoBehaviour
{
    [SerializeField]
    private int normalVariable = 10;

    [SerializeField, EditModeOnly]
    private int hiddenInPlayMode = 20;

    [SerializeField, EditModeOnly(HideMode.ReadOnly)]
    private string readOnlyInPlayMode = "You can't edit this in play mode!";
}
```

#### ShowIf

Only displays a property based on the condition passed.

**`ShowIfAttribute(string propertyName, params object[] targetValues)`**
   Displays the property based on the specified property name and target values.

**`ShowIfAttribute(string propertyName, HideMode hideMode, params object[] targetValue)`**
   Displays the property based on the specified property name, target values, and hide mode.
   - **Parameters:**
     - `propertyName` (string): The name of the property which needs to be used.
     - `targetValues` (object[]): The target values of the property which determine when the property is shown.
     - `hideMode` (HideMode): The hide mode of the property when the condition is not met.

```cs
using UnityEngine;
using UV.EzyInspector;

public class ShowIfExample : MonoBehaviour
{
    [SerializeField]
    private bool showProperty = true;

    [SerializeField, ShowIf(nameof(showProperty))]
    private int conditionalProperty = 50;

    [SerializeField, ShowIf(nameof(showProperty), HideMode.ReadOnly, false)]
    private string readOnlyProperty = "Read Only when showProperty is false!";
}
```

## Callback

Attributes that trigger callbacks based on editor events.

#### OnInspectorUpdated

Calls the method when the inspector of the object is updated.

**`OnInspectorUpdatedAttribute()`**
   Invokes the method whenever the inspector is updated, regardless of the editor play state.
   
**`OnInspectorUpdatedAttribute(EditorPlayState editorGameState)`**
   Invokes the method when the inspector is updated, but only during the specified `editorGameState`:
   - **Parameters:**
     - `editorGameState` (EditorPlayState): Specifies when the method should be called.
       - `EditorPlayState.Always`: Calls the method in all states (default behavior).
       - `EditorPlayState.Playing`: Calls the method only during play mode.
       - `EditorPlayState.NotPlaying`: Calls the method only during edit mode.

```cs
using UnityEngine;
using UV.EzyInspector;

public class ExampleScript : MonoBehaviour
{
    [OnInspectorUpdated]
    private void OnInspectorUpdate()
    {
        Debug.Log("Inspector updated!");
    }

    [OnInspectorUpdated(EditorPlayState.Playing)]
    private void OnInspectorUpdatePlaying()
    {
        Debug.Log("Inspector updated during play mode!");
    }

    [OnInspectorUpdated(EditorPlayState.NotPlaying)]
    private void OnInspectorUpdateNotPlaying()
    {
        Debug.Log("Inspector updated during edit mode!");
    }
}
```
