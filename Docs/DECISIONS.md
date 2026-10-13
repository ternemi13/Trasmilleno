# Decisiones de implementación

- Se trabaja en el proyecto que está abierto en Unity, `D:/UNITY/Transmilleno/Transmilleno`, no en la carpeta web NIVEL2.
- Se conserva Unity 6000.6.5f1 instalado. El número 6000.5f1 del spec no coincide con la versión real. Package Manager resuelve las versiones compatibles; Cinemachine instalado es 6.6.0, con el namespace Unity.Cinemachine.
- CharacterController para locomoción determinista y control de escalada; NavMesh para agentes. Se usa uGUI/TMP.
- Modelos originales conservados en Assets/Modelos. Los ZIP suministrados contienen COLLADA: se extraen en las categorías correspondientes, sin modificar los originales. El catálogo detecta ambas ubicaciones.
- El remoto contenía únicamente el .gitignore oficial de Unity. Se integró su historial antes de añadir archivos y se configuró Git LFS.
- La campaña usa una escena Gameplay y diez LevelDefinition; las variantes se construyen con un kit modular reproducible. Los nombres de estaciones y señales son ambientación de ficción.
- Ninguna licencia de los modelos suministrados está incluida. Se registra su procedencia como material del usuario sin atribuirles una licencia inventada.
- Los modelos ya skinned conservan su esqueleto, pesos y bindposes originales; la animación procedural respeta sus ejes nativos. Se genera un Avatar Humanoid compatible; para modelos estáticos se genera un esqueleto Humanoid con pesos anatómicos aproximados. Las manos usan IK de dos huesos durante el agarre. No se descargaron animaciones ni se usó Mixamo.
- El modelo de la Sabana incluía un fondo de presentación de 36 vértices, de altura desproporcionada. Se elimina únicamente del prefab derivado, conservando el archivo fuente y la arquitectura de la estación.
- La cámara usa Cinemachine para salida y mezcla; la órbita y la colisión se resuelven por código para mantener el mismo comportamiento con mouse y mando.
- Los efectos y voces de audio usan bancos acotados de objetos reutilizables. Los objetos lanzables vuelven al punto de origen después de doce segundos para evitar acumulación fuera del recorrido.
- El audio usa mezcla por grupos de AudioSource con cuatro controles persistentes. La creación automática de un AudioMixer con parámetros expuestos no se completó mediante la API pública disponible; esta parte del spec queda como diferencia explícita, no como función verificada.
- Los diez niveles combinan estaciones originales con módulos de circulación: kioscos, taquillas, pasarelas elevadas y barreras. El recorrido principal permite comprobar las tres entradas en todas las variantes; no se generaron diez ciudades independientes.
- La prueba automatizada de campaña coloca al personaje en la zona de embarque para aislar carga, buses, victoria y persistencia. No sustituye un recorrido humano que evalúe dificultad, sigilo y todas las animaciones.
- Las pruebas de rendimiento del editor no certifican por sí solas 1080p; se incluye un modo `--benchmark` del ejecutable para medir la estación 10.

