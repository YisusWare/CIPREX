# Documento Técnico — Run & Gun 2D

> **Estado:** borrador v26 (2026-10-14)
> **Motor:** Unity 6 · **Plataforma:** PC (teclado y gamepad) · **Arte:** pixel art hecho en Piskel
> **Referencias:** Metal Slug (personaje dividido en dos mitades), Mega Man (habilidades), Mega Man X (dash)
> Las secciones marcadas con **[DECIDIR]** son decisiones abiertas y las marcadas con **[AJUSTAR]** son valores que se afinan al probar. Todas están en la sección 17.

---

## 1. Visión general

Juego de acción 2D de scroll lateral para PC. Antes de cada misión, el jugador **elige entre 2 personajes**, y cada uno tiene sus propias estadísticas, su disparo básico y sus habilidades. Los dos corren, saltan, se agachan, hacen dash y disparan en varias direcciones al mismo tiempo. Las habilidades especiales se usan con un botón propio y comparten una sola barra de energía.

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
| Personajes | **2 personajes jugables** en la versión 1, que se eligen en una pantalla antes de cada misión (15.1). Cada uno tiene sus estadísticas, su disparo básico y sus habilidades. En la versión 2 se agrega **el comandante** como tercer personaje al completar todas las misiones (18.4) |
| Progresión | Una partida nueva empieza con la **Misión 0**, un entrenamiento en simulador con Rosslyn (15.1). Después vienen 5 misiones de selección libre, cada una con un jefe. Al completarlas se abre la misión final. Las habilidades **no las dan los jefes**: se encuentran como **coleccionables escondidos en los niveles** (8.9) |
| Historia | Dos soldados espaciales cumplen las misiones que les da su comandante, quien resulta ser un traidor y es el jefe final (ver 1.4). Cada misión tiene un diálogo corto al inicio y al final, el mismo para los dos personajes |
| Habilidades | Cada personaje tiene **su propio juego de habilidades** y su propio progreso de desbloqueo. **Rosslyn:** lanzallamas, granada de goma y espada de fuego. **Veyron:** taladro, láser, cuchillo y arma sónica. Se obtienen como coleccionables en el escenario; el taladro y el lanzallamas abren caminos opcionales, lo que invita a rejugar cada nivel con los dos personajes (8.9). Se cambian con botones de siguiente y anterior. Todas las de un personaje gastan de **una sola barra de energía compartida**, ya sea por uso o por segundo (las sostenidas). La habilidad seleccionada solo se muestra con un icono en el HUD |

### 1.2 Pilares técnicos

1. **Personaje dividido en dos mitades** (piernas y torso), cada una con su propio Animator, para combinar cualquier movimiento con cualquier dirección de disparo.
2. **Dos máquinas de estados en paralelo** (locomoción y acción) que reflejan esa división.
3. **La lógica manda; el Animator solo muestra.** El estado de gameplay nunca vive dentro del Animator.
4. **Contenido definido por datos** (ScriptableObjects) para los personajes, el disparo, las habilidades, los enemigos y los pickups.
5. **Pixel perfect**: sin subpíxeles, sin blur, sin temblores.

### 1.3 Fuera de alcance (por ahora)

Disparo cargado, melee fuera de la espada de fuego, armas que se recogen, wall jump, cambio de color del personaje por habilidad, cooperativo.

**Planeado para la versión 2** (sección 18): vehículos estilo Metal Slug, más misiones y el comandante como tercer personaje jugable.

### 1.4 Historia

Los protagonistas son **dos soldados espaciales** del mismo escuadrón: Rosslyn y Veyron. En cada misión, su **comandante** les da información sobre un **objetivo a neutralizar** (el jefe de la misión). Al completar las 5 misiones se abre la misión final, donde descubren que **el comandante es un traidor** y se enfrentan a él como jefe final.

| Personaje | Papel |
|---|---|
| Rosslyn | Soldado espacial, protagonista y personaje jugable |
| Veyron | Compañero de escuadrón y segundo personaje jugable |
| Comandante | Da los briefings y habla en los diálogos de cada misión. Es el jefe final. En la versión 2 se vuelve personaje jugable (18.4) |
| 5 objetivos | Jefes de las misiones |

**Los dos soldados van a cada misión.** Dentro del lore, ambos participan en todas las misiones aunque el jugador solo controle a uno. Por eso **los diálogos son los mismos** sin importar el personaje elegido: los dos pueden hablar en ellos y el texto nunca supone cuál se está jugando.

**Excepción: la Misión 0.** Es un entrenamiento de Rosslyn en un **simulador** y solo ella participa. Es lo primero que ve el jugador y sirve como misión de introducción, al estilo de la primera misión de Mega Man X (15.1).

**Cómo se cuenta con el sistema de escenas y diálogos (15.7):** como en Mega Man X, los diálogos van acompañados de **escenas cortas dentro del nivel**: los personajes entran, caminan, hacen gestos y se teletransportan mientras hablan.
- **Inicio de misión:** el comandante describe el objetivo y los soldados responden.
- **Cierre de misión:** el comandante felicita a los soldados o comenta el resultado.
- **Pistas de la traición:** conviene sembrar detalles pequeños en los diálogos de cierre, para que la revelación no salga de la nada. Por ejemplo, algo que dice un objetivo, un dato que no cuadra o una orden extraña del comandante. Como las misiones se juegan en cualquier orden, cada pista debe entenderse por sí sola.
- **Misión final:** la revelación de la traición (ver la decisión pendiente sobre cuándo ocurre, en 17.2).

**Estilo visual:** ciencia ficción. Escenarios como estaciones espaciales, naves, planetas y bases. El disparo básico es un bláster.

---

## 2. Stack técnico

| Área | Elección | Notas |
|---|---|---|
| Versión de Unity | Unity 6 (6000.x LTS) | Usar la misma versión menor en todo el proyecto |
| Render pipeline | URP con Renderer 2D | Plantilla *Universal 2D* de Unity Hub |
| Cámara | Cinemachine 3 + Pixel Perfect Camera (URP) | |
| Input | Input System (paquete nuevo) | Teclado y gamepad; remapeo de controles |
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
    │   │   ├── Portraits/          ← retratos de diálogo (48×48)
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
    │       ├── Cutscenes/          ← CutsceneRunner, pasos de escena, CutsceneActor, DialogueBox
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
- Los clips `Torso_Melee` y `Torso_Melee_Crouch` todavía no tienen celdas: se agregan como filas nuevas al final de la hoja en el Hito 4.
- Si en algún momento la hoja crece, se agregan filas al final (la hoja puede crecer hacia abajo).

**Varios personajes:** cada personaje tiene **su propio spritesheet con la misma cuadrícula y la misma tabla de clips**. Así `PlayerArtSetup` y `PlayerAnimation` funcionan igual para todos: el menú recibe el personaje, corta su hoja y genera sus clips y Animators en `Animations/Characters/<Personaje>/`. Las celdas de un clip que un personaje no usa (por ejemplo, `Torso_Melee` si no tiene espada) se dejan vacías.
- Hoy la hoja de Rosslyn está en `Art/Characters/Player/` y sus clips en `Animations/Player/`. Se mueven a la carpeta del personaje cuando se agregue el segundo (Hito 7).
- Si alguna habilidad de Veyron no sale de un cañón, puede necesitar filas propias en la hoja **[DECIDIR]** junto con su diseño.

### 4.3 Tiles y otros sprites

- Tiles: 16×16 px, exportados como una hoja (*tileset*), *slice* por grid de 16×16 y pivote centrado.
- Obstáculos por habilidad: bloque destructible (1 tile + VFX al romperse) y muro de acero en **3 etapas de calor** (normal, caliente, al rojo vivo) + VFX de metal fundido. Deben distinguirse a simple vista del suelo normal, para que el jugador sepa que se pueden destruir.
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
| `Torso_Melee` | No | Tajo cuerpo a cuerpo (espada de fuego de Rosslyn, cuchillo de Veyron), con Animation Events para la hitbox |
| `Torso_Melee_Crouch` | No | El mismo tajo agachado, con Animation Events para la hitbox |

La granada de goma, el taladro, el lanzallamas y el láser salen del cañón, así que reutilizan las poses de disparo y de apuntado. Solo las armas cuerpo a cuerpo (espada y cuchillo) necesitan clips propios del torso. Los dos clips se llaman igual en las dos hojas (`Torso_Melee`): en la de Rosslyn se dibuja la espada y en la de Veyron el cuchillo. El arma sónica de Veyron puede necesitar un clip propio **[DECIDIR]**.

**Cuerpo completo** (se ocultan las mitades y se muestra el SpriteRenderer `FullBody`): `FullBody_Hurt`, `FullBody_Death`, `FullBody_Teleport_In` (al empezar el nivel y al reaparecer) y `FullBody_Teleport_Out`.

**Gestos para escenas** (cuerpo completo, 15.7): por ejemplo `FullBody_Nod` (asentir), `FullBody_Point` (señalar), `FullBody_Salute` (saludo militar) y `FullBody_ArmsCrossed` (brazos cruzados, en loop) **[DECIDIR]** el set final. Cada personaje dibuja los mismos gestos en su hoja y se agregan al `SpritesheetLayout`. Para caminar en las escenas se usan `Legs_Run` + `Torso_Run`; un `Legs_Walk` es opcional **[AJUSTAR]**.

**Total por personaje:** 7 clips de piernas (`Legs_Land` es opcional), 13 de torso comunes más los clips propios de sus habilidades, y 4 de cuerpo completo. Los dos clips `Torso_Melee` existen en las dos hojas (espada para Rosslyn, cuchillo para Veyron). El arte de cada habilidad está en la sección 8.8.

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
| `PlayerAnimation` | Traducir el estado a parámetros de los Animators, parpadeo de i-frames y reenviar los Animation Events | Lógica de gameplay |

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
}
```

- **Un prefab por personaje**, como *Prefab Variant* de un prefab base `Player`. El variant solo cambia los sprites, los Animators y la posición de los FirePoints. Así un cambio en los componentes se aplica a todos.
- **El jugador no se coloca en la escena.** `MissionController` instancia `GameManager.SelectedCharacter.prefab` en el punto de inicio y llama a `PlayerController.Initialize(characterData)`. Ese método reparte los datos: `movement` al motor, `buster` a `PlayerBuster`, `maxHealth` a `PlayerHealth`, `maxEnergy` a `PlayerEnergy` y `abilities` a `PlayerAbilities`. Ningún componente tiene estos valores en sus propios campos serializados.
- La cámara (`SetTarget`, 13) y el HUD se conectan al jugador después de instanciarlo.
- **Collider:** mismo tamaño para todos por defecto, para que el diseño de niveles no cambie según el personaje **[AJUSTAR]**.
- **Diseño de niveles:** cada misión debe poder completarse con **cualquier** personaje. Si los saltos cambian entre personajes, se fija un mínimo común (por ejemplo, todos superan 3 tiles de alto y un foso de 5 tiles con salto normal) y los niveles se diseñan con ese mínimo **[AJUSTAR]**.
- Para probar en el editor sin pasar por los menús, la escena de prueba tiene un campo con el `CharacterData` que se usa si no hay uno seleccionado.

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
  └──(Cinemática / jefe derrotado)──▶ Disabled ──▶ Alive
```

- En `Hurt`, `Dead` y `Respawning`, ambas FSM se desactivan y se muestra `FullBody`.
- En `Disabled` las FSM también se desactivan, pero el personaje queda en manos de las escenas (15.7): se ven las mitades en `Idle`, los pasos de escena pueden moverlo (con `Run`) o reproducir un clip de cuerpo completo (gestos, teletransporte).
- En `Hurt` se aplica un knockback hacia atrás y se activan los i-frames. Al volver a `Alive`, los i-frames siguen corriendo (el personaje parpadea), pero ya se puede controlar.
- La versión 2 agrega un estado `Piloting` para los vehículos (18.1). Por eso esta FSM se implementa como las demás (`StateMachine` + `IState`) y no con `if` o `bool` sueltos.

### 6.3 FSM de locomoción

| Estado | Entra cuando | Sale a |
|---|---|---|
| `Idle` | En suelo, sin input horizontal | `Run` (input H) · `Crouch` (abajo) · `Airborne` (salto o pierde el suelo) · `Dash` (botón de dash) |
| `Run` | En suelo con input horizontal | `Idle` · `Crouch` · `Airborne` · `Dash` |
| `Crouch` | En suelo con abajo presionado | `Idle` (suelta abajo y hay espacio arriba) · `Airborne` (pierde el suelo) |
| `Dash` | Botón de dash en el suelo | `Airborne` con impulso de dash (salto o pierde el suelo) · `Idle` / `Run` (se acaba el tiempo) |
| `Airborne` | Salto o caída | `Idle` / `Run` al aterrizar · `AirDash` (botón de dash y hay air dash disponible) |
| `AirDash` | Botón de dash en el aire | `Airborne` (se acaba el tiempo) · `Idle` / `Run` (aterriza) |

Propiedades públicas: `IsCrouching`, `IsGrounded`, `IsDashing` (verdadero en `Dash` y `AirDash`), `FacingDir`.

#### Salto

Valores iniciales en unidades (1 unidad = 1 tile = 16 px):

| Parámetro | Valor inicial |
|---|---|
| Velocidad de correr | 6 u/s **[AJUSTAR]** |
| Altura máxima de salto | 3.5 u (≈ 3.5 tiles) **[AJUSTAR]** |
| Tiempo hasta la cima | 0.4 s **[AJUSTAR]** |
| Multiplicador de gravedad al caer | 1.5 **[AJUSTAR]** |
| Coyote time | 0.1 s **[AJUSTAR]** |
| Jump buffer | 0.1 s **[AJUSTAR]** |

- La gravedad y la velocidad inicial del salto se **calculan** a partir de la altura y el tiempo hasta la cima. Así se ajusta pensando en tiles y no en fuerzas.
- Altura variable: si se suelta el botón mientras sube, se reduce la velocidad Y.
- **Control en el aire libre:** se puede cambiar de dirección y velocidad en el aire, como en Mega Man. Se puede usar la misma aceleración que en el suelo o una un poco menor **[AJUSTAR]**.

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

Cada personaje tiene su propio `BusterData`, referenciado desde su `CharacterData` (5.4). Esta sección describe el bláster de Rosslyn.

**Disparo básico de Veyron:** más potente que el bláster, pero con **mucho menos alcance**. Sigue siendo un proyectil, así que no hace falta otro sistema: es otro `BusterData` con más `damage` y un `maxRange` corto (7.1). Valores iniciales **[AJUSTAR]**:

| Valor | Rosslyn | Veyron |
|---|---|---|
| Daño | 1 | 2–3 |
| Alcance (`maxRange`) | Sin límite (hasta salir de la cámara) | ~4–5 u (un cuarto de pantalla) |
| Cadencia, direcciones y shoot pose | Iguales | Iguales |

- Al llegar a `maxRange`, el proyectil se desactiva con un VFX corto de disipación, para que el jugador vea dónde termina el alcance.
- **Diseño de enemigos y jefes:** con poco alcance, Veyron tiene que acercarse. Torretas lejanas, enemigos sobre plataformas altas y jefes que se quedan lejos deben poder alcanzarse sin recibir daño inevitable (14.2).
- Si el sprite del proyectil es distinto al de Rosslyn, solo cambia el prefab referenciado en su `BusterData`.

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

---

## 8. Habilidades

### 8.1 Modelo

- **Cada personaje tiene su propia lista de habilidades** (`CharacterData.abilities`): 3 de Rosslyn y 4 de Veyron (8.2). Todas usan el mismo sistema (`AbilityData` + `AbilityEffect`).
- **El progreso es por personaje:** las habilidades que desbloquea un personaje no las tiene el otro (15.4).
- **Se obtienen como coleccionables** escondidos en los niveles, no al vencer jefes (8.9).
- **Una sola barra de energía compartida** para todas las habilidades del personaje. Su máximo depende del personaje (`CharacterData.maxEnergy`).
- La habilidad seleccionada se cambia con **siguiente y anterior** (en un ciclo que solo incluye las desbloqueadas) o **eligiéndola directamente en el menú de pausa**, como en Mega Man. Con 3 o 4 habilidades basta con el ciclo, pero el menú evita que se vuelva lento si en la V2 cada personaje tiene más.
- El botón de disparo siempre usa el disparo básico. El botón de habilidad usa la seleccionada.
- **El personaje no cambia de color.** La habilidad seleccionada solo se indica con su **icono en el HUD**.
- Hay dos modos de activación:
  - **Por pulsación:** gasta energía una vez por uso.
  - **Sostenida:** funciona mientras se mantiene el botón y gasta energía por segundo.

### 8.2 Lista de habilidades

Energía máxima: **28** **[AJUSTAR]**. Todos los costos y valores son iniciales **[AJUSTAR]**.

**Rosslyn**

| Habilidad | Tipo | Activación | Dirección | Costo | Resumen |
|---|---|---|---|---|---|
| Lanzallamas | Área sostenida | Sostenida | 4 direcciones | 6 / s | Llama de medio alcance que daña de forma continua. **Funde muros de acero** |
| Granada de goma | Proyectil físico | Pulsación | Adelante, en arco | 3 | Rebota hacia adelante; explota al tocar un enemigo o al acabarse la mecha |
| Espada de fuego | Cuerpo a cuerpo | Pulsación | Adelante | 2 | Tajo de corto alcance envuelto en llamas |

**Veyron**

| Habilidad | Tipo | Activación | Dirección | Costo | Resumen |
|---|---|---|---|---|---|
| Taladro | Proyectil perforante | Pulsación | 4 direcciones | 3 | Atraviesa a varios enemigos. **Rompe bloques destructibles** |
| Láser | Rayo sostenido | Sostenida | 4 direcciones | 8 / s | Rayo de largo alcance que daña de forma continua |
| Cuchillo | Cuerpo a cuerpo | Pulsación | Adelante | 1 | Tajo rápido y corto; reutiliza el sistema de la espada |
| Arma sónica | **[DECIDIR]** | **[DECIDIR]** | **[DECIDIR]** | — | Arma nueva, por diseñar |

"4 direcciones" significa las mismas del disparo básico: adelante, diagonal arriba-adelante, arriba y abajo (solo en el aire).

#### Granada de goma
- **Lanzamiento:** velocidad inicial de (6, 6) u/s hacia donde mira el jugador **[AJUSTAR]**.
- **Física:** es la única habilidad con `Rigidbody2D` **dinámico**, con un `PhysicsMaterial2D` de rebote 0.6 y fricción baja **[AJUSTAR]**. Rebota en el suelo y en las paredes.
- **Explota** al tocar una `EnemyHurtbox` o cuando se acaba la mecha (2 s **[AJUSTAR]**).
- **Explosión:** daño de área con `Physics2D.OverlapCircleAll`, radio de 1.5 u y 4 de daño **[AJUSTAR]**. Golpea una sola vez a cada enemigo.
- No daña al jugador.
- **Arco fijo:** siempre sale igual hacia adelante, sin importar la dirección que se presione.
- **Sin límite** de granadas en pantalla. Las limitan la energía y el cooldown (0.3 s **[AJUSTAR]**).
- **Sale del cañón** de Rosslyn: usa la pose de disparo hacia adelante (`Torso_Shoot_Forward`, o `Torso_Shoot_Crouch` agachado) y se lanza desde `FirePoint.Forward` o `FirePoint.Crouch`. No tiene animación de lanzamiento propia.
- Como el arco es fijo, la granada **fuerza la pose hacia adelante** durante su shoot pose, aunque el jugador esté manteniendo arriba. Así el cañón siempre apunta hacia donde sale la granada.

#### Espada de fuego
- **Hitbox** rectangular frente al jugador de 1.5 × 1.5 u **[AJUSTAR]**. Solo está activa en los frames del tajo, marcados con Animation Events (`OnHitboxOn` / `OnHitboxOff`).
- 4 de daño **[AJUSTAR]**. Golpea una sola vez a cada enemigo por tajo.
- Se puede usar en el suelo, corriendo y en el aire, porque la animación es solo del torso.
- **Se puede usar agachado** con su propio clip, `Torso_Melee_Crouch`. La hitbox se coloca más abajo (1.5 × 1 u a la altura del torso agachado **[AJUSTAR]**), lo que sirve contra enemigos pequeños.
- **Destruye proyectiles enemigos:** la hitbox también detecta la capa `EnemyProjectile` y desactiva los proyectiles que toca. Así la espada tiene un uso defensivo. Los proyectiles especiales de los jefes pueden marcarse como `indestructible`.
- Torso: `Torso_Melee` de pie, en el aire o corriendo, y `Torso_Melee_Crouch` agachado. Las llamas del tajo son un **VFX separado** en el `FirePoint.Forward`, porque no caben en el lienzo de 48×64.

#### Lanzallamas
- **Sostenido:** activo mientras el botón esté presionado y haya energía.
- **Alcance:** 4 u **[AJUSTAR]**. Si choca con una pared, la llama se recorta (raycast contra `Ground` y `AbilityObstacle`).
- **Funde muros de acero:** tiene la interacción `Melt` (15.2). Si la llama toca un muro que la acepta, le acumula calor hasta fundirlo.
- Atraviesa a los enemigos.
- **Daño por tick:** 1 de daño cada 0.15 s a cada enemigo dentro de la llama **[AJUSTAR]**.
- **Movimiento:** al 50% de la velocidad y se puede saltar.
- Sigue la dirección de apuntado en tiempo real.
- Torso: mantiene la pose de disparo de la dirección actual.
- VFX: `Flame_Start`, `Flame_Loop` y `Flame_End`.

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
- **Reutiliza todo el sistema de la espada de fuego:** el mismo `MeleeEffect` (8.4), `DamageHitbox` de un golpe por objetivo, Animation Events `OnHitboxOn` / `OnHitboxOff` y los clips `Torso_Melee` / `Torso_Melee_Crouch` de la hoja de Veyron. Solo cambian los valores y el VFX.
- Valores iniciales **[AJUSTAR]**: hitbox de 1 × 1 u (más chica que la espada), 3 de daño, costo 1 y un tajo más rápido (clip más corto).
- Se usa de pie, corriendo, en el aire y agachado, igual que la espada.
- **[DECIDIR]** si destruye proyectiles enemigos como la espada (17.2). Hay que darle algo que lo distinga del disparo básico de Veyron, que también es de corto alcance.

#### Arma sónica
- Arma nueva de Veyron. Tipo, activación, dirección, costo y efecto están por definir **[DECIDIR]** (17.2).
- Si encaja en las piezas de 8.7 (`Projectile`, `DamageHitbox`, `BeamCaster`, `Explosion`), se arma con ellas. Si no, se agrega una pieza nueva.

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
    public AnimationClip customTorsoClip;        // solo con CustomClip
    public AnimationClip customTorsoClipCrouch;  // solo con CustomClip; versión agachada
    public SfxData activateSfx;
    public SfxData loopSfx;                 // Hold: sonido en loop mientras está activa
}
```

| Habilidad | `activation` | `usesAim` | `usableWhileCrouching` | `moveSpeedMultiplier` | `obstacleInteractions` | `torsoPose` |
|---|---|---|---|---|---|---|
| Granada de goma | Press | No | Sí | 1 | — | `ShootPoseForward` |
| Espada de fuego | Press | No | Sí | 1 | — | `CustomClip` (`Torso_Melee`) |
| Lanzallamas | Hold | Sí | Sí | 0.5 | `Melt` | `ShootPose` |
| Taladro | Press | Sí | Sí | 1 | `Break` | `ShootPose` |
| Láser | Hold | Sí | Sí | 0 | — | `ShootPose` |
| Cuchillo | Press | No | Sí | 1 | — | `CustomClip` (`Torso_Melee`) |
| Arma sónica | **[DECIDIR]** | | | | | |

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

- **Por pulsación (granada, taladro):** el efecto sale de un pool, se coloca en el origen, se llama `Begin` y después vive por su cuenta.
- **Por pulsación pegada al jugador (espada, cuchillo):** el efecto es hijo del jugador y se desactiva solo al terminar el tajo.
- **Sostenida (lanzallamas, láser):** hay una sola instancia pegada al FirePoint. `UsingAbility` llama `Sustain` cada frame y `End` al terminar.

Implementaciones: `RubberGrenadeEffect`, `MeleeEffect` (espada de fuego y cuchillo, con distintos valores y VFX), `FlamethrowerEffect`, `DrillEffect`, `LaserEffect` y el del arma sónica.

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
| `DamageHitbox` | Trigger que daña en dos modos: **una vez por objetivo** por activación, o **por tick** cada X segundos por objetivo | Espada, lanzallamas |
| `BeamCaster` | Raycast con longitud variable y daño por tick a todo lo que toca | Láser |
| `Explosion` | `OverlapCircleAll` de un solo golpe + VFX | Granada, enemigos |

Para el daño por tick se lleva un registro por objetivo (`Dictionary<IDamageable, float>` con el próximo momento en que puede recibir daño). Así un enemigo dentro de la llama no recibe daño en cada frame.

### 8.8 Arte necesario

Todo el arte de las habilidades usa PPU 16. En pixel art no se rotan los sprites 45°, porque los píxeles se deforman: las piezas que apuntan en diagonal tienen **su propio sprite diagonal**. La dirección hacia abajo es el sprite vertical volteado.

| Elemento | Sprites | Direcciones |
|---|---|---|
| Iconos del HUD | 7 iconos de 16×16 (3 de Rosslyn, 4 de Veyron) | — |
| Coleccionable de habilidad | Sprite animado + versión holograma (8.9) | — |
| Clips del torso | `Torso_Melee`, `Torso_Melee_Crouch` en cada hoja | — |
| Granada | Giro (4 frames) + explosión | — |
| Espada | Tajo de llamas (VFX) | Adelante |
| Lanzallamas | `Flame_Start`, `Flame_Loop`, `Flame_End` | Horizontal, diagonal, vertical |
| Taladro | Proyectil girando | Horizontal, diagonal, vertical |
| Láser | Origen y impacto (animados) + textura del rayo que se repite | Una sola textura para todas las direcciones |
| Cuchillo | Tajo (VFX corto) | Adelante |
| Arma sónica | **[DECIDIR]** | **[DECIDIR]** |

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

Las habilidades **no las entregan los jefes**. Cada habilidad es un **coleccionable único** (`AbilityPickup`) escondido en alguna misión: 7 en total, 3 de Rosslyn y 4 de Veyron. Dónde va cada uno está por definir **[DECIDIR]** (17.2).

**El ciclo entre personajes:**
- El **taladro (Veyron)** rompe bloques destructibles y el **lanzallamas (Rosslyn)** funde muros de acero (15.2). Cada uno abre caminos opcionales que el otro personaje no puede abrir.
- Detrás de esos caminos hay coleccionables, sobre todo **del otro personaje**: al abrir un muro con Rosslyn se descubre algo para Veyron, y al revés. Para ver todo un nivel hay que **rejugarlo con los dos**.

**Reglas de diseño:**
- **El taladro y el lanzallamas deben estar en lugares accesibles sin ninguna habilidad.** Si quedan detrás de un obstáculo, el ciclo no puede empezar.
- Ningún coleccionable puede quedar detrás de un obstáculo que solo abre una habilidad que a su vez está detrás de él (dependencia circular). Conviene llevar, por misión, una tabla con cada coleccionable y lo que hace falta para llegar a él.
- Como todos los coleccionables son opcionales, ninguna misión ni jefe puede depender de tener alguno (14.2).

**Quién lo puede recoger [DECIDIR]** (17.2). Recomendación: **solo su dueño**. Si se juega con el otro personaje, el coleccionable aparece como un **holograma** con el icono de su dueño y no se puede tomar. Así el jugador sabe que hay algo que volver a buscar con el otro personaje.

**Al recogerlo:**
1. El jugador pasa a `Disabled` un momento (1–1.5 s **[AJUSTAR]**): suena un jingle, aparecen el icono y el nombre de la habilidad.
2. Se desbloquea en `PlayerAbilities`, queda seleccionada y la energía se llena.
3. **Se guarda en ese momento** (15.4). Solo se guarda el progreso, no el estado del nivel. Así no se pierde aunque el jugador abandone la misión o cierre el juego.

**Ya recogido:** el coleccionable no vuelve a aparecer para su dueño. Para el otro personaje sigue viéndose como holograma (o se oculta) **[AJUSTAR]**.

**Componentes y datos:**
- `AbilityPickup` (en la escena): referencia a un `AbilityData`. Al cargar la escena revisa en `GameManager` si el dueño ya lo tiene y con qué personaje se está jugando, y se muestra normal, como holograma u oculto.
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
| Clip de habilidad | `AbilityData.customTorsoClip` o `customTorsoClipCrouch` de la habilidad que se acaba de usar (`torsoPose = CustomClip`) | Torso |
| Pose forzada hacia adelante | `AbilityData.torsoPose = ShootPoseForward` en la habilidad en curso | Torso |

Los hashes de los estados se cachean (`Animator.StringToHash("Torso_Run")`). Nunca se usan strings en runtime.

### 9.2 Lógica del torso

Prioridad, de mayor a menor:

1. Dash → `Torso_Dash`
2. Habilidad con clip propio en curso → `Torso_Melee` o `Torso_Melee_Crouch` (hasta que termina el clip)
3. Shoot pose con pose forzada hacia adelante (granada) → `Torso_Shoot_Crouch` si está agachada, si no `Torso_Shoot_Forward`
4. Shoot pose + agachado → `Torso_Shoot_Crouch`
5. Shoot pose + apuntando `Up` → `Torso_Shoot_Up`
6. Shoot pose + apuntando `DiagUp` → `Torso_Shoot_DiagUp`
7. Shoot pose + apuntando `Down` → `Torso_Shoot_Down`
8. Shoot pose → `Torso_Shoot_Forward`
9. Apuntando `Up` (sin disparar) → `Torso_Aim_Up`
10. Apuntando `DiagUp` (sin disparar) → `Torso_Aim_DiagUp`
11. Estado base según la locomoción: `Torso_Idle`, `Torso_Run`, `Torso_JumpUp` / `Torso_Fall` (según la velocidad Y), `Torso_Crouch`

Mientras una habilidad sostenida (lanzallamas o láser) está activa, el shoot pose se mantiene, así que el torso se queda en la pose de disparo de la dirección actual. El taladro activa un shoot pose normal, igual que el disparo básico.

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
- Los clips en loop tienen *Loop Time* activado. `Torso_Melee`, `Torso_Melee_Crouch`, los `Torso_Shoot_*` y los `FullBody_*` no.
- Para saber si `Torso_Melee` o `Torso_Melee_Crouch` terminaron, se revisa `normalizedTime >= 1` en `PlayerAnimation`.
- No hace falta `AnimatorOverrideController`: los dos clips de habilidad son estados normales.
- El cambio entre poses en loop con la misma cantidad de frames (por ejemplo, `Torso_Run` y `Torso_Aim_DiagUp` corriendo) conserva el `normalizedTime`, como en el código de arriba.
- Las poses de disparo (`Torso_Shoot_*`) no tienen loop: el movimiento del arma se reproduce una vez y se queda en el último frame hasta que termina el *shoot pose*. **Cada disparo reinicia el clip desde el frame 0** (`PlayerAnimation` escucha `PlayerBuster.Fired`), aunque el estado del torso no cambie.

### 9.4 Animation Events

Los clips solo marcan **momentos**: `OnFootstep` y `OnHitboxOn` / `OnHitboxOff` (los frames activos del tajo en `Torso_Melee` y `Torso_Melee_Crouch`). `PlayerAnimation` los recibe y los reenvía como eventos C#. Los estados deciden qué hacer con ellos.

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
}
```

- `knockback` y `stunDuration` son **opcionales**: casi todo el daño los deja en 0. Los valores salen de quien hace el daño (el campo del efecto o del proyectil).
- **Cada receptor decide cómo reacciona.** `EnemyData` tiene `knockbackResistance` (0–1) y `canBeStunned` (14.1). Los jefes, por defecto, ignoran el empuje y el aturdimiento **[AJUSTAR]**. El jugador ya tiene su knockback fijo en `Hurt` (6.2) y lo usa en lugar de estos campos.
- Si en la V2 hace falta otro efecto (congelar, quemar en el tiempo…), se agrega aquí un campo opcional igual. Si llegan a ser muchos, se migra a una lista de efectos de estado.

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
- El objetivo de la cámara se asigna por código (`SetTarget`), no se deja fijo al jugador en el editor. Así se puede cambiar a un vehículo en la versión 2 (18.3).
- *Look ahead* ligero en la dirección en la que mira el jugador, más notorio durante el dash **[AJUSTAR]**.
- `CinemachineConfiner2D` por sección del nivel.
- **Puertas de jefe:** al entrar a la sala del jefe, la cámara se fija a la sala (20 tiles de ancho = una pantalla).
- Pixel Perfect Camera en la cámara principal, con la extensión `CinemachinePixelPerfect` en la Cinemachine Camera.

---

## 14. Enemigos y jefes

### 14.1 Enemigos

Usan el mismo núcleo de `StateMachine` con una sola FSM: `Idle/Patrol` → `Alert` → `Attack` → `Dead` (con un flash de daño al recibir golpes, sin estado propio). El estado `Stunned` se activa desde cualquiera de los anteriores cuando el daño trae `stunDuration` y el enemigo puede aturdirse (10.1); al terminar vuelve a `Alert`.

`EnemyData`: vida, daño por contacto, velocidad, rango de detección, tabla de pickups, `knockbackResistance` y `canBeStunned`. Un sprite completo por enemigo, sin dividir, con pivote en los pies y el mismo PPU de 16.

**Primeros enemigos:**
1. Torreta fija que dispara en línea recta.
2. Caminante que patrulla entre dos puntos.
3. Volador que persigue al jugador.

### 14.2 Jefes

- FSM con **fases** según su vida (por ejemplo, cambia de patrón por debajo del 50%).
- **5 jefes**, uno por misión. **Ya no entregan habilidades**: se encuentran en el escenario (8.9).
- **Sin debilidades:** todos los jefes reciben el mismo daño de cada fuente. El reto está en sus patrones, no en elegir la habilidad correcta.
- **Las misiones se eligen en cualquier orden** y las habilidades son opcionales, así que no se sabe cuáles tiene el jugador al llegar a un jefe. Todos los jefes deben poder vencerse **solo con el disparo básico de cualquier personaje**; las habilidades ayudan, pero nunca son obligatorias. Por el alcance corto de Veyron (sección 7), cada jefe debe tener momentos en los que se pueda llegar cerca de él sin recibir daño inevitable.
- **Escena del jefe** (`MissionData.bossCutscene`): al cruzar la puerta de la sala, la cámara se fija y empieza una escena con el mismo sistema de 15.7:
  1. El jefe aparece y hay un **diálogo corto** entre él y **los dos soldados**, que están en la sala (y, si hace falta, el comandante por radio). Como en el resto de las escenas, el que no se juega aparece como no jugable (15.7).
  2. **Fundido a negro:** mientras la pantalla está en negro, desaparece el soldado que no se juega y el jugador queda en su posición de pelea. La pantalla vuelve.
  3. El jefe hace su pose de presentación (`Boss_Intro`) mientras se llena su barra de vida, como en Mega Man. Empieza la pelea.
- **Al reintentar:** si el jugador muere en la pelea y vuelve desde el checkpoint, la escena se repite **sin el diálogo y sin el otro soldado**: solo aparición, pose y barra. El diálogo completo se ve una vez por intento de misión. Los pasos de diálogo se marcan como `firstTimeOnly` (15.7).
- Toda la escena se puede saltar. Al saltarla se llega directo al inicio de la pelea, con la barra llena.
- Cada jefe tiene su `SpeakerData` (nombre y retrato de 48×48) y un clip `Boss_Intro`.
- **Música:** durante el diálogo suena una pista de tensión o silencio **[AJUSTAR]**; el tema del jefe (`bossMusic`) empieza cuando se llena la barra.
- Al vencerlo: el jugador pasa a `Disabled`, se marca la misión como completada, sigue la escena de cierre y se guarda la partida.

---

## 15. Juego, misiones y guardado

### 15.1 Progresión: misiones de selección libre

En la historia, los protagonistas son **dos soldados espaciales**, Rosslyn y Veyron, y cada nivel es una **misión** que les asigna su comandante: neutralizar a un objetivo (ver 1.4). Las 5 misiones se eligen **en cualquier orden** desde una pantalla de selección, como en Mega Man. Al completar las 5 se desbloquea la misión final.

```
Boot → Menú principal → Partida nueva → Misión 0 (simulador, Rosslyn) → Selección de misiones
                     └── Continuar / Cargar → Selección de misiones ⇄ Selección de personaje → Misión → Selección de misiones
                                    └── (5 misiones completadas) → Misión final (el comandante) → Créditos

Misión: Escena de inicio → Nivel (coleccionables opcionales) → Escena del jefe (diálogo + pose) → Jefe → Escena de cierre
```

#### Misión 0: el simulador

Es lo **primero que ve el jugador** al empezar una partida nueva. En la historia, Rosslyn entrena en un **simulador** antes de recibir misiones reales. En el juego es una **misión de introducción al estilo de la primera de Mega Man X**: un nivel corto y real, **sin tutorial explícito**, que enseña con el diseño del nivel.

- **Arranque:** al crear una partida nueva se carga directo la Misión 0, sin pasar por la selección de misiones ni la de personaje.
- **Personaje fijo:** siempre se juega con **Rosslyn** (`MissionData.forcedCharacter`).
- **Sin carteles ni textos de controles.** Cada mecánica aparece primero en una situación segura y luego en una con riesgo. Por ejemplo, un escalón de 3 tiles antes del primer foso, un enemigo sobre el jugador para invitar a disparar hacia arriba, o un pasillo largo con un foso ancho al final que pide un dash jump. Orden sugerido: correr y disparar, saltar, disparar hacia arriba y en diagonal, agacharse, dash y dash jump **[AJUSTAR]**.
- **Corta y con ritmo:** unos pocos minutos, para que el jugador llegue rápido a la acción.
- Los controles se pueden consultar en el menú de pausa y en las opciones, nunca en pantalla durante el juego.
- **Estilo visual:** escenario holográfico de simulador, con enemigos holográficos (los mismos enemigos con otra paleta o material). Puede reutilizar la estética de la cuadrícula de prueba del Hito 1 **[AJUSTAR]**.
- **Habilidades [DECIDIR]:** como en Mega Man X, lo natural es jugarla solo con el disparo básico. La alternativa es prestar una habilidad temporal que no se guarda como desbloqueada.
- **Sin coleccionables** (`abilityPickups` vacío).
- **Final [DECIDIR]:** un jefe holográfico al final. Se puede imitar a Mega Man X, donde la pelea contra Vile no se puede ganar y sirve para presentar a la historia: por ejemplo, el simulador se descontrola o el comandante la detiene, y así se siembra la primera pista de su traición (1.4).
- **Escenas:** el comandante guía el entrenamiento en la escena de inicio y en la de cierre. Es la única misión en la que solo participa Rosslyn. El diálogo de cierre puede presentar a Veyron y dar paso a las misiones reales.
- **Al completarla:** se guarda como misión completada y se abre la selección de misiones. **No cuenta** entre las misiones necesarias para la misión final.
- **Si se cierra el juego antes de completarla:** al continuar ese slot se vuelve a cargar la Misión 0, porque la selección de misiones aún no está disponible.
- **Repetirla [DECIDIR]:** si aparece en la selección de misiones como "Simulador" para volver a practicar. Si se repite, sigue siendo con Rosslyn.
- **Saltarla [DECIDIR]:** si se puede saltar al empezar una partida nueva (útil al jugar por segunda vez).


- **Pantalla de selección de misiones:** muestra las 5 misiones con su nombre, imagen y un **briefing** corto (el objetivo de la misión dentro de la historia). Las completadas se marcan como resueltas. Cada misión muestra los **iconos de sus coleccionables**: los encontrados a color y los que faltan como silueta, con el personaje dueño de cada uno (8.9).
- **Dificultad:** como no hay un orden fijo, las 5 misiones deben tener una dificultad parecida. Opcionalmente, el briefing puede mostrar un nivel de amenaza para orientar al jugador **[DECIDIR]**.
- **Misión final:** se desbloquea al completar las 5 misiones. Su jefe es **el comandante**, después de revelarse como traidor. **[DECIDIR]** si es un solo nivel o varios (como la fortaleza de Wily en Mega Man).
  - Como las habilidades son coleccionables opcionales, **tampoco aquí se garantiza tener alguna**. Los obstáculos de 15.2 no cierran su camino principal y el comandante debe poder vencerse con el disparo básico de cualquier personaje.
  - **[DECIDIR]** cuándo ocurre la revelación (ver 17.2).
- **Repetir misiones:** las misiones completadas **se pueden volver a jugar** desde la pantalla de selección, con cualquier personaje, y siguen marcadas como completadas. Es la base del ciclo de 8.9: volver con el otro personaje, o con el taladro o el lanzallamas recién encontrados, para abrir los caminos de 15.2 y buscar los coleccionables que faltan.
- Desde el menú principal se puede **continuar** (carga el último slot usado y va a la selección de misiones), **cargar** otro slot o empezar una **partida nueva** en un slot (ver 15.4).
- `GameManager` persistente (escena `Boot` cargada una vez) con el estado de la partida.
- Carga de escenas con `SceneManager.LoadSceneAsync` y una pantalla de transición.
- Menú de pausa con la lista de habilidades desbloqueadas del personaje actual (se puede **seleccionar** una desde ahí, 8.1) y la opción de **abandonar la misión** (vuelve a la selección sin completarla).

#### Pantalla de selección de personaje

Aparece **cada vez** que se elige una misión, antes de cargarla. Incluye las misiones repetidas y la misión final. La única excepción es la Misión 0, que tiene personaje fijo (`forcedCharacter`).

- Muestra los personajes disponibles del `CharacterCatalog`: retrato, nombre, una descripción corta, sus estadísticas resumidas en barras (vida, velocidad, salto, energía) **[AJUSTAR]**, su disparo básico y los iconos de sus habilidades desbloqueadas.
- El cursor empieza en el **último personaje usado** en ese slot (`lastCharacter`, 15.4).
- *Cancelar* regresa a la selección de misiones.
- Al confirmar, `GameManager.SelectedCharacter` guarda la elección y se carga la escena de la misión, donde `MissionController` instancia al personaje (5.4).
- **No se cambia de personaje dentro de una misión**, ni al morir. Para cambiar hay que abandonar la misión desde la pausa.
- Si solo hay un personaje disponible, la pantalla se salta. En la versión 1 no pasa (los 2 están disponibles desde el inicio), pero la lógica queda lista.
- **Personajes bloqueados** (versión 2): **[DECIDIR]** si el comandante aparece como silueta con "???" o no aparece hasta desbloquearse.

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
}
```

Los dos personajes de la versión 1 tienen `requiredMissions` vacío. En la versión 2, el comandante lleva la misión final (18.4). Los personajes disponibles **no se guardan**: se calculan a partir del catálogo y de las misiones completadas del slot.

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
    public CharacterData forcedCharacter; // si no es null, se salta la selección de personaje (Misión 0)
}
```

Una lista de `MissionData` en un asset `MissionCatalog` define qué misiones aparecen en la pantalla de selección, cuál es la final, qué misiones se requieren para desbloquearla y cuál es la **misión inicial** (`firstMission`, la Misión 0) que se carga al crear una partida nueva. La Misión 0 no está en la lista de misiones requeridas para la final. La pantalla de selección se arma a partir de ese catálogo, y `abilityPickups` puede estar vacío. Esto deja listo el sistema para agregar misiones en la versión 2 (ver 18.2).

### 15.2 Niveles

- **Checkpoints:** al morir se reaparece en el último checkpoint activado.
- **Peligros:** pinchos y fosos (muerte instantánea), plataformas móviles y plataformas que caen.
- Tilemaps de Unity con grid de 1 unidad (16 px), *Tilemap Collider 2D* + *Composite Collider 2D* para el suelo.

#### Obstáculos que se destruyen con habilidades

Hay dos tipos de obstáculo. Cada uno pide una **interacción**, y cada habilidad declara qué interacciones tiene:

| Obstáculo | Interacción que pide | Habilidad que la tiene | Cómo se destruye |
|---|---|---|---|
| Bloque destructible | `Break` | Taladro (Veyron) | Al instante: un taladro rompe un bloque y se detiene |
| Muro de acero | `Melt` | Lanzallamas (Rosslyn) | Con calor acumulado: hay que mantener la llama sobre el bloque hasta que se funde |

**Interacciones como datos:**
- Cada interacción es un asset `ObstacleInteraction` (ScriptableObject vacío que solo sirve de etiqueta): `Break`, `Melt`, etc.
- `AbilityData.obstacleInteractions` lista las que tiene cada habilidad (8.3). `AbilityObstacleTilemap` dice cuál pide y en qué modo se destruye (instantáneo o por calor).
- Cuando un efecto toca un obstáculo, llama a `AbilityObstacleTilemap.TryInteract(celda, habilidad, dt)`. El obstáculo revisa si la habilidad tiene la interacción que pide y reacciona; si no la tiene, solo detiene el efecto.
- **Todos los obstáculos van en una sola capa física, `AbilityObstacle`.** Agregar un tipo nuevo (hielo, escudo de energía…) o hacer que otra habilidad rompa bloques es crear o asignar un asset, sin capas nuevas ni código en las habilidades. Solo un **modo** de destrucción nuevo (además de instantáneo y por calor) necesita código en `AbilityObstacleTilemap`.

**Reglas comunes:**
- Cada tipo va en su **propio Tilemap** (todos en la capa `AbilityObstacle`), con su propio collider. Para todo lo demás son paredes sólidas.
- **Cada personaje abre un tipo:** Veyron rompe bloques con el taladro y Rosslyn funde acero con el lanzallamas. Lo que hay detrás suelen ser coleccionables del otro personaje (8.9).
- Las interacciones van **por habilidad, no por elemento**: la espada de fuego **no** funde el acero aunque sea de fuego, y el láser no rompe ni funde nada.
- Como el jugador puede llegar a cualquier misión sin la habilidad necesaria, estos obstáculos **nunca** deben cerrar el camino principal, tampoco en la misión final. Solo sirven para caminos secretos, atajos, pickups y coleccionables de habilidad. Al repetir misiones con el otro personaje o con nuevas habilidades, el jugador puede volver por ellos.
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

### 15.3 Opciones de PC

- Pantalla completa, ventana y ventana sin bordes.
- Escala de ventana en múltiplos enteros (×2, ×3, ×4…) para el modo ventana.
- VSync activado y límite de FPS opcional.
- Volumen general, de música y de SFX.
- Remapeo de controles (ver sección 12).

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
  "lastCharacter": "rosslyn",
  "characters": [
    { "id": "rosslyn", "unlockedAbilities": ["flamethrower"], "selectedAbility": "flamethrower" },
    { "id": "veyron", "unlockedAbilities": ["drill", "knife"], "selectedAbility": "drill" }
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
  "lastUsedSlot": 1
}
```

`lastUsedSlot` permite que **Continuar** en el menú principal cargue directo la última partida.

#### Cuándo se guarda

**Autoguardado**, sin botón de guardar:

| Momento | Qué se guarda |
|---|---|
| Al recoger un coleccionable de habilidad | Habilidad desbloqueada del personaje y tiempo de juego (no el estado del nivel) |
| Al completar una misión (después del diálogo de cierre) | Misión completada, último personaje usado, tiempo de juego, estadísticas |
| Al cambiar una opción | `settings.json` |
| Al salir del juego desde el menú | Tiempo de juego |

- **No se guarda el estado de la misión.** La única escritura a mitad de misión es la de un coleccionable recogido, que solo guarda el progreso. Los checkpoints solo existen mientras se juega la misión. Si el jugador cierra el juego dentro de una misión, al volver aparece en la selección de misiones (o en la Misión 0, si todavía no la completa). Esto coincide con el estilo de Mega Man y evita guardar el estado del nivel (enemigos, obstáculos, posición).
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
- La versión 2 del juego (vehículos, más misiones) probablemente necesite su primera migración. Por eso este mecanismo existe desde el inicio.

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

Los sistemas del juego (habilidades, selección de misiones) **leen y modifican el estado a través de `GameManager`**, nunca leen archivos directamente.

#### Herramientas de depuración (solo en el editor)

Menú `Game/Save` en el editor de Unity:
- **Abrir carpeta de guardado** (`EditorUtility.RevealInFinder`).
- **Borrar todos los guardados.**
- **Crear guardado de prueba:** todas las misiones completadas y todas las habilidades de todos los personajes desbloqueadas.
- **Cargar un JSON específico**, para reproducir problemas reportados.

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
| Selección de misiones | Tema de selección |
| Misión (diálogo de inicio y nivel) | Tema de la misión (`MissionData.music`) |
| Sala del jefe | Diálogo con el jefe: tensión o silencio **[AJUSTAR]**. Al llenarse su barra: tema de jefe (`MissionData.bossMusic`) |
| Jefe vencido | *Jingle* de victoria (sin loop), luego silencio durante el diálogo de cierre |
| Misión final | Tema propio + tema del comandante |
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
| Habilidades | Lanzar granada, rebote, explosión, tajo de espada, tajo de cuchillo, proyectil destruido por la espada, lanzallamas (loop), taladro, láser (carga y loop), arma sónica **[DECIDIR]**, recoger coleccionable de habilidad (jingle), sin energía (error), cambio de habilidad |
| Obstáculos | Bloque roto, acero calentándose (loop), acero fundido |
| Enemigos | Disparo, golpe recibido, muerte (2–3 variantes genéricas al inicio) |
| Pickups | Vida, energía |
| UI | Mover cursor, confirmar, cancelar, pausa, letra del diálogo, misión desbloqueada |

| Música | Pistas |
|---|---|
| Menús | Tema principal, selección de misiones, créditos |
| Misiones | 5 temas de misión + misión final |
| Jefes | Tema de jefe + tema del comandante **[DECIDIR]** si cada jefe tiene su propio tema |
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
| [ ] | Arma sónica | — | — | **[DECIDIR]**. Hito 7 |

**Hito 5: enemigos, pickups y obstáculos**

| ✓ | Sonido | `SfxData` | Variantes | Notas |
|---|---|---|---|---|
| [ ] | Disparo enemigo | `Sfx_EnemyShoot` | 2 | Distinto al del jugador para reconocerlo |
| [ ] | Golpe a enemigo | `Sfx_EnemyHit` | 2–3 | Muy corto; suena con cada bala que acierta |
| [ ] | Muerte de enemigo | `Sfx_EnemyDeath` | 2–3 | Genéricas para todos los enemigos al inicio |
| [ ] | Pickup de vida | `Sfx_PickupHealth` | 1 | |
| [ ] | Coleccionable de habilidad | `Sfx_AbilityPickup` | 1 | Jingle corto al recogerlo (8.9) |
| [ ] | Pickup de energía | `Sfx_PickupEnergy` | 1 | |
| [ ] | Bloque roto | `Sfx_BlockBreak` | 1–2 | Taladro contra un bloque destructible |
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
- [ ] Selección de misiones
- [ ] Temas de las 5 misiones (uno por misión)
- [ ] Misión final
- [ ] Tema de jefe (compartido; ver decisión pendiente 6)
- [ ] Tema del comandante (jefe final)
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
- Las barras (vida, energía, jefe) son un mismo componente reutilizable, `SegmentedBar`, que recibe valor actual y máximo y dibuja segmentos. En la versión 2 se reutiliza para la vida de los vehículos.
- **Icono de la habilidad seleccionada** junto a la barra de energía. Al cambiar de habilidad, el icono se actualiza y aparece el nombre durante 1–2 segundos.
- Barra de vida del jefe.
- **Canvas pixel perfect:**
  - Canvas en *Screen Space - Overlay* con *Canvas Scaler* en modo *Constant Pixel Size*.
  - El layout se diseña a 320×180, con sprites de PPU 16 y `Filter Mode = Point`.
  - Un script asigna `scaleFactor` = el factor de escala entero de la Pixel Perfect Camera (×4, ×6, ×8…). Así cada píxel del HUD mide lo mismo que un píxel del juego.
  - Las posiciones de los elementos van en píxeles enteros.
  - Fuente bitmap (pixel font) con `Filter Mode = Point`.

### 15.7 Escenas y diálogos

Como en Mega Man X, los diálogos no son pantallas fijas: **ocurren dentro del nivel**, con **escenas cortas** en las que los personajes entran, caminan, se voltean, hacen gestos y se teletransportan mientras aparece el cuadro de texto. Son escenas sencillas hechas con los sprites del juego, sin cámaras complejas ni arte especial.

**Dónde se usan:**
- Inicio y cierre de cada misión (`MissionData.introCutscene` / `outroCutscene`).
- Al entrar a la sala de cada jefe (`MissionData.bossCutscene`): diálogo con el jefe, su pose y el llenado de su barra (14.2).
- Momentos especiales activados por una zona del nivel (`CutsceneTrigger`): el final de la Misión 0 y la revelación del comandante en la misión final (17.2).

#### Datos

Una escena es una **lista de pasos** que se ejecutan en orden. Las líneas de diálogo son un tipo de paso más.

```csharp
[CreateAssetMenu(menuName = "Game/Speaker")]
public class SpeakerData : ScriptableObject
{
    public string displayName;      // "Rosslyn", "Comandante", etc.
    public Sprite portrait;
}

[CreateAssetMenu(menuName = "Game/Cutscene")]
public class CutsceneData : ScriptableObject
{
    [SerializeReference] public List<CutsceneStep> steps = new();
}

[System.Serializable]
public abstract class CutsceneStep
{
    public bool waitToFinish = true;     // false = el siguiente paso empieza al mismo tiempo
    public abstract IEnumerator Run(CutsceneContext ctx);
    public abstract void Skip(CutsceneContext ctx);   // deja el estado final al instante
}
```

| Paso | Datos | Hace |
|---|---|---|
| `LineStep` | `SpeakerData`, texto | Muestra el cuadro de diálogo y espera a que el jugador avance |
| `MoveStep` | Actor, marcador, velocidad | Camina (o corre) hasta el marcador y se voltea en la dirección del movimiento |
| `FaceStep` | Actor, izquierda / derecha / hacia otro actor | Lo voltea |
| `PlayStep` | Actor, clip (gesto) | Reproduce un clip de cuerpo completo; espera a que termine si no es loop |
| `TeleportStep` | Actor, entrar o salir, marcador | `FullBody_Teleport_In` / `Out` en el marcador |
| `WaitStep` | Segundos | Pausa |
| `FadeStep` | A negro o desde negro, duración | Fundido de pantalla completa; mientras está en negro se pueden quitar o colocar actores sin que se vea |
| `CameraStep` | Marcador o actor, duración, sacudida opcional | Lleva la cámara a un punto (`SetTarget`, 13) o la regresa al jugador |
| `AudioStep` | `SfxData` o `MusicTrack` | Sonido, cambio de música o silencio |
| `BossBarStep` | Jefe | Llena la barra de vida del jefe (escena del jefe) |

- `firstTimeOnly`: campo de todos los pasos. Si es verdadero, el paso se omite cuando la escena se repite en el mismo intento de misión (por ejemplo, el diálogo con el jefe después de morir).
- `waitToFinish = false` permite pasos en paralelo: dos personajes caminando a la vez o un gesto mientras se escribe una línea.
- Los pasos usan **ids de texto** para actores y marcadores, porque un ScriptableObject no puede apuntar a objetos de una escena. `CutsceneRunner` los busca al empezar.
- Para agregar pasos en el inspector (`[SerializeReference]`) hace falta un editor pequeño con un menú para elegir el tipo de paso.
- Todo el texto vive en assets, no en código. Si algún día se traduce el juego, se puede migrar al paquete *Localization* de Unity.

> **Por qué no Timeline:** estas escenas son secuencias cortas que esperan a que el jugador avance el texto, con pocos actores y animaciones elegidas por código (`Animator.Play`, 9.2). Una lista de pasos es más fácil de escribir, de saltar y de depurar. Si en la V2 hace falta una escena más elaborada, Timeline se puede usar para esa escena sin cambiar el resto.

#### Actores y marcadores

- `CutsceneActor`: componente con un **id** (`rosslyn`, `veyron`, `commander`, el id de un jefe…). Puede estar en la escena (el comandante, un jefe) o crearse al empezar.
- `CutsceneMarker`: un punto con id, colocado en la escena, al que se mueven los actores o la cámara.
- **Los soldados se escriben por nombre, no como "el jugador".** Así la regla de 1.4 (los mismos diálogos para los dos) también aplica a las escenas:
  - El actor del personaje elegido es **el jugador**.
  - El otro soldado, si un paso lo usa, se **crea como personaje no jugable** con su prefab en estado `Disabled` (5.4, 6.2).
  - **Al terminar la escena**, si el soldado que no se juega sigue en pantalla, hay un **fundido a negro** (`FadeStep`): mientras la pantalla está en negro desaparece y el jugador queda en su posición. La pantalla vuelve y el jugador toma el control. Es igual en todas las escenas (inicio, cierre, jefe y momentos especiales).
  - En la Misión 0 solo existe Rosslyn: los pasos de Veyron se omiten.
  - **En la escena del jefe** el fundido ocurre después del diálogo y antes de la pose del jefe (14.2).
- **El comandante** aparece como un actor en la escena; cómo se presenta (holograma o en persona) está por definir **[DECIDIR]** (17.2).
- Los movimientos se redondean a la rejilla de píxeles (2.2), igual que todo lo animado por código.

#### Presentación del cuadro de diálogo

```
┌──────────────────────────────────────────────┐
│ ┌──────┐  COMANDANTE                         │
│ │      │  Rosslyn, detectamos al objetivo en  │
│ │ cara │  la estación Orión. Neutralízalo…    │
│ └──────┘                                  ▼  │
└──────────────────────────────────────────────┘
```

- Cuadro en la parte inferior de la pantalla: 320 × 56 px **[AJUSTAR]** en la resolución de referencia. Se acomoda arriba si un actor que habla queda tapado **[AJUSTAR]**.
- Retrato de **48×48 px** a la izquierda **[AJUSTAR]**, dibujado en Piskel con PPU 16.
- Nombre arriba del texto y hasta **3 líneas** de texto por cuadro.
- Indicador ▼ parpadeando cuando la línea terminó y se puede avanzar.
- Usa el mismo Canvas pixel perfect y la misma fuente pixel del HUD (15.6).

#### Comportamiento

- **Efecto máquina de escribir:** el texto aparece letra por letra (40 caracteres por segundo **[AJUSTAR]**), con un sonido corto cada pocas letras.
- **Avanzar** (`Jump`, `Fire` o `Enter`): si la línea aún se está escribiendo, se muestra completa; si ya terminó, pasa a la siguiente. Entre líneas, los pasos de movimiento y gestos corren solos.
- **Saltar escena** (`Pause` / `Esc`): termina la escena completa. `CutsceneRunner` llama a `Skip` en cada paso pendiente, en orden, para que actores, cámara y música queden en su estado final (por ejemplo, el jugador en su posición de inicio y el otro soldado ya fuera). Útil al repetir misiones.
- Mientras hay escena, el jugador está en el estado `Disabled`, el HUD se oculta (salvo la barra del jefe en su entrada), el menú de pausa no se abre y los enemigos están en pausa.
- Se activa el Action Map `Dialogue` y se desactiva `Gameplay`, para que los botones de avanzar no disparen ni salten.

#### Componentes

| Componente | Hace |
|---|---|
| `CutsceneRunner` | Recibe un `CutsceneData`, resuelve actores y marcadores, ejecuta los pasos (en orden o en paralelo), maneja el salto y lanza el evento `Finished` |
| `CutsceneStep` y sus tipos | Cada tipo de paso de la tabla de arriba |
| `CutsceneActor` / `CutsceneMarker` | Ids de actores y puntos en la escena |
| `CutsceneTrigger` | Zona del nivel que inicia una escena una sola vez |
| `DialogueBox` | La UI: retrato, nombre, texto con efecto máquina de escribir e indicador ▼ |

`MissionController` (en cada escena de misión) llama a `CutsceneRunner.Play(introCutscene)` al empezar y espera `Finished` antes de dar el control al jugador. Al vencer al jefe hace lo mismo con `outroCutscene` antes de volver a la selección de misiones.

**Validación** (`Game/Validate`): cada id de actor y de marcador que usa una escena existe en la escena de su misión.

#### Fuera de alcance

Opciones de respuesta, varias expresiones por retrato, retratos en ambos lados del cuadro y voces.

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

- [ ] `PlayerEnergy`, `PlayerAbilities`, `AbilityData`, `AbilityEffect`, estado `UsingAbility` y cambio de habilidad
- [ ] `AbilityEffect` con `IsFinished` y `ButtonReleased`; bases `TimedAbilityEffect` y `HeldAbilityEffect` (8.4, 8.5)
- [ ] Valores propios de cada habilidad en su efecto, no en `AbilityData`; `torsoPose` en lugar de `forceForwardPose` (8.3)
- [ ] `DamageInfo` con `knockback` y `stunDuration` (10.1)
- [ ] Asset `ObstacleInteraction` (`Break`, `Melt`) y `AbilityData.obstacleInteractions`; capa `AbilityObstacle` en lugar de `Breakable` y `Meltable` (15.2). Esas dos capas ya existen en `TagManager` y en `GameSetup.cs` (matriz de colisiones y máscaras `groundMask`, `ceilingMask` y `solidMask` del motor y del proyectil): hay que reemplazarlas ahí también
- [ ] HUD de vida, energía e icono de la habilidad
- [ ] **Taladro:** extiende `Projectile` con `pierceCount`
- [ ] **Granada de goma:** física con rebote + `Explosion`
- [ ] **Espada de fuego:** `MeleeEffect` con `DamageHitbox` de un golpe + Animation Events + `Torso_Melee` y `Torso_Melee_Crouch`
- [ ] **Lanzallamas:** modo sostenido, consumo por segundo y `DamageHitbox` por tick
- [ ] **Láser:** `BeamCaster` + `LineRenderer` pixel perfect (validar la diagonal)
- [ ] Modo de prueba (solo en el editor) que desbloquea todas las habilidades y llena la energía
- [ ] `SaveSystem` base: `SaveData`, escritura segura con respaldo, versión, menú `Game/Save` del editor y guardado de habilidades desbloqueadas por personaje
- [ ] `CharacterData` (5.4): mover vida, energía, `PlayerMovementData`, `BusterData` y habilidades al asset del personaje; `PlayerController.Initialize` y spawn del jugador desde datos

### Hito 5 — Enemigos y nivel de prueba
- [ ] Torreta, caminante y volador, con estado `Stunned` y reacción al empuje (14.1)
- [ ] Pickups de vida y energía con drops
- [ ] Peligros, checkpoints, muerte y respawn
- [ ] `AbilityObstacleTilemap` en la capa `AbilityObstacle`, con interacciones `ObstacleInteraction` (`Break`, `Melt`): bloques destructibles (taladro) y muros de acero con calor acumulado (lanzallamas)
- [ ] `AbilityPickup`: coleccionable de habilidad, secuencia al recogerlo, guardado inmediato y holograma para el otro personaje (8.9)
- [ ] Cámara con confiner por sección

### Hito 6 — Primera misión completa
- [ ] Misión 0 (simulador): misión de introducción con Rosslyn, sin tutorial explícito, arranque directo desde la partida nueva, `forcedCharacter` y `MissionCatalog.firstMission` (15.1)
- [ ] Jefe con fases
- [ ] `GameManager`, flujo de escenas, menú de pausa con selección de habilidad
- [ ] `MissionData`, `MissionCatalog` y pantalla de selección de misiones con briefing y coleccionables encontrados
- [ ] Validación del editor `Game/Validate` para los coleccionables (8.9)
- [ ] `CharacterCatalog` y pantalla de selección de personaje (15.1)
- [ ] Guardado de misiones completadas, `settings.json` y autoguardado al completar una misión
- [ ] Pantalla de slots: continuar, cargar, partida nueva y borrar (con confirmación)
- [ ] Opciones de PC: pantalla, audio y remapeo de controles
- [ ] `MusicPlayer` con intro + loop y crossfade, música de misión y de jefe
- [ ] Snapshot de pausa y volúmenes en opciones
- [ ] Sistema de escenas y diálogos (15.7): `CutsceneData` con pasos, `CutsceneRunner` con salto, `CutsceneActor`, `CutsceneMarker`, `CutsceneTrigger`, `DialogueBox` y editor de pasos
- [ ] Escenas de inicio y cierre de la misión, y escena del jefe: diálogo, `Boss_Intro` y barra de vida; versión corta al reintentar (`firstTimeOnly`)
- [ ] Gestos de cuerpo completo de Rosslyn en su hoja (4.4)

### Hito 7 — Segundo personaje (Veyron)

Depende de las decisiones pendientes 8, 10 y 11 (17.2).

- [ ] Diseño: estadísticas y arma sónica
- [ ] Disparo básico corto y potente: `BusterData` propio con `maxRange` y VFX de disipación (sección 7)
- [ ] Spritesheet con la misma cuadrícula que Rosslyn (4.2.1) y `PlayerArtSetup` por personaje; mover el arte y los clips de Rosslyn a `Characters/<Personaje>/`
- [ ] Prefab variant, `CharacterData`, `PlayerMovementData` y `BusterData` propios
- [ ] Cuchillo (reutiliza `MeleeEffect`) y arma sónica, con su arte; el taladro y el láser ya existen desde el Hito 4
- [ ] Colocar los coleccionables de los dos personajes en la primera misión y validar el ciclo de 8.9 (sin dependencias circulares)
- [ ] Retrato de diálogo (`SpeakerData`), gestos de cuerpo completo (4.4) e imagen de la pantalla de selección
- [ ] Probar las escenas con cada personaje: el elegido es el jugador y el otro aparece como no jugable (15.7)
- [ ] Recorrer el nivel de prueba y la primera misión con los dos personajes (mínimos de salto de 5.4)

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
| Disparo | Normal, un tiro por pulsación, sin carga |
| Direcciones de disparo | Adelante, diagonal arriba-adelante, arriba y abajo (solo en el aire) |
| Pose del torso | Cambia al mantener arriba o arriba-adelante (`Torso_Aim_*`); las 4 direcciones tienen además su clip de disparo (`Torso_Shoot_*`) |
| Dash | Suelo + aire (uno por salto), dash jump con impulso (también al salir de una orilla durante el dash), sin i-frames |
| Disparar durante el dash | No (tampoco usar habilidades) |
| Dash desde agachado | No |
| Collider durante el dash | Sin cambios |
| Habilidades | Propias de cada personaje, con progreso por personaje; barra de energía compartida, siguiente/anterior |
| Habilidades de Rosslyn | Lanzallamas, granada de goma y espada de fuego |
| Habilidades de Veyron | Taladro, láser, cuchillo y arma sónica |
| Obtención de habilidades | Coleccionables escondidos en los niveles; los jefes no entregan habilidades. Se guardan al recogerlos |
| Cuchillo | Reutiliza el sistema de la espada (`MeleeEffect`, `Torso_Melee`) |
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
| Granada de goma | Sale del cañón (pose de disparo hacia adelante, también agachada); arco fijo, sin límite en pantalla |
| Espada de fuego | Se usa de pie, en el aire y agachada (clip propio); destruye proyectiles enemigos |
| Lanzallamas | Movimiento al 50% |
| Taladro | Atraviesa enemigos sin límite; rompe un bloque destructible y se detiene |
| Obstáculos por habilidad | Bloques destructibles (taladro, Veyron) y muros de acero que se funden con calor acumulado (lanzallamas, Rosslyn). Nunca cierran el camino principal, tampoco en la misión final |
| Repetir misiones | Sí, con cualquier personaje, para abrir caminos y buscar coleccionables |
| Diálogos | Nombre + retrato + cuadro de texto, dentro de escenas cortas en el nivel (al estilo de Mega Man X): inicio y cierre de misión, entrada a la sala de cada jefe (diálogo + pose; sin diálogo al reintentar) y momentos especiales. Se pueden saltar |
| Escenas | Lista de pasos (`CutsceneData`), no Timeline. Los soldados se escriben por nombre: el elegido es el jugador y el otro aparece como no jugable. El que no se juega desaparece con un fundido a negro en todas las escenas |
| Historia | Dos soldados espaciales; el comandante da las misiones y resulta ser el traidor y jefe final |
| Personajes | 2 jugables en la versión 1, elegidos en una pantalla antes de cada misión; el comandante se agrega como tercero en la versión 2 al completar todas las misiones |
| Diferencias entre personajes | Estadísticas, disparo básico y habilidades. Todos comparten el movimiento (correr, saltar, agacharse, dash) y la cuadrícula del spritesheet |
| Progreso entre personajes | Las misiones completadas son del slot; las habilidades desbloqueadas son de cada personaje |
| Primer personaje | Rosslyn, la soldado espacial |
| Segundo personaje | Veyron, compañero de escuadrón de Rosslyn |
| Disparo básico de Veyron | Más daño y mucho menos alcance que el bláster (`maxRange`); mismas direcciones y cadencia |
| Diálogos y personajes | Los mismos para todos: en el lore los dos soldados van a cada misión |
| Audio | `AudioMixer` (Music, SFX/Gameplay, SFX/UI), `MusicPlayer` con intro + loop y crossfade, `SfxPlayer` con pool, variación, límite de voces y cooldown |
| Origen del audio | SFX de librerías (con registro de licencias en `CREDITS.md`); música compuesta por el equipo, exportada como intro + loop en `.ogg` |
| Guardado | 3 slots de partida; autoguardado al completar misiones y al recoger habilidades; `settings.json` separado de los slots; escritura segura con respaldo; versión y migraciones desde el inicio; no se guarda a mitad de misión |
| Láser | Atraviesa enemigos; jugador quieto en el suelo |
| Debilidades de jefes | No |
| Misión 0 | Misión de introducción en un simulador, al estilo de la primera de Mega Man X (sin tutorial explícito), solo con Rosslyn. Es lo primero de una partida nueva, desbloquea la selección de misiones y no cuenta para la misión final |
| Progresión | Misión 0 + selección libre de 5 misiones + misión final; todos los jefes se pueden vencer solo con el disparo básico de cualquier personaje |

### 17.2 Pendientes

Ninguna bloquea el inicio del desarrollo.

| # | Decisión | Opciones | Recomendación | Bloquea |
|---|---|---|---|---|
| 2 | En qué misión se esconde cada coleccionable | — | Repartir los 7 entre las 5 misiones, con el taladro y el lanzallamas accesibles sin habilidades y caminos de los dos personajes en cada misión (8.9) | Hito 6 |
| 3 | Nivel de amenaza en el briefing | Sí / no | Sí, orienta al jugador sin imponer un orden | Hito 6 |
| 4 | Misión final | Un nivel / varios niveles seguidos | Un nivel largo con varios checkpoints | Después del Hito 6 |
| 5 | Momento de la revelación del comandante | En la escena de inicio de la misión final / en la escena del jefe, antes de la pelea final (14.2) | Antes de la pelea: la misión empieza como una más y la traición se revela al llegar al jefe | Después del Hito 6 |
| 6 | Temas de jefe | Uno compartido + el del comandante / uno por jefe | Uno compartido al inicio; agregar más si hay tiempo | Hito 6 |
| 7 | Quién puede recoger un coleccionable | Solo su dueño / cualquiera (se desbloquea para su dueño) | Solo su dueño; el otro personaje lo ve como holograma. Deja claro que hay que volver con el otro personaje | Hito 5 |
| 8 | Veyron: diseño y estadísticas | — | Que se sienta distinto en algo medible (por ejemplo, más vida y menos velocidad) sin romper los mínimos de salto de 5.4 | Hito 7 |
| 10 | Arma sónica de Veyron | — | Que cubra algo que no hagan el láser (largo alcance) ni el cuchillo (cuerpo a cuerpo); por ejemplo, una onda corta que empuja enemigos | Hito 7 |
| 11 | Qué distingue al cuchillo del disparo básico de Veyron | Destruye proyectiles / casi sin costo y muy rápido / otro | Que destruya proyectiles enemigos como la espada; así tiene un uso defensivo que el disparo no tiene | Hito 7 |
| 12 | Misión 0: habilidad prestada | Sí (temporal, sin desbloquear) / no | No: como en Mega Man X, solo con el disparo básico. Las habilidades se descubren al encontrar los coleccionables | Hito 6 |
| 13 | Misión 0: final | Jefe holográfico que se puede vencer / pelea que no se puede ganar (como Vile en Mega Man X) | La pelea que no se gana: presenta la historia y puede sembrar la primera pista de la traición | Hito 6 |
| 14 | Misión 0: repetir y saltar | Repetible como "Simulador" / no · Saltable en partida nueva / no | Repetible desde la selección de misiones; saltable solo si ya hay otra partida que la completó | Hito 6 |
| 15 | Cómo aparece el comandante en las escenas | Holograma / transmisión en el cuadro de diálogo / en persona | Holograma durante las misiones y en persona en la misión final: refuerza la revelación | Hito 6 |
| 16 | Set de gestos de los personajes | — | Empezar con 4 (asentir, señalar, saludo, brazos cruzados) y agregar según lo pidan las escenas | Hito 6 |

---

## 18. Versión 2 (futuro)

Funciones planeadas para después de la versión 1. **No se implementan ahora**, pero la versión 1 se construye de forma que agregarlas no obligue a rehacer sistemas (ver 18.3).

### 18.1 Vehículos (estilo Metal Slug)

El personaje puede subirse a vehículos colocados en algunos niveles, pilotearlos y bajarse.

**Comportamiento base:**
- **Subir:** saltando dentro del vehículo o con un botón al estar junto a él.
- **Dentro del vehículo:** el personaje no recibe daño; el daño lo recibe el vehículo, que tiene su **propia barra de vida**.
- **Bajar:** con un botón. Al bajar, el personaje tiene invulnerabilidad breve.
- **Vehículo destruido:** expulsa al personaje automáticamente y explota, dañando a los enemigos cercanos (como el Metal Slug original).
- **Armas propias:** cada vehículo tiene su arma principal (por ejemplo, un cañón que apunta de forma independiente al movimiento) y opcionalmente una secundaria.

**Ideas de vehículos para el tema espacial:** tanque flotante (*hover tank*), mecha y nave para secciones de vuelo **[DECIDIR]**.

**Arquitectura:**

```csharp
public interface IPilotable
{
    void Enter(PlayerController pilot);
    void Exit();
    void Tick(InputSnapshot input, float dt);
    void FixedTick(float dt);
}
```

- `PlayerController` agrega un estado de nivel superior **`Piloting`**, junto a `Alive`, `Hurt`, `Dead` y `Disabled` (6.2). En `Piloting`, las FSM del jugador se desactivan, sus sprites y colliders se ocultan, y el `InputSnapshot` se le pasa al vehículo.
- Cada vehículo es un prefab con su propia FSM (`Idle`, `Move`, `Airborne`, `Destroyed`), su `Rigidbody2D`, su vida (`IDamageable`) y un `VehicleData` (ScriptableObject) con velocidad, vida, armas y sprites.
- **Apuntado del cañón en pixel art:** sin rotar sprites. El cañón tiene un sprite por dirección (8 o 16 direcciones **[DECIDIR]**).
- **Cámara:** cambia su objetivo al vehículo mientras se pilotea.
- **HUD:** muestra la barra de vida del vehículo mientras se pilotea.
- **Capas nuevas:** `Vehicle` y `VehicleHurtbox`.
- **[DECIDIR]** si se pueden usar habilidades dentro del vehículo. Recomendación: no, el vehículo reemplaza al arsenal del personaje mientras se usa.

### 18.2 Más misiones

La versión completa tendrá más misiones que las 5 de la versión 1.

- **Datos:** agregar una misión es crear un `MissionData` nuevo, su escena y sumarlo al `MissionCatalog`. El guardado usa `id`, así que las partidas de la versión 1 siguen funcionando.
- **Desbloqueo de la misión final:** se define en el `MissionCatalog` con una lista de misiones requeridas, no con un número fijo. Así se pueden agregar misiones opcionales o cambiar cuántas hacen falta.
- **Coleccionables:** `MissionData.abilityPickups` puede estar vacío, para misiones sin habilidades escondidas (por ejemplo, misiones de vehículo o secundarias).
- **Pantalla de selección:** debe admitir más de 5 misiones (cuadrícula con desplazamiento o páginas).
- **Historia:** las misiones nuevas pueden ampliar las pistas de la traición del comandante o agregar un arco posterior a la revelación **[DECIDIR]**.

**Decisiones de la versión 2:**

| # | Decisión | Opciones |
|---|---|---|
| 1 | Cuántas misiones tendrá la versión completa | — |
| 2 | ¿Las misiones nuevas esconden habilidades? | Sí (más habilidades) / solo algunas |
| 3 | Estructura de la historia | Más misiones antes de la revelación / un segundo acto después de ella |
| 4 | Tipos de vehículo | Tanque flotante / mecha / nave / otros |
| 5 | Misiones con vehículos | Secciones dentro de misiones normales / misiones completas en vehículo |
| 6 | Habilidades dentro del vehículo | Sí / no |
| 7 | Direcciones del cañón | 8 / 16 |
| 8 | Comandante jugable en la historia | Epílogo / modo aparte / sin explicación en el lore |
| 9 | Diálogos al jugar con el comandante | Los mismos / se omiten / variantes solo para él |
| 10 | Comandante bloqueado en la selección | Silueta con "???" / oculto hasta desbloquearse |

### 18.3 Preparación en la versión 1

Cambios baratos que se hacen **desde la versión 1** para que la versión 2 no obligue a rehacer sistemas:

| Preparación | Dónde | Por qué |
|---|---|---|
| El input se lee en `PlayerInputReader` y se pasa como `InputSnapshot`; nadie más lee el Input System | 5.2, 12 | El controller puede redirigir el input a un vehículo |
| La FSM de nivel superior del jugador admite estados nuevos | 6.2 | Agregar `Piloting` no toca la locomoción ni la acción |
| La cámara sigue un objetivo configurable, no al jugador fijo | 13 | Cambiar el objetivo al vehículo |
| Las barras del HUD son un componente reutilizable (`SegmentedBar`) | 15.6 | Vida, energía, jefe y vehículo usan la misma barra |
| El desbloqueo de la misión final se lee del `MissionCatalog` | 15.1 | Agregar misiones sin tocar código |
| `MissionData.abilityPickups` puede estar vacío | 15.1 | Misiones sin coleccionables |
| La pantalla de selección de misiones se arma desde el catálogo, no con 5 botones fijos | 15.1 | Admitir más misiones |
| El guardado tiene `version` y `SaveMigrator` desde el inicio | 15.4 | Las partidas de la versión 1 se actualizan solas al formato de la versión 2 |
| La pantalla de selección de personaje se arma desde el `CharacterCatalog`, con condiciones de desbloqueo | 15.1 | Agregar al comandante sin tocar código |
| `AbilityEffect` decide cuándo termina (`IsFinished`) | 8.4, 8.5 | Habilidades cargadas u otros modos sin tocar `UsingAbility` |
| Obstáculos por `ObstacleInteraction` en una sola capa `AbilityObstacle` | 15.2 | Tipos de obstáculo nuevos y más habilidades que los abran, sin capas nuevas |
| `DamageInfo` con `knockback` y `stunDuration` | 10.1 | Armas que empujan o aturden (el arma sónica, y más en la V2) |
| Valores propios de cada habilidad en su efecto, no en `AbilityData` | 8.3 | `AbilityData` no crece con cada habilidad |
| Tabla de clips en un asset `SpritesheetLayout` | 4.2.1 | Clips nuevos sin tocar `PlayerArtSetup.cs` |
| Selección de habilidad desde el menú de pausa | 8.1, 15.1 | Elegir rápido con más habilidades por personaje |
| El progreso de habilidades se guarda en una lista por `id` de personaje | 15.4 | El comandante tiene su propio progreso sin migración |
| Todo personaje es un `CharacterData` + prefab variant con la misma cuadrícula de spritesheet | 4.2.1, 5.4 | El comandante jugable reutiliza todo el pipeline |

### 18.4 Tercer personaje: el comandante

Al completar **todas las misiones** (en la versión 1 eso incluye la misión final, donde se le vence), el comandante se vuelve **personaje jugable**.

- **Desbloqueo:** en el `CharacterCatalog` (15.1) su `requiredMissions` incluye la misión final. Si en la versión 2 hay más misiones y se quiere exigir todas, se agregan a esa lista sin tocar código.
- **Personaje:** igual que los otros dos (5.4): su `CharacterData`, prefab variant, estadísticas, disparo básico y habilidades **[DECIDIR]**, con su propio progreso de habilidades en el guardado.
- **Arte:** un spritesheet con la misma cuadrícula que los soldados (4.2.1). El sprite del comandante como jefe es aparte (un enemigo de una sola pieza, 14.1), aunque puede servir de referencia.
- **Historia y diálogos [DECIDIR]:** en los diálogos de las misiones el comandante es quien da las órdenes, así que la regla de "los mismos diálogos para todos" choca con jugarlo a él. Ver decisiones 8 y 9 de 18.2.
