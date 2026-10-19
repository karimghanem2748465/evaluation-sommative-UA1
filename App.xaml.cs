using System.Globalization;
using System.Windows;
using System.Windows.Markup;
namespace GestCoop;
public partial class App : Application
{
 protected override void OnStartup(StartupEventArgs e)
 {
  CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-CA");
  CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("fr-CA");
  FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage("fr-CA")));
  base.OnStartup(e);
 }
}
