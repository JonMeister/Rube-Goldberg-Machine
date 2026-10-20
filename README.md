# Rube-Goldberg-Machine

Proyecto base en **Unity 6.6** para empezar una máquina de Rube Goldberg y validar colisiones/físicas.

## Requisitos
- Unity Editor **6.6** (proyecto configurado con `6000.6.0f1`)

## Qué incluye esta base
- Estructura mínima de proyecto Unity (`Assets`, `ProjectSettings`, `PackageManager`)
- Script `PhysicsBootstrap` que al iniciar Play crea automáticamente:
  - Piso
  - Rampa
  - Esfera inicial con `Rigidbody`
  - Bloque objetivo con `Rigidbody`
  - Línea de dominós para observar cadena de colisiones

## Cómo usarlo
1. Abrir la carpeta del repositorio en Unity Hub.
2. Confirmar apertura con Unity 6.6.
3. Ejecutar **Play** en una escena vacía.
4. Observar la interacción de físicas/colisiones del montaje inicial.

Desde esta base puedes reemplazar o ampliar piezas para construir una máquina de Goldberg más compleja.
