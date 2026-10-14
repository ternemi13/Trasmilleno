# Transmilleno

Juego de acción y sigilo urbano para Windows. Proyecto: https://github.com/ternemi13/Trasmilleno

## Abrir y jugar

1. Instalar Unity **6000.6.5f1** con Windows Build Support (IL2CPP), Visual Studio C++ y Windows SDK. Se utiliza la versión instalada; el identificador 6000.5f1 del spec original no es el del proyecto.
2. `git clone https://github.com/ternemi13/Trasmilleno.git`
3. Dentro del clon: `git lfs install` y `git lfs pull`.
4. Abrir la carpeta en Unity Hub y esperar a que termine la resolución de paquetes.
5. Abrir `Assets/_Project/Scenes/Boot.unity` y pulsar Play.
6. Menú → Nueva partida o Continuar. En cada nivel entra al andén de Calle 45 por una de las rutas, acércate a las puertas del bus parado y pulsa E. La victoria se confirma cuando el bus arranca.

El código, los assets derivados y la configuración se generan también mediante **Tools → Transmilleno → Build Everything**. Esta operación reconstruye escenas propias, catálogo, avatares y configuración; no elimina los modelos originales. No ejecutar durante Play Mode.

Para jugar sin el editor, abrir `Builds/Windows/Transmilleno.exe`. Mantener junto al ejecutable las DLL y la carpeta `Transmilleno_Data`; el ejecutable no funciona aislado. La carpeta de símbolos `Transmilleno_BackUpThisFolder_ButDontShipItWithYourGame` no forma parte de la distribución.

## Controles

| Acción | Control |
|---|---|
| Movimiento | WASD / stick izquierdo |
| Cámara | Mouse / stick derecho |
| Correr | Shift / L3 |
| Saltar, vault, enganchar reja | Espacio / botón sur |
| Desplazarse por la reja | WASD; Espacio para soltarse |
| Puños / lanzar objeto | Clic izquierdo / botón oeste |
| Navaja equipada | Clic derecho / RT |
| Recoger / subir al bus | E / botón norte |
| Movimiento lento | C / botón este |
| Pausa | Esc / Start |

La estamina se regenera tras un segundo sin consumo. No hay regeneración automática de vida. Los buses en movimiento causan daño letal. Las rejas cubiertas impiden el paso por arriba. Pelear aumenta la alerta; esconderse la reduce.

Los diez niveles mantienen la estación gris Calle 45 original, a escala humana, con entrada por el letrero y los torniquetes. Los buses paran en ambos costados y la seguridad aumenta entre niveles.

## Assets

Los archivos originales permanecen en `Assets/Modelos`. Los COLLADA de los ZIP del usuario se extrajeron en `Assets/_Project/Art/Models`. El catálogo procesa doce roles y genera prefabs con escala métrica, materiales compatibles y avatares. Los modelos nuevos se pueden poner en las carpetas Player, Pedestrians, Guards, TransMilenio, Station y Props. Los archivos directos de la categoría tienen prioridad sobre el original; los subdirectorios conservan las fuentes suministradas.

`Assets/_Project/Data/Catalogs/AssetCatalog.asset` es el punto de sustitución manual: asignar un prefab propio normalizado en `model`. El generador respeta referencias que no pertenecen a su carpeta `Prefabs/Models`. Una sustitución debe tener su pivote en el suelo y escala en metros.

## Datos, pruebas y compilación

- Diez niveles: `Assets/_Project/Data/Levels/`.
- Progreso: `Resources/Progression.asset`.
- Pruebas: Tools → Transmilleno → QA → Run EditMode Tests / Run PlayMode Tests.
- Validación de referencias: Validate Project.
- Capturas y rutas NavMesh: QA → Capture and Validate Levels.
- Build: Tools → Transmilleno → Build Windows, genera `Builds/Windows/Transmilleno.exe`.
- Las mediciones de rendimiento se escriben después de 3600 frames de juego, tras cinco segundos de calentamiento.

El ejecutable acepta `--smoke-test` para verificar automáticamente las diez transiciones de la campaña y `--benchmark` para medir la estación 10. En estos modos de QA se renderiza explícitamente a 1920 × 1080 y se lee la GPU cada frame, para poder verificar gráficos incluso en procesos ocultos. Esa lectura añade coste al benchmark y no se activa al jugar normalmente. Ambos modos generan informes en `Application.persistentDataPath`; el smoke test restaura el progreso original al terminar. En el editor, la prueba de campaña puede iniciarse añadiendo `CampaignSmoke` al GameApp y asignando su referencia App.

Guardado versionado en `Application.persistentDataPath/progress-v1.json`, con respaldo `.bak` y sustitución atómica. Ajustes y progreso se conservan entre sesiones. Los tests usan archivos temporales independientes.

## Git

Git LFS gestiona modelos, imágenes y audio. `Library`, `Temp`, `Logs`, `Builds` y archivos de solución generados no se versionan. Los commits de desarrollo usan identidad local `Codex <codex@users.noreply.github.com>` porque no había identidad configurada. No se modificó la configuración global.

Para publicar futuros commits: `git push origin main`. No se necesitan tokens dentro de los archivos del juego.

Consulta `QA-FINAL.md` para el alcance real de la validación y las diferencias pendientes respecto al spec.

Consulta `ARCHITECTURE.md`, `ART_BIBLE.md`, `DECISIONS.md`, `CREDITS.md` y los informes de QA para los detalles y límites comprobados.
