using System.Windows.Media;
using ImageConverter.Properties;

namespace ImageConverter
{
    class ThemeManager
    {
        //Background theme mode
        static Color LightTheme = Color.FromArgb(255, 255, 255, 255);
        static Color DarkTheme = Color.FromArgb(255, 37, 37, 38);

        //Theme colors
        static Color defaultThemeColor = Color.FromArgb(255, 218, 166, 25); //the color of the font  when the mouse ISN'T over the control
        static Color defaultThemeHoveringColor = Color.FromArgb(255, 162, 123, 17); //the color of the font when the mouse IS over the control

        static Color greenThemeColor = Color.FromArgb(255, 119, 227, 57);
        static Color greenThemeHoveringColor = Color.FromArgb(255, 107, 201, 52);

        static Color redThemeColor = Color.FromArgb(255, 255, 0, 0);
        static Color redThemeHoveringColor = Color.FromArgb(255, 170, 0, 0);

        static Color violetThemeColor = Color.FromArgb(255, 238, 130, 238);
        static Color violetThemeHoveringColor = Color.FromArgb(255, 207, 107, 207);

        static Color whiteThemeColor = Color.FromArgb(255, 200, 200, 200);
        static Color whiteThemeHoveringColor = Color.FromArgb(255, 175, 175, 175);


        //colors of the label, in MainWindow, which tells if the conversion is ongoing, ended successfully or ended with errors 
        public static Color RunningOrStaticConversionTextBlockColor = Color.FromArgb(255, 0, 0, 0);
        public static Color CompletedConversionTextBlockColor = Color.FromArgb(255, 53, 181, 87);
        public static Color CompletedWithErrorsConversionTextBlockColor = Color.FromArgb(255, 235, 64, 52);

        public static SolidColorBrush solidColorBrush;


        public static SolidColorBrush SelectedThemeType() //THEME OF THE APPLICATION (LIGHT,DARK)
        {
            solidColorBrush = new SolidColorBrush();

            if (Settings.Default.ThemeMode == "LightTheme")
            {
                solidColorBrush.Color = LightTheme;
                return solidColorBrush;
            }
            else if (Settings.Default.ThemeMode == "DarkTheme")
            {
                solidColorBrush.Color = DarkTheme;
                return solidColorBrush;
            }
            return solidColorBrush;
        }

        public static SolidColorBrush SelectedThemeColor()
        {
            solidColorBrush = new SolidColorBrush();

            if (Settings.Default.ThemeColor == "DefaultThemeColor")
            {
                solidColorBrush.Color = defaultThemeColor;
            }
            else if (Settings.Default.ThemeColor == "GreenThemeColor")
            {
                solidColorBrush.Color = greenThemeColor;
            }
            else if (Settings.Default.ThemeColor == "RedThemeColor")
            {
                solidColorBrush.Color = redThemeColor;
            }
            else if (Settings.Default.ThemeColor == "VioletThemeColor")
            {
                solidColorBrush.Color = violetThemeColor;
            }
            else if (Settings.Default.ThemeColor == "WhiteThemeColor") //if the Theme is dark the theme color will be white
            {
                solidColorBrush.Color = whiteThemeColor;
            }
            return solidColorBrush;
        } //COLORS OF THE FONT: DEFAULT, RED, GREEN, VIOLET, WHITE(for the dark theme)

        public static SolidColorBrush SelectedThemeHoveringColor()
        {
            solidColorBrush = new SolidColorBrush();

            if (Settings.Default.ThemeColor == "DefaultThemeColor")
            {
                solidColorBrush.Color = defaultThemeHoveringColor;
            }
            else if (Settings.Default.ThemeColor == "GreenThemeColor")
            {
                solidColorBrush.Color = greenThemeHoveringColor;
            }
            else if (Settings.Default.ThemeColor == "RedThemeColor")
            {
                solidColorBrush.Color = redThemeHoveringColor;
            }
            else if (Settings.Default.ThemeColor == "VioletThemeColor")
            {
                solidColorBrush.Color = violetThemeHoveringColor;
            }
            else if (Settings.Default.ThemeColor == "WhiteThemeColor") //if the Theme is dark the fontCol will be white
            {
                solidColorBrush.Color = whiteThemeHoveringColor;
            }
            return solidColorBrush;
        }//COLORs OF THE FONT WHEN THE MOUSE IS OVER IT
    }
}
