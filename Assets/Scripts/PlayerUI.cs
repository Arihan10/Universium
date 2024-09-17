using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class PlayerUI : MonoBehaviour
{
    public Image healthBar, progressBar; 

    public Sprite[] itemSprites;
    public Sprite idleCraftSprite; 

    public TextMeshProUGUI interactableText, itemInfoText; 

    List<GameObject> itemObjects = new List<GameObject>(); 
    public GameObject planetNameHolder, crosshair, loadingPanel, characterUI, craftItemsInventory, craftItemsUI, craftSpace, craftResultBox, holdablesInventory, abilitiesInventory, errorMsgHolder, gameOverScreen, shipUI, helpUI; 
    [SerializeField] GameObject itemPrefab; 

    List<CraftItem> craftItems = new List<CraftItem>();

    PlayerController player; 
    
    // Start is called before the first frame update
    void Start()
    {
        player = GetComponentInParent<PlayerController>(); 
    }

    public void ToggleCrosshair(bool _enable) {
        crosshair.SetActive(_enable); 
    }

    public void ToggleCrafting() {
        if (!craftItemsUI.activeSelf) {
            Cursor.lockState = CursorLockMode.None; 
            Cursor.visible = true;
            UpdateInventory(); 
        } else {
            Cursor.lockState = CursorLockMode.Locked; 
            Cursor.visible = false;
            RemoveAll(); 
        }

        itemInfoText.text = "";
        craftItemsUI.SetActive(!craftItemsUI.activeSelf);
    }

    public void SetItemInfoText(string _text) {
        itemInfoText.text = _text;
    }

    public void UpdateInventory() {
        int[] _inventory = transform.parent.GetComponent<PlayerController>().inventory; 

        foreach (GameObject GO in itemObjects) Destroy(GO);
        itemObjects.Clear(); 

        for (int i = 0; i < _inventory.Length; ++i) {
            if (_inventory[i] > 0) {
                AddItemInventory(itemSprites[i], _inventory[i], itemSprites[i].name, i); 
            }
        }
    }

    void AddItemInventory(Sprite _sprite, int _amount, string _name, int _itemIndex) {
        GameObject _item = Instantiate(itemPrefab, craftItemsInventory.transform); 
        _item.GetComponent<CraftItem>().Setup(_sprite, _amount, _name, _itemIndex, -1); 
        itemObjects.Add(_item); 
    }

    public void Add(CraftItem _item) {
        if (craftItems.Count > 2) return;

        // craftItems.Add(_item); 

        for (int i = 0; i < 3; ++i) {
            GameObject _child = craftSpace.transform.GetChild(i).gameObject; 

            if (_child.GetComponent<CraftItem>().img.sprite == idleCraftSprite) {
                _child.GetComponent<CraftItem>().Setup(_item.img.sprite, _item.amount, _item.img.sprite.name, _item.itemIndex, i); 
                break; 
            }
        }

        craftItems.Add(_item); 
        Destroy(_item.gameObject);

        ComputeCraftCombo(); 
    }

    public void Remove(CraftItem _item, int craftIndex) {
        /* for (int i = 0; i < 3; ++i) {
            if (craftSpace.transform.GetChild(i).GetComponent<CraftItem>().img.sprite == _item.img.sprite) {
                craftItems.Remove(_item); 
                break;
            }
        } */

        foreach (CraftItem _currItem in craftItems) {
            if (_currItem.img.sprite == _item.img.sprite) {
                craftItems.Remove(_currItem);
                break; 
            }
        }

        AddItemInventory(_item.GetComponent<Image>().sprite, player.inventory[_item.itemIndex], _item.img.sprite.name, _item.itemIndex); 
        // craftSpace.transform.GetChild(craftIndex).GetComponent<Image>().sprite = idleCraftSprite;
        craftSpace.transform.GetChild(craftIndex).GetComponent<CraftItem>().Setup(idleCraftSprite, 0, "Empty", 0, -1); 

        ComputeCraftCombo(); 
    }

    public void RemoveAll() {
        for (int i = 0; i < 3; ++i) {
            if (craftSpace.transform.GetChild(i).GetComponent<Image>().sprite != idleCraftSprite) {
                Remove(craftSpace.transform.GetChild(i).GetComponent<CraftItem>(), i); 
            }
        }

        UpdateInventory(); 
    }

    public void ComputeCraftCombo() {
        // Just Iron
        if (craftItems.Count == 1 && CraftContainsItem(0) != -1 && craftItems[CraftContainsItem(0)].amount >= 10) { // Iron
            craftResultBox.GetComponent<CraftItem>().Setup(itemSprites[2], 1, itemSprites[2].name, 2, -1); // Iron Pistol
        } else if (craftItems.Count == 1 && CraftContainsItem(1) != -1 && craftItems[CraftContainsItem(1)].amount >= 25) { // Rubber
            craftResultBox.GetComponent<CraftItem>().Setup(itemSprites[3], 1, itemSprites[3].name, 3, -1); // Rubber Armor
        } else if (craftItems.Count == 1 && CraftContainsItem(4) != -1 && craftItems[CraftContainsItem(4)].amount >= 5) { // Plasma Ball
            craftResultBox.GetComponent<CraftItem>().Setup(itemSprites[5], 1, itemSprites[5].name, 5, -1); // Plasma Gun
        } else if (craftItems.Count == 2 && CraftContainsItem(2) != -1 && craftItems[CraftContainsItem(2)].amount > 0 && CraftContainsItem(6) != -1 && craftItems[CraftContainsItem(6)].amount >= 10) { // Iron Pistol + Energy Crystal
            craftResultBox.GetComponent<CraftItem>().Setup(itemSprites[7], 1, itemSprites[7].name, 7, -1); // Auto Rifle
        } else if (craftItems.Count == 2 && CraftContainsItem(4) != -1 && craftItems[CraftContainsItem(4)].amount >= 10 && CraftContainsItem(1) != -1 && craftItems[CraftContainsItem(1)].amount >= 10) { // Plasma Ball + Rubber
            craftResultBox.GetComponent<CraftItem>().Setup(itemSprites[10], player.itemUnitAmounts[10], itemSprites[10].name, 10, -1); // Plasma
        } else if (craftItems.Count == 1 && CraftContainsItem(11) != -1 && craftItems[CraftContainsItem(11)].amount > 0) { // Molten Core
            craftResultBox.GetComponent<CraftItem>().Setup(itemSprites[13], 1, itemSprites[13].name, 13, -1); // Heavy Miner
        } else if (craftItems.Count == 2 && CraftContainsItem(12) != -1 && craftItems[CraftContainsItem(12)].amount > 0 && CraftContainsItem(8) != -1 && craftItems[CraftContainsItem(8)].amount >= 4) { // Magnetic Core + Diamond
            craftResultBox.GetComponent<CraftItem>().Setup(itemSprites[14], 1, itemSprites[14].name, 14, -1); // Energy Launcher
        } else if (craftItems.Count == 2 && CraftContainsItem(11) != -1 && craftItems[CraftContainsItem(11)].amount > 0 && CraftContainsItem(9) != -1 && craftItems[CraftContainsItem(9)].amount >= 5) { // Molten Core + Stalicite
            craftResultBox.GetComponent<CraftItem>().Setup(itemSprites[15], 1, itemSprites[15].name, 15, -1); // Heavy Destroyer
        } else if (craftItems.Count == 2 && CraftContainsItem(8) != -1 && craftItems[CraftContainsItem(8)].amount > 0 && CraftContainsItem(6) != -1 && craftItems[CraftContainsItem(6)].amount >= 2) { // Diamond + Energy Crystal
            craftResultBox.GetComponent<CraftItem>().Setup(itemSprites[16], player.itemUnitAmounts[16], itemSprites[16].name, 16, -1); // Energy Bomb
        } else if (craftItems.Count == 2 && CraftContainsItem(11) != -1 && craftItems[CraftContainsItem(11)].amount > 0 && CraftContainsItem(7) != -1 && craftItems[CraftContainsItem(7)].amount > 0) { // Molten Core + Auto Rifle
            craftResultBox.GetComponent<CraftItem>().Setup(itemSprites[17], 1, itemSprites[17].name, 17, -1); // Heavy AR
        } else if (craftItems.Count == 2 && CraftContainsItem(9) != -1 && craftItems[CraftContainsItem(9)].amount > 0 && CraftContainsItem(4) != -1 && craftItems[CraftContainsItem(4)].amount >= 5) { // Stalagcite + Plasma
            craftResultBox.GetComponent<CraftItem>().Setup(itemSprites[18], player.itemUnitAmounts[18], itemSprites[18].name, 18, -1); // Plasma Laser
        } else if (craftItems.Count == 3 && CraftContainsItem(6) != -1 && craftItems[CraftContainsItem(6)].amount >= 5 && CraftContainsItem(20) != -1 && craftItems[CraftContainsItem(20)].amount >= 5 && CraftContainsItem(12) != -1 && craftItems[CraftContainsItem(12)].amount > 0) { // Energy Crystal + BioMass + Magnetic Core
            craftResultBox.GetComponent<CraftItem>().Setup(itemSprites[19], 1, itemSprites[19].name, 19, -1); // Radar
        }
        else {
            craftResultBox.GetComponent<CraftItem>().Setup(null, 0, "Empty", 0, -1); 
        }
    }

    public int CraftContainsItem(int _itemIndex) {
        for (int i = 0; i < craftItems.Count; ++i) {
            if (craftItems[i].itemIndex == _itemIndex) return i;
        }
        return -1;
    }

    public void AddHoldable(CraftItem _item) {
        for (int i = 0; i < holdablesInventory.transform.childCount; ++i) {
            if (holdablesInventory.transform.GetChild(i).GetComponent<Image>().sprite == idleCraftSprite) {
                holdablesInventory.transform.GetChild(i).GetComponent<Image>().sprite = _item.img.sprite;
                holdablesInventory.transform.GetChild(i).GetComponent<Image>().color = new Color(1, 1, 1, 1);
                break; 
            }
        }
    }

    public void AddAbility(CraftItem _item) {
        Debug.Log(_item.img.sprite.name); 
        for (int i = 0; i < abilitiesInventory.transform.childCount; ++i) {
            if (abilitiesInventory.transform.GetChild(i).GetComponent<Image>().sprite == idleCraftSprite) {
                abilitiesInventory.transform.GetChild(i).GetComponent<Image>().sprite = _item.img.sprite;
                abilitiesInventory.transform.GetChild(i).GetComponent<Image>().color = new Color(1, 1, 1, 1);
                break;
            }
        }
    }

    public void GameOver() {
        gameOverScreen.SetActive(true); 
        if (GetComponentInParent<PlayerController>().health > 0f) DisplayMessage(planetNameHolder, 0.01f, 1f, 10f, "YOU WIN"); 
        else DisplayMessage(planetNameHolder, 0.01f, 1f, 10f, "YOU LOSE"); 

        enabled = false; 
    }

    public void DisplayMessage(GameObject _msgObj, float increment, float durationSecs, float stayDurationSecs, string _text) {
        StartCoroutine(Fade(_msgObj, increment, durationSecs, stayDurationSecs, _text)); 
    }

    IEnumerator Fade(GameObject _msgObj, float increment, float durationSecs, float stayDurationSecs, string _text) {
        // UI.ToggleCrosshair(false); 
        _msgObj.SetActive(true);
        _msgObj.GetComponentInChildren<TextMeshProUGUI>().text = _text;
        characterUI.SetActive(false);

        Color _col = _msgObj.GetComponentInChildren<TextMeshProUGUI>().faceColor;
        Color _col2 = _msgObj.GetComponentInChildren<Image>().color;

        for (int i = 0; i < durationSecs / increment; ++i) {
            _msgObj.GetComponentInChildren<TextMeshProUGUI>().color = new Color(_col.r, _col.g, _col.b, increment / durationSecs * (i));
            _msgObj.GetComponentInChildren<Image>().color = new Color(_col2.r, _col2.g, _col2.b, increment / durationSecs * (i));

            yield return new WaitForSeconds(increment);
        }

        _msgObj.GetComponentInChildren<TextMeshProUGUI>().color = new Color(_col.r, _col.g, _col.b, 1f);
        _msgObj.GetComponentInChildren<Image>().color = new Color(_col2.r, _col2.g, _col2.b, 1f);

        yield return new WaitForSeconds(stayDurationSecs);

        for (int i = 0; i < durationSecs / increment; ++i) {
            _msgObj.GetComponentInChildren<TextMeshProUGUI>().color = new Color(_col.r, _col.g, _col.b, 1f - increment / durationSecs * (i));
            _msgObj.GetComponentInChildren<Image>().color = new Color(_col2.r, _col2.g, _col2.b, 1f - increment / durationSecs * (i));

            yield return new WaitForSeconds(increment);
        }

        _msgObj.SetActive(false); 
        characterUI.SetActive(true); 
    }

    private void Update() {
        /*if (Input.GetKeyDown(KeyCode.Alpha6)) craftResultBox.GetComponent<CraftItem>().Setup(itemSprites[14], 1, itemSprites[14].name, 14, -1); // Energy Launcher
        else if (Input.GetKeyDown(KeyCode.Alpha7)) craftResultBox.GetComponent<CraftItem>().Setup(itemSprites[15], 1, itemSprites[15].name, 15, -1); // Heavy Destroyer*/
    }
}
