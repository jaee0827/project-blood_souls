using UnityEngine;
using UnityEngine.UI;

public class TitleQuit : MonoBehaviour {
    
    void Start() {
        Button button = GetComponent<Button>();
        
        if (button != null) {
            button.onClick.AddListener(QuitGame);
        }
    }

    void QuitGame() {
        UnityEditor.EditorApplication.isPlaying = false;
        
        Application.Quit();
        
        Debug.Log("게임 종료");
    }
}
