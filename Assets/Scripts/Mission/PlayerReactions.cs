using Echo.Interactions;
using Echo.Player;
using UnityEngine;

namespace Echo.Mission
{
    /// <summary>
    /// Va en el Player. Escucha las interacciones (recoger fragmento, usar portal, activar el núcleo)
    /// y el fin de la misión, y pide al Animator el gesto correspondiente. No cambia el
    /// comportamiento de ningún sistema: solo reacciona a sus eventos.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerReactions : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private InteractionController interactionController;
        [SerializeField] private MissionManager missionManager;
        [SerializeField] private PlayerAnimator playerAnimator;

        private void Awake()
        {
            if (interactionController == null || missionManager == null || playerAnimator == null)
            {
                Debug.LogError("PlayerReactions: faltan referencias (InteractionController, " +
                               "MissionManager, PlayerAnimator).", this);
                enabled = false;
                return;
            }

            interactionController.Interacted += OnInteracted;
            missionManager.MissionCompleted += OnMissionCompleted;
        }

        private void OnDestroy()
        {
            if (interactionController != null)
            {
                interactionController.Interacted -= OnInteracted;
            }

            if (missionManager != null)
            {
                missionManager.MissionCompleted -= OnMissionCompleted;
            }
        }

        private void OnInteracted(Interactable target)
        {
            playerAnimator.PlayInteract();
        }

        private void OnMissionCompleted()
        {
            playerAnimator.PlayVictory();
        }
    }
}
