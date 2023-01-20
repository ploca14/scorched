using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InventoryOpener : MonoBehaviour
{
    public UnityEvent onInventoryOpen;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            onInventoryOpen.Invoke();
        }
    }
}
