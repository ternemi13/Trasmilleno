# Performance measurement

2026-10-14 02:52:07Z UTC

- Environment: Windows player
- GPU: NVIDIA GeForce RTX 4060 Ti
- CPU: AMD Ryzen 7 8700F 8-Core Processor
- Resolution: 1920 × 1080
- Level: 10
- Frames: 3600
- Mean: 13,64 ms (73,3 FPS)
- P95: 14,55 ms
- Slowest frame: 16,38 ms (61,0 FPS)
- P99: 14,98 ms
- Frames over 16.67 ms: 0 / 3600
- Memory: 480 MiB

Editor samples include editor overhead. Hidden Windows QA renders the full scene and UI at 1920 x 1080 into a RenderTexture and performs a synchronous GPU readback each frame; this adds capture overhead. Measurements certify only this machine and route.

Final correction build: Medium quality, render scale 1.0, 2x MSAA, shadow distance 30 m. Continuous level-10 tour from z=6 to z=31 and back, rain, guards, attacks and both buses active. This finite sample measures this hardware and route.
