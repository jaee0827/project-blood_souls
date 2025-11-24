using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseTitle : MonoBehaviour
{
    [Header("Pause Canvas")]
    public GameObject pause;
    
    [Header("Title Scene")]
    public string titleScene = "Title";

    void Start()
    {
        Button button = GetComponent<Button>();
        
        if (button != null) {
            button.onClick.AddListener(LoadScene);
        }
    }
    
    public void LoadScene()
    {
        Time.timeScale = 1.0f;
        
        pause.SetActive(false);
        
        SceneManager.LoadScene(titleScene);
        
        Debug.Log("일시 정지 해제");
        Debug.Log(titleScene + " 씬 로드");
    }
}
