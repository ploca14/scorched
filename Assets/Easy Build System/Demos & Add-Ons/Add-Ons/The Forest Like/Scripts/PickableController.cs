using System.Collections.Generic;
using UnityEngine;
#if EBS_NEW_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
[System.Serializable]
public class LogSlot
{
    public GameObject Object;

    [HideInInspector]
    public bool IsAvailable;

    public void Show()
    {
        Object.gameObject.SetActive(true);
    }

    public void Hide()
    {
        Object.gameObject.SetActive(false);
    }
}

public class PickableController : MonoBehaviour
{
    public static PickableController Instance;

    public List<LogSlot> Slots = new List<LogSlot>();

    public GameObject DroppableObject;

    public List<string> TempElements = new List<string>();

    private void Awake()
    {
        Instance = this;

        for (int i = 0; i < Slots.Count; i++)
            Slots[i].Hide();
    }

    private void Update()
    {
#if EBS_NEW_INPUT_SYSTEM
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
#else
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
#endif
            for (int i = Slots.Count - 1; i >= 0; i--)
            {
                if (Slots[i].IsAvailable)
                {
                    Slots[i].IsAvailable = false;

                    Slots[i].Hide();

                    TempElements.RemoveAt(i);

                    Rigidbody Log = Instantiate(DroppableObject, transform.position + transform.forward * 2f, transform.rotation).GetComponent<Rigidbody>();

                    Log.AddForce(transform.forward * 100f, ForceMode.Impulse);

                    break;
                }
            }
        }
    }

    public void Pick(GameObject obj)
    {
        for (int i = 0; i < Slots.Count; i++)
        {
            if (!Slots[i].IsAvailable)
            {
                Slots[i].IsAvailable = true;

                Slots[i].Show();

                TempElements.Add(obj.tag);

                Destroy(obj);

                break;
            }
        }
    }

    public int GetCurrentLogCount()
    {
        int count = 0;

        for (int i = 0; i < Slots.Count; i++)
            if (Slots[i].IsAvailable)
                count++;

        return count;
    }
}