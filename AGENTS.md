# ECHO: El Santuario del Tiempo (Unity 6000.6.2f1, URP)

Videojuego 3D de exploración en tercera persona. Proyecto universitario de Computación Gráfica.
Responde en español. Los identificadores de código van en inglés.
Objetivo actual: FASE DE PULIDO para la entrega final (al final de este archivo).

## Reglas inviolables
- Una sola escena: Assets/Scenes/SantuarioPrincipal.unity. No crear otras.
- Un solo personaje (objeto Player con PlayerController). No crear otro controlador.
- Input System nuevo (UnityEngine.InputSystem). No usar UnityEngine.Input clásico. Sin Cinemachine.
- URP: shader "Universal Render Pipeline/Lit". Nunca Standard.
- Sin multijugador, IA, inventario, guardado ni paquetes nuevos sin pedir permiso.
- NO añadir mecánicas nuevas (salto, enemigos, etc.): solo pulido visual, audio y animación.
- NO editar a mano archivos .unity ni .prefab (YAML). NO tocar Library/, Temp/, Logs/,
  UserSettings/ ni ProjectSettings/ sin permiso. NO borrar ni editar archivos .meta.
  Para mover o borrar assets, usar AssetDatabase desde un script de Editor.
- NO ejecutes git commit, push, reset ni checkout, ni borres archivos con comandos de shell:
  lo hace el usuario.
- Muestra siempre el plan antes de escribir código (modo plan) y espera aprobación.
- Namespaces Echo.*; el nombre del archivo debe coincidir con el de la clase.
- Referencias por [SerializeField] con comprobaciones y Debug.LogError claros.
  Sin FindObjectOfType en Update. Eventos para comunicar sistemas.
- Solo añadir a las APIs existentes (eventos, métodos); no renombrar ni cambiar comportamiento.
- Respuestas concisas: no releas el proyecto entero ni reescribas archivos completos para cambios
  pequeños (los créditos de uso son limitados).

## Estructura real (carpetas directamente bajo Assets/; NO existe Assets/_Project)
Scenes, Scripts (Player, Camera, Environment, Portals, Interactions, Mission, UI, Editor),
Materials, Prefabs, Models (Kenney_NatureKit, Kenney_CastleKit, Player), Animations,
Textures, Audio, VFX. Documentos en docs/ (raíz del repositorio).
Los scripts de Editor van en Assets/Scripts/Editor (namespace Echo.EditorTools).

## Scripts existentes (no cambiar su API sin actualizar todos los usos)
- Echo.Player.PlayerController: SetControlEnabled(bool), Teleport(Vector3, Quaternion)
- Echo.Player.PlayerAnimator: lee el CharacterController y alimenta el Animator del modelo
  (parámetros Speed float, Grounded bool, Interact trigger, Victory trigger). PlayInteract(), PlayVictory().
- Echo.CameraSystem.ThirdPersonCamera: SnapTo(float yaw), SetInputEnabled(bool)
- Echo.World.TimeOfDayController: NightFactor, SetTimeOfDay(float); DayPhase con niebla.
  Teclas de prueba: 1-4 (hora), F (acelerar), P (pausa).
- Echo.World.CrystalNightGlow: brillo de cristales según NightFactor
- Echo.Interactions: Interactable (abstracta), InteractionController (capa "Interactable")
- Echo.Portals: PortalController, PortalTraveler
- Echo.Mission: MissionManager (eventos FragmentCollected, StageChanged, MissionCompleted),
  EnergyFragment, AncestralCore, VictoryController
- Echo.UI: ScreenFader, InteractionPromptUI, GameUIController, IntroPanelUI
- ForestScatter: ayudante de dispersión usado por BuildRegionA/B (revisar su ubicación y namespace).

## Herramientas de Editor existentes (menú Tools/ECHO/...)
Build Region A, Build Region B, Setup Crystal Glow, Setup Region B Volume, Optimize Static,
Build Windows, Setup Fragment Pickup VFX, Setup Portal VFX, Setup Player Character.
- Antes de crear una herramienta nueva, comprueba que no exista ya una equivalente.
- Build Region A y Build Region B VACÍAN y regeneran sus contenedores (Decor_*, Path_*, Lanterns_*,
  Cliffs_*, etc.). No volver a ejecutarlos sin acordarlo con el usuario: borrarían ajustes manuales
  y podrían deshacer el pulido posterior.
- Toda herramienta nueva: idempotente (recrear su contenedor por nombre, sin duplicar), con Undo,
  marcando la escena como modificada, SIN guardarla.

## Mundo
- Región A centro (0,0,0); Región B centro (200,0,0). Cada suelo mide 80x80 m.
- Los hijos usan posición LOCAL: local = mundo - posición del padre.
- Posiciones de mundo: PlayerSpawn (0,0.1,-25); Portal_A (30,0,10); llegada A (30,0.1,4);
  Fragment_1 (-25,1,20); AncestralCore (0,0,30); Portal_B (170,0,-10); llegada B (170,0.1,-16);
  Fragment_2 (225,1,20); Fragment_3 (190,2.5,32) sobre la plataforma de las escaleras
  (local de la región B: (-10,2.5,32)). Verificar en la escena.
- Mantén libre un radio de 3 m alrededor de esos puntos.
- Capa "Interactable" para zonas de interacción. Tag "Player" en el jugador.
- Muros de acantilado (Cliffs_A/B) de 84x14x4 en los bordes de cada región; actúan como colliders.
- Región A: verde cálida y luminosa (santuario del amanecer). Región B: violeta y nocturna
  (ruinas del crepúsculo), con un Volume local de post-procesado.

## Personaje
Mixamo, Humanoid. PlayerModel (prefab) hijo de Player, con Animator y AC_Player (Assets/Animations).
Hay clips Idle, Walk y Run. Aún NO existen los clips Interact ni Victory (los descarga una persona).
PlayerAnimator solo lee el estado: no mueve al personaje. Apply Root Motion desactivado.

## Criterios de aceptación del enunciado (no deben romperse)
- Un único personaje controlable al ejecutar. Camina y corre (W A S D, Shift); cámara con ratón;
  E interactúa; Esc libera el cursor.
- Cielo e iluminación cambian gradualmente con el tiempo (ciclo de 120 a 180 s, configurable,
  con forma de probarlo rápido).
- Dos regiones en la misma escena, conectadas por dos portales (ida y vuelta) sin bucles de activación.
- Tres fragmentos recogibles una sola vez; contador 0/3 a 3/3.
- El núcleo solo se activa con los tres fragmentos; misión termina con el panel "Santuario restaurado".
- Interfaz: título, instrucciones, contador, mensajes contextuales, estado de misión, victoria, reinicio.
- Sin errores de compilación ni referencias sin asignar. Funciona en una compilación para Windows.
- No se carga otra escena para completar la misión.
- Rendimiento estable en un computador de gama media.

## Cómo trabajar
- Haz cambios pequeños, un tema a la vez, y lista los archivos que tocaste.
- Los modelos de Kenney se buscan con AssetDatabase.FindAssets. Nunca supongas nombres:
  consulta docs/ModelCatalog.md.
- No puedes ejecutar Unity ni ver la pantalla. Nunca afirmes que algo funciona.
  Termina cada tarea con "Qué verificar en Unity" (pasos concretos) y "Errores probables".
- Efectos de partículas moderados y pocas luces dinámicas. Los objetos decorativos fijos van con
  Batching Static; no marcar como estático lo que se mueve o cambia de material.

## Colaboración (dos personas)
- Solo una persona modifica la escena a la vez (SantuarioPrincipal.unity no se fusiona bien en Git).
- Rama de trabajo "pulido"; la etiqueta v1-jugable marca la última versión estable jugable.
- git pull antes de abrir Unity y un commit por bloque verificado (lo hace el usuario).

## Lecciones aprendidas (errores que ya ocurrieron)
- Las posiciones locales confundieron varias veces: objetos que quedaron flotando en el vacío.
- Hubo dos VictoryController en el mismo objeto; revisar duplicados de componentes.
- Los prefabs no guardan referencias a objetos de la escena: se asignan en cada instancia.
- Un objeto PlayerController suelto en la raíz creó un segundo "jugador": nunca crear scripts
  sueltos en la Hierarchy.
- El Canvas del fundido (FadeCanvas) se ve enorme en la ventana Scene: es normal.
- El nombre del script y de la clase deben coincidir (hubo un EnergyFragment..cs).
- Los avisos "Identifier uniqueness violation" de los FBX de Kenney son inofensivos.
- Si el proyecto muestra "Default Render Pipeline: None" en Graphics, asignar PC_RPAsset ahí.

## Estado (actualizar al terminar cada bloque)
Verificado en Unity por el usuario:
- Mecánicas completas: movimiento, cámara, ciclo de día y noche, portales en ambos sentidos,
  fragmentos, núcleo, victoria, reinicio e interfaz.
- Regiones A y B construidas con Build Region A/B (escala y densidad ajustadas) y Volume de B.
- Personaje Mixamo con Idle, Walk y Run funcionando.
Escrito pero sin verificar en Unity (verificar antes de depender de ello):
- CrystalNightGlow / Setup Crystal Glow, Optimize Static, niebla por fase, Setup Fragment Pickup VFX,
  Setup Portal VFX, Build Windows, ForestScatter. Si TimeOfDayController no refleja los campos
  de niebla, usar "Restaurar fases por defecto" en su menú contextual.
Pendiente: los bloques de la fase de pulido y la Fase 8.

# FASE DE PULIDO PARA LA ENTREGA FINAL

Cómo se usa: el usuario dirá "Haz la auditoría" o "Haz el Bloque N". Lee solo lo necesario, propón
un plan corto, espera aprobación, implementa, y termina con "Qué verificar en Unity".
Los bloques marcados (HUMANO) los hace una persona: no los hagas tú.

## Auditoría (solo lectura, antes del Bloque 1)
No modifiques nada. Lista scripts por carpeta (namespace y clase), herramientas de Editor con su
menú y prefabs. Compara con "Scripts existentes", "Herramientas" y "Estado" y señala lo que no
coincida (faltantes, nombres distintos, tareas dadas por hechas que no lo parecen). Resumen corto.

## Bloque 0 (HUMANO) — Seguridad
Borrar Test_Wall, Test_Step y Test_Tall si siguen en la escena. Compilar con Tools/ECHO/Build Windows,
abrir el .exe y completar la misión. Commit.

## Bloque 1 — Acantilados y escenario lejano
Cliffs_A y Cliffs_B son cubos grises. Herramienta Tools/ECHO/Build Cliff Dressing que, SIN eliminar
los cubos, desactive su MeshRenderer (quedan como colliders) y añada, en contenedores Dressing_A y
Dressing_B, una fila continua de modelos cliff_block* (ver docs/ModelCatalog.md) a lo largo de los
cuatro lados de cada región, muy escalados (aprox. 6 a 10x), con rotación y escala aleatorias
deterministas (semilla fija). Segunda fila apilada hasta unos 16-18 m. En la región B usar
M_StoneDark o un tinte violáceo. Añadir 3 o 4 formaciones tipo montaña detrás de los muros, sin
colliders. Batching Static. No tocar gameplay, portales ni fragmentos. Imprimir conteos.
Verificar: los muros no parecen cajas, no se puede salir de las regiones, FPS estables.

## Bloque 2 (HUMANO) — Ajustes globales de render
En Global Volume: Tonemapping (ACES o Neutral), Bloom (Threshold 1, Intensity 0.6), Color Adjustments
suave. SSAO como Renderer Feature en el renderer URP; medir FPS con y sin él.

## Bloque 3 — Atmósfera viva
1. Verificar que la niebla de TimeOfDayController (ExponentialSquared, densidad máxima 0.02) cambie
   con las fases; corregir solo si falla.
2. StarField (Echo.World) y menú para añadirlo: Particle System de unos 500 puntos en un cascarón
   esférico grande que siga a la cámara; opacidad según NightFactor (invisible de día); que la
   niebla no lo oculte.
3. Partículas ambientales moderadas: luciérnagas en A solo de noche (máx. 80 partículas) y esporas
   violetas flotantes en B (máx. 120), con material URP Particles/Unlit.
Verificar: de día no hay estrellas ni luciérnagas; de noche sí, sin coste notable de FPS.

## Bloque 4 (HUMANO) — Textura del suelo de B
Material PBR de ambientCG (1K-JPG, CC0) en M_Ground_B con Tiling aprox. (20,20) y tinte violeta.

## Bloque 5 — Personaje, etapa 2 (requiere que un HUMANO descargue de Mixamo Interact.fbx y Victory.fbx
##            en Assets/Models/Player/Animations: FBX for Unity, Without Skin, In Place)
1. Importar ambos como Humanoid copiando el Avatar de Character.fbx; Bake Into Pose para el root.
2. En AC_Player: capa "UpperBody" (peso 1, Override) con un AvatarMask humanoide solo de torso, brazos
   y cabeza (sin piernas ni root); estado vacío por defecto y estado Interact (trigger Interact) que
   vuelve al vacío. En la capa base: Any State a Victory con el trigger Victory, sin salida de vuelta.
3. Evento aditivo Interacted (Action<Interactable>) en InteractionController, sin cambiar su comportamiento.
4. Assets/Scripts/Mission/PlayerReactions.cs (namespace Echo.Mission) que escuche
   InteractionController.Interacted y MissionManager.MissionCompleted y llame a PlayInteract() y
   PlayVictory(). Sin referencias circulares entre namespaces ni búsquedas en Update.
Verificar: al recoger o usar un portal hace un gesto sin frenar al caminar; al ganar queda en
pose de victoria mientras aparece el panel.

## Bloque 6 — Audio (un HUMANO descarga y elige 6 a 10 sonidos en Assets/Audio/Music, Ambience, SFX;
##            fuentes: Kenney RPG Audio, Interface Sounds e Impact Sounds, todas CC0)
Sin singletons estáticos.
1. PlayerFootsteps (en Player): pasos con AudioSource según la velocidad real del CharacterController,
   intervalos distintos al caminar y al correr, variación de clip y pitch.
2. SfxOnEvents: sonidos al recoger fragmento, activar núcleo, viajar por portal y ganar. Eventos mínimos
   y aditivos en PortalTraveler (TravelStarted, TravelCompleted).
3. AmbientAudio: música y ambiente en loop a volumen suave; el ambiente puede variar con NightFactor.
4. Sonido de clic en el botón de reinicio.
Herramienta de Editor idempotente. Verificar: pasos al caminar y correr, sin saturar, la música no se
corta al reiniciar.

## Bloque 7 — Baliza, reloj y créditos
1. ObjectiveBeacon: haz de luz vertical emisivo, sin colliders, que se mueve al objetivo según
   MissionManager: etapa 1 sobre Fragment_1; etapa 2 sobre Portal_A si el jugador está en la región A
   (X menor que 100) o sobre el fragmento pendiente más cercano si está en B; etapa 3 sobre Portal_B si
   está en B o sobre el núcleo si está en A. Se oculta al ganar. Sin FindObjectOfType en Update.
2. El indicador de hora (OnGUI) de TimeOfDayController se muestra u oculta con F1 y empieza oculto.
   Las teclas 1-4, F y P siguen activas.
3. Texto de créditos pequeño en el panel de título: Modelos Kenney (CC0), personaje y animaciones
   Mixamo, texturas ambientCG (CC0), audio Kenney (CC0).
Verificar: la baliza siempre indica el siguiente paso, F1 alterna el reloj, los créditos son legibles.

## Bloque 8 — Fase 8 (cierre)
Completar la columna "Resultado real" de docs/PlanDePruebas.md solo con resultados que el usuario
confirme. Compilar con Build Windows y probar el .exe. Medir Stats (FPS, Batches, Triangles) y anotar
solo valores medidos. Cotejar con los criterios de aceptación de este archivo y cerrar la documentación
académica sin afirmar pruebas ni mediciones que no se hayan hecho. Mantener docs/Creditos.md al día.