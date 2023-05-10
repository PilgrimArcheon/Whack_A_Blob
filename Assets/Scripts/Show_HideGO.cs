using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Show_HideGO : MonoBehaviour
{
    public GameObject[] goToShow;

    public void ShowGameObject()
    {
        foreach (var uiGO in goToShow)
        {
            if(uiGO.activeSelf) uiGO.SetActive(false);
            else uiGO.SetActive(true);
        }
    } 
}
