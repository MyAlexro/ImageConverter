using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


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
