using EasyBuildSystem.Features.Scripts.Core.Base.Builder;
using UnityEngine;
using UnityEngine.UI;

public class Demo_UI_CurrentMode : MonoBehaviour
{
    #region Public Fields

    public string TextFormat = "Current Mode : {0}";

    #endregion

    #region Private Fields

    private Text Text;

    #endregion

    #region Private Methods

    private void Start ()
    {
        Text = GetComponent<Text>();
    }

    private void Update ()
    {
        if (BuilderBehaviour.Instance == null)
            return;

        if (BuilderBehaviour.Instance.SelectedPiece == null)
            return;

        Text.text = string.Format(TextFormat, BuilderBehaviour.Instance.CurrentMode.ToString());
    }

    #endregion
}