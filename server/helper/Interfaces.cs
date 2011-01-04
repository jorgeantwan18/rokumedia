using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace HDPVRRecoder_W.helper
{
    public interface ISupportAddOutputStream
    {
        void AddOutputStream(Stream stream);
        void RemoveOutputStream(Stream stream);
    }
    public interface ITryStopAble
    {
        void TryStop();
    }
    public interface ISupportGetOutputStream
    {
        Stream GetOutputStream();
    }
    public interface IFileStream
    {
        string GetFileName();
    }
    public interface IFFMpegParameters
    {
        string GetFFMpegParameters();
        string GetSize();
    }
}
