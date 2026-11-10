namespace GestionPartidosACB;

using System.IO;
using System.Text;

public static class Exportador
{
    public static string PedirNombreFichero()
    {
        while (true)
        {
            Console.Write("Introduce el nombre del fichero .txt (o vacío para cancelar): ");
            string nombre = "";
            try
            {
                string linea = Console.ReadLine();
                if (linea == null)
                    linea = "";
                nombre = linea.Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al leer el nombre: {ex.Message}");
                continue;
            }

            if (string.IsNullOrWhiteSpace(nombre))
                return "";

            // Asegura la extensión .txt si el usuario la omitió
            if (!nombre.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                nombre += ".txt";

            // Comprueba que no se haya introducido únicamente la extensión como nombre
            string nombreSinExt = nombre.Substring(0, nombre.Length - 4);

            if (nombreSinExt.Trim().Length == 0)
            {
                Console.WriteLine("Nombre inválido: no puede estar vacío.");
                continue;
            }
            return nombre;
        }
    }

    public static bool GuardarEnCarpeta(string contenido, string nombreFichero, string nombreCarpeta)
    {
        try
        {
            // Construye la ruta al directorio combinándola de forma segura con el directorio de trabajo del sistema
            string carpetaCompleta = Path.Combine(Directory.GetCurrentDirectory(), nombreCarpeta);
            Directory.CreateDirectory(carpetaCompleta);

            // Genera la ruta absoluta del archivo y sobrescribe su contenido en codificación UTF-8
            string rutaCompleta = Path.Combine(carpetaCompleta, nombreFichero);
            File.WriteAllText(rutaCompleta, contenido, Encoding.UTF8);
            Console.WriteLine($"Guardado en '{nombreCarpeta}\\{nombreFichero}'.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"No se pudo guardar '{nombreFichero}': {ex.Message}");
            return false;
        }
    }

    public static bool AñadirEnCarpeta(string contenido, string nombreFichero, string nombreCarpeta)
    {
        try
        {
            string carpetaCompleta = Path.Combine(Directory.GetCurrentDirectory(), nombreCarpeta);
            Directory.CreateDirectory(carpetaCompleta);

            string rutaCompleta = Path.Combine(carpetaCompleta, nombreFichero);
            // Anexa el nuevo bloque de contenido al final del archivo existente (modo append) agregando salto de línea
            File.AppendAllText(rutaCompleta, contenido + Environment.NewLine, Encoding.UTF8);
            Console.WriteLine($"Añadido en '{nombreCarpeta}\\{nombreFichero}'.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"No se pudo añadir a '{nombreFichero}': {ex.Message}");
            return false;
        }
    }

    public static void OfrecerGuardar(string contenido, string nombreCarpeta)
    {
        Console.Write("¿Desea guardar esta información en un fichero .txt? (s/n): ");
        string respuesta = "";
        try
        {
            string linea = Console.ReadLine();
            if (linea == null)
                linea = "";
            respuesta = linea.Trim().ToLowerInvariant();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al leer la respuesta: {ex.Message}");
            return;
        }

        if (respuesta == "s")
        {
            string nombreFichero = PedirNombreFichero();
            if (nombreFichero != "")
                // Llama al guardado que sobrescribe el archivo existente
                GuardarEnCarpeta(contenido, nombreFichero, nombreCarpeta);
        }
    }

    public static void OfrecerGuardarAcumulado(string contenido, string nombreCarpeta)
    {
        Console.Write("¿Desea guardar esta información en un fichero .txt? (s/n): ");
        string respuesta = "";
        try
        {
            string linea = Console.ReadLine();
            if (linea == null)
                linea = "";
            respuesta = linea.Trim().ToLowerInvariant();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al leer la respuesta: {ex.Message}");
            return;
        }

        if (respuesta == "s")
        {
            string nombreFichero = PedirNombreFichero();
            if (nombreFichero != "")
                // Llama al guardado que anexa/acumula información al final del archivo existente
                AñadirEnCarpeta(contenido, nombreFichero, nombreCarpeta);
        }
    }

    public static (int creados, int errores, string carpeta) ExportarPorEquipo(List<Partido> partidos, List<string> equipos)
    {
        string nombreCarpeta = "ExportEquipos";
        string carpetaCompleta = Path.Combine(Directory.GetCurrentDirectory(), nombreCarpeta);
        try
        {
            Directory.CreateDirectory(carpetaCompleta);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"No se pudo crear la carpeta '{nombreCarpeta}': {ex.Message}");
            return (0, 0, "");
        }

        int creados = 0;
        int errores = 0;

        // Procesa la exportación masiva iterando todos los clubes en orden alfabético
        foreach (string equipo in equipos.OrderBy(e => e, StringComparer.OrdinalIgnoreCase))
        {
            var partidosEquipo = GestorDatos.BuscarPorEquipo(partidos, equipo);
            StringBuilder sb = new();
            sb.AppendLine($"PARTIDOS DEL EQUIPO: {equipo}");
            sb.AppendLine();

            foreach (var p in partidosEquipo)
            {
                sb.AppendLine($"  {p}");
            }

            // Normaliza el nombre del equipo eliminando caracteres no aptos para ficheros
            string nombreSeguro = NombreSeguroParaFichero(equipo);
            string nombreFichero = nombreSeguro + ".txt";
            string rutaCompleta = Path.Combine(carpetaCompleta, nombreFichero);

            try
            {
                File.WriteAllText(rutaCompleta, sb.ToString(), Encoding.UTF8);
                Console.WriteLine($"  {nombreFichero}");
                creados++;
            }
            catch
            {
                errores++;
            }
        }

        // Retorna las métricas del proceso empaquetadas en una tupla
        return (creados, errores, carpetaCompleta);
    }

    private static string NombreSeguroParaFichero(string equipo)
    {
        string limpio = "";
        foreach (char c in equipo)
        {
            // Sustituye símbolos conflictivos para el sistema operativo por guiones bajos
            if (char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '_')
                limpio += c;
            else
                limpio += '_';
        }
        limpio = limpio.Trim();
        if (string.IsNullOrWhiteSpace(limpio))
            limpio = "equipo_desconocido";
        return limpio;
    }
}