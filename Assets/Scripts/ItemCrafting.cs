using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemCrafting : MonoBehaviour
{
    public RectTransform playerSlotsContainer;
    public RectTransform craftingSlotsContainer;
    public RectTransform resultSlotContainer;
    public Button craftButton;
    public SlotTemplate slotTemplate;
    public SlotTemplate resultSlotTemplate;

    [System.Serializable]
    public class SlotContainer
    {
        public Sprite itemSprite; 
        public int itemCount; 
        [HideInInspector]
        public int tableID;
        [HideInInspector]
        public SlotTemplate slot;
    }

    [System.Serializable]
    public class Item
    {
        public Sprite itemSprite;
        public bool stackable = false;
        public string craftRecipe; 
    }

    public SlotContainer[] playerSlots;
    SlotContainer[] craftSlots = new SlotContainer[9];
    SlotContainer resultSlot = new SlotContainer();
    public Item[] items;

    SlotContainer selectedItemSlot = null;

    int craftTableID = -1; 
    int resultTableID = -1;

    ColorBlock defaultButtonColors;

    void Start()
    {
        slotTemplate.container.rectTransform.pivot = new Vector2(0, 1);
        slotTemplate.container.rectTransform.anchorMax = slotTemplate.container.rectTransform.anchorMin = new Vector2(0, 1);
        slotTemplate.craftingController = this;
        slotTemplate.gameObject.SetActive(false);
        resultSlotTemplate.container.rectTransform.pivot = new Vector2(0, 1);
        resultSlotTemplate.container.rectTransform.anchorMax = resultSlotTemplate.container.rectTransform.anchorMin = new Vector2(0, 1);
        resultSlotTemplate.craftingController = this;
        resultSlotTemplate.gameObject.SetActive(false);

        craftButton.onClick.AddListener(PerformCrafting);
        defaultButtonColors = craftButton.colors;

        InitializeSlotTable(craftingSlotsContainer, slotTemplate, craftSlots, 5, 0);
        UpdateItems(craftSlots);
        craftTableID = 0;

        InitializeSlotTable(playerSlotsContainer, slotTemplate, playerSlots, 5, 1);
        UpdateItems(playerSlots);

        InitializeSlotTable(resultSlotContainer, resultSlotTemplate, new SlotContainer[] { resultSlot }, 0, 2);
        UpdateItems(new SlotContainer[] { resultSlot });
        resultTableID = 2;

        slotTemplate.container.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        slotTemplate.container.raycastTarget = slotTemplate.item.raycastTarget = slotTemplate.count.raycastTarget = false;
    }

    void InitializeSlotTable(RectTransform container, SlotTemplate slotTemplateTmp, SlotContainer[] slots, int margin, int tableIDTmp)
    {
        int resetIndex = 0;
        int rowTmp = 0;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = new SlotContainer();
            }
            GameObject newSlot = Instantiate(slotTemplateTmp.gameObject, container.transform);
            slots[i].slot = newSlot.GetComponent<SlotTemplate>();
            slots[i].slot.gameObject.SetActive(true);
            slots[i].tableID = tableIDTmp;

            float xTmp = (int)((margin + slots[i].slot.container.rectTransform.sizeDelta.x) * (i - resetIndex));
            if (xTmp + slots[i].slot.container.rectTransform.sizeDelta.x + margin > container.rect.width)
            {
                resetIndex = i;
                rowTmp++;
                xTmp = 0;
            }
            slots[i].slot.container.rectTransform.anchoredPosition = new Vector2(margin + xTmp, -margin - ((margin + slots[i].slot.container.rectTransform.sizeDelta.y) * rowTmp));
        }
    }

    void UpdateItems(SlotContainer[] slots)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            Item slotItem = FindItem(slots[i].itemSprite);
            if (slotItem != null)
            {
                if (!slotItem.stackable)
                {
                    slots[i].itemCount = 1;
                }
                if (slots[i].itemCount > 1)
                {
                    slots[i].slot.count.enabled = true;
                    slots[i].slot.count.text = slots[i].itemCount.ToString();
                }
                else
                {
                    slots[i].slot.count.enabled = false;
                }
                slots[i].slot.item.enabled = true;
                slots[i].slot.item.sprite = slotItem.itemSprite;
            }
            else
            {
                slots[i].slot.count.enabled = false;
                slots[i].slot.item.enabled = false;
            }
        }
    }

    Item FindItem(Sprite sprite)
    {
        if (!sprite)
            return null;

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].itemSprite == sprite)
            {
                return items[i];
            }
        }

        return null;
    }

    Item FindItem(string recipe)
    {
        if (recipe == "")
            return null;

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].craftRecipe == recipe)
            {
                return items[i];
            }
        }

        return null;
    }

    public void ClickEventRecheck()
    {
        if (selectedItemSlot == null)
        {
            selectedItemSlot = GetClickedSlot();
            if (selectedItemSlot != null)
            {
                if (selectedItemSlot.itemSprite != null)
                {
                    selectedItemSlot.slot.count.color = selectedItemSlot.slot.item.color = new Color(1, 1, 1, 0.5f);
                }
                else
                {
                    selectedItemSlot = null;
                }
            }
        }
        else
        {
            SlotContainer newClickedSlot = GetClickedSlot();
            if (newClickedSlot != null)
            {
                bool swapPositions = false;
                bool releaseClick = true;

                if (newClickedSlot != selectedItemSlot)
                {
                    if (newClickedSlot.tableID == selectedItemSlot.tableID)
                    {
                        if (newClickedSlot.itemSprite == selectedItemSlot.itemSprite)
                        {
                            Item slotItem = FindItem(selectedItemSlot.itemSprite);
                            if (slotItem.stackable)
                            {
                                selectedItemSlot.itemSprite = null;
                                newClickedSlot.itemCount += selectedItemSlot.itemCount;
                                selectedItemSlot.itemCount = 0;
                            }
                            else
                            {
                                swapPositions = true;
                            }
                        }
                        else
                        {
                            swapPositions = true;
                        }
                    }
                    else
                    {
                        if (resultTableID != newClickedSlot.tableID)
                        {
                            if (craftTableID != newClickedSlot.tableID)
                            {
                                if (newClickedSlot.itemSprite == selectedItemSlot.itemSprite)
                                {
                                    Item slotItem = FindItem(selectedItemSlot.itemSprite);
                                    if (slotItem.stackable)
                                    {
                                        selectedItemSlot.itemSprite = null;
                                        newClickedSlot.itemCount += selectedItemSlot.itemCount;
                                        selectedItemSlot.itemCount = 0;
                                    }
                                    else
                                    {
                                        swapPositions = true;
                                    }
                                }
                                else
                                {
                                    swapPositions = true;
                                }
                            }
                            else
                            {
                                if (newClickedSlot.itemSprite == null || newClickedSlot.itemSprite == selectedItemSlot.itemSprite)
                                {
                                    newClickedSlot.itemSprite = selectedItemSlot.itemSprite;
                                    newClickedSlot.itemCount++;
                                    selectedItemSlot.itemCount--;
                                    if (selectedItemSlot.itemCount <= 0)
                                    {
                                        selectedItemSlot.itemSprite = null;
                                    }
                                    else
                                    {
                                        releaseClick = false;
                                    }
                                }
                                else
                                {
                                    swapPositions = true;
                                }
                            }
                        }
                    }
                }

                if (swapPositions)
                {
                    Sprite previousItemSprite = selectedItemSlot.itemSprite;
                    int previousItemConunt = selectedItemSlot.itemCount;

                    selectedItemSlot.itemSprite = newClickedSlot.itemSprite;
                    selectedItemSlot.itemCount = newClickedSlot.itemCount;

                    newClickedSlot.itemSprite = previousItemSprite;
                    newClickedSlot.itemCount = previousItemConunt;
                }

                if (releaseClick)
                {
                    selectedItemSlot.slot.count.color = selectedItemSlot.slot.item.color = Color.white;
                    selectedItemSlot = null;
                }

                UpdateItems(playerSlots);
                UpdateItems(craftSlots);
                UpdateItems(new SlotContainer[] { resultSlot });
            }
        }
    }

    SlotContainer GetClickedSlot()
    {
        for (int i = 0; i < playerSlots.Length; i++)
        {
            if (playerSlots[i].slot.hasClicked)
            {
                playerSlots[i].slot.hasClicked = false;
                return playerSlots[i];
            }
        }

        for (int i = 0; i < craftSlots.Length; i++)
        {
            if (craftSlots[i].slot.hasClicked)
            {
                craftSlots[i].slot.hasClicked = false;
                return craftSlots[i];
            }
        }

        if (resultSlot.slot.hasClicked)
        {
            resultSlot.slot.hasClicked = false;
            return resultSlot;
        }

        return null;
    }

    void PerformCrafting()
    {
        string[] combinedItemRecipe = new string[craftSlots.Length];

        craftButton.colors = defaultButtonColors;

        for (int i = 0; i < craftSlots.Length; i++)
        {
            Item slotItem = FindItem(craftSlots[i].itemSprite);
            if (slotItem != null)
            {
                combinedItemRecipe[i] = slotItem.itemSprite.name + (craftSlots[i].itemCount > 1 ? "(" + craftSlots[i].itemCount + ")" : "");
            }
            else
            {
                combinedItemRecipe[i] = "";
            }
        }

        string combinedRecipe = string.Join(",", combinedItemRecipe);
        print(combinedRecipe);

        Item craftedItem = FindItem(combinedRecipe);
        if (craftedItem != null)
        {
            for (int i = 0; i < craftSlots.Length; i++)
            {
                craftSlots[i].itemSprite = null;
                craftSlots[i].itemCount = 0;
            }

            resultSlot.itemSprite = craftedItem.itemSprite;
            resultSlot.itemCount = 1;

            UpdateItems(craftSlots);
            UpdateItems(new SlotContainer[] { resultSlot });
        }
        else
        {
            ColorBlock colors = craftButton.colors;
            colors.selectedColor = colors.pressedColor = new Color(0.8f, 0.55f, 0.55f, 1);
            craftButton.colors = colors;
        }
    }

    void Update()
    {
        if (selectedItemSlot != null)
        {
            if (!slotTemplate.gameObject.activeSelf)
            {
                slotTemplate.gameObject.SetActive(true);
                slotTemplate.container.enabled = false;

                slotTemplate.count.color = selectedItemSlot.slot.count.color;
                slotTemplate.item.sprite = selectedItemSlot.slot.item.sprite;
                slotTemplate.item.color = selectedItemSlot.slot.item.color;
            }

            slotTemplate.container.rectTransform.position = Input.mousePosition;
            slotTemplate.count.text = selectedItemSlot.slot.count.text;
            slotTemplate.count.enabled = selectedItemSlot.slot.count.enabled;
        }
        else
        {
            if (slotTemplate.gameObject.activeSelf)
            {
                slotTemplate.gameObject.SetActive(false);
            }
        }
    }
}