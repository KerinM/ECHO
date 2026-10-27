# ECHO: El Santuario del Tiempo (Unity 6000.6.2f1, URP)

Proyecto universitario de Computación Gráfica. Responde en español.
Los identificadores de código van en inglés.

## Reglas inviolables
- Una sola escena: Assets/_Project/Scenes/SantuarioPrincipal.unity. No crear otras.
- Un solo personaje (objeto Player con PlayerController). No crear otro controlador.
- Input System nuevo (UnityEngine.InputSystem). No usar UnityEngine.Input clásico. Sin Cinemachine.
- URP: shader "Universal Render Pipeline/Lit". Nunca Standard.
- Sin multijugador, IA, inventario, guardado ni paquetes nuevos sin pedir permiso.
- NO editar a mano archivos .unity ni .prefab (YAML). NO tocar Library/, Temp/, Logs/,
  UserSettings/ ni ProjectSettings/ sin permiso. NO borrar ni editar archivos .meta.
  Para mover o borrar assets, usar AssetDatabase desde un script de Editor.
- Namespaces Echo.*; el nombre del archivo debe coincidir con el de la clase.
- Referencias por [SerializeField] con comprobaciones y Debug.LogError claros.
  Sin FindObjectOfType en Update. Eventos para comunicar sistemas.

## Estructura (carpetas directamente bajo Assets/)
Scenes, Scripts (Player, Camera, Environment, Portals, Interactions, Mission, UI, Editor),
Materials, Prefabs, Models (Kenney_NatureKit, Kenney_CastleKit), Textures, Audio, VFX.
Los scripts de Editor van en Assets/Scripts/Editor.

## Scripts existentes (no cambiar su API sin actualizar todos los usos)
- Echo.Player.PlayerController: SetControlEnabled(bool), Teleport(Vector3, Quaternion)
- Echo.CameraSystem.ThirdPersonCamera: SnapTo(float yaw), SetInputEnabled(bool)
- Echo.World.TimeOfDayController: NightFactor, SetTimeOfDay(float)
- Echo.Interactions: Interactable (abstracta), InteractionController (capa "Interactable")
- Echo.Portals: PortalController, PortalTraveler
- Echo.Mission: MissionManager (eventos), EnergyFragment, AncestralCore, VictoryController
- Echo.UI: ScreenFader, InteractionPromptUI, GameUIController, IntroPanelUI
- Echo.Player.PlayerAnimator: lee el CharacterController y alimenta el Animator del modelo
  (parámetros Speed float, Grounded bool, Interact trigger, Victory trigger). Métodos PlayInteract(), PlayVictory().

## Mundo
- Región A centro (0,0,0); Región B centro (200,0,0). Cada suelo mide 80x80 m.
- Los hijos usan posición LOCAL: local = mundo - posición del padre.
- Posiciones de mundo: PlayerSpawn (0,0.1,-25); Portal_A (30,0,10); llegada A (30,0.1,4);
  Fragment_1 (-25,1,20); AncestralCore (0,0,30); Portal_B (170,0,-10); llegada B (170,0.1,-16);
  Fragment_2 (225,1,20); Fragment_3 (190,1,35).
- Mantén libre un radio de 3 m alrededor de esos puntos.
- Capa "Interactable" para zonas de interacción. Tag "Player" en el jugador.

## Cómo trabajar
- Haz cambios pequeños, un tema a la vez, y lista los archivos que tocaste.
- Para construir escenario: scripts en Assets/_Project/Scripts/Editor (namespace Echo.EditorTools),
  con [MenuItem("Tools/ECHO/...")]. Deben ser idempotentes (recrear el contenedor por nombre,
  sin duplicar), usar Undo y marcar la escena como modificada. No guardar la escena sin pedirlo.
- Los modelos de Kenney se buscan con AssetDatabase.FindAssets. Nunca supongas nombres:
  consulta docs/ModelCatalog.md.
- No puedes ejecutar Unity ni ver la pantalla. Nunca afirmes que algo funciona.
  Termina cada tarea con "Qué verificar en Unity" (pasos concretos) y "Errores probables".

## Estado (actualizar al terminar cada tarea)
Hecho: Fases 1-6, pasos 1-4 de la Fase 7, scripts de los pasos 5-7
(CrystalNightGlow, Setup Crystal Glow, Setup Region B Volume y Optimize Static),
plan de pruebas, documentación académica en borrador, script Tools/ECHO/Build Windows y
ajustes de escala/densidad en BuildRegionA/B con ForestScatter, niebla interpolada
por fase en TimeOfDayController, script SetupVfx para recogida y portales y
SetupPlayerCharacter para modelo, animaciones, controller, prefab y jerarquía
(compilados estáticamente, sin ejecutar en Unity).
Pendiente: ejecutar Tools/ECHO/Setup Player Character y verificar importación Humanoid,
materiales URP, animaciones, AC_Player, PlayerModel y jerarquía del Player;
ejecutar Tools/ECHO/Setup Fragment Pickup VFX y Tools/ECHO/Setup Portal VFX;
ejecutar Tools/ECHO/Build Region A, Tools/ECHO/Build Region B y Tools/ECHO/Optimize Static;
restaurar las fases del TimeOfDayController si los campos de niebla no se actualizan solos;
compilar con Tools/ECHO/Build Windows y probar el .exe; completar la columna "Resultado real"
de docs/PlanDePruebas.md; medir Stats (paso 7); cotejar criterios y documentación con el
enunciado original y cerrar la Fase 8.