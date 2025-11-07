using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Button))]
public class TitlePlay : MonoBehaviour {

    [Header("Start Scene")]
    public string startScene = "Stage"; // 게임 시작 씬

    void Start() {
        Button button = GetComponent<Button>();
        
        if (button != null) {
            button.onClick.AddListener(LoadScene);
        }
    }

    void LoadScene() {
        SceneManager.LoadScene(startScene); // 설정된 씬 로드
        
        Debug.Log(startScene + " 씬 로드");
    }
}
