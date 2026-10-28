# Documento Técnico — Run & Gun 2D

> **Estado:** borrador v40 (2026-10-28)
> **Motor:** Unity 6 · **Plataforma:** PC (teclado y gamepad) · **Arte:** pixel art hecho en Piskel
> **Referencias:** Metal Slug (personaje dividido en dos mitades), Mega Man (habilidades), Mega Man X (dash)
> Las secciones marcadas con **[DECIDIR]** son decisiones abiertas y las marcadas con **[AJUSTAR]** son valores que se afinan al probar. Todas están en la sección 17.

---

## 1. Visión general

Juego de acción 2D de scroll lateral para PC, con **10 niveles que se juegan en orden**. Antes de cada nivel, el jugador **elige entre los personajes que tenga disponibles (5 en total)**, y cada uno tiene sus propias estadísticas, su disparo básico, sus habilidades y, algunos, una pasiva. Todos corren, saltan, se agachan, hacen dash y disparan en varias direcciones al mismo tiempo. Las habilidades especiales se usan con un botón propio y comparten una sola barra de energía. En algunos niveles se pueden pilotear **vehículos** al estilo Metal Slug (sección 18).

### 1.1 Mecánicas del jugador

Todos los personajes comparten estas mecánicas de movimiento. Lo que cambia entre ellos son los **valores** (velocidad, salto, dash, vida, energía), el **disparo básico** y las **habilidades** (ver 5.4). Los valores de esta tabla son los de Rosslyn.

| Mecánica | Resumen |
|---|---|
| Correr | Movimiento horizontal en el suelo |
| Saltar | Altura variable, coyote time y jump buffer |
| Agacharse | Reduce el collider; permite disparar agachado |
| Dash | En el suelo y en el aire (uno por salto). El dash jump conserva el impulso. Sin invulnerabilidad. **No se puede disparar durante el dash** |
| Disparo básico | Un disparo por pulsación, sin carga y sin límite de balas en pantalla. Hacia adelante, en diagonal arriba-adelante, hacia arriba y hacia abajo (solo en el aire) |
| Vida | 28 puntos (cada personaje define la suya), vidas infinitas, reaparece en el último checkpoint |
| Personajes | **5 personajes jugables**: Rosslyn, Veyron, Aquamarine, Agente X y Neotisleo (el General). Se eligen en una pantalla antes de cada nivel (15.1). Cada uno tiene sus estadísticas, su disparo básico, 3 habilidades y, algunos, una pasiva (5.4). Rosslyn y Veyron están desde el inicio; Aquamarine se une después del Bosque en llamas y Agente X después del Tren militar; Neotisleo se une antes de la misión final si se **rescató a todos los prisioneros** (15.2) |
| Progresión | **10 niveles en orden** (15.1). El primero es el **Simulador de combate** (Misión 0), con Rosslyn. Los niveles 2 a 10 tienen un jefe cada uno; el último es Coliel en la Torre alienígena. Los niveles completados se pueden repetir. Las habilidades **no las dan los jefes**: se encuentran como **coleccionables escondidos en los niveles** (8.9) |
| Historia | Un escuadrón cumple las misiones que le da el General Neotisleo, contra una amenaza alienígena cuyo líder, Coliel, es el jefe final (ver 1.4). Cada nivel tiene escenas cortas al inicio, antes del jefe y al final, las mismas sin importar el personaje elegido |
| Habilidades | Cada personaje tiene **3 habilidades propias** y su propio progreso de desbloqueo (8.2). Se obtienen como coleccionables en el escenario; el taladro (Veyron) y el lanzallamas (Rosslyn) abren caminos opcionales, lo que invita a rejugar cada nivel con distintos personajes (8.9). Se cambian con botones de siguiente y anterior. Todas las de un personaje gastan de **una sola barra de energía compartida**, ya sea por uso o por segundo (las sostenidas). La habilidad seleccionada solo se muestra con un icono en el HUD |

### 1.2 Pilares técnicos

1. **Personaje dividido en dos mitades** (piernas y torso), cada una con su propio Animator, para combinar cualquier movimiento con cualquier dirección de disparo.
2. **Dos máquinas de estados en paralelo** (locomoción y acción) que reflejan esa división.
3. **La lógica manda; el Animator solo muestra.** El estado de gameplay nunca vive dentro del Animator.
4. **Contenido definido por datos** (ScriptableObjects) para los personajes, el disparo, las habilidades, los enemigos y los pickups.
5. **Pixel perfect**: sin subpíxeles, sin blur, sin temblores.

### 1.3 Fuera de alcance (por ahora)

Disparo cargado **para los personajes** (el Exoesqueleto sí lo tiene, 18.3), armas que se recogen, wall jump, cambio de color del personaje por habilidad, cooperativo.

**Planeado para la versión 2** (sección 19): más niveles.

### 1.4 Historia

Un **escuadrón** recibe misiones del **General Neotisleo** para enfrentar una **amenaza alienígena**. El recorrido va de un simulador de entrenamiento a un bosque, unas minas y una montaña, pasa por un bosque en llamas, un laboratorio, un tren militar y un castillo, y termina en **Coleópolis**, la ciudad alienígena, y su **torre**, donde espera **Coliel**, el jefe final.

**La misión final:** el comando quiere **bombardear Coleópolis** para detener a Coliel. Si el jugador rescató a **todos los prisioneros**, estos convencen a Neotisleo de pelear junto a Rosslyn para detener a Coliel **sin destruir la ciudad**, y Neotisleo se vuelve jugable. Hay **un solo final**: los prisioneros agregan a Neotisleo y su escena, pero no cambian el desenlace.

El resto del detalle de la historia llega con el guion **[DECIDIR]**.

| Personaje | Papel |
|---|---|
| Rosslyn | Soldado espacial, protagonista y personaje jugable desde el inicio |
| Veyron | Compañero de escuadrón, jugable desde el inicio |
| Aquamarine | Se une al escuadrón después del Bosque en llamas (nivel 5); jugable desde el Laboratorio |
| Agente X | Se une al escuadrón después del Tren militar (nivel 7); jugable desde el Castillo |
| Goldo | Otro general. En la misión final, si se juega con Neotisleo, da las órdenes por radio en su lugar (15.7) |
| Neotisleo (el General) | Da los briefings y habla en las escenas. Si se rescató a todos los prisioneros, estos lo convencen de pelear junto a Rosslyn en la misión final y se vuelve jugable (15.2) |
| Prisioneros | 3 escondidos en cada nivel (15.2) |
| Jefes | Uno por nivel, del 2 al 10 (14.2) |
| Coliel | Jefe final, en la Torre alienígena |

**El escuadrón va a cada misión.** Dentro del lore, los miembros disponibles participan aunque el jugador solo controle a uno. Por eso **las escenas y los diálogos son los mismos** sin importar el personaje elegido: el texto nunca supone cuál se está jugando (15.7).

**Excepción: el Simulador (Misión 0).** Es un entrenamiento de Rosslyn y solo ella participa. Es lo primero que ve el jugador y sirve como misión de introducción, al estilo de la primera misión de Mega Man X (15.1).

**Cómo se cuenta con el sistema de escenas y diálogos (15.7):** los diálogos son escenas de **novela visual**, al estilo de *Sonic Battle*: una ilustración de la situación en el centro, hasta 4 personajes a los lados con retratos por emoción y el cuadro de texto abajo.
- **Inicio de nivel:** el General describe el objetivo y el escuadrón responde.
- **Sala del jefe:** diálogo corto con el jefe antes de la pelea (14.2).
- **Cierre de nivel:** el General comenta el resultado y da paso al siguiente nivel.
- **Incorporaciones:** las escenas de cierre presentan a Aquamarine y a Agente X cuando se unen.

**Estilo visual:** ciencia ficción con escenarios naturales y militares (bosque, minas, montaña, laboratorio, tren, castillo) que terminan en una ciudad alienígena. El disparo básico de Rosslyn es un bláster.

---

## 2. Stack técnico

| Área | Elección | Notas |
|---|---|---|
| Versión de Unity | Unity 6 (6000.x LTS) | Usar la misma versión menor en todo el proyecto |
| Render pipeline | URP con Renderer 2D | Plantilla *Universal 2D* de Unity Hub |
| Cámara | Cinemachine 3 + Pixel Perfect Camera (URP) | |
| Input | Input System (paquete nuevo) | Teclado y gamepad; remapeo de controles |
| Idiomas | Paquete *Localization* de Unity | **Español e inglés**, desde el inicio (15.3) |
| Física | Physics 2D (Rigidbody2D) | Ver sección 11 |
| Animación | Animator + clips de sprites | Sin rig de huesos |
| Arte | Piskel → PNG spritesheet | Ver sección 4 |
| Control de versiones | GitHub, sin Git LFS | Ver 2.4 |

### 2.1 Escala del mundo

| Valor | Definición |
|---|---|
| Tamaño de tile | 16×16 px = **1 unidad de Unity** |
| Pixels Per Unit (PPU) | **16** en todos los sprites del juego |
| Altura del personaje | **32 px = 2 unidades** (2 tiles) |
| Resolución de referencia | **320×180** (20 × 11.25 tiles visibles) |
| Escalado | ×4 a 1280×720 · ×6 a 1920×1080 · ×8 a 2560×1440 · ×12 a 3840×2160 |

Se eligió 320×180 porque escala con un **factor entero** en todas las resoluciones comunes de PC, incluido 1440p. Con eso no hay píxeles deformados ni barras negras en 16:9.

> **Validado en el Hito 1:** se mantiene 320×180. Si más adelante el campo de visión se siente corto, la alternativa es 384×216 (24 × 13.5 tiles), que escala bien en 1080p y 4K pero deja bordes negros en 1440p.

### 2.2 Configuración de pixel art en Unity

- Importación de sprites: `Pixels Per Unit = 16`, `Filter Mode = Point`, `Compression = None`, `Sprite Mode = Multiple`.
- Un `AssetPostprocessor` (`PixelArtImportPostprocessor`, en `Scripts/Editor`) aplica estos valores a todo PNG nuevo dentro de `Assets/_Project/Art/`, solo en su primera importación. Cumple la función de un Preset por carpeta sin configurar el Preset Manager.
- Pixel Perfect Camera: `Assets PPU = 16`, `Reference Resolution = 320×180`, `Crop Frame = None`, `Grid Snapping = Upscale Render Texture`.
- Todo desplazamiento animado por código se redondea a múltiplos de `1 / 16 = 0.0625`.

### 2.3 Framerate y física

| Valor | Configuración |
|---|---|
| Objetivo | **60 FPS** |
| VSync | Activado por defecto (`QualitySettings.vSyncCount = 1`) |
| Sin VSync | `Application.targetFrameRate = 60` (opción en el menú, ver 15.3) |
| Fixed Timestep | **1/60 = 0.0166667 s** (*Project Settings → Time*). Unity usa 1/50 por defecto |
| Maximum Allowed Timestep | 0.1 s (valor por defecto) |
| Rigidbody2D del jugador | `Interpolate` activado |

- Con el Fixed Timestep en 1/60, la física y el render van al mismo ritmo a 60 FPS, y el movimiento en pixel art se ve más estable.
- **Monitores de más de 60 Hz:** con VSync en un monitor de 144 Hz, el juego dibuja 144 FPS pero la física sigue a 60. La interpolación del Rigidbody2D suaviza el movimiento entre pasos de física.
- **La jugabilidad nunca depende del framerate:**
  - El input se lee en `Update` y la física se aplica en `FixedUpdate` (5.3).
  - Todos los temporizadores (coyote time, buffers, dash, shoot pose, i-frames, cooldowns) se miden en **segundos** con `Time.deltaTime` o `Time.fixedDeltaTime`, nunca contando frames.
  - La velocidad de las animaciones la define el *Sample Rate* de cada clip, no el framerate del juego.
- **Validación:** probar el nivel de prueba a 60 Hz y, si es posible, a 144 Hz (o con VSync desactivado). La altura del salto y la distancia del dash deben ser idénticas.

### 2.4 Control de versiones (GitHub)

El proyecto se versiona en **GitHub**. **No se usa Git LFS** (decisión del 2026-10-09): los binarios se suben al historial normal.

**Configuración inicial** (antes del primer commit con assets):

1. Crear el proyecto con Unity Hub y luego el repositorio en GitHub (privado o público, sin README para evitar conflictos). En la carpeta del proyecto: `git init`, `git remote add origin <url>`.
2. **`.gitignore`:** usar la plantilla oficial de Unity (`github/gitignore` → `Unity.gitignore`). Excluye `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `Obj/` y los builds.
3. **`.gitattributes`:** marca los binarios (`*.png`, `*.wav`, `*.ogg`, etc.) como `binary`, para que Git no intente compararlos ni convertir sus fines de línea. Los `.piskel` son JSON y se versionan como texto.
4. **En Unity** (*Project Settings → Editor*): verificar *Version Control = Visible Meta Files* y *Asset Serialization = Force Text* (son los valores por defecto en Unity 6). Así las escenas, prefabs y ScriptableObjects se guardan como texto y se pueden comparar en Git.
5. Primer commit: el proyecto vacío, `.gitignore`, `.gitattributes` y este documento.

**Reglas:**
- Los archivos `.meta` **siempre** se suben junto a su asset. Si falta uno, Unity crea otro con un GUID distinto y se rompen las referencias.
- **Sin LFS, vigilar el tamaño del repositorio.** GitHub rechaza archivos de más de 100 MB y avisa a partir de 50 MB. Cada versión de un binario se queda para siempre en el historial, así que conviene no subir muchas versiones intermedias de música o audio pesado. Si el repositorio crece demasiado, se puede migrar a LFS más adelante (`git lfs migrate`), aunque reescribe el historial.
- Commits pequeños y frecuentes, por lo menos uno al terminar cada tarea de un hito.
- **Ramas:** `main` siempre funciona. Cada hito o función grande se trabaja en su propia rama (por ejemplo, `hito-1-movimiento`) y se une a `main` cuando funciona.

---

## 3. Estructura del proyecto

```
Game/                               ← raíz del repositorio
├── ArtSource/                      ← archivos .piskel (fuera de Assets, Unity no los importa)
│   ├── Characters/<Personaje>/     ← un .piskel/spritesheet por personaje
│   ├── Enemies/
│   ├── Tiles/
│   └── UI/
├── AudioSource/                    ← originales de SFX descargados y proyectos de música
│   ├── SFX/
│   └── Music/
├── CREDITS.md                      ← autor y licencia de cada sonido de librería
└── Assets/
    ├── _Project/
    │   ├── Art/                    ← PNG exportados desde Piskel
    │   │   ├── Characters/<Personaje>/ ← un spritesheet por personaje (4.2.1)
    │   │   ├── Enemies/
    │   │   ├── Projectiles/
    │   │   ├── Tiles/
    │   │   ├── Portraits/          ← retratos de busto por emoción (15.7)
    │   │   ├── Illustrations/      ← ilustraciones de las escenas, por nivel
    │   │   ├── UI/                 ← iconos de habilidades, barras, cuadro de diálogo
    │   │   └── VFX/
    │   ├── Animations/
    │   │   ├── Characters/<Personaje>/{Legs,Torso,FullBody}/
    │   │   └── Enemies/
    │   ├── Audio/{SFX,Music}/      ← .wav y .ogg
    │   ├── Audio/Mixer/            ← GameAudioMixer
    │   ├── Data/
    │   │   ├── Characters/         ← CharacterData, CharacterCatalog y, por personaje, PlayerMovementData y BusterData
    │   │   ├── Abilities/          ← AbilityData (.asset)
    │   │   ├── Enemies/
    │   │   ├── Pickups/
    │   │   ├── Missions/           ← MissionData, MissionCatalog
    │   │   ├── Cutscenes/          ← CutsceneData, SpeakerData
    │   │   └── Audio/              ← SfxData, MusicTrack
    │   ├── Input/                  ← PlayerControls.inputactions
    │   ├── Prefabs/{Player,Enemies,Projectiles,Abilities,Pickups,VFX}/
    │   ├── Presets/                ← preset de importación de sprites
    │   ├── Scenes/
    │   └── Scripts/
    │       ├── Core/               ← StateMachine, IState, pooling, utilidades
    │       ├── Player/
    │       │   ├── States/Locomotion/
    │       │   └── States/Action/
    │       ├── Abilities/
    │       ├── Combat/             ← IDamageable, DamageInfo, Projectile
    │       ├── Enemies/
    │       ├── Level/              ← checkpoints, peligros, cámara
    │       ├── Game/               ← GameManager, flujo de escenas
    │       ├── Save/               ← SaveData, SettingsData, SaveSystem, SaveMigrator
    │       ├── Cutscenes/          ← CutsceneRunner, pasos de escena, VisualNovelView, DialogueBox
    │       ├── Audio/              ← MusicPlayer, SfxPlayer, VolumeSettings
    │       └── UI/
    └── Plugins/
```

Usar **Assembly Definitions** (`Game.Core`, `Game.Gameplay`, `Game.UI`) desde el inicio para compilar más rápido y evitar dependencias circulares.

---

## 4. Pipeline de arte

### 4.1 Lienzo y alineación del jugador

| Valor | Definición |
|---|---|
| Lienzo en Piskel | **48×64 px** (ancho × alto) en todos los archivos del jugador |
| Pies | Fila inferior del personaje en **y = 12** del lienzo (contando desde abajo). Deja 12 px debajo de los pies para el cañón al disparar hacia abajo |
| Centro horizontal | **x = 24** |
| Cintura | Fila **y = 26** (14 px sobre los pies) **[AJUSTAR]** al dibujar |
| Espacio sobre la cabeza | La cabeza llega a y = 44; quedan 20 px arriba para el brazo y el arma al apuntar hacia arriba |
| Pivote en Unity | **Custom, en píxeles (24, 12)**: los pies, igual para piernas, torso y cuerpo completo |

Como las dos mitades se dibujan en el **mismo lienzo y en su posición real**, comparten el mismo pivote y quedan alineadas solas. No hace falta calcular un pivote en la cintura.

- **Traslape:** el torso se dibuja 1–2 px más largo hacia abajo de la cintura, para que no aparezca un hueco cuando las piernas rebotan.
- **Orden de dibujo:** `Legs` con *Order in Layer* = 0 y `Torso` con *Order in Layer* = 1.
- **Sin fogonazo:** el disparo no tiene destello del cañón (decisión del 2026-10-14). El movimiento del arma en los clips `Torso_Shoot_*` es el feedback visual del disparo.

### 4.2 Flujo de trabajo en Piskel

1. **Un archivo `.piskel` por clip y por mitad** (por ejemplo, `ArtSource/Player/Legs/Legs_Run.piskel`), todos con lienzo de 48×64.
2. Para dibujar el torso alineado, agrega una capa de **referencia** con las piernas (copiadas del archivo de piernas) y dibuja el torso en otra capa. **Antes de exportar, elimina la capa de referencia** o exporta desde una copia sin ella, y revisa que el PNG no la incluya.
3. Exporta como **PNG spritesheet en una sola fila** (todas las columnas en un renglón). Así el *slicing* en Unity es siempre *Grid By Cell Size* de 48×64.
4. Guarda el PNG en la carpeta equivalente dentro de `Assets/_Project/Art/`, con el mismo nombre que el `.piskel`.
5. En Unity: *Sprite Editor → Slice → Grid By Cell Size 48×64*, con *Pivot = Custom* y *Pivot Unit Mode = Pixels* en (24, 12).
6. Los FPS de Piskel son solo de vista previa. La velocidad real se define en el clip de Unity (*Sample Rate*, por ejemplo 12 fps) **[AJUSTAR]**.

> Al volver a exportar un PNG con el mismo número de frames y el mismo tamaño, Unity conserva el *slicing* y los clips siguen funcionando. Si cambias la cantidad de frames, vuelve a hacer el *slice* y revisa el clip.

### 4.2.1 Spritesheet del jugador (flujo actual)

Todo el jugador está en **un solo archivo**, `Art/Characters/Player/RosslynSpitesheet.png`: una cuadrícula de celdas de **48 × 64** (20 columnas × 15 filas). No hace falta cortarlo a mano: el menú **Game → Setup → 4. Personaje animado** lo corta (pivote en los pies, PPU 16), crea los clips en `Animations/Player/{Legs,Torso,FullBody}/`, los tres Animators y actualiza el prefab del jugador.

Cada clip empieza en una celda fija y toma las celdas **no vacías consecutivas** hacia la derecha, hasta la siguiente columna inicial de la misma fila. Para agregar frames basta con dibujarlos a la derecha y volver a ejecutar el menú. Las filas y columnas se cuentan desde 1, de arriba hacia abajo:

| Fila | Columna inicial → clip |
|---|---|
| 1 | 1 → `Torso_Idle` |
| 2 | 1 → `Legs_Idle` |
| 3 | 1 → `Torso_Run` |
| 4 | 1 → `Legs_Run` |
| 5 | 1 → `Torso_JumpUp` · 2 → `Torso_Fall` · 3 → `Torso_Crouch` · 4 → `Torso_Dash` · 6 → `Torso_Aim_Up` · 7 → `Torso_Aim_DiagUp` |
| 6 | 1 → `Legs_JumpUp` · 2 → `Legs_Fall` · 3 → `Legs_Crouch` · 4 → `Legs_Dash` |
| 7 | (vacía) |
| 8 | 1 → `Torso_Shoot_Forward` · 6 → `Torso_Shoot_Up` · 11 → `Torso_Shoot_DiagUp` · 16 → `Torso_Shoot_Crouch` |
| 9 | 1 → `Torso_Shoot_Down` |
| 10 | 1 → `FullBody_Hurt` |
| 11 | 1 → `FullBody_Death` |
| 12 | 1 → `FullBody_Teleport_In` |
| 13 | 1 → `FullBody_Teleport_Out` |

- Los clips se crean a 12 fps. Si se cambia el *Sample Rate* de un clip en Unity, al volver a ejecutar el menú se respeta.
- **La tabla de esta sección vive en un asset `SpritesheetLayout`** (`Data/Characters/`), no en el código de `PlayerArtSetup.cs`. Cada entrada tiene el nombre del clip, la fila, la columna inicial, si hace loop y a qué mitad pertenece (piernas, torso o cuerpo completo). Todos los personajes usan el mismo layout. Para agregar un clip (por ejemplo, el de una habilidad nueva) se agrega una entrada al asset, se dibujan las celdas y se vuelve a ejecutar el menú, sin tocar código.
- **No mover** un clip a otra celda sin cambiar su entrada en el `SpritesheetLayout`.
- El asset se abre con **Game → Setup → Abrir SpritesheetLayout**. Si no existe, el menú lo crea con los clips actuales. Antes de cortar la hoja, el menú revisa que no haya nombres vacíos o repetidos ni dos clips en la misma celda inicial, y avisa si una entrada apunta fuera de la hoja.
- Cada entrada tiene: nombre del clip (debe coincidir con el que busca `PlayerAnimation`), mitad (`Legs`, `Torso` o `FullBody`), fila y columna inicial (desde 1) y si hace loop. El asset también guarda el tamaño de celda (48 × 64), el pivote (24, 12), los fps de los clips nuevos (12) y el estado inicial de cada Animator.
- **Clips en PNG aparte (`SpritesheetLayout.strips`):** los clips que no caben en la celda de 48×64 van en su propio PNG, en la misma carpeta que la hoja: tira horizontal con frames de un tamaño propio y el pivote de los pies en píxeles (para que el torso encaje con las piernas). El menú **4. Personaje animado** los corta y genera sus clips igual que los de la hoja. Los usa el tajo de la espada de Rosslyn: `RosslynFireSlash_horizontal.png` (100×64, `Torso_Melee`), `RosslynFireSlash_crouch.png` (100×48, `Torso_Melee_Crouch`), `RosslynFireSlash_Up.png` (48×100, `Torso_Melee_Up`) y `rosslynFireSlash_down.png` (48×100, `Torso_Melee_Down`). No hay `Torso_Melee_DiagUp`: el tajo diagonal usa el frontal.
- Si en algún momento la hoja crece, se agregan filas al final (la hoja puede crecer hacia abajo).

**Varios personajes:** cada personaje tiene **su propio spritesheet con la misma cuadrícula y la misma tabla de clips**. Así `PlayerArtSetup` y `PlayerAnimation` funcionan igual para todos: el menú recibe el personaje, corta su hoja y genera sus clips y Animators en `Animations/Characters/<Personaje>/`. Las celdas de un clip que un personaje no usa (por ejemplo, `Torso_Melee` si no tiene espada) se dejan vacías.
- Hoy la hoja de Rosslyn está en `Art/Characters/Player/` y sus clips en `Animations/Player/`. Se mueven a la carpeta del personaje cuando se agregue el segundo (Hito 7).

### 4.3 Tiles y otros sprites

- Tiles: 16×16 px, exportados como una hoja (*tileset*), *slice* por grid de 16×16 y pivote centrado.
- Obstáculos por habilidad: bloque destructible (1 tile + VFX al romperse), muro de acero en **3 etapas de calor** (normal, caliente, al rojo vivo) + VFX de metal fundido, y **fuego** (tile animado en loop + VFX de vapor al apagarse). Deben distinguirse a simple vista del suelo normal, para que el jugador sepa que se pueden destruir.
- Enemigos: un solo sprite de cuerpo completo, con pivote en los pies.
- Iconos de habilidades: 16×16 px **[AJUSTAR]**.
- Paleta: usa una paleta fija para todo el juego, guardada en Piskel (*Palettes → Create/Import*). Así el arte mantiene coherencia.

### 4.4 Lista de animaciones del jugador

**Legs**

| Clip | Loop | Notas |
|---|---|---|
| `Legs_Idle` | Sí | |
| `Legs_Run` | Sí | El rebote del torso está dibujado en los frames de `Torso_Run`; no se mueve por código |
| `Legs_JumpUp` | No | |
| `Legs_Fall` | Sí | |
| `Legs_Land` | No | Opcional |
| `Legs_Crouch` | Sí | |
| `Legs_Dash` | Sí | Se usa también para el air dash |

**Torso**

El torso cambia de pose **al mantener arriba o arriba-adelante**, aunque no se esté disparando. Las poses hacia adelante y hacia abajo solo aparecen al disparar.

| Clip | Loop | Cuándo se ve |
|---|---|---|
| `Torso_Idle` | Sí | Reposo en el suelo |
| `Torso_Run` | Sí | Corriendo |
| `Torso_JumpUp` | Sí | En el aire, subiendo (va con `Legs_JumpUp`) |
| `Torso_Fall` | Sí | En el aire, cayendo (va con `Legs_Fall`) |
| `Torso_Crouch` | Sí | Agachado |
| `Torso_Dash` | Sí | Dash y air dash |
| `Torso_Aim_Up` | Sí | Mientras se mantiene arriba (disparando o no) |
| `Torso_Aim_DiagUp` | Sí | Mientras se mantiene arriba + horizontal (disparando o no) |
| `Torso_Shoot_Forward` | No | *Shoot pose* hacia adelante |
| `Torso_Shoot_DiagUp` | No | *Shoot pose* en diagonal arriba-adelante (movimiento del arma al disparar) |
| `Torso_Shoot_Up` | No | *Shoot pose* hacia arriba (movimiento del arma al disparar) |
| `Torso_Shoot_Down` | No | *Shoot pose* hacia abajo, solo en el aire |
| `Torso_Shoot_Crouch` | No | *Shoot pose* agachado |
| `Torso_Melee` | No | Tajo cuerpo a cuerpo hacia adelante (espada de fuego de Rosslyn, cuchillo de Veyron). Los frames con hitbox se configuran en el efecto (8.2) |
| `Torso_Melee_DiagUp` | No | El tajo en diagonal arriba-adelante |
| `Torso_Melee_Up` | No | El tajo hacia arriba |
| `Torso_Melee_Down` | No | El tajo hacia abajo, solo en el aire (rebota al golpear, 8.2) |
| `Torso_Melee_Crouch` | No | El tajo agachado |

La granada de goma, el taladro, el lanzallamas y el láser salen del cañón, así que reutilizan las poses de disparo y de apuntado. Solo las armas cuerpo a cuerpo (espada y cuchillo) necesitan clips propios del torso, uno por dirección, igual que los `Torso_Shoot_*`. Todos los clips de tajo se llaman igual en las dos hojas (`Torso_Melee*`): en la de Rosslyn se dibuja la espada y en la de Veyron el cuchillo. El arma sónica y la ráfaga de burbujas usan la pose de disparo, como las demás.

**Cuerpo completo** (se ocultan las mitades y se muestra el SpriteRenderer `FullBody`): `FullBody_Hurt`, `FullBody_Death`, `FullBody_Teleport_In` (al empezar el nivel y al reaparecer) y `FullBody_Teleport_Out`.

**Total por personaje:** 7 clips de piernas (`Legs_Land` es opcional), 13 de torso comunes más los clips propios de sus habilidades, y 4 de cuerpo completo. Los 5 clips `Torso_Melee*` existen en las dos hojas (espada para Rosslyn, cuchillo para Veyron). El arte de cada habilidad está en la sección 8.8.

---

## 5. Arquitectura del jugador

### 5.1 Jerarquía del prefab

```
Player                  Rigidbody2D, BoxCollider2D, PlayerController,
│                       PlayerInputReader, PlayerMotor, PlayerBuster,
│                       PlayerAbilities, PlayerEnergy, PlayerHealth,
│                       PlayerAnimation
├── Visual              ← se voltea con localScale.x = ±1
│   ├── Legs            SpriteRenderer, Animator (Legs.controller)
│   ├── Torso           SpriteRenderer, Animator (Torso.controller)
│   │   └── FirePoints
│   │       ├── Forward
│   │       ├── DiagUp
│   │       ├── Up
│   │       ├── Down
│   │       └── Crouch
│   └── FullBody        SpriteRenderer, Animator (FullBody.controller), desactivado
├── CameraTarget        ← CameraLookAhead; lo sigue la Cinemachine Camera
└── Hurtbox             BoxCollider2D (trigger), capa PlayerHurtbox
```

- Se voltea `Visual` y no el objeto raíz, para no invertir colliders ni la física.
- Collider de pie: 12×30 px aproximadamente (0.75 × 1.875 unidades) **[AJUSTAR]**. Agachado: mitad de alto.
- Se usa `BoxCollider2D` y no cápsula: la base redonda de una cápsula resbala en las orillas de las plataformas.
- El suelo se detecta proyectando el collider 0.05 u hacia abajo (`Rigidbody2D.Cast` con un `ContactFilter2D` de normal hacia arriba), no con un objeto `GroundCheck`. No se usan los contactos (`IsTouching`) porque Unity los borra al cambiar el tamaño del collider al agacharse. El cast ignora los colliders que ya se traslapan, así que las plataformas atravesables funcionan sin casos especiales. El techo para levantarse se revisa con un `OverlapBox` sobre el collider agachado.
- Los FirePoints se colocan en posiciones de píxel exactas (múltiplos de 0.0625), en la boca del cañón de cada pose.

### 5.2 Responsabilidades

| Componente | Hace | No hace |
|---|---|---|
| `PlayerController` | Recibe el `CharacterData` (5.4), crea el contexto, actualiza las FSM, reacciona al daño y a la muerte | Leer input o mover el Rigidbody directamente |
| `PlayerInputReader` | Lee el Input System y llena `InputSnapshot` (con buffers) | Tomar decisiones |
| `PlayerMotor` | Mover, saltar, hacer dash, detectar suelo y techo, cambiar el collider al agacharse | Decidir *cuándo* saltar o hacer dash |
| `PlayerBuster` | Disparo básico: cadencia, límite de balas en pantalla, spawn de proyectiles | Animar |
| `PlayerAbilities` | Habilidades desbloqueadas, habilidad seleccionada, cambio, cooldowns, activación. Evento `SelectedChanged` para el HUD | Gastar energía por su cuenta (usa `PlayerEnergy`) |
| `PlayerEnergy` | Barra de energía compartida: `TrySpend`, `Refill`, evento `Changed` | Saber qué habilidad la usa |
| `PlayerHealth` | Vida, i-frames tras recibir daño, eventos `Damaged` y `Died` | Cambiar estados |
| `PlayerAnimation` | Elegir el estado de cada Animator, parpadeo de i-frames y reenviar los Animation Events (`OnFootstep`) | Lógica de gameplay |

Los valores de ajuste (velocidades, alturas de salto, dash) viven en un ScriptableObject `PlayerMovementData`, para poder ajustarlos en Play Mode sin tocar código. Cada personaje tiene el suyo, referenciado desde su `CharacterData` (5.4).

### 5.3 Orden de actualización

```
Update:
  1. InputReader.Poll()
  2. LocomotionFSM.Tick()
  3. ActionFSM.Tick()        ← ya conoce el resultado de locomoción
  4. PlayerAnimation.Sync()

FixedUpdate:
  1. LocomotionFSM.FixedTick()  ← física
```

### 5.4 Personajes (CharacterData)

Todos los personajes usan **los mismos componentes, la misma jerarquía (5.1) y las mismas FSM**. Lo que los distingue vive en un ScriptableObject `CharacterData`:

```csharp
[CreateAssetMenu(menuName = "Game/Character")]
public class CharacterData : ScriptableObject
{
    public string id;                       // clave para el guardado
    public string displayName;
    [TextArea] public string description;   // texto de la pantalla de selección
    public Sprite selectPortrait;           // imagen de la pantalla de selección
    public SpeakerData speaker;             // nombre y retrato en los diálogos (15.7)
    public PlayerController prefab;         // su arte, Animators y FirePoints

    [Header("Estadísticas")]
    public PlayerMovementData movement;
    public int maxHealth = 28;
    public float maxEnergy = 28f;

    [Header("Ataque")]
    public BusterData buster;               // disparo básico (sección 7)
    public AbilityData[] abilities;         // en el orden del ciclo siguiente/anterior
    public AbilityData[] testModeAbilities; // solo en el editor con el modo de prueba (Hito 4)
}
```

- **Un prefab por personaje**, como *Prefab Variant* de un prefab base `Player`. El variant solo cambia los sprites, los Animators y la posición de los FirePoints. Así un cambio en los componentes se aplica a todos.
- **El jugador no se coloca en la escena.** `MissionController` instancia `GameManager.SelectedCharacter.prefab` en el punto de inicio y llama a `PlayerController.Initialize(characterData)`. Ese método reparte los datos: `movement` al motor, `buster` a `PlayerBuster`, `maxHealth` a `PlayerHealth`, `maxEnergy` a `PlayerEnergy` y `abilities` a `PlayerAbilities`. Ningún componente tiene estos valores en sus propios campos serializados.
- La cámara (`SetTarget`, 13) y el HUD se conectan al jugador después de instanciarlo.
- **Hasta que exista `MissionController` (Hito 6)**, lo hace `PlayerSpawner` (Hito 4): un objeto en el punto de inicio de la escena con un `CharacterData` de respaldo y la Cinemachine Camera. En `Awake` instancia el prefab, llama a `Initialize` y asigna el `CameraTarget` a la cámara; en `Start` lanza el evento estático `PlayerSpawned`, que usa el HUD para conectarse. El `MissionController` del Hito 6 lo reemplaza o lo usa por dentro.
- Los datos de cada personaje van en `Data/Characters/<Personaje>/` (por ejemplo, `Rosslyn.asset`, `Rosslyn_Movement.asset` y `Rosslyn_Buster.asset`).
- **Collider:** mismo tamaño para todos por defecto, para que el diseño de niveles no cambie según el personaje **[AJUSTAR]**.
- **Diseño de niveles:** cada misión debe poder completarse con **cualquier** personaje. Como la velocidad y el salto cambian entre personajes (Veyron es el más lento, Agente X salta más), las distancias de los fosos se diseñan con los valores del más lento y el salto más bajo. Se fija un mínimo común (por ejemplo, todos superan 3 tiles de alto y un foso de 5 tiles con salto normal) y los niveles se diseñan con ese mínimo **[AJUSTAR]**.
- Para probar en el editor sin pasar por los menús, la escena de prueba tiene un campo con el `CharacterData` que se usa si no hay uno seleccionado.

#### Los 5 personajes

| Personaje | Disparo básico (7) | Habilidades (8.2) | Pasiva |
|---|---|---|---|
| Rosslyn | Bláster normal | Granada de goma, lanzallamas, espada de fuego | — |
| Veyron | Más potente, mucho menos alcance | Taladro, arma sónica, cuchillo | — (es **más lento**: conserva la velocidad original de 6 u/s y Rosslyn sube a ~7 u/s) |
| Aquamarine | Pistola de burbujas: como el normal pero más lento | Ráfaga de burbujas, chorro de agua, rayo congelante | **Burbuja:** al mantener salto en el aire, cae más lento (6.3) |
| Agente X | Pistola láser: bala más rápida, menos daño | Láser, dron, granada incendiaria | **Salto más alto** |
| Neotisleo | Bláster normal | Disco de energía, espada de luz, munición eléctrica | **Más energía** que los demás |

#### Pasivas

Dos de las tres pasivas son solo **valores** y no necesitan código:
- **Agente X, salto más alto:** `PlayerMovementData.jumpHeight` mayor. Los niveles se siguen diseñando con el mínimo común de salto, así que su salto solo abre atajos.
- **Neotisleo, más energía:** `CharacterData.maxEnergy` mayor.

La de Aquamarine sí es una mecánica: el **planeo con burbuja** (6.3). Se activa con un campo de `PlayerMovementData` (`canGlide` y sus valores), así que cualquier personaje podría tenerlo sin código nuevo.

---

## 6. Máquinas de estados

### 6.1 Núcleo

```csharp
public interface IState
{
    void Enter();
    void Tick(float dt);
    void FixedTick(float dt);
    void Exit();
}

public class StateMachine
{
    public IState Current { get; private set; }

    public void ChangeState(IState next)
    {
        if (next == Current) return;
        Current?.Exit();
        Current = next;
        Current.Enter();
    }

    public void Tick(float dt) => Current?.Tick(dt);
    public void FixedTick(float dt) => Current?.FixedTick(dt);
}
```

```csharp
public class PlayerContext
{
    public InputSnapshot Input;
    public PlayerMovementData Movement;
    public PlayerMotor Motor;
    public PlayerBuster Buster;
    public PlayerAbilities Abilities;
    public PlayerEnergy Energy;
    public PlayerAnimation Anim;
    public LocomotionFSM Locomotion;
    public ActionFSM Action;
}
```

Las transiciones viven **dentro de cada estado**. El controller solo actualiza las máquinas.

### 6.2 Nivel superior (vida del jugador)

```
Alive ──(Damaged)──▶ Hurt ──(fin del knockback)──▶ Alive
  │
  ├──(Died)──▶ Dead ──(timer)──▶ Respawning ──▶ Alive
  │
  ├──(Cinemática / jefe derrotado)──▶ Disabled ──▶ Alive
  │
  ├──(Atrapado por un enemigo)──▶ Grabbed ──(se suelta)──▶ Alive
  │
  └──(Sube a un vehículo)──▶ Piloting ──(baja o el vehículo se destruye)──▶ Alive
```

- En `Hurt`, `Dead` y `Respawning`, ambas FSM se desactivan y se muestra `FullBody`.
- En `Disabled` las FSM también se desactivan: se ven las mitades en `Idle`, y las escenas (15.7) pueden reproducir un clip de cuerpo completo (teletransporte). Lo usan las escenas, el jefe vencido y el rescate de habilidades.
- En `Hurt` se aplica un knockback hacia atrás y se activan los i-frames. Al volver a `Alive`, los i-frames siguen corriendo (el personaje parpadea), pero ya se puede controlar.
- **`Grabbed`:** un enemigo atrapa al jugador (la Mantis atea, 14.1). Las FSM se desactivan y el jugador recibe daño por tick. Para soltarse hay que **presionar disparo repetidamente**: cada pulsación llena una barra de escape; al llenarse, el jugador se suelta con i-frames cortos. Si muere atrapado, pasa a `Dead` **[AJUSTAR]** el daño y las pulsaciones.
- **`Piloting`:** el jugador maneja un vehículo (18). Sus FSM se desactivan, sus sprites y colliders se ocultan y el `InputSnapshot` se le pasa al vehículo.
- Esta FSM se implementa como las demás (`StateMachine` + `IState`) y no con `if` o `bool` sueltos, para poder agregar estados como estos.

### 6.3 FSM de locomoción

| Estado | Entra cuando | Sale a |
|---|---|---|
| `Idle` | En suelo, sin input horizontal | `Run` (input H) · `Crouch` (abajo) · `Airborne` (salto o pierde el suelo) · `Dash` (botón de dash) |
| `Run` | En suelo con input horizontal | `Idle` · `Crouch` · `Airborne` · `Dash` |
| `Crouch` | En suelo con abajo presionado | `Idle` (suelta abajo y hay espacio arriba) · `Airborne` (pierde el suelo) |
| `Dash` | Botón de dash en el suelo | `Airborne` con impulso de dash (salto o pierde el suelo) · `Idle` / `Run` (se acaba el tiempo) |
| `Airborne` | Salto, caída o rebote del tajo hacia abajo (8.2) | `Idle` / `Run` al aterrizar · `AirDash` (botón de dash y hay air dash disponible) |
| `AirDash` | Botón de dash en el aire | `Airborne` (se acaba el tiempo) · `Idle` / `Run` (aterriza) |

Propiedades públicas: `IsCrouching`, `IsGrounded`, `IsDashing` (verdadero en `Dash` y `AirDash`), `FacingDir`.

#### Salto

Valores iniciales en unidades (1 unidad = 1 tile = 16 px):

| Parámetro | Valor inicial |
|---|---|
| Velocidad de correr | 7 u/s para Rosslyn; 6 u/s para Veyron **[AJUSTAR]** |
| Altura máxima de salto | 3.5 u (≈ 3.5 tiles) **[AJUSTAR]** |
| Tiempo hasta la cima | 0.4 s **[AJUSTAR]** |
| Multiplicador de gravedad al caer | 1.5 **[AJUSTAR]** |
| Coyote time | 0.1 s **[AJUSTAR]** |
| Jump buffer | 0.1 s **[AJUSTAR]** |

- La gravedad y la velocidad inicial del salto se **calculan** a partir de la altura y el tiempo hasta la cima. Así se ajusta pensando en tiles y no en fuerzas.
- Altura variable: si se suelta el botón mientras sube, se reduce la velocidad Y.
- **Control en el aire libre:** se puede cambiar de dirección y velocidad en el aire, como en Mega Man. Se puede usar la misma aceleración que en el suelo o una un poco menor **[AJUSTAR]**.

#### Planeo con burbuja (pasiva de Aquamarine)

- En `Airborne`, si el personaje **está cayendo** (velocidad Y ≤ 0) y **mantiene salto**, aparece una burbuja a su alrededor y su velocidad de caída se limita a `glideMaxFallSpeed` (por ejemplo, 2 u/s) **[AJUSTAR]**. Al soltar el botón, la caída vuelve a la normal.
- No interfiere con la altura variable del salto: mientras sube, mantener el botón sigue siendo el salto normal.
- Se puede disparar y usar habilidades mientras planea. El air dash cancela el planeo; al terminar el dash se puede volver a planear.
- Solo se activa para personajes con `PlayerMovementData.canGlide`. La burbuja es un VFX encima del personaje; no hace falta otro clip de piernas o torso.

#### Dash

| Parámetro | Valor inicial | Notas |
|---|---|---|
| Velocidad del dash | 2.5× la velocidad de correr | **[AJUSTAR]** |
| Duración | 0.25 s | **[AJUSTAR]** |
| Cooldown | 0 s | Se puede volver a hacer dash al terminar el anterior |
| Air dashes por salto | 1 | Se recarga al aterrizar |
| Invulnerabilidad | No | |
| Disparar durante el dash | **No** | Ver 6.4 |

- **Dirección:** hacia donde mira el personaje. Durante el dash no se puede voltear.
- **Air dash:** horizontal y sin gravedad durante su duración. Al terminar, la velocidad Y empieza en 0.
- **Dash jump:** si se salta desde `Dash` (o dentro del jump buffer), se entra a `Airborne` con `carriesDashMomentum = true`. **Lo mismo al salir de una orilla durante el dash** (decisión del 2026-10-10): el impulso se conserva en la caída y también en un salto de coyote time. Mientras sea verdadero, la velocidad horizontal máxima es la del dash. El impulso se pierde al aterrizar o si el jugador suelta la dirección o cambia de dirección.
- **Collider:** el dash en el suelo mantiene la altura normal del collider.
- **No hay dash desde agachado.**
- **Dash por botón:** solo con el botón dedicado. No hay doble toque de dirección.

### 6.4 FSM de acción

| Estado | Entra cuando | Sale a |
|---|---|---|
| `Ready` | Por defecto | `Shooting` · `UsingAbility` |
| `Shooting` | Botón de disparo, `Buster.CanFire` y `!IsDashing` | `Ready` cuando se acaba el *shoot pose* o empieza un dash · `UsingAbility` |
| `UsingAbility` | Botón de habilidad, `Abilities.CanUseSelected` y `!IsDashing` | `Ready` cuando termina el *lock* (por pulsación) o cuando se suelta el botón o se acaba la energía (sostenida). Ver 8.5 |

Propiedades públicas: `AimDir` (ver 6.5), `IsInShootPose` y `MoveSpeedMultiplier` (lo toma de la habilidad activa; 1 si no hay ninguna).

**Durante el dash:**
- Los botones de disparo y de habilidad se ignoran. Su buffer **no** se guarda para el final del dash.
- Si empieza un dash mientras hay *shoot pose*, la pose se cancela y el torso pasa a `Torso_Dash`.
- No se puede hacer dash durante una habilidad por pulsación. El dash **sí cancela** las habilidades sostenidas (ver 8.5).

**Shoot pose:** cada disparo pone al torso en la pose de disparo durante 0.3 s **[AJUSTAR]**. Si se dispara otra vez dentro de ese tiempo, el temporizador se reinicia. Así el brazo no sube y baja con cada tiro. En las direcciones `Up` y `DiagUp` el torso ya está en pose de apuntado (`Torso_Aim_*`) con solo mantener la dirección; al disparar cambia a `Torso_Shoot_Up` o `Torso_Shoot_DiagUp`, que tienen el mismo movimiento del arma que el disparo hacia adelante (decisión del 2026-10-11).

**Cambio de habilidad:** los botones de siguiente y anterior se procesan en cualquier estado de la FSM de acción, excepto en `UsingAbility`, y también durante el dash.

### 6.5 Dirección de apuntado

```csharp
public enum AimDirection { Forward, DiagUp, Up, Down }
```

`AimDir` se calcula **en cada frame**, se dispare o no, porque el torso muestra las poses `Up` y `DiagUp` con solo mantener la dirección.

| Input | En el suelo | En el aire |
|---|---|---|
| Nada / solo horizontal | `Forward` | `Forward` |
| Arriba + horizontal | `DiagUp` (corre apuntando en diagonal) | `DiagUp` |
| Solo arriba | `Up` (quieto apuntando arriba) | `Up` |
| Abajo | *se agacha* → `Forward` desde `FirePoint.Crouch` | `Down` |
| Durante dash | No aplica (no se dispara) | No aplica |

| Dirección | Pose del torso | Cuándo cambia el sprite |
|---|---|---|
| `Up` | `Torso_Aim_Up` → `Torso_Shoot_Up` | Al mantener la dirección, y otra vez al disparar |
| `DiagUp` | `Torso_Aim_DiagUp` → `Torso_Shoot_DiagUp` | Al mantener la dirección, y otra vez al disparar |
| `Forward` | `Torso_Shoot_Forward` / `Torso_Shoot_Crouch` | Solo al disparar |
| `Down` | `Torso_Shoot_Down` | Solo al disparar |

> Para disparar en diagonal estando en el suelo hay que moverse (arriba + horizontal hace correr al personaje). **No hay aim lock** por ahora. Si en las pruebas hace falta, se agrega como un botón que inmoviliza al personaje y deja el stick solo para apuntar.

---

## 7. Disparo básico

Cada personaje tiene su propio `BusterData`, referenciado desde su `CharacterData` (5.4). Todos son proyectiles con valores distintos, así que no hace falta otro sistema. Esta sección describe el bláster de Rosslyn; la tabla resume los cinco. Valores iniciales **[AJUSTAR]**:

| Valor | Rosslyn | Veyron | Aquamarine | Agente X | Neotisleo |
|---|---|---|---|---|---|
| Daño | 1 | 2–3 | 1 | 1 | 1 |
| Velocidad del proyectil | 18 u/s | 18 u/s | ~10 u/s (burbujas lentas) | ~28 u/s | 18 u/s |
| Alcance (`maxRange`) | Hasta salir de la cámara | ~4–5 u | Hasta salir de la cámara | Hasta salir de la cámara | Hasta salir de la cámara |
| Arte | Bláster | Disparo pesado | Burbuja | Bala láser | Bláster |

Cadencia, direcciones y shoot pose son iguales para los cinco.

**Disparo básico de Veyron:** más potente que el bláster, pero con **mucho menos alcance** (`maxRange` corto).
- Al llegar a `maxRange`, el proyectil se desactiva con un VFX corto de disipación, para que el jugador vea dónde termina el alcance.
- **Diseño de enemigos y jefes:** con poco alcance, Veyron tiene que acercarse. Torretas lejanas, enemigos sobre plataformas altas y jefes que se quedan lejos deben poder alcanzarse sin recibir daño inevitable (14.2).

**Daño de Agente X:** hace 1 de daño, igual que el bláster; su ventaja es la velocidad de la bala.

Si el sprite del proyectil es distinto, solo cambia el prefab referenciado en su `BusterData`.

### 7.1 BusterData (ScriptableObject)

```csharp
[CreateAssetMenu(menuName = "Game/Buster")]
public class BusterData : ScriptableObject
{
    public Projectile projectilePrefab;
    public int damage = 1;
    public float projectileSpeed = 18f;      // unidades por segundo
    public float minTimeBetweenShots = 0.1f;
    public int maxProjectilesOnScreen = 0;   // 0 = sin límite
    public float maxRange = 0f;              // unidades; 0 = hasta salir de la cámara
    public float shootPoseDuration = 0.3f;
    public SfxData fireSfx;
}
```

### 7.2 Reglas
- **Semiautomático:** un disparo por cada pulsación. Se usa un buffer de entrada corto (0.05 s) para no perder pulsaciones rápidas.
- **No se dispara durante el dash** (ver 6.4).
- **Sin límite de balas en pantalla:** lo único que limita el disparo es la cadencia (`minTimeBetweenShots`). El campo `maxProjectilesOnScreen` se deja por si en las pruebas hace falta un límite.
- La dirección del proyectil sale de `AimDirection` y de `FacingDir`. La diagonal se normaliza.

### 7.3 Proyectiles (compartido con habilidades y enemigos)
- **Object pooling** obligatorio (`UnityEngine.Pool.ObjectPool<T>`).
- Se mueven por código (`transform` + raycast, o Rigidbody2D cinemático), no con física dinámica. **Excepción:** la granada de goma usa física dinámica para rebotar.
- Se desactivan al salir de la cámara, al chocar, al terminar su tiempo de vida o al recorrer su distancia máxima (`maxRange`, por ejemplo el disparo de Veyron).
- **Proyectiles enemigos:** para que la espada los pueda destruir necesitan un collider trigger en la capa `EnemyProjectile` (con un `Rigidbody2D` cinemático, porque se mueven). Los especiales de los jefes marcan `indestructible` (Hito 5).

---

## 8. Habilidades

### 8.1 Modelo

- **Cada personaje tiene su propia lista de habilidades** (`CharacterData.abilities`): 3 por personaje, 15 en total (8.2). Todas usan el mismo sistema (`AbilityData` + `AbilityEffect`), y la mayoría de las nuevas reutiliza efectos existentes.
- **El progreso es por personaje:** las habilidades que desbloquea un personaje no las tiene el otro (15.4).
- **Se obtienen como coleccionables** escondidos en los niveles, no al vencer jefes (8.9).
- **Una sola barra de energía compartida** para todas las habilidades del personaje. Su máximo depende del personaje (`CharacterData.maxEnergy`).
- La habilidad seleccionada se cambia con **siguiente y anterior** (en un ciclo que solo incluye las desbloqueadas) o **eligiéndola directamente en el menú de pausa**, como en Mega Man. Con 3 habilidades basta con el ciclo, pero el menú evita que se vuelva lento si en la V2 cada personaje tiene más.
- El botón de disparo siempre usa el disparo básico. El botón de habilidad usa la seleccionada.
- **El personaje no cambia de color.** La habilidad seleccionada solo se indica con su **icono en el HUD**.
- Hay dos modos de activación:
  - **Por pulsación:** gasta energía una vez por uso.
  - **Sostenida:** funciona mientras se mantiene el botón y gasta energía por segundo.

### 8.2 Lista de habilidades

Energía máxima: **28** **[AJUSTAR]** (más para Neotisleo). Todos los costos y valores son iniciales **[AJUSTAR]**. La columna "Reutiliza" indica de qué efecto sale cada habilidad nueva (8.10).

**Rosslyn**

| Habilidad | Tipo | Activación | Dirección | Costo | Resumen |
|---|---|---|---|---|---|
| Granada de goma | Proyectil físico | Pulsación | 4 direcciones, en arco | 3 | Rebota; explota al tocar un enemigo o al acabarse la mecha |
| Lanzallamas | Área sostenida | Sostenida | 4 direcciones | 6 / s | Llama de medio alcance que daña de forma continua. **Funde muros de metal** |
| Espada de fuego | Cuerpo a cuerpo | Pulsación | 4 direcciones | 2 | Tajo de corto alcance envuelto en llamas; el tajo hacia abajo rebota |

**Veyron**

| Habilidad | Tipo | Activación | Dirección | Costo | Resumen |
|---|---|---|---|---|---|
| Taladro | Proyectil perforante | Pulsación | 4 direcciones | 3 | Atraviesa a varios enemigos. **Rompe muros especiales** (bloques destructibles) |
| Arma sónica | Área corta (escopeta) | Pulsación | 4 direcciones | 3 | Como la escopeta de Metal Slug: un **boom sónico** de corto alcance que golpea fuerte a todo lo que está en un área frente a él |
| Cuchillo | Cuerpo a cuerpo | Pulsación | 4 direcciones | 1 | Tajo rápido y corto; reutiliza el sistema de la espada |

**Aquamarine**

| Habilidad | Tipo | Activación | Costo | Reutiliza | Resumen |
|---|---|---|---|---|---|
| Ráfaga de burbujas | Área corta (escopeta) | Pulsación | 3 | `BlastEffect` (el del arma sónica) | La misma escopeta, representada como una **ráfaga de burbujas** |
| Chorro de agua | Proyectil físico | Pulsación | 3 | `RubberGrenadeEffect` | Sale en arco como la granada, pero **se deshace al tocar el suelo** en lugar de rebotar. **Apaga el fuego** |
| Rayo congelante | Rayo sostenido | Sostenida | 8 / s | `LaserEffect` | Como el láser, pero **congela** a los enemigos (10.1) |

**Agente X**

| Habilidad | Tipo | Activación | Costo | Reutiliza | Resumen |
|---|---|---|---|---|---|
| Láser | Rayo sostenido | Sostenida | 8 / s | — (ya existe) | Rayo de largo alcance que daña de forma continua (antes era de Veyron) |
| Dron | Compañero | Pulsación | 6 | Nuevo: `DroneEffect` + `CompanionDrone` | Crea un dron que acompaña al jugador y ataca solo a los enemigos cercanos; se destruye cuando el jugador recibe daño |
| Granada incendiaria | Proyectil físico + área | Pulsación | 4 | `RubberGrenadeEffect` + `DamageZone` | Granada que, al explotar, deja un área de fuego en el suelo que daña por tick |

**Neotisleo**

| Habilidad | Tipo | Activación | Costo | Reutiliza | Resumen |
|---|---|---|---|---|---|
| Disco de energía | Proyectil perforante | Pulsación | 3 | `DrillEffect` | Como el taladro, **sin romper muros** (sin interacción `Break`) |
| Espada de luz | Cuerpo a cuerpo | Pulsación | 2 | `MeleeEffect` | Como la espada de fuego, **con más alcance** (hitbox más grande) |
| Munición eléctrica | Proyectil + explosión | Pulsación | 3 | `Projectile` + `Explosion` | Bola de energía que **explota al golpear** un enemigo o un muro; la explosión daña en área |

#### Granada de goma
- **Lanzamiento según la dirección de apuntado** (decisión del 2026-10-14). La X se multiplica por la dirección en la que mira el jugador. Valores en u/s **[AJUSTAR]**:

  | Dirección | Velocidad inicial | Resultado |
  |---|---|---|
  | Adelante | (6, 6) | El arco normal |
  | Diagonal arriba | (4, 9) | Arco alto, para enemigos en plataformas |
  | Arriba | (0, 11) | Sube y cae cerca del jugador (no le hace daño) |
  | Abajo (solo en el aire) | (3, −6) | Va directo al suelo y rebota |
  | Agachado | (7, 3) | Arco bajo, casi rodando |
- **Física:** es la única habilidad con `Rigidbody2D` **dinámico**, con un `PhysicsMaterial2D` de rebote 0.6 y fricción 0.1 y `gravityScale` 2 **[AJUSTAR]**. Rebota en el suelo, en las paredes y sobre las plataformas atravesables (la matriz de colisiones deja chocar `PlayerProjectile` con `Platform`).
- **Explota** al tocar una `EnemyHurtbox` o cuando se acaba la mecha (2 s **[AJUSTAR]**).
- **Explosión:** daño de área con `Physics2D.OverlapCircleAll`, radio de 1.5 u y 4 de daño **[AJUSTAR]**. Golpea una sola vez a cada enemigo.
- No daña al jugador.
- **Sin límite** de granadas en pantalla. Las limitan la energía y el cooldown (0.3 s **[AJUSTAR]**).
- **Sale del cañón** de Rosslyn: usa la pose de disparo de la dirección (`Torso_Shoot_*`, igual que el disparo básico) y se lanza desde el `FirePoint` de esa dirección. No tiene animación de lanzamiento propia.

#### Espada de fuego
- **4 direcciones** (decisión del 2026-10-14): adelante, diagonal arriba-adelante, arriba, abajo (solo en el aire) y agachada. La dirección se fija al empezar el tajo. En el suelo, abajo es agacharse, así que da el tajo agachado.
- **Hitbox** rectangular de 1.5 × 1.5 u **[AJUSTAR]** en la dirección del tajo: frente al jugador, en diagonal, sobre la cabeza o bajo los pies. El prefab del efecto tiene un hijo con `DamageHitbox` por dirección (`Hitbox_Forward`, `Hitbox_DiagUp`, `Hitbox_Up`, `Hitbox_Down`, `Hitbox_Crouch`), colocados mirando a la derecha, y solo se activa el de la dirección elegida; se ven y se ajustan con Gizmos en el editor.
- **Frames activos:** la hitbox solo está activa en una ventana de frames del clip (`activeFromFrame` / `activeToFrame` en `MeleeEffect`, 2–3 al empezar **[AJUSTAR]**), calculada con el *Sample Rate* del clip. No se usan Animation Events: así funciona igual con cualquier dirección, y también antes de que el clip esté dibujado (con `fallbackFrameCount` frames a 12 fps). La duración del tajo es la del clip de esa dirección.
- 4 de daño **[AJUSTAR]**. Golpea una sola vez a cada enemigo por tajo.
- Se puede usar en el suelo, corriendo y en el aire, porque la animación es solo del torso.
- **Se puede usar agachado** con su propio clip, `Torso_Melee_Crouch`. La hitbox se coloca más abajo (1.5 × 1 u a la altura del torso agachado **[AJUSTAR]**), lo que sirve contra enemigos pequeños.
- **Rebote del tajo hacia abajo (pogo)** (decisión del 2026-10-14): si el tajo hacia abajo golpea a un enemigo o destruye un proyectil enemigo, Rosslyn rebota hacia arriba, al estilo de Shovel Knight.
  - Velocidad del rebote: la de un salto de 2.5 u de altura **[AJUSTAR]**. La altura es fija: soltar el botón de salto no la corta.
  - Un solo rebote por tajo, aunque golpee a varios objetivos.
  - El rebote **recarga el air dash** **[AJUSTAR]**, para poder encadenarlo con el movimiento.
  - No rebota en el suelo, en paredes, en obstáculos (`AbilityObstacle`) ni en peligros como pinchos.
  - Implementación: `MeleeEffect` tiene un campo `pogoJumpHeight` (0 = sin rebote). Al primer golpe del tajo hacia abajo llama a `LocomotionFSM.Bounce(altura)`, que pasa a `Airborne` con la velocidad Y calculada, sin corte de salto. Si el jugador está en `AirDash`, el rebote lo termina.
  - Al diseñar niveles hay que tenerlo en cuenta: encadenando rebotes sobre enemigos voladores se llega más alto que con un salto.
- **Destruye proyectiles enemigos:** la hitbox también detecta la capa `EnemyProjectile` y desactiva los proyectiles que toca. Así la espada tiene un uso defensivo. Los proyectiles especiales de los jefes pueden marcarse como `indestructible`.
- Torso: el clip de la dirección (`Torso_Melee`, `Torso_Melee_Up`, `Torso_Melee_Down` o `Torso_Melee_Crouch`), de pie, corriendo o en el aire. **La llama del tajo está dibujada en el mismo clip** (tiras más grandes que la celda, 4.2.1), así que la espada de Rosslyn no usa VFX aparte; el campo de VFX de `MeleeEffect` queda para el cuchillo u otras armas. Si falta el clip de una dirección (Rosslyn no tiene diagonal), el tajo y su hitbox usan los de adelante.
- Valores medidos sobre el arte (5 frames a 12 fps, 0.42 s): golpe en los frames 1–3; hitboxes (u, desde los pies, mirando a la derecha) adelante centro (0.94, 1.28) y 3.5 × 1.69, agachada (1.25, 1.06) y 3.5 × 2, arriba (0, 2.5) y 2.13 × 2.5, abajo (0.19, 0.06) y 2 × 1.38 **[AJUSTAR]**.

#### Lanzallamas
- **Sostenido:** activo mientras el botón esté presionado y haya energía.
- **Alcance:** 4 u **[AJUSTAR]**. Si choca con una pared, la llama se recorta (raycast contra `Ground` y `AbilityObstacle`).
- **Funde muros de acero:** tiene la interacción `Melt` (15.2). Si la llama toca un muro que la acepta, le acumula calor hasta fundirlo.
- Atraviesa a los enemigos.
- **Daño por tick:** 1 de daño cada 0.15 s a cada enemigo dentro de la llama **[AJUSTAR]**.
- **Movimiento:** al 50% de la velocidad y se puede saltar.
- Sigue la dirección de apuntado en tiempo real.
- Torso: mantiene la pose de disparo de la dirección actual.
- **Visual: un chorro de llamas sueltas** (decisión del 2026-10-14), en lugar de un sprite largo de llama:
  - Mientras está activo, `FlamethrowerEffect` suelta una llama (`FlamePuff`) cada 0.03 s desde el `FirePoint`, con una dispersión de ±6° y ±10 % de velocidad al azar **[AJUSTAR]**.
  - Cada llama viaja a 9 u/s **[AJUSTAR]** hasta el alcance (4 u, recortado por la pared al momento de salir). Vive unos 0.45 s, así que hay unas 15 llamas a la vez. Salen de un pool de 24.
  - **Animación por vida, no por Animator:** el frame se elige según qué parte de su vida lleva (`frames[(int)(t * frames.Length)]`): pequeña al salir del cañón, grande a media distancia y humo al final. Si choca con una pared antes de tiempo, salta a los frames de humo.
  - La llama es redonda, así que **no tiene sprites por dirección ni se rota**. Para que no se vea repetida, cada una se voltea en X o Y al azar y puede usar una de dos variantes.
  - Las llamas viajan en el mundo, no pegadas al jugador: al moverse o cambiar de apuntado, el chorro se curva solo.
  - Al soltar el botón ya no salen llamas nuevas y las que están en el aire terminan su vida. Eso reemplaza a una animación de apagado.
- **Daño: una zona fija, no cada llama** (decisión del 2026-10-14). Un `DamageHitbox` por tick girado hacia la dirección de apuntado (girar una zona de daño no deforma nada; lo que no se rota son los sprites), de 0.75 u de ancho **[AJUSTAR]** y largo igual al alcance recortado por la pared. La misma zona aplica `Melt`. Las llamas son solo visuales: así el daño es predecible y fácil de ajustar.

#### Taladro
- Proyectil movido por código a 10 u/s **[AJUSTAR]**. Gira hacia la dirección de apuntado.
- **Perfora sin límite:** atraviesa a todos los enemigos y golpea a cada uno una sola vez.
- Se destruye al chocar con `Ground` o al salir de la pantalla.
- **Bloques destructibles:** tiene la interacción `Break` (15.2). Al chocar con un bloque que la acepta, lo rompe y **se detiene**. Cualquier otro obstáculo, como el acero, lo detiene sin romperse. Un taladro rompe un solo bloque, así que un muro de varios bloques cuesta varios taladros (y energía). Ver 15.2.
- 3 de daño por enemigo **[AJUSTAR]**.
- Máximo **1** taladro en pantalla **[AJUSTAR]**. Como atraviesa todo, este límite es lo que evita que domine el combate.
- Torso: pose de disparo.

#### Láser
- **Sostenido:** activo mientras el botón esté presionado y haya energía.
- **Alcance:** raycast desde el FirePoint hasta `Ground` o hasta 20 u (más que el ancho de la pantalla).
- **Atraviesa a los enemigos:** solo lo detienen las paredes (`Ground` y `AbilityObstacle`). No tiene interacciones con obstáculos: no rompe ni funde nada.
- **Daño por tick:** 1 de daño cada 0.2 s a cada enemigo dentro del rayo **[AJUSTAR]**.
- Arranque de 0.1 s antes de hacer daño, para que se vea cómo se carga **[AJUSTAR]**.
- **Movimiento:** el jugador se queda quieto en el suelo (`moveSpeedMultiplier = 0`). En el aire cae de forma normal y no puede moverse horizontalmente. Esto equilibra que el rayo atraviese a todos los enemigos.
- Sigue la dirección de apuntado en tiempo real.
- Torso: mantiene la pose de disparo de la dirección actual.

#### Cuchillo
- **Reutiliza todo el sistema de la espada de fuego:** el mismo `MeleeEffect` (8.4), `DamageHitbox` de un golpe por objetivo, la ventana de frames activos y los clips `Torso_Melee*` de la hoja de Veyron. Solo cambian los valores y el VFX.
- Valores iniciales **[AJUSTAR]**: hitbox de 1 × 1 u (más chica que la espada), 3 de daño, costo 1 y un tajo más rápido (clip más corto).
- Se usa de pie, corriendo, en el aire y agachado, en las **4 direcciones** y con el **rebote del tajo hacia abajo**, igual que la espada (decisión del 2026-10-14). Necesita los mismos 5 clips `Torso_Melee*` en la hoja de Veyron.
- **Destruye proyectiles enemigos**, como la espada. Eso lo distingue del disparo básico de Veyron, que también es de corto alcance.

#### Arma sónica
- Funciona como la **escopeta de Metal Slug**: un disparo de **corto alcance** que golpea **una vez a todo lo que está en un área** frente al personaje, con mucho daño y poca cadencia. En Veyron se ve como un **boom sónico**.
- **Área:** un abanico que se abre desde el FirePoint, de unas 3.5 u de largo y unas 2.5 u de ancho al final **[AJUSTAR]**. Se aproxima con 2 o 3 `DamageHitbox` rectangulares escalonados, activos unos pocos frames, en modo **una vez por objetivo**.
- **Daño:** alto, por ejemplo 4 por objetivo **[AJUSTAR]**. Atraviesa enemigos: golpea a todos los del área, no solo al primero.
- **Cooldown:** ~0.5 s **[AJUSTAR]**, para que no reemplace al disparo básico.
- **Direcciones:** las 4 de las habilidades (adelante, diagonal arriba, arriba, abajo en el aire) y agachado. La dirección se fija al activarse.
- Las paredes cortan el área (no golpea a través de `Ground` ni de `AbilityObstacle`). No rompe ni abre obstáculos.
- **Efecto compartido:** `BlastEffect`. La **ráfaga de burbujas** de Aquamarine es el mismo efecto con otro arte (burbujas en abanico que revientan) y sus propios valores **[AJUSTAR]**.
- Encaja con Veyron: refuerza su estilo de corto alcance (disparo pesado, cuchillo) con un golpe de área para grupos de enemigos.

#### Ráfaga de burbujas (Aquamarine)
- Mismo `BlastEffect` que el arma sónica, con el arte de una **ráfaga de burbujas** en abanico.
- Los valores pueden diferir (por ejemplo, un poco más de alcance y menos daño) **[AJUSTAR]**.

#### Chorro de agua (Aquamarine)
- Mismo `RubberGrenadeEffect` que la granada de goma, con un campo nuevo `destroyOnGround`: al tocar el suelo, el chorro se deshace con un VFX de salpicadura en lugar de rebotar. Daña al tocar un enemigo.
- Tiene la interacción `Extinguish`: al tocar un tile de fuego lo **apaga** y se deshace (15.2).
- Arco por dirección igual que la granada.

#### Rayo congelante (Aquamarine)
- Mismo `LaserEffect` (y `BeamCaster`) que el láser, con otro arte y menos daño **[AJUSTAR]**.
- En cada tick agrega **congelamiento** al enemigo (`DamageInfo.freezeBuildup`, 10.1). Al acumular lo suficiente, el enemigo pasa al estado `Frozen` (14.1).
- Movimiento como el láser (quieto en el suelo) **[AJUSTAR]**.

#### Láser (Agente X)
- Es el láser descrito arriba, que pasa de Veyron a Agente X. No cambia nada del efecto.

#### Dron (Agente X)
- Al activarse aparece un **dron** (`CompanionDrone`) que flota cerca del jugador, siguiéndolo con retraso.
- Busca al enemigo más cercano dentro de un radio (por ejemplo, 6 u) y le dispara proyectiles pequeños cada cierto tiempo **[AJUSTAR]**. Usa `Projectile` y el pool del efecto.
- **Se destruye cuando el jugador recibe daño** (escucha `PlayerHealth.Damaged`), al morir y al terminar el nivel.
- Solo puede haber **1 dron**. Si ya hay uno, la habilidad no se activa y no gasta energía.
- Es la primera habilidad que deja algo vivo por su cuenta: `IsFinished` es verdadero en cuanto aparece el dron, y el dron vive fuera de `UsingAbility`.

#### Granada incendiaria (Agente X)
- Mismo `RubberGrenadeEffect` que la granada de goma. Al explotar, además de la `Explosion`, crea una **`DamageZone`** de fuego en el suelo (8.7): un área que dura unos segundos y daña por tick a los enemigos que la pisan **[AJUSTAR]**.
- No funde muros de metal: las interacciones van por habilidad, no por elemento (15.2).

#### Disco de energía (Neotisleo)
- Mismo `DrillEffect` que el taladro, con otro arte y **sin la interacción `Break`**: atraviesa enemigos, pero se detiene en los bloques destructibles sin romperlos.

#### Espada de luz (Neotisleo)
- Mismo `MeleeEffect` que la espada de fuego, con una hitbox más grande (por ejemplo, 2 × 1.5 u) **[AJUSTAR]**. Usa los clips `Torso_Melee*` de su hoja.

#### Munición eléctrica (Neotisleo)
- Proyectil de energía (`Projectile`) que al golpear un enemigo o un muro crea una `Explosion` eléctrica (radio ~1.5 u) **[AJUSTAR]**. **Aturde** a los enemigos que alcanza (`stunDuration`).
- No rompe bloques destructibles.

### 8.3 AbilityData (ScriptableObject)

```csharp
public enum AbilityActivation { Press, Hold }   // cómo se cobra la energía
public enum AbilityTorsoPose { ShootPose, ShootPoseForward, CustomClip }

[CreateAssetMenu(menuName = "Game/Ability")]
public class AbilityData : ScriptableObject
{
    public string id;                       // clave para el guardado
    public string displayName;
    public Sprite icon;                     // icono del HUD

    [Header("Activación")]
    public AbilityActivation activation;
    public float energyCost;                // Press: costo por uso
    public float energyPerSecond;           // Hold: consumo continuo
    public float cooldown;
    public int maxInstancesOnScreen;        // 0 = sin límite

    [Header("Jugador")]
    public bool usesAim;                    // ¿respeta AimDirection?
    public bool usableWhileCrouching;
    public float moveSpeedMultiplier = 1f;  // 0 = inmoviliza en el suelo

    [Header("Mundo")]
    public ObstacleInteraction[] obstacleInteractions;  // Break, Melt… (15.2)

    [Header("Presentación")]
    public AbilityEffect effectPrefab;
    public AbilityTorsoPose torsoPose;           // pose de disparo según el apuntado, siempre hacia adelante o clip propio
    public DirectionalClips customTorsoClips;    // solo con CustomClip: un clip por dirección
    public SfxData activateSfx;
    public SfxData loopSfx;                 // Hold: sonido en loop mientras está activa
}

[System.Serializable]
public struct DirectionalClips
{
    public AnimationClip forward, diagUp, up, down, crouch;

    // El clip de la dirección. Si falta, usa forward (o crouch si está agachada).
    public AnimationClip Get(AimDirection aim, bool crouching);
}
```

`ShootPoseForward` (forzar la pose hacia adelante) se conserva para habilidades futuras, pero ninguna la usa desde que la granada apunta en 4 direcciones.

| Habilidad | `activation` | `usesAim` | `usableWhileCrouching` | `moveSpeedMultiplier` | `obstacleInteractions` | `torsoPose` |
|---|---|---|---|---|---|---|
| Granada de goma | Press | Sí | Sí | 1 | — | `ShootPose` |
| Espada de fuego | Press | Sí | Sí | 1 | — | `CustomClip` (`Torso_Melee*`) |
| Lanzallamas | Hold | Sí | Sí | 0.5 | `Melt` | `ShootPose` |
| Taladro | Press | Sí | Sí | 1 | `Break` | `ShootPose` |
| Láser (Agente X) | Hold | Sí | Sí | 0 | — | `ShootPose` |
| Cuchillo | Press | Sí | Sí | 1 | — | `CustomClip` (`Torso_Melee*`) |
| Arma sónica / Ráfaga de burbujas | Press | Sí | Sí | 1 | — | `ShootPose` |
| Chorro de agua | Press | Sí | Sí | 1 | `Extinguish` | `ShootPose` |
| Rayo congelante | Hold | Sí | Sí | 0 | — | `ShootPose` |
| Dron | Press | No | Sí | 1 | — | `ShootPose` |
| Granada incendiaria | Press | Sí | Sí | 1 | — | `ShootPose` |
| Disco de energía | Press | Sí | Sí | 1 | — | `ShootPose` |
| Espada de luz | Press | Sí | Sí | 1 | — | `CustomClip` (`Torso_Melee*`) |
| Munición eléctrica | Press | Sí | Sí | 1 | — | `ShootPose` |

**Qué va en `AbilityData` y qué va en el efecto:** `AbilityData` solo tiene lo que leen **otros sistemas** (guardado, HUD, energía, FSM, animación, obstáculos). Los valores propios de cada habilidad (daño, radio, velocidad, alcance, mecha, rebote, calor por segundo, duración del bloqueo…) son **campos serializados de su `AbilityEffect`**, en su prefab. Regla para campos nuevos: si solo lo lee el efecto, va en el efecto. Así `AbilityData` no se llena de bools para casos especiales.

### 8.4 Efecto de la habilidad

Cada habilidad es un prefab con un componente que hereda de `AbilityEffect`. Así la FSM no necesita saber nada de cada habilidad.

```csharp
public abstract class AbilityEffect : MonoBehaviour
{
    // Al activar la habilidad.
    public abstract void Begin(AbilityContext ctx);

    // Cada frame mientras UsingAbility está activo, con el contexto actualizado.
    public virtual void Sustain(AbilityContext ctx, float dt) { }

    // Cuando el jugador suelta el botón de habilidad.
    public virtual void ButtonReleased() { }

    // UsingAbility sale cuando el efecto dice que terminó.
    public abstract bool IsFinished { get; }

    // Al terminar o al cancelarse (energía agotada, daño o dash).
    public virtual void End() { }
}

public struct AbilityContext
{
    public Transform owner;
    public Vector2 origin;          // FirePoint según AimDirection
    public Vector2 direction;
    public AimDirection aim;
    public int facing;
}
```

**El efecto decide cuándo termina** (`IsFinished`), no la FSM. Así un modo nuevo de habilidad no cambia `UsingAbility`:

| Comportamiento | Cuándo `IsFinished` es verdadero | Ejemplos |
|---|---|---|
| Por pulsación con bloqueo | Al pasar su tiempo de bloqueo (campo del efecto) | Granada, taladro, espada, cuchillo |
| Sostenida | Al soltar el botón (`ButtonReleased`) | Lanzallamas, láser |
| Cargada (posible en la V2) | Al soltar el botón y disparar la carga | — |

Una clase base `TimedAbilityEffect` implementa el bloqueo por tiempo y otra `HeldAbilityEffect` el botón sostenido, para no repetirlos en cada habilidad. Una habilidad que siga activa mientras el jugador hace otras cosas (un escudo, una postura) no cabe en `UsingAbility`; si la V2 la necesita, `PlayerAbilities` llevaría una lista de efectos activos aparte.

**Una instancia del efecto por habilidad** (implementado en el Hito 4): `PlayerAbilities` instancia el prefab de cada habilidad una sola vez, como hijo del jugador, al recibir el `CharacterData`. `UsingAbility` llama a `Begin`, `Sustain` y `End` sobre esa instancia.
- **Lo que la habilidad lanza sale de pools del propio efecto:** granadas, explosiones, taladros y llamas (`ComponentPool<T>` o `ProjectilePool`, en un contenedor en la raíz de la escena para que no sigan al jugador). Lo lanzado vive por su cuenta aunque el efecto ya haya terminado. `ActiveInstances` cuenta lo que hay en pantalla (para `maxInstancesOnScreen`).
- **Pegadas al jugador (espada, cuchillo):** la instancia se voltea con la dirección del jugador y activa su hitbox solo en los frames del tajo.
- **Sostenidas (lanzallamas, láser):** la instancia se coloca cada frame en el FirePoint según el contexto.

Implementaciones: `RubberGrenadeEffect` (con `RubberGrenade`), `MeleeEffect` (espada de fuego y cuchillo, con distintos valores y VFX), `FlamethrowerEffect`, `DrillEffect`, `LaserEffect` y `BlastEffect` (arma sónica y ráfaga de burbujas).

### 8.5 Estado `UsingAbility`

El estado es **el mismo para todas las habilidades**:

1. `Enter`: revisa la energía (`Press`: `energyCost` completo; `Hold`: al menos 0.1 s de uso) y cobra el costo si es `Press`. Llama a `Begin` y pone el torso según `torsoPose`: el clip propio (de pie o agachado), la pose de disparo hacia adelante o la pose de disparo según el apuntado.
2. `Tick`: si es `Hold`, descuenta `energyPerSecond × dt`. Recalcula la dirección y llama a `Sustain`. Si se soltó el botón, llama a `ButtonReleased` una vez.
3. Sale cuando `IsFinished` es verdadero, o si se acaba la energía o el jugador recibe daño. En todos los casos llama a `End` y vuelve a `Ready`.

Con esto, las habilidades por pulsación se comportan igual que antes (bloqueo por tiempo) y las sostenidas también (terminan al soltar el botón).

Reglas comunes:
- No se puede cambiar de habilidad mientras `UsingAbility` está activo.
- Recibir daño (`Hurt`) cancela cualquier habilidad sostenida.
- **Dash:** no se puede hacer dash durante una habilidad por pulsación. Durante una habilidad sostenida, el dash **la cancela**: se llama a `End`, la FSM de acción vuelve a `Ready` y el dash empieza en el mismo frame. Así el jugador nunca queda atrapado con el láser activo. Para volver a usar la habilidad hay que soltar el botón y presionarlo de nuevo después del dash.
- La locomoción lee `moveSpeedMultiplier` de la habilidad activa para limitar la velocidad.

### 8.6 Energía

- `PlayerEnergy`: `Current` (float), `Max`, `TrySpend(float)`, `Drain(float)`, `Refill(float)` y evento `Changed`.
- Internamente es un `float` (por el consumo por segundo). El HUD la muestra en **segmentos enteros**, como la barra de Mega Man.
- Si no hay energía suficiente, la habilidad no se activa y suena un SFX de error.
- **Recarga:**
  - Se rellena por completo **al morir** (al reaparecer en el checkpoint).
  - Se recupera con los **pickups de energía que sueltan los enemigos** (ver sección 10).
  - Al empezar cada nivel, la energía está llena.
  - Los checkpoints **no** la recargan.

### 8.7 Piezas de combate reutilizables

Las habilidades se construyen con piezas que también usan los enemigos y el disparo básico:

| Pieza | Para qué sirve | La usan |
|---|---|---|
| `Projectile` | Movimiento por código, colisión con raycast, pool. Opciones: `pierceCount` y golpear una sola vez a cada objetivo | Buster, taladro, enemigos |
| `DamageHitbox` | Zona rectangular que daña en dos modos: **una vez por objetivo** por activación, o **por tick** cada X segundos por objetivo. No es un collider: consulta con `Physics2D.OverlapBox` usando su tamaño y su transform, así que se puede girar, voltear y activar en el mismo frame. También puede destruir proyectiles (`projectileMask`) | Espada, cuchillo, lanzallamas |
| `BeamCaster` | Raycast con longitud variable y daño por tick a todo lo que toca | Láser |
| `Explosion` | `OverlapCircleAll` de un solo golpe + VFX | Granadas, munición eléctrica, enemigos (Mr. Bombastic) |
| `DamageZone` | Área que **se queda** en el suelo un tiempo y daña por tick a una capa (enemigos o jugador). Con su sprite animado y su tiempo de vida | Granada incendiaria; charcos de ácido (Alterado, Mr. Bombastic) y gotas de veneno (Gosarou) |
| `CompanionDrone` | Entidad que sigue al jugador, busca un objetivo cercano y le dispara con `Projectile` | Dron de Agente X |

Otras piezas: `ComponentPool<T>` (pool genérico con `IPoolable`), `DirectionalFrames` (frames horizontal, diagonal y vertical con el volteo de cada dirección) y `IAbilityObstacle` (15.2).

Para el daño por tick se lleva un registro por objetivo (`Dictionary<IDamageable, float>` con el próximo momento en que puede recibir daño). Así un enemigo dentro de la llama no recibe daño en cada frame.

### 8.8 Arte necesario

Todo el arte de las habilidades usa PPU 16. En pixel art no se rotan los sprites 45°, porque los píxeles se deforman: las piezas que apuntan en diagonal tienen **su propio sprite diagonal**. La dirección hacia abajo es el sprite vertical volteado.

**Archivos (Hito 4):** cada animación es un PNG en tira horizontal, con frames del mismo tamaño, en `Art/Abilities/<Habilidad>/`. El setup crea placeholders con estos nombres y tamaños; para poner el arte final se reemplaza el PNG con el mismo nombre y tamaño de frame (puede tener más frames) y se vuelve a ejecutar **Game → Setup → 2. Prefabs**, que corta la tira de nuevo (salta las celdas vacías) y asigna los frames a los prefabs:

| Archivo | Frame | Frames del placeholder |
|---|---|---|
| `Grenade/Grenade_Spin.png` | 8×8 | 4 |
| `Grenade/Explosion.png` | 48×48 (radio de 1.5 u) | 5 |
| `FireSword/SwordSlash_Horizontal.png`, `_Diagonal.png`, `_Vertical.png` | 32×32 | 3 |
| `Flamethrower/FlamePuff.png` | 16×16 | 6 (4 de fuego + 2 de humo; `smokeFrames` en el efecto) |
| `Drill/Drill_Horizontal.png`, `_Diagonal.png`, `_Vertical.png` | 16×16 | 2 |
| `Laser/Laser_Beam_0.png`, `Laser_Beam_1.png` | 16×6, textura (no sprite) con *Repeat* | 1 cada una; se alternan |
| `Laser/Laser_Origin.png` | 12×12 | 2 |
| `Laser/Laser_Impact.png` | 12×12 | 3 |

Las horizontales se dibujan mirando a la derecha, las diagonales hacia arriba-derecha y las verticales hacia arriba. Los prefabs de efecto (`Prefabs/Abilities/Ability_*.prefab`) se crean una sola vez para no pisar los valores ajustados; el setup solo reasigna su arte, sus prefabs y sus sonidos.

| Elemento | Sprites | Direcciones |
|---|---|---|
| Iconos del HUD | 15 iconos de 16×16 (3 por personaje) | — |
| Coleccionable de habilidad | Sprite animado (8.9) | — |
| Clips del torso | `Torso_Melee`, `Torso_Melee_DiagUp`, `Torso_Melee_Up`, `Torso_Melee_Down`, `Torso_Melee_Crouch` en cada hoja | Las 4 + agachado |
| Granada | Giro (4 frames) + explosión | — |
| Espada | Tajo de llamas (VFX) | Horizontal, diagonal, vertical (abajo = vertical volteado) |
| Lanzallamas | `FlamePuff`: una llama de 16×16 con ~6 frames (2 al aparecer, 2 ardiendo, 2 de humo) + una variante opcional | Ninguna: es redonda y no se rota |
| Taladro | Proyectil girando | Horizontal, diagonal, vertical |
| Láser | Origen y impacto (animados) + textura del rayo que se repite | Una sola textura para todas las direcciones |
| Cuchillo | Tajo (VFX corto) | Horizontal, diagonal, vertical (abajo = vertical volteado) |
| Arma sónica | Boom sónico: onda que se abre en abanico (3–4 frames) | Horizontal, diagonal, vertical |
| Ráfaga de burbujas | Abanico de burbujas que revientan (3–4 frames) | Horizontal, diagonal, vertical |
| Chorro de agua | Gota en vuelo + salpicadura + vapor al apagar fuego | — |
| Rayo congelante | Como el láser: origen, impacto y textura del rayo | Una sola textura |
| Dron | Dron flotando (2–4 frames), su bala y su destrucción | — |
| Granada incendiaria | Granada + explosión + fuego en el suelo (loop) | — |
| Disco de energía | Disco girando | Horizontal, diagonal, vertical |
| Espada de luz | Tajo (VFX) | Horizontal, diagonal, vertical |
| Munición eléctrica | Bola de energía + explosión eléctrica | — |
| Enemigo congelado | Bloque de hielo encima del sprite (genérico, escalable por tamaño) | — |

**Construcción del láser con `LineRenderer`:**

Funciona en pixel art porque la Pixel Perfect Camera usa *Upscale Render Texture*: la línea se dibuja a 320×180 junto con el resto de la escena, y en diagonal sale como una escalera de píxeles. Un rayo es casi uniforme a lo largo, así que su textura casi no se deforma en diagonal.

- Material `Sprite-Unlit-Default` (o `Sprite-Lit-Default` para luces 2D).
- Textura del rayo con `Filter Mode = Point` y `Wrap Mode = Repeat`; `Texture Mode = Tile`, con la escala ajustada para que se repita cada 16 px.
- Ancho en múltiplos de 1/16 (por ejemplo, 0.375 u = 6 px) y extremos redondeados a la rejilla de píxeles, para que el grosor no cambie entre frames.
- La longitud sale del raycast: `SetPosition(1, puntoDeImpacto)`.
- `sortingLayerName` y `sortingOrder` asignados por código.
- Origen e impacto son sprites animados en los extremos.
- Animación: desplazar el *offset* de la textura o alternar entre 2–3 texturas. Si el ancho "pulsa", que cambie en saltos de píxel entero.
- En diagonal, el ancho se mide perpendicular a la línea, así que se ve un poco más grueso (6 px → unos 8 px en vertical). Si molesta, usar un ancho menor en diagonal.

> Esto depende de *Upscale Render Texture*. Si algún día se desactiva, el `LineRenderer` se dibujaría suavizado a la resolución nativa y dejaría de verse como pixel art.

### 8.9 Obtención: coleccionables en el escenario

Las habilidades **no las entregan los jefes**. Cada habilidad es un **coleccionable único** (`AbilityPickup`) escondido en algún nivel: **15 en total**, 3 por personaje, repartidos entre los niveles 2 a 10. Dónde va cada uno está por definir **[DECIDIR]** (17.2).

**Cualquier personaje recoge cualquier coleccionable.** La habilidad se desbloquea **para su dueño**: si Rosslyn toma el taladro, el taladro queda desbloqueado para Veyron. Así se pueden poner coleccionables detrás de obstáculos de otros personajes (por ejemplo, el taladro detrás de un muro de fuego que solo apaga Aquamarine). Los coleccionables de personajes que aún no se unen también se pueden recoger: quedan guardados para cuando lleguen.

**El ciclo entre personajes:**
- El **taladro (Veyron)** rompe bloques destructibles, el **lanzallamas (Rosslyn)** funde muros de acero y el **chorro de agua (Aquamarine)** apaga el fuego (15.2). Cada uno abre caminos opcionales que los demás personajes no pueden abrir.
- Detrás de esos caminos hay coleccionables y prisioneros de cualquier personaje. Para ver todo un nivel hay que **rejugarlo con varios personajes**.
- Aquamarine se une después del Bosque en llamas, así que los muros de fuego de los niveles 2 a 5 solo se abren al repetirlos con ella.

**Reglas de diseño:**
- **Las habilidades que abren caminos** (taladro, lanzallamas y chorro de agua) pueden estar detrás de obstáculos de otros personajes, pero **al menos una debe poder alcanzarse sin ninguna habilidad**; si no, la cadena no puede empezar.
- **Sin dependencias circulares:** ninguna habilidad puede quedar detrás de un obstáculo que solo se abre con ella misma, directa o indirectamente (por ejemplo, el taladro tras un fuego y el chorro tras un bloque).
- Cada `AbilityPickup` y cada `Prisoner` declara qué interacciones hacen falta para llegar a él (`requires`: `Break`, `Melt`, `Extinguish`). `Game/Validate` simula el orden de los niveles y avisa si algo no se puede alcanzar.
- Como todos los coleccionables son opcionales, ninguna misión ni jefe puede depender de tener alguno (14.2).

**Al recogerlo:**
1. El jugador pasa a `Disabled` un momento (1–1.5 s **[AJUSTAR]**): suena un jingle, aparecen el icono, el nombre de la habilidad y su dueño ("Taladro — Veyron").
2. Se desbloquea para su dueño. Si el dueño es el personaje que se está jugando, queda seleccionada y la energía se llena.
3. **Se guarda en ese momento** (15.4). Solo se guarda el progreso, no el estado del nivel. Así no se pierde aunque el jugador abandone la misión o cierre el juego.

**Ya recogido:** el coleccionable no vuelve a aparecer, se juegue con quien se juegue.

**Componentes y datos:**
- `AbilityPickup` (en la escena): referencia a un `AbilityData`. Al cargar la escena revisa en `GameManager` si el dueño ya lo tiene; si es así, no aparece.
- El dueño de una habilidad es el `CharacterData` que la tiene en `abilities`. Cada `AbilityData` pertenece a un solo personaje.
- `MissionData.abilityPickups` lista los coleccionables de esa misión, para que la pantalla de selección muestre cuáles faltan sin cargar la escena (15.1).
- **Validación en el editor** (menú `Game/Validate`): cada `AbilityData` está en exactamente un `MissionData.abilityPickups`, y cada escena tiene un `AbilityPickup` por cada habilidad de su lista.

### 8.10 Agregar una habilidad nueva

Con el diseño de esta sección, agregar una habilidad (en la V1 o en la V2) es casi todo trabajo de datos y arte:

1. Crear su `AbilityData` (8.3) con un `id` único.
2. Crear el prefab con su `AbilityEffect`. Heredar de `TimedAbilityEffect` o `HeldAbilityEffect` y armarlo con las piezas de 8.7. Solo es código nuevo si su comportamiento no existe.
3. Si interactúa con obstáculos, asignarle sus `ObstacleInteraction`. Si es un tipo de obstáculo nuevo, crear el asset y su Tilemap (15.2).
4. Si empuja o aturde, llenar `knockback` / `stunDuration` en su daño (10.1).
5. Si necesita un clip propio del torso, agregar su entrada al `SpritesheetLayout` y dibujarlo en la hoja de su personaje (4.2.1).
6. Agregarla a `CharacterData.abilities` de su dueño (5.4).
7. Ponerla como coleccionable: `AbilityPickup` en la escena y en `MissionData.abilityPickups` (8.9).
8. Icono del HUD, sonidos (`SfxData`) y ejecutar `Game/Validate`.

El guardado no cambia: las habilidades se guardan por `id`.

---

## 9. Animators

### 9.1 Datos que lee `PlayerAnimation`

Como los estados se eligen por código con `Animator.Play()`, los Animators **no usan parámetros**. `PlayerAnimation.Sync()` lee directamente del contexto:

| Dato | Fuente | Lo usan |
|---|---|---|
| Velocidad horizontal | `Motor` | Piernas y torso (idle o run) |
| En el suelo / velocidad Y | `Motor` | Piernas (salto o caída) y torso |
| Agachado | `Locomotion.IsCrouching` | Piernas y torso |
| Dash | `Locomotion.IsDashing` | Piernas y torso |
| Dirección de apuntado | `Action.AimDir` | Torso |
| Shoot pose | `Action.IsInShootPose` (también activo durante habilidades sostenidas) | Torso |
| Clip de habilidad | `AbilityData.customTorsoClips.Get(dirección, agachada)` de la habilidad que se acaba de usar (`torsoPose = CustomClip`), con la dirección fijada al activarla | Torso |
| Pose forzada hacia adelante | `AbilityData.torsoPose = ShootPoseForward` en la habilidad en curso | Torso |

Los hashes de los estados se cachean (`Animator.StringToHash("Torso_Run")`). Nunca se usan strings en runtime.

### 9.2 Lógica del torso

Prioridad, de mayor a menor:

1. Dash → `Torso_Dash`
2. Habilidad con clip propio en curso → el clip de su dirección: `Torso_Melee`, `Torso_Melee_DiagUp`, `Torso_Melee_Up`, `Torso_Melee_Down` o `Torso_Melee_Crouch` (hasta que termina el clip)
3. Shoot pose con pose forzada hacia adelante (`ShootPoseForward`; ninguna habilidad la usa por ahora) → `Torso_Shoot_Crouch` si está agachada, si no `Torso_Shoot_Forward`
4. Shoot pose + agachado → `Torso_Shoot_Crouch`
5. Shoot pose + apuntando `Up` → `Torso_Shoot_Up`
6. Shoot pose + apuntando `DiagUp` → `Torso_Shoot_DiagUp`
7. Shoot pose + apuntando `Down` → `Torso_Shoot_Down`
8. Shoot pose → `Torso_Shoot_Forward`
9. Apuntando `Up` (sin disparar) → `Torso_Aim_Up`
10. Apuntando `DiagUp` (sin disparar) → `Torso_Aim_DiagUp`
11. Estado base según la locomoción: `Torso_Idle`, `Torso_Run`, `Torso_JumpUp` / `Torso_Fall` (según la velocidad Y), `Torso_Crouch`

Mientras una habilidad sostenida (lanzallamas o láser) está activa, el shoot pose se mantiene, así que el torso se queda en la pose de disparo de la dirección actual. El taladro y la granada activan un shoot pose normal, igual que el disparo básico.

**Implementación con `Play()` por código:** `PlayerAnimation` evalúa esta lista en cada frame y, si el estado elegido cambió, llama a `Animator.Play(hash)`. Es válido en pixel art porque no hay blend, y es mucho más fácil de depurar que una red de transiciones.

```csharp
int next = ChooseTorsoState(ctx);   // aplica la lista de prioridades
if (next != currentTorsoState)
{
    // Conservar el ciclo al cambiar entre poses con los mismos frames (p. ej. Run ↔ Aim_DiagUp)
    float t = KeepsCycle(currentTorsoState, next)
        ? torso.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f
        : 0f;
    torso.Play(next, 0, t);
    currentTorsoState = next;
}
```

Las piernas siguen el mismo patrón con su propia lista: dash → agachado → en el aire (subiendo o cayendo) → run → idle.

### 9.3 Configuración de los Animators

- Cada Animator tiene **un estado por clip, sin flechas de transición**. El nombre del estado es igual al del clip.
- Los clips en loop tienen *Loop Time* activado. Los `Torso_Melee*`, los `Torso_Shoot_*` y los `FullBody_*` no.
- Para saber si un clip `Torso_Melee*` terminó, se revisa `normalizedTime >= 1` en `PlayerAnimation`.
- No hace falta `AnimatorOverrideController`: los clips de habilidad son estados normales.
- El cambio entre poses en loop con la misma cantidad de frames (por ejemplo, `Torso_Run` y `Torso_Aim_DiagUp` corriendo) conserva el `normalizedTime`, como en el código de arriba.
- Las poses de disparo (`Torso_Shoot_*`) no tienen loop: el movimiento del arma se reproduce una vez y se queda en el último frame hasta que termina el *shoot pose*. **Cada disparo reinicia el clip desde el frame 0** (`PlayerAnimation` escucha `PlayerBuster.Fired`), aunque el estado del torso no cambie.

### 9.4 Animation Events

Los clips solo marcan **momentos**: `OnFootstep`. `PlayerAnimation` lo recibe y lo reenvía como evento C#.

Los frames activos del tajo **no** usan Animation Events (decisión del Hito 4): los define `MeleeEffect` con una ventana de frames (8.2). Así no hay que poner eventos en los 5 clips de cada personaje y la espada funciona aunque falte un clip.

---

## 10. Vida, daño y pickups

### 10.1 Vida y daño

- **Barra de vida de 28 puntos** por defecto, como en Mega Man. Cada personaje define la suya (`CharacterData.maxHealth`). Se rellena al reaparecer.
- **Vidas infinitas:** al morir se reaparece en el último checkpoint. No hay Game Over ni contador de vidas.
- Al recibir daño: estado `Hurt` con knockback de 0.3 s, luego i-frames de 1.5 s **[AJUSTAR]**.
- **Parpadeo de i-frames:** se activan y desactivan los SpriteRenderers del jugador cada pocos frames. No requiere shader.
- **Flash de daño en enemigos:** shader `Game/Sprite-Lit-Flash` (`Art/Shaders/Sprite-Lit-Flash.shader`), una copia del `Sprite-Lit-Default` de URP con `_FlashColor` y `_FlashAmount` que mezclan el sprite con blanco. Lo controla `DamageFlash` con `MaterialPropertyBlock`. Se escribió en HLSL en lugar de Shader Graph porque es un cambio de dos líneas sobre el shader de URP. Si se actualiza URP, conviene volver a copiar el shader original y repetir el cambio.
- Durante los i-frames se ignora el daño por contacto y por proyectiles, pero **no** los peligros que matan al instante (fosos, pinchos).

```csharp
public interface IDamageable
{
    void TakeDamage(DamageInfo info);
}

public struct DamageInfo
{
    public int amount;
    public Vector2 direction;
    public GameObject source;
    public bool instantKill;     // fosos, pinchos
    public float knockback;      // fuerza de empuje (u/s); 0 = ninguno
    public float stunDuration;   // segundos de aturdimiento; 0 = ninguno
    public float freezeBuildup;  // congelamiento que suma este golpe; 0 = ninguno
}
```

- `knockback` y `stunDuration` son **opcionales**: casi todo el daño los deja en 0. Los valores salen de quien hace el daño (el campo del efecto o del proyectil).
- **Cada receptor decide cómo reacciona.** `EnemyData` tiene `knockbackResistance` (0–1) y `canBeStunned` (14.1). Los jefes, por defecto, ignoran el empuje y el aturdimiento **[AJUSTAR]**. El jugador ya tiene su knockback fijo en `Hurt` (6.2) y lo usa en lugar de estos campos.
- **Congelamiento** (rayo congelante, 8.2): cada golpe con `freezeBuildup` suma a un medidor del enemigo, que baja solo con el tiempo. Al llegar a su umbral (`EnemyData.freezeThreshold`), el enemigo pasa a `Frozen` (14.1) durante unos segundos **[AJUSTAR]**. Los jefes no se congelan.
- Si hace falta otro efecto (quemar en el tiempo, envenenar…), se agrega aquí un campo opcional igual. Si llegan a ser muchos, se migra a una lista de efectos de estado.

### 10.2 Pickups

| Pickup | Efecto |
|---|---|
| Vida pequeña / grande | Recupera vida |
| Energía pequeña / grande | Recupera la barra compartida de habilidades |

No hay pickup de vida extra, porque las vidas son infinitas.

Los enemigos sueltan pickups según una tabla de probabilidades en `EnemyData`. Los pickups definidos por datos (`PickupData`) desaparecen tras unos segundos si los soltó un enemigo.

---

## 11. Física y capas

### 11.1 Capas

| Capa | Uso |
|---|---|
| `Ground` | Suelo sólido |
| `AbilityObstacle` | Obstáculos que se destruyen con habilidades: bloques destructibles, muros de acero y los que se agreguen después (15.2). Sólidos para todo lo demás |
| `Platform` | Plataformas atravesables (PlatformEffector2D) |
| `Hazard` | Pinchos, fosos y otros peligros |
| `Player` | Collider físico del jugador |
| `PlayerHurtbox` | Recibe daño |
| `PlayerProjectile` | Disparo básico y habilidades |
| `Enemy` | Collider físico de los enemigos |
| `EnemyHurtbox` | Recibe daño |
| `EnemyProjectile` | Balas enemigas |
| `Pickup` | Vida y energía |

La matriz de colisiones se configura para que cada proyectil solo detecte la hurtbox contraria, `Ground` y `AbilityObstacle`. Además, la hitbox de la espada detecta `EnemyProjectile` para destruir proyectiles.

### 11.2 Movimiento del jugador

- `Rigidbody2D` dinámico con `Interpolate`, `Collision Detection = Continuous` y rotación congelada.
- La velocidad se asigna directamente (`linearVelocity`) desde el estado actual. No se usa `AddForce`.
- La gravedad la aplica el código (con `gravityScale = 0`), calculada desde la altura de salto (ver 6.3).
- Material físico sin fricción en el collider del jugador, para no pegarse a las paredes.

---

## 12. Input

Action Map `Gameplay`:

| Acción | Tipo | Teclado | Gamepad |
|---|---|---|---|
| `Move` | Vector2 | WASD / flechas | Stick izquierdo / D-pad |
| `Jump` | Button | Espacio | A / Cruz |
| `Fire` | Button | J | X / Cuadrado |
| `Ability` | Button | K | Y / Triángulo |
| `Dash` | Button | L / Shift izquierdo | B / Círculo o RT |
| `AbilityNext` | Button | E | RB / R1 |
| `AbilityPrev` | Button | Q | LB / L1 |
| `Pause` | Button | Esc | Start |

Action Map `UI`: navegación de menús (lo da Unity por defecto).

Action Map `Dialogue`: `Advance` (Espacio, J, Enter / A, X) y `Skip` (Esc / Start). Solo está activo mientras hay una escena o un diálogo (ver 15.7).

`PlayerInputReader` convierte esto en un `InputSnapshot` por frame:
- Dirección discreta (-1, 0, 1) en cada eje, con *deadzone* para el stick.
- Buffers con tiempo de vida para `JumpPressed`, `FirePressed`, `AbilityPressed` y `DashPressed`.
- `JumpHeld` para la altura variable del salto.

**Para PC:**
- **Remapeo de controles** en el menú de opciones, con `InputActionRebindingExtensions.PerformInteractiveRebinding`. Los cambios se guardan con `SaveBindingOverridesAsJson`.
- Iconos de botones en la UI según el último dispositivo usado (teclado o gamepad).

---

## 13. Cámara

- Cinemachine Camera con *Position Composer* y *dead zone* horizontal.
- El objetivo de la cámara se asigna por código (`SetTarget`), no se deja fijo al jugador en el editor. Así se puede cambiar a un vehículo (18.4).
- *Look ahead* ligero en la dirección en la que mira el jugador, más notorio durante el dash **[AJUSTAR]**.
- `CinemachineConfiner2D` por sección del nivel.
- **Puertas de jefe:** al entrar a la sala del jefe, la cámara se fija a la sala (20 tiles de ancho = una pantalla).
- Pixel Perfect Camera en la cámara principal, con la extensión `CinemachinePixelPerfect` en la Cinemachine Camera.

---

## 14. Enemigos y jefes

### 14.1 Enemigos

El arte de los enemigos básicos y de los vehículos está en `Enemies.png` (raíz del proyecto). Un sprite completo por enemigo, sin dividir, con pivote en los pies y PPU 16. Los más grandes (escarabajo, tank, araña) siguen la misma regla.

#### Lista

| Enemigo | Comportamiento | Lo que necesita |
|---|---|---|
| **Soldados** | Varias variantes; cada una con un disparo distinto | `EnemyWeaponData` por variante |
| **Soldado carreta** | Uno empuja la carreta de un lado a otro mientras el otro dispara desde ella. Si uno muere, el otro sigue disparando | Enemigo compuesto (dos partes independientes) |
| **Abuelita** | Camina despacio hacia el jugador; al tenerlo de frente le dispara con su andadera y el retroceso la lanza y la mata | Muerte por su propio ataque |
| **Soldado carreola** | Avanza despacio; al acercarse al jugador, el soldado sale de la carreola y dispara | Enemigo compuesto (la carreola suelta al soldado) |
| **Furro** | Dispara de lejos; si el jugador se acerca, ataca cuerpo a cuerpo con la navaja del rifle | Cambio de ataque por distancia |
| **Escarabajo** | Lanza cañonazos en parábola; si el jugador se acerca, ataca con las patas delanteras | Proyectil en parábola + cuerpo a cuerpo |
| **Robo-dron** | Lanza misiles al abrir el pecho. Al morir, la cabeza se separa y sigue disparando balas láser | Al morir, crea otro enemigo (la cabeza) |
| **El de las recargas** | Intenta arrollar al jugador, se detiene y dispara | Embestida + disparo |
| **Mantis atea** | Lanza balas de la boca y salta hacia el jugador. Si lo atrapa, lo estrangula; el jugador se suelta presionando disparo repetidamente | Estado `Grabbed` del jugador (6.2) |
| **Araña cañón** | Dispara cañonazos y se mueve muy rápido hacia adelante y atrás | Movimiento de vaivén rápido |
| **Araña spawner** | Genera cabezas de robo-dron | Generador de enemigos con límite |
| **Tank** | Dispara en línea recta en horizontal, diagonal y hacia arriba | Arma con 3 direcciones |
| **Robot con escudo** | El escudo es una entidad aparte. El robot ataca cada cierto tiempo y expone su cuerpo; el escudo también se puede destruir para dejarlo expuesto | Enemigo compuesto (escudo con vida propia que bloquea balas) |
| **Robot volador** | Se mueve entre varios puntos de vuelo y dispara desde sus manos | Vuelo por puntos |
| **Alterado** | Camina despacio hacia el jugador y escupe ácido que deja un charco que daña | `DamageZone` (8.7) |
| **Mr. Bombastic** | Al acercarse mucho al jugador se infla y explota; la explosión daña y deja un charco de ácido | Autodestrucción + `Explosion` + `DamageZone` |
| **Fire bat** | Volador que lanza bolas de fuego | Vuelo + disparo |
| **Mole** | Persigue al jugador y, al tenerlo cerca, intenta aplastarlo con los brazos | Persecución + cuerpo a cuerpo |
| **Medusa** | Vuela hacia el jugador; daña al contacto con los tentáculos, pero se puede **rebotar en su cabeza** | Superficie de rebote (`BounceTop`) |
| **Oruga** | Se arrastra por el suelo o por el techo; las del techo se dejan caer cuando el jugador pasa debajo. Dañan al contacto | Movimiento por el techo + caída |
| **Moto** | Como el de las recargas, con disparo más rápido | Variante de datos del de las recargas |
| **Torreta** | Apunta en 8 direcciones y dispara hacia el jugador | Arma que apunta en 8 direcciones |

Qué enemigos van en cada nivel está por definir **[DECIDIR]** (17.2).

#### Arquitectura: enemigos por piezas

Con 22 enemigos, cada uno no puede ser una clase hecha a mano. Se arman con **piezas reutilizables**, igual que las habilidades:

- **`EnemyData`** (ScriptableObject): vida, daño por contacto, velocidad, rango de detección, tabla de pickups, `knockbackResistance`, `canBeStunned`, `canBeFrozen` y `freezeThreshold`.
- **`EnemyWeaponData`** (ScriptableObject): proyectil, daño, cadencia, ráfaga y **modo de apuntado**: fijo hacia adelante, 3 direcciones (tank), 8 direcciones hacia el jugador (torreta), directo al jugador o en parábola (escarabajo). Los soldados son el mismo enemigo con distinto `EnemyWeaponData`.
- **Biblioteca de estados** para la FSM de cada enemigo: `Patrol`, `Chase`, `Charge` (embestida), `FlyToPoints`, `Shoot`, `Melee`, `Jump`, `CeilingCrawl` / `Drop`, `SelfDestruct`, `Stunned`, `Frozen` y `Dead`. Cada prefab elige sus estados y transiciones; un enemigo nuevo es casi siempre una combinación de estados existentes.
- **Al morir** (`OnDeath`, configurable): soltar pickups, `Explosion`, dejar una `DamageZone` (Mr. Bombastic) o **crear otro enemigo** (la cabeza del robo-dron).
- **Enemigos compuestos** (`EnemyGroup`): varias partes, cada una con su propia vida (`IDamageable`). El grupo coordina el comportamiento y decide qué pasa cuando muere una parte. Lo usan el soldado carreta, el soldado carreola y el robot con escudo (el escudo bloquea proyectiles y se puede destruir).
- **Generadores** (`EnemySpawner`): crean enemigos del pool con un máximo vivo al mismo tiempo (araña spawner).
- **`BounceTop`**: si el jugador cae sobre la parte de arriba, rebota con `LocomotionFSM.Bounce` (el mismo del tajo hacia abajo) y no recibe daño (medusa).
- **`Grab`**: atrapa al jugador y lo pasa al estado `Grabbed` (6.2) (mantis atea).

**Estados especiales:**
- `Stunned`: se activa cuando el daño trae `stunDuration` y el enemigo puede aturdirse (10.1); al terminar vuelve a `Alert`.
- `Frozen`: se activa al llenar el medidor de congelamiento (10.1). El enemigo no se mueve ni ataca, y se ve un bloque de hielo encima. Al descongelarse vuelve a `Alert`. Un enemigo congelado **sirve de plataforma**, como en Mega Man: su collider se vuelve sólido para el jugador mientras dura el congelamiento.

### 14.2 Jefes

| Nivel | Jefe | Patrones |
|---|---|---|
| 2. Bosque | **Excavadora de combate** | Un cañón dispara granadas en parábola cada cierto tiempo. La garra cae sobre el jugador; al levantarla saca una roca que deja caer sobre él, y el ciclo vuelve a empezar |
| 3. Minas | **Ayahuastli** | Criatura con poderes eléctricos que alterna entre: cubrirse de energía y correr hacia el jugador; lanzar bolas de energía que al tocar el suelo explotan en un rayo vertical; lanzar una onda de energía que avanza por el suelo |
| 4. Montaña | **Robot de batalla** | **[DECIDIR]** (en diseño) |
| 5. Bosque en llamas | **Wolfang** | Hombre lobo del espacio: saca un tentáculo de la boca y ataca en horizontal; gira como el demonio de Tasmania cargando contra el jugador; arranca pedazos del suelo y los lanza; se hace bola y va por el suelo o rebota |
| 6. Laboratorio | **Gosarou** | Insecto alienígena: suelta insectos pequeños que disparan; si el jugador se acerca, ataca con las patas traseras; lanza gotas de veneno que quedan en el suelo y dañan; lanza sus navajas |
| 7. Tren militar | **Zaipoc** | Ser de energía pura: se convierte en láser y rebota contra las paredes (inmune en ese estado); se vuelve bola de energía y lanza ráfagas; lanza cristales que se clavan en el suelo como picos que el jugador debe romper para despejar la zona |
| 8. Castillo | **Grallet** | **[DECIDIR]** (en diseño) |
| 9. Ciudad alienígena | **Narabee** | **[DECIDIR]** (en diseño) |
| 10. Torre alienígena | **Coliel** (jefe final) | **[DECIDIR]** (en diseño) |

El Simulador (nivel 1) no tiene jefe: termina en una línea de meta (15.1).

**Piezas que piden los jefes** (se suman a las de los enemigos):
- **Estado inmune** (`invulnerable` en la fase o estado): Zaipoc en forma de láser.
- **Objetos que el jugador debe destruir** durante la pelea: los cristales de Zaipoc son pequeños enemigos (`IDamageable`) que bloquean zonas del suelo.
- **Invocaciones:** los insectos de Gosarou usan `EnemySpawner`.
- **Áreas en el suelo:** veneno de Gosarou y onda de Ayahuastli (`DamageZone` y `Projectile` por el suelo).
- **Objetos que caen:** roca de la Excavadora, pedazos de suelo de Wolfang (proyectiles con gravedad).
- **Rebote contra paredes:** Zaipoc y Wolfang en bola.

**Reglas comunes:**
- FSM con **fases** según su vida (por ejemplo, cambia de patrón por debajo del 50%).
- **Ya no entregan habilidades**: se encuentran en el escenario (8.9).
- **Sin debilidades:** todos los jefes reciben el mismo daño de cada fuente. El reto está en sus patrones, no en elegir la habilidad correcta.
- Las habilidades son opcionales y por personaje, así que no se sabe cuáles tiene el jugador al llegar a un jefe. Todos los jefes deben poder vencerse **solo con el disparo básico de cualquier personaje**; las habilidades ayudan, pero nunca son obligatorias. Por el alcance corto de Veyron (sección 7), cada jefe debe tener momentos en los que se pueda llegar cerca de él sin recibir daño inevitable.
- Los jefes **no se congelan ni se aturden**.
- **Escena del jefe** (`MissionData.bossCutscene`): al cruzar la puerta de la sala, la cámara se fija y empieza una escena con el mismo sistema de 15.7:
  1. La cámara muestra al jefe en la sala (`CameraStep`).
  2. Se abre la novela visual con un **diálogo corto** entre el jefe y el escuadrón (y, si hace falta, el General por radio).
  3. Se cierra la novela visual y, ya en el nivel, el jefe hace su pose de presentación (`Boss_Intro`) mientras se llena su barra de vida, como en Mega Man. Empieza la pelea.
- **Al reintentar:** si el jugador muere en la pelea y vuelve desde el checkpoint, la escena se repite **sin el diálogo**: solo cámara, pose y barra. El diálogo completo se ve una vez por intento de nivel. Los pasos de diálogo se marcan como `firstTimeOnly` (15.7).
- Toda la escena se puede saltar. Al saltarla se llega directo al inicio de la pelea, con la barra llena.
- Cada jefe tiene su `SpeakerData` (nombre y retratos por emoción, 15.7), una ilustración para su escena y un clip `Boss_Intro`.
- **Música:** durante el diálogo suena una pista de tensión o silencio **[AJUSTAR]**; el tema del jefe (`bossMusic`) empieza cuando se llena la barra.
- Al vencerlo: el jugador pasa a `Disabled`, se marca el nivel como completado, sigue la escena de cierre y se guarda la partida.

---

## 15. Juego, misiones y guardado

### 15.1 Progresión: 10 niveles en orden

Los niveles se juegan **en orden**. Cada uno es una misión que asigna el General Neotisleo (1.4). Al completar un nivel se abre el siguiente, y los completados se pueden repetir.

| # | Nivel | Jefe (14.2) | Vehículo (18) | Notas |
|---|---|---|---|---|
| 1 | Simulador de combate (Misión 0) | — (termina en una línea de meta) | — | — |
| 2 | Bosque | Excavadora de combate | — | — |
| 3 | Minas | Ayahuastli | Exoesqueleto | — |
| 4 | Montaña | Robot de batalla | Robotanque | — |
| 5 | Bosque en llamas | Wolfang | — | Después se une Aquamarine |
| 6 | Laboratorio | Gosarou | Exoesqueleto | — |
| 7 | Tren militar | Zaipoc | Exoesqueleto | Después se une Agente X |
| 8 | Castillo | Grallet | — | — |
| 9 | Ciudad alienígena (Coleópolis) | Narabee | — | Después, si están los 24 prisioneros, escena en la que se une Neotisleo |
| 10 | Torre alienígena | Coliel (jefe final) | Robotanque | — |

```
Boot → Menú principal → Partida nueva → Simulador (Misión 0, Rosslyn) → Selección de nivel
                     └── Continuar / Cargar → Selección de nivel ⇄ Selección de personaje → Nivel → Selección de nivel
                                    └── (Torre alienígena completada) → Créditos

Nivel: Escena de inicio → Nivel (coleccionables opcionales, vehículos en algunos) → Escena del jefe (diálogo + pose) → Jefe → Escena de cierre
```

#### Misión 0: el simulador

Es lo **primero que ve el jugador** al empezar una partida nueva. En la historia, Rosslyn entrena en un **simulador** antes de recibir misiones reales. En el juego es una **misión de introducción al estilo de la primera de Mega Man X**: un nivel corto y real, **sin tutorial explícito**, que enseña con el diseño del nivel.

- **Es un nivel más.** No se puede saltar: al crear una partida nueva se carga directo, sin pasar por la selección de nivel ni la de personaje.
- **Personaje:** la primera vez se juega con **Rosslyn** (`MissionData.forcedCharacter`). Al repetirlo desde la selección de nivel, se elige personaje como en cualquier otro **[AJUSTAR]**.
- **Sin carteles ni textos de controles.** Cada mecánica aparece primero en una situación segura y luego en una con riesgo. Por ejemplo, un escalón de 3 tiles antes del primer foso, un enemigo sobre el jugador para invitar a disparar hacia arriba, o un pasillo largo con un foso ancho al final que pide un dash jump. Orden sugerido: correr y disparar, saltar, disparar hacia arriba y en diagonal, agacharse, dash y dash jump **[AJUSTAR]**.
- **Corta y con ritmo:** unos pocos minutos, para que el jugador llegue rápido a la acción.
- Los controles se pueden consultar en el menú de pausa y en las opciones, nunca en pantalla durante el juego.
- **Estilo visual:** escenario holográfico de simulador, con enemigos holográficos (los mismos enemigos con otra paleta o material). Puede reutilizar la estética de la cuadrícula de prueba del Hito 1 **[AJUSTAR]**.
- Sin habilidades prestadas: se juega con el disparo básico.
- **Sin coleccionables** (`abilityPickups` vacío).
- **Final:** no tiene jefe. Termina al llegar a una **línea de meta**.
- **Escenas:** el General guía el entrenamiento en la escena de inicio y en la de cierre. Es el único nivel en el que solo participa Rosslyn. El diálogo de cierre puede presentar a Veyron y dar paso a las misiones reales.
- **Al completarla:** se guarda como completada y se abre la selección de nivel con el Bosque disponible.
- **Si se cierra el juego antes de completarla:** al continuar ese slot se vuelve a cargar la Misión 0, porque la selección de nivel aún no está disponible.
- Se puede **repetir** desde la selección de nivel, como cualquier nivel.


- **Pantalla de selección de nivel:** muestra los niveles en orden con su nombre, imagen y un **briefing** corto. Los completados se marcan como resueltos y se pueden repetir; el siguiente está disponible; los demás se ven bloqueados **[AJUSTAR]**. Cada nivel muestra los **iconos de sus coleccionables**: los encontrados a color y los que faltan como silueta, con el personaje dueño de cada uno (8.9), y los **prisioneros rescatados** (por ejemplo, 2/3).
- **Dificultad:** sube de nivel en nivel, porque el orden es fijo.
- **Nivel final:** la Torre alienígena (10), con Coliel. Como las habilidades son coleccionables opcionales, **tampoco aquí se garantiza tener alguna**: los obstáculos de 15.2 no cierran su camino principal y Coliel debe poder vencerse con el disparo básico de cualquier personaje. Al vencerlo siguen los créditos.
- **Repetir niveles:** los niveles completados **se pueden volver a jugar** desde la pantalla de selección, con cualquier personaje disponible, y siguen marcados como completados. Es la base del ciclo de 8.9: volver con otro personaje para abrir los caminos de 15.2 y buscar los coleccionables que faltan.
- Desde el menú principal se puede **continuar** (carga el último slot usado y va a la selección de nivel), **cargar** otro slot o empezar una **partida nueva** en un slot (ver 15.4).
- `GameManager` persistente (escena `Boot` cargada una vez) con el estado de la partida.
- Carga de escenas con `SceneManager.LoadSceneAsync` y una pantalla de transición.
- Menú de pausa con la lista de habilidades desbloqueadas del personaje actual (se puede **seleccionar** una desde ahí, 8.1) los prisioneros rescatados en el nivel actual y la opción de **abandonar la misión** (vuelve a la selección de nivel sin completarlo).

#### Pantalla de selección de personaje

Aparece **cada vez** que se elige un nivel, antes de cargarlo, incluidos los repetidos. La única excepción es la primera vez del Simulador, que se juega con Rosslyn (`forcedCharacter`).

- Muestra los personajes disponibles del `CharacterCatalog`: retrato, nombre, una descripción corta, sus estadísticas resumidas en barras (vida, velocidad, salto, energía) **[AJUSTAR]**, su disparo básico y los iconos de sus habilidades desbloqueadas.
- El cursor empieza en el **último personaje usado** en ese slot (`lastCharacter`, 15.4).
- *Cancelar* regresa a la selección de nivel.
- Al confirmar, `GameManager.SelectedCharacter` guarda la elección y se carga la escena de la misión, donde `MissionController` instancia al personaje (5.4).
- **No se cambia de personaje dentro de una misión**, ni al morir. Para cambiar hay que abandonar la misión desde la pausa.
- Si solo hay un personaje disponible, la pantalla se salta. Después del Simulador no pasa (Rosslyn y Veyron están disponibles), pero la lógica queda lista.
- **Personajes que aún no se unen:** aparecen como **silueta con "???"**.

#### CharacterData y CharacterCatalog

El personaje se define en `CharacterData` (5.4). Un asset `CharacterCatalog` define qué personajes aparecen en la pantalla y cuándo se desbloquean:

```csharp
[CreateAssetMenu(menuName = "Game/Character Catalog")]
public class CharacterCatalog : ScriptableObject
{
    public CharacterEntry[] characters;     // en el orden de la pantalla
}

[System.Serializable]
public struct CharacterEntry
{
    public CharacterData character;
    public MissionData[] requiredMissions;  // vacío = disponible desde el inicio
    public bool requiresAllPrisoners;       // Neotisleo: todos los prisioneros rescatados (15.2)
}
```

Rosslyn y Veyron tienen `requiredMissions` vacío. Aquamarine lleva el Bosque en llamas y Agente X el Tren militar en `requiredMissions`. Neotisleo lleva la Ciudad alienígena en `requiredMissions` y `requiresAllPrisoners`: se une cuando se cumplen las dos. Los personajes disponibles **no se guardan**: se calculan a partir del catálogo, de las misiones completadas y de los prisioneros rescatados del slot.

#### MissionData (ScriptableObject)

```csharp
[CreateAssetMenu(menuName = "Game/Mission")]
public class MissionData : ScriptableObject
{
    public string id;                   // clave para el guardado
    public string displayName;
    [TextArea] public string briefing;  // objetivo de la misión en la historia
    public Sprite portrait;             // imagen en la pantalla de selección
    public string sceneName;
    public AbilityData[] abilityPickups; // coleccionables escondidos en la misión (8.9)
    public CutsceneData introCutscene;  // al empezar la misión (ver 15.7)
    public CutsceneData outroCutscene;  // al terminar la misión
    public MusicTrack music;            // tema de la misión (ver 15.5)
    public MusicTrack bossMusic;        // tema de la sala del jefe
    public CutsceneData bossCutscene;   // al entrar a la sala del jefe: diálogo y pose (14.2)
    public bool isFinal;
    public CharacterData forcedCharacter; // la primera vez se juega con este personaje (Simulador)
}
```

Una lista de `MissionData` en un asset `MissionCatalog` define **el orden de los niveles** (cada uno se abre al completar el anterior), cuál es el final y cuál es el **nivel inicial** (`firstMission`, el Simulador) que se carga al crear una partida nueva. La pantalla de selección se arma a partir de ese catálogo, y `abilityPickups` puede estar vacío. Esto deja listo el sistema para agregar niveles en la versión 2 (ver 19.1).

### 15.2 Niveles

- **Checkpoints:** al morir se reaparece en el último checkpoint activado.
- **Peligros:** pinchos y fosos (muerte instantánea), plataformas móviles y plataformas que caen.
- Tilemaps de Unity con grid de 1 unidad (16 px), *Tilemap Collider 2D* (*Composite Operation = Merge*) + *Composite Collider 2D* para el suelo.
- **El suelo usa *Geometry Type = Polygons*** (sólido), no *Outlines*: con *Outlines* el collider es solo un contorno hueco y, si el jugador queda un poco metido en el suelo, la física no lo empuja hacia fuera y lo atraviesa. Las plataformas atravesables sí usan *Outlines*. `PlayerPhysicsMonitor` (en el prefab del jugador, solo en el editor y en builds de desarrollo) avisa en la consola si el jugador se hunde en un tile sólido o cae fuera del nivel, con el historial de los últimos pasos de física (F9 lo muestra en cualquier momento).

#### Obstáculos que se destruyen con habilidades

Hay tres tipos de obstáculo. Cada uno pide una **interacción**, y cada habilidad declara qué interacciones tiene:

| Obstáculo | Interacción que pide | Habilidad que la tiene | Cómo se destruye |
|---|---|---|---|
| Bloque destructible | `Break` | Taladro (Veyron) | Al instante: un taladro rompe un bloque y se detiene |
| Muro de acero | `Melt` | Lanzallamas (Rosslyn) | Con calor acumulado: hay que mantener la llama sobre el bloque hasta que se funde |
| Fuego | `Extinguish` | Chorro de agua (Aquamarine) | Al instante: un chorro apaga un tile de fuego y se deshace |

**Fuego:**
- Funciona como los muros: es **sólido** y cierra el paso. Además **daña al tocarlo** (`ContactDamage` en su Tilemap), así que no se puede atravesar recibiendo daño.
- Un chorro apaga un tile. Una pared de fuego de varios tiles cuesta varios chorros, como los bloques con el taladro.
- Al apagarse se borra el tile y sale un VFX de vapor.
- Encaja de forma natural en el Bosque en llamas, pero puede aparecer en cualquier nivel.
- El lanzallamas, la espada de fuego y la granada incendiaria **no** crean ni avivan fuego de obstáculo: las interacciones van por habilidad, no por elemento.

**Interacciones como datos:**
- Cada interacción es un asset `ObstacleInteraction` (ScriptableObject vacío que solo sirve de etiqueta): `Break`, `Melt`, `Extinguish`, etc.
- `AbilityData.obstacleInteractions` lista las que tiene cada habilidad (8.3). `AbilityObstacleTilemap` dice cuál pide y en qué modo se destruye (instantáneo o por calor).
- Cuando un efecto toca un obstáculo, llama a `TryInteract(punto, dirección, habilidad, dt)` de la interfaz `IAbilityObstacle`, que implementa `AbilityObstacleTilemap` (la celda sale del punto de impacto y la dirección). El obstáculo revisa si la habilidad tiene la interacción que pide y reacciona; si no la tiene, solo detiene el efecto. Las habilidades ya hacen esta llamada desde el Hito 4 (taladro al chocar, lanzallamas y láser al tocar la pared), así que el Hito 5 solo implementa el obstáculo.
- **Todos los obstáculos van en una sola capa física, `AbilityObstacle`.** Agregar un tipo nuevo (hielo, escudo de energía…) o hacer que otra habilidad rompa bloques es crear o asignar un asset, sin capas nuevas ni código en las habilidades. Solo un **modo** de destrucción nuevo (además de instantáneo y por calor) necesita código en `AbilityObstacleTilemap`.

**Reglas comunes:**
- Cada tipo va en su **propio Tilemap** (todos en la capa `AbilityObstacle`), con su propio collider. Para todo lo demás son paredes sólidas.
- **Tres personajes abren obstáculos:** Veyron rompe bloques con el taladro, Rosslyn funde metal con el lanzallamas y Aquamarine apaga el fuego con el chorro de agua. Lo que hay detrás suelen ser coleccionables y prisioneros (8.9, abajo). Como Aquamarine se une más tarde, los caminos de fuego de los primeros niveles se abren al repetirlos con ella. El disco de energía, la munición eléctrica y la granada incendiaria **no** rompen ni funden nada.
- Los vehículos **no** rompen obstáculos (18.1).
- Las interacciones van **por habilidad, no por elemento**: la espada de fuego **no** funde el acero aunque sea de fuego, y el láser no rompe ni funde nada.
- Como el jugador puede llegar a cualquier misión sin la habilidad necesaria, estos obstáculos **nunca** deben cerrar el camino principal, tampoco en el nivel final. Solo sirven para caminos secretos, atajos, pickups y coleccionables de habilidad. Al repetir misiones con el otro personaje o con nuevas habilidades, el jugador puede volver por ellos.
- Los obstáculos destruidos **no** se guardan: reaparecen al reiniciar la misión. Al morir y volver al checkpoint, siguen destruidos.
- Un componente `AbilityObstacleTilemap` en cada Tilemap guarda la configuración (interacción que pide, modo instantáneo o por calor, sprites por etapa y VFX) y el estado de cada celda.

**Bloques destructibles (taladro):**
- Al recibir un taladro, se borra el tile en la celda del impacto (`tilemap.SetTile(celda, null)`), se reproduce un VFX y el taladro se detiene.
- Un bloque = un taladro. Un muro de 3 bloques cuesta 3 taladros (9 de energía con el costo actual).

**Muros de acero (lanzallamas):**
- La llama hace un raycast contra `Ground` y `AbilityObstacle`. Si lo primero que toca es acero, la llama se recorta ahí y **calienta esa celda** (`TryInteract` con `dt`).
- Cada celda acumula calor: `calor += dt` mientras la llama la toca. Se funde al llegar a **1.5 s** de llama continua **[AJUSTAR]** (unos 9 de energía con el costo actual).
- Si se deja de calentar, la celda **se enfría** a 0.5 de calor por segundo **[AJUSTAR]**. Así no se puede fundir un muro con toques cortos separados.
- **Feedback visual por etapas:** el tile cambia de sprite según el calor (normal → caliente → al rojo vivo), con `SetTile` a la variante correspondiente. Al fundirse, se borra el tile y se reproduce un VFX de metal derretido.
- Un muro de varios bloques se funde **de uno en uno**, desde el lado donde está el jugador.
- El estado de calor se guarda en un `Dictionary<Vector3Int, float>` por celda dentro de `AbilityObstacleTilemap`. Solo se actualizan las celdas con calor mayor a 0.

#### Prisioneros (desbloqueo de Neotisleo)

Como en *Gravity Circuit*, cada nivel esconde **3 prisioneros**. Al rescatarlos a todos se desbloquea **Neotisleo** como personaje jugable.

- **Cuántos:** 3 por nivel en los niveles 2 a 9, **24 en total**. El Simulador no tiene (es una simulación) y la Torre alienígena tampoco, para que quien los busque pueda usar a Neotisleo contra Coliel.
- **Dónde:** escondidos en caminos secundarios, atajos y zonas difíciles. Algunos detrás de obstáculos del taladro o del lanzallamas (15.2), así que completar la lista pide repetir niveles con distintos personajes, igual que los coleccionables de habilidad (8.9). Al menos uno por nivel debe poder rescatarse sin habilidades **[AJUSTAR]**.
- **Quién los rescata:** **cualquier personaje** (a diferencia de los coleccionables de habilidad, que son de su dueño).
- **Cómo se rescata:** el prisionero está en una celda o atado. Basta con **tocarlo** (la celda se abre sola); agradece con un gesto y desaparece con un teletransporte. Aparece un contador en pantalla (por ejemplo, "Prisioneros 2/3"). **No detiene el juego**: el jugador no pasa a `Disabled`.
- **Se guarda en el momento** (15.4), como los coleccionables de habilidad: no se pierde si el jugador muere, abandona el nivel o cierra el juego. Un prisionero rescatado ya no aparece al repetir el nivel.
- **Desbloqueo:** antes de la misión final, el comando quiere bombardear Coleópolis. Si están los 24 prisioneros, estos **convencen a Neotisleo de pelear junto a Rosslyn** para detener a Coliel sin destruir la ciudad (1.4). Se reproduce esa escena y Neotisleo aparece en la selección de personaje, ya disponible para la Torre alienígena.
- **Cuándo se ve la escena:** la primera vez que se cumplen las dos condiciones (Ciudad alienígena completada y los 24 prisioneros rescatados), al terminar el nivel en el que ocurre. Normalmente es al terminar la Ciudad alienígena; si el último prisionero se rescata después, al repetir un nivel, la escena sale al terminar ese nivel.
- Hay un solo final: sin todos los prisioneros, la misión final se juega igual, solo sin Neotisleo.
- **Dónde se ve el progreso:** en la selección de nivel (rescatados de cada nivel), en el menú de pausa (los del nivel actual) y un total en la pantalla de slots **[AJUSTAR]**.

**Componentes y datos:**
- `Prisoner` (en la escena): un id único (por ejemplo, `forest_p1`), su sprite y su celda. Al cargar la escena revisa en `GameManager` si ya fue rescatado y, si es así, no aparece.
- `MissionData.prisonerIds`: los ids de los prisioneros del nivel, para mostrar el progreso sin cargar la escena. El total se calcula sumando los del `MissionCatalog`.
- **Validación** (`Game/Validate`): cada nivel tiene 3 ids únicos en `MissionData.prisonerIds` y un `Prisoner` en la escena por cada uno.

### 15.3 Opciones de PC

- Pantalla completa, ventana y ventana sin bordes.
- Escala de ventana en múltiplos enteros (×2, ×3, ×4…) para el modo ventana.
- VSync activado y límite de FPS opcional.
- Volumen general, de música y de SFX.
- Remapeo de controles (ver sección 12).
- **Idioma:** español o inglés. La primera vez se elige según el idioma del sistema (inglés si no es español).

#### Idiomas

El juego sale en **español e inglés**. Todo el texto se maneja con el paquete *Localization* de Unity desde el inicio, no se traduce al final:
- **String Tables** por área: `UI`, `Gameplay` (nombres de habilidades, personajes, niveles, briefings) y una por nivel para sus escenas.
- Los campos de texto de los datos (`MissionData.briefing`, `CharacterData.description`, `AbilityData.displayName`, `SpeakerData.displayName`, el texto de `LineStep`…) son `LocalizedString`, no `string`.
- La fuente pixel incluye acentos, `ñ`, `¿` y `¡`.
- El cuadro de diálogo y los menús se prueban con los dos idiomas: el inglés suele ser más corto y el español más largo.
- `Game/Validate` avisa si a una entrada le falta la traducción.

### 15.4 Guardado

El sistema de guardado se diseña desde el inicio, aunque la mayoría de los datos que guarda llegan en hitos posteriores. Cambiar el formato después de tener partidas guardadas es caro, por eso se definen ya el formato, las versiones y la protección contra archivos dañados.

#### Qué se guarda y dónde

Hay **dos tipos de archivo**, separados a propósito:

| Archivo | Contenido | Por qué va aparte |
|---|---|---|
| `settings.json` | Volúmenes, modo de pantalla, controles remapeados | Las opciones son del jugador, no de la partida. Empezar una partida nueva o borrar un slot no las reinicia |
| `slot_1.json`, `slot_2.json`, `slot_3.json` | Progreso de cada partida | Cada slot es independiente |

- **Slots de partida: 3.** El código admite cualquier cantidad (`SaveSystem.SlotCount`); el número solo afecta la pantalla de selección de slot.
- **Ubicación:** `Application.persistentDataPath`. En Windows es `%USERPROFILE%\AppData\LocalLow\<Company Name>\<Product Name>\`.
- **Importante:** definir el **Company Name** y el **Product Name** en *Project Settings → Player* desde el Hito 1. Si se cambian después, la ruta cambia y los jugadores "pierden" sus partidas.
- Se usa JSON legible y sin cifrar: es un juego de un jugador, y poder abrir el archivo facilita mucho depurar.

#### Formato del slot

```json
{
  "version": 1,
  "createdAt": "2026-10-08T18:30:00Z",
  "lastSavedAt": "2026-10-09T21:12:44Z",
  "playTimeSeconds": 5420.5,
  "completedMissions": ["mission_station"],
  "rescuedPrisoners": ["forest_p1", "forest_p3"],
  "lastCharacter": "rosslyn",
  "characters": [
    { "id": "rosslyn", "unlockedAbilities": ["flamethrower"], "selectedAbility": "flamethrower" },
    { "id": "veyron", "unlockedAbilities": ["drill", "knife"], "selectedAbility": "drill" },
    { "id": "aquamarine", "unlockedAbilities": [], "selectedAbility": "" }
  ],
  "stats": {
    "deaths": 37
  }
}
```

```csharp
[System.Serializable]
public class SaveData
{
    public int version = SaveSystem.CurrentVersion;
    public string createdAt;
    public string lastSavedAt;
    public float playTimeSeconds;
    public List<string> completedMissions = new();
    public List<string> rescuedPrisoners = new();                // Prisoner ids (15.2)
    public string lastCharacter;                                // CharacterData.id
    public List<CharacterProgress> characters = new();
    public SaveStats stats = new();
}

[System.Serializable]
public class CharacterProgress
{
    public string id;                                           // CharacterData.id
    public List<string> unlockedAbilities = new();
    public string selectedAbility;
}
```

- **Las misiones completadas son del slot**, no del personaje: en el lore los dos soldados van a cada misión.
- **Las habilidades son de cada personaje** (`characters`), igual que la habilidad seleccionada.
- El formato sigue en `version = 1`: el guardado todavía no se implementa (Hito 4), así que el cambio no necesita migración.

- Todo se guarda por `id` (`MissionData.id`, `AbilityData.id`, `CharacterData.id`), nunca por índice ni por nombre visible.
- Se serializa con `JsonUtility`. No admite `Dictionary`, así que se usan listas.
- `SaveData` es **solo datos**: no tiene lógica ni referencias a objetos de Unity.
- La pantalla de slots muestra, sin cargar la partida: misiones completadas, último personaje usado, tiempo de juego y fecha del último guardado.

#### Formato de opciones

```json
{
  "version": 1,
  "masterVolume": 1.0,
  "musicVolume": 0.8,
  "sfxVolume": 1.0,
  "fullscreenMode": "FullScreenWindow",
  "windowScale": 4,
  "vSync": true,
  "bindingOverrides": "{...}",
  "lastUsedSlot": 1,
  "language": "es"
}
```

`lastUsedSlot` permite que **Continuar** en el menú principal cargue directo la última partida.

#### Cuándo se guarda

**Autoguardado**, sin botón de guardar:

| Momento | Qué se guarda |
|---|---|
| Al recoger un coleccionable de habilidad | Habilidad desbloqueada del personaje y tiempo de juego (no el estado del nivel) |
| Al rescatar un prisionero | Prisionero rescatado y tiempo de juego (no el estado del nivel) |
| Al completar una misión (después del diálogo de cierre) | Misión completada, último personaje usado, tiempo de juego, estadísticas |
| Al cambiar una opción | `settings.json` |
| Al salir del juego desde el menú | Tiempo de juego |

- **No se guarda el estado de la misión.** La única escritura a mitad de misión es la de un coleccionable recogido, que solo guarda el progreso. Los checkpoints solo existen mientras se juega la misión. Si el jugador cierra el juego dentro de una misión, al volver aparece en la selección de nivel (o en la Misión 0, si todavía no la completa). Esto coincide con el estilo de Mega Man y evita guardar el estado del nivel (enemigos, obstáculos, posición).
- Mientras se guarda, aparece un icono pequeño en una esquina de la pantalla.

#### Escritura segura

Si el juego se cierra o se va la luz justo mientras escribe, el archivo puede quedar dañado. Para evitarlo:

1. Se escribe el JSON a un archivo temporal: `slot_1.json.tmp`.
2. Se reemplaza el archivo real con `File.Replace(tmp, real, backup)`, que deja la versión anterior como `slot_1.json.bak`.
3. **Al cargar:** si `slot_1.json` no se puede leer, se intenta con `slot_1.json.bak`. Si ninguno funciona, el slot se muestra como dañado y se ofrece borrarlo; nunca se sobrescribe sin avisar.

#### Versiones y migraciones

- Cada archivo tiene `version`. `SaveSystem.CurrentVersion` es la versión actual del formato.
- Al cargar un archivo con versión anterior, `SaveMigrator` aplica los pasos en orden (`v1 → v2 → v3…`) y después se guarda con la versión nueva.
- Si el archivo tiene una versión **mayor** que la del juego (por ejemplo, el jugador volvió a una versión anterior), no se carga ni se sobrescribe.
- La versión 2 del juego (más niveles) probablemente necesite su primera migración. Por eso este mecanismo existe desde el inicio.

#### Validación al cargar

- Los `id` que ya no existen en el juego (contenido eliminado) se ignoran con un aviso en el log.
- Si un personaje tiene desbloqueada una habilidad que no está en su `CharacterData.abilities`, se ignora con un aviso en el log.
- Si un personaje del catálogo no tiene entrada en `characters`, se crea vacía.
- Si el `selectedAbility` de un personaje no está desbloqueado, se selecciona la primera disponible.
- Si `lastCharacter` no existe o no está disponible, se usa el primero del catálogo.

#### Componentes

| Componente | Hace |
|---|---|
| `SaveData`, `SettingsData` | Clases de datos serializables |
| `SaveSystem` | Lectura y escritura de archivos: rutas, escritura segura, respaldo, carga con respaldo, borrado de slots |
| `SaveMigrator` | Actualiza archivos de versiones anteriores |
| `GameManager` | Tiene la partida actual en memoria (`SaveData`), la actualiza durante el juego y pide a `SaveSystem` que guarde en los momentos definidos |

Los sistemas del juego (habilidades, selección de nivel) **leen y modifican el estado a través de `GameManager`**, nunca leen archivos directamente.

**Implementación del Hito 4** (antes de la escena `Boot` y de la pantalla de slots):
- `GameManager` se crea solo la primera vez que se usa (`GameManager.Instance`, `DontDestroyOnLoad`). En el Hito 6 pasa a vivir en la escena `Boot`.
- Al jugar sin pasar por los menús, `EnsureGameLoaded` carga el slot `settings.lastUsedSlot` (o el 1) y, si está vacío, crea una partida nueva.
- `PlayerSpawner` llama a `GameManager.BindPlayer(player)` después de `Initialize`: aplica las habilidades desbloqueadas y la seleccionada del personaje, guarda en el acto cuando se desbloquea una (`PlayerAbilities.AbilityUnlocked`), recuerda la seleccionada y cuenta las muertes (en memoria, se escriben en el siguiente guardado).
- **Con el modo de prueba de habilidades activado no se lee ni se escribe el progreso de habilidades**, para no mezclar las de prueba con las de la partida.
- Un slot dañado o de una versión más nueva se marca de solo lectura (`IsReadOnly`): se juega con una partida en memoria y no se escribe nada.
- Al cerrar el juego se guarda el tiempo de juego (`OnApplicationQuit`).
- `GameManager.Saved` muestra el icono de guardado en el HUD (abajo a la derecha, 1 s).

#### Herramientas de depuración (solo en el editor)

Menú `Game/Save` en el editor de Unity:
- **Abrir carpeta de guardado** (`EditorUtility.RevealInFinder`).
- **Borrar todos los guardados.**
- **Crear guardado de prueba** (slot 1): todas las habilidades de todos los personajes desbloqueadas y todos los prisioneros rescatados; las misiones completadas se agregan cuando exista `MissionCatalog` (Hito 6).
- **Cargar un JSON específico**, para reproducir problemas reportados (se copia como `slot_1.json`).
- **Mostrar los slots en la consola** (estado y contenido de cada uno).
- **Desbloquear la siguiente habilidad (Play)**: simula un coleccionable mientras no existan (Hito 5). Solo con el modo de prueba de habilidades apagado.

### 15.5 Audio

El audio tiene dos partes independientes: **música** (una pista a la vez, con cambios suaves) y **efectos de sonido** (muchos a la vez, cortos). Las dos pasan por un `AudioMixer` para controlar volúmenes y efectos globales.

#### AudioMixer

```
Master
├── Music
└── SFX
    ├── Gameplay     ← jugador, enemigos, habilidades, obstáculos
    └── UI           ← menús, cambio de habilidad, diálogos
```

- Cada grupo expone su volumen como parámetro (`MasterVolume`, `MusicVolume`, `SfxVolume`).
- Los sliders de opciones van de 0 a 1 y se convierten a decibeles: `dB = 20 × log10(valor)`, con un mínimo de -80 dB cuando el valor es 0.
- **Snapshots:** `Default` y `Paused`. En pausa, la música baja de volumen y pasa por un filtro *low-pass*, para que suene "apagada".

#### Componentes

| Componente | Hace |
|---|---|
| `MusicPlayer` | Persistente (vive en la escena `Boot`). Reproduce la pista actual, cambia de pista con *crossfade* y maneja intro + loop |
| `SfxPlayer` | Persistente. Pool de `AudioSource` (24 **[AJUSTAR]**) que reproduce `SfxData`, con límite de voces y cooldown por sonido |
| `VolumeSettings` | Aplica y guarda los volúmenes (ver 15.4). No se llama `AudioSettings` para no chocar con la clase de Unity del mismo nombre |

Los demás sistemas solo llaman a `MusicPlayer.Play(track)` o `SfxPlayer.Play(sfx)`. No crean `AudioSource` propios.

#### Música

```csharp
[CreateAssetMenu(menuName = "Game/Music Track")]
public class MusicTrack : ScriptableObject
{
    public AudioClip intro;     // opcional: suena una vez
    public AudioClip loop;      // se repite sin fin
    [Range(0, 1)] public float volume = 1f;
}
```

- **Intro + loop sin cortes:** muchas pistas de videojuego tienen una introducción que no se repite. `MusicPlayer` usa dos `AudioSource`: reproduce `intro` y programa `loop` con `PlayScheduled(AudioSettings.dspTime + duraciónIntro)` para que empiece exactamente al terminar, sin silencios. Si una pista no tiene intro, se reproduce solo el loop.
- **Crossfade:** al cambiar de pista, la anterior baja y la nueva sube en 0.5 s **[AJUSTAR]**. Si se pide la pista que ya está sonando, no se reinicia.
- **Pistas por contexto:**

| Momento | Pista |
|---|---|
| Menú principal | Tema principal |
| Selección de nivel | Tema de selección |
| Nivel (escena de inicio y nivel) | Tema del nivel (`MissionData.music`) |
| Sala del jefe | Diálogo con el jefe: tensión o silencio **[AJUSTAR]**. Al llenarse su barra: tema de jefe (`MissionData.bossMusic`) |
| Jefe vencido | *Jingle* de victoria (sin loop), luego silencio durante el diálogo de cierre |
| Torre alienígena (nivel final) | Tema propio + tema de Coliel |
| Créditos | Tema de créditos |

- Al morir, la música **sigue sonando** (como en Mega Man). No se reinicia al reaparecer.
- `MissionData` agrega los campos `MusicTrack music` y `MusicTrack bossMusic`.

#### Efectos de sonido

```csharp
[CreateAssetMenu(menuName = "Game/Sfx")]
public class SfxData : ScriptableObject
{
    public AudioClip[] clips;               // si hay varios, se elige uno al azar
    [Range(0, 1)] public float volume = 1f;
    public Vector2 pitchRange = new(0.95f, 1.05f);  // variación para que no suene repetitivo
    public AudioMixerGroup group;           // Gameplay o UI
    public int maxVoices = 4;               // cuántas copias pueden sonar a la vez
    public float cooldown = 0.03f;          // tiempo mínimo entre dos reproducciones
    public bool loop;                       // lanzallamas, láser
}
```

- **Variación:** cada reproducción elige un clip al azar y un pitch dentro del rango. Así el disparo, que suena cientos de veces, no cansa.
- **Límite de voces y cooldown:** el disparo no tiene límite de balas, y el lanzallamas y el láser golpean muchas veces por segundo. Sin estos límites, los sonidos se apilan y saturan. Si se llega a `maxVoices`, se reemplaza la copia más vieja.
- **Sonidos en loop:** `SfxPlayer.Play()` devuelve un `SfxHandle` para detenerlo después. El lanzallamas y el láser lo inician en `Begin` y lo detienen en `End`.
- **Sonido 2D con paneo:** no hay audio 3D. El sonido se reproduce en 2D y se le aplica un *pan* estéreo leve según la posición horizontal en pantalla (-0.5 a la izquierda, 0.5 a la derecha) **[AJUSTAR]**.
- **Fuera de cámara:** los sonidos de enemigos que están fuera de la pantalla (con un margen de 2 tiles) no se reproducen.
- `BusterData.fireSfx`, `AbilityData.activateSfx` y `AbilityData.loopSfx` son de tipo `SfxData`, no `AudioClip`.
- Los pasos se disparan con el Animation Event `OnFootstep` (9.4).

#### Pausa

- `Time.timeScale = 0` no detiene el audio. Al pausar se usa `AudioListener.pause = true`, que pausa todos los sonidos del juego.
- Las fuentes de **música** y de **UI** tienen `ignoreListenerPause = true`, para que la música siga (con el snapshot `Paused`) y los menús tengan sonido.

#### Importación

| Tipo | Formato | Load Type | Otros |
|---|---|---|---|
| SFX | `.wav` | *Decompress On Load* | *Force To Mono* activado |
| Música | `.ogg` | *Streaming* | *Preload Audio Data* desactivado |

- Volumen de referencia: la música más baja que los SFX importantes (disparo, golpe, daño), para que el jugador siempre oiga lo que pasa en el juego.
- Al exportar un loop, revisar que empiece y termine en el mismo punto para que no se escuche un "clic".

#### Lista de sonidos

| Categoría | Sonidos |
|---|---|
| Jugador | Salto, aterrizaje, pasos, dash, disparo, daño, muerte, teletransporte (entrada y salida) |
| Habilidades | Lanzar granada, rebote, explosión, tajo de espada, tajo de cuchillo, proyectil destruido por la espada, lanzallamas (loop), taladro, láser (carga y loop), boom sónico, ráfaga de burbujas, recoger coleccionable de habilidad (jingle), sin energía (error), cambio de habilidad |
| Obstáculos | Bloque roto, acero calentándose (loop), acero fundido, fuego (loop), fuego apagado |
| Enemigos | Disparo, golpe recibido, muerte (2–3 variantes genéricas al inicio) |
| Pickups | Vida, energía |
| UI | Mover cursor, confirmar, cancelar, pausa, letra del diálogo, misión desbloqueada |

| Música | Pistas |
|---|---|
| Menús | Tema principal, selección de nivel, créditos |
| Niveles | 10 temas, uno por nivel (el Simulador incluido) |
| Jefes | Un tema de jefe compartido + el tema de Coliel |
| Jingles | Victoria, habilidad obtenida |

#### Checklist de clips por conseguir

Marca cada casilla al importar el clip y asignarlo a su `SfxData` (en `Assets/_Project/Data/Audio/`). Registra el origen en `CREDITS.md` en el momento de descargarlo.

- **Nombre del archivo:** igual al del `SfxData`, por ejemplo `Sfx_PlayerJump.wav`. Si hay variantes, se agrega `_01`, `_02`, etc., y todas van en el arreglo `clips` del mismo `SfxData`.
- **Formato:** `.wav` mono a 44.1 kHz, sin silencio al inicio (ver *Origen del audio*).
- **Loop:** los sonidos marcados con *loop* deben repetirse sin "clic"; en su `SfxData` se activa `loop`.
- **Columna `SfxData`:** las marcadas con ✔ ya existen como asset vacío (las crea **Game → Setup**). Las demás se crean al llegar a su hito.

**Hito 2: jugador (ya se usan en el juego)**

| ✓ | Sonido | `SfxData` | Variantes | Notas |
|---|---|---|---|---|
| [ ] | Disparo del bláster | `Sfx_PlayerShoot` ✔ | 2–3 | Muy corto (< 0.15 s); suena cientos de veces |
| [ ] | Salto | `Sfx_PlayerJump` ✔ | 1–2 | |
| [ ] | Aterrizaje | `Sfx_PlayerLand` ✔ | 1–2 | Suave; suena en cada caída |
| [ ] | Dash | `Sfx_PlayerDash` ✔ | 1 | También para el air dash |
| [ ] | Daño recibido | `Sfx_PlayerHurt` ✔ | 1–2 | |
| [ ] | Muerte | `Sfx_PlayerDeath` ✔ | 1 | Se importa con la animación de muerte (Hito 3) |

**Hito 3: animación del jugador**

| ✓ | Sonido | `SfxData` | Variantes | Notas |
|---|---|---|---|---|
| [ ] | Pasos | `Sfx_PlayerStep` | 3–4 | Los dispara el Animation Event `OnFootstep` |
| [ ] | Teletransporte (entrada) | `Sfx_PlayerTeleportIn` | 1 | Al empezar la misión y al reaparecer |
| [ ] | Teletransporte (salida) | `Sfx_PlayerTeleportOut` | 1 | Al terminar la misión |

**Hito 4: habilidades**

| ✓ | Sonido | `SfxData` | Variantes | Notas |
|---|---|---|---|---|
| [ ] | Cambio de habilidad | `Sfx_AbilitySwitch` | 1 | Grupo `UI` |
| [ ] | Sin energía (error) | `Sfx_AbilityNoEnergy` | 1 | Grupo `UI` |
| [ ] | Lanzar granada | `Sfx_GrenadeThrow` | 1 | |
| [ ] | Rebote de granada | `Sfx_GrenadeBounce` | 2 | Cooldown corto para que no se sature |
| [ ] | Explosión | `Sfx_Explosion` | 2 | Se reutiliza para enemigos |
| [ ] | Tajo de espada de fuego | `Sfx_SwordSlash` | 2 | |
| [ ] | Proyectil destruido por la espada | `Sfx_SwordDeflect` | 1 | |
| [ ] | Lanzallamas | `Sfx_FlamethrowerLoop` | 1 | *loop* |
| [ ] | Taladro | `Sfx_DrillShoot` | 1 | |
| [ ] | Láser: carga | `Sfx_LaserCharge` | 1 | Dura ~0.1 s (el arranque del láser) |
| [ ] | Láser: rayo | `Sfx_LaserLoop` | 1 | *loop* |
| [ ] | Disparo básico de Veyron | `Sfx_VeyronShoot` | 1–2 | Más grave que el bláster. Hito 7 |
| [ ] | Tajo de cuchillo | `Sfx_KnifeSlash` | 2 | Hito 7 |
| [ ] | Boom sónico | `Sfx_SonicBoom` | 1–2 | Golpe grave, como escopeta. Hito 7 |
| [ ] | Ráfaga de burbujas | `Sfx_BubbleBurst` | 1–2 | Burbujas reventando. Hito 8 |

**Hito 5: enemigos, pickups y obstáculos**

| ✓ | Sonido | `SfxData` | Variantes | Notas |
|---|---|---|---|---|
| [ ] | Disparo enemigo | `Sfx_EnemyShoot` | 2 | Distinto al del jugador para reconocerlo |
| [ ] | Golpe a enemigo | `Sfx_EnemyHit` | 2–3 | Muy corto; suena con cada bala que acierta |
| [ ] | Muerte de enemigo | `Sfx_EnemyDeath` | 2–3 | Genéricas para todos los enemigos al inicio |
| [ ] | Pickup de vida | `Sfx_PickupHealth` | 1 | |
| [ ] | Coleccionable de habilidad | `Sfx_AbilityPickup` | 1 | Jingle corto al recogerlo (8.9) |
| [ ] | Prisionero rescatado | `Sfx_PrisonerRescue` | 1 | Agradecimiento + teletransporte (15.2) |
| [ ] | Pickup de energía | `Sfx_PickupEnergy` | 1 | |
| [ ] | Bloque roto | `Sfx_BlockBreak` | 1–2 | Taladro contra un bloque destructible |
| [ ] | Fuego (loop) | `Sfx_FireLoop` | 1 | Suena cerca de los tiles de fuego |
| [ ] | Fuego apagado | `Sfx_FireExtinguish` | 1–2 | Chorro de agua contra el fuego (vapor) |
| [ ] | Acero calentándose | `Sfx_SteelHeatLoop` | 1 | *loop* |
| [ ] | Acero fundido | `Sfx_SteelMelt` | 1 | |

**Hito 6: interfaz y diálogos** (grupo `UI`)

| ✓ | Sonido | `SfxData` | Variantes | Notas |
|---|---|---|---|---|
| [ ] | Mover cursor | `Sfx_UIMove` | 1 | |
| [ ] | Confirmar | `Sfx_UIConfirm` | 1 | |
| [ ] | Cancelar | `Sfx_UICancel` | 1 | |
| [ ] | Pausa | `Sfx_UIPause` | 1 | |
| [ ] | Letra del diálogo | `Sfx_DialogueBlip` | 1–2 | Muy corto; suena cada pocas letras |
| [ ] | Misión desbloqueada | `Sfx_UIMissionUnlocked` | 1 | |

**Música (la compone el equipo, Hito 6 en adelante)**

Cada pista con introducción se exporta como `Track_Nombre_Intro.ogg` + `Track_Nombre_Loop.ogg`.

- [ ] Tema principal (menú)
- [ ] Selección de nivel
- [ ] Temas de los 10 niveles (uno por nivel)
- [ ] Tema de jefe (compartido)
- [ ] Tema de Coliel (jefe final)
- [ ] Jingle de victoria (sin loop)
- [ ] Jingle de habilidad obtenida (sin loop)
- [ ] Créditos

#### Origen del audio

**SFX: librerías de audio.**
- **Licencias:** preferir sonidos **CC0** (no piden nada). Si un sonido es CC-BY, hay que dar crédito al autor. Evitar licencias *NonCommercial* (CC-BY-NC) si el juego podría venderse.
- **Registro de créditos desde el primer sonido:** un archivo `CREDITS.md` en la raíz del repositorio, con una fila por sonido: archivo en el proyecto, nombre original, autor, licencia y URL. Es mucho más fácil anotarlo al descargar que reconstruirlo al final.
- **Coherencia de estilo:** usar pocas fuentes (2–3 paquetes de estilo retro o ciencia ficción) para que los sonidos suenen al mismo juego.
- **Preparación antes de importar** (por ejemplo, con Audacity):
  - Recortar el silencio al inicio, para que el sonido salga en el mismo frame que la acción.
  - Convertir a mono y exportar en `.wav` a 44.1 kHz.
  - Igualar el volumen entre sonidos de la misma categoría.
  - Guardar el archivo original descargado en `AudioSource/SFX/` (fuera de `Assets`) y el editado en `Assets/_Project/Audio/SFX/`.
- Desde el Hito 2 se usan sonidos de librería. Si todavía no se encuentra el definitivo, cualquier sonido temporal sirve: lo importante es que el `SfxData` exista y se pueda cambiar el clip después.

**Música: compuesta por el equipo.**
- Los archivos de proyecto de la música se guardan en `AudioSource/Music/` (fuera de `Assets`). Si son pesados, conviene no versionarlos (o versionar solo las exportaciones finales), porque el repositorio no usa LFS.
- **Exportar cada pista en dos archivos** cuando tenga introducción: `Track_Name_Intro.ogg` y `Track_Name_Loop.ogg`. El loop debe empezar y terminar exactamente en el compás de repetición, para que `PlayScheduled` lo una sin cortes.
- Formato: `.ogg`, 44.1 kHz, estéreo.
- Volumen: todas las pistas con una sonoridad parecida (por ejemplo, alrededor de -16 LUFS **[AJUSTAR]**) y por debajo de los SFX importantes.
- Duración sugerida de cada loop: 1–2 minutos, para que no se sienta repetitivo en una misión.
- Probar cada loop dentro de Unity, no solo en el programa de música: es la única forma de confirmar que el punto de repetición no hace "clic".

### 15.6 HUD

- Barra de vida.
- Barra de energía compartida.
- Las barras de vida y energía toman su máximo del `CharacterData` del personaje en juego.
- Las barras (vida, energía, jefe) son un mismo componente reutilizable, `SegmentedBar`, que recibe valor actual y máximo y dibuja segmentos. También se usa para la vida y la energía de los vehículos (18).
- **Icono de la habilidad seleccionada** junto a la barra de energía. Al cambiar de habilidad, el icono se actualiza y aparece el nombre durante 1–2 segundos.
- Barra de vida del jefe.
- **Canvas pixel perfect:**
  - Canvas en *Screen Space - Overlay* con *Canvas Scaler* en modo *Constant Pixel Size*.
  - El layout se diseña a 320×180, con sprites de PPU 16 y `Filter Mode = Point`.
  - Un script asigna `scaleFactor` = el factor de escala entero de la Pixel Perfect Camera (×4, ×6, ×8…). Así cada píxel del HUD mide lo mismo que un píxel del juego.
  - Las posiciones de los elementos van en píxeles enteros.
  - Fuente bitmap (pixel font) con `Filter Mode = Point`.

### 15.7 Escenas y diálogos (estilo novela visual)

Los diálogos se presentan como una **novela visual**, al estilo de *Sonic Battle*: una pantalla con **una ilustración en el centro**, **de 1 a 4 personajes a los lados** y el cuadro de texto abajo. El personaje que habla se ve con **opacidad completa** y los demás atenuados. Cada personaje tiene **varios retratos, uno por emoción**, y la ilustración puede cambiar conforme avanza la escena.

La novela visual **reemplaza** a las escenas con personajes caminando dentro del nivel. Dentro del nivel solo quedan acciones cortas: el teletransporte del jugador, la entrada del jefe (pose y barra) y movimientos de cámara.

**Dónde se usan:**
- Inicio y cierre de cada nivel (`MissionData.introCutscene` / `outroCutscene`).
- Al entrar a la sala de cada jefe (`MissionData.bossCutscene`): diálogo con el jefe y, ya en el nivel, su pose y el llenado de su barra (14.2).
- Momentos especiales activados por una zona del nivel (`CutsceneTrigger`): el final del Simulador, las incorporaciones de personajes y otros eventos de la historia.

#### Presentación

Distribución en la resolución de referencia (320×180) **[AJUSTAR]**:

```
┌────────────────────────────────────────────────────────┐
│  [1] [2]     ┌──────────────────────────┐    [3] [4]   │
│              │                          │              │
│  retratos    │   Ilustración central    │  retratos    │
│  izquierda   │        144×104           │  derecha     │
│              │                          │              │
│              └──────────────────────────┘              │
├────────────────────────────────────────────────────────┤
│ ROSSLYN                                                │
│ General, el objetivo está en las minas.                │
│ Vamos para allá.                                    ▼  │
└────────────────────────────────────────────────────────┘
```

- **4 posiciones:** 1 y 2 a la izquierda, 3 y 4 a la derecha. Las interiores (2 y 3) quedan un poco delante de las exteriores y se pueden traslapar.
- **Retratos de busto** de unos 64×96 px **[AJUSTAR]**, dibujados mirando a la derecha. Los de la derecha se voltean en el juego.
- **El que habla** se ve normal; los demás se **oscurecen** (por ejemplo, multiplicando su color por 50% de gris). Se oscurecen en lugar de volverse transparentes porque, al traslaparse, la transparencia dejaría ver un retrato a través del otro. Opcionalmente, el que habla sube 1–2 px **[AJUSTAR]**.
- **Ilustración central** de unos 144×104 px **[AJUSTAR]** que muestra la situación (el lugar, el jefe, un objeto). Puede cambiar en cualquier momento de la escena, con corte o fundido.
- **Fondo:** un panel o marco fijo que tapa el nivel mientras dura la novela visual **[AJUSTAR]**.
- **Cuadro de texto** abajo, de 320 × 56 px **[AJUSTAR]**: el **nombre** del que habla en una pestaña de su lado, hasta **3 líneas** de texto e indicador ▼ cuando se puede avanzar.
- **Líneas por radio:** el retrato del que habla aparece con un marco de transmisión (estática o líneas) **[AJUSTAR]**. Lo usan el General cuando no está presente y su sustituto (ver abajo).
- Entradas y salidas: los retratos entran deslizándose desde su lado y salen igual.
- Usa el mismo Canvas pixel perfect y la misma fuente pixel del HUD (15.6).

#### Emociones

- Cada personaje que habla tiene un set de retratos por emoción. Set: `Neutral`, `Feliz`, `Enojado`, `Sorprendido`, `Triste`, `Determinado`.
- Las emociones son un `enum` compartido por todos. Si a un personaje le falta una emoción, se usa `Neutral` y `Game/Validate` avisa.
- Los jefes pueden tener menos (por ejemplo, `Neutral` y `Enojado`).

#### Datos

Una escena es una **lista de pasos** que se ejecutan en orden. Las líneas de diálogo son un tipo de paso más.

```csharp
public enum Emotion { Neutral, Happy, Angry, Surprised, Sad, Determined }

[CreateAssetMenu(menuName = "Game/Speaker")]
public class SpeakerData : ScriptableObject
{
    public string id;                       // "rosslyn", "neotisleo", "wolfang"…
    public string displayName;              // "Rosslyn", "General Neotisleo", etc.
    public Sprite[] portraits;              // uno por Emotion, en el orden del enum
    public SpeakerData standIn;             // quién dice sus líneas cuando se juega con este personaje (Neotisleo)
}

[CreateAssetMenu(menuName = "Game/Cutscene")]
public class CutsceneData : ScriptableObject
{
    public Sprite startIllustration;        // ilustración al abrir la novela visual
    [SerializeReference] public List<CutsceneStep> steps = new();
}

[System.Serializable]
public abstract class CutsceneStep
{
    public bool firstTimeOnly;           // se omite si la escena se repite en el mismo intento de nivel
    public bool waitToFinish = true;     // false = el siguiente paso empieza al mismo tiempo
    public abstract IEnumerator Run(CutsceneContext ctx);
    public abstract void Skip(CutsceneContext ctx);   // deja el estado final al instante
}
```

**Pasos de la novela visual:**

| Paso | Datos | Hace |
|---|---|---|
| `ShowStep` | Personaje, posición (1–4), emoción | El retrato entra deslizándose a su posición. Si la posición está ocupada, reemplaza al anterior |
| `HideStep` | Personaje | El retrato sale |
| `LineStep` | Personaje, emoción (opcional), texto, por radio (sí/no) | Resalta al que habla, cambia su emoción si se indica, escribe el texto y espera a que el jugador avance |
| `EmotionStep` | Personaje, emoción | Cambia la emoción sin hablar (una reacción) |
| `IllustrationStep` | Sprite, transición (corte o fundido) | Cambia la ilustración central |
| `WaitStep` | Segundos | Pausa |
| `AudioStep` | `SfxData` o `MusicTrack` | Sonido, cambio de música o silencio |

**Pasos dentro del nivel:**

| Paso | Datos | Hace |
|---|---|---|
| `TeleportStep` | Entrar o salir | El jugador aparece o se va con `FullBody_Teleport_In` / `Out` |
| `CameraStep` | Punto o jefe, duración, sacudida opcional | Lleva la cámara a un punto (`SetTarget`, 13) o la regresa al jugador |
| `BossIntroStep` | Jefe | El jefe hace su pose `Boss_Intro` mientras se llena su barra de vida (14.2) |

- La novela visual **se abre** con un fundido cuando empieza el primer paso de novela visual y **se cierra** con otro fundido cuando sigue un paso dentro del nivel o termina la escena.
- `waitToFinish = false` permite pasos en paralelo, por ejemplo cambiar la ilustración mientras un personaje entra.
- Para agregar pasos en el inspector (`[SerializeReference]`) hace falta un editor pequeño con un menú para elegir el tipo de paso.
- Todo el texto vive en String Tables del paquete *Localization*, en español e inglés (15.3).

#### Personajes en las escenas

- **Las escenas son las mismas sin importar el personaje elegido** (1.4). Se escriben con los personajes por nombre; el jugador no necesita aparecer en ellas.
- **Personajes que aún no se unen:** los pasos de un personaje que todavía no está disponible se **omiten**. Las escenas se escriben para que sus líneas se puedan quitar sin romper el diálogo.
- En el Simulador solo aparece Rosslyn (además del General).
- **Si se juega con Neotisleo:**
  - En los niveles 1 a 9 (que solo se juegan con él al repetirlos, porque se desbloquea con los prisioneros de esos niveles), las escenas **no cambian**.
  - **En la misión final (Torre alienígena)**, sus líneas de mando las dice **Goldo, otro general, por radio**. `SpeakerData.standIn` de Neotisleo apunta al `SpeakerData` de Goldo, y `CutsceneRunner` solo lo aplica en el nivel con `MissionData.isFinal`.

#### Comportamiento

- **Efecto máquina de escribir:** el texto aparece letra por letra (40 caracteres por segundo **[AJUSTAR]**), con un sonido corto cada pocas letras.
- **Avanzar** (`Jump`, `Fire` o `Enter`): si la línea aún se está escribiendo, se muestra completa; si ya terminó, pasa a la siguiente.
- **Saltar escena** (`Pause` / `Esc`): termina la escena completa. `CutsceneRunner` llama a `Skip` en cada paso pendiente, en orden, para que el jugador, la cámara, el jefe y la música queden en su estado final. Útil al repetir niveles.
- Mientras hay escena, el jugador está en el estado `Disabled`, el HUD se oculta (salvo la barra del jefe en su entrada), el menú de pausa no se abre y los enemigos están en pausa.
- Se activa el Action Map `Dialogue` y se desactiva `Gameplay`, para que los botones de avanzar no disparen ni salten.

#### Arte

- **Retratos:** una tira horizontal por personaje en `Art/Portraits/<Personaje>/`, con un frame por emoción en el orden del `enum`. Un setup los corta y los asigna al `SpeakerData`, igual que el arte de las habilidades (8.8).
- **Ilustraciones:** `Art/Illustrations/<Nivel>/`, un PNG por ilustración.
- **Volumen de arte:** unos 5 personajes × 6 emociones, más el General, su sustituto y los 9 jefes, son **unos 60 retratos**. Con 2 o 3 ilustraciones por nivel, son **20–30 ilustraciones**. Se reutilizan ilustraciones entre las escenas de un mismo nivel (2 o 3 por nivel).

#### Componentes

| Componente | Hace |
|---|---|
| `CutsceneRunner` | Recibe un `CutsceneData`, ejecuta los pasos (en orden o en paralelo), abre y cierra la novela visual, maneja el salto y lanza el evento `Finished` |
| `CutsceneStep` y sus tipos | Cada tipo de paso de las tablas de arriba |
| `VisualNovelView` | La UI: fondo, ilustración central, 4 posiciones de retrato con su resaltado y animaciones de entrada y salida |
| `DialogueBox` | El cuadro de texto: nombre, texto con efecto máquina de escribir, marco de radio e indicador ▼ |
| `CutsceneTrigger` | Zona del nivel que inicia una escena una sola vez |

`MissionController` (en cada escena de nivel) llama a `CutsceneRunner.Play(introCutscene)` al empezar y espera `Finished` antes de dar el control al jugador. Al vencer al jefe hace lo mismo con `outroCutscene` antes de volver a la selección de nivel.

**Validación** (`Game/Validate`): cada `SpeakerData` tiene los retratos de las emociones que usan sus líneas, y cada `CutsceneData` empieza con una ilustración.

#### Fuera de alcance

Opciones de respuesta, animación de los retratos (parpadeo, boca) y voces.

---

## 16. Plan de trabajo

### Hito 1 — Prototipo de movimiento (cajas de colores)

**Tareas:**
- [x] Proyecto Unity 6 con la plantilla Universal 2D, Input System y Cinemachine
- [x] Repositorio en GitHub: `git init` en la carpeta del proyecto, `.gitignore` de Unity, `.gitattributes` y primer commit con este documento (2.4)
- [x] Verificar *Visible Meta Files* y *Asset Serialization = Force Text* (son los valores por defecto en Unity 6) (2.4)
- [x] Fixed Timestep en 1/60, VSync activado e `Interpolate` en el Rigidbody2D del jugador (2.3)
- [x] Definir *Company Name* y *Product Name* en Player Settings (fijan la ruta de guardado, ver 15.4)
- [x] Pixel Perfect Camera a 320×180 y preset de importación de sprites
- [x] Importar los placeholders (abajo) y validar PPU 16, pivote en los pies y filtro Point
- [x] `StateMachine` + `IState`
- [x] `PlayerInputReader` con buffers, `PlayerMotor`, `PlayerMovementData`
- [x] Estados `Idle`, `Run`, `Crouch` y `Airborne`
- [x] Coyote time, jump buffer y salto variable
- [x] `Dash`, `AirDash` y dash jump
- [x] Color del SpriteRenderer según el estado de locomoción (solo para depurar)
- [x] Nivel de prueba con tilemap de 16 px (abajo)
- [x] Validar la resolución de 320×180 (decisión pendiente 1): se mantiene 320×180
- [x] Validar que el salto y el dash midan lo mismo a 60 Hz y con más FPS (2.3)

**Sprites placeholder (Piskel):**

No se necesita arte final en este hito. Estos placeholders sirven para validar el pipeline de importación desde el primer día.

| Sprite | Lienzo | Contenido |
|---|---|---|
| `Player_Placeholder` | 48×64 | Rectángulo sólido de 12×30 px centrado en x = 24, con la base en y = 12 (mismo tamaño que el collider). Una marca de otro color en el lado derecho, a la altura de la cabeza, para ver hacia dónde mira |
| `Player_Placeholder_Crouch` | 48×64 | Opcional. Mismo rectángulo, de 12×15 px |
| `Tile_Ground` | 16×16 | Color sólido con borde de 1 px más oscuro, para contar tiles a simple vista |
| `Tile_Platform` | 16×16 | Otro color, con solo la parte superior marcada (plataforma atravesable) |
| `Tile_Grid` | 16×16 | Casi transparente, con líneas tenues en el borde. Va en un Tilemap de fondo para medir alturas y distancias |

Los estados (correr, saltar, dash) se distinguen cambiando el `color` del SpriteRenderer por código, no con sprites distintos.

**Nivel de prueba:**

| Elemento | Qué valida |
|---|---|
| Escalones de 1, 2, 3 y 4 tiles de alto | El salto completo (3.5 tiles) supera el de 3 y no el de 4 |
| Fosos de 3, 5, 7 y 9 tiles de ancho | Distancia del salto normal, del dash jump y del air dash |
| Plataforma flotante | Coyote time al caminar fuera del borde |
| Plataformas atravesables | Subir desde abajo y aterrizar encima |
| Pared alta | El personaje no se pega a la pared (material sin fricción) |
| Pasillo largo y plano | Velocidad de correr, dash y cámara con *look ahead* |

**Arte en paralelo para el Hito 3**, en orden de prioridad:
1. `Legs_Idle` + `Torso_Idle`: definen el diseño de Rosslyn y la altura de la cintura.
2. `Legs_Run` + `Torso_Run`.
3. `Legs_JumpUp`, `Legs_Fall` + `Torso_JumpUp`, `Torso_Fall`.
4. `Legs_Dash` + `Torso_Dash`.
5. `Legs_Crouch` + `Torso_Crouch`.

### Hito 2 — Disparo y daño

Herramienta: menú **Game → Setup** (`Scripts/Editor/GameSetup.cs`), que crea el arte placeholder, los datos, la matriz de colisiones, los prefabs (jugador, bala, muñeco y zona de daño) y la escena `Test_Movement`.

- [x] FSM de acción: `Ready` y `Shooting` con shoot pose, bloqueada durante el dash
- [x] `PlayerBuster` + `BusterData` + pool de proyectiles
- [x] Las cuatro direcciones de apuntado y el disparo agachado
- [x] Matriz de capas, `IDamageable`, `PlayerHealth` con knockback e i-frames
- [x] Muñeco de práctica que recibe daño
- [x] FSM de nivel superior: `Alive`, `Hurt`, `Dead`, `Respawning` y `Disabled` (respawn temporal en el punto de inicio hasta los checkpoints del Hito 5). `Disabled` se probará con los diálogos y el jefe
- [x] `SfxPlayer`, `SfxData` y `AudioMixer` con los primeros sonidos de librería (disparo, salto, dash, daño) y `CREDITS.md` iniciado. Falta `Sfx_PlayerDeath`, que se importa junto con la animación de muerte (Hito 3)

### Hito 3 — Personaje animado
- [x] Sprites en Piskel de piernas, torso y cuerpo completo (lienzo 48×64, pies en y = 12)
- [x] `Legs.controller`, `Torso.controller` y `FullBody.controller`
- [x] `PlayerAnimation.Sync()` con las poses de apuntado arriba y en diagonal
- [x] Validar alineación en Pixel Perfect al correr, saltar, agacharse y hacer dash, y la boca del cañón en cada pose
- [x] Mover la tabla de clips de `PlayerArtSetup.cs` a un asset `SpritesheetLayout` (4.2.1)
- [x] Importar `Sfx_PlayerDeath` junto con la animación de muerte (pendiente del Hito 2)

### Hito 4 — Habilidades

Ordenadas de la más sencilla a la más compleja; cada una agrega una pieza nueva al sistema. Se prueban con Rosslyn y el modo de prueba, aunque el taladro y el láser son de Veyron:

- [x] `PlayerEnergy`, `PlayerAbilities`, `AbilityData`, `AbilityEffect`, estado `UsingAbility` y cambio de habilidad
- [x] `AbilityEffect` con `IsFinished` y `ButtonReleased`; bases `TimedAbilityEffect` y `HeldAbilityEffect` (8.4, 8.5)
- [x] Valores propios de cada habilidad en su efecto, no en `AbilityData`; `torsoPose` en lugar de `forceForwardPose` (8.3)
- [x] `DamageInfo` con `knockback` y `stunDuration` (10.1)
- [x] Asset `ObstacleInteraction` (`Break`, `Melt`) y `AbilityData.obstacleInteractions`; capa `AbilityObstacle` en lugar de `Breakable` y `Meltable` (15.2). Esas dos capas ya existen en `TagManager` y en `GameSetup.cs` (matriz de colisiones y máscaras `groundMask`, `ceilingMask` y `solidMask` del motor y del proyectil): hay que reemplazarlas ahí también
- [x] HUD de vida, energía e icono de la habilidad
- [x] **Taladro:** extiende `Projectile` con `pierceCount`
- [x] **Granada de goma:** física con rebote + `Explosion`; arco por dirección (4 direcciones y agachada)
- [x] **Espada de fuego:** `MeleeEffect` con `DamageHitbox` de un golpe + ventana de frames activos + 5 clips `Torso_Melee*` (`DirectionalClips`), hitbox por dirección y rebote del tajo hacia abajo (`LocomotionFSM.Bounce`). Falta dibujar los clips `Torso_Melee*` y el arte final de las habilidades (8.8)
- [x] **Lanzallamas:** modo sostenido, consumo por segundo, `DamageHitbox` por tick y chorro de `FlamePuff` con pool
- [x] **Láser:** `BeamCaster` + `LineRenderer` pixel perfect (validar la diagonal)
- [x] Modo de prueba (solo en el editor) que desbloquea todas las habilidades y llena la energía: menú **Game → Modo de prueba → Todas las habilidades** y `CharacterData.testModeAbilities` (el taladro y el láser se prueban con Rosslyn)
- [x] `SaveSystem` base: `SaveData`, escritura segura con respaldo, versión, menú `Game/Save` del editor y guardado de habilidades desbloqueadas por personaje
- [x] `CharacterData` (5.4): mover vida, energía, `PlayerMovementData`, `BusterData` y habilidades al asset del personaje; `PlayerController.Initialize` y spawn del jugador desde datos

### Hito 5 — Enemigos base y nivel de prueba
- [ ] Arquitectura de enemigos por piezas (14.1): `EnemyData`, `EnemyWeaponData`, biblioteca de estados, `OnDeath`
- [ ] Primeros enemigos, que cubren las piezas básicas: **Torreta** (apunta en 8 direcciones), **Soldado** (patrulla y dispara) y **Robot volador** (vuelo por puntos), con `Stunned`, `Frozen` y reacción al empuje
- [ ] `DamageZone` (8.7)
- [ ] Pickups de vida y energía con drops
- [ ] Peligros, checkpoints, muerte y respawn
- [ ] `AbilityObstacleTilemap` en la capa `AbilityObstacle`, con interacciones `ObstacleInteraction` (`Break`, `Melt`): bloques destructibles (taladro) y muros de metal con calor acumulado (lanzallamas)
- [ ] `AbilityPickup`: coleccionable de habilidad que recoge cualquier personaje y se desbloquea para su dueño, secuencia al recogerlo, guardado inmediato y validación de la cadena de obstáculos (8.9)
- [ ] `Prisoner`: rescate, contador en pantalla, guardado inmediato y `MissionData.prisonerIds` (15.2)
- [ ] Cámara con confiner por sección

### Hito 6 — Simulador y Bosque completos

El Simulador (nivel 1) y el Bosque (nivel 2) con la Excavadora de combate, de principio a fin.

- [ ] Misión 0 (simulador): misión de introducción con Rosslyn, sin tutorial explícito, arranque directo desde la partida nueva, `forcedCharacter` y `MissionCatalog.firstMission` (15.1)
- [ ] Bosque: nivel, enemigos y la **Excavadora de combate** con sus fases (14.2)
- [ ] `GameManager`, flujo de escenas, menú de pausa con selección de habilidad
- [ ] `MissionData`, `MissionCatalog` (orden de los 10 niveles) y pantalla de selección de nivel con briefing y coleccionables encontrados
- [ ] Validación del editor `Game/Validate` para los coleccionables (8.9)
- [ ] `CharacterCatalog` y pantalla de selección de personaje (15.1)
- [ ] Guardado de misiones completadas, `settings.json` y autoguardado al completar una misión
- [ ] Pantalla de slots: continuar, cargar, partida nueva y borrar (con confirmación)
- [ ] Opciones de PC: pantalla, audio, remapeo de controles e idioma
- [ ] Paquete *Localization*: String Tables en español e inglés, `LocalizedString` en los datos y fuente con acentos (15.3)
- [ ] `MusicPlayer` con intro + loop y crossfade, música de misión y de jefe
- [ ] Snapshot de pausa y volúmenes en opciones
- [ ] Sistema de escenas estilo novela visual (15.7): `CutsceneData` con pasos, `CutsceneRunner` con salto, `VisualNovelView` (ilustración, 4 posiciones, resaltado del que habla), `DialogueBox` con marco de radio, `CutsceneTrigger` y editor de pasos
- [ ] Escenas de inicio y cierre del nivel, y escena del jefe: cámara, diálogo, `Boss_Intro` y barra de vida; versión corta al reintentar (`firstTimeOnly`)
- [ ] Retratos por emoción de Rosslyn, Veyron, el General y la Excavadora, e ilustraciones del Simulador y el Bosque (15.7)

### Hito 7 — Segundo personaje (Veyron)

Depende de las decisiones pendientes 8, 10 y 11 (17.2).

- [ ] `PlayerMovementData` de Veyron con la velocidad original (6 u/s) y la de Rosslyn subida (~7 u/s)
- [ ] Disparo básico corto y potente: `BusterData` propio con `maxRange` y VFX de disipación (sección 7)
- [ ] Spritesheet con la misma cuadrícula que Rosslyn (4.2.1) y `PlayerArtSetup` por personaje; mover el arte y los clips de Rosslyn a `Characters/<Personaje>/`
- [ ] Prefab variant, `CharacterData`, `PlayerMovementData` y `BusterData` propios
- [ ] Cuchillo (reutiliza `MeleeEffect`) y arma sónica (`BlastEffect`), con su arte; el taladro ya existe desde el Hito 4 (el láser pasa a Agente X)
- [ ] Colocar los coleccionables de Rosslyn y Veyron en el Bosque y validar el ciclo de 8.9 (sin dependencias circulares)
- [ ] Imagen de la pantalla de selección
- [ ] Recorrer el nivel de prueba y el Bosque con los dos personajes (mínimos de salto de 5.4)

### Hito 8 — Aquamarine y Agente X
- [ ] Sus `CharacterData`, hojas, prefabs, `BusterData` (burbujas lentas y bala láser rápida) y desbloqueo por historia (`requiredMissions`)
- [ ] Planeo con burbuja (6.3) y salto alto (valor)
- [ ] Ráfaga de burbujas (`BlastEffect` con otro arte), chorro de agua (`destroyOnGround`, interacción `Extinguish` y obstáculo de fuego) y rayo congelante (láser + `freezeBuildup` y estado `Frozen`)
- [ ] Láser movido a Agente X, dron (`CompanionDrone`) y granada incendiaria (`DamageZone`)
- [ ] Retratos por emoción de Aquamarine y Agente X, y escenas de incorporación al escuadrón

### Hito 9 — Vehículos
- [ ] Estado `Piloting` (6.2), `IPilotable`, `VehicleData`, cámara y HUD del vehículo (18)
- [ ] Sistema de habilidades y energía reutilizable por los vehículos (18.4); subir saltando encima y bajar con salto + arriba
- [ ] **Robotanque:** ametralladoras automáticas en 8 direcciones, lanzacohetes y tenazas
- [ ] **Exoesqueleto:** disparo cargado, rayo de corto alcance, misil y arma láser

### Hito 10 — Resto de enemigos
Agrupados por la pieza nueva que necesitan (14.1):
- [ ] Variantes de datos: soldados, moto, fire bat, tank, araña cañón
- [ ] Cuerpo a cuerpo por distancia: furro, escarabajo, mole
- [ ] Embestida: el de las recargas
- [ ] Enemigos compuestos (`EnemyGroup`): soldado carreta, soldado carreola, robot con escudo
- [ ] Al morir crea otro: robo-dron; generador: araña spawner
- [ ] Autodestrucción y charcos: abuelita, Mr. Bombastic, alterado
- [ ] Rebote y techo: medusa (`BounceTop`), oruga
- [ ] Agarre: mantis atea y estado `Grabbed` (6.2)

### Hito 11 — Niveles 3 a 10 y sus jefes
Un nivel a la vez, en orden, cada uno con su jefe, sus escenas, su música y sus coleccionables:
- [ ] 3. Minas — Ayahuastli
- [ ] 4. Montaña — Robot de batalla
- [ ] 5. Bosque en llamas — Wolfang
- [ ] 6. Laboratorio — Gosarou
- [ ] 7. Tren militar — Zaipoc
- [ ] 8. Castillo — Grallet
- [ ] 9. Ciudad alienígena — Narabee
- [ ] 10. Torre alienígena — Coliel, y créditos

### Hito 12 — Neotisleo
Se une antes de la misión final si se rescató a los 24 prisioneros (15.2).
- [ ] `CharacterEntry.requiresAllPrisoners` + Ciudad alienígena en `requiredMissions`, escena en la que los prisioneros lo convencen, `CharacterData` con más energía, hoja y prefab
- [ ] Disco de energía (`DrillEffect` sin `Break`), espada de luz (`MeleeEffect`) y munición eléctrica (`Projectile` + `Explosion`)
- [ ] Escenas cuando se juega con él (17.2)

---

## 17. Decisiones

### 17.1 Tomadas

| Decisión | Resultado |
|---|---|
| Versión de Unity | Unity 6 |
| Plataforma | PC (teclado y gamepad) |
| Tamaño del personaje | 32 px de alto |
| Tamaño de tile | 16×16 px · PPU 16 |
| Resolución de referencia | 320×180 (validada en el Hito 1) |
| Herramienta de arte | Piskel (lienzo de 48×64 para el jugador, pies en y = 12) |
| Framerate y física | 60 FPS con VSync, Fixed Timestep de 1/60, temporizadores en segundos |
| Control de versiones | GitHub, sin Git LFS |
| Disparo | Normal, un tiro por pulsación, sin carga (los personajes; el Exoesqueleto sí carga) |
| Direcciones de disparo | Adelante, diagonal arriba-adelante, arriba y abajo (solo en el aire) |
| Pose del torso | Cambia al mantener arriba o arriba-adelante (`Torso_Aim_*`); las 4 direcciones tienen además su clip de disparo (`Torso_Shoot_*`) |
| Dash | Suelo + aire (uno por salto), dash jump con impulso (también al salir de una orilla durante el dash), sin i-frames |
| Disparar durante el dash | No (tampoco usar habilidades) |
| Dash desde agachado | No |
| Collider durante el dash | Sin cambios |
| Habilidades | Propias de cada personaje, con progreso por personaje; barra de energía compartida, siguiente/anterior |
| Habilidades de Rosslyn | Lanzallamas, granada de goma y espada de fuego |
| Habilidades de Veyron | Taladro, arma sónica y cuchillo (el láser pasó a Agente X) |
| Arma sónica | Como la escopeta de Metal Slug: área corta frente al personaje, mucho daño a todo lo que toca, poca cadencia. Veyron: boom sónico; Aquamarine: ráfaga de burbujas (mismo `BlastEffect`) |
| Habilidades de Aquamarine | Ráfaga de burbujas (efecto del arma sónica), chorro de agua (granada que se deshace en el suelo y apaga el fuego) y rayo congelante (láser que congela) |
| Habilidades de Agente X | Láser, dron y granada incendiaria |
| Habilidades de Neotisleo | Disco de energía (taladro sin romper muros), espada de luz (espada con más alcance) y munición eléctrica |
| Pasivas | Aquamarine: planeo con burbuja. Agente X: salto más alto. Neotisleo: más energía |
| Obtención de habilidades | Coleccionables escondidos en los niveles; los jefes no entregan habilidades. Se guardan al recogerlos |
| Cuchillo | Reutiliza el sistema de la espada (`MeleeEffect`, `Torso_Melee*`): 4 direcciones, rebote hacia abajo y destruye proyectiles |
| Frames activos del tajo | Ventana de frames en `MeleeEffect`, sin Animation Events |
| Instancias de los efectos | Una por habilidad, hija del jugador; lo que se lanza sale de pools del efecto |
| Fogonazo del disparo | No hay; el movimiento del arma en los clips de disparo es el feedback |
| Escalabilidad de habilidades | El efecto decide cuándo termina (`IsFinished`); obstáculos por interacción (`ObstacleInteraction`) en una sola capa; `DamageInfo` con empuje y aturdimiento; valores propios en el efecto; layout del spritesheet en un asset; selección de habilidad también desde la pausa |
| Modos de activación | Por pulsación (granada, espada, taladro, cuchillo) y sostenida con consumo por segundo (lanzallamas, láser) |
| Dash y habilidades | El dash cancela las sostenidas; no se puede hacer dash durante una por pulsación |
| Feedback de la habilidad | Solo icono en el HUD; el personaje no cambia de color |
| Control en el aire | Libre |
| Animación del torso y las piernas | `Animator.Play()` por código, sin transiciones |
| Aim lock | No por ahora |
| Balas del disparo básico en pantalla | Sin límite (solo cadencia) |
| Vida | 28 puntos por defecto; cada personaje define la suya |
| Vidas | Infinitas, con checkpoints |
| Recarga de energía | Al morir, al empezar el nivel y con pickups de enemigos. Los checkpoints no recargan |
| HUD | Canvas pixel perfect con escala entera |
| Granada de goma | Sale del cañón con la pose de disparo de la dirección; arco distinto en cada una de las 4 direcciones y agachada; sin límite en pantalla |
| Espada de fuego | 4 direcciones y agachada, con un clip `Torso_Melee*` por dirección; destruye proyectiles enemigos; el tajo hacia abajo rebota al golpear (pogo) |
| Lanzallamas | Movimiento al 50%; visual de llamas sueltas que aparecen y se disipan; daño con una zona fija |
| Taladro | Atraviesa enemigos sin límite; rompe un bloque destructible y se detiene |
| Obstáculos por habilidad | Bloques destructibles (taladro, Veyron), muros de metal que se funden con calor acumulado (lanzallamas, Rosslyn) y fuego sólido que daña al tocarlo y se apaga con agua (chorro de agua, Aquamarine). Solo esas tres habilidades los abren. Nunca cierran el camino principal, tampoco en el nivel final |
| Repetir niveles | Sí, con cualquier personaje disponible, para abrir caminos y buscar coleccionables |
| Diálogos | Novela visual al estilo de *Sonic Battle*: ilustración central que puede cambiar, de 1 a 4 personajes a los lados con retratos por emoción, el que habla con opacidad completa y los demás atenuados. Inicio y cierre de nivel, sala de cada jefe (sin diálogo al reintentar) y momentos especiales. Se pueden saltar |
| Escenas | Lista de pasos (`CutsceneData`), no Timeline. La novela visual reemplaza a las escenas con personajes caminando en el nivel; dentro del nivel solo quedan el teletransporte, la cámara y la entrada del jefe. Las escenas son las mismas para todos los personajes |
| Historia | Un escuadrón recibe misiones del General Neotisleo contra una amenaza alienígena; el jefe final es Coliel en la Torre alienígena. Ya no hay traición del comandante |
| Personajes | 5 jugables en la versión 1, elegidos en una pantalla antes de cada nivel: Rosslyn y Veyron desde el inicio, Aquamarine y Agente X con la historia, Neotisleo al rescatar a todos los prisioneros |
| Prisioneros | 3 por nivel en los niveles 2 a 9 (24; ni el Simulador ni la Torre tienen), los rescata cualquier personaje tocándolos, se guardan al rescatarlos; con todos, antes de la misión final convencen a Neotisleo de pelear junto a Rosslyn sin destruir Coleópolis, y se vuelve jugable. Un solo final |
| Neotisleo en las escenas | Las mismas escenas. Solo en la misión final, si se juega con él, sus líneas de mando las dice Goldo, otro general, por radio (`SpeakerData.standIn`) |
| Diferencias entre personajes | Estadísticas, disparo básico, habilidades y pasiva. Todos comparten el movimiento (correr, saltar, agacharse, dash) y la cuadrícula del spritesheet |
| Progreso entre personajes | Las misiones completadas son del slot; las habilidades desbloqueadas son de cada personaje |
| Primer personaje | Rosslyn, la soldado espacial |
| Segundo personaje | Veyron, compañero de escuadrón de Rosslyn |
| Disparos básicos de los nuevos | Aquamarine: burbujas más lentas. Agente X: bala más rápida con menos daño. Neotisleo: normal |
| Disparo básico de Veyron | Más daño y mucho menos alcance que el bláster (`maxRange`); mismas direcciones y cadencia |
| Diálogos y personajes | Los mismos para todos: en el lore el escuadrón va a cada misión |
| Audio | `AudioMixer` (Music, SFX/Gameplay, SFX/UI), `MusicPlayer` con intro + loop y crossfade, `SfxPlayer` con pool, variación, límite de voces y cooldown |
| Origen del audio | SFX de librerías (con registro de licencias en `CREDITS.md`); música compuesta por el equipo, exportada como intro + loop en `.ogg` |
| Guardado | 3 slots de partida; autoguardado al completar misiones y al recoger habilidades; `settings.json` separado de los slots; escritura segura con respaldo; versión y migraciones desde el inicio; no se guarda a mitad de misión |
| Láser | De Agente X. Atraviesa enemigos; jugador quieto en el suelo |
| Debilidades de jefes | No |
| Misión 0 | El Simulador de combate (nivel 1): misión de introducción al estilo de la primera de Mega Man X (sin tutorial explícito), solo con Rosslyn. Es lo primero de una partida nueva |
| Progresión | 10 niveles en orden (Simulador, Bosque, Minas, Montaña, Bosque en llamas, Laboratorio, Tren militar, Castillo, Ciudad alienígena, Torre alienígena); 9 jefes; todos se pueden vencer solo con el disparo básico de cualquier personaje |
| Vehículos | En la versión 1: Robotanque y Exoesqueleto, con su propia vida, energía, disparo y habilidades (18) |
| Enemigos | 22 tipos (14.1), armados con piezas reutilizables; arte en `Enemies.png` |
| Velocidad de Veyron | Más lento: conserva la velocidad original (6 u/s); Rosslyn sube a ~7 u/s |
| Incorporaciones | Aquamarine después del Bosque en llamas; Agente X después del Tren militar |
| Vehículos por nivel | Exoesqueleto: Minas, Laboratorio, Tren militar. Robotanque: Montaña, Torre alienígena |
| Subir y bajar del vehículo | Subir saltando encima; bajar saltando con arriba |
| Vehículos: recarga y obstáculos | Se recargan con los mismos pickups; no rompen obstáculos |
| Apuntado del Robotanque | 8 direcciones según la dirección presionada, sin bloqueo |
| Arma láser del Exoesqueleto | Rayo como el láser, más potente que el de Agente X |
| Coleccionables de habilidad | Los recoge cualquier personaje y se desbloquean para su dueño; se pueden poner detrás de obstáculos de otros personajes |
| Simulador | Un nivel más, sin jefe; termina en una línea de meta. No se puede saltar; se puede repetir. Sin habilidades prestadas |
| Novela visual | General por radio en las misiones y en persona en momentos importantes; 6 emociones (2 para jefes); bustos de 64×96 e ilustración de 144×104; 2 o 3 ilustraciones por nivel; panel fijo detrás |
| Temas de jefe | Uno compartido + el de Coliel |
| Personajes sin desbloquear | Silueta con "???" en la selección |
| Disparo de Agente X | 1 de daño; su ventaja es la velocidad de la bala |
| Estados | Los enemigos congelados sirven de plataforma; los jefes no se congelan ni se aturden |
| Dron | Si ya hay uno, no se activa ni gasta energía |
| Mantis | 1 de daño cada 0.5 s; unas 8 pulsaciones para soltarse |
| Munición eléctrica | Aturde |
| Dificultad | Una sola |
| Idiomas | Español e inglés, con el paquete *Localization* desde el inicio |

### 17.2 Pendientes

| # | Decisión | Opciones | Recomendación | Bloquea |
|---|---|---|---|---|
| 2 | En qué nivel se esconde cada coleccionable y cada prisionero | — | Que al menos una de las habilidades que abren caminos se alcance sin habilidades y que la cadena no tenga ciclos; `Game/Validate` lo revisa (8.9) | Hito 6 |
| 30 | Qué enemigos van en cada nivel | — | Agruparlos por tema (bosque: soldados, abuelita, oruga; minas: mole, araña…) | Hito 10 |
| 32 | Diseño de jefes pendientes | Robot de batalla, Grallet, Narabee, Coliel | — | Hito 11 |
| 33 | Historia detallada | — | Definir quién es Coliel y por qué se une cada personaje | Hito 6 |

---

## 18. Vehículos (estilo Metal Slug)

Los vehículos entran en la **versión 1**. Hay dos: el **Robotanque** y el **Exoesqueleto** (arte en `Enemies.png`). Cualquier personaje puede pilotearlos.

| Vehículo | Niveles |
|---|---|
| Exoesqueleto | Minas, Laboratorio, Tren militar |
| Robotanque | Montaña, Torre alienígena |

### 18.1 Comportamiento base

- **Subir:** saltando sobre el vehículo (al caer encima, el personaje entra).
- **Dentro del vehículo:** el personaje no recibe daño; el daño lo recibe el vehículo, que tiene su **propia barra de vida**.
- **Energía y habilidades propias:** cada vehículo tiene su **propia barra de energía** y **sus propias habilidades**, que reemplazan a las del personaje mientras lo pilotea. Se cambian con los mismos botones de siguiente y anterior y se usan con el botón de habilidad.
- **Disparo principal propio** con el botón de disparo.
- **Bajar:** saltando con **arriba** presionado. Al bajar, el personaje tiene invulnerabilidad breve. Saltar sin arriba hace saltar al vehículo.
- No hace falta una acción nueva en el Input System: subir y bajar usan `Jump` y `Move` (12).
- **Vehículo destruido:** expulsa al personaje automáticamente y explota, dañando a los enemigos cercanos (como el Metal Slug original).
- **Recarga:** los pickups de vida y energía recargan al vehículo mientras se pilotea.
- Los vehículos **no rompen obstáculos**: esos caminos quedan ligados a los personajes.

### 18.2 Robotanque

| | |
|---|---|
| Disparo principal | Ametralladoras en las tenazas. **Automático**: mientras se mantiene el botón, dispara en ráfaga continua |
| Apuntado | **8 direcciones**, según la dirección presionada (sin bloqueo al disparar) |
| Habilidad 1 | **Lanzacohetes** (gasta energía del vehículo) |
| Habilidad 2 | **Ataque con las tenazas**: cuerpo a cuerpo, **no gasta energía** (`MeleeEffect` con costo 0) |

### 18.3 Exoesqueleto

| | |
|---|---|
| Disparo principal | **Manual**. Al mantener el botón **se carga**, como el disparo de Mega Man X: soltar sin cargar da un tiro normal, con carga media un tiro más fuerte y con carga completa el más fuerte **[AJUSTAR]** niveles y tiempos |
| Apuntado | Como el personaje: 4 direcciones (5.4, 6.5) |
| Habilidad 1 | **Rayo de corto alcance**, como el arma de Winston en Overwatch: sostenido, daña por tick a los enemigos que estén delante a poca distancia sin tener que apuntar con precisión (`DamageHitbox` por tick en forma de cono o rectángulo corto) |
| Habilidad 2 | **Misil** que lanza desde el brazo (`Projectile` + `Explosion`) |
| Habilidad 3 | **Arma láser**: un rayo como el láser (`LaserEffect`), **más potente que el de Agente X**, que apunta igual que el personaje |

El disparo cargado es nuevo; solo lo tiene el Exoesqueleto (los personajes no cargan, 1.3). Encaja en el modelo de habilidades: el arma cuenta el tiempo mientras se mantiene el botón y dispara en `ButtonReleased` (8.4).

### 18.4 Arquitectura

```csharp
public interface IPilotable
{
    void Enter(PlayerController pilot);
    void Exit();
    void Tick(InputSnapshot input, float dt);
    void FixedTick(float dt);
}
```

- `PlayerController` pasa al estado **`Piloting`** (6.2). Las FSM del jugador se desactivan, sus sprites y colliders se ocultan y el `InputSnapshot` se le pasa al vehículo.
- Cada vehículo es un prefab con su propia FSM (`Idle`, `Move`, `Airborne`, `Destroyed`), su `Rigidbody2D`, su vida (`IDamageable`) y un **`VehicleData`** (ScriptableObject): velocidad, salto, vida, energía, disparo principal (`VehicleWeaponData`: automático, manual o cargado, y modo de apuntado) y su lista de `AbilityData`.
- **Las habilidades del vehículo usan el mismo sistema que las del personaje** (`AbilityData` + `AbilityEffect`, sección 8). Para eso, la energía y la lista de habilidades salen de `PlayerEnergy` y `PlayerAbilities` a componentes que también puede tener el vehículo (por ejemplo, `Energy` y `AbilityUser`). Los efectos reciben el `AbilityContext` del vehículo (su FirePoint y su apuntado).
- **Apuntado en pixel art:** sin rotar sprites. El arma del Robotanque tiene un sprite por cada una de sus **8 direcciones**.
- **Cámara:** cambia su objetivo al vehículo mientras se pilotea (`SetTarget`, 13).
- **HUD:** muestra la vida y la energía del vehículo (`SegmentedBar`, 15.6) y el icono de su habilidad seleccionada.
- **Capas nuevas:** `Vehicle` y `VehicleHurtbox`.

---

## 19. Versión 2 (futuro)

Funciones planeadas para después de la versión 1. **No se implementan ahora**, pero la versión 1 se construye de forma que agregarlas no obligue a rehacer sistemas (ver 19.2).

### 19.1 Más niveles

La versión completa podría tener más niveles que los 10 de la versión 1.

- **Datos:** agregar un nivel es crear un `MissionData` nuevo, su escena y sumarlo al `MissionCatalog`. El guardado usa `id`, así que las partidas de la versión 1 siguen funcionando.
- **Coleccionables:** `MissionData.abilityPickups` puede estar vacío, para niveles sin habilidades escondidas.
- **Pantalla de selección:** debe admitir más niveles (desplazamiento o páginas).

**Decisiones de la versión 2:**

| # | Decisión | Opciones |
|---|---|---|
| 1 | Cuántos niveles tendrá la versión completa | — |
| 2 | ¿Los niveles nuevos esconden habilidades? | Sí (más habilidades) / solo algunos |
| 3 | Estructura de la historia | Un segundo acto después de Coliel / niveles opcionales |

### 19.2 Preparación en la versión 1

Cambios baratos que se hacen **desde la versión 1** para que la versión 2 no obligue a rehacer sistemas:

| Preparación | Dónde | Por qué |
|---|---|---|
| El input se lee en `PlayerInputReader` y se pasa como `InputSnapshot`; nadie más lee el Input System | 5.2, 12 | El controller redirige el input a un vehículo (ya se usa en la V1) |
| La FSM de nivel superior del jugador admite estados nuevos | 6.2 | `Grabbed` y `Piloting` no tocan la locomoción ni la acción |
| La cámara sigue un objetivo configurable, no al jugador fijo | 13 | Cambiar el objetivo al vehículo |
| Las barras del HUD son un componente reutilizable (`SegmentedBar`) | 15.6 | Vida, energía, jefe y vehículo usan la misma barra |
| El orden de los niveles se lee del `MissionCatalog` | 15.1 | Agregar niveles sin tocar código |
| `MissionData.abilityPickups` puede estar vacío | 15.1 | Niveles sin coleccionables |
| La pantalla de selección de nivel se arma desde el catálogo, no con botones fijos | 15.1 | Admitir más niveles |
| El guardado tiene `version` y `SaveMigrator` desde el inicio | 15.4 | Las partidas de la versión 1 se actualizan solas al formato de la versión 2 |
| La pantalla de selección de personaje se arma desde el `CharacterCatalog`, con condiciones de desbloqueo | 15.1 | Agregar personajes sin tocar código |
| `AbilityEffect` decide cuándo termina (`IsFinished`) | 8.4, 8.5 | El disparo cargado y otros modos sin tocar `UsingAbility` |
| Obstáculos por `ObstacleInteraction` en una sola capa `AbilityObstacle` | 15.2 | Tipos de obstáculo nuevos y más habilidades que los abran, sin capas nuevas |
| `DamageInfo` con `knockback`, `stunDuration` y `freezeBuildup` | 10.1 | Armas que empujan, aturden o congelan |
| Valores propios de cada habilidad en su efecto, no en `AbilityData` | 8.3 | `AbilityData` no crece con cada habilidad |
| Tabla de clips en un asset `SpritesheetLayout` | 4.2.1 | Clips nuevos sin tocar `PlayerArtSetup.cs` |
| Selección de habilidad desde el menú de pausa | 8.1, 15.1 | Elegir rápido con más habilidades por personaje |
| El progreso de habilidades se guarda en una lista por `id` de personaje | 15.4 | Personajes nuevos sin migración |
| Todo personaje es un `CharacterData` + prefab variant con la misma cuadrícula de spritesheet | 4.2.1, 5.4 | Personajes nuevos reutilizan todo el pipeline |
| Enemigos por piezas (`EnemyData`, `EnemyWeaponData`, biblioteca de estados) | 14.1 | Enemigos nuevos como combinaciones de piezas existentes |
