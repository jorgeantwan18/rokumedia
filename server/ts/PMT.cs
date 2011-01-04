using System;
using System.Collections.Generic;
using System.Text;

namespace HDPVRRecoder_W.ts
{
    public class PMT : PSI
    {
        public int tableid = 0;
        public int section_syntax_indicator;
        public int reversed1;
        public int section_length;
        public int program_number;
        public int reversed2;
        public int version_number;
        public int current_next_indicator;
        public int section_number;
        public int last_section_number;
        public int reversed3;
        public int PCR_PID;
        
        public int reversed4;
        public int program_info_length;
        public byte[] descriptor;

        public List<int> stream_types;
        public List<int> elementary_PIDs_reserved;
        public List<int> elementary_PIDs;
        public List<int> ES_info_lengths_reserved;
        public List<int> ES_info_lengths;
        public List<byte[]> ES_info_descriptors;

        public int CRC_32;
        public static new PMT Parse(byte[] buffer, int pos)
        {
            PMT package = new PMT();
            package.section_syntax_indicator = buffer[pos] >> 0x7;
            if (((buffer[pos] >> 6) & 0x1) != 0)
                throw new NotImplementedException("bit after section_syntax_indicator  is not 0");
            package.reversed1 = (buffer[pos] >> 4) & 0x3;
            package.section_length = ((buffer[pos] & 0xF) << 8) | buffer[pos + 1];
            pos += 2;

            int left = package.section_length;
            package.program_number = (buffer[pos] << 8) | buffer[pos + 1];
            pos += 2;
            left -= 2;
            package.reversed2 = (buffer[pos] >> 6) & 0x3;
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
            package.reversed3 = buffer[pos] >> 5;
            package.PCR_PID = ((buffer[pos] & 0x1F) << 8) | buffer[pos + 1];
            pos += 2;
            left -= 2;
            package.reversed4 = buffer[pos] >> 4;
            package.program_info_length = ((buffer[pos] & 0xF) << 8) | buffer[pos + 1];
            pos += 2;
            left -= 2;
            package.descriptor = new byte[package.program_info_length];
            if (package.program_info_length > 0)
            {
                Buffer.BlockCopy(buffer, pos, package.descriptor, 0, package.program_info_length);
                pos += package.program_info_length;
                left -= package.program_info_length;
            }
            package.stream_types = new List<int>();
            package.elementary_PIDs_reserved = new List<int>();
            package.elementary_PIDs = new List<int>();
            package.ES_info_lengths_reserved = new List<int>();
            package.ES_info_lengths = new List<int>();
            package.ES_info_descriptors = new List<byte[]>();
            int end = pos + left - 4;
            while (pos < end)
            {
                int stream_type = buffer[pos];
                pos += 1;
                left -= 1;
                int elementary_PID_reserved = buffer[pos] >> 5;
                int elementary_PID = ((buffer[pos] & 0x1F) << 8) | buffer[pos + 1];
                pos += 2;
                left -= 2;
                int ES_info_length_reserved = buffer[pos] >> 4;
                int ES_info_length = ((buffer[pos] & 0xF) << 8) | buffer[pos + 1];
                pos += 2;
                left -= 2;
                byte[] ES_info_descriptor = new byte[ES_info_length];
                if (ES_info_length > 0)
                {
                    Buffer.BlockCopy(buffer, pos, ES_info_descriptor, 0, ES_info_length);
                    pos += ES_info_length;
                    left -= ES_info_length;
                }
                package.stream_types.Add(stream_type);
                package.elementary_PIDs_reserved.Add(elementary_PID_reserved);
                package.elementary_PIDs.Add(elementary_PID);
                package.ES_info_lengths_reserved.Add(ES_info_length_reserved);
                package.ES_info_lengths.Add(ES_info_length);
                package.ES_info_descriptors.Add(ES_info_descriptor);

            }
            package.CRC_32 = (buffer[pos] << 24) | (buffer[pos + 1] << 16) | (buffer[pos + 2] << 8) | buffer[pos + 3];
            left -= 4;
            pos += 4;
            return package;
        }
    }
}
