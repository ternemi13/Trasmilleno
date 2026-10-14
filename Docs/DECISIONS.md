# Decisiones de implementación

- Se trabaja en el proyecto que está abierto en Unity, `D:/UNITY/Transmilleno/Transmilleno`, no en la carpeta web NIVEL2.
- Se conserva Unity 6000.6.5f1 instalado. El número 6000.5f1 del spec no coincide con la versión real. Package Manager resuelve las versiones compatibles; Cinemachine instalado es 6.6.0, con el namespace Unity.Cinemachine.
- CharacterController para locomoción determinista y control de escalada; NavMesh para agentes. Se usa uGUI/TMP.
- Modelos originales conservados en Assets/Modelos. Los ZIP suministrados contienen COLLADA: se extraen en las categorías correspondientes, sin modificar los originales. El catálogo detecta ambas ubicaciones.
- El remoto contenía únicamente el .gitignore oficial de Unity. Se integró su historial antes de añadir archivos y se configuró Git LFS.
- La campaña usa una escena Gameplay y diez LevelDefinition; todos los niveles usan la estación Calle 45 del usuario, con seguridad y ambiente progresivos. Los nombres de estaciones y señales son ambientación de ficción.
- Ninguna licencia de los modelos suministrados está incluida. Se registra su procedencia como material del usuario sin atribuirles una licencia inventada.
- Los modelos ya skinned conservan su esqueleto, pesos y bindposes originales; la animación procedural respeta sus ejes nativos. Se genera un Avatar Humanoid compatible; para modelos estáticos se genera un esqueleto Humanoid con pesos anatómicos aproximados. La locomoción usa IK de piernas y brazos, apoyos por distancia recorrida y transición temporal de ataques. Las manos usan sockets métricos de palma; el jugador incorpora un blend shape de cierre de dedos. No se descargaron animaciones ni se usó Mixamo.
- El modelo de la Sabana incluía un fondo de presentación de 36 vértices, de altura desproporcionada. Se elimina únicamente del prefab derivado, conservando el archivo fuente y la arquitectura de la estación.
- La cámara usa Cinemachine para salida y mezcla; la órbita y la colisión se resuelven por código para mantener el mismo comportamiento con mouse y mando.
- Los efectos y voces de audio usan bancos acotados de objetos reutilizables. Los objetos lanzables vuelven al punto de origen después de doce segundos para evitar acumulación fuera del recorrido.
- El audio usa mezcla por grupos de AudioSource con cuatro controles persistentes. La creación automática de un AudioMixer con parámetros expuestos no se completó mediante la API pública disponible; esta parte del spec queda como diferencia explícita, no como función verificada.
- Se eliminó por completo la estación procedural y la estación de fondo. Calle 45 se normaliza con escala uniforme, se alinea con los carriles y mantiene su entrada, torniquetes y arquitectura originales. Las barreras adicionales de los niveles avanzados se sitúan dentro del modelo.
- La prueba automatizada de campaña coloca al personaje en la zona de embarque para aislar carga, buses, victoria y persistencia. No sustituye un recorrido humano que evalúe dificultad, sigilo y todas las animaciones.
- Las pruebas de rendimiento del editor no certifican por sí solas 1080p; se incluye un modo `--benchmark` del ejecutable para medir la estación 10.


- La regresión de resultados incluye raycast de interfaz e inyección de mouse. Reprodujo la excepción CanvasRenderer ausente en StarGraphic; ya se corrigió y las estrellas no participan en el raycast.
- La medición de capacidad gráfica desactiva el límite FPS sólo durante --benchmark; no altera los ajustes guardados del jugador. El recorrido automático del nivel 10 mantiene población, persecución, lluvia y tráfico, con invulnerabilidad exclusiva del modo de verificación.

- Actualización de mallas derivadas por buffers explícitos en GeometryOptimizer.SaveMesh, sin CopySerialized sobre Mesh: se reprodujo corrupción de vértices/índices al regenerar. Los pesos de perneras estáticas respetan componentes conectados bajo rodilla, evitando estiramientos entre pies cuando el pantalón cruza el eje central.

- Calidad media concentra sombras en 30 m; los personajes fuera de cámara omiten resolución de poses hasta volver a ser visibles. La medición final tras optimizar dio 73,3 FPS promedio y 61,0 FPS mínimo en 3600 frames, con la geometría final conservada.

- Cámara con orientación independiente del desplazamiento: zoom de rueda acotado a 1,8–8 m, FOV constante y retícula centrada por anclas. El barrido de atropello usa carrocería completa; el cuerpo articulado se construye sólo al morir por bus y se muestra durante 2,6 segundos antes del reintento. Embarcar o morir detiene la regeneración de estamina.
