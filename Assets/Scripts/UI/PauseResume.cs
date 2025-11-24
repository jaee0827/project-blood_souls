using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseResume : MonoBehaviour
{
    [Header("Pause Canvas")]
    public GameObject pause;
    
    [Header("Pause Text")]
    public TMP_Text pauseText;
    
    void Start()
    {
        Button button = GetComponent<Button>();
        
        if (button != null) {
            button.onClick.AddListener(ResumeGame);
        }
    }
    
    public void ResumeGame()
    {
        if (!pauseText.text.Equals("PAUSE"))
        {
            return;
        }
        
        Time.timeScale = 1.0f;
        
        pause.SetActive(false);
        
        Debug.Log("일시 정지 해제");
    }
}
