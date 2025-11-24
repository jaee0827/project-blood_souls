using TMPro;
using UnityEngine;

public class PauseOpen : MonoBehaviour
{
    [Header("Pause Text")]
    public TMP_Text pauseText;
    
    public void Pause(string text)
    {
        pauseText.text = text;
        
        Time.timeScale = 0.0f;
        
        gameObject.SetActive(true);
        
        Debug.Log("일시 정지");
    }
}
