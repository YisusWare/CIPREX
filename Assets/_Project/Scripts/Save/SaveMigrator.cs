using System;
using UnityEngine;

namespace Game.Save
{
    /// <summary>
    /// Actualiza partidas de versiones anteriores (sección 15.4). Los pasos se aplican en orden
    /// (v1 → v2 → v3…). Hoy el formato está en la versión 1, así que no hay pasos.
    ///
    /// Para cambiar el formato:
    /// 1. Subir SaveSystem.CurrentVersion.
    /// 2. Agregar aquí el paso de la versión anterior a la nueva.
    /// Los campos nuevos no necesitan paso (JsonUtility deja su valor por defecto). Un paso hace falta
    /// cuando un campo cambia de nombre o de significado: el paso recibe el JSON original para leer
    /// los campos viejos que ya no existen en SaveData.
    /// </summary>
    public static class SaveMigrator
    {
        /// <summary>Paso de la versión 'from' a 'from + 1'.</summary>
        public delegate void Step(SaveData data, string originalJson);

        /// <summary>Índice 0 = paso de v1 a v2, índice 1 = paso de v2 a v3…</summary>
        static readonly Step[] Steps =
        {
            // (data, json) => { ... },   // v1 → v2
        };

        /// <summary>Aplica los pasos que falten. Devuelve false si falta alguno (no se debe usar la partida).</summary>
        public static bool Migrate(SaveData data, string originalJson)
        {
            while (data.version < SaveSystem.CurrentVersion)
            {
                int index = data.version - 1;
                if (index < 0 || index >= Steps.Length)
                {
                    Debug.LogError($"[Guardado] No hay paso de migración desde la versión {data.version}.");
                    return false;
                }
                try
                {
                    Steps[index](data, originalJson);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Guardado] Falló la migración desde la versión {data.version}: {e}");
                    return false;
                }
                Debug.Log($"[Guardado] Migrado de la versión {data.version} a la {data.version + 1}.");
                data.version++;
            }
            return true;
        }
    }
}
