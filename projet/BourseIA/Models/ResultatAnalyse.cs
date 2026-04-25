using System.ComponentModel.DataAnnotations;

namespace BourseIA.Models;

public class ResultatAnalyse
{
    public int Id { get; set; }

    public string Tendance { get; set; } = "Inconnue";

    public double? PrixMin { get; set; }
    public double? PrixMax { get; set; }
    public double? PrixMoyen { get; set; }
    public double? EcartType { get; set; }

    public string? PointsCles { get; set; }

    public string? RapportJson { get; set; }

    public string StatutAnalyse { get; set; } = "EnAttente";

    public DateTime DateAnalyse { get; set; } = DateTime.UtcNow;

    public string? MessageErreur { get; set; }

    public int CourbeBoursiereId { get; set; }
    public CourbeBoursiere CourbeBoursiere { get; set; } = null!;
}
