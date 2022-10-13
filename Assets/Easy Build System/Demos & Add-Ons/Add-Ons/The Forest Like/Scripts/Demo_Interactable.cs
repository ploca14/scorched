using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo_Interactable : MonoBehaviour
{
    public virtual string InteractionMessage { get; }

    public virtual void Interaction() { }

    public virtual void Show(Vector3 point) { }

    public virtual void Hide() { }
}
