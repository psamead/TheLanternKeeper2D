using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class EndCardController : MonoBehaviour
{
    [SerializeField] private StoryboardPlayer storyboard;
    [SerializeField] private GameObject buttonsRoot;
    [SerializeField] private TMP_Text backLabel;
    [SerializeField] private TMP_Text quitLabel;
    [SerializeField] private LanternFlowText flowText;

    private void Start()
    {
        if (backLabel && flowText) backLabel.text = flowText.backToMenu;
        if (quitLabel && flowText) quitLabel.text = flowText.quit;
        if (buttonsRoot) buttonsRoot.SetActive(false);
        if (storyboard)
        {
            storyboard.playOnStart = false;
            storyboard.onFinished = ShowButtons;
            storyboard.StartCoroutine(storyboard.Play());
        }
        else ShowButtons();
    }

    private void ShowButtons()
    {
        if (storyboard && !storyboard.WasSkipped)
        {
            storyboard.blackFade.alpha = 0f;
            storyboard.captionGroup.alpha = 1f;
        }
        if (buttonsRoot) buttonsRoot.SetActive(true);
    }

    public void BackToMenu()
    {
        GameState.ResetForMenu();
        SceneManager.LoadScene("00_StartMenu", LoadSceneMode.Single);
    }

    public void Quit() => Application.Quit();
}
