using System;
using Tobii.Interaction;
using Tobii.Interaction.Framework;

using System.IO;
using System.Diagnostics;

namespace DDM_test1
{
    class Program
    {
        private static Host _host;
        private static GazePointDataStream _gazePointDataStream;
        private static FixationDataStream _fixationDataStream;
        private static DateTime _fixationBeginTime = default(DateTime);
        private static string path_base = @"C:\Users\lello\Source\Repos\DDM_eye_tracker\DDM_test1\Streams\";



        public static void Main(string[] args)
        {
            // create host stream for Tobii
            _host = new Host();
            _gazePointDataStream = _host.Streams.CreateGazePointDataStream();

            // set params to save streams on file
            string gaze_path = "gazeStream.txt";
            string fixs_path = "fixationStream.txt";

            // set params to connect with python script
            string path = @"C:\Users\lello\source\repos\DDM_eye_tracker\DDM_test1\test.py";

            // call for save on file function
            stream_read_write(path, gaze_path, fixs_path);


            Console.Write("----------------------------------------------------------------------------------------------------------------------------------------");
            Console.ReadKey();
            
        }

        


        private static void stream_read_write(string path, string gaze_path, string fixs_path)
        {
            ProcessStartInfo start = new ProcessStartInfo();

            start.FileName = @"C:\Users\lello\AppData\Local\Programs\Python\Python37\python.exe";  //"C:\\Python27\\python.exe";
            start.Arguments = string.Format("{0}", path);
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.RedirectStandardInput = true;
            start.CreateNoWindow = true;

            int count = 0;
            bool flag = true;

            using (Process process = Process.Start(start))
            using (StreamWriter writer = process.StandardInput)
            using (StreamReader reader = process.StandardOutput)
            {
                while (flag)
                {
                    string all_paths = path_base + count.ToString() + gaze_path + " " + path_base + count.ToString() + fixs_path;
                    writer.WriteLine(all_paths);

                    stream_on_file(path_base + count.ToString() + gaze_path, path_base + count.ToString() + fixs_path);

                    Console.WriteLine("written: " + all_paths);

                    string result = null;

                    while (result == null || result.Length == 0)
                    { result = reader.ReadLine(); }

                    if (System.Text.RegularExpressions.Regex.Match(result, @"\bACK\b").Length > 0)
                    {
                        count = Int32.Parse(System.Text.RegularExpressions.Regex.Match(result, @"\d+").Value);
                    }
                    else
                    {
                        flag = false;
                    }
                    
                    Console.WriteLine("read: " + result + "\n");
                    count = count + 1;
                    Console.WriteLine(string.Format("calculate next: {0}", count));

                }
            }
        }

        private static void stream_on_file(string gaze_path, string fixs_path)
        {
            StreamWriter gaze_outputFile = new StreamWriter(Path.Combine(path_base, gaze_path));
            StreamWriter fixation_outputFile = new StreamWriter(Path.Combine(path_base, fixs_path));

            _gazePointDataStream.GazePoint((x, y, ts) =>
            {
                // Console.WriteLine("Timestamp: {0}\tX:{1}, Y:{2}", ts, x, y);
                gaze_outputFile.WriteLine("Timestamp: {0}\tX:{1}, Y:{2}", ts, x, y);
            });

            _fixationDataStream = _host.Streams.CreateFixationDataStream();
            _fixationDataStream
            .Begin((x, y, _) =>
            {
                // Console.WriteLine("\n" + "Fixation started at X: {0}, Y: {1}", x, y);
                fixation_outputFile.WriteLine("\n" + "Fixation started at X: {0}, Y: {1}", x, y);
                _fixationBeginTime = DateTime.Now;
            })
            .Data((x, y, _) =>
            {
                // Console.WriteLine("During fixation, currently at: X: {0}, Y: {1}", x, y);
                fixation_outputFile.WriteLine("During fixation, currently at: X: {0}, Y: {1}", x, y);
            })
            .End((x, y, _) =>
            {
                fixation_outputFile.WriteLine("Fixation ended at X: {0}, Y: {1}", x, y);
                if (_fixationBeginTime != default(DateTime))
                {
                    // Console.WriteLine("Fixation duration: {0}", DateTime.Now - _fixationBeginTime);
                    fixation_outputFile.WriteLine("Fixation duration: {0}", DateTime.Now - _fixationBeginTime);
                }
            });

        }

    }
    
}
