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
        if (settingsPanel != null) settingsPanel.SetActive(false);
        Select(0);
    }

    private void Update()
    {
        if (buttons.Count == 0 || Keyboard.current == null) return;
        if (Keyboard.current.escapeKey.wasPressedThisFrame && settingsPanel != null && settingsPanel.activeSelf)
        {
            settingsPanel.SetActive(false);
            return;
        }
        if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame) Select((selectedIndex + buttons.Count - 1) % buttons.Count);
        if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame) Select((selectedIndex + 1) % buttons.Count);
        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame) Confirm(selectedIndex);
}
    public void Select(int index)
    {
        if (buttons.Count == 0) return;
        selectedIndex = Mathf.Clamp(index, 0, buttons.Count - 1);
        for (int i = 0; i < labels.Count; i++) labels[i].color = i == selectedIndex ? selectedColor : normalColor;
        RectTransform item = buttons[selectedIndex].GetComponent<RectTransform>();
        if (selectionIcon != null) selectionIcon.anchoredPosition = new Vector2(-44f, item.anchoredPosition.y);
        if (underline != null && selectedIndex < labels.Count)
        {
            underline.anchoredPosition = new Vector2(0f, item.anchoredPosition.y - 35f);
            underline.sizeDelta = new Vector2(labels[selectedIndex].preferredWidth, 3f);
        }
    }

    private void Confirm(int index)
    {
        Select(index);
        switch (index)
        {
            case 0: SceneLoader.Load("02_Home"); break;
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
