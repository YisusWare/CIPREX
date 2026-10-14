using System.Collections.Generic;
using System.Text;
using Game.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace Game.Player
{
    /// <summary>
    /// Diagnóstico de física del jugador (solo en el editor y en builds de desarrollo).
    /// Guarda un historial de los últimos pasos de física y avisa en la consola cuando:
    /// - el jugador queda hundido dentro de un tile sólido (se mide con los tiles, no con los colliders,
    ///   así que también detecta el caso en que el collider del suelo es hueco y no lo empuja hacia fuera);
    /// - cae por debajo del nivel (y opcionalmente lo devuelve a su punto de reaparición).
    /// Al empezar, revisa los colliders del suelo y avisa de configuraciones riesgosas.
    /// F9 escribe el historial en la consola en cualquier momento.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class PlayerPhysicsMonitor : MonoBehaviour
    {
        [SerializeField] PlayerController player;
        [Tooltip("Capas sólidas que se revisan (Ground, AbilityObstacle).")]
        [SerializeField] LayerMask solidMask;
        [Tooltip("Hundimiento (u) a partir del cual se avisa. 1 px = 0.0625.")]
        [SerializeField] float sinkWarning = 0.0625f;
        [Tooltip("Pasos de física que se guardan (60 = 1 s).")]
        [SerializeField, Min(10)] int historySize = 120;
        [Tooltip("Pasos que se muestran en cada aviso.")]
        [SerializeField, Min(5)] int reportSize = 45;
        [Tooltip("Al caer fuera del nivel, devolver al jugador a su punto de reaparición para seguir probando.")]
        [SerializeField] bool rescueOnFall = true;

        struct Sample
        {
            public int step;
            public float time;
            public Vector2 position, velocity, colliderSize;
            public bool grounded, touchingSolid, simulated;
            public IState loco, action;
            public PlayerLifeState life;
            public float sink;
            public string sinkTilemap;
        }

        Sample[] history;
        int head, count, step;
        Rigidbody2D rb;
        BoxCollider2D box;
        readonly List<Tilemap> solidTilemaps = new();
        readonly List<CompositeCollider2D> composites = new();
        float killY = float.NegativeInfinity;
        bool sinkReported;
        int stepsOutside;
        bool emptyGeometryReported;

        void Awake()
        {
            if (!Debug.isDebugBuild) { enabled = false; return; }
            if (player == null) player = GetComponent<PlayerController>();
            rb = GetComponent<Rigidbody2D>();
            box = GetComponent<BoxCollider2D>();
            history = new Sample[historySize];
        }

        void Start()
        {
            ScanLevel();
        }

        // ------------------------------------------------------------------ Nivel

        void ScanLevel()
        {
            var report = new StringBuilder("[Física] Colliders sólidos del nivel:\n");
            float minY = float.PositiveInfinity;
            bool risky = false;

            foreach (var col in FindObjectsByType<Collider2D>(FindObjectsSortMode.None))
            {
                if ((solidMask.value & (1 << col.gameObject.layer)) == 0 || col.isTrigger) continue;
                if (col is CompositeCollider2D composite)
                {
                    composites.Add(composite);
                    bool hollow = composite.geometryType == CompositeCollider2D.GeometryType.Outlines;
                    report.AppendLine($"  {Path(col.transform)}: CompositeCollider2D {composite.geometryType}, " +
                                      $"{composite.pathCount} contornos, {composite.shapeCount} formas" +
                                      (hollow ? "  ← HUECO: si el jugador queda dentro, no lo empuja hacia fuera" : ""));
                    risky |= hollow;
                }
                else if (col is TilemapCollider2D tc)
                {
                    report.AppendLine($"  {Path(col.transform)}: TilemapCollider2D, composite = {tc.compositeOperation}, " +
                                      $"{tc.shapeCount} formas" +
                                      (tc.compositeOperation == Collider2D.CompositeOperation.None
                                          ? "  ← un collider por tile: puede trabarse en las uniones entre tiles"
                                          : ""));
                }
                if (col.shapeCount > 0) minY = Mathf.Min(minY, col.bounds.min.y);
            }

            foreach (var tilemap in FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
                if ((solidMask.value & (1 << tilemap.gameObject.layer)) != 0) solidTilemaps.Add(tilemap);

            if (!float.IsInfinity(minY)) killY = minY - 2f;
            report.Append($"  Línea de caída: y = {killY:0.##}");
            if (risky) Debug.LogWarning(report.ToString(), this);
            else Debug.Log(report.ToString(), this);
        }

        static string Path(Transform t) => t.parent != null ? $"{t.parent.name}/{t.name}" : t.name;

        // ------------------------------------------------------------------ Muestreo

        void FixedUpdate()
        {
            if (player == null || !player.IsInitialized || player.Context == null) return;
            step++;

            // ¿El suelo todavía no tiene geometría cuando el jugador ya se mueve?
            if (!emptyGeometryReported)
                foreach (var c in composites)
                    if (c.pathCount == 0 && c.shapeCount == 0)
                    {
                        emptyGeometryReported = true;
                        Debug.LogWarning($"[Física] En el paso {step} el collider {Path(c.transform)} aún no tiene geometría.", c);
                    }

            float sink = MeasureSink(out string tilemapName);
            var s = new Sample
            {
                step = step,
                time = Time.time,
                position = rb.position,
                velocity = rb.linearVelocity,
                colliderSize = box.size,
                grounded = player.Motor.IsGrounded,
                touchingSolid = box.IsTouchingLayers(solidMask),
                simulated = rb.simulated,
                loco = player.Locomotion.Current,
                action = player.Action.Current,
                life = player.LifeState,
                sink = sink,
                sinkTilemap = tilemapName,
            };
            history[head] = s;
            head = (head + 1) % history.Length;
            count = Mathf.Min(count + 1, history.Length);

            if (sink >= sinkWarning)
            {
                stepsOutside = 0;
                if (!sinkReported)
                {
                    sinkReported = true;
                    Debug.LogWarning($"[Física] El jugador está hundido {sink / 0.0625f:0.#} px dentro de '{tilemapName}'.\n" +
                                     Report(), this);
                }
            }
            else if (sinkReported && ++stepsOutside > 30)
            {
                sinkReported = false; // vuelve a avisar en el próximo episodio
            }

            if (rb.position.y < killY)
            {
                Debug.LogError($"[Física] El jugador cayó fuera del nivel (y = {rb.position.y:0.##} < {killY:0.##}).\n" +
                               Report(), this);
                if (rescueOnFall)
                {
                    player.Motor.Teleport(player.RespawnPoint);
                    Debug.Log("[Física] Devuelto al punto de reaparición (rescueOnFall).", this);
                }
                else
                {
                    killY = float.NegativeInfinity; // un solo aviso
                }
            }
        }

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.f9Key.wasPressedThisFrame)
                Debug.Log("[Física] Historial (F9):\n" + Report(), this);
        }

        /// <summary>
        /// Cuánto están los pies por debajo de la superficie del tile sólido en el que están metidos.
        /// Revisa tres puntos un poco por encima de la base del collider (izquierda, centro, derecha).
        /// </summary>
        float MeasureSink(out string tilemapName)
        {
            tilemapName = null;
            Bounds b = box.bounds;
            float inset = 0.05f;
            float y = b.min.y + 0.01f;
            float best = 0f;
            float[] xs = { b.min.x + inset, b.center.x, b.max.x - inset };

            foreach (var tilemap in solidTilemaps)
            {
                if (tilemap == null) continue;
                float cellH = tilemap.layoutGrid != null ? tilemap.layoutGrid.cellSize.y : 1f;
                foreach (float x in xs)
                {
                    var cell = tilemap.WorldToCell(new Vector3(x, y, 0f));
                    if (!tilemap.HasTile(cell)) continue;
                    // Subir hasta la superficie de esa columna.
                    var top = cell;
                    int guard = 0;
                    while (tilemap.HasTile(top + Vector3Int.up) && guard++ < 64) top += Vector3Int.up;
                    float surface = tilemap.CellToWorld(top).y + cellH;
                    float depth = surface - b.min.y;
                    if (depth > best)
                    {
                        best = depth;
                        tilemapName = tilemap.name;
                    }
                }
            }
            return best;
        }

        string Report()
        {
            var sb = new StringBuilder();
            sb.AppendLine("paso   tiempo   posición          velocidad        suelo toca hundido  collider     loco / acción / vida");
            int n = Mathf.Min(reportSize, count);
            for (int i = n; i >= 1; i--)
            {
                var s = history[(head - i + history.Length) % history.Length];
                sb.AppendLine(
                    $"{s.step,5}  {s.time,6:0.000}  ({s.position.x,7:0.000},{s.position.y,7:0.000})  " +
                    $"({s.velocity.x,6:0.0},{s.velocity.y,6:0.0})  {(s.grounded ? "sí" : "no"),-4} " +
                    $"{(s.touchingSolid ? "sí" : "no"),-4} {s.sink / 0.0625f,5:0.#}px  " +
                    $"{s.colliderSize.x:0.##}×{s.colliderSize.y:0.##}  " +
                    $"{Name(s.loco)} / {Name(s.action)} / {s.life}{(s.simulated ? "" : " (sin física)")}");
            }
            sb.Append("Suelo = PlayerMotor.IsGrounded · toca = el collider toca una capa sólida · hundido = medido con los tiles");
            return sb.ToString();
        }

        static string Name(IState state) =>
            state == null ? "-" : state.GetType().Name.Replace("State", "").Replace("Action", "");
    }
}
