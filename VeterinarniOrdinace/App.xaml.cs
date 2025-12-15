using System.Configuration;
using System.Data;
using System.Windows;
using VeterinarniOrdinace.Data;

namespace VeterinarniOrdinace
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private void OnStartup(object sender, StartupEventArgs e)
        {
            DatabaseInitializer.Initialize();
            base.OnStartup(e);
        }
    }

}
