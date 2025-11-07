using System.Collections;
using UnityEngine;

public class Pattern : MonoBehaviour {

    [Header("Stat")] // 수치 설정
    public string pattern = "Pattern";
    public float damage = 10;
    public float hitboxDuration = 0.5f;
    [Range(0, 100)]
    public float chance = 20;
    public float cool = 10;
    public float duration = 1.0f;
    
    [Header("Component")]
    public GameObject hitboxObject;

    private float _timer;
    private bool _enabled;
    private float _cooldown;

    private void Start() => ResetValue();

    private void Reset() => ResetValue();
    
    private void FixedUpdate() {
        if (_enabled) {
            _timer += Time.deltaTime;

            if (_timer > 1.0f) {
                _timer = 0.0f;

                if (!gameObject.tag.Equals("UsedPattern") && Random.value * 100 < chance) {
                    _timer = 0;
                    _enabled = false;

                    gameObject.tag = "UsedPattern";

                    hitboxObject.GetComponent<Front>().StartAttack(damage, hitboxDuration);

                    Debug.Log($"{gameObject.name} 패턴 사용: {pattern}");

                    StartCoroutine(ChangeTag());
                    return;
                }
            }
        }
        
        _cooldown += Time.fixedDeltaTime;
        
        if (_cooldown < cool) {
            return;
        }
        
        ResetValue();
    }

    private void ResetValue() {
        _enabled = true;
        _cooldown = 0;
    }

    private IEnumerator ChangeTag() {
        yield return new WaitForSeconds(duration);
        gameObject.tag = "Boss";
    }

}