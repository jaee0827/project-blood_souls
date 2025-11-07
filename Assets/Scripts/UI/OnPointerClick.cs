using UnityEngine;
using UnityEngine.EventSystems;

public class RightClickResume : MonoBehaviour, IPointerClickHandler
{
    // 너가 이미 쓰는 일시정지 매니저(예: PauseMenuToggler/PauseManager 등)를 연결
    [SerializeField] private MonoBehaviour pauseManagerLike; // Resume() 메서드가 있는 컴포넌트
    [SerializeField] private string resumeMethodName = "Resume";

    // 매니저 없이 이 스크립트 혼자 처리하게 하고 싶으면 이것도 사용
    [Header("Fallback (매니저가 없을 때 쓰는 기본 재개 처리)")]
    [SerializeField] private GameObject pauseRoot;     // 캔버스 루트(숨길 패널)
    [SerializeField] private bool unpauseAudio = true;
    [SerializeField] private bool relockCursor = true;

    public void OnPointerClick(PointerEventData e)
    {
        if (e.button != PointerEventData.InputButton.Right) return;

        // 1) 매니저에 Resume()가 있으면 그걸 호출
        if (pauseManagerLike != null)
        {
            var m = pauseManagerLike.GetType().GetMethod(resumeMethodName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (m != null) { m.Invoke(pauseManagerLike, null); return; }
        }

        // 2) 매니저가 없으면 여기서 기본 재개 처리
        Time.timeScale = 1f;
        if (unpauseAudio) AudioListener.pause = false;

        if (relockCursor)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked; // 마우스 룩 게임 아니면 취향대로
        }

        if (pauseRoot != null) pauseRoot.SetActive(false);
        Debug.Log("[RightClickResume] Resume by Right Click");
    }
}