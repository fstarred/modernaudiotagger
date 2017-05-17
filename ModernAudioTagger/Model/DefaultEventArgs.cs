using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModernAudioTagger.Model
{
    class DefaultEventArgs : EventArgs
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public object ObjArg { get; set; }
    }
}
