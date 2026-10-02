// 사방치기의 시작과 전체 상태를 관리하고 투척 상태 변경을 연결한다.
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class SabangGameManager : MonoBehaviour
{
    public enum GameState
    {
        Ready, SelectingPosition, SelectingPower, Throwing, MovingForward,
        Turning, Returning, PickingUpStone, RoundComplete, ClaimingLand, Cleared
    }

    [SerializeField] private SabangStone stone;
    [Tooltip("투척 성공 후 전진 시작 알림. 실제 이동은 후속 구현에서 연결한다.")]
    [SerializeField] private UnityEvent onMovingForward = new UnityEvent();
    [Tooltip("일반 이동·회전·텔레포트 입력 컴포넌트만 연결. XR Origin, 입력 관리자, 추적, QTE 이동 컴포넌트는 제외.")]
    [SerializeField] private Behaviour[] manualMovementComponents = new Behaviour[0];
    public GameState State { get; private set; } = GameState.Ready;
    public int CurrentTargetNumber { get; private set; } = 1;
    public event System.Action<GameState> StateChanged;

    private SabangStone subscribedStone;
    private bool starting;
    private readonly Dictionary<Behaviour, bool> movementStates = new Dictionary<Behaviour, bool>();

    private void OnEnable()
    {
        subscribedStone = stone;
        if (subscribedStone != null) subscribedStone.StateChanged += HandleThrowState;
    }

    public bool CanStartGame()
    {
        if (!isActiveAndEnabled || starting || State != GameState.Ready) return false;
        if (stone == null || subscribedStone != stone || stone.StartsAutomatically)
        {
            Debug.LogError("SabangGameManager: Stone을 연결하고 Start Automatically를 끈 뒤 실행하세요.", this);
            return false;
        }
        return stone.CanBeginRound(1) && stone.State == SabangStone.ThrowState.Idle;
    }

    // UIDoor가 SpawnPoint 이동 성공을 확인한 뒤 호출한다.
    public bool TryStartGame()
    {
        if (!CanStartGame()) return false;
        starting = true;
        CurrentTargetNumber = 1;
        bool started = false;
        try
        {
            LockManualMovement();
            started = stone.TryBeginRound(CurrentTargetNumber);
            return started;
        }
        finally
        {
            starting = false;
            if (!started) RestoreManualMovement();
        }
    }

    private void HandleThrowState(SabangStone.ThrowState next)
    {
        if (!starting && State == GameState.Ready) return;
        switch (next)
        {
            case SabangStone.ThrowState.Selecting: SetState(GameState.SelectingPosition); break;
            case SabangStone.ThrowState.Charging: SetState(GameState.SelectingPower); break;
            case SabangStone.ThrowState.Flying:
            case SabangStone.ThrowState.Result: SetState(GameState.Throwing); break;
            case SabangStone.ThrowState.Complete: SetState(GameState.MovingForward); break;
            case SabangStone.ThrowState.Idle: SetState(GameState.Ready); break;
        }
    }

    private void SetState(GameState next)
    {
        if (next == GameState.Ready || next == GameState.Cleared) RestoreManualMovement();
        if (State == next) return;
        State = next;
        StateChanged?.Invoke(next);
        if (next == GameState.MovingForward) onMovingForward.Invoke();
    }

    private void LockManualMovement()
    {
        if (movementStates.Count > 0 || manualMovementComponents == null) return;
        // 중복 참조가 있어도 비활성화 전 상태를 한 번만 저장한다.
        foreach (Behaviour component in manualMovementComponents)
        {
            if (component == null || component == this || component == stone ||
                movementStates.ContainsKey(component)) continue;
            movementStates.Add(component, component.enabled);
        }
        foreach (var entry in movementStates) entry.Key.enabled = false;
    }

    private void RestoreManualMovement()
    {
        foreach (var entry in movementStates)
        {
            if (entry.Key != null) entry.Key.enabled = entry.Value;
        }
        movementStates.Clear();
    }

    public void StopGame()
    {
        if (stone != null && State != GameState.Ready) stone.StopThrowing();
        SetState(GameState.Ready);
    }

    private void OnDisable()
    {
        if (subscribedStone != null)
        {
            subscribedStone.StateChanged -= HandleThrowState;
            if (State != GameState.Ready) subscribedStone.StopThrowing();
        }
        subscribedStone = null;
        starting = false;
        SetState(GameState.Ready);
    }
}
