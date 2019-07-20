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
            BttnBackground.Fill = ThemeManager.SelectedFontColor(); //applica il tema quando il bttn viene inizializzato
            if (Settings.Default.Language == "it")
            {
                ConvertLabel.Content = LanguageManager.IT_ConvertLabelTxt;
            }
            else if (Settings.Default.Language == "en")
            {
                ConvertLabel.Content = LanguageManager.EN_ConvertLabelTxt;
            }
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            BttnBackground.Fill = ThemeManager.SelectedFontHoveringColor(); //se la freccetta del mouse va sopra al bttn lo scurisce
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e) //se invece se ne va gli mette il colore normale
        {
            BttnBackground.Fill = ThemeManager.SelectedFontColor();
        }
    }
}
