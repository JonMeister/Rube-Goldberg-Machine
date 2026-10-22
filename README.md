# Máquina de Rube Goldberg - Simulación de Físicas en Unity

**Autor:** Jonathan Aristizábal  
**Materia:** Desarrollo de Videojuegos (Semestre VIII)  
**Motor:** Unity 6 (`6000.6.0f1`)  

---

## 📌 Descripción del Proyecto

Este proyecto consiste en el diseño, modelado e implementación de una **Máquina de Rube Goldberg** completamente interactiva y funcional, desarrollada en **Unity 6**. Una máquina de Rube Goldberg es un sistema diseñado deliberadamente para realizar una tarea sencilla mediante una compleja reacción en cadena de eventos sucesivos, gobernada estrictamente por las leyes de la física newtoniana (gravedad, conservación del momento, torque, fricción y transferencia de energía cinética).

El objetivo final de la máquina es desencadenar una secuencia ininterrumpida de colisiones mecánicas que culmine con el derribo y colapso de una **pirámide de bloques objetivo** ubicada al final del escenario.

---

## 🏛️ Ambientación y Estilo Visual

La escena se desarrolla en un entorno tridimensional cerrado y estilizado:
- **Paredes de piedra rústica:** Texturas de mampostería medieval que enmarcan el recorrido.
- **Piso de mármol pulido:** Superficie reflectante y limpia con material propio (`floor_material`).
- **Iluminación cinemática:** Luz direccional configurada para proyectar sombras suaves sobre cada plataforma, rampa y engranaje.
- **Banda sonora ambiental:** Música integrada en el proyecto (`game_music.mp3`) que acompaña la ejecución.

---

## ⚙️ ¿En qué consiste la Escena? (Fases de la Reacción en Cadena)

La máquina está dividida en **5 fases mecánicas interconectadas**:

```
[Bola 1 (Amarilla)] 
       ↓ (Rampa de inicio con rieles)
[Laberinto de Clavos / Pines] 
       ↓ (Colisiones sucesivas tipo Plinko)
[Bola 2 (Azul)] 
       ↓ (Empuje y transferencia de momento)
[Línea de Dominós] 
       ↓ (Efecto dominó secuencial sobre plataforma)
[Gatillo y Martillo Péndulo] 
       ↓ (Liberación y caída por gravedad)
[Bola 3 (Roja)] 
       ↓ (Golpe de impacto por la cabeza del martillo)
[Rampa Final] 
       ↓ (Descenso acelerado)
[Pirámide de Bloques Meta] 💥 (Colapso total de la estructura)
```

### 1. Fase 1: El Despegue (Bola 1 - Amarilla)
- La simulación inicia en una plataforma elevada donde reposa la primera esfera (`Ball_1_Starter`), de color amarillo/dorado.
- La bola rueda a lo largo de una rampa de madera inclinada a 24° provista de rieles laterales que evitan descarrilamientos por inercia centrífuga.

### 2. Fase 2: El Laberinto de Pines y la Segunda Bola
- La bola 1 ingresa a una sección intermedia equipada con una matriz de clavos/pines verticales de acero (estilo *Plinko* o *Pachinko*).
- Los sucesivos rebotes dosifican la energía de la esfera hasta dirigirla directamente contra la segunda esfera (`Ball_2_Secondary`), de color azul, transfiriéndole su impulso lineal.

### 3. Fase 3: La Reacción en Cadena (Fichas de Dominó)
- La bola azul avanza hacia una plataforma central donde impacta la primera ficha de una hilera de dominós colocados verticalmente (`Dominoes_Set_1` / `Dominoes_Set_2`).
- Se desencadena el clásico **efecto dominó**, donde el torque gravitatorio de cada pieza derriba sucesivamente a la siguiente con gran precisión.

### 4. Fase 4: El Martillo Péndulo
- La última pieza de la cadena de dominós impacta contra el gatillo (`Hammer_Latch`) que sostiene un martillo pesado suspendido (`Hammer_Pendulum`).
- Al liberarse el soporte, el martillo gira libremente sobre su eje gracias a una articulación tipo bisagra (`HingeJoint`), transformando su energía potencial gravitatoria en un contundente golpe de energía cinética.

### 5. Fase 5: Impacto y Derribo Final (Bola 3 - Roja y Bloques)
- La cabeza metálica del martillo impacta con fuerza contra la tercera bola (`Ball_3_Final`), de color rojo, que descansa sobre su pedestal.
- La bola 3 sale impulsada a gran velocidad por la `Final_Ramp` hacia la base del escenario.
- **Meta Final:** La esfera colisiona de lleno contra una pirámide de bloques objetivo (`Final_Target_Blocks`: 3 en la base, 2 en el nivel medio y 1 en la cúspide), provocando la dispersión y el colapso total de la estructura.

---

## 🛠️ Arquitectura Técnica y Físicas (Unity PhysX)

| Componente | Configuración Técnica | Función en la Máquina |
| :--- | :--- | :--- |
| **Rigidbodies** | Masa calibrada y detección `ContinuousDynamic` | Evita el efecto de *tunneling* (que las bolas atraviesen paredes a alta velocidad). |
| **SmoothPhysics** | Fricción mínima (`0.05`), bounciness medio (`0.25`) | Proporciona un rodamiento fluido y continuo a las esferas y rampas. |
| **DominoPhysics** | Fricción estática/dinámica (`0.50` / `0.45`) | Garantiza estabilidad antes del choque y una caída firme sin deslizamientos indeseados. |
| **HingeJoint** | Articulación con eje Z y límites angulares | Modela el movimiento pendular del martillo y los balancines. |
| **CameraFollowMachine** | `SmoothDamp` y seguimiento multi-objetivo | Cámara cinematográfica que sigue dinámicamente a cada bola conforme se activan en la secuencia. |
| **GoldbergCollisionTracker** | Proxies de colisión con detección de pares únicos | Registra en consola cada choque relevante con marcas de tiempo y conteo de colisiones. |

---

## 📂 Estructura del Repositorio

```plaintext
Rube-Goldberg-Machine/
├── Assets/
│   ├── Main_Scene.unity             # Escena principal con todo el montaje de la máquina
│   ├── My objects/
│   │   ├── My images/               # Texturas auxiliares
│   │   ├── My materials/            # Materiales (mármol, piedra, esferas, dominós, madera)
│   │   └── My music/
│   │       └── game_music.mp3       # Pista de audio ambiental
│   └── Scripts/
│       ├── CameraFollowMachine.cs   # Controlador cinemático inteligente de la cámara
│       ├── GoldbergCollisionTracker.cs # Rastreador y logger de colisiones
│       ├── GoldbergCollisionProxy.cs   # Puente de eventos de colisión
│       └── PhysicsBootstrap.cs      # Generador y utilidades del montaje
├── Packages/                        # Configuración de dependencias UPM
├── ProjectSettings/                 # Parámetros del motor y físicas
└── README.md                        # Documentación oficial del proyecto
```

---

## 🚀 Instrucciones de Ejecución

1. Clonar o descargar este repositorio:
   ```bash
   git clone https://github.com/JonMeister/Rube-Goldberg-Machine.git
   ```
2. Abrir **Unity Hub** y añadir la carpeta del proyecto.
3. Abrir el proyecto usando **Unity 6** (recomendado versión `6000.0.0f1` o compatible).
4. En la ventana **Project**, ir a `Assets/` y abrir la escena **`Main_Scene.unity`**.
5. Presionar el botón **Play** (▶) en la parte superior del editor.
6. Disfrutar de la reacción en cadena completa con seguimiento automático de cámara.
