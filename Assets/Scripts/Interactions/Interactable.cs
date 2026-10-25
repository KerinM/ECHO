using UnityEngine;

namespace Echo.Interactions
{
    /// <summary>
    /// Base de todo lo que el jugador puede activar con E (portales, fragmentos, núcleo).
    /// El GameObject debe tener un Collider trigger en la capa "Interactable":
    /// ese collider define la zona de interacción.
    /// </summary>
    public abstract class Interactable : MonoBehaviour
    {
        [Header("Interacción")]
        [SerializeField] private string promptText = "Pulsa E para interactuar";

        /// <summary>Mensaje contextual que se muestra al estar en la zona.</summary>
        public virtual string PromptText => promptText;

        /// <summary>Indica si en este momento se puede interactuar.</summary>
        public virtual bool CanInteract(GameObject interactor) => true;

        /// <summary>Se ejecuta una vez por cada pulsación de E.</summary>
        public abstract void Interact(GameObject interactor);
    }
}