using EasyBuildSystem.Features.Scripts.Core.Base.Builder;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
using UnityEngine;
using UnityEngine.UI;

public class Demo_UI_Pieces_Selection : MonoBehaviour
{
    public Button ButtonTemplate;
    public Transform Container;

    private void Start()
    {
        for (int i = 0; i < BuildManager.Instance.Pieces.Count; i++)
        {
            if (BuildManager.Instance.Pieces[i] == null) continue;

            GameObject Button = (GameObject)Instantiate(ButtonTemplate.gameObject, Container);
            Button.SetActive(true);

            int Index = i;
            Button.GetComponent<Button>().onClick.AddListener(() =>
            {
                BuilderBehaviour.Instance.ChangeMode(EasyBuildSystem.Features.Scripts.Core.Base.Builder.Enums.BuildMode.None);
                BuilderBehaviour.Instance.SelectPrefab(BuildManager.Instance.Pieces[Index]);
                BuilderBehaviour.Instance.ChangeMode(EasyBuildSystem.Features.Scripts.Core.Base.Builder.Enums.BuildMode.Placement);
            });

            Button.transform.GetChild(0).GetComponent<Image>().sprite = BuildManager.Instance.Pieces[i].Icon;
            Button.transform.GetChild(0).GetComponent<Image>().preserveAspect = true;

            Button.transform.GetChild(1).GetComponent<Text>().text = BuildManager.Instance.Pieces[i].Name;
        }
    }
}