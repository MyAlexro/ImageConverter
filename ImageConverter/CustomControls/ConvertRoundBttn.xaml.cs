using System.Windows.Controls;
using System.Windows.Input;
using ImageConverter.Properties;


namespace ImageConverter
{
    /// <summary>
    /// Interaction logic for RoundButton.xaml
    /// </summary>
    public partial class ConvertRoundBttn : UserControl
    {
        public ConvertRoundBttn()
        {
            InitializeComponent();
            Background.Fill = ThemeManager.SelectedThemeColor(); //applica il tema quando il bttn viene inizializzato
            if (Settings.Default.Language == "it")
            {
                label.Content = "Converti immagine";
            }
            else if (Settings.Default.Language == "en")
            {
                label.Content = "Convert image";
            }
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            Background.Fill = ThemeManager.SelectedThemeHoveringColor(); //se la freccetta del mouse va sopra al bttn lo scurisce
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e) //se invece se ne va gli mette il colore normale
        {
            Background.Fill = ThemeManager.SelectedThemeColor();
        }
    }
}
