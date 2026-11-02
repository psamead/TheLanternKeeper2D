using UnityEngine;
using UnityEngine.EventSystems;

public sealed class MenuItemHover : MonoBehaviour, IPointerEnterHandler
{
    private MainMenuController owner;
    private int index;

    public void Initialize(MainMenuController controller, int itemIndex)
    {
        owner = controller;
        index = itemIndex;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (owner != null) owner.SelectFromPointer(index);
    }
}
