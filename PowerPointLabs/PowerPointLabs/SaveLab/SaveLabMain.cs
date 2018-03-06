﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Microsoft.Office.Interop.PowerPoint;

namespace PowerPointLabs.SaveLab
{
    class SaveLabMain
    {
        private static string defaultTestSaveName = Path.Combine(SaveLabSettings.GetDefaultSavePath(), "SaveLab_Copy.pptx");

        public static void SaveFile(Models.PowerPointPresentation currentPresentation, bool isTesting = false)
        {
            // Opens up a new Save File Dialog
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            Models.PowerPointPresentation newPresentation;

            List<Models.PowerPointSlide> selectedSlides = currentPresentation.SelectedSlides;

            // Setting for Save File Dialog
            saveFileDialog.InitialDirectory = SaveLabSettings.SaveFolderPath;
            saveFileDialog.Filter = "PowerPoint Presentations|*.pptx";
            saveFileDialog.Title = "Save Selected Slides";
            saveFileDialog.OverwritePrompt = true;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Check if functional test is switched on
                if (isTesting)
                {
                    // Save for functional test
                    currentPresentation.Presentation.SaveCopyAs(defaultTestSaveName, PpSaveAsFileType.ppSaveAsDefault);
                }
                else
                {
                    // Copy the Current Presentation under a new name
                    currentPresentation.Presentation.SaveCopyAs(saveFileDialog.FileName, PpSaveAsFileType.ppSaveAsDefault);
                }

                try
                {
                    // Re-open the save copy in the same directory in the background
                    Presentations newPres = new Microsoft.Office.Interop.PowerPoint.Application().Presentations;
                    Presentation tempPresentation;
                    // Check if functional test is switched on
                    if (isTesting)
                    {
                        tempPresentation = newPres.Open(defaultTestSaveName, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoFalse);
                    }
                    else
                    {
                        tempPresentation = newPres.Open(saveFileDialog.FileName, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoFalse);
                    }
                    newPresentation = new Models.PowerPointPresentation(tempPresentation);

                    // Check and remove un-selected slides using unique slide ID
                    bool removeSlide;
                    for (int i = newPresentation.SlideCount - 1; i >= 0; i--)
                    {
                        removeSlide = true;
                        foreach (Models.PowerPointSlide selectedSlide in selectedSlides)
                        {
                            if (newPresentation.Slides[i].ID == selectedSlide.ID)
                            {
                                removeSlide = false;
                                break;
                            }
                        }
                        if (removeSlide)
                        {
                            newPresentation.RemoveSlide(i);
                        }
                    }
                    // Save and then close the presentation
                    newPresentation.Save();
                    newPresentation.Close();
                }
                catch (System.Runtime.InteropServices.COMException)
                {
                    // do nothing as file is successfully copied
                }
            }
        }
    }
}
