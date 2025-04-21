using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_Manager : MonoBehaviour
{

    public static UI_Manager instance;
    // Start is called before the first frame update


    private void Awake()
    {
        if(instance == null)
        {
          instance = this;
        }
    }
    public void OpenGame()
    {
        SceneManager.LoadSceneAsync(0);
    }





}
