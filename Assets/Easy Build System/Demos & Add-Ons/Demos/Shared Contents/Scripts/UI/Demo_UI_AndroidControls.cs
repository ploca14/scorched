using EasyBuildSystem.Features.Scripts.Core.Base.Builder;
using EasyBuildSystem.Features.Scripts.Core.Base.Builder.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
using UnityEngine;
using UnityEngine.UI;

public class Demo_UI_AndroidControls : MonoBehaviour
{
    public GameObject BuildContent;
    public Button BuildButton;
    public Button DestructionButton;
    public Button ValidateButton;
    public Button CancelButton;
    public Button RotateButton;
    public Button RightButton;
    public Button LeftButton;

    private int SelectedIndex;

    private void Start()
    {
        BuildButton.onClick.AddListener(() => 
        {
            BuilderBehaviour.Instance.ChangeMode(BuildMode.Placement);

            BuildContent.SetActive(true);
        });

        DestructionButton.onClick.AddListener(() =>
        {
            BuilderBehaviour.Instance.ChangeMode(BuildMode.Destruction);

            BuildContent.SetActive(true);
        });

        ValidateButton.onClick.AddListener(() =>
        {
            if (BuilderBehaviour.Instance.CurrentMode == BuildMode.Placement)
                BuilderBehaviour.Instance.PlacePrefab(BuilderBehaviour.Instance.NearestGroup);
            else if (BuilderBehaviour.Instance.CurrentMode == BuildMode.Destruction)
                BuilderBehaviour.Instance.DestroyPrefab();
            else if (BuilderBehaviour.Instance.CurrentMode == BuildMode.Edit)
                BuilderBehaviour.Instance.EditPrefab();
        });

        CancelButton.onClick.AddListener(() =>
        {
            BuilderBehaviour.Instance.ChangeMode(BuildMode.None);
        });

        RotateButton.onClick.AddListener(() =>
        {
            if (BuilderBehaviour.Instance.SelectedPiece != null)
                BuilderBehaviour.Instance.RotatePreview(BuilderBehaviour.Instance.SelectedPiece.RotationAxis);
        });

        RightButton.onClick.AddListener(() =>
        {
            if (SelectedIndex < BuildManager.Instance.Pieces.Count - 1)
                SelectedIndex++;
            else
                SelectedIndex = 0;

            BuilderBehaviour.Instance.ChangeMode(BuildMode.None);
            BuilderBehaviour.Instance.SelectPrefab(BuildManager.Instance.Pieces[SelectedIndex]);
            BuilderBehaviour.Instance.ChangeMode(BuildMode.Placement);
        });

        LeftButton.onClick.AddListener(() => 
        {
            if (SelectedIndex > 0)
                SelectedIndex--;
            else
                SelectedIndex = BuildManager.Instance.Pieces.Count - 1;

            BuilderBehaviour.Instance.ChangeMode(BuildMode.None);
            BuilderBehaviour.Instance.SelectPrefab(BuildManager.Instance.Pieces[SelectedIndex]);
            BuilderBehaviour.Instance.ChangeMode(BuildMode.Placement);
        });

        BuilderBehaviour.Instance.ChangeMode(BuildMode.None);
        BuilderBehaviour.Instance.SelectPrefab(BuildManager.Instance.Pieces[0]);
        BuilderBehaviour.Instance.ChangeMode(BuildMode.Placement);
    }

    private void Update()
    {
        if (BuilderBehaviour.Instance != null)
        {
            RotateButton.gameObject.SetActive(BuilderBehaviour.Instance.CurrentMode == BuildMode.Placement);
            BuildContent.SetActive(BuilderBehaviour.Instance.CurrentMode != BuildMode.None);
        }
    }
}
