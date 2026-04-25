using UnityEngine;

public class HelpManager : MonoBehaviour
{
    [SerializeField] private GameObject helpPanel;
    [SerializeField] private GameObject closeArea;

    private void Start()
    {
        closeArea.SetActive(false);
    }

    public void ToggleHelp()
    {
        bool next = !helpPanel.activeSelf;
        helpPanel.SetActive(next);
        closeArea.SetActive(next);
    }

    public void CloseInfo()
    {
        helpPanel.SetActive(false);
        closeArea.SetActive(false);
    }
}
