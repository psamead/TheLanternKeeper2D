using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class EndCardController : MonoBehaviour
{
    [SerializeField] private StoryboardPlayer storyboard;
    [SerializeField] private GameObject buttonsRoot;
    [SerializeField] private TMP_Text backLabel;
    [SerializeField] private TMP_Text quitLabel;
    [SerializeField] private TMP_Text advanceHint;
    [SerializeField] private LanternFlowText flowText;
    [SerializeField] private AudioSource storyboardMusic;
    [SerializeField, Min(0f)] private float musicFadeSeconds = 1.5f;
    private Coroutine musicFade;

    private void Start()
    {
        if (backLabel && flowText) backLabel.text = flowText.backToMenu;
        if (quitLabel && flowText) quitLabel.text = flowText.quit;
        if (buttonsRoot) buttonsRoot.SetActive(false);
        if (storyboardMusic && storyboardMusic.clip)
        {
            if (!AudioManager.TryRestoreEndCardMusic(storyboardMusic))
            {
                storyboardMusic.volume = 0f;
                storyboardMusic.Play();
            }
            StartMusicFade(0.6f);
        }
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
        if (storyboardMusic) StartMusicFade(0f);
        if (storyboard)
        {
            if (storyboard.WasSkipped) storyboard.ShowFinalFrame();
            storyboard.blackFade.alpha = 0f;
            if (!storyboard.WasSkipped) storyboard.captionGroup.alpha = 1f;
        }
        if (buttonsRoot) buttonsRoot.SetActive(true);
        if (advanceHint) advanceHint.gameObject.SetActive(false);
    }

    private void StartMusicFade(float target)
    {
        if (musicFade != null) StopCoroutine(musicFade);
        musicFade = StartCoroutine(FadeMusic(target));
    }

    private System.Collections.IEnumerator FadeMusic(float target)
    {
        float start = storyboardMusic.volume;
        float elapsed = 0f;
        while (elapsed < musicFadeSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            storyboardMusic.volume = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / musicFadeSeconds));
            yield return null;
        }
        storyboardMusic.volume = target;
        if (target <= 0f) storyboardMusic.Stop();
        musicFade = null;
    }

    public void BackToMenu()
    {
        GameState.ResetForMenu();
        SceneManager.LoadScene("00_StartMenu", LoadSceneMode.Single);
    }

    public void Quit() => Application.Quit();
}
