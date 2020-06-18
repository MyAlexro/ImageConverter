
using System;
using System.Collections.Generic;

namespace ImageConverter
{
    public class LanguageManager
    {
        /// <summary>
        /// Available languages
        /// </summary>
        public static readonly string[] languages = new string[2] { "it", "en" };

        //--ITALIAN--
        public static readonly string
         //Conversion options
         IT_GifFramesDelayTimeLabelText = "Tempo di passaggio da un frame all'altro:",
         IT_AddToExistingImagesToolTip = "Aggiungi le immagini trascinate a quelle già presenti",
         IT_ReplaceExistingImagesToolTip = "Sostituisci le immagini presenti con quelle trascinate",
         IT_StartConversionBttnText = "Converti immagini",
         IT_ChooseFormatLabelText = "Scegliere il formato in \ncui convertire l'immagine:",
         IT_ImageViewerContextMenuText = "Svuota",
         IT_CompressionAlgoLabelText = "Algortimo di compressione:",
         IT_NoneAlgoLabelText = "Nessuno",
         IT_ReplacePngTransparencyLabelText = "Sostituisci la trasparenza della png con:",
         IT_GifRepeatTimesLabelText = "Volte che la gif si ripeterà:",
         IT_QualityLabelText = "Qualità:",
         IT_Nothing = "Niente",
         IT_White = "Bianco",
         IT_Black = "Nero",

         //Settings menu
         IT_SettingsLabelText = "Impostazioni",
         IT_ThemeColorLabelText = "Colore tema:",
         IT_ThemeLabelText = "Tema:",
         IT_LanguageLabelText = "Lingua:",
         IT_CreditsLabelText = "Creatore: Alessandro Dinardo(MyAlexro)",

         //MessageBoxes, dialogs etc
         IT_ApplyThemeModeMsgBoxCaption = "Applica modalità tema",
         IT_ApplyThemeModeMsgBox = "Per applicare il tema bisogna riavviare l'applicazione, riavviarla adesso?",
         IT_ApplyThemeColorMsgBoxCaption = "Applica colore tema",
         IT_ApplyThemeColorMsgBox = "Per applicare il colore del tema bisogna riavviare l'applicazione, riavviarla adesso?",
         IT_ApplyLanguageMsgBoxCaption = "Applica lingua",
         IT_ApplyLanguageMsgBox = "Per applicare completamente la lingua bisogna riavviare l'applicazione, riavviarla adesso?",
         IT_SelectFormatMsgBox = "Selezionare un formato in cui convertire l'immagine!",
         IT_ConversionResultTextBlockRunning = "Conversione in corso",
         IT_ConversionResultTextBlockFinishedText = "Immagine convertita e salvata",
         IT_MultipleConversionResultTextBlockFinishedText = "Immagini convertite e salvate",
         IT_UnsuccConversionResultTextBlockFinishedText = "Non è stato possibile convertire e/o comprimere alcune immagini: ",
         IT_CantConvertImageToSameFormat = "Non è possibile un'immagine allo stesso formato",
         IT_CantConvertThisImageToIco = "Solo le immagini con il formato \"png\" o \"bmp\" possono essere convertite in immagini \"ico\" o \"cur\"!",
         IT_WarningUnsupportedFile = "⚠ Il file che si vuole convertire non è un'immagine o non è supportato",
         IT_SomeImagesAreAlreadyPresent = "⚠ Alcune immagini non verranno aggiunte perchè sono già presenti",
         IT_CantFindDroppedImagesInOriginalFolder = "Non è stato possibile trovare una o più immagini da convertire nella loro cartella originale";

        //--ENGLISH--
        public static readonly string
        //Conversion options
         EN_GifFramesDelayTimeLabelText = "Delay time between two frames:",
         EN_AddToExistingImagesToolTip = "Add the dropped images to the current ones",
         EN_ReplaceExistingImagesToolTip = "Replace the current images with the dropped ones",
         EN_StartConversionBttnText = "Convert images",
         EN_ChooseFormatLabelText = "Choose the format \nto convert the image to:",
         EN_ImageViewerContextMenuText = "Empty",
         EN_CompressionAlgoLabelText = "Compression algorithm:",
         EN_NoneAlgoLabelText = "None",
         EN_ReplacePngTransparencyLabelText = "Replace the transparency of png images with:",
         EN_GifRepeatTimesLabelText = "Times the gif will repeat:",
         EN_QualityLabelText = "Quality:",
         EN_Nothing = "Nothing",
         EN_White = "White",
         EN_Black = "Black",

         //Settings menu
         EN_SettingsLabelText = "Settings",
         EN_ThemeColorLabelText = "Theme color:",
         EN_ThemeLabelText = "Theme:",
         EN_LanguageLabelText = "Language:",
         EN_CreditsLabelText = "Creator: Alessandro Dinardo(MyAlexro)",

         //MessageBoxes, dialogs etc
         EN_ApplyThemeModeMsgBoxCaption = "Apply theme mode",
         EN_ApplyThemeModeMsgBox = "To apply the theme the application must be restarted, restart it now?",
         EN_ApplyThemeColorMsgBoxCaption = "Apply theme color",
         EN_ApplyThemeColorMsgBox = "To apply the color of the theme the application must be restarted, restart it now?",
         EN_ApplyLanguageMsgBoxCaption = "Apply language",
         EN_ApplyLanguageMsgBox = "To fully apply the language the application must be restarted, restart it now?",
         EN_SelectFormatMsgBox = "Select a format to convert the image to!",
         EN_ConversionResultTextBlockRunning = "Converting",
         EN_ConversionResultTextBlockFinishedText = "Image(s) converted and saved",
         EN_MultipleConversionResultTextBlockFinishedText = "Image(s) converted and saved",
         EN_UnsuccConversionResultTextBlockFinishedText = "It wasn't possible to convert and/or compress some images: ",
         EN_CantConvertImageToSameFormat = "You can't convert an image to the same format",
         EN_CantConvertThisImageToIco = "Only \"png\" or \"bmp\" images can be converted to \"ico\" or \"cur\" images!",
         EN_WarningUnsupportedFile = "⚠ The file you are trying to convert isn't an image or isn't supported",
         EN_SomeImagesAreAlreadyPresent = "⚠ Some images won't be added because they are already present",
         EN_CantFindDroppedImagesInOriginalFolder = "Some images to convert couldn't be found in their original folder";
    }
}
