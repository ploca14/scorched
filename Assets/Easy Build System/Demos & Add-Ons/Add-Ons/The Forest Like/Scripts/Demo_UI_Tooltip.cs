using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Demo_UI_Tooltip : MonoBehaviour
{
    public static Demo_UI_Tooltip Instance;

    public Transform Content;
    public Text Message;

    private void Awake()
    {
        Instance = this;
    }

    public void Show(Vector3 point, string message)
    {
        if (Vector3.Distance(transform.position, point) < 2f)
            transform.position = Vector3.Lerp(transform.position, point, 10f * Time.deltaTime);
        else
            transform.position = point;

        Content.gameObject.SetActive(true);

        Message.text = message;
    }

    public void Hide()
    {
        if (Content == null) return;

        Content.gameObject.SetActive(false);
    }
}
