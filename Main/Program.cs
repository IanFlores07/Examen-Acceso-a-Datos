namespace GestionPartidosACB;

using System.IO;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        string rutaCsv = "acb2526.csv";
        if (!File.Exists(rutaCsv))
        {
            Console.WriteLine($"No se encontró '{rutaCsv}'.");
            Console.Write("Ruta del CSV: ");
            rutaCsv = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(rutaCsv))
            {
                Console.WriteLine("Ruta inválida. Saliendo.");
                return;
            }
        }

        // Carga la colección completa de partidos parseando el archivo CSV
        var partidos = GestorDatos.CargarPartidos(rutaCsv);
        // Aborta la ejecución si no se pudo obtener ningún registro válido
        if (partidos.Count == 0)
        {
            Console.WriteLine("No se pudieron cargar los datos. Saliendo.");
            return;
        }

        Console.WriteLine($"\nCargados {partidos.Count} partidos de '{rutaCsv}'.");
        // Inicia el bucle de la aplicación pasándole los partidos en memoria
        EjecutarMenu(partidos);
    }

    static void EjecutarMenu(List<Partido> partidos)
    {
        bool salir = false;
        while (!salir)
        {
            Console.WriteLine("    GESTIÓN DE PARTIDOS ACB - TEMPORADA 25-26    ");
            Console.WriteLine("1. Resultados de un equipo concreto");
            Console.WriteLine("2. Datos de una jornada concreta");
            Console.WriteLine("3. Top 3 de MVPs");
            Console.WriteLine("4. Clasificación");
            Console.WriteLine("5. Exportar resultados por equipo");
            Console.WriteLine("6. Salir");
            Console.Write("Selecciona una opción (1-6): ");
            string opcion = LeerOpcion();

            switch (opcion)
            {
                case "1": OpcionResultadosEquipo(partidos); break;
                case "2": OpcionDatosJornada(partidos); break;
                case "3": OpcionTopMvps(partidos); break;
                case "4": OpcionClasificacion(partidos); break;
                case "5": OpcionExportarEquipos(partidos); break;
                case "6":
                    Console.WriteLine("Cerrando programa...");
                    salir = true;
                    break;
                default:
                    Console.WriteLine("Opción no válida. Selecciona una opción del 1 al 6.");
                    break;
            }

            if (!salir)
            {
                Console.WriteLine();
                Console.Write("Presiona Enter para continuar...");
                try { Console.ReadLine(); } catch { }
                Console.WriteLine();
            }
        }
    }

    static string LeerOpcion()
    {
        try
        {
            string linea = Console.ReadLine();
            if (linea == null) linea = "";
            return linea.Trim();
        }
        catch { return ""; }
    }

    static void OpcionResultadosEquipo(List<Partido> partidos)
    {
        Console.Write("Introduce el nombre del equipo (ej. 'Bilbao Basket'): ");
        string nombre = "";
        try
        {
            string linea = Console.ReadLine();
            if (linea == null) linea = "";
            nombre = linea.Trim();
        }
        catch (Exception ex) { Console.WriteLine($"Error al leer el nombre: {ex.Message}"); return; }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            Console.WriteLine("Debes introducir un nombre de equipo.");
            return;
        }

        // Filtra los partidos del equipo solicitado y los devuelve ordenados cronológicamente
        var partidosEquipo = GestorDatos.BuscarPorEquipo(partidos, nombre);

        if (partidosEquipo.Count == 0)
        {
            Console.WriteLine($"No se encontró ningún equipo llamado '{nombre}'.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"Resultados de {nombre}:");
        Console.WriteLine();

        // Construye de forma eficiente en memoria el bloque de texto que se ofrecerá guardar
        StringBuilder sb = new();
        sb.AppendLine($"Resultados de {nombre}:");
        sb.AppendLine();

        foreach (var p in partidosEquipo)
        {
            Console.WriteLine($"  {p}");
            sb.AppendLine($"  {p}");
        }

        Console.WriteLine();
        Console.WriteLine("Al guardar, los datos se irán acumulando en el mismo fichero .txt cada vez que elijas un equipo (no se borrará lo anterior).");
        Console.WriteLine();

        // Solicita confirmación para anexar (modo append) el texto generado al fichero del equipo
        Exportador.OfrecerGuardarAcumulado(sb.ToString(), "Equipos");
    }

    static void OpcionDatosJornada(List<Partido> partidos)
    {
        Console.Write("Introduce el número de jornada: ");
        string texto = "";
        try
        {
            string linea = Console.ReadLine();
            if (linea == null) linea = "";
            texto = linea.Trim();
        }
        catch (Exception ex) { Console.WriteLine($"Error al leer la jornada: {ex.Message}"); return; }

        int numero;
        // Convierte de forma segura el texto ingresado a un número entero
        if (!int.TryParse(texto, out numero))
        {
            Console.WriteLine("Número de jornada inválido.");
            return;
        }

        // Obtiene únicamente los partidos correspondientes al número de jornada indicado
        var partidosJornada = GestorDatos.BuscarPorJornada(partidos, numero);

        if (partidosJornada.Count == 0)
        {
            Console.WriteLine($"No existe la jornada {numero} o no tiene partidos.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"Jornada {numero}:");
        Console.WriteLine();

        StringBuilder sb = new();
        sb.AppendLine($"Jornada {numero}:");
        sb.AppendLine();

        foreach (var p in partidosJornada)
        {
            Console.WriteLine($"  {p}");
            sb.AppendLine($"  {p}");
        }

        // Ofrece la opción de guardar en fichero sobrescribiendo su contenido
        Exportador.OfrecerGuardar(sb.ToString(), "Jornadas");
    }

    static void OpcionTopMvps(List<Partido> partidos)
    {
        // Calcula y recupera los 3 jugadores que más veces han sido nombrados MVP
        var top = GestorDatos.TopMvps(partidos, 3);

        Console.WriteLine();
        Console.WriteLine("Top 3 de MVPs:");
        Console.WriteLine();

        StringBuilder sb = new();
        sb.AppendLine("Top 3 de MVPs:");
        sb.AppendLine();

        if (top.Count == 0)
        {
            Console.WriteLine("  No hay registros de MVP.");
            sb.AppendLine("  No hay registros de MVP.");
        }
        else
        {
            int i = 1;
            // Desestructura las tuplas devueltas (nombre del jugador, cantidad de MVPs)
            foreach (var (jugador, veces) in top)
            {
                Console.WriteLine($"  {i}. {jugador} - {veces} veces como MVP");
                sb.AppendLine($"  {i}. {jugador} - {veces} veces como MVP");
                i++;
            }
        }

        Exportador.OfrecerGuardar(sb.ToString(), "Top3MVP");
    }

    static void OpcionClasificacion(List<Partido> partidos)
    {
        // Procesa el historial completo de encuentros para calcular victorias, derrotas y puntos
        var clasificacion = GestorDatos.CalcularClasificacion(partidos);

        Console.WriteLine();
        Console.WriteLine("Clasificación ACB - Temporada 25-26:");
        Console.WriteLine();
        // Alinea las cabeceras fijando anchos de columna mediante intrapolación de cadenas
        Console.WriteLine($"{"Pos.",-6}{"Equipo",-40}{"J",-4}{"V",-4}{"D",-4}{"PF",-6}{"PC",-6}{"Dif",-6}");
        Console.WriteLine(new string('-', 72));

        StringBuilder sb = new();
        sb.AppendLine("Clasificación ACB - Temporada 25-26:");
        sb.AppendLine();
        sb.AppendLine($"{"Pos.",-6}{"Equipo",-40}{"J",-4}{"V",-4}{"D",-4}{"PF",-6}{"PC",-6}{"Dif",-6}");
        sb.AppendLine(new string('-', 72));

        int pos = 1;
        foreach (var e in clasificacion)
        {
            // Da formato a la fila del equipo alineando adecuadamente cada métrica
            string linea = $"  {pos,-4}{e.Nombre,-40}{e.Jugados,-4}{e.Victorias,-4}{e.Derrotas,-4}{e.PuntosFavor,-6}{e.PuntosContra,-6}{e.Diferencia,-6}";
            Console.WriteLine(linea);
            sb.AppendLine(linea);
            pos++;
        }

        sb.AppendLine();
        sb.AppendLine("Criterio de desempate: 1) diferencia de puntos, 2) puntos a favor, 3) nombre alfabético.");

        Console.WriteLine();
        Console.WriteLine("Criterio de desempate: 1) diferencia de puntos, 2) puntos a favor, 3) nombre alfabético.");

        Exportador.OfrecerGuardar(sb.ToString(), "Clasificacion");
    }

    static void OpcionExportarEquipos(List<Partido> partidos)
    {
        // Obtiene el listado único de todos los clubes participantes
        var equipos = GestorDatos.ObtenerEquipos(partidos);
        // Ejecuta el proceso masivo de generación de ficheros y captura las métricas resultantes (tupla)
        var (creados, errores, carpeta) = Exportador.ExportarPorEquipo(partidos, equipos);

        Console.WriteLine();
        Console.WriteLine($"Exportación finalizada. Carpeta: {carpeta}");
        Console.WriteLine($"Ficheros creados: {creados}.");
        if (errores > 0)
            Console.WriteLine($"Ficheros con errores: {errores}.");
    }
}