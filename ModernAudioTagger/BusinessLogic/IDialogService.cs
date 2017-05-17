using ModernAudioTagger.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModernUILogViewer.BusinessLogic
{
    public interface IDialogService
    {
        SaveDialogResult SaveFile();
        string OpenPath(string defaultPath);
        string[] OpenFile(bool multiselect);
    }
}
