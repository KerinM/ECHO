using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Echo.Interactions
{
    /// <summary>
    /// Va en el Player. Detecta qué Interactable tiene el jugador dentro de su zona
    /// y lo ejecuta al pulsar E (una vez por pulsación).
    /// </summary>
    public class InteractionController : MonoBehaviour
    {
        [Header("Detección")]
        [Tooltip("Capa(s) donde están las zonas de interacción. Debe ser 'Interactable'.")]
        [SerializeField] private LayerMask interactableMask;
        [Tooltip("Radio de la esfera que representa al jugador al detectar zonas.")]
        [SerializeField] private float detectionRadius = 0.5f;
        [Tooltip("Desplazamiento local del centro de detección (aprox. el torso).")]
        [SerializeField] private Vector3 detectionOffset = new Vector3(0f, 0.9f, 0f);

        private const int BufferSize = 16;
        private readonly Collider[] hits = new Collider[BufferSize];
        private Interactable current;

        /// <summary>Interactable actualmente enfocado (puede ser null).</summary>
        public Interactable CurrentTarget => current;

        /// <summary>Se dispara solo cuando cambia el objetivo enfocado (incluye null).</summary>
        public event Action<Interactable> FocusChanged;

        private void Awake()
        {
            if (interactableMask.value == 0)
            {
                Debug.LogError("InteractionController: 'Interactable Mask' está vacío. " +
                               "Selecciona la capa 'Interactable' en el Inspector.", this);
                enabled = false;
            }
        }

        private void OnDisable()
        {
            if (current != null)
            {
                current = null;
                FocusChanged?.Invoke(null);
            }
        }

        private void Update()
        {
            Interactable best = FindBestInteractable();
            if (best != current)
            {
                current = best;
                FocusChanged?.Invoke(current);
            }

            if (current != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                current.Interact(gameObject);
            }
        }

        private Interactable FindBestInteractable()
        {
            Vector3 center = transform.TransformPoint(detectionOffset);
            int count = Physics.OverlapSphereNonAlloc(
                center, detectionRadius, hits, interactableMask, QueryTriggerInteraction.Collide);

            Interactable best = null;
            float bestSqrDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                Interactable candidate = hits[i].GetComponentInParent<Interactable>();
                if (candidate == null || !candidate.isActiveAndEnabled) continue;
                if (!candidate.CanInteract(gameObject)) continue;

                float sqrDistance = (candidate.transform.position - center).sqrMagnitude;
                if (sqrDistance < bestSqrDistance)
                {
                    bestSqrDistance = sqrDistance;
                    best = candidate;
                }
            }

            return best;
        }
    }
}