# Plan de pruebas — ECHO: El Santuario del Tiempo

**Proyecto:** ECHO: El Santuario del Tiempo<br>
**Motor:** Unity 6000.6.2f1, URP 17.6.0<br>
**Escena:** `Assets/Scenes/SantuarioPrincipal.unity`<br>
**Plataforma objetivo:** Windows x64<br>
**Estado del documento:** plantilla de ejecución

## 1. Propósito y alcance

Este documento define los casos de prueba para verificar el prototipo jugable de ECHO. Cubre arranque, movimiento, cámara, interacción, misión, portales, núcleo, victoria, reinicio, ciclo día/noche, ambiente, escenario, optimización y compilación para Windows.

Las pruebas se ejecutan en el Editor de Unity y, cuando corresponda, sobre el ejecutable generado en `Builds/`. La columna **Resultado real** queda vacía para que la complete quien ejecute cada caso.

## 2. Nota de veracidad

> No afirmes pruebas ni mediciones que no consten en el repositorio.

Este documento no afirma que las pruebas se hayan ejecutado ni que el juego funcione. Tampoco incluye cifras de FPS, batches o triángulos: esas mediciones deben registrarse con evidencia después de ejecutarlas.

## 3. Entorno de prueba

| Elemento | Valor |
|---|---|
| Editor | Unity 6000.6.2f1 |
| Render pipeline | Universal Render Pipeline 17.6.0 |
| Sistema de entrada | Input System 1.20.0 |
| Escena | `Assets/Scenes/SantuarioPrincipal.unity` |
| Resolución de referencia | 1024 × 768 (valor del proyecto) |
| Plataforma de build | Windows x64 |
| Periféricos | Teclado y ratón |

## 4. Criterios de aceptación

El repositorio no contiene una copia del enunciado original ni una lista oficial de criterios. Los siguientes criterios se derivan de `AGENTS.md`, de `docs/Fase7_Escenario.md`, de la escena y de los scripts. Deben cotejarse con el enunciado cuando esté disponible.

| ID | Criterio de aceptación derivado | Fuente principal |
|---|---|---|
| CA-01 | El proyecto arranca en una única escena, sin escenas adicionales ni errores rojos en la consola. | `AGENTS.md`, `EditorBuildSettings.asset` |
| CA-02 | El jugador se mueve en tercera persona con WASD, corre con Shift, la cámara orbita con el ratón y respeta límites y colisiones. | `PlayerController`, `ThirdPersonCamera` |
| CA-03 | Las zonas interactuables muestran un prompt contextual y ejecutan una acción con E. | `Interactable`, `InteractionController` |
| CA-04 | Los tres fragmentos se recogen, actualizan el HUD y avanzan las etapas de la misión. | `MissionManager`, `EnergyFragment` |
| CA-05 | El portal A está bloqueado hasta recoger un fragmento; el viaje usa fundido, destino seguro, reorientación de cámara y enfriamiento. | `PortalController`, `PortalTraveler` |
| CA-06 | La región B es accesible, contiene los fragmentos 2 y 3 y permite regresar por el portal B. | Escena, `MissionManager` |
| CA-07 | El núcleo solo se activa con los tres fragmentos; al activarlo se completa la misión y aparece la victoria. | `AncestralCore`, `VictoryController` |
| CA-08 | El botón de reinicio recarga la misma escena y restablece el estado inicial. | `VictoryController` |
| CA-09 | La interfaz muestra introducción, contador, objetivo, prompt, avisos y fundidos sin bloquear indebidamente al jugador. | `GameUIController`, `InteractionPromptUI`, `IntroPanelUI`, `ScreenFader` |
| CA-10 | El ciclo día/noche cambia sol, cielo, ambiente y luces nocturnas, con teclas de prueba. | `TimeOfDayController` |
| CA-11 | Los cristales de la región B aumentan su brillo de noche y la región B aplica tinte, viñeta y bloom. | `CrystalNightGlow`, `VP_RegionB.asset`, escena |
| CA-12 | El escenario permite recorrer caminos y escaleras, bloquea la salida de las regiones y distingue visualmente A y B. | `docs/Fase7_Escenario.md`, escena |
| CA-13 | Los contenedores estáticos usan `Batching Static`; las luces no se marcan como estáticas; se pueden registrar métricas con Stats. | `OptimizeStatic`, `PC_RPAsset.asset` |
| CA-14 | El menú `Tools/ECHO/Build Windows` genera un ejecutable Windows x64 en `Builds/` y el mismo método se puede invocar por `-executeMethod`. | `BuildWindows.cs` |
| CA-15 | La partida completa se ejecuta sin errores rojos, sin referencias faltantes y usando el Input System nuevo. | `AGENTS.md`, scripts |
| CA-16 | El audio acompaña la acción: pasos al caminar y correr, sonidos por eventos (fragmento, núcleo, portal, victoria), clic de reinicio, música y ambiente en loop; sin singletons estáticos ni saturación. | `AGENTS.md` Bloque 6, `PlayerFootsteps`, `SfxOnEvents`, `AmbientAudio` |
| CA-17 | La baliza indica el siguiente paso de la misión según etapa y región del jugador (X menor que 100 = región A) y se oculta al ganar. | `AGENTS.md` Bloque 7, `ObjectiveBeacon`, `MissionManager` |
| CA-18 | El personaje ejecuta el gesto Interact sin frenar el caminado (capa UpperBody) y mantiene la pose Victory al terminar la misión. | `AGENTS.md` Bloque 5, `PlayerReactions`, `InteractionController` |
| CA-19 | Atmósfera viva: estrellas y luciérnagas solo de noche, esporas violetas en B día y noche, niebla por fases. | `AGENTS.md` Bloque 3, `StarField`, `AmbientParticles`, `TimeOfDayController` |
| CA-20 | Detalle final: muros vestidos que no parecen cajas, créditos legibles en el panel de título y reloj F1 que empieza oculto. | `AGENTS.md` Bloques 1 y 7, `BuildCliffDressing`, `SetupCredits` |

## 5. Datos de referencia

| Objeto | Posición de mundo | Nota |
|---|---|---|
| `PlayerSpawn` | `(0, 0.1, -25)` | Aparición del jugador. |
| `Portal_A` | `(30, 0, 10)` | Requiere 1 fragmento. |
| Llegada desde A | `(170, 0.1, -16)` | `Marker_PortalB_Arrival`. |
| `Portal_B` | `(170, 0, -10)` | Sin requisito adicional. |
| Llegada desde B | `(30, 0.1, 4)` | `Marker_PortalA_Arrival`. |
| `Fragment_1` | `(-25, 1, 20)` | Región A. |
| `Fragment_2` | `(225, 1, 20)` | Región B. |
| `Fragment_3` | `(190, 2.5, 32)` | Instancia real de la escena; el marcador antiguo está en `(190, 3, 35)` y `AGENTS.md` indica `(190, 1, 35)`. |
| `AncestralCore` | `(0, 0, 30)` | Región A. |
| Región A | Centro `(0, 0, 0)` | Suelo de 80 × 80 m. |
| Región B | Centro `(200, 0, 0)` | Suelo de 80 × 80 m. |

**Controles**

| Acción | Entrada |
|---|---|
| Mover | `W`, `A`, `S`, `D` |
| Correr | Mantener `Shift` |
| Orbitar cámara | Ratón |
| Liberar/recuperar cursor | `Esc` |
| Interactuar | `E` |
| Mostrar/ocultar introducción | `H` |
| Amanecer / mediodía / atardecer / medianoche | `1` / `2` / `3` / `4` |
| Acelerar tiempo | Mantener `F` |
| Pausar ciclo | `P` |
| Mostrar/ocultar indicador de hora | `F1` |

## 6. Cómo registrar resultados

En la columna **Resultado real** anotar uno de estos estados y una evidencia breve:

- `OK`: el resultado coincide con lo esperado.
- `FALLA`: el resultado difiere; describir qué ocurrió.
- `BLOQUEADO`: no se pudo ejecutar por un impedimento previo.
- `N/A`: el caso no aplica al entorno evaluado.

Añadir entre paréntesis la evidencia: captura, mensaje de consola, posición observada o enlace al log. No completar esta columna sin haber ejecutado el caso.

## 7. Casos de prueba

| ID | Criterio | Pasos | Resultado esperado | Resultado real |
|---|---|---|---|---|
| P-01 | CA-01 | Abrir `Assets/Scenes/SantuarioPrincipal.unity` y pulsar Play. Observar la consola. | La escena carga; el jugador aparece en `(0, 0.1, -25)`; el HUD muestra `Fragmentos: 0/3` y el objetivo inicial; no hay errores rojos. | |
| P-02 | CA-01 | Revisar `File > Build Profiles` o `EditorBuildSettings`: comprobar que `SantuarioPrincipal` está habilitada y que no se carga ninguna otra escena. | Solo hay una escena jugable activa; no se añaden escenas aditivas. | |
| P-03 | CA-09 | Al iniciar, observar el panel de introducción; esperar unos segundos; pulsar `H`; volver a pulsar `H`. | El panel aparece al inicio, se desvanece solo y `H` lo muestra u oculta sin bloquear el movimiento. | |
| P-04 | CA-02 | Moverse con `WASD` en varias direcciones; mantener `Shift`; detenerse y girar la cámara. | El movimiento es relativo a la cámara, la velocidad aumenta al correr, el personaje gira suavemente y permanece apoyado en el suelo. | |
| P-05 | CA-02 | Mover el ratón para orbitar; mirar hacia arriba y hacia abajo hasta los extremos; pulsar `Esc`; acercar la cámara a una pared. | La cámara orbita, respeta los límites de pitch, `Esc` libera/recupera el cursor y la cámara evita atravesar la pared. | |
| P-06 | CA-03 | Acercarse a `Fragment_1` o a un portal; alejarse; pulsar `E` dentro y fuera de la zona. | El prompt aparece solo dentro de la zona, desaparece al salir y `E` ejecuta una acción por pulsación, no de forma continua. | |
| P-07 | CA-04 | Ir a `Fragment_1` en `(-25, 1, 20)` y pulsar `E`. | El fragmento desaparece, el HUD pasa a `1/3`, aparece el aviso y el objetivo cambia a cruzar el portal. | |
| P-08 | CA-05 | En una partida nueva, ir a `Portal_A` en `(30, 0, 10)` con 0 fragmentos y pulsar `E`. | El portal muestra el prompt de bloqueo y no se produce ningún viaje. | |
| P-09 | CA-05 | Recoger `Fragment_1`; ir a `Portal_A`; pulsar `E`; observar la transición; intentar viajar de nuevo de inmediato. | Hay fundido a negro, llegada a `(170, 0.1, -16)`, la cámara se reorienta, el control se recupera y el enfriamiento impide un viaje repetido. | |
| P-10 | CA-06 | En la región B, ir a `Portal_B` en `(170, 0, -10)` y pulsar `E`; observar la llegada. | El jugador llega a `(30, 0.1, 4)`, recupera el control y no puede repetir el viaje durante la transición. | |
| P-11 | CA-04 | En la región B, ir a `Fragment_2` en `(225, 1, 20)` y pulsar `E`. | El fragmento se recoge y el HUD pasa a `2/3`. | |
| P-12 | CA-04 | Ir a la zona de escaleras de la región B, subir a la plataforma y pulsar `E` en `Fragment_3` (instancia en `(190, 2.5, 32)`). | Se pueden subir las escaleras, el fragmento se recoge y el HUD pasa a `3/3`; el objetivo cambia a activar el núcleo. | |
| P-13 | CA-07 | En una partida nueva, ir al núcleo en `(0, 0, 30)` con menos de 3 fragmentos y pulsar `E`. | El prompt indica cuántos fragmentos faltan y el núcleo no completa la misión. | |
| P-14 | CA-07 | Con 3 fragmentos, ir al núcleo y pulsar `E`; esperar la transición. | La misión se completa, el brillo del núcleo cambia, el panel de victoria aparece tras la espera configurada, se bloquean jugador y cámara y el cursor queda visible. | |
| P-15 | CA-08 | En el panel de victoria, pulsar el botón de reinicio. | La misma escena se recarga, el HUD vuelve a `0/3`, el jugador reaparece en el spawn y la introducción vuelve a mostrarse. | |
| P-16 | CA-10 | Pulsar `1`, `2`, `3` y `4`; pulsar `P`; mantener `F`; observar el reloj, el sol, el cielo y el ambiente. | Cada tecla fija la fase correspondiente, `P` pausa, `F` acelera, el reloj se actualiza y la iluminación responde. | |
| P-17 | CA-11 | Pulsar `4` (medianoche) y observar los faroles y los cristales de la región B. | Los faroles se encienden, los cristales de B aumentan su emisión/intensidad y el ambiente se ve nocturno. Solo hay `CrystalGlow_B` configurado. | |
| P-18 | CA-11 | Viajar a la región B, observar el ambiente; regresar a la región A y comparar. | En B se aprecian tinte violeta, viñeta y bloom; A no recibe ese volumen local; el post-procesado de la cámara está activo. | |
| P-19 | CA-12 | Recorrer los bordes y muros de ambas regiones; chocar con árboles, rocas y muros; atravesar hierba y arbustos; recorrer caminos y escaleras. | No se sale de la región ni se cae al vacío; los obstáculos sólidos bloquean; la vegetación pequeña no bloquea; caminos y escaleras son transitables. | |
| P-20 | CA-13 | Revisar `Batching Static` en `Decor_A`, `Path_A`, `Cliffs_A`, `Decor_B`, `Path_B`, `Cliffs_B` y `Stairs_B`; comprobar que portales, fragmentos, núcleo y cristales no estén estáticos; abrir Stats y jugar. | Solo los contenedores estáticos tienen `Batching Static`; las luces no se marcan; se anotan FPS, Batches y Triángulos sin fijar un umbral no medido. | |
| P-21 | CA-14 | Ejecutar `Tools/ECHO/Build Windows` desde el menú de Unity. | El reporte del build termina correctamente y aparecen `Builds/ECHO_SantuarioDelTiempo.exe` y su carpeta `_Data`; no hay errores de compilación. | |
| P-22 | CA-14 | Ejecutar `Builds/ECHO_SantuarioDelTiempo.exe` y comprobar el arranque, la entrada, el HUD y la consola del build. | El ejecutable inicia `SantuarioPrincipal`, el jugador y la interfaz responden y no aparecen errores rojos. | |
| P-23 | CA-14 | Ejecutar Unity en batch con `-executeMethod Echo.EditorTools.BuildWindows.Build`; revisar el código de salida y el log. | El proceso termina con código 0, actualiza los artefactos de `Builds/` y escribe un reporte claro; si falla, termina con código 1 y un error comprensible. | |
| P-24 | CA-15 | Ejecutar una partida completa: inicio, fragmento 1, portal, fragmentos 2 y 3, regreso, núcleo, victoria y reinicio; revisar la consola durante todo el recorrido. | La secuencia se completa sin errores rojos ni referencias faltantes; no se usa `UnityEngine.Input` clásico. | |
| P-25 | CA-10, CA-20 | Al iniciar no debe verse el indicador de hora. Pulsar `F1`; volver a pulsar `F1`; luego comprobar que `1`, `2`, `3`, `4`, `F` y `P` siguen funcionando. | El reloj empieza oculto, `F1` lo muestra y lo oculta, y las demás teclas de prueba conservan su comportamiento. | |
| P-26 | CA-17 | Jugar la misión observando la baliza: al inicio; tras el primer fragmento; en la región B; con los tres fragmentos; en la región A al final; al ganar; tras reiniciar. | La baliza está sobre Fragment_1; salta a Portal_A (jugador con X menor que 100); en B va al fragmento pendiente más cercano; luego a Portal_B o al núcleo según la región; desaparece al ganar y reaparece tras el reinicio. | |
| P-27 | CA-18 | Recoger un fragmento y usar un portal caminando; completar la misión y observar el personaje. | El gesto Interact se ve en el torso/brazos sin detener el caminado (capa UpperBody) y el personaje queda en pose de victoria hasta que aparece el panel. | |
| P-28 | CA-19 | Pulsar `2` (mediodía) y observar el cielo y la región A; pulsar `4` (medianoche) y observar de nuevo; viajar a la región B de día y de noche. | De día no hay estrellas ni luciérnagas; de noche sí, sin coste notable de FPS; las esporas violetas flotan en B de día y de noche. | |
| P-29 | CA-20 | Observar los muros de `Cliffs_A` y `Cliffs_B` desde dentro de cada región y desde el portal; intentar salir por los bordes. | Los muros parecen roca continua (no cajas grises) y no se puede salir de las regiones. | |
| P-30 | CA-09, CA-20 | Al iniciar la partida, leer el texto de créditos del panel de título. | Los créditos (Kenney, Mixamo, ambientCG, audio Kenney) son legibles y pequeños. | |
| P-31 | CA-16 | Caminar y correr por el suelo de ambas regiones escuchando los pasos. | Los pasos suenan al caminar y al correr con intervalos distintos, con variación de clip y tono, y sin saturar. | |
| P-32 | CA-16 | Recoger un fragmento, usar un portal, activar el núcleo con 3 fragmentos y reiniciar. | Suena un efecto por cada evento (fragmento, portal, núcleo, victoria) y un clic al pulsar el botón de reinicio. | |
| P-33 | CA-16 | Escuchar la música y el ambiente al inicio, de noche, y tras reiniciar la partida. | Música y ambiente en loop a volumen suave; el ambiente varía con la noche y la música no se corta al reiniciar. | |

## 8. Observaciones y pendientes

- La escena contiene `Test_Wall`, `Test_Step` y `Test_Tall`. La Fase 7 pedía eliminarlos; no se modifican en esta tarea. Si el enunciado exige su ausencia, deben registrarse como fallo o limpiarse en una tarea aparte.
- `Assets/Scripts/Mission/EnergyFragment..cs` fue renombrado a `EnergyFragment.cs` (la clase siempre fue `EnergyFragment`); la observación original quedó obsoleta y se retira.
- Solo existe `CrystalGlow_B`; no hay `CrystalGlow_A` en la escena. Si el enunciado exige brillo nocturno en ambas regiones, queda pendiente.
- La instancia real de `Fragment_3` está en `(190, 2.5, 32)`. El marcador `Marker_Fragment3` y `AGENTS.md` tienen valores distintos; el caso P-12 usa la instancia real.
- El asset `PC_RPAsset` limita las luces adicionales por objeto a 4, aunque `AGENTS.md` recomiende mantener menos de 8 luces cerca del jugador. Para la prueba P-20 se usa el valor real del asset.
- No hay mediciones de rendimiento en el repositorio. Cualquier cifra debe obtenerse con Stats y guardarse como evidencia antes de citarla.
- Los bloques de pulido 1, 3, 5 y 7 (`BuildCliffDressing`, `StarField`/`AmbientParticles`/`Setup Ambience`, etapa 2 de `Setup Player Character`, `ObjectiveBeacon`/`Setup Credits`/reloj F1) están escritos pero sin verificar en Unity; los casos P-25 a P-30 dependen de ejecutar sus herramientas primero.
- El bloque 6 (audio) no está implementado aún: los casos P-31 a P-33 deben marcarse `BLOQUEADO` o `N/A` hasta entonces, sin inventar resultados.

## 9. Registro de ejecución

| Fecha | Ejecutor | Equipo / versión | Resultado global | Evidencia |
|---|---|---|---|---|
| | | | | |
