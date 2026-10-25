using UnityEngine;
using UnityEngine.InputSystem;

namespace Echo.Player
{
    /// <summary>
    /// Única implementación de movimiento del jugador.
    /// Movimiento relativo a la cámara, caminar/correr, gravedad y rotación suave.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Referencias")]
        [Tooltip("Transform de la cámara. Si se deja vacío se usa Camera.main.")]
        [SerializeField] private Transform cameraTransform;

        [Header("Movimiento")]
        [SerializeField] private float walkSpeed = 4f;
        [SerializeField] private float runSpeed = 7f;
        [Tooltip("Unidades/segundo² para acelerar o frenar.")]
        [SerializeField] private float acceleration = 30f;
        [Tooltip("Tiempo en segundos para girar hacia la dirección de movimiento.")]
        [SerializeField] private float rotationSmoothTime = 0.1f;

        [Header("Gravedad")]
        [SerializeField] private float gravity = -20f;
        [Tooltip("Pequeña fuerza hacia abajo para mantener al personaje pegado al suelo.")]
        [SerializeField] private float groundedStickForce = -2f;

        private CharacterController controller;
        private Vector3 horizontalVelocity;
        private float verticalVelocity;
        private float rotationVelocity;
        private bool controlEnabled = true;

        public bool IsGrounded => controller != null && controller.isGrounded;
        public bool IsRunning { get; private set; }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            if (cameraTransform == null)
            {
                if (Camera.main != null)
                {
                    cameraTransform = Camera.main.transform;
                    Debug.LogWarning("PlayerController: no había cámara asignada; se usa Camera.main. " +
                                     "Asigna la Main Camera en el Inspector.", this);
                }
                else
                {
                    Debug.LogError("PlayerController: falta la referencia a la cámara y no existe Camera.main.", this);
                    enabled = false;
                }
            }
        }

        private void Update()
        {
            Vector2 input = controlEnabled ? ReadMoveInput() : Vector2.zero;
            IsRunning = controlEnabled && Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;

            Vector3 moveDirection = GetCameraRelativeDirection(input);
            float speed = IsRunning ? runSpeed : walkSpeed;
            Vector3 targetVelocity = moveDirection * speed;

            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity, targetVelocity, acceleration * Time.deltaTime);

            RotateTowards(moveDirection);
            ApplyGravity();

            Vector3 velocity = horizontalVelocity + Vector3.up * verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
        }

        /// <summary>Activa o bloquea el control del jugador (viajes, pantalla de victoria...).</summary>
        public void SetControlEnabled(bool value)
        {
            controlEnabled = value;
            if (!value)
            {
                horizontalVelocity = Vector3.zero;
            }
        }

        /// <summary>
        /// Coloca al jugador en una posición y rotación nuevas de forma segura.
        /// Un CharacterController se debe desactivar al mover el Transform directamente.
        /// </summary>
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            controller.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            controller.enabled = true;
            Physics.SyncTransforms();

            horizontalVelocity = Vector3.zero;
            verticalVelocity = groundedStickForce;
            rotationVelocity = 0f;
        }

        private Vector2 ReadMoveInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return Vector2.zero;

            float x = 0f;
            float y = 0f;
            if (keyboard.dKey.isPressed) x += 1f;
            if (keyboard.aKey.isPressed) x -= 1f;
            if (keyboard.wKey.isPressed) y += 1f;
            if (keyboard.sKey.isPressed) y -= 1f;

            Vector2 input = new Vector2(x, y);
            return input.sqrMagnitude > 1f ? input.normalized : input;
        }

        private Vector3 GetCameraRelativeDirection(Vector2 input)
        {
            if (input == Vector2.zero) return Vector3.zero;

            Vector3 forward = cameraTransform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f) forward = transform.forward;
            forward.Normalize();

            Vector3 right = cameraTransform.right;
            right.y = 0f;
            right.Normalize();

            Vector3 direction = forward * input.y + right * input.x;
            return direction.sqrMagnitude > 1f ? direction.normalized : direction;
        }

        private void RotateTowards(Vector3 moveDirection)
        {
            if (moveDirection.sqrMagnitude < 0.01f) return;

            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y, targetAngle, ref rotationVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }

        private void ApplyGravity()
        {
            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = groundedStickForce;
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }
        }
    }
}