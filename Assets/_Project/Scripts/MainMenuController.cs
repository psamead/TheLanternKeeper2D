using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class MainMenuController : MonoBehaviour
{
    [SerializeField] private List<Button> buttons = new List<Button>();
    [SerializeField] private List<TMP_Text> labels = new List<TMP_Text>();
    [SerializeField] private RectTransform selectionIcon;
    [SerializeField] private RectTransform underline;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private MenuAudio menuAudio;
    [SerializeField, Min(0f)] private float underlineGap = 8f;
    [SerializeField] private Color normalColor = new Color32(0xF2, 0xEB, 0xDD, 0xFF);
    [SerializeField] private Color selectedColor = new Color32(0xE8, 0xB4, 0x5A, 0xFF);
    private int selectedIndex;

    private void Awake()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            int index = i;
            buttons[i].onClick.AddListener(() => Confirm(index));
            var hover = buttons[i].gameObject.AddComponent<MenuItemHover>();
            hover.Initialize(this, index);
        }
        if (menuAudio == null) menuAudio = GetComponent<MenuAudio>();
        if (menuAudio == null) menuAudio = gameObject.AddComponent<MenuAudio>();
        if (settingsPanel != null) settingsPanel.SetActive(false);
        SetSelection(0, false);
    }

    private void Update()
    {
        if (buttons.Count == 0 || Keyboard.current == null) return;
        if (Keyboard.current.escapeKey.wasPressedThisFrame && settingsPanel != null && settingsPanel.activeSelf)
        {
            settingsPanel.SetActive(false);
            if (menuAudio != null) menuAudio.PlayBack();
            return;
        }
        if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame) Select((selectedIndex + buttons.Count - 1) % buttons.Count);
        if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame) Select((selectedIndex + 1) % buttons.Count);
        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame) Confirm(selectedIndex);
}
    public void Select(int index)
    {
        SetSelection(index, true);
    }

    public void SelectFromPointer(int index)
    {
        SetSelection(index, true);
    }

    private void SetSelection(int index, bool playHoverSound)
    {
        if (buttons.Count == 0) return;
        int nextIndex = Mathf.Clamp(index, 0, buttons.Count - 1);
        bool changed = nextIndex != selectedIndex;
        selectedIndex = nextIndex;
        for (int i = 0; i < labels.Count; i++) labels[i].color = i == selectedIndex ? selectedColor : normalColor;
        RectTransform item = buttons[selectedIndex].GetComponent<RectTransform>();
        if (selectionIcon != null) selectionIcon.anchoredPosition = new Vector2(-44f, item.anchoredPosition.y);
        if (underline != null && selectedIndex < labels.Count)
        {
            TMP_Text label = labels[selectedIndex];
            label.ForceMeshUpdate();
            Bounds textBounds = label.textBounds;
            if (textBounds.size.x <= Mathf.Epsilon)
                textBounds = new Bounds(Vector3.zero, new Vector3(label.preferredWidth, label.preferredHeight, 0f));

            RectTransform underlineParent = underline.parent as RectTransform;
            if (underlineParent != null)
            {
                Vector3 leftWorld = label.rectTransform.TransformPoint(
                    new Vector3(textBounds.min.x, textBounds.min.y - underlineGap, 0f));
                Vector3 rightWorld = label.rectTransform.TransformPoint(
                    new Vector3(textBounds.max.x, textBounds.min.y - underlineGap, 0f));
                Vector3 leftLocal = underlineParent.InverseTransformPoint(leftWorld);
                Vector3 rightLocal = underlineParent.InverseTransformPoint(rightWorld);
                underline.anchoredPosition = LocalToAnchoredPosition(
                    underline, new Vector2(leftLocal.x, leftLocal.y));
                underline.sizeDelta = new Vector2(Mathf.Abs(rightLocal.x - leftLocal.x), 3f);
            }
        }
        if (changed && playHoverSound && menuAudio != null) menuAudio.PlayHover();
    }

    private static Vector2 LocalToAnchoredPosition(RectTransform rectTransform, Vector2 parentLocalPosition)
    {
        RectTransform parent = rectTransform.parent as RectTransform;
        if (parent == null) return parentLocalPosition;

        Vector2 anchor = Vector2.Scale(rectTransform.anchorMin, Vector2.one - rectTransform.pivot)
            + Vector2.Scale(rectTransform.anchorMax, rectTransform.pivot);
        Vector2 anchorReference = parent.rect.min + Vector2.Scale(parent.rect.size, anchor);
        return parentLocalPosition - anchorReference;
    }

    private void Confirm(int index)
    {
        SetSelection(index, false);
        if (menuAudio != null) menuAudio.PlayConfirm();
        switch (index)
        {
            case 0:
                MenuMusic menuMusic = FindFirstObjectByType<MenuMusic>();
                if (menuMusic != null) menuMusic.FadeOutAndLoad("01_Home");
                else SceneLoader.Load("01_Home");
                break;
            case 1: if (settingsPanel != null) settingsPanel.SetActive(true); break;
            case 2:
#if UNITY_EDITOR
                Debug.Log("Quit selected (Application.Quit is disabled in the Editor).");
#else
                Application.Quit();
#endif
                break;
        }
    }

}
