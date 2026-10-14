# Validación de las correcciones — 13 de octubre de 2026 (Bogotá)

## Resultado comprobado

- Windows x64 IL2CPP: compilación Succeeded, cero errores (BUILD-RESULT.txt).
- EditMode: 9/9; PlayMode: 11/11. Informes XML con fecha UTC 2026-10-14, correspondiente a la noche local del 13 de octubre.
- Ejecutable: diez niveles completos y nueve clics de mouse en Siguiente, con comprobación de raycast, guardado y final: PASS (WINDOWS-SMOKE.md). La automatización utiliza dispositivos virtuales aislados para no depender del foco de una ventana oculta y restaura la partida guardada.
- Escalada: pruebas sobre la reja de acceso original y salto de los torniquetes; el personaje termina del otro lado. Ambos buses tienen hojas de puerta originales y la parada sur se comprueba con su dirección invertida.
- NavMesh: tres conexiones desde zonas de aterrizaje hasta embarque por cada uno de los diez niveles (ROUTE-VALIDATION.md).
- Calle 45 original es la única estación jugable; escala uniforme medida, entrada y rampas originales. Seguridad creciente en los diez niveles.
- Locomoción: IK, transición de velocidad, apoyo alternado, torso y pelvis. Revisadas cuatro fases de carrera, preparación/golpe/recuperación y agarres de navaja y pistola. Pruebas de regresión de buffers de malla y pesos cruzados entre piernas.
- Capturas finales: Captures/level-01.png a level-10.png, stride-0.png a stride-3.png, pose-final-1.png y pose-final-2.png; ventanas de Windows en windows-gameplay.png y windows-finale.png.

- Rendimiento Windows: 73,5 FPS de promedio; P95 14,54 ms; P99 15,01 ms; fotograma más lento 15,95 ms (62,7 FPS). 3600 muestras, 1080p, calidad media, RTX 4060 Ti / Ryzen 7 8700F. Memoria Unity: 474 MiB.

## Alcance de la validación

- La campaña automatizada coloca al jugador en embarque: certifica carga, interacción, botones, salida y progreso; no sustituye una evaluación humana del balance de todos los recorridos.
- El benchmark recorre la zona jugable del nivel 10 entre z=6 y z=31, con lluvia, persecución, ataques y ambos buses activos. Lee la GPU de forma síncrona a 1080p; ese coste adicional sólo existe en QA. Ver PERFORMANCE-WINDOWS.md para valores de esta máquina y ruta; no es una garantía para todos los equipos.
- Los rigs originales conservan sus huesos y pesos. Los personajes que llegaron como geometría estática usan skinning e IK generados; no se incorporaron animaciones capturadas.
- El audio sigue usando grupos de AudioSource con controles persistentes; el AudioMixer con parámetros expuestos del spec inicial sigue siendo una diferencia documentada. La salida emulada de audio en QA no certifica mezcla subjetiva.
- El giro de guardias ignora vectores de longitud cero al coincidir con el jugador; la campaña final revisa que no reaparezca el aviso Look rotation.
- Los avisos de compilación de shaders de paquetes no utilizados no son errores de compilación. Los logs del smoke y benchmark se revisan por excepciones de gameplay.

## Entrega local

Abrir Builds/Windows/Transmilleno.exe. Conservar sus DLL y Transmilleno_Data. Builds/Transmilleno-Windows-x64.zip incluye la distribución y LEEME.md, sin símbolos internos ni logs. El código, assets, configuración e informes se versionan; Builds queda fuera de Git.

## Ampliación: cámara y atropello

Zoom con rueda de 1,8 a 8 m, retícula centrada (error medido: 0 píxeles), orientación estable y colisión de cámara comprobados. Cinco pruebas adicionales cubren zoom, parachoques frontal, roce lateral en sentido inverso, regeneración junto al bus parado/embarque y ragdoll con el modelo original. El control de integración confirma lanzamiento, giro del cuerpo y aparición del menú de muerte después de 2,6 segundos, manteniendo la estamina en 30. Ver CAMERA-IMPACT.md y CAMERA-IMPACT-SMOKE.md.
