using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Demo_UI_CurrentObjectsCount : MonoBehaviour
{
	void Update () {
        GetComponent<Text>().text = "Log(s) : " + PickableController.Instance.GetCurrentLogCount() + "/" + PickableController.Instance.Slots.Count;
    }
}
