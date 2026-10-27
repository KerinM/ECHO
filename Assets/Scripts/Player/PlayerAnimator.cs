using System.Collections.Generic;
using UnityEngine;

namespace Echo.Player
{
    /// <summary>
    /// Alimenta el Animator del modelo con la velocidad real del CharacterController.
    /// No mueve al personaje. Los parámetros que no existan en el Animator Controller se ignoran.
    /// Parámetros esperados: Speed (float), Grounded (bool), Interact (trigger), Victory (trigger).
    /// </summary>
    public class PlayerAnimator : MonoBehaviour
    {
        [Header("Referencias")]
        [Tooltip("CharacterController del Player. Si se deja vacío se busca en este objeto.")]
        [SerializeField] private CharacterController controller;
        [Tooltip("Animator del modelo (PlayerModel).")]
        [SerializeField] private Animator animator;

        [Header("Nombres de parámetros")]
        [SerializeField] private string speedParam = "Speed";
        [SerializeField] private string groundedParam = "Grounded";
        [SerializeField] private string interactTrigger = "Interact";
        [SerializeField] private string victoryTrigger = "Victory";

        [Header("Suavizado")]
        [SerializeField] private float speedDampTime = 0.1f;

        private readonly HashSet<int> availableParams = new HashSet<int>();
        private int speedId;
        private int groundedId;
        private int interactId;
        private int victoryId;

        private void Awake()
        {
            if (controller == null) controller = GetComponent<CharacterController>();

            if (controller == null || animator == null)
            {
                Debug.LogError("PlayerAnimator: faltan referencias (Controller, Animator).", this);
                enabled = false;
                return;
            }

            if (animator.runtimeAnimatorController == null)
            {
                Debug.LogError("PlayerAnimator: el Animator no tiene un Animator Controller asignado.", this);
                enabled = false;
                return;
            }

            // El movimiento lo hace el CharacterController, no la animación.
            animator.applyRootMotion = false;

            foreach (AnimatorControllerParameter p in animator.parameters)
            {
                availableParams.Add(p.nameHash);
            }

            speedId = Animator.StringToHash(speedParam);
            groundedId = Animator.StringToHash(groundedParam);
            interactId = Animator.StringToHash(interactTrigger);
            victoryId = Animator.StringToHash(victoryTrigger);

            if (!availableParams.Contains(speedId))
            {
                Debug.LogWarning($"PlayerAnimator: el Animator no tiene el parámetro '{speedParam}'. " +
                                 "El personaje no cambiará de animación al moverse.", this);
            }
        }

        private void Update()
        {
            Vector3 velocity = controller.velocity;
            float horizontalSpeed = new Vector2(velocity.x, velocity.z).magnitude;
            if (horizontalSpeed < 0.1f) horizontalSpeed = 0f;

            if (availableParams.Contains(speedId))
            {
                animator.SetFloat(speedId, horizontalSpeed, speedDampTime, Time.deltaTime);
            }

            if (availableParams.Contains(groundedId))
            {
                animator.SetBool(groundedId, controller.isGrounded);
            }
        }

        /// <summary>Lanza el gesto de interacción (recoger, activar). Se usa en la etapa 2.</summary>
        public void PlayInteract()
        {
            if (enabled && availableParams.Contains(interactId)) animator.SetTrigger(interactId);
        }

        /// <summary>Lanza la animación de victoria. Se usa en la etapa 2.</summary>
        public void PlayVictory()
        {
            if (enabled && availableParams.Contains(victoryId)) animator.SetTrigger(victoryId);
        }
    }
}