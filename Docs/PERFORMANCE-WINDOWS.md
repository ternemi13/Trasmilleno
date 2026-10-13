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
