# Performance measurement

2026-10-13 04:56:35Z UTC

- Environment: Unity Editor
- GPU: NVIDIA GeForce RTX 4060 Ti
- CPU: AMD Ryzen 7 8700F 8-Core Processor 
- Resolution: 1469 × 719
- Level: 1
- Frames: 1200
- Mean: 16,84 ms (59,4 FPS)
- P95: 23,13 ms
- Memory: 1733 MiB

Editor samples include editor overhead. Hidden Windows QA renders the full scene and UI at 1920 x 1080 into a RenderTexture and performs a synchronous GPU readback each frame; this adds capture overhead. Measurements certify only this machine and route.
