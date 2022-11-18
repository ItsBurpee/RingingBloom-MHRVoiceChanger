using MHRVoiceChanger;
using System.Windows;

namespace RingingBloom.Windows
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        public OptionsWindow(Options options)
        {
            InitializeComponent();
            DefaultImport.Text = options.defaultImport;
            DefaultExport.Text = options.defaultExport;
            DefaultGame.SelectedIndex = (int)options.defaultGame;

        }
        public void Confirm(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
