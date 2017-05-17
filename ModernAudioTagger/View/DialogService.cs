using Microsoft.Win32;
using ModernAudioTagger.BusinessLogic;
using ModernUILogViewer.BusinessLogic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ModernAudioTagger.View
{
    class DialogService : IDialogService
    {
        public string Title { get; set; }
        public string Filter { get; set; }

        public string OpenPath(string defaultPath)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (Directory.Exists(defaultPath))
                dialog.SelectedPath = defaultPath;

            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            return result == System.Windows.Forms.DialogResult.OK ? dialog.SelectedPath : null;            
        }

        public string[] OpenFile(bool multiselect)
        {
            string[] filenames = null;

            OpenFileDialog dialog = new OpenFileDialog();            
            dialog.Multiselect = multiselect;
            dialog.Filter = this.Filter;
            dialog.Title = this.Title;
            if (dialog.ShowDialog() == true)
            {
                filenames = dialog.FileNames;
            }

            return filenames ?? new string[0];
        }

        public SaveDialogResult SaveFile()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = this.Filter;
            dialog.ShowDialog();

            return new SaveDialogResult { Filename = dialog.FileName, FilterIndex = dialog.FilterIndex };
        }
    }
}
