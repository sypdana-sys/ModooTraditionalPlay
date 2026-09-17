// 사방망의 위치 선택, 왕복 파워 게이지, 물리 투척과 실패 재시도를 관리한다.
using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SabangStone : MonoBehaviour
{
    public enum ThrowState { Idle, Selecting, Charging, Flying, Result, Complete }

    [Header("직접 배치한 참조")]
    [SerializeField] private Map map;
    [SerializeField] private Rigidbody stoneBody;
    [SerializeField] private Collider stoneCollider;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private InputActionReference rightTrigger;
    [SerializeField] private Transform targetMarker;
    [SerializeField] private LayerMask tileLayers;
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private LayerMask borderLayers;
    [SerializeField, Min(0.1f)] private float rayDistance = 10f;
    [Tooltip("표시용 마커만 표면 위로 띄우는 거리(m). 실제 목표 위치에는 적용하지 않는다.")]
    [SerializeField, Min(0f)] private float markerSurfaceOffset = 0.005f;

    [Header("직접 배치한 UI")]
    [SerializeField] private GameObject gaugeRoot;
    [SerializeField] private Slider powerSlider;
    [Tooltip("게이지 전체 폭을 가진 부모 아래의 성공 구간 Image. 좌우 anchor를 코드로 변경한다.")]
    [SerializeField] private RectTransform successBand;
    [SerializeField] private TMP_Text resultText;

    [Header("파워와 비행")]
    [SerializeField, Range(1, 8)] private int targetNumber = 1;
    [SerializeField] private float[] requiredPowers = { 25f, 30f, 40f, 50f, 55f, 65f, 75f, 80f };
    [SerializeField, Range(0f, 50f)] private float tolerance = 10f;
    [SerializeField, Min(1f)] private float powerPerSecond = 60f;
    [Tooltip("허용 범위를 벗어난 파워 1당 짧거나 길게 떨어질 거리(m)")]
    [SerializeField, Min(0.001f)] private float missDistancePerPower = 0.04f;
    [SerializeField, Min(0.1f)] private float flightTime = 1f;
    [SerializeField, Min(0.01f)] private float restingCenterHeight = 0.03f;
    [SerializeField, Min(0.01f)] private float settleSpeed = 0.15f;
    [Tooltip("착지 안정화로 인정할 최대 회전 속도(rad/s)")]
    [SerializeField, Min(0.01f)] private float settleAngularSpeed = 0.5f;
    [SerializeField, Min(0.01f)] private float settleDuration = 0.2f;
    [SerializeField, Min(1f)] private float flightTimeout = 5f;
    [SerializeField, Min(0.1f)] private float resultDuration = 1.2f;
    [SerializeField] private bool startAutomatically = false;

    [Header("외부 게임 진행 연결")]
    [SerializeField] private UnityEvent onThrowSucceeded = new UnityEvent();
    [SerializeField] private UnityEvent onThrowFailed = new UnityEvent();

    public ThrowState State { get; private set; }
    public int FailureCount { get; private set; }
    public float SelectedPower { get; private set; }
    public Vector3 SelectedPoint => selectedPoint;
    public int TargetNumber => targetNumber;
    public bool StartsAutomatically => startAutomatically;
    public event System.Action<ThrowState> StateChanged;

    private float elapsed, stableTime, chargingStartedAt;
    private Vector3 selectedPoint;
    private bool triggerReleased, powerMatched, touchedBorder;
    private readonly HashSet<Collider> groundContacts = new HashSet<Collider>();
    private bool enabledInput;

    private void OnEnable()
    {
        if (rightTrigger != null && rightTrigger.action != null && !rightTrigger.action.enabled)
        {
            rightTrigger.action.Enable();
            enabledInput = true;
        }
    }

    private void Start()
    {
        if (State != ThrowState.Idle) return;
        SetGauge(false);
        SetTargetMarker(false);
        ShowResult("");
        if (startAutomatically) BeginRound(targetNumber);
    }

    public void BeginRound(int number)
    {
        TryBeginRound(number);
    }

    public bool CanBeginRound(int number)
    {
        if (!isActiveAndEnabled) return ConfigurationError("SabangStone을 활성화하세요.");
        if (map == null || !map.isActiveAndEnabled) return ConfigurationError("활성화된 Map을 연결하세요.");
        if (stoneBody == null || stoneBody.gameObject != gameObject)
            return ConfigurationError("SabangStone과 같은 오브젝트의 Rigidbody를 Stone Body에 연결하세요.");
        if (stoneCollider == null || stoneCollider.attachedRigidbody != stoneBody ||
            !stoneCollider.enabled || stoneCollider.isTrigger)
            return ConfigurationError("Stone Body에 속한 활성 Collider를 연결하고 Is Trigger를 끄세요.");
        if (throwPoint == null || throwPoint.IsChildOf(transform))
            return ConfigurationError("망 자신이나 자식이 아닌 Throw Point를 연결하세요.");
        if (rayOrigin == null) return ConfigurationError("오른손 Ray Origin을 연결하세요.");
        if (rightTrigger == null || rightTrigger.action == null || !rightTrigger.action.enabled)
            return ConfigurationError("활성화된 오른손 Trigger Input Action을 연결하세요.");
        if (targetMarker == null || transform.IsChildOf(targetMarker))
            return ConfigurationError("망 자신이나 부모가 아닌 별도 Target Marker를 연결하세요.");
        if (tileLayers.value == 0 || (tileLayers.value & (1 << map.gameObject.layer)) == 0)
            return ConfigurationError("Tile Layers에 Map 오브젝트의 Layer를 포함하세요.");
        if ((tileLayers.value & (groundLayers.value | borderLayers.value)) != 0)
            return ConfigurationError("Tile Layers는 Ground Layers 및 Border Layers와 분리하세요.");
        if (groundLayers.value == 0 || borderLayers.value == 0)
            return ConfigurationError("Ground Layers와 Border Layers를 지정하세요.");
        if (gaugeRoot == null || powerSlider == null || successBand == null || resultText == null)
            return ConfigurationError("Gauge Root, Power Slider, Success Band, Result Text를 연결하세요.");
        if (transform.IsChildOf(gaugeRoot.transform) || resultText.transform.IsChildOf(gaugeRoot.transform))
            return ConfigurationError("망과 Result Text는 꺼지는 Gauge Root 바깥에 배치하세요.");
        if (flightTime <= 0f || flightTimeout <= flightTime || settleDuration <= 0f || resultDuration <= 0f)
            return ConfigurationError("비행·안정화·결과 시간은 양수이며 Flight Timeout은 Flight Time보다 커야 합니다.");
        if (number < 1 || number > 8 || requiredPowers == null || requiredPowers.Length != 8)
            return ConfigurationError("목표 번호는 1~8, Required Powers는 8개로 설정하세요.");
        return true;
    }

    private bool ConfigurationError(string message)
    {
        Debug.LogError("SabangStone: " + message, this);
        return false;
    }

    public bool TryBeginRound(int number)
    {
        // On Yes 등에 BeginRound가 중복 연결되어도 진행 중인 선택을 초기화하지 않는다.
        if (State != ThrowState.Idle && State != ThrowState.Complete) return false;
        if (!CanBeginRound(number)) return false;
        targetNumber = number;
        ResetAttempt();
        return true;
    }

    private void ResetAttempt()
    {
        FreezeStone();
        stoneBody.position = throwPoint.position;
        stoneBody.rotation = throwPoint.rotation;
        touchedBorder = powerMatched = false;
        selectedPoint = Vector3.zero;
        SelectedPower = 0f;
        triggerReleased = false;
        elapsed = stableTime = 0f;
        SetGauge(false);
        ShowResult("");
        SetTargetMarker(false);
        SetState(ThrowState.Selecting);
    }

    private void Update()
    {
        if (State == ThrowState.Idle || State == ThrowState.Complete) return;
        // 비행 중 입력도 소비하여 다음 단계로 눌림이 넘어가지 않게 한다.
        bool confirm = ReadConfirmation();
        switch (State)
        {
            case ThrowState.Selecting:
                UpdateSelection(confirm);
                break;
            case ThrowState.Charging:
                UpdateCharging(confirm);
                break;
            case ThrowState.Result:
                elapsed += Time.deltaTime;
                if (elapsed >= resultDuration) ResetAttempt();
                break;
        }
    }

    private bool ReadConfirmation()
    {
        bool pressed = rightTrigger.action.IsPressed();
        if (!pressed) triggerReleased = true;
        bool confirm = pressed && triggerReleased;
        if (confirm) triggerReleased = false;
        return confirm;
    }

    private void UpdateSelection(bool confirm)
    {
        bool valid = Physics.Raycast(rayOrigin.position, rayOrigin.forward, out RaycastHit hit,
            rayDistance, tileLayers, QueryTriggerInteraction.Collide)
            && map.TryGetRegion(hit, out Map.Region region) && (int)region == targetNumber;
        if (targetMarker != null)
        {
            SetTargetMarker(valid);
            if (valid) targetMarker.position = hit.point + map.transform.up * markerSurfaceOffset;
        }
        if (!confirm || !valid) return;
        selectedPoint = hit.point;
        chargingStartedAt = Time.time;
        SetGauge(true);
        UpdateGauge(0f);
        SetState(ThrowState.Charging);
    }

    private void UpdateCharging(bool confirm)
    {
        float power = Mathf.PingPong((Time.time - chargingStartedAt) * powerPerSecond, 100f);
        UpdateGauge(power);
        if (confirm) Throw(power);
    }

    private void Throw(float power)
    {
        if (State != ThrowState.Charging) return;
        SelectedPower = power;
        SetGauge(false);
        SetTargetMarker(false);
        float error = power - Mathf.Clamp(requiredPowers[targetNumber - 1], 0f, 100f);
        powerMatched = Mathf.Abs(error) <= tolerance;
        // 적정 파워는 선택 지점에 도달하고, 범위 밖에서는 오차에 비례해 거리가 변한다.
        float miss = powerMatched ? 0f : Mathf.Sign(error) * (Mathf.Abs(error) - tolerance) * missDistancePerPower;
        Vector3 up = map.transform.up;
        Vector3 direction = Vector3.ProjectOnPlane(selectedPoint - throwPoint.position, up).normalized;
        Vector3 destination = selectedPoint + direction * miss + up * restingCenterHeight;
        stoneBody.position = throwPoint.position;
        stoneBody.rotation = throwPoint.rotation;
        stoneBody.isKinematic = false;
        stoneBody.useGravity = true;
        stoneBody.angularVelocity = Vector3.zero;
        // Rigidbody 위치가 아니라 Collider 중심이 목표 높이에 도달하도록 보정한다.
        Physics.SyncTransforms();
        Vector3 centerOffset = stoneCollider.bounds.center - stoneBody.position;
        stoneBody.linearVelocity = (destination - centerOffset - stoneBody.position) / flightTime
            - 0.5f * Physics.gravity * flightTime;
        elapsed = stableTime = 0f;
        touchedBorder = false;
        groundContacts.Clear();
        SetState(ThrowState.Flying);
    }

    private void FixedUpdate()
    {
        if (State != ThrowState.Flying) return;
        elapsed += Time.fixedDeltaTime;
        // 잠든 Rigidbody에는 OnCollisionStay가 생략되므로 Enter/Exit 사이 접촉을 보존한다.
        groundContacts.RemoveWhere(IsInactiveCollider);
        stableTime = groundContacts.Count > 0 && stoneBody.linearVelocity.sqrMagnitude <= settleSpeed * settleSpeed
            && stoneBody.angularVelocity.sqrMagnitude <= settleAngularSpeed * settleAngularSpeed
            ? stableTime + Time.fixedDeltaTime : 0f;
        if (stableTime >= settleDuration) FinishThrow(false);
        else if (elapsed >= flightTimeout) FinishThrow(true);
    }

    private void FinishThrow(bool timedOut)
    {
        if (State != ThrowState.Flying) return;
        bool success = !timedOut && powerMatched && !touchedBorder
            && map.TryGetRegion(stoneCollider.bounds.center, out Map.Region region)
            && (int)region == targetNumber;
        FreezeStone();
        elapsed = 0f;
        SetGauge(false);
        SetTargetMarker(false);
        ShowResult(success ? "성공" : "실패");
        if (!success) FailureCount++;
        SetState(success ? ThrowState.Complete : ThrowState.Result);
        if (success) onThrowSucceeded.Invoke();
        else onThrowFailed.Invoke();
    }

    private void FreezeStone()
    {
        if (stoneBody == null) return;
        if (!stoneBody.isKinematic)
        {
            stoneBody.linearVelocity = Vector3.zero;
            stoneBody.angularVelocity = Vector3.zero;
        }
        stoneBody.isKinematic = true;
        groundContacts.Clear();
    }

    private static bool IsInactiveCollider(Collider collider)
    {
        return collider == null || !collider.enabled || !collider.gameObject.activeInHierarchy;
    }

    private void OnCollisionEnter(Collision collision) { RegisterCollision(collision.collider); }
    private void OnCollisionStay(Collision collision) { RegisterCollision(collision.collider); }
    private void OnCollisionExit(Collision collision) { groundContacts.Remove(collision.collider); }
    private void RegisterCollision(Collider other)
    {
        if (State != ThrowState.Flying) return;
        if (InLayers(other, groundLayers)) groundContacts.Add(other);
        if (InLayers(other, borderLayers)) touchedBorder = true;
    }
    private void OnTriggerEnter(Collider other) { RegisterBorder(other); }
    private void OnTriggerStay(Collider other) { RegisterBorder(other); }
    private void RegisterBorder(Collider other)
    {
        if (State == ThrowState.Flying && InLayers(other, borderLayers)) touchedBorder = true;
    }
    private static bool InLayers(Collider other, LayerMask layers)
    {
        return (layers.value & (1 << other.gameObject.layer)) != 0;
    }

    private void UpdateGauge(float power)
    {
        if (powerSlider != null) powerSlider.normalizedValue = power / 100f;
    }
    private void SetGauge(bool visible)
    {
        if (gaugeRoot != null && gaugeRoot.activeSelf != visible) gaugeRoot.SetActive(visible);
        if (!visible || successBand == null) return;
        float required = Mathf.Clamp(requiredPowers[targetNumber - 1], 0f, 100f);
        Vector2 min = successBand.anchorMin, max = successBand.anchorMax;
        min.x = Mathf.Clamp01((required - tolerance) / 100f);
        max.x = Mathf.Clamp01((required + tolerance) / 100f);
        successBand.anchorMin = min;
        successBand.anchorMax = max;
        Vector2 low = successBand.offsetMin, high = successBand.offsetMax;
        low.x = high.x = 0f;
        successBand.offsetMin = low;
        successBand.offsetMax = high;
    }
    private void ShowResult(string message)
    {
        if (resultText != null && resultText.text != message) resultText.text = message;
    }
    private void SetTargetMarker(bool visible)
    {
        if (targetMarker != null && targetMarker.gameObject.activeSelf != visible)
            targetMarker.gameObject.SetActive(visible);
    }
    private void OnDisable()
    {
        if (enabledInput && rightTrigger != null && rightTrigger.action != null) rightTrigger.action.Disable();
        enabledInput = false;
        FreezeStone();
        SetGauge(false);
        ShowResult("");
        SetTargetMarker(false);
        SetState(ThrowState.Idle);
    }

    public void StopThrowing()
    {
        FreezeStone();
        SetGauge(false);
        ShowResult("");
        SetTargetMarker(false);
        SetState(ThrowState.Idle);
    }

    private void SetState(ThrowState next)
    {
        if (State == next) return;
        State = next;
        StateChanged?.Invoke(next);
    }
}
