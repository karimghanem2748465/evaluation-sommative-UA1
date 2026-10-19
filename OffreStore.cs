using System.IO;
using System.Text.Json;
using GestCoop.Models;
namespace GestCoop.Services;
public static class OffreStore
{
 public static string FilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GestCoop", "offres.json");
 public static List<Offre> Load() => File.Exists(FilePath) ? JsonSerializer.Deserialize<List<Offre>>(File.ReadAllText(FilePath)) ?? throw new InvalidDataException("Le fichier est vide ou invalide.") : Examples();
 // Écriture temporaire puis remplacement pour éviter un fichier partiellement écrit.
 public static void Save(List<Offre> offres)
 {
  Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
  File.WriteAllText(FilePath + ".tmp", JsonSerializer.Serialize(offres, new JsonSerializerOptions { WriteIndented = true }));
  File.Move(FilePath + ".tmp", FilePath, true);
 }
 public static List<Offre> Examples() => [
  Example("Ubisoft Montréal", "Développeur junior", "Informatique", "Montréal, QC", "2025-09-02", "2026-04-24", "Ouvert"),
  Example("CAE inc.", "Analyste de données", "Informatique", "Ottawa, ON", "2025-09-02", "2026-04-24", "Ouvert"),
  Example("Bell Canada", "Intégrateur réseau", "Informatique", "Toronto, ON", "2025-01-06", "2025-04-18", "En révision"),
  Example("Desjardins", "Développeur C#", "Informatique", "Lévis, QC", "2024-09-03", "2024-12-20", "Expiré"),
  Example("Hydro-Québec", "Technicien TI", "Réseautique", "Montréal, QC (hybride)", "2025-09-02", "2026-04-24", "Ouvert")];
 static Offre Example(string entreprise, string poste, string programme, string lieu, string debut, string fin, string statut) => new() { Entreprise = entreprise, Poste = poste, Programmes = [programme], Lieu = lieu, Description = "Participez aux projets de l'équipe et développez vos compétences dans le cadre de votre stage.", Debut = DateTime.Parse(debut, System.Globalization.CultureInfo.InvariantCulture), Fin = DateTime.Parse(fin, System.Globalization.CultureInfo.InvariantCulture), Statut = statut };
}
