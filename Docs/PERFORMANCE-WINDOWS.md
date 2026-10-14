# Performance measurement

2026-10-14 03:42:32Z UTC

- Environment: Windows player
- GPU: NVIDIA GeForce RTX 4060 Ti
- CPU: AMD Ryzen 7 8700F 8-Core Processor 
- Resolution: 1920 × 1080
- Level: 10
- Frames: 3600
- Mean: 13,61 ms (73,5 FPS)
- P95: 14,54 ms
- Slowest frame: 15,95 ms (62,7 FPS)
- P99: 15,01 ms
- Frames over 16.67 ms: 0 / 3600
- Memory: 474 MiB

Editor samples include editor overhead. Hidden Windows QA renders the full scene and UI at 1920 x 1080 into a RenderTexture and performs a synchronous GPU readback each frame; this adds capture overhead. Measurements certify only this machine and route.

Camera/impact update: Medium quality, render scale 1.0, 2x MSAA, shadow distance 30 m. Level-10 traversal with guards, rain and both buses. New stable camera at default zoom.
