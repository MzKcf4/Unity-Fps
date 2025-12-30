using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;

public class MainMenuUiManager : MonoBehaviour
{
    [SerializeField] private Button btnQuit;

    void Start()
    {
        if (btnQuit != null)
            btnQuit.onClick.AddListener(Quit);
    }

    private void Quit() 
    {
        Application.Quit();
    }
}

