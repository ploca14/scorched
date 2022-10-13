using UnityEngine;

public class Demo_TopDown_Camera : MonoBehaviour
{
    #region Public Fields

    public Transform Target;

    #endregion

    #region Private Fields

    private Vector3 InitalOffset;

    #endregion

    #region Private Methods

    private void Start ()
    {
        InitalOffset = transform.position;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update ()
    {
        if (Target == null)
            return;

        transform.position = Vector3.Lerp(transform.position, InitalOffset + Target.position, 5f * Time.deltaTime);
	}

    #endregion
}