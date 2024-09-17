using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance; 
    
    [SerializeField] GameObject[] menus;

    private void Awake() {
        instance = this; 
    }

    public void OpenMenu(string menuName) {
        for (int i = 0; i < menus.Length; ++i) {
            if (menus[i].name == menuName) menus[i].SetActive(true); 
            else menus[i].SetActive(false); 
        }
    }
}
