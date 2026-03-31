using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
public class LevelManager : MonoBehaviour
{
    //CRISTIAN
    [SerializeField] private GameObject _loaderCanvas;
    public static LevelManager Instance; //{ get; private set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); //if current scene gets destroyed next scene will not destroy this gameobject
        }
        else
        {
            Destroy(gameObject);  //if has been declared destory the new one
        }
        
    }
    public async void LoadScene(string sceneName)
    {
        var scene = SceneManager.LoadSceneAsync(sceneName);
        scene.allowSceneActivation = false; //prevent the scene from activating immediately
    }
}
