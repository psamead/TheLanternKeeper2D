using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class EndCardController : MonoBehaviour
{
    [SerializeField] private StoryboardPlayer storyboard;
    [SerializeField] private GameObject buttonsRoot;
    [SerializeField] private TMP_Text backLabel;
    [SerializeField] private TMP_Text quitLabel;
    [SerializeField] private TMP_Text advanceHint;
    [SerializeField] private Button backButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private RectTransform selectionIcon;
    [SerializeField] private RectTransform underline;
    [SerializeField] private LanternFlowText flowText;
    [SerializeField] private AudioSource storyboardMusic;
    [SerializeField, Min(0f)] private float musicFadeSeconds = 1.5f;
    private Coroutine musicFade;
    private int selectedIndex;

    private void Awake()
    {
        BindHover(backButton, 0);
        BindHover(quitButton, 1);
    }

    private void BindHover(Button button, int index)
    {
        if (!button) return;
        var hover = button.gameObject.GetComponent<EndCardItemHover>();
        if (!hover) hover = button.gameObject.AddComponent<EndCardItemHover>();
        hover.Initialize(this, index);
    }

    private void Update()
    {
        if (!buttonsRoot || !buttonsRoot.activeInHierarchy || Keyboard.current == null) return;
        if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame ||
            Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
            Select(1 - selectedIndex);
        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (selectedIndex == 0) BackToMenu();
            else Quit();
        }
    }

    public void Select(int index)
    {
        selectedIndex = Mathf.Clamp(index, 0, 1);
        if (!backLabel || !quitLabel) return;
        backLabel.color = selectedIndex == 0 ? SelectedColor : NormalColor;
        quitLabel.color = selectedIndex == 1 ? SelectedColor : NormalColor;

        Button button = selectedIndex == 0 ? backButton : quitButton;
        TMP_Text label = selectedIndex == 0 ? backLabel : quitLabel;
        if (!button || !label) return;
        var item = button.GetComponent<RectTransform>();
        if (selectionIcon) selectionIcon.anchoredPosition =
            new Vector2(item.anchoredPosition.x - item.rect.width * 0.5f - 34f,
                item.anchoredPosition.y + selectionIcon.rect.height * 0.5f);
        if (!underline) return;
        label.ForceMeshUpdate();
        Bounds bounds = label.textBounds;
        if (bounds.size.x <= Mathf.Epsilon)
            bounds = new Bounds(Vector3.zero, new Vector3(label.preferredWidth, label.preferredHeight, 0f));
        var parent = underline.parent as RectTransform;
        if (!parent) return;
        Vector3 left = parent.InverseTransformPoint(label.rectTransform.TransformPoint(
            new Vector3(bounds.min.x, bounds.min.y - 8f, 0f)));
        Vector3 right = parent.InverseTransformPoint(label.rectTransform.TransformPoint(
            new Vector3(bounds.max.x, bounds.min.y - 8f, 0f)));
        underline.anchoredPosition = new Vector2(left.x, left.y);
        underline.sizeDelta = new Vector2(Mathf.Abs(right.x - left.x), 3f);
    }

    private static readonly Color NormalColor = new Color32(0xF2, 0xEB, 0xDD, 0xFF);
    private static readonly Color SelectedColor = new Color32(0xE8, 0xB4, 0x5A, 0xFF);

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
        Select(0);
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

public sealed class EndCardItemHover : MonoBehaviour, IPointerEnterHandler
{
    private EndCardController owner;
    private int index;

    public void Initialize(EndCardController controller, int itemIndex)
    {
        owner = controller;
        index = itemIndex;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (owner) owner.Select(index);
    }
}
