namespace BourseIA.Utils;

/// <summary>
/// Utilitaires pour la validation et le prétraitement des images de courbes boursières.
/// L'analyse réelle sera déléguée à l'API IA externe.
/// </summary>
public static class ImageProcessingHelper
{
    private static readonly string[] ExtensionsAutorisees = [".png", ".jpg", ".jpeg"];
    private const long TailleMaxOctets = 10 * 1024 * 1024;

    public static bool EstImageValide(IFormFile fichier)
    {
        if (fichier is null || fichier.Length == 0) return false;
        if (fichier.Length > TailleMaxOctets) return false;

        var ext = Path.GetExtension(fichier.FileName).ToLowerInvariant();
        if (!ExtensionsAutorisees.Contains(ext)) return false;

        return VerifierSignatureFichier(fichier);
    }

    public static string GenererNomFichier(string extension)
        => $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}{extension}";

    public static async Task<string> SauvegarderImageAsync(IFormFile fichier, string dossierBase, int userId)
    {
        var ext = Path.GetExtension(fichier.FileName).ToLowerInvariant();
        var sousDossier = Path.Combine(dossierBase, "courbes", userId.ToString());
        Directory.CreateDirectory(sousDossier);

        var nomFichier = GenererNomFichier(ext);
        var cheminComplet = Path.Combine(sousDossier, nomFichier);

        await using var stream = new FileStream(cheminComplet, FileMode.Create);
        await fichier.CopyToAsync(stream);

        return $"/courbes/{userId}/{nomFichier}";
    }

    public static void SupprimerImage(string cheminRelatif, string dossierBase)
    {
        var cheminComplet = Path.Combine(dossierBase, cheminRelatif.TrimStart('/'));
        if (File.Exists(cheminComplet))
            File.Delete(cheminComplet);
    }

    private static bool VerifierSignatureFichier(IFormFile fichier)
    {
        try
        {
            using var reader = new BinaryReader(fichier.OpenReadStream());
            var octets = reader.ReadBytes(4);

            bool estPng = octets.Length >= 4 &&
                          octets[0] == 0x89 && octets[1] == 0x50 &&
                          octets[2] == 0x4E && octets[3] == 0x47;

            bool estJpeg = octets.Length >= 3 &&
                           octets[0] == 0xFF && octets[1] == 0xD8 && octets[2] == 0xFF;

            return estPng || estJpeg;
        }
        catch
        {
            return false;
        }
    }
}
