namespace GestionPartidosACB;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public static class GestorDatos
{
    // Constantes utilizadas para localizar las columnas en la cabecera del CSV sin acoplar el código a una posición fija
    private const string ColJornada = "jornada";
    private const string ColLocal = "equipo_local";
    private const string ColVisitante = "equipo_visitante";
    private const string ColResultado = "resultado";
    private const string ColMvp = "mvp_partido";
    private const string ColPuntosMvp = "puntos_mvp";
    private const string ColAsistenciasMvp = "asistencias_mvp";
    private const string ColRebotesMvp = "rebotes_mvp";
    private const string ColEquipoMvp = "equipo_mvp";

    public static List<Partido> CargarPartidos(string rutaCsv)
    {
        var partidos = new List<Partido>();

        try
        {
            if (!File.Exists(rutaCsv))
            {
                Console.WriteLine($"Error: no se encontró el fichero CSV '{rutaCsv}'.");
                return new List<Partido>();
            }

            // Carga todo el texto del archivo asegurando el formato UTF-8 y eliminando el BOM inicial si existiera
            string texto = File.ReadAllText(rutaCsv, Encoding.UTF8);
            texto = texto.TrimStart('\uFEFF');

            // Trocea el archivo completo por sus saltos de línea
            var lineas = texto.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            if (lineas.Length == 0 || string.IsNullOrWhiteSpace(lineas[0]))
            {
                Console.WriteLine("Error: el fichero CSV está vacío o no tiene encabezado.");
                return new List<Partido>();
            }

            // Obtiene los encabezados de la primera línea dividiendo por comas
            string[] encabezados = lineas[0].Split(',');

            // Mapea dinámicamente las posiciones numéricas de cada columna dentro de la cabecera
            int idxJornada = IndexDe(encabezados, ColJornada);
            int idxLocal = IndexDe(encabezados, ColLocal);
            int idxVisitante = IndexDe(encabezados, ColVisitante);
            int idxResultado = IndexDe(encabezados, ColResultado);
            int idxMvp = IndexDe(encabezados, ColMvp);
            int idxPuntosMvp = IndexDe(encabezados, ColPuntosMvp);
            int idxAsistenciasMvp = IndexDe(encabezados, ColAsistenciasMvp);
            int idxRebotesMvp = IndexDe(encabezados, ColRebotesMvp);
            int idxEquipoMvp = IndexDe(encabezados, ColEquipoMvp);

            for (int i = 1; i < lineas.Length; i++)
            {
                string linea = lineas[i];
                if (string.IsNullOrWhiteSpace(linea)) continue;

                // Separa los campos individuales del partido actual
                string[] campos = linea.Split(',');

                // Crea el modelo con los textos leídos de forma segura
                var p = new Partido
                {
                    Jornada = Obtener(campos, idxJornada),
                    NumeroJornada = ExtraerNumeroJornada(Obtener(campos, idxJornada)),
                    EquipoLocal = Obtener(campos, idxLocal),
                    EquipoVisitante = Obtener(campos, idxVisitante),
                    Resultado = Obtener(campos, idxResultado),
                    Mvp = Obtener(campos, idxMvp),
                    EquipoMvp = Obtener(campos, idxEquipoMvp)
                };

                // Descompone la puntuación (ej. "80-75") en sus dos marcadores enteros
                (p.PuntosLocal, p.PuntosVisitante) = ParsearResultado(p.Resultado);

                // Parsea numéricamente la información estadística evitando que campos vacíos rompan el flujo
                int.TryParse(Obtener(campos, idxPuntosMvp), out int pts);
                int.TryParse(Obtener(campos, idxAsistenciasMvp), out int ast);
                int.TryParse(Obtener(campos, idxRebotesMvp), out int reb);
                p.PuntosMvp = pts;
                p.AsistenciasMvp = ast;
                p.RebotesMvp = reb;

                partidos.Add(p);
            }
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine($"Error: no se tienen permisos para leer '{rutaCsv}'.");
            return new List<Partido>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error inesperado al leer el CSV: {ex.Message}");
            return new List<Partido>();
        }

        return partidos;
    }

    private static int IndexDe(string[] encabezados, string nombre)
    {
        for (int i = 0; i < encabezados.Length; i++)
        {
            // Compara los títulos de columna sin importar espacios o mayúsculas
            if (string.Equals(encabezados[i].Trim(), nombre, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }

    private static string Obtener(string[] campos, int indice)
    {
        // Protege la extracción evitando excepciones de fuera de rango en arrays con celdas faltantes
        if (indice < 0 || indice >= campos.Length) return string.Empty;
        return campos[indice].Trim();
    }

    private static int ExtraerNumeroJornada(string textoJornada)
    {
        if (string.IsNullOrWhiteSpace(textoJornada)) return 0;
        var partes = textoJornada.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length == 0) return 0;
        // Toma la última palabra de la cadena (donde se ubica el número de jornada) para convertirla a entero
        string ultimo = partes[partes.Length - 1];
        int n = 0;
        if (int.TryParse(ultimo, out n))
            return n;
        return 0;
    }

    private static (int, int) ParsearResultado(string resultado)
    {
        if (string.IsNullOrWhiteSpace(resultado)) return (-1, -1);
        // Divide el tanteo separado por guion
        var partes = resultado.Trim().Split('-');
        if (partes.Length != 2) return (-1, -1);
        if (!int.TryParse(partes[0].Trim(), out int local)) return (-1, -1);
        if (!int.TryParse(partes[1].Trim(), out int visitante)) return (-1, -1);
        return (local, visitante);
    }

    public static List<Partido> BuscarPorEquipo(List<Partido> partidos, string nombreEquipo)
    {
        // Filtra los partidos donde juegue el equipo y los ordena cronológicamente por número de jornada
        return partidos
            .Where(p => p.Participa(nombreEquipo))
            .OrderBy(p => p.NumeroJornada)
            .ToList();
    }

    public static List<Partido> BuscarPorJornada(List<Partido> partidos, int numeroJornada)
    {
        // Filtra los encuentros que coincidan exactamente con el número de jornada
        return partidos
            .Where(p => p.NumeroJornada == numeroJornada)
            .OrderBy(p => p.NumeroJornada)
            .ToList();
    }

    public static List<(string Jugador, int Veces)> TopMvps(List<Partido> partidos, int top = 3)
    {
        // Mantiene la cuenta de galardones utilizando un diccionario insensible a mayúsculas
        var conteo = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in partidos)
        {
            if (!string.IsNullOrWhiteSpace(p.Mvp))
            {
                if (conteo.ContainsKey(p.Mvp))
                    conteo[p.Mvp]++;
                else
                    conteo[p.Mvp] = 1;
            }
        }

        // Ordena por premios (descendente) y desempata por nombre alfabético, tomando solo la cantidad deseada
        return conteo
            .OrderByDescending(kvp => kvp.Value)
            .ThenBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase)
            .Take(top)
            .Select(kvp => (kvp.Key, kvp.Value))
            .ToList();
    }

    public sealed class EquipoStats
    {
        public string Nombre { get; set; } = string.Empty;
        public int Victorias { get; set; }
        public int Derrotas { get; set; }
        public int PuntosFavor { get; set; }
        public int PuntosContra { get; set; }
        // Propiedades dinámicas que calculan automáticamente partidos jugados y la diferencia de puntos
        public int Jugados => Victorias + Derrotas;
        public int Diferencia => PuntosFavor - PuntosContra;
    }

    public static List<EquipoStats> CalcularClasificacion(List<Partido> partidos)
    {
        var stats = new Dictionary<string, EquipoStats>(StringComparer.OrdinalIgnoreCase);

        foreach (var p in partidos)
        {
            // Inicializa las fichas estadísticas de los equipos involucrados si es la primera vez que aparecen
            if (!stats.ContainsKey(p.EquipoLocal))
                stats[p.EquipoLocal] = new EquipoStats { Nombre = p.EquipoLocal };
            if (!stats.ContainsKey(p.EquipoVisitante))
                stats[p.EquipoVisitante] = new EquipoStats { Nombre = p.EquipoVisitante };

            var local = stats[p.EquipoLocal];
            var visitante = stats[p.EquipoVisitante];

            // Acumula el volumen de puntos anotados y recibidos por cada bando
            local.PuntosFavor += p.PuntosLocal;
            local.PuntosContra += p.PuntosVisitante;
            visitante.PuntosFavor += p.PuntosVisitante;
            visitante.PuntosContra += p.PuntosLocal;

            // Determina la victoria/derrota y computa el balance
            if (p.PuntosLocal > p.PuntosVisitante)
            {
                local.Victorias++;
                visitante.Derrotas++;
            }
            else if (p.PuntosVisitante > p.PuntosLocal)
            {
                visitante.Victorias++;
                local.Derrotas++;
            }
            else
            {
                local.Victorias++;
                local.Derrotas++;
                visitante.Victorias++;
                visitante.Derrotas++;
            }
        }

        // Resuelve la tabla de clasificación aplicando los 4 criterios de desempate en orden en una sola consulta
        return stats.Values
            .OrderByDescending(e => e.Victorias)
            .ThenByDescending(e => e.Diferencia)
            .ThenByDescending(e => e.PuntosFavor)
            .ThenBy(e => e.Nombre, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static List<string> ObtenerEquipos(List<Partido> partidos)
    {
        // Proyecta y aplana las dos listas de equipos (Locales y Visitantes) en un único flujo de datos
        return partidos
            .SelectMany(p => new[] { p.EquipoLocal, p.EquipoVisitante })
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}