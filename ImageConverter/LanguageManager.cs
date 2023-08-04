
using System;
using System.Collections.Generic;

namespace ImageConverter
{
    public class LanguageManager
    {
        public static readonly string[] languages = new string[2] { "it", "en" };

        //--ITALIAN--
        public static readonly string

         //Conversion options
         IT_DelayTimeLabelTxt = "Tempo di passaggio da un frame all'altro:",
         IT_AddToExistingImagesToolTip = "Aggiungi le immagini trascinate a quelle già presenti",
         IT_ReplaceExistingImagesToolTip = "Sostituisci le immagini presenti con quelle trascinate",
         IT_StartConversionLabelTxt = "Converti immagini",
         IT_ChooseFormatLabelTxt = "Scegliere il formato in \ncui convertire l'immagine:",
         IT_EmpyBttnCntxtMenu = "Svuota",
         IT_CompressionAlgoLabelText = "Algortimo di compressione:",
         IT_NoneAlgoLabelText = "Nessuno",
         IT_ReplacePngTransparencyLabelTxt = "Sostituisci la trasparenza della png con:",
         IT_GifLoopsOptionText = "Volte che la gif si ripeterà:",
         IT_QualityLabelText = "Qualità:",
         IT_Nothing = "Niente",
         IT_White = "Bianco",
         IT_Black = "Nero",

         //Settings menu
         IT_SettingsLabelTxt = "Impostazioni",
         IT_ThemeColorLabelTxT = "Colore tema:",
         IT_ThemeLabelTxt = "Tema:",
         IT_LanguageLabelTxt = "Lingua:",
         IT_CreditsLabelTxt = "Creatore: Alessandro Dinardo(MyAlexro)",

         //MessageBoxes, dialogs etc
         IT_ApplyThemeMsgBox = "Per applicare il tema bisogna riavviare l'applicazione, riavviarla adesso?",
         IT_ApplyThemeColorMsgBox = "Per applicare il colore del tema bisogna riavviare l'applicazione, riavviarla adesso?",
         IT_ApplyLanguageMsgBox = "Per applicare la lingua bisogna riavviare l'applicazione, riavviarla adesso?",
         IT_SelectFormatMsgBox = "Selezionare un formato in cui convertire l'immagine!",
         IT_ConversionResultTextBlockRunning = "Conversione in corso",
         IT_ConversionResultTextBlockFinishedTxt = "Immagine convertita e salvata",
         IT_MultipleConversionResultTextBlockFinishedTxt = "Immagini convertite e salvate",
         IT_UnsuccConversionResultTextBlockFinishedTxt = "Non è stato possibile convertire o comprimere alcune immagini: ",
         IT_CantConvertImageToSameFormat = "Non è possibile un'immagine allo stesso formato",
         IT_CantConvertThisImageToIco = "Solo le immagini con il formato \"png\" o \"bmp\" possono essere convertite in immagini \"ico\" o \"cur\"!",
         IT_WarningUnsupportedFile = "⚠ Il file che si vuole convertire non è un'immagine o non è supportato",
         IT_SomeImagesAreAlreadyPresent = "⚠ Alcune immagini non verranno aggiunte perchè sono già presenti",
         IT_CantFindDroppedImagesInOriginalFolder = "Non è stato possibile trovare una o più immagini da convertire nella loro cartella",

         blank = "";

        //--ENGLISH--
        public static readonly string

        //Conversion options
         EN_DelayTimeLabelTxt = "Delay time between two frames:",
         EN_AddToExistingImagesToolTip = "Add the dropped images to the current ones",
         EN_ReplaceExistingImagesToolTip = "Replace the current images with the dropped ones",
         EN_StartConversionLabelTxt = "Convert images",
         EN_ChooseFormatLabelTxt = "Choose the format \nto convert the image to:",
         EN_EmpyBttnCntxtMenu = "Empty",
         EN_CompressionAlgoLabelText = "Compression algorithm:",
         EN_NoneAlgoLabelText = "None",
         EN_ReplacePngTransparencyLabelTxt = "Replace the transparency of png images with:",
         EN_GifLoopsOptionText = "Times the gif will repeat:",
         EN_QualityLabelText = "Quality:",
         EN_Nothing = "Nothing",
         EN_White = "White",
         EN_Black = "Black",

         //Settings menu
         EN_SettingsLabelTxt = "Settings",
         EN_ThemeColorLabelTxT = "Theme color:",
         EN_ThemeLabelTxt = "Theme:",
         EN_LanguageLabelTxt = "Language:",
         EN_CreditsLabelTxt = "Creator: Alessandro Dinardo(MyAlexro)",

         //MessageBoxes, dialogs etc
         EN_ApplyThemeMsgBox = "To apply the theme the application must be restarted, restart it now?",
         EN_ApplyThemeColorMsgBox = "To apply the color of the theme the application must be restarted, restart it now?",
         EN_ApplyLanguageMsgBox = "To apply the language the application must be restarted, restart it now?",
         EN_SelectFormatMsgBox = "Select a format to convert the image to!",
         EN_ConversionResultTextBlockRunning = "Converting",
         EN_ConversionResultTextBlockFinishedTxt = "Image(s) converted and saved",
         EN_MultipleConversionResultTextBlockFinishedTxt = "Image(s) converted and saved",
         EN_UnsuccConversionResultTextBlockFinishedTxt = "It wasn't possible to convert or compress some images: ",
         EN_CantConvertImageToSameFormat = "You can't convert an image to the same format",
         EN_CantConvertThisImageToIco = "Only \"png\" or \"bmp\" images can be converted to \"ico\" or \"cur\" images!",
         EN_WarningUnsupportedFile = "⚠ The file you are trying to convert isn't an image or isn't supported",
         EN_SomeImagesAreAlreadyPresent = "⚠ Some images won't be added because they are already present",
         EN_CantFindDroppedImagesInOriginalFolder = "Some images to convert couldn't be found in their folder",

         blank2 = "";

    }
}
