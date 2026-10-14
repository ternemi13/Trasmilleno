# Arquitectura

## Dependencias

- **Core**: datos, Actor/IDamageable, eventos, estamina, guardado, entradas, catálogo, animación y módulos visuales. No depende de gameplay concreto.
- **Player** → Core: CharacterController, escalada, combate e interacción; OrbitCamera controla una CinemachineCamera con colisión por SphereCast.
- **AI** → Core: NavMeshAgent, percepción escalonada, estados explícitos, ruido y reacciones.
- **Transit** → Core + Player: spline, tiempos de parada, puerta, embarque y colisión letal.
- **UI** → módulos anteriores: GameApp coordina carga asíncrona y ciclo de campaña, LevelWorld ensambla las estaciones, GameUI construye uGUI/TMP. Audio y mediciones tienen componentes propios.
- **Editor** → runtime: construcción reproducible, normalización, rigging y validación. Nunca está incluido en el jugador final.
- **Tests**: assemblies separados para EditMode y PlayMode.

Boot crea un GameApp persistente. MainMenu y Gameplay se cargan asíncronamente y sus objetos pertenecen a la escena, por lo que se limpian al cambiar de estación. El canvas y el servicio de audio persisten. GameEvents comunica eventos; Session concentra únicamente el estado de la partida activa y referencias al jugador y al guardado.

## Estado y persistencia

PlayerController: Grounded / Airborne / Vaulting / Fence / Boarded / Dead. GuardAgent: Patrol / Suspicious / Chase / Attack / Search / Stunned / Flee / Dead. BusController: Waiting / Arriving / Braking / Opening / Boarding / Closing / Departing.

La victoria requiere acceso al andén, un bus con puertas abiertas, proximidad a la puerta y la acción Interactuar. La finalización se emite al terminar el cierre de puertas. Resultados guarda estrellas, mejor tiempo y desbloqueos antes de mostrar las opciones de continuación.

## Arte y unidades

CatalogBuilder genera prefabs normalizados sin cambiar los modelos fuente. RigBuilder crea un Avatar Humanoid, conserva pesos originales cuando existen y aproxima pesos anatómicos para mallas estáticas. PoseDriver anima articulaciones por fase de locomoción y estado de acción. Los materiales originales glTF se conservan; los materiales Built-in se convierten a URP.

La geometría adicional usa mallas biseladas y piezas cilíndricas, materiales compartidos y texturas de desgaste. La colisión del entorno está en capa 0; jugador 8, personajes 9 y buses 10. Los casts de cámara y escalada excluyen actores. NavMesh usa colliders de arquitectura.

## Verificación

Los tests de lógica comprueban agotamiento, recuperación, persistencia y corrupción, campaña y catálogo. Los de PlayMode comprueban vault, daño letal y requisitos de embarque. La validación de rutas comprueba continuidad NavMesh desde el aterrizaje de cada acceso hasta la parada; por sí sola no demuestra que la animación de cada acceso sea ejecutable. Las capturas y pruebas de interacción complementan esa comprobación.

## Correcciones de estación e interacción

StationLayout concentra las coordenadas métricas de entrada, límites del andén y puertas. GeometryOptimizer agrupa mallas por material y tramo de 20 metros, manteniendo hojas de puerta independientes. LevelWorld añade colisiones medidas sobre la estación original y usa sus mallas de piso para rampas. Los buses comparten ciclo de servicio, con trayectorias y orientación invertidas; ambos permiten embarcar.

GameUI desactiva inmediatamente la pantalla anterior al reconstruirla. StarGraphic requiere CanvasRenderer y no recibe raycasts. CampaignSmoke prueba la navegación de resultados mediante eventos reales de mouse.

OrbitCamera recibe una acción Zoom del Input System y mantiene orientación/FOV independientes de la posición suavizada. La colisión retrae la distancia elegida, sin cambiarla. PlayerRagdoll construye el cuerpo físico sólo al morir por bus; GameApp permite 2,6 segundos de simulación antes de congelar la escena y presentar el reintento. El barrido de BusController cubre carrocería completa y recorrido entre frames.
