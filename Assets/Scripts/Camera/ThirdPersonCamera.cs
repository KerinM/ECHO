using UnityEngine;
using UnityEngine.InputSystem;

namespace Echo.CameraSystem
{
    /// <summary>
    /// Cámara en tercera persona con órbita de ratón, límites verticales,
    /// seguimiento suave y prevención básica de que atraviese paredes.
    /// Esc libera o recupera el cursor.
    /// </summary>
    public class ThirdPersonCamera : MonoBehaviour
    {
        [Header("Objetivo")]
        [Tooltip("Punto que la cámara mira, normalmente CameraTarget (hijo del Player).")]
        [SerializeField] private Transform target;

        [Header("Distancia")]
        [SerializeField] private float distance = 5f;
        [SerializeField] private float minDistance = 1f;

        [Header("Ratón")]
        [SerializeField] private float sensitivity = 0.12f;
        [SerializeField] private float minPitch = -25f;
        [SerializeField] private float maxPitch = 60f;
        [SerializeField] private float initialPitch = 15f;

        [Header("Suavizado")]
        [Tooltip("Tiempo de suavizado del seguimiento. 0 = sin suavizado.")]
        [SerializeField] private float followSmoothTime = 0.05f;

        [Header("Colisión de cámara")]
        [SerializeField] private float collisionRadius = 0.25f;
        [SerializeField] private LayerMask collisionMask = ~0;

        [Header("Cursor")]
        [SerializeField] private bool lockCursorOnStart = true;

        private float yaw;
        private float pitch;
        private float currentDistance;
        private Vector3 smoothedPivot;
        private Vector3 pivotVelocity;
        private bool cursorLocked;
        private bool inputEnabled = true;

        public float Yaw => yaw;

        private void Start()
        {
            if (target == null)
            {
                Debug.LogError("ThirdPersonCamera: falta asignar el 'Target' (CameraTarget) en el Inspector.", this);
                enabled = false;
                return;
            }

            yaw = target.eulerAngles.y;
            pitch = Mathf.Clamp(initialPitch, minPitch, maxPitch);
            currentDistance = distance;
            smoothedPivot = target.position;

            SetCursorLocked(lockCursorOnStart);
        }

        private void Update()
        {
            if (!inputEnabled) return;

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                SetCursorLocked(!cursorLocked);
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            if (inputEnabled && cursorLocked && Mouse.current != null)
            {
                Vector2 delta = Mouse.current.delta.ReadValue();
                yaw += delta.x * sensitivity;
                pitch -= delta.y * sensitivity;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }

            smoothedPivot = followSmoothTime > 0f
                ? Vector3.SmoothDamp(smoothedPivot, target.position, ref pivotVelocity, followSmoothTime)
                : target.position;

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 direction = rotation * Vector3.back;

            float targetDistance = distance;
            if (Physics.SphereCast(smoothedPivot, collisionRadius, direction, out RaycastHit hit,
                    distance, collisionMask, QueryTriggerInteraction.Ignore))
            {
                targetDistance = Mathf.Max(minDistance, hit.distance);
            }

            // Se acerca de inmediato si hay un obstáculo; se aleja de forma gradual.
            currentDistance = targetDistance < currentDistance
                ? targetDistance
                : Mathf.MoveTowards(currentDistance, targetDistance, 10f * Time.deltaTime);

            transform.SetPositionAndRotation(smoothedPivot + direction * currentDistance, rotation);
        }

        /// <summary>
        /// Coloca la cámara de inmediato detrás del objetivo con un giro horizontal dado.
        /// Se usa después de un teletransporte para evitar que el suavizado arrastre la cámara.
        /// </summary>
        public void SnapTo(float newYaw)
        {
            if (target == null) return;

            yaw = newYaw;
            smoothedPivot = target.position;
            pivotVelocity = Vector3.zero;
            currentDistance = distance;
        }

        /// <summary>
        /// Activa o bloquea el control de la cámara por el jugador.
        /// Al bloquear, también libera el cursor (necesario para pulsar botones de la interfaz).
        /// </summary>
        public void SetInputEnabled(bool value)
        {
            inputEnabled = value;
            SetCursorLocked(value);
        }

        private void SetCursorLocked(bool locked)
        {
            cursorLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}