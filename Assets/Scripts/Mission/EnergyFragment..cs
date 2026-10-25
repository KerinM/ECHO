using Echo.Interactions;
using UnityEngine;

namespace Echo.Mission
{
    /// <summary>
    /// Fragmento de energía reutilizable. Va en la raíz del fragmento, que también tiene
    /// un Sphere Collider (trigger) en la capa "Interactable".
    /// Cada instancia necesita un ID único y la referencia a MissionManager.
    /// </summary>
    public class EnergyFragment : Interactable
    {
        [Header("Fragmento")]
        [Tooltip("Identificador único: fragment_1, fragment_2, fragment_3.")]
        [SerializeField] private string fragmentId = "fragment_1";
        [SerializeField] private MissionManager mission;

        [Header("Animación")]
        [Tooltip("Objeto hijo que gira y flota (el cristal).")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private float spinSpeed = 90f;
        [SerializeField] private float bobAmplitude = 0.15f;
        [SerializeField] private float bobSpeed = 2f;

        private bool collected;
        private Vector3 visualStartLocalPosition;
        private float bobPhase;

        public string FragmentId => fragmentId;
        public bool IsCollected => collected;

        private void Awake()
        {
            if (mission == null)
            {
                Debug.LogError($"EnergyFragment '{name}': falta asignar 'Mission' (MissionManager).", this);
                enabled = false;
                return;
            }

            if (string.IsNullOrWhiteSpace(fragmentId))
            {
                Debug.LogError($"EnergyFragment '{name}': el 'Fragment Id' está vacío.", this);
                enabled = false;
                return;
            }

            if (visualRoot != null) visualStartLocalPosition = visualRoot.localPosition;
            bobPhase = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Start()
        {
            // Si el ID es inválido o repetido, el fragmento se desactiva y deja de ser interactuable.
            if (!mission.RegisterFragment(fragmentId)) enabled = false;
        }

        private void Update()
        {
            if (visualRoot == null) return;

            visualRoot.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.Self);
            float offset = Mathf.Sin(Time.time * bobSpeed + bobPhase) * bobAmplitude;
            visualRoot.localPosition = visualStartLocalPosition + Vector3.up * offset;
        }

        public override bool CanInteract(GameObject interactor)
        {
            return !collected && mission != null && !mission.IsCompleted;
        }

        public override void Interact(GameObject interactor)
        {
            if (collected) return;
            if (!mission.CollectFragment(fragmentId)) return;

            collected = true;
            gameObject.SetActive(false);
        }
    }
}