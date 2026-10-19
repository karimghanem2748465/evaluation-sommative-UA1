namespace GestCoop.Models;
public class Offre
{
 public Guid Id { get; set; } = Guid.NewGuid();
 public string Entreprise { get; set; } = "";
 public string Poste { get; set; } = "";
 public List<string> Programmes { get; set; } = [];
 public string Programme => string.Join(", ", Programmes);
 public string Lieu { get; set; } = "";
 public string Description { get; set; } = "";
 public string Type { get; set; } = "Stage coopératif";
 public DateTime Debut { get; set; }
 public DateTime Fin { get; set; }
 public int Postes { get; set; } = 1;
 public string Statut { get; set; } = "Ouvert";
}
