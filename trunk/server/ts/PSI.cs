using System;
using System.Collections.Generic;
using System.Text;

namespace HDPVRRecoder_W.ts
{
    public abstract class PSI:TSPackage
    {
        public static PSI Parse(byte[] buffer,int pos)
        {
            int tableid = (buffer[pos] << 8) | buffer[pos + 1];
            pos += 2;
            if (tableid == 0)//PAT
            {
                return PAT.Parse(buffer, pos);
            }
            else if (tableid == 0x0002)
            {
                return PMT.Parse(buffer, pos);
            }
            else
                return new UnknownPSI();
        }
    }
}
