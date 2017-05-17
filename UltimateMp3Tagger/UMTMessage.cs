using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UltimateMusicTagger
{
    public class UMTMessage
    {
        public enum M_TYPE { INFO, WARNING, ERROR };

        public M_TYPE TypeMsg { get; private set; }

        public string Message { get; private set; }

        public UMTMessage(M_TYPE type, string message)
        {
            this.TypeMsg = type;
            this.Message = message;
        }
    }
}
