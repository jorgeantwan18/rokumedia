using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.ComponentModel;

namespace HDPVRRecoder_W
{
    public class WindowsNamedPipe:Stream
    {
        [StructLayout(LayoutKind.Sequential)]
        public class SecurityAttributes
        {
        }

        public AutoResetEvent _connected;
        [DllImport("kernel32")]
        public static extern IntPtr CreateNamedPipeA(String lpName, int dwOpenMode, int dwPipeMode,int nMaxInstances, int nOutBufferSize, int nInBufferSize,int nDefaultTimeOut, object lpSecurityAttributes);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateFile(
            String lpFileName,						  // file name
            uint dwDesiredAccess,					  // access mode
            uint dwShareMode,								// share mode
            SecurityAttributes attr,				// SD
            uint dwCreationDisposition,			// how to create
            uint dwFlagsAndAttributes,			// file attributes
            uint hTemplateFile);					  // handle to template file
        [DllImport("kernel32")]
        public static extern bool ConnectNamedPipe(IntPtr handle, object overlapped);
        [DllImport("kernel32")]
        public static extern bool CloseHandle(IntPtr handle);
        [DllImport("kernel32")]
        public static extern bool ReadFile(IntPtr hFile, byte[] buffer, int nNumberOfBytesToRead, ref int lpNumberOfBytesRead,object lpOverlapped);
        [DllImport("kernel32")]
        public static extern bool WriteFile(IntPtr hFile, byte[] buffer, int nNumberOfBytesToRead, ref int lpNumberOfBytesRead, object lpOverlapped);
        public const uint OPEN_EXISTING = 3;
        public const uint GENERIC_READ = (0x80000000);
        public const uint GENERIC_WRITE = (0x40000000);

        private string _name;
        private IntPtr _handle;
        public WindowsNamedPipe(string name,bool server)
        {

            this._connected = new AutoResetEvent(false);
            
            this._name = "\\\\.\\pipe\\" + name;
            if (server)
            {
                _handle = CreateNamedPipeA(this._name, 3, 0, 255, 188 * 20 * 1024, 188 * 20 * 1024, 0, null);
                new Thread(new ThreadStart(delegate
                {
                    bool result = ConnectNamedPipe(_handle, null);
                    this._connected.Set();
                })).Start();
            }
            else
            {
                _handle = CreateFile(this._name, GENERIC_READ | GENERIC_WRITE, 0, null, OPEN_EXISTING, 0, 0);
                if (_handle.ToInt32() == -1)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        public override bool CanRead
        {
            get { throw new NotImplementedException(); }
        }

        public override bool CanSeek
        {
            get { throw new NotImplementedException(); }
        }

        public override bool CanWrite
        {
            get { throw new NotImplementedException(); }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public int Read(byte[] buffer)
        {
            int read = 0;
            bool result = ReadFile(this._handle, buffer, buffer.Length, ref read, null);
            if (!result)
            {
                CloseHandle(this._handle);
                Log.WriteLine("read from pipe failed");
                return -1;
            }
            return read;
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
        public bool Write(byte[] buffer)
        { 
            int write = 0;
            bool result = WriteFile(this._handle,buffer,buffer.Length,ref write,null);
            if (!result)
            {
                Log.WriteLine("write to name pipe error!!!!!!");
                return false;
            }
            else if (write != buffer.Length)
            {
                Log.WriteLine("write to name pipe error,write " + write + " bytes, should write " + buffer.Length + " bytes");
                return false;
            }
            return true;
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            byte[] data = new byte[count];
            Buffer.BlockCopy(buffer, offset, data, 0, count);
            this.Write(data);    
        }
        public override void Close()
        {
            CloseHandle(this._handle);
            base.Close();
        }
    }
}
