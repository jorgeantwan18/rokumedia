using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.Text.RegularExpressions;

namespace HDPVRRecoder_W
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string file = "";
            foreach (string arg in args)
                file += arg + " ";
            file = file.Trim();
            Process process = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcessesByName(process.ProcessName);
            foreach (Process p in processes)
            {
                if (process.Id != p.Id)
                {
                    try
                    {
                        WindowsNamedPipe pipe = new WindowsNamedPipe("hdpvrqueue", false);
                        StreamReader reader = new StreamReader(file, Encoding.UTF8);
                        string data = reader.ReadToEnd();
                        reader.Close();
                        pipe.Write(Encoding.UTF8.GetBytes(data));
                        pipe.Close();
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(ex.ToString());
                    }
                    return;
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            FormMain formmain = new FormMain();
            formmain.Show();
            if (file != "")
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(file);
                formmain.AddNew(doc, false);
            }
            Application.Run();

        }
    }
}
