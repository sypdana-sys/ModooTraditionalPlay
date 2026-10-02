// 장구채가 컨트롤러를 속도 기반으로 따라가게 해서 장구 같은 단단한 Collider를 통과하지 않게 한다.
using UnityEngine;

namespace FindOurSound.JangGuRhythm
{
    /// <summary>
    /// 시작 시 부모(StickPivot)를 추적 대상으로 기억한다. JangGuSettings.sticksCollideWithJanggu에 따라 두 모드를 오간다.
    /// - 막힘: 부모에서 분리하고 FixedUpdate마다 대상 위치·회전으로 가는 속도를 Rigidbody에 넣는다.
    ///   속도로 움직이므로 물리 엔진이 장구 표면에서 멈춰 세운다.
    /// - 통과: 대상 아래로 다시 붙이고 Kinematic으로 둔다. 예전처럼 컨트롤러에 붙어 장구를 통과한다.
    /// Play 중에 설정을 바꾸면 다음 물리 프레임에 모드가 바뀐다.
    /// 분리되면 컨트롤러의 ControllerSideMarker를 찾을 수 없으므로 채 루트에도 ControllerSideMarker를 둔다.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class StickPhysicsFollower : MonoBehaviour
    {
        [Tooltip("채 충돌 여부를 읽을 설정. 비우면 항상 장구에 막힌다.")]
        [SerializeField] JangGuSettings settings;

        [Tooltip("따라갈 대상. 비우면 시작 시의 부모(StickPivot)를 사용한다.")]
        [SerializeField] Transform target;

        [Tooltip("대상과 이 거리(m) 이상 벌어지면 순간이동한다. 스냅 회전·텔레포트 이동 후 채가 날아가는 것을 막는다.")]
        [SerializeField] float snapDistance = 0.5f;

        [Tooltip("최대 회전 속도(rad/s). 빠른 손목 스냅을 따라가도록 Unity 기본값(7)보다 높게 둔다.")]
        [SerializeField] float maxAngularSpeed = 50f;

        /// <summary>채가 따라가는 대상(StickPivot). 진동을 보낼 컨트롤러를 찾을 때 쓴다.</summary>
        public Transform Target => target;

        Rigidbody body;
        Vector3 localPosition;
        Quaternion localRotation = Quaternion.identity;
        bool collidesWithJanggu;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.useGravity = false;
            body.maxAngularVelocity = maxAngularSpeed;

            if (target == null) target = transform.parent;
            if (target == null)
            {
                Debug.LogError($"[JangGuRhythm] {name}: StickPhysicsFollower의 따라갈 대상이 없습니다. StickPivot 아래에 두거나 Target을 연결하세요.");
                enabled = false;
                return;
            }

            // 대상 기준 상대 위치를 기억해 두면 Inspector에서 맞춘 채 위치·회전이 그대로 유지된다.
            localPosition = target.InverseTransformPoint(transform.position);
            localRotation = Quaternion.Inverse(target.rotation) * transform.rotation;

            IgnoreRigColliders(transform.root);
            ApplyMode(ShouldCollide());
        }

        bool ShouldCollide()
        {
            return settings == null || settings.sticksCollideWithJanggu;
        }

        void ApplyMode(bool collide)
        {
            collidesWithJanggu = collide;

            if (collide)
            {
                transform.SetParent(null, true);
                body.isKinematic = false;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                body.interpolation = RigidbodyInterpolation.Interpolate;
                return;
            }

            // Kinematic은 ContinuousDynamic을 지원하지 않으므로 먼저 바꾼다.
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            body.isKinematic = true;
            // 부모를 따라 움직이는 동안 보간이 켜져 있으면 위치가 한 프레임씩 늦게 보여 떨린다.
            body.interpolation = RigidbodyInterpolation.None;
            transform.SetParent(target, false);
            transform.localPosition = localPosition;
            transform.localRotation = localRotation;
        }

        /// <summary>플레이어 몸(CharacterController 등)에 채가 걸리지 않도록 같은 리그 안의 Collider와 충돌하지 않게 한다.</summary>
        void IgnoreRigColliders(Transform rigRoot)
        {
            Collider[] ownColliders = GetComponentsInChildren<Collider>(true);
            foreach (Collider rigCollider in rigRoot.GetComponentsInChildren<Collider>(true))
            {
                if (rigCollider.transform.IsChildOf(transform)) continue;
                foreach (Collider own in ownColliders)
                {
                    Physics.IgnoreCollision(own, rigCollider, true);
                }
            }
        }

        void FixedUpdate()
        {
            if (target == null) return;

            bool collide = ShouldCollide();
            if (collide != collidesWithJanggu) ApplyMode(collide);
            if (!collidesWithJanggu) return;

            Vector3 targetPosition = target.TransformPoint(localPosition);
            Quaternion targetRotation = target.rotation * localRotation;

            if (Vector3.Distance(body.position, targetPosition) > snapDistance)
            {
                body.position = targetPosition;
                body.rotation = targetRotation;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                return;
            }

            float dt = Time.fixedDeltaTime;
            body.linearVelocity = (targetPosition - body.position) / dt;

            Quaternion delta = targetRotation * Quaternion.Inverse(body.rotation);
            delta.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180f) angle -= 360f;
            body.angularVelocity = Mathf.Abs(angle) < 0.01f || float.IsNaN(axis.x)
                ? Vector3.zero
                : axis * (angle * Mathf.Deg2Rad / dt);
        }
    }
}
