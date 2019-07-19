using System.Windows.Media;
using ImageConverter.Properties;

namespace ImageConverter
{
    class ThemeManager
    {
        static Color LightTheme= Color.FromArgb(255, 255, 255, 255);
        static Color DarkTheme = Color.FromArgb(255, 29, 29, 29);

        static Color defaultThemeColor = Color.FromArgb(255, 218, 166, 25); //colore quando il mouse NON è sopra il pulsante
        static Color defaultThemeHoveringColor = Color.FromArgb(255, 162, 123, 17); //colore quando il mouse è sopra il pulsante

        static Color GreenThemeColor = Color.FromArgb(255, 119, 227, 57);
        static Color GreenThemeHoveringColor = Color.FromArgb(255, 107, 201, 52);

        static Color RedThemeColor = Color.FromArgb(255, 255, 0, 0);
        static Color RedThemeHoveringColor = Color.FromArgb(255, 170, 0, 0);

        static Color VioletThemeColor = Color.FromArgb(255, 238, 130, 238);
        static Color VioletThemeHoveringColor = Color.FromArgb(255, 207, 107, 207);

        public static SolidColorBrush solidColorBrush;


        public static SolidColorBrush SelectedThemeType()
        {
            solidColorBrush = new SolidColorBrush();

            if (Settings.Default.ThemeType == "LightTheme")
            { 
                solidColorBrush.Color = LightTheme;
                return solidColorBrush;
            }
            else if (Settings.Default.ThemeType == "DarkTheme")
            {
                solidColorBrush.Color = DarkTheme;
                return solidColorBrush;
            }
            return solidColorBrush;
        }

        public static SolidColorBrush SelectedThemeColor()
        {
            solidColorBrush = new SolidColorBrush();

            if (Settings.Default.ThemeCol == "DefaultTheme")
            {
                solidColorBrush.Color = defaultThemeColor;
            }
            else if (Settings.Default.ThemeCol == "GreenTheme")
            {
                solidColorBrush.Color = GreenThemeColor;
            }
            else if (Settings.Default.ThemeCol == "RedTheme")
            {
                solidColorBrush.Color = RedThemeColor;
            }
            else if (Settings.Default.ThemeCol == "VioletTheme")
            {
                solidColorBrush.Color = VioletThemeColor;
            }
            return solidColorBrush;
        }

        public static SolidColorBrush SelectedThemeHoveringColor()
        {
            solidColorBrush = new SolidColorBrush();

            if (Settings.Default.ThemeCol == "DefaultTheme")
            {
                solidColorBrush.Color = defaultThemeHoveringColor;
            }
            else if (Settings.Default.ThemeCol == "GreenTheme")
            {
                solidColorBrush.Color = GreenThemeHoveringColor;
            }
            else if (Settings.Default.ThemeCol == "RedTheme")
            {
                solidColorBrush.Color = RedThemeHoveringColor;
            }
            else if (Settings.Default.ThemeCol == "VioletTheme")
            {
                solidColorBrush.Color = VioletThemeHoveringColor;
            }
            return solidColorBrush;
        }
    }
}
