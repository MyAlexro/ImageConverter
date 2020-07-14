using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImageConverter
{
    /// <summary>
    /// Interaction logic for RoundButton.xaml
    /// </summary>
    public partial class RoundBttn : UserControl
    {
        public new int FontSize
        {
            get { return (int)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FontSize.  This enables animation, styling, binding, etc...
        public static new readonly DependencyProperty FontSizeProperty = DependencyProperty.Register("FontSize", typeof(int), typeof(RoundBttn), new PropertyMetadata(12));


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
            BttnBackground.Fill = ThemeManager.SolidColorBrushOfSelectedThemeColor();
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            BttnBackground.Fill = ThemeManager.SelectedThemeHoveringColor(); //If the mouse gets over the bttn darken it
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e) //Otherwise set the normal colour
        {
            BttnBackground.Fill = ThemeManager.SolidColorBrushOfSelectedThemeColor();
        }
    }
}
