using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Echo.UI;

namespace Echo.EditorTools
{
    // Bloque 7.3: créditos en el panel de título. Crea (una sola vez) un texto TMP pequeño
    // como hijo del panel de IntroPanelUI y lo asigna al campo 'creditsText'. Idempotente.
    public static class SetupCredits
    {
        private const string MenuName = "Setup Credits";
        private const string CreditsChildName = "CreditsText";
        private const string CreditsTextValue =
            "Modelos Kenney (CC0) · Personaje y animaciones Mixamo · Texturas ambientCG (CC0) · Audio Kenney (CC0)";
        private const float FontSize = 12f;

        [MenuItem("Tools/ECHO/Setup Credits", false, 31)]
        public static void Execute()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning($"[ECHO] {MenuName}: solo disponible en modo edición.");
                return;
            }

            IntroPanelUI introPanel = UnityEngine.Object.FindAnyObjectByType<IntroPanelUI>(FindObjectsInactive.Include);
            if (introPanel == null)
            {
                Debug.LogError($"[ECHO] {MenuName}: no se encontró IntroPanelUI en la escena.");
                return;
            }

            SerializedObject introSerialized = new SerializedObject(introPanel);
            SerializedProperty panelProperty = introSerialized.FindProperty("panel");
            CanvasGroup panel = panelProperty != null ? panelProperty.objectReferenceValue as CanvasGroup : null;
            if (panel == null)
            {
                Debug.LogError($"[ECHO] {MenuName}: IntroPanelUI no tiene 'Panel' asignado.");
                return;
            }

            TextMeshProUGUI credits = null;
            for (int i = 0; i < panel.transform.childCount; i++)
            {
                Transform child = panel.transform.GetChild(i);
                if (child.name == CreditsChildName)
                {
                    credits = child.GetComponent<TextMeshProUGUI>();
                    break;
                }
            }

            if (credits == null)
            {
                GameObject created = new GameObject(CreditsChildName);
                Undo.RegisterCreatedObjectUndo(created, MenuName);
                created.transform.SetParent(panel.transform, false);
                credits = created.AddComponent<TextMeshProUGUI>();
            }

            Undo.RecordObject(credits, MenuName);

            RectTransform rectTransform = credits.rectTransform;
            rectTransform.anchorMin = new Vector2(0f, 0f);
            rectTransform.anchorMax = new Vector2(1f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.sizeDelta = new Vector2(-40f, 30f);
            rectTransform.anchoredPosition = new Vector2(0f, 14f);

            credits.fontSize = FontSize;
            credits.text = CreditsTextValue;
            credits.color = Color.white;
            credits.alignment = TextAlignmentOptions.Center;
            credits.raycastTarget = false;

            EditorUtility.SetDirty(credits);

            SerializedProperty creditsProperty = introSerialized.FindProperty("creditsText");
            if (creditsProperty == null)
            {
                Debug.LogError($"[ECHO] {MenuName}: IntroPanelUI no tiene la propiedad serializada " +
                               "'creditsText'. ¿Se compiló la versión nueva del script?");
                return;
            }

            creditsProperty.objectReferenceValue = credits;
            introSerialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log($"[ECHO] {MenuName}: '{CreditsChildName}' listo bajo el panel de título " +
                      "con los créditos CC0.", credits.gameObject);
        }
    }
}
