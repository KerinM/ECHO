using Echo.Interactions;
using Echo.Mission;
using UnityEngine;

namespace Echo.Portals
{
    /// <summary>
    /// Lógica de un portal. Va en el objeto raíz del portal, que también tiene el
    /// Sphere Collider (trigger) en la capa "Interactable".
    /// 'destination' es el punto donde aparece el jugador al usar ESTE portal.
    /// Opcionalmente el portal está dormido hasta recoger cierto número de fragmentos.
    /// </summary>
    public class PortalController : Interactable
    {
        [Header("Destino")]
        [Tooltip("Punto de llegada (junto al portal opuesto). Su rotación en Y define hacia dónde mira el jugador.")]
        [SerializeField] private Transform destination;

        [Header("Activación por misión")]
        [Tooltip("Solo necesario si 'Required Fragments' es mayor que 0.")]
        [SerializeField] private MissionManager mission;
        [Tooltip("Fragmentos necesarios para activar el portal. 0 = siempre activo.")]
        [Min(0)][SerializeField] private int requiredFragments = 0;
        [SerializeField] private string lockedPromptText = "El portal está dormido. Encuentra el primer fragmento.";

        private GameObject cachedInteractor;
        private PortalTraveler cachedTraveler;

        private bool IsLocked =>
            mission != null && requiredFragments > 0 && mission.CollectedCount < requiredFragments;

        public override string PromptText => IsLocked ? lockedPromptText : base.PromptText;

        private void Awake()
        {
            if (destination == null)
            {
                Debug.LogError($"PortalController en '{name}': falta asignar 'Destination'.", this);
            }

            if (requiredFragments > 0 && mission == null)
            {
                Debug.LogError($"PortalController en '{name}': 'Required Fragments' es {requiredFragments} " +
                               "pero falta asignar 'Mission'. El portal quedará siempre activo.", this);
            }
        }

        public override bool CanInteract(GameObject interactor)
        {
            if (destination == null) return false;

            PortalTraveler traveler = GetTraveler(interactor);
            return traveler != null && traveler.CanTravel;
        }

        public override void Interact(GameObject interactor)
        {
            if (IsLocked) return;

            PortalTraveler traveler = GetTraveler(interactor);
            if (traveler == null || destination == null) return;

            traveler.TryTravel(destination);
        }

        private PortalTraveler GetTraveler(GameObject interactor)
        {
            if (interactor != cachedInteractor)
            {
                cachedInteractor = interactor;
                cachedTraveler = interactor != null ? interactor.GetComponent<PortalTraveler>() : null;

                if (cachedTraveler == null)
                {
                    Debug.LogError("PortalController: el jugador no tiene el componente PortalTraveler.", this);
                }
            }

            return cachedTraveler;
        }
    }
}