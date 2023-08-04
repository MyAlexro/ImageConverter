using System.Windows.Controls;
using System.Windows.Input;


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
            rect1.Fill = ThemeManager.SelectedFontColor();
            rect2.Fill = ThemeManager.SelectedFontColor();
            rect3.Fill = ThemeManager.SelectedFontColor(); //applica il colore del tema selezionato quando il bttn è inizializzato
        }

        private void StackPanel_MouseEnter(object sender, MouseEventArgs e) //se la freccetta del mouse va sopra al bttn lo scurisce
        {
            rect1.Fill = ThemeManager.SelectedFontHoveringColor();
            rect2.Fill = ThemeManager.SelectedFontHoveringColor();
            rect3.Fill = ThemeManager.SelectedFontHoveringColor();
        }

        private void StackPanel_MouseLeave(object sender, MouseEventArgs e) //se invece se ne va gli mette il colore normale
        {
            rect1.Fill = ThemeManager.SelectedFontColor();
            rect2.Fill = ThemeManager.SelectedFontColor();
            rect3.Fill = ThemeManager.SelectedFontColor();
        }
    }
}
