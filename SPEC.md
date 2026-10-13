# SPEC: Juego 3D de colarse en TransMilenio (nombre de trabajo: "Transmilleno")

> Documento de especificación para un agente de IA (GitHub Copilot agent mode). Léelo COMPLETO antes de escribir código. Este archivo es la fuente de verdad del proyecto. Si algo no está definido aquí, decide tú con criterio profesional y documenta la decisión en `Docs/DECISIONS.md`.

---

## 0. Reglas de trabajo para el agente (LEER PRIMERO)

0. **Actúa como un desarrollador profesional de videojuegos (senior).** Eres a la vez programador de gameplay, artista técnico, diseñador de niveles, diseñador de UI/UX y productor. Toma las decisiones que tomaría un profesional con años en la industria: de diseño, de arquitectura, de arte, de balance, de rendimiento y de calidad. Cuando algo no esté especificado, **decide tú con criterio profesional, justifícalo brevemente en `Docs/DECISIONS.md` y sigue adelante**, sin preguntar. El estándar de referencia es un juego indie pulido y listo para publicar, no una demo técnica ni un prototipo.
1. **Autonomía total.** No te detengas a pedir permiso por cada cosa. Trabaja por hitos (sección 18) hasta dejar el juego completo, jugable de principio a fin y sin errores en consola.
2. **Instala lo que falte.** Si falta un paquete de Unity (por ejemplo, soporte para modelos `.glb`), instálalo tú editando `Packages/manifest.json` y espera a que Unity lo resuelva. No pares hasta que todo el proyecto compile.
3. **Verifica siempre que compila.** Después de cada bloque de cambios, comprueba que el proyecto compila sin errores. Si tienes herramientas MCP de Unity, úsalas para leer la Console. Si no, usa modo batch:
   `"<RutaUnity>\Unity.exe" -batchmode -quit -nographics -projectPath "<RutaProyecto>" -logFile -`
   y lee el log en busca de `error CS`.
4. **No edites a mano** archivos `.meta`, `Library/`, `Temp/`, ni YAML de escenas/prefabs. Para crear escenas, prefabs, materiales, NavMesh, animator controllers, etc., escribe **scripts de Editor** (`Assets/_Project/Scripts/Editor/`) con menú `Tools/Transmilleno/...` y ejecútalos con `-executeMethod` en modo batch (o con MCP si está disponible). Debe existir un menú `Tools/Transmilleno/Build Everything` que reconstruya escenas, prefabs y configuración desde cero de forma reproducible.
5. **Si falta un modelo o asset, TÚ lo creas, y debe verse bien.** El usuario dejará modelos 3D en `Assets/_Project/Art/Models/` (ver sección 3). **Usa siempre los modelos del usuario cuando existan.** Si falta alguno (o está incompleto), **está prohibido usar cubos, cápsulas, esferas o primitivas sin trabajar como arte final.** Debes diseñar y construir tú mismo el modelo faltante con calidad de juego real (ver sección 3.1), de modo que el juego **se vea bonito y coherente** en todo momento. Si el usuario agrega después un modelo propio, un solo punto de configuración (ScriptableObject de catálogo) debe permitir sustituirlo sin tocar código.
6. **Calidad.** Código C# limpio, modular, con namespaces, `[SerializeField] private`, sin `Find*` en `Update`, sin LINQ ni asignaciones en bucles calientes, sin singletons mágicos salvo los explícitamente listados.
7. **Git y repositorio remoto.** Todo el proyecto debe quedar conectado al repositorio de GitHub del usuario: **https://github.com/ternemi13/Trasmilleno** (ver sección 0.1). Haz commits pequeños y descriptivos por cada hito y súbelos al remoto. Asegúrate de tener el `.gitignore` oficial de Unity.
8. **Documenta.** Mantén `Docs/README.md` (cómo abrir, compilar y jugar), `Docs/ARCHITECTURE.md` y `Docs/DECISIONS.md` actualizados.
9. **Definición de terminado:** sección 19.

### 0.1 Conexión con el repositorio de GitHub

**Repositorio remoto:** `https://github.com/ternemi13/Trasmilleno`
(Nota: el repositorio se escribe "Trasmilleno" y la carpeta local del proyecto puede llamarse "Transmilleno". Es normal; usa la URL tal cual está escrita.)

Antes de escribir código, haz esto en orden:

1. **Inspecciona el estado actual.** Revisa si la carpeta local ya es un repositorio Git (`git status`) y si ya tiene un remoto (`git remote -v`).
2. **Conecta el remoto `origin`:**
   - Si no hay repositorio local: `git init`, y luego `git remote add origin https://github.com/ternemi13/Trasmilleno.git`.
   - Si ya existe un `origin` distinto, no lo borres a ciegas: cámbialo con `git remote set-url origin https://github.com/ternemi13/Trasmilleno.git` y anótalo en `Docs/DECISIONS.md`.
3. **Revisa qué hay en el remoto.** Haz `git fetch origin`. Si el repositorio remoto ya tiene commits (por ejemplo un README o un `.gitignore` inicial), integra ese historial con `git pull origin <rama> --allow-unrelated-histories` y resuelve conflictos conservando el contenido útil del remoto. **Nunca uses `git push --force`** sin que el usuario lo pida expresamente.
4. **Rama principal:** usa la rama por defecto del remoto (normalmente `main`). Si no existe, créala como `main`.
5. **`.gitignore` de Unity** en la raíz (ignora `Library/`, `Temp/`, `Obj/`, `Logs/`, `Builds/`, `UserSettings/`, archivos `.csproj`/`.sln` generados, etc.). **Sí deben versionarse** `Assets/`, `Packages/` y `ProjectSettings/`, incluidos los `.meta`.
6. **Archivos grandes (modelos, texturas, audio):** configura **Git LFS** antes del primer commit con assets pesados:
   `git lfs install` y `git lfs track` para `*.fbx`, `*.glb`, `*.gltf`, `*.obj`, `*.png`, `*.jpg`, `*.tga`, `*.psd`, `*.wav`, `*.mp3`, `*.ogg`, `*.mp4`, `*.exr`, `*.hdr`. Confirma que `.gitattributes` quede commiteado. (Ten en cuenta los límites de GitHub: archivos de más de 100 MB se rechazan sin LFS.)
7. **Flujo de commits:** un commit por cada hito de la sección 18 (como mínimo), con mensajes claros, por ejemplo `feat(player): locomoción y estamina` o `feat(transit): ciclo de parada del bus`. Después de cada hito que compile, haz `git push origin main`.
8. **Si el push falla** por autenticación o permisos, **no te detengas**: sigue trabajando con commits locales, deja la instrucción exacta en `Docs/README.md` para que el usuario haga el push (o configure sus credenciales) y continúa con el siguiente hito. Nunca escribas ni guardes tokens o contraseñas en el repositorio.
9. **No subas** secretos, archivos personales ni la carpeta `Library/`.
10. Añade al `Docs/README.md` el enlace del repositorio y los pasos para clonar y abrir el proyecto en Unity 6.6 (6000.5f1).

---

## 1. Visión del juego

Juego 3D en tercera persona para **PC (Windows)** ambientado en una estación de TransMilenio (Bogotá). El jugador debe **colarse al sistema sin pagar** y **subirse a un bus** para completar cada nivel, evitando o enfrentando a celadores y policía. Cada nivel repite el objetivo con **más dificultad, más guardias y un escenario distinto**. El juego tiene un **número máximo de niveles** (por defecto **10**) y un final.

Tono: acción/sigilo arcade con humor urbano, violencia estilizada (no gore explícito). Cámara en tercera persona, controles precisos, sensación de "parkour callejero".

**Fantasía central:** leer el escenario y escoger tu ruta de colada (saltar torniquete, cruzar la vía del bus, trepar puertas o rejas), improvisar peleas con puños, navaja y objetos del piso, y llegar al bus a tiempo.

---

## 2. Stack técnico

- **Unity 6.6 (6000.5f1)**, plantilla **Universal 3D (URP)**. Backend de scripting Mono para editor, IL2CPP para build final.
- Paquetes (verifica la versión compatible con Unity 6.6 en el Package Manager e instálalos):
  - `com.unity.inputsystem` (Input System nuevo, usar **solo** este; desactivar el input legacy si hace falta).
  - `com.unity.cinemachine` (3.x) para la cámara de tercera persona.
  - `com.unity.ai.navigation` (NavMesh para peatones y guardias).
  - `com.unity.splines` (rutas de buses).
  - `com.unity.cloud.gltfast` (glTFast) para importar `.glb/.gltf`. **Si hay archivos `.glb` en el proyecto y glTFast no está instalado, instálalo automáticamente.**
  - `com.unity.timeline` solo si es necesario.
  - TextMeshPro (incluido en uGUI).
  - `com.unity.ai.assistant` ya viene instalado (no tocar).
- **UI:** UI Toolkit o uGUI (elige uno y sé consistente; recomendado **uGUI + TextMeshPro** por madurez).
- **Datos:** `ScriptableObject` para configuración (niveles, armas, enemigos, ajustes de dificultad).
- **Guardado:** JSON en `Application.persistentDataPath`.

---

## 3. Modelos y assets del usuario

El usuario colocará modelos en estas rutas (créalas si no existen):

```
Assets/_Project/Art/Models/
  Player/          # Personaje principal
  Pedestrians/     # Peatones (uno o varios)
  Guards/          # Celadores (y policía si hay)
  TransMilenio/    # El bus articulado
  Station/         # Estación, torniquetes, rejas, puertas
  Props/           # Objetos recogibles/lanzables y otros
```

Reglas:
- Detecta automáticamente lo que haya en cada carpeta mediante un **script de Editor "Asset Catalog Builder"** que genere/actualice un `AssetCatalog` (ScriptableObject) con referencias por categoría.
- Los modelos pueden venir en `.fbx`, `.obj` o `.glb`. Configura importación correcta (escala, ejes, materiales URP). Si un material viene en Built-in, conviértelo a URP/Lit.
- **Si un modelo no trae rig humanoide**, configúralo como Humanoid si es posible; si no, constrúyele tú un rig/esqueleto funcional (o rehace el modelo con un rig correcto) para que se anime bien. Documenta lo que hiciste.
- **Animaciones:** si el usuario no provee animaciones, usa animaciones libres de licencia compatibles (por ejemplo, descargables por paquete) o genéralas por código (IK, bobbing, tweens). Documenta la fuente y la licencia de todo asset externo en `Docs/CREDITS.md`. No uses assets sin licencia clara.
- **Prohibido el arte "de relleno".** No se aceptan cubos, cápsulas ni esferas con un color como modelo final de nada que el jugador vea (personaje, guardias, peatones, bus, estación, props, escenario).
- **Escala todo a metros reales** (humano ≈ 1.75 m, bus articulado ≈ 18 m, torniquete ≈ 1 m de alto), tanto los modelos del usuario como los que crees.
- Para cualquier asset adicional necesario (modelos, sonidos, texturas, partículas, props de calle), **créalo o consíguelo tú**, con la calidad descrita en 3.1.

### 3.1 Cómo crear el arte que falte (calidad profesional)

Lo que no venga en los modelos del usuario lo produces tú, como lo haría un equipo de desarrollo profesional:

1. **Define una dirección de arte antes de modelar nada** y escríbela en `Docs/ART_BIBLE.md`: estilo visual (recomendado: **estilizado semi-realista / low-poly de buena calidad**, coherente con los modelos del usuario si los hay), paleta de colores, proporciones, nivel de detalle y referencias de la Bogotá real (estaciones, buses rojos, grafitis, calle). Todo el arte nuevo debe seguirla y verse como parte del mismo juego.
2. **Opciones válidas para producir un modelo faltante** (elige la que dé mejor resultado en cada caso):
   - **Modelado procedural por código o Editor scripts** (puedes usar `com.unity.probuilder` o generar mallas propias): extrusiones, biseles, revoluciones, módulos con detalle (ventanas, puertas, logos, luces, ruedas, manijas, barandas, rejas, torniquetes con su mecanismo, etc.). Siluetas reconocibles, **bordes biselados** y varios elementos por objeto, no una sola forma.
   - **Packs de assets libres de derechos (CC0 o licencia que permita uso comercial)**, por ejemplo de fuentes como Kenney, Quaternius, Poly Haven o ambientCG, para props, personajes base, texturas y materiales. Úsalos como base y **adáptalos** (materiales, colores, ropa, accesorios) para que encajen con la dirección de arte.
   - **Combinación:** base libre + personalización procedural.
3. **Personajes** (jugador, guardias, policía, peatones): proporciones humanas correctas, ropa y accesorios distintivos (uniforme de celador, chaleco, gorra, bolillo, uniforme de policía, ropa variada en peatones), **rig Humanoid** válido y **variantes** (colores, ropa, estatura) para que la multitud no se vea repetida.
4. **Texturas y materiales:** no uses colores planos como acabado final. Genera o consigue texturas (ruido, suciedad, desgaste, hojas de detalle, vertex colors, trim sheets) y materiales URP con rugosidad/metalicidad coherentes: concreto, metal, vidrio, pintura roja del bus, asfalto, etc. Añade **decals** de grafitis, manchas y señales.
5. **Animaciones:** usa animaciones libres de derechos compatibles con el rig Humanoid o créalas con código (IK, curvas, tweens). Mixamo requiere cuenta y no es automatizable; si lo usas, documéntalo. El trepar, los golpes y el correr deben verse fluidos, no rígidos.
6. **Verifica visualmente tu propio trabajo.** Crea un script de Editor que tome **capturas de cada modelo y de cada nivel** (a distintas distancias y ángulos) y revísalas. Si algo se ve feo, rígido o poco claro, **rehazlo** antes de seguir. No avances de hito con arte que un jugador notaría como "temporal".
7. **Rendimiento:** todo el arte que crees debe respetar los presupuestos de la sección 14 (polígonos razonables, LODs, materiales compartidos, atlas).
8. **Licencias:** documenta la fuente y la licencia de todo asset externo en `Docs/CREDITS.md`. No uses assets sin licencia clara.
9. **Catálogo:** todos los modelos, tanto los del usuario como los creados por ti, se registran en el mismo `AssetCatalog` para poder reemplazarlos fácilmente.

---

## 4. Arquitectura del proyecto

### 4.1 Estructura de carpetas

```
Assets/_Project/
  Art/            (Models, Materials, Textures, Animations, VFX, UI)
  Audio/          (Music, SFX, Mixers)
  Data/           (ScriptableObjects: Levels, Weapons, Enemies, Catalogs, Settings)
  Prefabs/        (Player, Enemies, Buses, Station, Props, UI, Systems)
  Scenes/         (Boot, MainMenu, Level_01 ... Level_N, o una escena Gameplay + datos)
  Scripts/
    Core/         (GameManager, SceneLoader, ServiceLocator ligero, EventBus, Pooling)
    Player/       (Controller, Climb, Stamina, Health, Combat, Interaction, Camera)
    Combat/       (Hitbox, Damage, Weapons, Throwables)
    AI/           (Guard, Police, Pedestrian, Perception, Behaviours)
    Transit/      (Bus, BusStop, BusSpawner, Doors, Route)
    World/        (Turnstile, Fence, Gate, Climbable, Fare/AccessZones)
    Levels/       (LevelDefinition, LevelBuilder, Difficulty, Progression)
    UI/           (Menus, HUD, Settings, Popups)
    Save/         (SaveSystem, SettingsService)
    Audio/        (AudioManager)
    Utils/
    Editor/       (Build tools, Catalog builder, Validators)
  Tests/          (EditMode y PlayMode)
```

### 4.2 Principios

- **Assembly Definitions** (`.asmdef`) por módulo grande (Core, Player, AI, Transit, UI, Editor, Tests) para compilar rápido y evitar dependencias circulares.
- **Desacople por eventos:** `EventBus` o eventos C# estáticos tipados (`PlayerDied`, `LevelCompleted`, `BusArrived`, `AlertRaised`...). Evita referencias directas entre sistemas de UI y gameplay.
- **Máquinas de estado** explícitas (jugador, guardias, buses). Un estado por clase o enum + switch claro.
- **Data-driven:** niveles, armas, enemigos y dificultad en ScriptableObjects, no hardcodeados.
- **Object pooling** para proyectiles/objetos lanzados, efectos, peatones y sonidos.
- Escenas: `Boot` (inicializa servicios) → `MainMenu` → `Gameplay` (carga el nivel por datos) con carga asíncrona y pantalla de carga.

---

## 5. Jugador

### 5.1 Controles (Input System, con rebind en ajustes)

| Acción | Teclado/Mouse | Mando (soporte recomendado) |
|---|---|---|
| Mover | W A S D | Stick izquierdo |
| Cámara | Mouse | Stick derecho |
| Saltar / Trepar | Espacio | A / Cruz |
| Correr | Shift (mantener) | Gatillo / L3 |
| Puñetazo | Clic izquierdo | X / Cuadrado |
| Navaja / arma equipada | Clic derecho | RT |
| Recoger / Lanzar objeto | E (recoger), clic izq. (lanzar) | Y / Triángulo |
| Agacharse (opcional) | C | B / Círculo |
| Pausa | Esc | Start |

### 5.2 Locomoción

- Controlador **cinemático propio** basado en `CharacterController` o Rigidbody (elige y justifica). Movimiento relativo a la cámara, rotación suave hacia la dirección de movimiento.
- Velocidades: caminar ~2.2 m/s, correr ~5.5 m/s. Aceleración/desaceleración suaves. Salto con altura ~1.2 m, "coyote time" (~0.12 s) y "jump buffer" (~0.15 s).
- Detección de suelo robusta (SphereCast), manejo de pendientes y escalones pequeños (step offset).
- Correr consume **estamina**. Sin estamina no se puede correr ni trepar, y el golpeo es más lento.

### 5.3 Escalar / trepar (mecánica clave)

Cuando el jugador **presiona Espacio frente a un obstáculo escalable**, en vez de un salto normal debe ejecutarse una **animación de trepar** (o de vault si es bajo).

Detección:
- Lanza rayos/SphereCasts hacia delante (a altura de pecho y de cabeza) y hacia arriba/abajo para encontrar el **borde superior** (ledge).
- Clasifica por altura relativa al jugador:
  - **Bajo** (< ~0.5 m): se sube con paso normal.
  - **Vault** (0.5–1.2 m): salto por encima (torniquetes, barandas bajas).
  - **Trepar** (1.2–2.5 m): animación de escalada a pie de pared hasta subirse arriba.
  - **Muy alto** (> 2.5 m): no se puede trepar (salvo rejas, ver 5.4).
- Valida que arriba haya espacio libre (capsule check) y que la superficie superior sea plana/caminable.
- Se activa con Espacio **solo si** el jugador está cerca del obstáculo y mirando hacia él. Si no, salto normal.
- Durante la animación: control bloqueado, el personaje se alinea al borde con **Root Motion o interpolación con curvas** + **IK de manos** a los puntos de agarre. Cancelable solo al inicio.
- Marcador de componente `Climbable` (con tipo, altura máxima, si es escalable) para controlar qué sí y qué no se puede trepar. Los obstáculos sin el componente no se trepan.
- Costo de estamina por trepar.

### 5.4 Escalada de rejas

- Componente `ClimbableFence` para rejas de la estación (altura típica 2.5–4 m).
- El jugador puede **engancharse a la reja** (Espacio/E cerca de ella), moverse en 2D sobre la superficie (arriba/abajo/izquierda/derecha) con animación de agarre, consumiendo estamina de forma continua.
- **Reglas de remate:** puede pasar al otro lado **solo si la reja no tiene techo/obstáculo superior** (campo `HasTopClearance` calculado automáticamente con raycast o configurado). Si tiene techo, el jugador puede colgarse pero no cruzar.
- Si la reja está electrificada o con alambre de púas en algún nivel avanzado, causa daño (opcional, variante por nivel).
- Poder soltarse y caer (daño por caída si altura > 4 m).

### 5.5 Salud y estamina

- **Vida:** 100 HP base. Regeneración **no** automática (o muy lenta fuera de combate; decide y deja configurable). Pickups de curación opcionales en niveles.
- **Estamina:** 100 puntos. Consumo: correr, trepar, golpear, lanzar. Regeneración tras ~1 s sin consumo. Si llega a 0, "agotado" con penalización breve.
- Muerte → pantalla de Game Over con opción de reintentar el nivel o volver al menú.
- Feedback: viñeta roja al recibir daño, sacudida de cámara leve, sonidos.

### 5.6 Combate del jugador

- **Puños:** combo de 3 golpes con ventanas de cancelación, daño bajo, rápido, bajo costo de estamina. Hitboxes activadas por eventos de animación.
- **Navaja** (navajazo): daño alto, corto alcance, rápido. Se obtiene en el escenario o desde el inicio según nivel (configurable). Puede causar sangrado (daño en el tiempo) en enemigos.
- **Objetos del piso:** botellas, ladrillos, piedras, cajas pequeñas, latas, etc. (`Pickable` + `Throwable`).
  - Recoger con `E` (animación de agacharse y levantar), sostener en mano (socket), lanzar con clic izquierdo con arco balístico y apuntado con retícula.
  - Daño según masa/velocidad y tipo; algunos objetos pueden también **distraer** a los guardias por el ruido al impactar.
  - Pool de objetos y límite de objetos activos por rendimiento.
- Sistema de daño unificado: `IDamageable`, `DamageInfo { amount, type, source, knockback }`, hit-stop leve, knockback, reacciones de golpe en enemigos y peatones.

### 5.7 Cámara

- **Cinemachine** tercera persona, orbital con colisión, ajuste de FOV al correr, hombro configurable, sensibilidad ajustable y opción de invertir eje Y.
- Cámara no debe atravesar paredes ni quedar dentro de buses.

---

## 6. Enemigos y NPCs

### 6.1 Celadores (guardias de seguridad)

- Usan **NavMeshAgent** + percepción (visión con cono y distancia, audición para ruidos como objetos lanzados o peleas).
- Estados: `Patrol` → `Suspicious` → `Alert` → `Chase` → `Attack` → `Search` → vuelve a `Patrol`.
- Armamento: **bolillo (porra)**, cuerpo a cuerpo con telegráfico claro (ventana de reacción para esquivar), y en niveles avanzados **policía con armas de fuego** (disparo con precisión/cadencia limitada y daño medio, nada hiperrealista).
- Comunicación: al ver al jugador, **alertan a guardias cercanos** (radio de alerta).
- Pueden **matar al jugador**. Dificultad escala con el nivel: más HP, más velocidad, mejor percepción, más agresividad y menos tiempo de olvido.
- **Reacciones:** aturdirse, caer, huir con poca vida (policía no huye, celador sí a veces), pedir refuerzos.

### 6.2 Policía

- Aparece a partir de cierto nivel (configurable) o cuando el **nivel de alerta global** sube.
- Más resistentes, con **armas de fuego** o taser. Pueden perseguir fuera de la estación.

### 6.3 Peatones

- Poblados con **NavMesh**: caminan por aceras, esperan en andenes, suben/bajan de buses, reaccionan al pánico (huyen cuando hay peleas, gritan, pueden estorbar).
- Densidad configurable por nivel y por ajuste de gráficos. **Optimización:** LOD, culling por distancia, animación simplificada lejos, variantes por material/color (GPU instancing).
- Pueden ser golpeados (con consecuencias: sube el nivel de alerta). Opcional: algunos peatones "ayudan" o "delatan" (moral).

### 6.4 Sistema de alerta global

- Nivel de alerta 0–3 (Tranquilo / Sospecha / Búsqueda / Persecución policial).
- Sube al ser visto colándote, al pelear o robar; baja con el tiempo si estás oculto.
- Afecta la cantidad de guardias que llegan y su comportamiento.

---

## 7. TransMilenio (buses y estación)

### 7.1 Buses

- Bus articulado (modelo del usuario; si falta, constrúyelo tú con el estándar de 3.1: rojo TransMilenio, puertas, ventanas, ruedas, luces y detalles) que recorre una **ruta por spline** en el carril exclusivo.
- Ciclo constante por parada: **Llegar → Frenar → Alinear con puertas → Abrir puertas → Bajan/suben peatones → Cerrar puertas → Arrancar → Salir**.
- **Siempre llegan buses** (spawner con intervalos configurables por nivel, varios buses en distintos carriles).
- **El jugador completa el nivel** si **sube al bus** (cuando tiene las puertas abiertas en parada) estando dentro de la estación. Si el bus parte con el jugador dentro, nivel completado. Si el jugador no logra subir, debe esperar el siguiente.
- **Atropello:** un bus en movimiento **mata al jugador** (o causa daño letal) si lo golpea al cruzar la vía. Detección de colisión por velocidad (por encima de umbral = muerte). También puede atropellar peatones y guardias.
- Los guardias **no** entran al bus tras de ti salvo en niveles específicos (variante: un guardia puede subir).
- Sonidos de motor, frenos y puertas, con atenuación espacial.

### 7.2 Estación

- Elementos: torniquetes con validador, taquillas, puertas de acceso, rejas perimetrales (con y sin techo), plataformas/andenes, puertas de andén, cámaras de seguridad (opcional: aumentan visibilidad de guardias), carteles, bancas, canecas y objetos lanzables.
- **Cada nivel tiene un layout distinto** (estación pequeña a gran estación con múltiples accesos, pasarelas, doble nivel).

---

## 8. Formas de colarse (rutas de entrada)

El juego debe permitir **varias rutas**, y los niveles deben diseñarse de modo que ofrezcan al menos 2 o 3:

1. **Saltar el torniquete:** vault sobre el torniquete (altura ~1 m) mientras un guardia puede o no estar mirando. Hacer ruido si lo ven.
2. **Cruzar la vía del TransMilenio:** correr por el carril exclusivo entre buses, con riesgo de **atropello**. Luego trepar al andén.
3. **Trepar la puerta de la estación:** puertas de entrada de altura media que se pueden escalar con la mecánica de trepar.
4. **Escalar las rejas de la estación:** si la reja tiene espacio libre arriba (sin techo), se puede pasar al otro lado. Algunas rejas están diseñadas como atajos.
5. **Rutas alternas por nivel:** huecos en el cerco, tejados, pasarelas, ventilación, etc. (a criterio, 1 ruta secreta por nivel avanzado).

Cada ruta tiene riesgos/costos distintos (estamina, ruido, tiempo, visibilidad).

---

## 9. Niveles y progresión

- **Cantidad máxima de niveles: 10** (configurable en un `GameProgressionConfig`). Tras el nivel 10 → pantalla de final/créditos.
- Cada nivel se define por un `LevelDefinition` (ScriptableObject) que incluye: layout/escena, número de guardias y policías, tipos de arma, densidad de peatones, frecuencia de buses, objetos recogibles, rutas de entrada, tiempo máximo opcional, objetivos opcionales (secundarios) y modificadores (de noche, lluvia, hora pico).
- **Curva de dificultad sugerida:**
  - N1–2: tutorial implícito, 1–2 celadores, sin policía, estación simple.
  - N3–5: más celadores, rejas con/sin techo, hora pico, primeros policías.
  - N6–8: policías con armas, más buses, estaciones grandes, cámaras.
  - N9–10: máxima densidad, alerta rápida, niveles de noche/lluvia, jefe opcional (jefe de seguridad) en el 10.
- **Generación:** puede ser **escenas diseñadas por scripts de Editor** o **ensamblado modular por prefabs** (módulos de estación, calles, rejas) con semilla fija por nivel. Preferir modularidad: tiles/kit de módulos reutilizables para reducir trabajo.
- Los niveles deben ser **siempre completables** (validador automático que compruebe con NavMesh y marcadores que existe al menos una ruta posible de entrada y un andén accesible).
- Pantalla de resultados al ganar: tiempo, golpes, alertas, ruta usada, estrellas (1–3).

---

## 10. UI/UX (debe verse muy bien)

Estilo visual: moderno, limpio, con identidad urbana bogotana (colores rojo TransMilenio, grises de concreto, tipografías legibles). Animaciones suaves (fade, slide), sonidos de UI y soporte de resoluciones (16:9, 16:10, 21:9) con escalado correcto.

### 10.1 Menú principal
- Fondo atractivo (escena 3D con la estación y bus pasando, cámara suave) o arte.
- Botones: **Continuar**, **Nueva partida**, **Seleccionar nivel** (solo niveles desbloqueados), **Ajustes**, **Créditos**, **Salir**.
- Confirmación al borrar partida guardada.

### 10.2 HUD
- Barra de **vida** y **estamina**, icono del arma/objeto equipado, indicador de **nivel de alerta**, minimapa o brújula opcional, indicador del próximo bus (tiempo estimado), prompts contextuales ("Espacio: trepar", "E: recoger"), objetivos del nivel.
- Indicadores de dirección de daño. Mensajes breves de eventos.

### 10.3 Pausa, Game Over, Nivel completado
- Pausa: continuar, ajustes, reiniciar nivel, menú principal.
- Game Over: causa de muerte y reintentar.
- Nivel completado: resumen, estrellas, **Siguiente nivel**, repetir, menú.

### 10.4 Ajustes
- **Audio:** volumen maestro, música, efectos, voces/ambiente (AudioMixer con parámetros expuestos).
- **Gráficos:** calidad (Bajo/Medio/Alto/Ultra), resolución, pantalla completa/ventana/sin bordes, VSync, límite de FPS, sombras, calidad de texturas, anti-aliasing, distancia de dibujado, densidad de peatones, post-procesado (bloom, SSAO, motion blur on/off), escala de render.
- **Controles:** rebind de teclas (Input System), sensibilidad del mouse, invertir Y.
- **Juego:** idioma (español por defecto, preparar localización con tablas), subtítulos, dificultad (Casual/Normal/Difícil), mostrar ayudas.
- Todo se **guarda y aplica al instante** (con botón "Restaurar valores").

### 10.5 Localización
- Todas las cadenas en tabla (por ejemplo `Localization` package o sistema propio con JSON). Idioma inicial: **español (Colombia)**. Preparado para inglés.

---

## 11. Guardado y progreso

- `SaveSystem` con JSON versionado: niveles desbloqueados, estrellas por nivel, mejor tiempo, ajustes, estadísticas globales.
- **Guardado automático** al completar cada nivel y al cambiar ajustes. Múltiples slots opcionales (mínimo 1).
- Manejo de errores/corrupción (copia de seguridad `.bak`).
- Opcional: checkpoints dentro de nivel.

---

## 12. Audio

- `AudioManager` con AudioMixer (Master, Music, SFX, Ambience, UI).
- Música dinámica: calma → tensión según nivel de alerta.
- SFX: pasos según superficie, golpes, navaja, impactos, buses (motor/frenos/puertas), ambiente de ciudad (tráfico, multitud), voces/gritos genéricos, alarmas.
- Audio 3D espacial con atenuación y oclusión simple. Pooling de `AudioSource`.
- Usa sonidos libres de licencia o generados proceduralmente, documentados en `Docs/CREDITS.md`.

---

## 13. Gráficos y ambientación

- **URP** con perfil de calidad por nivel (Low/Med/High/Ultra) y Volume de post-procesado (bloom, color grading, SSAO, viñeta suave, tone mapping).
- Iluminación: luz direccional + luces puntuales de la estación; **baked/mixed lighting** donde sea posible, **light probes** y **reflection probes**.
- Variantes: día, atardecer, noche, lluvia (partículas y mojado del suelo) según nivel.
- Escenario creíble y bonito, construido con arte de calidad (no bloques simples): calle, aceras, postes, semáforos, grafitis, vendedores ambulantes (props), tráfico de fondo (bajo costo), edificios con LOD o skybox/ciudad lejana.
- Efectos: golpes, sangre estilizada leve, polvo, chispas, humo de buses. Todo con **pooling** y límites.

---

## 14. Optimización (requisito, no opcional)

- Objetivo: **60 FPS estables** en una PC media (GPU gama media, 16 GB RAM) a 1080p en calidad Media.
- **LOD Groups**, occlusion culling, static batching, GPU instancing, SRP Batcher compatible, atlas de texturas, compresión de texturas y mallas correcta.
- NavMesh agents con **actualización escalonada** (time slicing) y límite de agentes activos; peatones lejanos desactivados o simplificados.
- Evitar GC: pooling, cachear componentes, sin `GetComponent`/`Find` en `Update`, sin strings concatenados en bucles.
- Física: capas y matriz de colisiones bien configuradas, colliders simples.
- **Profiling:** crea una escena/herramienta de test de estrés y registra resultados en `Docs/PERFORMANCE.md`.
- Tiempos de carga cortos con carga asíncrona.

---

## 15. Dificultad (ajustes)

- Niveles de dificultad globales (Casual / Normal / Difícil) que modifican: daño recibido, percepción de guardias, regeneración, tiempo entre buses.
- Todo expuesto en ScriptableObjects (`DifficultySettings`) para balancear sin tocar código.

---

## 16. Herramientas de Editor y calidad

- `Tools/Transmilleno/Build Everything`: reconstruye escenas, prefabs, catálogo y configuración.
- `Tools/Transmilleno/Validate Project`: revisa referencias nulas, modelos faltantes, NavMesh, que cada nivel sea completable, y que no haya errores.
- `Tools/Transmilleno/Setup Packages`: verifica e instala paquetes requeridos.
- **Tests:**
  - EditMode: cálculo de daño, estamina, guardado/carga, validación de datos de nivel.
  - PlayMode: el jugador puede trepar un `Climbable`, un bus atropella, completar un nivel de prueba.
- Logging estructurado con niveles, desactivable en build.

---

## 17. Build y entrega

- Configura `ProjectSettings` para **Windows 64-bit**, IL2CPP, calidad por defecto Media, resolución y pantalla completa por defecto, icono y nombre del producto.
- Script de build `Tools/Transmilleno/Build Windows` que genere un `.exe` jugable en `Builds/`.
- Escena `Boot` como primera en Build Settings.

---

## 18. Hitos (ejecutar en orden, sin detenerse)

1. **Base del proyecto:** estructura de carpetas, asmdefs, paquetes, `.gitignore`, ScriptableObjects base, `Boot` + `MainMenu` mínimos. Compila.
2. **Jugador:** locomoción (caminar/correr/saltar), cámara, estamina y vida, HUD básico y escena de pruebas. El personaje del jugador ya debe tener un modelo con aspecto real (del usuario o creado según 3.1), no una cápsula.
3. **Trepar y vault:** detección de bordes, animación/IK, `Climbable`, rejas con y sin techo.
4. **Combate:** puños, navaja, recoger/lanzar objetos, `IDamageable`, feedback.
5. **Enemigos:** celadores con percepción y estados, policía, peatones, sistema de alerta.
6. **TransMilenio:** buses con ciclo completo, spawner, puertas, atropello, condición de victoria al subir.
7. **Estación y rutas de entrada:** torniquetes, puertas, rejas, múltiples rutas. Primer nivel jugable completo.
8. **Niveles 2–10:** `LevelDefinition`, curva de dificultad, kit modular de estación, variantes (noche/lluvia), validador de niveles.
9. **UI completa:** menú, ajustes (audio/gráficos/controles/juego), pausa, game over, resultados, selección de nivel, guardado.
10. **Arte y modelos:** integrar los modelos del usuario (catálogo automático, escala, materiales URP, rigs) y crear/pulir todo lo que falte según 3.1. Revisión visual con capturas de cada modelo y nivel. No debe quedar ningún objeto con aspecto de relleno.
11. **Audio y pulido visual:** música dinámica, SFX, post-procesado, iluminación, VFX.
12. **Optimización y QA:** profiling, pooling, tests, corrección de bugs, build final de Windows.

---

## 19. Definición de terminado (checklist)

- [ ] El proyecto compila sin errores y sin advertencias críticas.
- [ ] Se puede jugar de **menú principal → nivel 1 → … → nivel 10 → final** sin bloqueos.
- [ ] Todas las mecánicas funcionan: caminar, correr, saltar, trepar/vault, escalar rejas, puños, navaja, recoger/lanzar, vida, estamina.
- [ ] Guardias y policía persiguen, atacan y pueden matar al jugador; el bus atropella y mata.
- [ ] Los buses llegan de forma constante, abren/cierran puertas y arrancan; subir al bus completa el nivel.
- [ ] Hay al menos 2–3 rutas de entrada por nivel y cada nivel es completable (validador pasa).
- [ ] Menú, HUD, pausa, ajustes (audio/gráficos/controles) y guardado funcionan y persisten.
- [ ] Usa los modelos del usuario cuando existen; todo lo que faltaba fue creado con calidad (sin cubos ni cápsulas como arte final) y el juego se ve bonito y coherente.
- [ ] Existe `Docs/ART_BIBLE.md` y el arte sigue esa dirección.
- [ ] Los modelos nuevos o reemplazados se detectan automáticamente desde el catálogo.
- [ ] Rendimiento objetivo cumplido y documentado.
- [ ] Documentación (`README`, `ARCHITECTURE`, `DECISIONS`, `CREDITS`, `PERFORMANCE`) actualizada.
- [ ] Build de Windows generado y probado.

---

## 20. Notas finales para el agente

- Si un requisito choca con otro o con una limitación técnica, **elige la opción más jugable y simple**, regístrala en `Docs/DECISIONS.md` y sigue adelante.
- Prioriza que el juego **se sienta bien**: controles responsivos, feedback claro, cámara estable. Eso importa más que la cantidad de contenido.
- Mantén todo **modular y extensible** para añadir niveles, enemigos y armas desde datos.
- **No te detengas** hasta completar los 12 hitos y la checklist de la sección 19.
