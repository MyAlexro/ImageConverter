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
            rect3.Fill = ThemeManager.SelectedFontColor(); //applies the color of the selected theme
        }

        private void StackPanel_MouseEnter(object sender, MouseEventArgs e) //if the mouse goes over the button, the button gets darkes
        {
            rect1.Fill = ThemeManager.SelectedFontHoveringColor();
            rect2.Fill = ThemeManager.SelectedFontHoveringColor();
            rect3.Fill = ThemeManager.SelectedFontHoveringColor();
        }

        private void StackPanel_MouseLeave(object sender, MouseEventArgs e) //if it leaves the button it returns to the same color
        {
            rect1.Fill = ThemeManager.SelectedFontColor();
            rect2.Fill = ThemeManager.SelectedFontColor();
            rect3.Fill = ThemeManager.SelectedFontColor();
        }
    }
}
