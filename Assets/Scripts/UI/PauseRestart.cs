using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseRestart : MonoBehaviour
{
    [Header("Pause Canvas")]
    public GameObject pause;
    
    [Header("Stage Scene")]
    public string stageScene = "Stage";

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
        
        SceneManager.LoadScene(stageScene);
        
        Debug.Log("일시 정지 해제");
        Debug.Log(stageScene + " 씬 로드");
    }
}
