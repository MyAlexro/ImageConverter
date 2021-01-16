using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using ImageConverter.HelperClasses;
using ImageConverter.Properties;

namespace ImageConverter
{
    class ThemeManager
    {
        /// <summary>
        /// Available theme colors
        /// </summary>
        public static readonly List<string> ThemeColors = new List<string>() { "DefaultThemeColor", "GreenThemeColor", "RedThemeColor", "VioletThemeColor", "WhiteThemeColor" };
        /// <summary>
        /// Available theme modes
        /// </summary>
        public static readonly List<string> ThemeModes = new List<string>() { "LightTheme", "DarkTheme" };


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

        static Color whiteThemeColor = Color.FromArgb(255, 232, 232, 232); 
        static Color whiteThemeHoveringColor = Color.FromArgb(255, 200, 200, 200);

        //colors of the label, in MainWindow, which tells if the conversion is ongoing, ended successfully or ended with errors 
        public static Color RunningOrStaticConversionTextBlockColor = Color.FromArgb(255, 0, 0, 0);
        public static Color CompletedConversionTextBlockColor = Color.FromArgb(255, 53, 181, 87);
        public static Color CompletedWithErrorsConversionTextBlockColor = Color.FromArgb(255, 235, 64, 52);

        public static SolidColorBrush solidColorBrush;

        /// <summary>
        /// Return the current theme mode as a SolidColorBrush.
        /// <para> Returns a SolidColorBrush, its color is white if the theme mode is LightTheme, or the ThemeManager.DarkTheme color if the selected theme mode is "DarkTheme" </para>
        /// </summary>
        /// <returns> SolidColorBrush, its color is white if the theme mode is LightTheme, or the ThemeManager.DarkTheme color if the selected theme mode is "DarkTheme" </returns>
        public static SolidColorBrush SolidColorBrushOfThemeMode() //THEMES OF THE APPLICATION (LIGHT,DARK)
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

        /// <summary>
        /// Returns the current theme mode color as a SolidColorBrush
        /// <para>Returns a SolidColorBrush with the corresponding color, depending on the selected theme color</para>
        /// <para>Colors: Default(Yellowish), red, green, violet, white</para>
        /// <para>Reminder: Only if the ThemeMode is DarkTheme, the ThemeColor will be applied to the text of labels and textblocks</para>
        /// </summary>
        /// <returns>Returns a SolidColorBrush with the corresponding color, depending on the selected theme color</returns>
        public static SolidColorBrush SolidColorBrushOfThemeColor()
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
            else if (Settings.Default.ThemeColor == "WhiteThemeColor") //reminder: if the Theme is dark the theme color of the labels and textblocks will be white
            {
                solidColorBrush.Color = whiteThemeColor;
            }
            return solidColorBrush;
        }

        /// <summary>
        /// Returns the color that controls should be when the mouse is hovering them
        /// <para>Returns a SolidColorBrush with the corresponding color, depending on the selected theme color</para>
        /// </summary>
        /// <returns>Returns a SolidColorBrush with the corresponding color, depending on the selected theme color</returns>
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
        }

        /// <summary>
        /// Applies the current theme color as foreground of every Label found in the stackpanel passed by reference
        /// </summary>
        /// <param name="stackPanel"></param>
        public static void ApplyThemeColorToLabelsInSP(ref StackPanel stackPanel)
        {
            foreach (Label label in UtilityMethods.FindAllLabelsInPanelOrDerivedObjs(stackPanel))
            {
                label.Foreground = ThemeManager.SolidColorBrushOfThemeColor();
            }
        }

        /// <summary>
        /// Applies the current theme color as foreground of every TextBlock found in the stackpanel passed by reference
        /// </summary>
        /// <param name="stackPanel"></param>
        public static void ApplyThemeColorToTextBlocksInSP(ref StackPanel stackPanel)
        {
            foreach (TextBlock textblock in UtilityMethods.FindTextBlocksInStackPanel(stackPanel))
            {
                textblock.Foreground = ThemeManager.SolidColorBrushOfThemeColor();
            }
        }
    }
}
