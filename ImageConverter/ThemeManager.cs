using System.Windows.Media;
using ImageConverter.Properties;

namespace ImageConverter
{
    class ThemeManager
    {
        static Color LightTheme = Color.FromArgb(255, 255, 255, 255);
        static Color DarkTheme = Color.FromArgb(255, 29, 29, 29);

        static Color defaultFontColor = Color.FromArgb(255, 218, 166, 25); //colore quando il mouse NON è sopra il pulsante
        static Color defaultFontHoveringColor = Color.FromArgb(255, 162, 123, 17); //colore quando il mouse è sopra il pulsante

        static Color GreenFontColor = Color.FromArgb(255, 119, 227, 57);
        static Color GreenFontHoveringColor = Color.FromArgb(255, 107, 201, 52);

        static Color RedFontColor = Color.FromArgb(255, 255, 0, 0);
        static Color RedFontHoveringColor = Color.FromArgb(255, 170, 0, 0);

        static Color VioletFontColor = Color.FromArgb(255, 238, 130, 238);
        static Color VioletFontHoveringColor = Color.FromArgb(255, 207, 107, 207);

        static Color WhiteFontColor = Color.FromArgb(255, 255, 255, 255); //colore quando il mouse NON è sopra il pulsante
        static Color WhiteFontHoveringColor = Color.FromArgb(255, 214, 214, 214); //colore quando il mouse è sopra il pulsante

        public static SolidColorBrush solidColorBrush;


        public static SolidColorBrush SelectedThemeType() //TEMA DELL'APPLICAZIONE (LIGHT,DARK)
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

        public static SolidColorBrush SelectedFontColor()
        {
            solidColorBrush = new SolidColorBrush();

            if (Settings.Default.FontCol == "DefaultFontCol")
            {
                solidColorBrush.Color = defaultFontColor;
            }
            else if (Settings.Default.FontCol == "GreenFontCol")
            {
                solidColorBrush.Color = GreenFontColor;
            }
            else if (Settings.Default.FontCol == "RedFontCol")
            {
                solidColorBrush.Color = RedFontColor;
            }
            else if (Settings.Default.FontCol == "VioletFontCol")
            {
                solidColorBrush.Color = VioletFontColor;
            }
            else if (Settings.Default.FontCol == "WhiteFontCol") //se il Font type è dark le scritte saranno bianche perchè se no non si legge
            {
                solidColorBrush.Color = WhiteFontColor;
            }
            return solidColorBrush;
        } //COLORE DEL FONT (DEFAULT, ROSSO, VERDE, LILLA)

        public static SolidColorBrush SelectedFontHoveringColor()
        {
            solidColorBrush = new SolidColorBrush();

            if (Settings.Default.FontCol == "DefaultFontCol")
            {
                solidColorBrush.Color = defaultFontHoveringColor;
            }
            else if (Settings.Default.FontCol == "GreenFontCol")
            {
                solidColorBrush.Color = GreenFontHoveringColor;
            }
            else if (Settings.Default.FontCol == "RedFontCol")
            {
                solidColorBrush.Color = RedFontHoveringColor;
            }
            else if (Settings.Default.FontCol == "VioletFontCol")
            {
                solidColorBrush.Color = VioletFontHoveringColor;
            }
            else if (Settings.Default.FontCol == "WhiteFontCol") //se il Font type è dark le scritte saranno bianche perchè se no non si legge
            {
                solidColorBrush.Color = WhiteFontHoveringColor;
            }
            return solidColorBrush;
        }//COLORE DEL FONT QUANDO C'E' SOPRA LA FRECCETTA DEL MOUSE
    }
}
