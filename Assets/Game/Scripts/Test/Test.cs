using StarForce;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public Button button;

    public void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(Onclick);
    }

    public void Onclick()
    {
        GameEntry.UI.OpenUIForm(UIFormId.GameMenuForm);
    }
    public void OnDestroy()
    {
        button.onClick.RemoveListener(Onclick);
    }
}
