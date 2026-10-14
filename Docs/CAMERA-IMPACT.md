# Cámara, atropellos y estamina

- Rueda hacia arriba: acercar; hacia abajo: alejar. Distancia elegida entre 1,8 y 8 metros, con transición suave. La cámara se retrae al encontrar paredes, cubierta o carrocería y recupera la distancia elegida al quedar libre.
- Orientación controlada por mouse/mando y FOV constante; se eliminan el ruido aleatorio de daño y el zoom automático al correr. La mira usa geometría UI anclada exactamente al centro, independiente del tamaño de pantalla y de la tipografía.
- El detector anterior medía 16,6 × 2,7 m frente a los 17,8 × 2,8 m de la carrocería: faltaba cobertura en parachoques y costados. Ahora barre el volumen completo entre posiciones de cada frame y transmite velocidad de lanzamiento en el sentido del bus.
- El atropello desactiva movimiento y animación, activa once cuerpos rígidos y diez articulaciones sobre el modelo original, y lanza el cuerpo hacia arriba y hacia delante. Colisiones internas y con el bus causante se ignoran para evitar explosiones de física; el suelo y la arquitectura siguen teniendo colisión. La cámara sigue la pelvis durante 2,6 segundos antes del menú de muerte.
- No existe una recarga de estamina asociada al contacto. El fallo de detección dejaba al jugador vivo y seguía ejecutando su regeneración habitual. Un atropello válido ya detiene ese ciclo; también se mueve la actualización de estamina después de la comprobación de embarque para impedir regeneración mientras se está a bordo. Un bus detenido conserva la regeneración normal de descanso.

Pruebas específicas: rueda real mediante Input System, límites del zoom, orientación sin deriva, cámara frente a una pared, contacto frontal, contacto lateral inverso, impulso vertical/horizontal, cuerpo articulado con el modelo original, estamina junto al bus detenido y estamina al estar a bordo.

Las pruebas de embarque y escalada usan teclados virtuales propios y restauran la política de foco al terminar; no dependen de eventos del teclado físico ni del foco de la ventana del editor.

Validación final: 11/11 pruebas PlayMode y campaña Windows 10/10. Retícula: error de centrado 0 px en la revisión de integración. Rendimiento de Windows a 1080p: 73,5 FPS promedio, 62,7 FPS mínimo observado, 0/3600 frames sobre 16,67 ms. Compilación IL2CPP: cero errores.
