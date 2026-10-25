using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Echo.World
{
    /// <summary>
    /// Una fase del día. Se interpolan los valores entre fases consecutivas.
    /// </summary>
    [Serializable]
    public class DayPhase
    {
        public string name = "Fase";
        [Range(0f, 1f)] public float time;
        public Color lightColor = Color.white;
        [Min(0f)] public float lightIntensity = 1f;
        public Color skyTint = new Color(0.5f, 0.5f, 0.5f);
        [Range(0f, 8f)] public float skyExposure = 1.3f;
        [Range(0f, 5f)] public float atmosphereThickness = 1f;
        public Color ambientSky = Color.gray;
        public Color ambientEquator = Color.gray;
        public Color ambientGround = Color.gray;
    }

    /// <summary>
    /// Ciclo continuo día / atardecer / noche / amanecer.
    /// Tiempo normalizado: 0 = medianoche, 0.25 = amanecer, 0.5 = mediodía, 0.75 = atardecer.
    /// Controla: rotación e intensidad del sol, color de la luz, cielo (Skybox/Procedural),
    /// luz ambiental (Trilight) y luces decorativas nocturnas.
    /// </summary>
    public class TimeOfDayController : MonoBehaviour
    {
        [Header("Referencias")]
        [Tooltip("Luz direccional que representa al sol (la luz 'Sun').")]
        [SerializeField] private Light sunLight;
        [Tooltip("Material de cielo (shader Skybox/Procedural). Se usa una copia en tiempo de ejecución.")]
        [SerializeField] private Material skyboxMaterial;
        [Tooltip("Luces decorativas que se encienden de noche (opcional).")]
        [SerializeField] private Light[] nightLights;

        [Header("Ciclo")]
        [Tooltip("Duración de un ciclo completo en segundos (recomendado 120-180).")]
        [Min(10f)][SerializeField] private float cycleDurationSeconds = 150f;
        [Tooltip("Multiplicador de velocidad. 1 = normal, 0 = congelado.")]
        [Min(0f)][SerializeField] private float speedMultiplier = 1f;
        [Tooltip("Hora inicial y actual. 0 = medianoche, 0.25 = amanecer, 0.5 = mediodía, 0.75 = atardecer.")]
        [Range(0f, 1f)][SerializeField] private float timeOfDay = 0.30f;
        [SerializeField] private bool paused;
        [Tooltip("Dirección horizontal por la que se mueve el sol (grados en Y).")]
        [Range(0f, 360f)][SerializeField] private float sunAzimuth = 170f;

        [Header("Fases (editables)")]
        [SerializeField] private List<DayPhase> phases = CreateDefaultPhases();

        [Header("Pruebas rápidas")]
        [Tooltip("Teclas: F (mantener) acelera, P pausa, 1 amanecer, 2 mediodía, 3 atardecer, 4 medianoche.")]
        [SerializeField] private bool enableDebugKeys = true;
        [SerializeField] private float debugFastMultiplier = 20f;

        private static readonly int SkyTintId = Shader.PropertyToID("_SkyTint");
        private static readonly int TintId = Shader.PropertyToID("_Tint");
        private static readonly int ExposureId = Shader.PropertyToID("_Exposure");
        private static readonly int AtmosphereId = Shader.PropertyToID("_AtmosphereThickness");

        private List<DayPhase> sortedPhases;
        private Material runtimeSkybox;
        private bool hasSkyTint;
        private bool hasTint;
        private bool hasExposure;
        private bool hasAtmosphere;
        private LightShadows baseShadows;
        private float[] nightLightBaseIntensity;
        private GUIStyle labelStyle;
        private string currentPhaseName = "";
        private float currentMultiplier = 1f;

        /// <summary>Hora actual normalizada (0-1).</summary>
        public float TimeOfDay01 => timeOfDay;

        /// <summary>0 = pleno día, 1 = noche cerrada. Útil para otros sistemas (cristales, etc.).</summary>
        public float NightFactor { get; private set; }

        private void Awake()
        {
            if (sunLight == null)
            {
                Debug.LogError("TimeOfDayController: falta asignar 'Sun Light' en el Inspector.", this);
                enabled = false;
                return;
            }

            if (sunLight.type != LightType.Directional)
            {
                Debug.LogWarning("TimeOfDayController: 'Sun Light' debería ser una luz Directional.", this);
            }

            if (phases == null || phases.Count < 2)
            {
                Debug.LogWarning("TimeOfDayController: se necesitan al menos 2 fases; se restauran las de por defecto.", this);
                phases = CreateDefaultPhases();
            }

            sortedPhases = new List<DayPhase>(phases);
            sortedPhases.Sort((a, b) => a.time.CompareTo(b.time));

            baseShadows = sunLight.shadows;
            RenderSettings.sun = sunLight;
            RenderSettings.ambientMode = AmbientMode.Trilight;

            SetupSkybox();
            CacheNightLights();
        }

        private void Update()
        {
            currentMultiplier = speedMultiplier;

            if (enableDebugKeys) HandleDebugKeys();

            if (!paused)
            {
                timeOfDay = Mathf.Repeat(
                    timeOfDay + Time.deltaTime * currentMultiplier / cycleDurationSeconds, 1f);
            }

            ApplyTime();
        }

        private void OnDestroy()
        {
            if (runtimeSkybox != null) Destroy(runtimeSkybox);
        }

        /// <summary>Cambia la hora (0-1) por código.</summary>
        public void SetTimeOfDay(float value)
        {
            timeOfDay = Mathf.Repeat(value, 1f);
        }

        [ContextMenu("Restaurar fases por defecto")]
        private void RestoreDefaultPhases()
        {
            phases = CreateDefaultPhases();
        }

        private void SetupSkybox()
        {
            if (skyboxMaterial == null)
            {
                Debug.LogWarning("TimeOfDayController: no hay 'Skybox Material'. " +
                                 "La luz y el ambiente cambiarán, pero el cielo no.", this);
                return;
            }

            runtimeSkybox = new Material(skyboxMaterial) { name = skyboxMaterial.name + " (Runtime)" };
            RenderSettings.skybox = runtimeSkybox;

            hasSkyTint = runtimeSkybox.HasProperty(SkyTintId);
            hasTint = runtimeSkybox.HasProperty(TintId);
            hasExposure = runtimeSkybox.HasProperty(ExposureId);
            hasAtmosphere = runtimeSkybox.HasProperty(AtmosphereId);

            if (!hasSkyTint && !hasTint && !hasExposure)
            {
                Debug.LogWarning("TimeOfDayController: el shader del cielo no tiene _SkyTint, _Tint ni _Exposure. " +
                                 "Usa un material con shader Skybox/Procedural.", this);
            }
        }

        private void CacheNightLights()
        {
            if (nightLights == null) return;

            nightLightBaseIntensity = new float[nightLights.Length];
            for (int i = 0; i < nightLights.Length; i++)
            {
                if (nightLights[i] != null) nightLightBaseIntensity[i] = nightLights[i].intensity;
            }
        }

        private void ApplyTime()
        {
            EvaluatePhases(timeOfDay, out DayPhase a, out DayPhase b, out float blend);
            float k = Mathf.SmoothStep(0f, 1f, blend);
            currentPhaseName = blend < 0.5f ? a.name : b.name;

            // 1) Rotación del sol: x = 0 en el horizonte (amanecer), 90 = mediodía.
            sunLight.transform.rotation =
                Quaternion.Euler((timeOfDay - 0.25f) * 360f, sunAzimuth, 0f);

            // 2) y 3) Intensidad y color de la luz principal.
            float intensity = Mathf.Lerp(a.lightIntensity, b.lightIntensity, k);
            sunLight.intensity = intensity;
            sunLight.color = Color.Lerp(a.lightColor, b.lightColor, k);
            sunLight.shadows = intensity > 0.01f ? baseShadows : LightShadows.None;

            // 5) Iluminación ambiental (Trilight).
            RenderSettings.ambientSkyColor = Color.Lerp(a.ambientSky, b.ambientSky, k);
            RenderSettings.ambientEquatorColor = Color.Lerp(a.ambientEquator, b.ambientEquator, k);
            RenderSettings.ambientGroundColor = Color.Lerp(a.ambientGround, b.ambientGround, k);

            // 4) Cielo.
            if (runtimeSkybox != null)
            {
                Color tint = Color.Lerp(a.skyTint, b.skyTint, k);
                if (hasSkyTint) runtimeSkybox.SetColor(SkyTintId, tint);
                else if (hasTint) runtimeSkybox.SetColor(TintId, tint);

                if (hasExposure)
                    runtimeSkybox.SetFloat(ExposureId, Mathf.Lerp(a.skyExposure, b.skyExposure, k));
                if (hasAtmosphere)
                    runtimeSkybox.SetFloat(AtmosphereId, Mathf.Lerp(a.atmosphereThickness, b.atmosphereThickness, k));
            }

            // 6) Luces decorativas nocturnas.
            float sunHeight = Mathf.Sin((timeOfDay - 0.25f) * Mathf.PI * 2f);
            NightFactor = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.15f, -0.2f, sunHeight));
            UpdateNightLights();
        }

        private void UpdateNightLights()
        {
            if (nightLights == null || nightLightBaseIntensity == null) return;

            for (int i = 0; i < nightLights.Length; i++)
            {
                Light l = nightLights[i];
                if (l == null) continue;

                l.intensity = nightLightBaseIntensity[i] * NightFactor;
                l.enabled = NightFactor > 0.01f;
            }
        }

        /// <summary>Busca las dos fases que rodean a 't' (con vuelta circular) y el progreso entre ellas.</summary>
        private void EvaluatePhases(float t, out DayPhase a, out DayPhase b, out float blend)
        {
            int count = sortedPhases.Count;
            int nextIndex = count;
            for (int i = 0; i < count; i++)
            {
                if (sortedPhases[i].time > t)
                {
                    nextIndex = i;
                    break;
                }
            }

            int prevIndex = (nextIndex - 1 + count) % count;
            nextIndex %= count;

            a = sortedPhases[prevIndex];
            b = sortedPhases[nextIndex];

            float aTime = a.time;
            float bTime = b.time;
            if (bTime <= aTime) bTime += 1f;
            float tt = t < aTime ? t + 1f : t;

            float span = bTime - aTime;
            blend = span > 0.0001f ? Mathf.Clamp01((tt - aTime) / span) : 0f;
        }

        private void HandleDebugKeys()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null) return;

            if (kb.fKey.isPressed) currentMultiplier *= debugFastMultiplier;
            if (kb.pKey.wasPressedThisFrame) paused = !paused;
            if (kb.digit1Key.wasPressedThisFrame) timeOfDay = 0.25f;
            if (kb.digit2Key.wasPressedThisFrame) timeOfDay = 0.50f;
            if (kb.digit3Key.wasPressedThisFrame) timeOfDay = 0.72f;
            if (kb.digit4Key.wasPressedThisFrame) timeOfDay = 0.00f;
        }

        private void OnGUI()
        {
            if (!enableDebugKeys) return;

            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold };
                labelStyle.normal.textColor = Color.white;
            }

            int totalMinutes = (int)(timeOfDay * 1440f);
            string clock = $"{totalMinutes / 60:00}:{totalMinutes % 60:00}";
            string state = paused ? "PAUSA" : $"x{currentMultiplier:0.#}";
            GUI.Label(new Rect(10f, 10f, 520f, 28f), $"Hora {clock} | {currentPhaseName} | {state}", labelStyle);
        }

        private static List<DayPhase> CreateDefaultPhases()
        {
            return new List<DayPhase>
            {
                P("Noche",       0.00f, new Color(0.35f, 0.45f, 0.80f), 0.00f, new Color(0.25f, 0.30f, 0.55f), 0.50f, 0.8f,
                  new Color(0.08f, 0.11f, 0.24f), new Color(0.06f, 0.08f, 0.18f), new Color(0.03f, 0.04f, 0.08f)),
                P("Noche",       0.20f, new Color(0.35f, 0.45f, 0.80f), 0.00f, new Color(0.25f, 0.30f, 0.55f), 0.50f, 0.8f,
                  new Color(0.08f, 0.11f, 0.24f), new Color(0.06f, 0.08f, 0.18f), new Color(0.03f, 0.04f, 0.08f)),
                P("Amanecer",    0.26f, new Color(1.00f, 0.62f, 0.40f), 0.25f, new Color(0.95f, 0.60f, 0.50f), 1.00f, 1.3f,
                  new Color(0.45f, 0.40f, 0.50f), new Color(0.50f, 0.40f, 0.40f), new Color(0.25f, 0.20f, 0.20f)),
                P("Mañana",      0.33f, new Color(1.00f, 0.82f, 0.62f), 1.00f, new Color(0.60f, 0.55f, 0.50f), 1.20f, 1.1f,
                  new Color(0.50f, 0.58f, 0.75f), new Color(0.58f, 0.58f, 0.60f), new Color(0.30f, 0.28f, 0.26f)),
                P("Día",         0.45f, new Color(1.00f, 0.96f, 0.88f), 1.20f, new Color(0.50f, 0.50f, 0.50f), 1.30f, 1.0f,
                  new Color(0.50f, 0.65f, 0.90f), new Color(0.60f, 0.65f, 0.70f), new Color(0.30f, 0.30f, 0.28f)),
                P("Día",         0.55f, new Color(1.00f, 0.96f, 0.88f), 1.20f, new Color(0.50f, 0.50f, 0.50f), 1.30f, 1.0f,
                  new Color(0.50f, 0.65f, 0.90f), new Color(0.60f, 0.65f, 0.70f), new Color(0.30f, 0.30f, 0.28f)),
                P("Tarde",       0.67f, new Color(1.00f, 0.70f, 0.45f), 1.00f, new Color(0.85f, 0.55f, 0.40f), 1.10f, 1.3f,
                  new Color(0.50f, 0.50f, 0.60f), new Color(0.60f, 0.48f, 0.42f), new Color(0.30f, 0.25f, 0.22f)),
                P("Atardecer",   0.74f, new Color(1.00f, 0.45f, 0.25f), 0.25f, new Color(1.00f, 0.42f, 0.30f), 1.00f, 1.5f,
                  new Color(0.50f, 0.35f, 0.40f), new Color(0.60f, 0.35f, 0.30f), new Color(0.30f, 0.20f, 0.18f)),
                P("Crepúsculo",  0.79f, new Color(0.90f, 0.35f, 0.45f), 0.00f, new Color(0.60f, 0.30f, 0.50f), 0.70f, 1.2f,
                  new Color(0.20f, 0.18f, 0.35f), new Color(0.25f, 0.15f, 0.25f), new Color(0.10f, 0.08f, 0.12f)),
                P("Noche",       0.87f, new Color(0.35f, 0.45f, 0.80f), 0.00f, new Color(0.25f, 0.30f, 0.55f), 0.50f, 0.8f,
                  new Color(0.08f, 0.11f, 0.24f), new Color(0.06f, 0.08f, 0.18f), new Color(0.03f, 0.04f, 0.08f)),
            };
        }

        private static DayPhase P(string name, float time, Color light, float intensity, Color skyTint,
                                  float exposure, float atmosphere, Color ambSky, Color ambEq, Color ambGround)
        {
            return new DayPhase
            {
                name = name,
                time = time,
                lightColor = light,
                lightIntensity = intensity,
                skyTint = skyTint,
                skyExposure = exposure,
                atmosphereThickness = atmosphere,
                ambientSky = ambSky,
                ambientEquator = ambEq,
                ambientGround = ambGround
            };
        }
    }
}