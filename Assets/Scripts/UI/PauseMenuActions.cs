using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuActions : MonoBehaviour
{
    [Header("Title Scene Name")]
    [SerializeField] private string titleSceneName = "TitleScene";

    public void OnClickQuitToTitle()
    {
        // 일시정지/오디오/커서 상태 복구
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Cursor.visible = true;                         // 마우스 룩 게임이 아니면 취향대로
        Cursor.lockState = CursorLockMode.None;

        // 타이틀로 이동
        SceneManager.LoadScene(titleSceneName);
    }

    // (선택) 비동기 로딩 버전 – 로딩 화면 쓰고 싶을 때
    public void OnClickQuitToTitleAsync()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        StartCoroutine(LoadTitleAsync());
    }

    private System.Collections.IEnumerator LoadTitleAsync()
    {
        var op = SceneManager.LoadSceneAsync(titleSceneName);
        op.allowSceneActivation = true; // 로딩바를 표시하고 싶다면 false로 두고 직접 전환도 가능
        while (!op.isDone) yield return null;
    }
}