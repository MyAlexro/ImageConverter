using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ImageConverter.Properties;


namespace ImageConverter
{
    /// <summary>
    /// Interaction logic for MenuButton.xaml
    /// </summary>
    public partial class MenuButton : UserControl
    {

        public MenuButton()
        {
            InitializeComponent();
            BackgroundRect1.Fill = ThemeManager.SelectedThemeType();
            BackgroundRect2.Fill = ThemeManager.SelectedThemeType();
            rect1.Fill = ThemeManager.SelectedThemeColor();
            rect2.Fill = ThemeManager.SelectedThemeColor();
            rect3.Fill = ThemeManager.SelectedThemeColor(); //applica il colore del tema selezionato quando il bttn è inizializzato
        }

        private void StackPanel_MouseEnter(object sender, MouseEventArgs e) //se la freccetta del mouse va sopra al bttn lo scurisce
        {
            rect1.Fill = ThemeManager.SelectedThemeHoveringColor();
            rect2.Fill = ThemeManager.SelectedThemeHoveringColor();
            rect3.Fill = ThemeManager.SelectedThemeHoveringColor();
        }

        private void StackPanel_MouseLeave(object sender, MouseEventArgs e) //se invece se ne va gli mette il colore normale
        {
            rect1.Fill = ThemeManager.SelectedThemeColor();
            rect2.Fill = ThemeManager.SelectedThemeColor();
            rect3.Fill = ThemeManager.SelectedThemeColor();
        }
    }
}
