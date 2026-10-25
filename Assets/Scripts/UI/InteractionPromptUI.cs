using Echo.Interactions;
using TMPro;
using UnityEngine;

namespace Echo.UI
{
    /// <summary>
    /// Muestra el mensaje contextual ("Pulsa E para...") del Interactable enfocado.
    /// Va en HUD_Canvas (NO en el objeto de texto, que se activa y desactiva).
    /// </summary>
    public class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private InteractionController interactionController;
        [Tooltip("Objeto de texto que se muestra u oculta.")]
        [SerializeField] private GameObject promptRoot;
        [SerializeField] private TMP_Text promptLabel;

        private void Awake()
        {
            if (interactionController == null || promptRoot == null || promptLabel == null)
            {
                Debug.LogError("InteractionPromptUI: faltan referencias en el Inspector " +
                               "(Interaction Controller, Prompt Root, Prompt Label).", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            interactionController.FocusChanged += HandleFocusChanged;
            HandleFocusChanged(interactionController.CurrentTarget);
        }

        private void OnDisable()
        {
            if (interactionController != null) interactionController.FocusChanged -= HandleFocusChanged;
        }

        private void HandleFocusChanged(Interactable target)
        {
            bool show = target != null;
            promptRoot.SetActive(show);
            if (show) promptLabel.text = target.PromptText;
        }
    }
}