using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class CraftItem : MonoBehaviour
{
    public Image img; 

    [SerializeField] TextMeshProUGUI text;

    PlayerUI UI;

    public int itemIndex, craftIndex, amount; 
    
    // Start is called before the first frame update
    void Start()
    {
        UI = GetComponentInParent<PlayerUI>(); 
    }

    public void Setup(Sprite sprite, int _amount, string _name, int _itemIndex, int _craftIndex) {
        img.sprite = sprite; 
        text.text = _amount.ToString();
        amount = _amount; 
        name = _name;
        itemIndex = _itemIndex;
        craftIndex = _craftIndex; 
    }

    public void Hover(bool enter) {
        if (enter) UI.SetItemInfoText(name); 
        else UI.SetItemInfoText(""); 
    }

    public void Add() {
        UI.Add(this); 
        // Destroy(gameObject); 
    }

    public void Remove() {
        if (itemIndex == -1) return; 

        UI.Remove(this, craftIndex); 
    }

    public void ResultClick() {
        if (itemIndex < 0) return; 

        if (itemIndex == 2) { // Iron gun
            GetComponentInParent<PlayerController>().inventory[0] -= 10; // Change to 15 for final
            GetComponentInParent<PlayerController>().AddHoldableItem(this); 
        } else if (itemIndex == 3) { // Rubber armor
            GetComponentInParent<PlayerController>().inventory[1] -= 25; // Change to 10 for final
            GetComponentInParent<PlayerController>().AddAbility(this); 
        } else if (itemIndex == 5) { // Plasma Gun
            GetComponentInParent<PlayerController>().inventory[4] -= 10; // Change to 10 for final
            GetComponentInParent<PlayerController>().AddHoldableItem(this); 
        } else if (itemIndex == 7) { // Auto Rifle
            GetComponentInParent<PlayerController>().inventory[6] -= 10; // Change to 10 for final
            GetComponentInParent<PlayerController>().AddHoldableItem(this); 
        } else if (itemIndex == 10) {
            GetComponentInParent<PlayerController>().inventory[1] -= 10; // Change to 15 for final
            GetComponentInParent<PlayerController>().inventory[4] -= 10; // Change to 4 for final
            GetComponentInParent<PlayerController>().AddAmmo(this); 
        } else if (itemIndex == 13) {
            GetComponentInParent<PlayerController>().inventory[11] -= 1; 
            GetComponentInParent<PlayerController>().AddHoldableItem(this); 
        } else if (itemIndex == 14) {
            GetComponentInParent<PlayerController>().inventory[12] -= 1; 
            GetComponentInParent<PlayerController>().inventory[8] -= 4; // Change to 3 for final
            GetComponentInParent<PlayerController>().AddHoldableItem(this); 
        } else if (itemIndex == 15) {
            GetComponentInParent<PlayerController>().inventory[11] -= 1;
            GetComponentInParent<PlayerController>().inventory[9] -= 5; // Change to 3 for final
            GetComponentInParent<PlayerController>().AddHoldableItem(this);
        } else if (itemIndex == 16) {
            GetComponentInParent<PlayerController>().inventory[6] -= 2; 
            GetComponentInParent<PlayerController>().inventory[8] -= 1; // Change to 3 for final
            GetComponentInParent<PlayerController>().AddAmmo(this); 
        } else if (itemIndex == 17) {
            GetComponentInParent<PlayerController>().inventory[7] -= 1; 
            GetComponentInParent<PlayerController>().inventory[11] -= 1; // Change to 3 for final
            GetComponentInParent<PlayerController>().AddHoldableItem(this);
        } else if (itemIndex == 18) {
            GetComponentInParent<PlayerController>().inventory[9] -= 1; 
            GetComponentInParent<PlayerController>().inventory[4] -= 5; 
            GetComponentInParent<PlayerController>().AddAmmo(this); 
        } else if (itemIndex == 19) {
            GetComponentInParent<PlayerController>().inventory[6] -= 5;
            GetComponentInParent<PlayerController>().inventory[20] -= 5; 
            GetComponentInParent<PlayerController>().inventory[12] -= 1;
            GetComponentInParent<PlayerController>().AddAbility(this); 
        }

        amount = 0; 
        itemIndex = -1; 
        craftIndex = -1;
        GetComponent<Image>().sprite = null;

        GetComponentInParent<PlayerUI>().RemoveAll(); 
    }
}
