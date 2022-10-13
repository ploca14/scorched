using EasyBuildSystem.Features.Scripts.Core.Base.Builder;
using UnityEngine;
using UnityEngine.UI;

public class Demo_UI_CurrentSelection : MonoBehaviour
{
    #region Public Fields

    public string TextFormat = "Current Selection : {0}";

    #endregion

    #region Private Fields

    private Text Text;

    #endregion

    #region Private Methods

    private void Start()
    {
        Text = GetComponent<Text>();
    }

    private void Update()
    {
        if (BuilderBehaviour.Instance == null)
            return;

        if (BuilderBehaviour.Instance.SelectedPiece == null)
            return;

        Text.text = string.Format(TextFormat, BuilderBehaviour.Instance.SelectedPiece.Name);
    }

    #endregion
}