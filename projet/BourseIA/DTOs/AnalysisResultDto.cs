namespace BourseIA.DTOs;

public class AnalysisResultDto
{
    public int Id { get; set; }
    public string Tendance { get; set; } = string.Empty;
    public double? PrixMin { get; set; }
    public double? PrixMax { get; set; }
    public double? PrixMoyen { get; set; }
    public double? EcartType { get; set; }
    public string? PointsCles { get; set; }
    public string? RapportJson { get; set; }
    public string StatutAnalyse { get; set; } = string.Empty;
    public DateTime DateAnalyse { get; set; }
    public string? MessageErreur { get; set; }
    public int CourbeBoursiereId { get; set; }
}

public class AnalyseRequestDto
{
    public int CourbeId { get; set; }
}

public class StatistiquesDto
{
    public int TotalAnalyses { get; set; }
    public int AnalysesEnHausse { get; set; }
    public int AnalysesEnBaisse { get; set; }
    public int AnalysesStables { get; set; }
    public double? PrixMoyenGlobal { get; set; }
    public List<AnalysisResultDto> DernieresAnalyses { get; set; } = new();
}
