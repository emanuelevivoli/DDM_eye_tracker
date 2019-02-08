using System;
using Tobii.Interaction;
using Tobii.Interaction.Framework;

// using System.Diagnostics;
using System.IO;

namespace DDM_test1
{
    class Program
    {
        private static Host _host;
        private static GazePointDataStream _gazePointDataStream;
        private static FixationDataStream _fixationDataStream;
        private static DateTime _fixationBeginTime = default(DateTime);
        


        public static void Main(string[] args)
        {
            _host = new Host();
            
            StreamWriter gaze_outputFile = new StreamWriter(@"C: \Users\lello\source\repos\DDM_test1\gazeStream.txt", true);
            StreamWriter fixation_outputFile = new StreamWriter(@"C: \Users\lello\source\repos\DDM_test1\fixationStream.txt", true);

            _gazePointDataStream = _host.Streams.CreateGazePointDataStream();
            
            _gazePointDataStream.GazePoint((x, y, ts) =>
            {
                Console.WriteLine("Timestamp: {0}\tX:{1}, Y:{2}", ts, x, y);
                gaze_outputFile.WriteLine("Timestamp: {0}\tX:{1}, Y:{2}", ts, x, y);
            });
                
            
            




            _fixationDataStream = _host.Streams.CreateFixationDataStream();
            _fixationDataStream
            .Begin((x, y, _) =>
            {
                Console.WriteLine("\n" + "Fixation started at X: {0}, Y: {1}", x, y);
                fixation_outputFile.WriteLine("\n" + "Fixation started at X: {0}, Y: {1}", x, y);
                _fixationBeginTime = DateTime.Now;
            })
            .Data((x, y, _) =>
            {
                Console.WriteLine("During fixation, currently at: X: {0}, Y: {1}", x, y);
                fixation_outputFile.WriteLine("During fixation, currently at: X: {0}, Y: {1}", x, y);
            })
            .End((x, y, _) =>
            {
                fixation_outputFile.WriteLine("Fixation ended at X: {0}, Y: {1}", x, y);
                if (_fixationBeginTime != default(DateTime))
                {
                    Console.WriteLine("Fixation duration: {0}", DateTime.Now - _fixationBeginTime);
                    fixation_outputFile.WriteLine("Fixation duration: {0}", DateTime.Now - _fixationBeginTime);
                }
            });
                
            Console.ReadKey();
            
        }
    }
    
}
