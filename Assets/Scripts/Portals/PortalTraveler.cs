using System.Collections;
using Echo.CameraSystem;
using Echo.Player;
using Echo.UI;
using UnityEngine;

namespace Echo.Portals
{
    /// <summary>
    /// Va en el Player. Ejecuta el viaje entre portales: bloquea el control,
    /// hace fundido a negro, coloca al jugador en un punto seguro, reorienta la
    /// cámara y devuelve el control. Protege contra viajes repetidos.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(CharacterController))]
    public class PortalTraveler : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private ScreenFader screenFader;
        [SerializeField] private ThirdPersonCamera cameraRig;

        [Header("Transición")]
        [SerializeField] private float fadeOutDuration = 0.35f;
        [SerializeField] private float blackHoldDuration = 0.1f;
        [SerializeField] private float fadeInDuration = 0.35f;

        [Header("Protección")]
        [Tooltip("Segundos tras llegar durante los cuales no se puede usar otro portal.")]
        [SerializeField] private float cooldownSeconds = 1f;

        [Header("Destino seguro")]
        [SerializeField] private float probeHeight = 2f;
        [SerializeField] private float probeDistance = 6f;
        [SerializeField] private LayerMask groundMask = ~0;

        private PlayerController player;
        private CharacterController controller;
        private float nextAllowedTime;

        public bool IsTraveling { get; private set; }

        /// <summary>True si el jugador puede usar un portal ahora mismo.</summary>
        public bool CanTravel => !IsTraveling && Time.time >= nextAllowedTime;

        private void Awake()
        {
            player = GetComponent<PlayerController>();
            controller = GetComponent<CharacterController>();

            if (cameraRig == null)
            {
                Debug.LogError("PortalTraveler: falta asignar 'Camera Rig' (la Main Camera).", this);
                enabled = false;
            }

            if (screenFader == null)
            {
                Debug.LogWarning("PortalTraveler: no hay 'Screen Fader'; el viaje funcionará sin fundido.", this);
            }
        }

        /// <summary>Inicia un viaje. Devuelve false si no se puede (ya viajando o en enfriamiento).</summary>
        public bool TryTravel(Transform destination)
        {
            if (!enabled || destination == null || !CanTravel) return false;

            StartCoroutine(TravelRoutine(destination));
            return true;
        }

        private IEnumerator TravelRoutine(Transform destination)
        {
            IsTraveling = true;
            player.SetControlEnabled(false);

            if (screenFader != null) yield return screenFader.FadeTo(1f, fadeOutDuration);

            Vector3 position = ResolveSafePosition(destination.position);
            Quaternion rotation = Quaternion.Euler(0f, destination.eulerAngles.y, 0f);

            player.Teleport(position, rotation);
            cameraRig.SnapTo(rotation.eulerAngles.y);

            yield return new WaitForSeconds(blackHoldDuration);

            if (screenFader != null) yield return screenFader.FadeTo(0f, fadeInDuration);

            player.SetControlEnabled(true);
            IsTraveling = false;
            nextAllowedTime = Time.time + cooldownSeconds;
        }

        /// <summary>
        /// Ajusta el destino al suelo real (raycast hacia abajo) y avisa si el espacio está ocupado.
        /// </summary>
        private Vector3 ResolveSafePosition(Vector3 target)
        {
            Vector3 origin = target + Vector3.up * probeHeight;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, probeHeight + probeDistance,
                    groundMask, QueryTriggerInteraction.Ignore))
            {
                target = hit.point + Vector3.up * 0.05f;
            }
            else
            {
                Debug.LogWarning($"PortalTraveler: no se encontró suelo bajo el destino {target}. " +
                                 "Se usa la posición tal cual; revisa la altura del punto de llegada.", this);
            }

            float radius = controller.radius;
            Vector3 bottom = target + Vector3.up * (radius + 0.05f);
            Vector3 top = target + Vector3.up * (controller.height - radius);
            if (Physics.CheckCapsule(bottom, top, radius * 0.9f, groundMask, QueryTriggerInteraction.Ignore))
            {
                Debug.LogWarning($"PortalTraveler: el destino {target} parece ocupado por un collider. " +
                                 "Mueve el punto de llegada.", this);
            }

            return target;
        }
    }
}