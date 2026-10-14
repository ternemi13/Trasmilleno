# Correcciones solicitadas

1. Locomoción reconstruida con IK de piernas, fases por distancia recorrida, apoyo de pies, giro de pelvis y torso y transición suave entre caminar/correr. Ataques con anticipación, golpe y recuperación. Preservación de bindposes de rigs originales; cierre de mano del jugador mediante blend shape.
2. Agrupación espacial por material del modelo original: 2264 → 183 renderers de estación; bus → 18 renderers. Se elimina la segunda estación de fondo. Simplificación de geometría de pistolas; el personaje secundario conserva su topología original. Windows 1080p, calidad media, RTX 4060 Ti / Ryzen 7 8700F: 3600 frames, 73,3 FPS promedio y 61,0 FPS mínimo observado. Sombras medias a 30 m y poses fuera de cámara omitidas.
3. Única estación jugable: Calle 45 del usuario. Escala uniforme medida con altura de torniquetes, suelo de andén y ancho de paso. Entrada original bajo letrero; rampas físicas originales. Dos carriles con circulación opuesta y paradas alineadas con puertas.
4. Armas sujetas a socket de palma independiente de la escala de huesos. Navaja alineada desde empuñadura; pistola de 34 cm, postura baja/encarada a dos manos y retroceso.
5. Causa reproducida: StarGraphic carecía de CanvasRenderer y GraphicRaycaster lanzaba MissingComponentException al apuntar en resultados. Se añade dependencia y se excluyen estrellas de raycast. Prueba ampliada: click de mouse real en cada botón Siguiente.
6. Los diez niveles usan el mismo modelo original, con aumento de celadores, policías, barreras y variantes de luz/clima.
7. Colliders de rejas/torniquetes medidos sobre el modelo original. Vault aterriza sobre la superficie del otro lado, con recogida de piernas bajo cubierta. Pruebas de agarrar y cruzar reja, saltar torniquete y navegación hasta ambos andenes.

Los meshes generados se actualizan vaciando primero buffers y copiando juntos vértices, índices, pesos y bindposes. Esto evita la corrupción nativa observada al usar CopySerialized. Los bajos del pantalón se asignan según conectividad de la superficie: los vértices que cruzan el eje central ya no se ligan a la pierna contraria. Pruebas de regresión para ambos fallos incluidas.
