using System;
using System.Text;
using System.Collections.Generic;

namespace HDPVRRecoder_W.ts
{
    public class PAT:PSI
    {
       
        public int tableid = 0;
        public int section_syntax_indicator;
        public int reversed1;
        public int section_length;
        public int transport_stream_id;
        public int reserved2;
        public int version_number;
        public int current_next_indicator;
        public int section_number;
        public int last_section_number;

        public List<int> program_numbers;
        public List<int> networkids_reversed;
        public List<int> networkids;

        public int CRC_32;


        public static new PAT Parse(byte[] buffer, int pos)
        {
            PAT package = new PAT();
            package.section_syntax_indicator = buffer[pos] >> 0x7;
            if (((buffer[pos] >> 6) & 0x1) != 0)
                throw new NotImplementedException("bit after section_syntax_indicator  is not 0");
            package.reversed1 = (buffer[pos] >> 4) & 0x3;
            package.section_length = ((buffer[pos] & 0xF) << 8) | buffer[pos + 1];
            pos += 2;
            int left = package.section_length;
            package.transport_stream_id = (buffer[pos] << 8) | buffer[pos + 1];
            pos += 2;
            left -= 2;
            package.reserved2 = buffer[pos] >> 6;
            package.version_number = (buffer[pos] >> 1) & 0x1F;
            package.current_next_indicator = buffer[pos] & 0x1;
            pos += 1;
            left -= 1;

            package.section_number = buffer[pos];
            pos += 1;
            left -= 1;
            package.last_section_number = buffer[pos];
            pos += 1;
            left -= 1;
            package.program_numbers = new List<int>();
            package.networkids_reversed = new List<int>();
            package.networkids = new List<int>();
            int end = pos + left - 4;
            while (pos < end)
            {
                int program_number = (buffer[pos] << 8) | buffer[pos + 1];
                pos += 2;
                left -= 2;

                int networkid_reversed = buffer[pos] >> 5;
                int networkid = ((buffer[pos] & 0x1F) << 8) | buffer[pos + 1];
                pos += 2;
                left -= 2;
                package.program_numbers.Add(program_number);
                package.networkids_reversed.Add(networkid_reversed);
                package.networkids.Add(networkid);
            }
            package.CRC_32 = (buffer[pos] << 24) | (buffer[pos + 1] << 16) | (buffer[pos + 2] << 8) | buffer[pos + 3];
            left -= 4;
            pos += 4;
            return package;
        }
    }
}
