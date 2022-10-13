using System;
using UnityEngine;

[Serializable]
public class Demo_MouseLook
{
    #region Public Fields

    public float XSensitivity = 2f;
    public float YSensitivity = 2f;
    public bool ClampVerticalRotation = true;
    public float MinimumX = -90F;
    public float MaximumX = 90F;
    public bool Smooth;
    public float SmoothTime = 5f;
    public bool LockCursor = true;

    #endregion Public Fields

    #region Private Fields

    private Quaternion CharacterTargetRot;
    private Quaternion CameraTargetRot;

    #endregion Private Fields

    #region Public Methods

    public void Init(Transform character, Transform camera)
    {
        CharacterTargetRot = character.localRotation;
        CameraTargetRot = camera.localRotation;
    }

    public void LookRotation(Transform character, Transform camera)
    {
#if EBS_NEW_INPUT_SYSTEM
        float yRot = Demo_InputHandler.Instance.player.LookX.ReadValue<float>() * XSensitivity;
        float xRot = Demo_InputHandler.Instance.player.LookY.ReadValue<float>() * YSensitivity;
#else
        float yRot = Input.GetAxis("Mouse X") * XSensitivity * 2;
        float xRot = Input.GetAxis("Mouse Y") * YSensitivity * 2;
#endif

        CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);

        CameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

        if (ClampVerticalRotation)
            CameraTargetRot = ClampRotationAroundXAxis(CameraTargetRot);

        if (Smooth)
        {
            character.localRotation = Quaternion.Slerp(character.localRotation, CharacterTargetRot,
                SmoothTime * Time.deltaTime);
            camera.localRotation = Quaternion.Slerp(camera.localRotation, CameraTargetRot,
                SmoothTime * Time.deltaTime);
        }
        else
        {
            character.localRotation = CharacterTargetRot;
            camera.localRotation = CameraTargetRot;
        }
    }

#endregion Public Methods

#region Private Methods

    private Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

        angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }

#endregion Private Methods
}