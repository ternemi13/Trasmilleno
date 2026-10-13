# Decisiones de implementación

- Se trabaja en el proyecto que está abierto en Unity, `D:/UNITY/Transmilleno/Transmilleno`, no en la carpeta web NIVEL2.
- Se conserva Unity 6000.6.5f1 instalado. El número 6000.5f1 del spec no coincide con la versión real. Package Manager resuelve las versiones compatibles; Cinemachine instalado es 6.6.0, con el namespace Unity.Cinemachine.
- CharacterController para locomoción determinista y control de escalada; NavMesh para agentes. Se usa uGUI/TMP.
- Modelos originales conservados en Assets/Modelos. Los ZIP suministrados contienen COLLADA: se extraen en las categorías correspondientes, sin modificar los originales. El catálogo detecta ambas ubicaciones.
- El remoto contenía únicamente el .gitignore oficial de Unity. Se integró su historial antes de añadir archivos y se configuró Git LFS.
- La campaña usa una escena Gameplay y diez LevelDefinition; las variantes se construyen con un kit modular reproducible. Los nombres de estaciones y señales son ambientación de ficción.
- Ninguna licencia de los modelos suministrados está incluida. Se registra su procedencia como material del usuario sin atribuirles una licencia inventada.

