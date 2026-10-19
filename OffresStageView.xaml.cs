using System.IO;
using System.Windows;
using System.Windows.Controls;
using GestCoop.Models;
using GestCoop.Services;
namespace GestCoop.Views;
public partial class OffresStageView : Window
{
 List<Offre> offres = new List<Offre>();
 Guid? idOffreModifiee;
 bool fenetrePrete;
 bool fichierDisponible = true;
 int page = 1;
 const int OffresParPage = 5;
 readonly string[] statuts = ["Ouvert", "En révision", "Expiré"];
 public OffresStageView()
 {
  InitializeComponent();
  Entreprise.ItemsSource = new[] { "Ubisoft Montréal", "CAE inc.", "Bell Canada", "Desjardins", "Hydro-Québec" };
  TypeStage.ItemsSource = new[] { "Stage coopératif", "Stage d'été", "Stage de fin d'études" };
  Statut.ItemsSource = statuts;
  Filtre.ItemsSource = new[] { "Tous les statuts" }.Concat(statuts);
  Filtre.SelectedIndex = 0;
  try { offres = OffreStore.Load(); }
  catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException or InvalidDataException)
  {
   fichierDisponible = false;
   Notification.Text = "Chargement impossible. Le fichier existant est conservé. Corrigez-le puis relancez : " + OffreStore.FilePath;
   MessageBox.Show(this, "Impossible de lire les offres : " + ex.Message + "\nLa consultation reste possible ; les modifications sont désactivées pour protéger vos données.", "Sauvegarde", MessageBoxButton.OK, MessageBoxImage.Warning);
  }
  // Un seul gestionnaire réévalue les champs à chaque saisie.
  AddHandler(TextBox.TextChangedEvent, new TextChangedEventHandler(Texte_Modifie));
  AddHandler(ComboBox.SelectionChangedEvent, new SelectionChangedEventHandler(Choix_Modifie));
  foreach (CheckBox check in Programmes.Children) { check.Checked += Programme_Modifie; check.Unchecked += Programme_Modifie; }
  Debut.SelectedDateChanged += Choix_Modifie;
  Fin.SelectedDateChanged += Choix_Modifie;
  Debut.DateValidationError += Date_Invalide;
  Fin.DateValidationError += Date_Invalide;
  fenetrePrete = true;
  ViderFormulaire();
  AfficherOffres();
 }
 List<string> LireProgrammes()
 {
  List<string> selection = new List<string>();
  foreach (CheckBox caseProgramme in Programmes.Children)
  {
   if (caseProgramme.IsChecked == true)
   {
    selection.Add(caseProgramme.Content.ToString()!);
   }
  }
  return selection;
 }
 string VerifierChamps()
 {
  if (!fichierDisponible) return "Sauvegarde inaccessible : modifications désactivées.";
  if (string.IsNullOrWhiteSpace(Entreprise.Text) || string.IsNullOrWhiteSpace(Poste.Text) || string.IsNullOrWhiteSpace(Lieu.Text) || string.IsNullOrWhiteSpace(Description.Text) || LireProgrammes().Count == 0 || TypeStage.SelectedItem == null || Statut.SelectedItem == null) return "Remplissez tous les champs obligatoires (*) et choisissez au moins un programme.";
  if (!int.TryParse(Nombre.Text, out int n) || n < 1 || n > 999) return "Le nombre de postes doit être un entier entre 1 et 999.";
  if (!DateTime.TryParse(Debut.Text, out var start) || !DateTime.TryParse(Fin.Text, out var end)) return "Saisissez deux dates valides ou utilisez les calendriers.";
  if (start.Date > end.Date) return "La date de début doit être antérieure ou égale à la date de fin.";
  return "";
 }
 void VerifierFormulaire()
 {
  if (!fenetrePrete) return;
  Compteur.Text = $"{Description.Text.Length} / 1000";
  Erreur.Text = VerifierChamps();
  Publier.IsEnabled = Erreur.Text.Length == 0;
 }
 void Texte_Modifie(object sender, TextChangedEventArgs e) => VerifierFormulaire();
 void Choix_Modifie(object? sender, SelectionChangedEventArgs e) => VerifierFormulaire();
 void Programme_Modifie(object sender, RoutedEventArgs e) => VerifierFormulaire();
 void Date_Invalide(object? sender, DatePickerDateValidationErrorEventArgs e) { e.ThrowException = false; VerifierFormulaire(); }
 void ViderFormulaire()
 {
  idOffreModifiee = null;
  Entreprise.SelectedIndex = -1; Entreprise.Text = "";
  Poste.Clear(); Lieu.Clear(); Description.Clear(); Nombre.Text = "1";
  foreach (CheckBox c in Programmes.Children) c.IsChecked = false;
  Debut.SelectedDate = null; Debut.Text = ""; Fin.SelectedDate = null; Fin.Text = "";
  TypeStage.SelectedIndex = 0; Statut.SelectedIndex = 0;
  FormTitle.Text = "⊞  Nouvelle offre de stage"; Publier.Content = "➤  Publier l'offre";
  VerifierFormulaire();
 }
 void Reinitialiser_Click(object sender, RoutedEventArgs e) => ViderFormulaire();
 // On ne met à jour l'écran qu'après une sauvegarde réussie.
 bool SauvegarderListe(List<Offre> nouvelleListe)
 {
  try { OffreStore.Save(nouvelleListe); offres = nouvelleListe; return true; }
  catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
  { MessageBox.Show(this, "Enregistrement impossible : " + ex.Message, "Sauvegarde", MessageBoxButton.OK, MessageBoxImage.Error); return false; }
 }
 void Publier_Click(object sender, RoutedEventArgs e)
 {
  VerifierFormulaire(); if (!Publier.IsEnabled) return;
  var offre = new Offre { Id = idOffreModifiee ?? Guid.NewGuid(), Entreprise = Entreprise.Text.Trim(), Poste = Poste.Text.Trim(), Programmes = LireProgrammes(), Lieu = Lieu.Text.Trim(), Description = Description.Text.Trim(), Type = TypeStage.SelectedItem.ToString()!, Debut = DateTime.Parse(Debut.Text).Date, Fin = DateTime.Parse(Fin.Text).Date, Postes = int.Parse(Nombre.Text), Statut = Statut.SelectedItem.ToString()! };
  var nouvelleListe = offres.ToList();
  int index = nouvelleListe.FindIndex(o => o.Id == offre.Id);
  if (index >= 0) nouvelleListe[index] = offre; else nouvelleListe.Insert(0, offre);
  if (!SauvegarderListe(nouvelleListe)) return;
  Notification.Text = index >= 0 ? "L'offre a été modifiée." : "L'offre a été publiée.";
  Recherche.Clear(); Filtre.SelectedIndex = 0; page = 1;
  ViderFormulaire(); AfficherOffres();
 }
 void Modifier_Click(object sender, RoutedEventArgs e)
 {
  if (!fichierDisponible) return;
  var o = (Offre)((Button)sender).Tag;
  idOffreModifiee = o.Id; Entreprise.Text = o.Entreprise; Poste.Text = o.Poste; Lieu.Text = o.Lieu; Description.Text = o.Description; Nombre.Text = o.Postes.ToString();
  foreach (CheckBox c in Programmes.Children) c.IsChecked = o.Programmes.Contains(c.Content.ToString()!);
  Debut.SelectedDate = o.Debut; Fin.SelectedDate = o.Fin; TypeStage.SelectedItem = o.Type; Statut.SelectedItem = o.Statut;
  FormTitle.Text = "✎  Modifier l'offre de stage"; Publier.Content = "Enregistrer les modifications";
  VerifierFormulaire(); Poste.BringIntoView(); Poste.Focus();
 }
 void Supprimer_Click(object sender, RoutedEventArgs e)
 {
  if (!fichierDisponible) return;
  var o = (Offre)((Button)sender).Tag;
  if (MessageBox.Show(this, $"Supprimer l'offre « {o.Poste} » de {o.Entreprise} ?", "Confirmer la suppression", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes) return;
  if (!SauvegarderListe(offres.Where(x => x.Id != o.Id).ToList())) return;
  if (idOffreModifiee == o.Id) ViderFormulaire();
  Notification.Text = "L'offre a été supprimée."; AfficherOffres();
 }
 void AfficherOffres()
 {
  if (!fenetrePrete) return;
  string recherche = Recherche.Text.Trim();
  var resultats = offres.Where(o => (Filtre.SelectedIndex <= 0 || o.Statut == Filtre.SelectedItem?.ToString()) && $"{o.Entreprise} {o.Poste} {o.Programme} {o.Lieu}".Contains(recherche, StringComparison.CurrentCultureIgnoreCase)).ToList();
  int pages = Math.Max(1, (resultats.Count + OffresParPage - 1) / OffresParPage); page = Math.Clamp(page, 1, pages);
  var offresPage = resultats.Skip((page - 1) * OffresParPage).Take(OffresParPage).ToList();
  Table.ItemsSource = offresPage;
  Total.Text = $"{offresPage.Count} offre(s) affichée(s) • {resultats.Count} résultat(s) sur {offres.Count}";
  PageLabel.Text = $"{page} / {pages}";
 }
 void Recherche_Modifiee(object sender, TextChangedEventArgs e) { page = 1; AfficherOffres(); }
 void Filtre_Modifie(object sender, SelectionChangedEventArgs e) { page = 1; AfficherOffres(); }
 void Precedent_Click(object sender, RoutedEventArgs e) { page--; AfficherOffres(); }
 void Suivant_Click(object sender, RoutedEventArgs e) { page++; AfficherOffres(); }
 void Offres_Click(object sender, RoutedEventArgs e) { FormTitle.BringIntoView(); }
 void Navigation_Click(object sender, RoutedEventArgs e) => MessageBox.Show(this, $"Le module « {((Button)sender).Tag} » n'est pas inclus dans cet écran de gestion des offres.", "GestCoop", MessageBoxButton.OK, MessageBoxImage.Information);
}


