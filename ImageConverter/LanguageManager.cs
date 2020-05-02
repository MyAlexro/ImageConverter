
using System;
using System.Collections.Generic;

namespace ImageConverter
{
    public class LanguageManager
    {
        public static readonly string[] languages = new string[2] { "it", "en" };
        
        //Italian
        public static readonly string
         IT_SettingsLabelTxt = "Impostazioni",
         IT_FontColorLabelTxT = "Colore Tema:",
         IT_ThemeLabelTxt = "Tema:",
         IT_ApplyThemeMsgBox = "Per applicare il tema bisogna riavviare l'applicazione, riavviarla adesso?",
         IT_ApplyFontColorMsgBox = "Per applicare il colore del font bisogna riavviare l'applicazione, riavviarla adesso?",
         IT_CreditsLabelTxt = "Creatore: Alessandro Dinardo(MyAlexro)",
         IT_StartConversionLabelTxt = "Converti immagine",
         IT_ChooseFormatLabelTxt = "Scegliere il formato in cui \nconvertire l'immagine:",
         IT_WarningLabelTxt = "⚠ Il file che si vuole convertire non è un'immagine o non è supportato",
         IT_SelectFormatMsgBox = "Selezionare un formato in cui convertire l'immagine",
         IT_DropImageMsgBox = "Selezionare un formato in cui convertire l'immagine",
         IT_CantConvertThisFile = "Non è possibile convertire questo file",
         IT_ConversionResultTextBlockRunningTxt = "Conversione in corso",
         IT_ConversionResultTextBlockFinishedTxt = "Immagine convertita e salvata",
         IT_MultipleConversionResultTextBlockFinishedTxt = "Immagini convertite e salvate",
         IT_UnsuccConversionResultTextBlockFinishedTxt = "Non è stato possibile convertire alcune immagini: ",
         IT_CantConvertImageToSameFormat = "Non si può convertire un'immagine allo stesso formato",
         IT_CantConvertThisImageToIco = "Solo le immagini con il formato \"png\" o \"bmp\" possono essere convertite in immagini \"ico\" o \"cur\"!",
         IT_GifLoopsOptionText = "Volte che la gif si ripeterà:",
         IT_EmpyBttnCntxtMenu = "Svuota",
         IT_ApplyLanguageMsgBox = "Per applicare la lingua bisogna riavviare l'applicazione, riavviarla desso?",
         IT_LanguageLabelTxt = "Lingua:",
         IT_DelayTimeLabelTxt = "Tempo di passaggio da un frame all'altro:",
         IT_AddToExistingImagesToolTip = "Aggiungi le immagini trascinate",
         IT_ReplaceExistingImagesToolTip = "Sostituisci con le immagini presente con quelle trascinate";

        //English
        public static readonly string
         EN_SettingsLabelTxt = "Settings",
         EN_FontColorLabelTxT = "FontColor:",
         EN_ThemeLabelTxt = "Theme:",
         EN_ApplyThemeMsgBox = "To apply the theme the application needs to be restarted, restart it now?",
         EN_ApplyFontColorMsgBox = "To apply the color of the font the application needs to be restarted, restart it now?",
         EN_CreditsLabelTxt = "Creator: Alessandro Dinardo(MyAlexro)",
         EN_StartConversionLabelTxt = "Convert image",
         EN_ChooseFormatLabelTxt = "Choose the format in which \nto convert the image:",
         EN_WarningLabelTxt = "⚠ The file you are trying to convert isn't an image or isn't supported",
         EN_SelectFormatMsgBox = "Select a format in which to convert the image",
         EN_DropImageMsgBox = "Drop an image",
         EN_CantConvertThisFile = "This file can't be converted",
         EN_ConversionResultTextBlockRunningTxt = "Converting",
         EN_ConversionResultTextBlockFinishedTxt = "Image(s) converted and saved",
         EN_UnsuccConversionResultTextBlockFinishedTxt = "It wasn't possible to convert some images: ",
         EN_CantConvertImageToSameFormat = "You can't convert an image to the same format",
         EN_CantConvertThisImageToIco = "Only \"png\" or \"bmp\" images can be converted to \"ico\" or \"cur\" images!",
         EN_GifLoopOptionText = "Times the gif will repeat:",
         EN_EmpyBttnCntxtMenu = "Empty",
         EN_ApplyLanguageMsgBox = "To apply the language the application needs to be restarted, restart it now?",
         EN_LanguageLabelTxt = "Language:",
         EN_DelayTimeLabelTxt = "Delay time between two frames:",
         EN_AddToExistingImagesToolTip = "Add the dropped images",
         EN_ReplaceExistingImagesToolTip = "Replace current images with dropped images";

    }
}
