# Validación de la entrega — 13 de octubre de 2026

## Resultado comprobado

- Windows x64 IL2CPP: compilación Succeeded, cero errores (BUILD-RESULT.txt).
- EditMode: 7/7; PlayMode: 3/3. Informes XML con fecha UTC 2026-10-13 05:06.
- Ejecutable: carga de los diez niveles, espera del bus, puertas abiertas, embarque, salida, resultados, guardado y final: PASS (WINDOWS-SMOKE.md).
- NavMesh: tres conexiones desde zonas de aterrizaje hasta embarque por cada nivel (ROUTE-VALIDATION.md).
- Modelos suministrados integrados; revisión visual de materiales, escala y rigs. Los rigs skinned originales conservan huesos, pesos y bindposes. Los modelos estáticos usan rig generado.
- Capturas reales del ejecutable a 1920 × 1080: Captures/windows-gameplay.png y Captures/windows-finale.png.
- Rendimiento en RTX 4060 Ti / Ryzen 7 8700F: promedio 59,3 FPS; P95 18,11 ms; 700 MiB asignados por Unity. Ver PERFORMANCE.md para condiciones y límites.

## Límites y diferencias respecto al spec

- La prueba de campaña sitúa al jugador en embarque: no certifica balance de dificultad ni un recorrido humano completo de las tres entradas. La comprobación NavMesh no prueba las pulsaciones de escalada.
- El audio usa grupos de AudioSource y controles persistentes; no se generó un AudioMixer con parámetros expuestos. La sesión de QA no dispuso de salida física de audio (FMOD usó salida emulada), por lo que no se certifica escucha ni mezcla subjetiva.
- El benchmark incluye una lectura síncrona de la GPU y sólo mide la vista inicial del nivel 10 en esta máquina. No certifica todavía 60 FPS estables en todo el recorrido ni en el hardware mínimo.
- El player informó shaders eliminados de Depth of Field y Panini; son efectos no utilizados por el diseño actual. No se detectaron excepciones de gameplay en los logs del smoke test o benchmark.
- La campaña utiliza diez variantes de un kit modular, con noche, lluvia, kioscos, taquillas y pasarelas. No son diez reproducciones geográficas completas de Bogotá.
- La validación automática y las capturas no equivalen a certificación de publicación: quedan la revisión humana de balance, accesibilidad, animaciones, audio y licencias de los modelos aportados.

## Entrega local

Abrir Builds/Windows/Transmilleno.exe. Conservar sus DLL y Transmilleno_Data. Builds/Transmilleno-Windows-x64.zip contiene los archivos necesarios y excluye símbolos internos y logs. Builds se excluye de Git; el código fuente, assets, configuración e informes sí se versionan.
