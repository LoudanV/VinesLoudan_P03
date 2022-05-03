using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SlotTemplate : MonoBehaviour, IPointerClickHandler
{
    public Image container;
    public Image item;
    public Text count;

    [HideInInspector]
    public bool hasClicked = false;
    [HideInInspector]
    public ItemCrafting craftingController;

    public void OnPointerClick(PointerEventData eventData)
    {
        hasClicked = true;
        craftingController.ClickEventRecheck();
    }
}