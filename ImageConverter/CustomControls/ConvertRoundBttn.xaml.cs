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
            BttnBackground.Fill = ThemeManager.SelectedFontColor(); //apply the chosen theme when the button gets initialized
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
            BttnBackground.Fill = ThemeManager.SelectedFontHoveringColor(); //if the mouse gets over the bttn darken it
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e) //otherwise set the normal colour
        {
            BttnBackground.Fill = ThemeManager.SelectedFontColor();
        }
    }
}
