using EasyBuildSystem.Features.Scripts.Core.Base.Event;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece;
using System;
using UnityEngine;
using UnityEngine.Events;
#if EBS_NEW_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Demo_Basic_Interaction : MonoBehaviour
{
    #region Public Fields

    [Header("Interaction Settings")]
    public float InteractionDistance = 3.0f;
    public GUIStyle Font;
    public LayerMask Layers;
    public bool ShowGUI;

    [Serializable] public class Interacted : UnityEvent<GameObject> { }
    public Interacted OnInteracted;

    #endregion

    #region Private Methods

    private Demo_Interactable LastInteractable;

    private void Start()
    {
        BuildEvent.Instance.OnPieceDestroyed.AddListener((PieceBehaviour piece) =>
        {
            if (LastInteractable != null)
                LastInteractable.Hide();
        });
    }

    bool showInteractGUI;
    private void OnGUI()
    {
        if (ShowGUI && showInteractGUI)
            GUI.Label(new Rect(Screen.width / 2, Screen.height / 2, 200, 30), "<size=18>Press F to interact.</size>");
    }

    private void Update()
    {
        Ray Ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        RaycastHit Hit;

        if (Physics.Raycast(Ray, out Hit, InteractionDistance, Layers))
        {
            if (Hit.collider.GetComponentInParent<Demo_Interactable>() && Hit.collider.GetComponentInParent<Demo_Interactable>().enabled)
            {
                LastInteractable = Hit.collider.GetComponentInParent<Demo_Interactable>();

                Hit.collider.GetComponentInParent<Demo_Interactable>().Show(Hit.point);

                showInteractGUI = true;

#if EBS_NEW_INPUT_SYSTEM
                if (Keyboard.current.fKey.wasPressedThisFrame)
                {
#else
                if (Input.GetKeyDown(KeyCode.F))
                {
#endif
                    OnInteracted.Invoke(Hit.collider.gameObject);

                    Hit.collider.GetComponentInParent<Demo_Interactable>().Interaction();

                    LastInteractable.Hide();
                }
            }
            else
            {
                if (LastInteractable != null)
                    LastInteractable.Hide();

                showInteractGUI = false;
            }
        }
        else
        {
            if (LastInteractable != null)
                LastInteractable.Hide();

            showInteractGUI = false;
        }
    }

    #endregion
}