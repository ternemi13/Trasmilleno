# Performance measurement

2026-10-13 05:07:33Z UTC

- Environment: Windows player
- GPU: NVIDIA GeForce RTX 4060 Ti
- CPU: AMD Ryzen 7 8700F 8-Core Processor 
- Resolution: 1920 × 1080
- Level: 10
- Frames: 1200
- Mean: 16,87 ms (59,3 FPS)
- P95: 18,11 ms
- Memory: 700 MiB

Editor samples include editor overhead. Hidden Windows QA renders the full scene and UI at 1920 x 1080 into a RenderTexture and performs a synchronous GPU readback each frame; this adds capture overhead. Measurements certify only this machine and route.

## Alcance

Calidad Media (perfil por defecto). Cámara en el punto inicial de Portal Norte, con población, buses y lluvia activos. No representa un recorrido de combate completo ni una prueba en todas las GPU de gama media. El promedio próximo a 60 FPS no demuestra 60 FPS estables: P95 18,11 ms. La lectura síncrona GPU de la herramienta añade coste.
