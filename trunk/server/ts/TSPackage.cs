using System;
using System.Collections.Generic;
using System.Text;

namespace HDPVRRecoder_W.ts
{
    public abstract class TSPackage
    {
        public const int PACKAGELENGTH = 188;
        public const byte SYNCBYTE = 0x47;


        public int pid;

        public byte[] Data;
        
    }
}
