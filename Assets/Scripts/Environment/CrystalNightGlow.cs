using UnityEngine;

namespace Echo.World
{
    [DisallowMultipleComponent]
    public class CrystalNightGlow : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private TimeOfDayController timeOfDay;
        [Tooltip("Padre de todos los cristales (por ejemplo Crystals_B).")]
        [SerializeField] private Transform crystalsRoot;

        [Header("Brillo")]
        [Tooltip("Multiplicador de la emisión durante el día.")]
        [SerializeField, Min(0f)] private float dayMultiplier = 0.6f;
        [Tooltip("Multiplicador de la emisión durante la noche.")]
        [SerializeField, Min(0f)] private float nightMultiplier = 2.5f;
        [SerializeField] private bool controlLights = true;
        [Tooltip("Si la emisión del material es negra, usa su color base como emisión.")]
        [SerializeField] private bool useBaseColorWhenEmissionIsBlack = true;

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private Renderer[] renderers;
        private Color[] baseEmission;
        private bool[] hasEmission;
        private Light[] lights;
        private float[] baseLightIntensity;
        private MaterialPropertyBlock block;

        private void Awake()
        {
            if (timeOfDay == null || crystalsRoot == null)
            {
                Debug.LogError("CrystalNightGlow: faltan referencias (Time Of Day, Crystals Root).", this);
                enabled = false;
                return;
            }

            block = new MaterialPropertyBlock();

            renderers = crystalsRoot.GetComponentsInChildren<Renderer>(true);
            baseEmission = new Color[renderers.Length];
            hasEmission = new bool[renderers.Length];

            int emissiveCount = 0;
            for (int i = 0; i < renderers.Length; i++)
            {
                Material material = renderers[i].sharedMaterial;
                if (material == null || !material.HasProperty(EmissionColorId))
                {
                    continue;
                }

                Color emission = material.GetColor(EmissionColorId);
                if (emission.maxColorComponent <= 0.001f && useBaseColorWhenEmissionIsBlack &&
                    material.HasProperty(BaseColorId))
                {
                    emission = material.GetColor(BaseColorId);
                }

                if (emission.maxColorComponent <= 0.001f)
                {
                    continue;
                }

                baseEmission[i] = emission;
                hasEmission[i] = true;
                emissiveCount++;
            }

            lights = controlLights ? crystalsRoot.GetComponentsInChildren<Light>(true) : new Light[0];
            baseLightIntensity = new float[lights.Length];
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null)
                {
                    baseLightIntensity[i] = lights[i].intensity;
                }
            }

            if (renderers.Length == 0)
            {
                Debug.LogWarning("CrystalNightGlow: 'Crystals Root' no tiene renderers hijos.", this);
            }
            else if (emissiveCount == 0)
            {
                Debug.LogWarning("CrystalNightGlow: ningún material de los cristales tiene emisión utilizable. " +
                                 "Activa Emission en el material.", this);
            }
        }

        private void LateUpdate()
        {
            if (timeOfDay == null || block == null || renderers == null)
            {
                return;
            }

            float multiplier = Mathf.Lerp(dayMultiplier, nightMultiplier, timeOfDay.NightFactor);

            for (int i = 0; i < renderers.Length; i++)
            {
                if (!hasEmission[i])
                {
                    continue;
                }

                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                renderer.GetPropertyBlock(block);
                block.SetColor(EmissionColorId, baseEmission[i] * multiplier);
                renderer.SetPropertyBlock(block);
            }

            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] == null)
                {
                    continue;
                }

                lights[i].intensity = baseLightIntensity[i] * multiplier;
            }
        }
    }
}
