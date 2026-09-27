using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonDownListener : MonoBehaviour, IPointerDownHandler
{
    public Button button;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        button.onClick.Invoke();
    }
}