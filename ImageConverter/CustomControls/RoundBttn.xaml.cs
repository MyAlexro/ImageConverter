using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ImageConverter.Properties;


namespace ImageConverter
{
    /// <summary>
    /// Interaction logic for RoundButton.xaml
    /// </summary>
    public partial class RoundBttn : UserControl
    {


        public string ButtonText
        {
            get { return (string)GetValue(ButtonTextProperty); }
            set { SetValue(ButtonTextProperty, value); }
        }

        public static readonly DependencyProperty ButtonTextProperty =
            DependencyProperty.Register("ButtonText", typeof(string), typeof(RoundBttn), new PropertyMetadata("Text"));

        public RoundBttn()
        {
            InitializeComponent();
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
