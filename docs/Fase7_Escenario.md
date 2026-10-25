# FASE 7: Diseño del escenario

**Objetivo:** convertir las dos regiones de planos con cubos en un escenario con identidad propia, sin romper ninguna mecánica ya probada.

**Decisiones de diseño:**
- **Muros de acantilado.** Cada región queda rodeada por cuatro muros altos que impiden caerse y bloquean la vista entre regiones. El `Ground` termina en ±40 m del centro, y sin muros el jugador caería al vacío.
- **Diferencia de ambiente.** El ciclo de día y cielo es global, así que la región B se distingue con un **Volume local de URP** (tinte violeta, viñeta, bloom). Es un concepto de computación gráfica que no necesita código.
- **Cristales con protagonismo nocturno.** Un script pequeño sube el brillo de los cristales cuando cae la noche.
- **Una sola pieza por tipo de objeto.** Creas un prefab con su collider una vez y lo reutilizas.

**Importante:** no verifiqué los nombres exactos de los modelos de Kenney. Búscalos en el Project por palabra clave y mira las miniaturas: `tree`, `rock`, `stone`, `bush`, `plant`, `grass` en Nature Kit; `wall`, `tower`, `gate`, `stairs` en Castle Kit. Si algo no importa bien, los cubos escalados sirven igual.

---

## Paso 0. Verificaciones

1. `RegionA_Amanecer` está en (0, 0, 0) y `RegionB_Crepusculo` en (200, 0, 0).
2. Los modelos de Kenney están en `Assets/_Project/Models/` y se ven sin color rosa.
3. Los fragmentos, portales y núcleo siguen en sus posiciones. **Todas las coordenadas de este paso son locales**, relativas al padre de cada región, y los mismos números sirven para A y B.
4. Guarda una copia .zip del proyecto antes de empezar.

## Paso 1. Limpieza y muros

1. **Borra** los objetos de prueba `Test_Wall`, `Test_Step` y `Test_Tall`.
2. **Muros de acantilado.** Dentro de `RegionA_Amanecer`, crea un objeto vacío `Cliffs_A`. Dentro de él, cuatro cubos (clic derecho > *3D Object > Cube*) con material `M_Stone`:

   | Nombre | Position | Scale |
   |---|---|---|
   | `Cliff_N` | (0, 7, 40) | (84, 14, 4) |
   | `Cliff_S` | (0, 7, -40) | (84, 14, 4) |
   | `Cliff_E` | (40, 7, 0) | (4, 14, 84) |
   | `Cliff_W` | (-40, 7, 0) | (4, 14, 84) |

3. Duplica `Cliffs_A` con `Ctrl+D`, renómbralo `Cliffs_B` y arrástralo dentro de `RegionB_Crepusculo`. Pon su Position local en (0, 0, 0) y cambia el material a `M_StoneDark` (se crea en el paso 2).
4. Los cubos ya traen Box Collider, así que bloquean al jugador y a la cámara.
5. **Opcional, horizonte:** un *Plane* `Ground_Far` en (100, -2, 0), Scale (100, 1, 100), sin Mesh Collider y con material `M_StoneDark`. Evita que se vea el vacío en el horizonte.

## Paso 2. Materiales y prefabs base

**Materiales nuevos** (clic derecho en `Materials` > *Create > Material*, shader *Universal Render Pipeline/Lit*):
- `M_StoneDark`: gris violáceo oscuro, aprox. (60, 55, 75).
- `M_Crystal_Violet`: base violeta; activa *Emission* con un color HDR magenta (aprox. 1.6, 0.2, 2.2).

**Prefabs con collider.** Un modelo importado no trae collider. Para cada tipo que uses (un árbol, una roca, una pieza de muro, una columna):
1. Arrástralo a la escena.
2. Agrega el collider adecuado: *Capsule Collider* en el tronco del árbol, *Box Collider* en rocas y columnas, *Mesh Collider* en muros irregulares.
3. Arrástralo a `Assets/_Project/Prefabs` para crear su prefab.
4. Usa las instancias de ese prefab en toda la escena.

La hierba, las flores y las plantas pequeñas **no** llevan collider.

**Prefab de cristal.**
1. Crea un objeto vacío `Crystal_Cluster`.
2. Añade tres cubos hijos con Scale (0.3, 1.2, 0.3), rotaciones distintas, p. ej. (10, 0, 15), (-15, 40, -10) y (5, 80, 20), y material `M_Crystal_Emissive` (o `M_Crystal_Violet`).
3. Quita el Box Collider de los tres.
4. Guárdalo en `Prefabs`.

Los clusters grandes llevarán una *Point Light* hija (cian o violeta, Intensity 1.5, Range 5). Pon luces solo en 3 o 4 clusters y deja el resto solo emisivo.

## Paso 3. Región A: Santuario del Amanecer

Crea tres padres vacíos dentro de `RegionA_Amanecer`: `Decor_A`, `Path_A` y `Lanterns_A`. Todo lo siguiente va dentro de ellos.

**Mantén libre un radio de 3 m** alrededor de `PlayerSpawn`, `Fragment_1`, `Portal_A`, el núcleo y el camino.

1. **Camino de losas** en `Path_A`: cubos planos Scale (1.5, 0.1, 1.5), material `M_Stone`, en estas posiciones (X, Y, Z): (0, 0.05, -18), (0.5, 0.05, -12), (-0.5, 0.05, -6), (0, 0.05, 0), (0.5, 0.05, 6), (0, 0.05, 12), (-0.5, 0.05, 18), (0, 0.05, 24). Guían de la aparición al núcleo.
2. **Ruinas del núcleo** en `Decor_A`: cuatro columnas o pilares de Castle Kit en (±6, 0, 26) y (±6, 0, 34). Añade un par de muros rotos de Castle Kit detrás del núcleo, en z = 36.
3. **Vegetación:** unos 12 árboles en la franja entre 30 y 37 m del centro (junto a los muros), 8 arbustos y 8 rocas repartidos a mano. Varía la rotación en Y y la escala (0.8 a 1.3) para que no parezca repetido.
4. **Rocas al pie de los acantilados:** pon rocas grandes (escala 3 a 5) en la base de `Cliff_N`, `Cliff_E` y `Cliff_W` para tapar la forma de cubo.
5. **Faroles** (guían y alimentan las luces nocturnas), en `Lanterns_A`. Crea uno y duplícalo:
   - Un cilindro `Post` con Scale (0.15, 0.8, 0.15) en Y = 0.8, material `M_Stone`.
   - Una *Point Light* hija `Lamp` en Y = 1.8, color (255, 165, 80), Intensity 2, Range 7.
   - Posiciones: (2, 0, -14), (-2, 0, -3), (2, 0, 9), (-2, 0, 21), (26, 0, 7).
6. **Conecta los faroles al ciclo.** Borra `TestNightLight_1` y `TestNightLight_2`. Selecciona `TimeOfDay`, abre **Night Lights**, pon *Size* en 5 y arrastra las 5 luces `Lamp`.

## Paso 4. Región B: Ruinas del Crepúsculo

Dentro de `RegionB_Crepusculo` crea `Decor_B`, `Path_B`, `Crystals_B` y `Stairs_B`. Material de piedra: `M_StoneDark`. Posiciones locales.

1. **Camino** en `Path_B`: las mismas losas planas, con estas posiciones: (-23, 0.05, -6), (-16, 0.05, -2.5), (-9.5, 0.05, 1), (-2.5, 0.05, 5), (4.5, 0.05, 9), (11, 0.05, 12.5), (18, 0.05, 16), (25, 0.05, 20). Un ramal hacia las escaleras: (-6, 0.05, 10), (-8, 0.05, 15), (-10, 0.05, 19).
2. **Plataforma del fragmento 3:** en `Stairs_B`, un cubo `Platform` en (-10, 0.75, 32), Scale (6, 1.5, 6).
3. **Escaleras** (6 escalones de 0.25 m, que el personaje sube porque su *Step Offset* es 0.3). Cubos con X = -10 y Scale X = 3:

   | Escalón | Scale (X, Y, Z) | Position (Y, Z) |
   |---|---|---|
   | 1 | (3, 0.25, 1.2) | (0.125, 22.4) |
   | 2 | (3, 0.5, 1.2) | (0.25, 23.6) |
   | 3 | (3, 0.75, 1.2) | (0.375, 24.8) |
   | 4 | (3, 1.0, 1.2) | (0.5, 26.0) |
   | 5 | (3, 1.25, 1.2) | (0.625, 27.2) |
   | 6 | (3, 1.5, 1.2) | (0.75, 28.4) |

4. **Reubica el fragmento 3.** Selecciona `Fragment_3` y pon su Position local en **(-10, 2.5, 32)**, sobre la plataforma. Es un valor local y en el mundo queda en (190, 2.5, 32).
5. **Ruinas:** piezas de Castle Kit rotas, 8 a 10 en total, con posiciones libres como (-20, 0, 15), (-15, 0, 25), (10, 0, -5), (20, 0, 5), (5, 0, 28). Inclina o hunde algunas para que parezcan caídas.
6. **Cristales:** 8 a 10 clusters en `Crystals_B`, con los materiales cian y violeta mezclados. Colócalos junto a `Fragment_2` (25, 1, 20) y en los bordes, no sobre el camino.
7. **Rocas** al pie de los acantilados, como en la región A.
8. **Luces de ambiente:** coloca una *Point Light* violeta (180, 60, 255), Intensity 1.5, Range 8, en 2 o 3 puntos del camino.

## Paso 5. Script del brillo nocturno

Crea `Assets/_Project/Scripts/Environment/CrystalNightGlow.cs`:

```csharp
using UnityEngine;

namespace Echo.World
{
    /// <summary>
    /// Sube el brillo de los cristales (emisión y luces) a medida que cae la noche.
    /// Va en un objeto vacío. 'crystalsRoot' es el padre que contiene los cristales.
    /// Usa la emisión del material de cada renderer como valor base.
    /// </summary>
    public class CrystalNightGlow : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private TimeOfDayController timeOfDay;
        [Tooltip("Padre de todos los cristales (por ejemplo Crystals_B).")]
        [SerializeField] private Transform crystalsRoot;

        [Header("Brillo")]
        [Tooltip("Multiplicador de la emisión durante el día.")]
        [SerializeField] private float dayMultiplier = 0.6f;
        [Tooltip("Multiplicador de la emisión durante la noche.")]
        [SerializeField] private float nightMultiplier = 2.5f;
        [SerializeField] private bool controlLights = true;

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

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

            for (int i = 0; i < renderers.Length; i++)
            {
                Material material = renderers[i].sharedMaterial;
                if (material != null && material.HasProperty(EmissionColorId))
                {
                    baseEmission[i] = material.GetColor(EmissionColorId);
                    hasEmission[i] = true;
                }
            }

            lights = controlLights ? crystalsRoot.GetComponentsInChildren<Light>(true) : new Light[0];
            baseLightIntensity = new float[lights.Length];
            for (int i = 0; i < lights.Length; i++) baseLightIntensity[i] = lights[i].intensity;

            if (renderers.Length == 0)
            {
                Debug.LogWarning("CrystalNightGlow: 'Crystals Root' no tiene renderers hijos.", this);
            }
        }

        private void Update()
        {
            float multiplier = Mathf.Lerp(dayMultiplier, nightMultiplier, timeOfDay.NightFactor);

            for (int i = 0; i < renderers.Length; i++)
            {
                if (!hasEmission[i]) continue;

                renderers[i].GetPropertyBlock(block);
                block.SetColor(EmissionColorId, baseEmission[i] * multiplier);
                renderers[i].SetPropertyBlock(block);
            }

            for (int i = 0; i < lights.Length; i++)
            {
                lights[i].intensity = baseLightIntensity[i] * multiplier;
            }
        }
    }
}
```

**Asignación:**
1. Crea un objeto vacío `CrystalGlow` y arrastra el script sobre él.
2. *Time Of Day*: arrastra el objeto `TimeOfDay`.
3. *Crystals Root*: arrastra `Crystals_B`.
4. Valores por defecto: Day 0.6, Night 2.5.

Para que los cristales de la región A también brillen, crea un `Crystals_A` y otro `CrystalGlow_A` con el mismo procedimiento.

## Paso 6. Ambiente de la región B (Volume local)

1. **Comprueba el post-procesado.** Selecciona `Main Camera` y verifica en el Inspector que *Rendering > Post Processing* esté marcado.
2. **Global Volume.** Selecciónalo. En su perfil, comprueba que exista **Bloom** (Threshold 1, Intensity 0.6). Si no está, usa *Add Override > Post-processing > Bloom*.
3. **Volume local.** Clic derecho sobre `RegionB_Crepusculo` > *Volume > Box Volume* (en el menú *GameObject* si no aparece). Nómbralo `Volume_RegionB`, con Position local (0, 15, 0).
4. En el **Box Collider** del objeto: Size (84, 30, 84).
5. En el componente **Volume**: *Mode* = **Local**, Weight 1, Priority 1, *Blend Distance* 8. Pulsa **New** junto a *Profile*. Mueve después ese asset a `_Project/VFX`.
6. *Add Override* y configura:
   - **Color Adjustments:** *Color Filter* violeta claro (200, 170, 255), *Saturation* +15, *Post Exposure* -0.3.
   - **Vignette:** *Intensity* 0.3.
   - **Bloom:** *Threshold* 0.9, *Intensity* 1.2.
7. Para cada override, marca la casilla de cada parámetro que quieras que se aplique.

Esto cambia el aspecto de toda la región B sin tocar el cielo, que sigue el ciclo global.

## Paso 7. Optimización

1. **Objetos estáticos.** Selecciona `Decor_A`, `Path_A`, `Cliffs_A`, `Decor_B`, `Path_B`, `Cliffs_B` y `Stairs_B`. En el Inspector, abre el desplegable **Static** y marca solo **Batching Static** (sin *Contribute GI*, porque la iluminación es dinámica). Acepta el cambio a los hijos.
2. **No marques como estático** los portales, fragmentos, núcleo, cristales con script ni nada que se mueva o cambie de material.
3. **Mide con Stats.** Pulsa Play y abre *Stats* arriba en la ventana Game. Anota FPS, *Batches* y *Triangles*. No afirmes ninguna cifra en tu documentación hasta que la hayas medido tú.
4. **Luces.** Cuenta las luces en cada región. Mantén cada una por debajo de unas 8 cerca del jugador, que es el límite por defecto de luces adicionales por objeto en URP.
5. **Sombras.** Si baja el rendimiento, reduce *Shadows > Max Distance* en el asset URP, de 60 a 40.
6. Si el escenario es pesado, reduce la cantidad de árboles y rocas antes que bajar la calidad visual.

## Paso 8. Pruebas

Guarda con `Ctrl+S` y pulsa Play.

- [ ] Desde `PlayerSpawn` **no se ve** la región B. Los muros y las rocas bloquean la vista.
- [ ] No puedes salir de ninguna región ni caerte por el borde, pegándote a cada muro en las cuatro direcciones.
- [ ] El camino de losas va de la aparición al núcleo y se puede caminar sin atascos.
- [ ] Las regiones se ven claramente distintas: A verde y cálida, B violeta y oscura.
- [ ] Al llegar a B por el portal, el tinte violeta, la viñeta y el bloom ya están aplicados.
- [ ] Los árboles, rocas y muros **bloquean** al jugador y a la cámara. La hierba y las plantas no.
- [ ] Puedes subir las escaleras hasta la plataforma y recoger `Fragment_3`. Los fragmentos 1 y 2 siguen siendo alcanzables.
- [ ] Pulsa **4** (medianoche): los faroles se encienden, los cristales brillan **más** que de día y el entorno se ve azulado.
- [ ] Portales, fragmentos, núcleo, victoria y reinicio siguen funcionando.
- [ ] La Console no tiene errores rojos y los FPS se mantienen fluidos en tu computador.

## Errores probables

- **Se ven muros demasiado cuadrados:** pon más rocas al pie y en la parte alta. No hace falta tocar los cubos.
- **El personaje se atasca en las escaleras:** revisa el *Step Offset* del Character Controller (0.3) y que los escalones sean de 0.25 m. Si sigue atascado, baja los peldaños a 0.2.
- **No sube a la plataforma:** el último escalón mide 1.5 y la plataforma también 1.5, así que deben quedar a ras. Compara las alturas con la tabla.
- **Un árbol bloquea la cámara de forma molesta:** pon el collider más fino, solo en el tronco.
- **No se ve el tinte violeta en B:** revisa *Post Processing* en la cámara, que el Volume esté en *Local*, que el Box Collider sea de tamaño (84, 30, 84), y que cada override tenga marcadas las casillas de sus parámetros.
- **El tinte aparece en A también:** el Box Collider del volume es demasiado grande, o no está dentro de `RegionB_Crepusculo`.
- **Los cristales no cambian de brillo:** `Crystals Root` está vacío o los materiales no tienen *Emission* activada. La Console dará el mensaje.
- **Los cristales se ven rosa:** algún material no es URP/Lit.
- **Aviso por luces `Missing` en `TimeOfDay`:** la lista de Night Lights aún apunta a las luces borradas. Reasígnala con los faroles.
- **Bajón de rendimiento:** revisa que `Decor_A`, `Decor_B` y los acantilados tengan *Batching Static* y que no haya demasiadas luces.

Construye primero la región A, pruébala y avísame antes de seguir con la B. Si todo funciona, pasamos a la **Fase 8**: prueba completa, compilación para Windows y documentación académica.