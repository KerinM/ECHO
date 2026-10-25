using System;
using System.Collections.Generic;
using UnityEngine;

namespace Echo.Mission
{
    /// <summary>Etapas de la misión, según la historia del juego.</summary>
    public enum MissionStage
    {
        ExploreSanctuary,    // Etapa 1: explorar y localizar el primer fragmento
        PortalAndFragments,  // Etapa 2: usar el portal, recoger los demás y regresar
        ActivateCore,        // Etapa 3: interactuar con el núcleo
        Completed            // Misión terminada
    }

    /// <summary>
    /// Estado de la misión: fragmentos recogidos, etapa actual y victoria.
    /// No depende del jugador, la UI ni los portales: ellos escuchan sus eventos.
    /// </summary>
    public class MissionManager : MonoBehaviour
    {
        [Header("Misión")]
        [Min(1)][SerializeField] private int totalFragments = 3;

        private readonly HashSet<string> registeredIds = new HashSet<string>();
        private readonly HashSet<string> collectedIds = new HashSet<string>();

        /// <summary>Se dispara al recoger un fragmento nuevo (recogidos, total).</summary>
        public event Action<int, int> FragmentCollected;

        /// <summary>Se dispara cuando cambia la etapa.</summary>
        public event Action<MissionStage> StageChanged;

        /// <summary>Se dispara una sola vez, al completar la misión.</summary>
        public event Action MissionCompleted;

        public int CollectedCount => collectedIds.Count;
        public int TotalFragments => totalFragments;
        public bool AllFragmentsCollected => collectedIds.Count >= totalFragments;
        public MissionStage Stage { get; private set; } = MissionStage.ExploreSanctuary;
        public bool IsCompleted => Stage == MissionStage.Completed;

        /// <summary>Texto de objetivo para mostrar en la interfaz.</summary>
        public string StageDescription
        {
            get
            {
                switch (Stage)
                {
                    case MissionStage.ExploreSanctuary:
                        return "Explora el santuario y encuentra el primer fragmento de energía.";
                    case MissionStage.PortalAndFragments:
                        return "Cruza el portal y encuentra los fragmentos restantes.";
                    case MissionStage.ActivateCore:
                        return "Regresa al santuario y activa el núcleo ancestral.";
                    default:
                        return "Santuario restaurado.";
                }
            }
        }

        private void Start()
        {
            Debug.Log($"[Misión] Iniciada. Fragmentos: 0/{totalFragments}.", this);
        }

        /// <summary>
        /// Los fragmentos se registran al iniciar para detectar IDs vacíos o repetidos.
        /// Devuelve false si el ID no es válido.
        /// </summary>
        public bool RegisterFragment(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogError("MissionManager: un fragmento tiene el ID vacío.", this);
                return false;
            }

            if (!registeredIds.Add(id))
            {
                Debug.LogError($"MissionManager: el ID de fragmento '{id}' está repetido. " +
                               "Cada fragmento necesita un ID único.", this);
                return false;
            }

            return true;
        }

        /// <summary>Marca un fragmento como recogido. Devuelve false si ya estaba recogido o la misión terminó.</summary>
        public bool CollectFragment(string id)
        {
            if (IsCompleted || string.IsNullOrWhiteSpace(id)) return false;

            if (!collectedIds.Add(id))
            {
                Debug.LogWarning($"MissionManager: el fragmento '{id}' ya estaba recogido.", this);
                return false;
            }

            Debug.Log($"[Misión] Fragmento '{id}' recogido: {collectedIds.Count}/{totalFragments}.", this);
            FragmentCollected?.Invoke(collectedIds.Count, totalFragments);
            RefreshStage();
            return true;
        }

        /// <summary>Completa la misión si ya están todos los fragmentos. Devuelve true si se completó.</summary>
        public bool CompleteMission()
        {
            if (IsCompleted || !AllFragmentsCollected) return false;

            SetStage(MissionStage.Completed);
            Debug.Log("[Misión] ¡Santuario restaurado!", this);
            MissionCompleted?.Invoke();
            return true;
        }

        private void RefreshStage()
        {
            if (IsCompleted) return;

            MissionStage next;
            if (collectedIds.Count >= totalFragments) next = MissionStage.ActivateCore;
            else if (collectedIds.Count >= 1) next = MissionStage.PortalAndFragments;
            else next = MissionStage.ExploreSanctuary;

            SetStage(next);
        }

        private void SetStage(MissionStage next)
        {
            if (Stage == next) return;

            Stage = next;
            StageChanged?.Invoke(Stage);
        }
    }
}