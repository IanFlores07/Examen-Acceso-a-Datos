namespace GestionPartidosACB;

public class Partido
{
    // Estructura de propiedades de la entidad Partido para alojar los datos parseados del CSV
    public string Jornada { get; set; } = string.Empty;
    public int NumeroJornada { get; set; }
    public string EquipoLocal { get; set; } = string.Empty;
    public string EquipoVisitante { get; set; } = string.Empty;
    public string Resultado { get; set; } = string.Empty;
    public int PuntosLocal { get; set; }
    public int PuntosVisitante { get; set; }
    public string Mvp { get; set; } = string.Empty;
    public int PuntosMvp { get; set; }
    public int AsistenciasMvp { get; set; }
    public int RebotesMvp { get; set; }
    public string EquipoMvp { get; set; } = string.Empty;

    public bool Participa(string nombreEquipo)
    {
        // Evalúa si el equipo recibido actuó en el partido como local o como visitante
        return string.Equals(EquipoLocal, nombreEquipo, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(EquipoVisitante, nombreEquipo, StringComparison.OrdinalIgnoreCase);
    }

    public override string ToString()
    {
        // Formatea la representación estándar en una sola línea de texto para listas rápidas
        return $"{Jornada}: {EquipoLocal} {PuntosLocal} - {PuntosVisitante} {EquipoVisitante} | MVP: {Mvp} ({EquipoMvp}) - {PuntosMvp} pts, {AsistenciasMvp} ast, {RebotesMvp} reb";
    }

    public string ToStringDetallado()
    {
        // Formatea la representación multilínea expandida con alineación de campos
        return $"Jornada: {Jornada}\n" +
               $"  Equipo local:      {EquipoLocal}\n" +
               $"  Equipo visitante:  {EquipoVisitante}\n" +
               $"  Resultado:         {Resultado} ({PuntosLocal} - {PuntosVisitante})\n" +
               $"  MVP:               {Mvp} ({EquipoMvp})\n" +
               $"  Estadísticas MVP:  {PuntosMvp} pts, {AsistenciasMvp} ast, {RebotesMvp} reb";
    }
}