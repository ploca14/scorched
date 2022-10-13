using UnityEngine;

[ExecuteInEditMode]
public class Demo_Billboard : MonoBehaviour
{
    #region Private Methods

    private void Update()
    {
        Camera CurrentCamera = Camera.current != null ? Camera.current : Camera.main;

        if (CurrentCamera == null)
            return;

        transform.LookAt(transform.position + CurrentCamera.transform.rotation * Vector3.forward,
            CurrentCamera.transform.rotation * Vector3.up);
    }

    #endregion
}
