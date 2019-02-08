using System;
using Tobii.Interaction;
using Tobii.Interaction.Framework;

// using System;
// using System.Diagnostics;
// using System.IO;

namespace DDM_test1
{
    class Program
    {
        private static Host _host;
        private static GazePointDataStream _gazePointDataStream;
        private static FixationDataStream _fixationDataStream;
        private static EyePositionStream _eyePositionDataStream;
        private static HeadPoseStream _headPoseStream;
        private static DateTime _fixationBeginTime = default(DateTime);
        private static double _fixationBeginTimestamp = Double.NaN;

        public static void Main(string[] args)
        {
            _host = new Host();
            
            string input = "0";
            while (!input.Equals("exit"))
            {
                switch (input)
                {
                    case "0":
                        _host.DisableConnection();
                        Console.Clear();
                        Console.WriteLine("============================================================");
                        Console.WriteLine("|           DDM First Release  v0.1                        |");
                        Console.WriteLine("============================================================");
                        Console.WriteLine();
                        Console.WriteLine();
                        Console.WriteLine("'0'   - to jump back to main menu");
                        Console.WriteLine("'1.0' - Gaze point data stream FILTERED");
                        Console.WriteLine("'1.1' - Gaze point data stream UN-FILTERED");
                        Console.WriteLine("'2.0' - Fixation data stream BASE");
                        Console.WriteLine("'2.1' - Fixation data stream NEXT");
                        Console.WriteLine("'3.0' - Eyes position stream BASE");
                        Console.WriteLine("'4.0' - Head position stream NEXT");
                        Console.WriteLine("");
                        Console.WriteLine("You can use any other key to pause the running stream: ");
                        _host = new Host();
                        break;


                    case "1.0":
                        _gazePointDataStream = _host.Streams.CreateGazePointDataStream();
                        _gazePointDataStream.GazePoint((x, y, ts) =>
                        {
                            Console.WriteLine("Timestamp: {0}\tX:{1}, Y:{2}", ts, x, y);
                        });
                        break;

                    case "1.1":
                        if (_gazePointDataStream != null)
                            _gazePointDataStream.Next -= OnNextGazePoint;

                        _gazePointDataStream = _host.Streams.CreateGazePointDataStream(GazePointDataMode.Unfiltered);
                        _gazePointDataStream.Next += OnNextGazePoint;
                        break;
                

                    case "2.0":
                        _fixationDataStream = _host.Streams.CreateFixationDataStream();
                        _fixationDataStream
                            .Begin((x, y, _) =>
                            {
                                Console.WriteLine("\n" +
                                      "Fixation started at X: {0}, Y: {1}", x, y);
                                _fixationBeginTime = DateTime.Now;
                            })
                            .Data((x, y, _) =>
                            {
                                Console.WriteLine("During fixation, currently at: X: {0}, Y: {1}", x, y);
                            })
                            .End((x, y, _) =>
                            {
                                Console.WriteLine("Fixation ended at X: {0}, Y: {1}", x, y);
                                if (_fixationBeginTime != default(DateTime))
                                {
                                    Console.ForegroundColor = ConsoleColor.Cyan;
                                    Console.WriteLine("Fixation duration: {0}", DateTime.Now - _fixationBeginTime);
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                            });
                        break;

                    case "2.1":
                        if (_fixationDataStream != null)
                            _fixationDataStream.Next -= OnNextFixation;
                        _fixationDataStream = _host.Streams.CreateFixationDataStream(FixationDataMode.Slow);
                        _fixationDataStream.Next += OnNextFixation;
                        break;

                    case "3.0":
                        _eyePositionDataStream = _host.Streams.CreateEyePositionStream();
                        _eyePositionDataStream.EyePosition(eyePosition =>
                        {
                            Console.WriteLine("Has Left eye position: {0}", eyePosition.HasLeftEyePosition);
                            Console.WriteLine("Left eye position: X:{0} Y:{1} Z:{2}",
                                eyePosition.LeftEye.X, eyePosition.LeftEye.Y, eyePosition.LeftEye.Z);
                            Console.WriteLine("Left eye position (normalized): X:{0} Y:{1} Z:{2}",
                                eyePosition.LeftEyeNormalized.X, eyePosition.LeftEyeNormalized.Y, eyePosition.LeftEyeNormalized.Z);

                            Console.WriteLine("Has Right eye position: {0}", eyePosition.HasRightEyePosition);
                            Console.WriteLine("Right eye position: X:{0} Y:{1} Z:{2}",
                                eyePosition.RightEye.X, eyePosition.RightEye.Y, eyePosition.RightEye.Z);
                            Console.WriteLine("Right eye position (normalized): X:{0} Y:{1} Z:{2}",
                                eyePosition.RightEyeNormalized.X, eyePosition.RightEyeNormalized.Y, eyePosition.RightEyeNormalized.Z);
                            Console.WriteLine();
                        });
                        break;

                    case "4.0":
                        _headPoseStream = _host.Streams.CreateHeadPoseStream();
                        _headPoseStream.Next += OnNextHeadPose;
                        break;

                    default:
                        input = "exit";
                        break;
                }
                input = Console.ReadLine();

            }

        }



        private static void OnNextGazePoint(object sender, StreamData<GazePointData> gazePoint)
        {
            Console.WriteLine("Timestamp: {0}\tX:{1}, Y:{2}", gazePoint.Data.Timestamp, gazePoint.Data.X, gazePoint.Data.Y);
        }

        private static void OnNextFixation(object sender, StreamData<FixationData> fixation)
        {
            switch (fixation.Data.EventType)
            {
                case FixationDataEventType.Begin:
                    Console.WriteLine("\n" +
                                      "Fixation started at X: {0}, Y: {1}", fixation.Data.X, fixation.Data.Y);
                    _fixationBeginTimestamp = fixation.Data.Timestamp;
                    break;

                case FixationDataEventType.Data:
                    Console.WriteLine("During fixation, currently at: X: {0}, Y: {1}", fixation.Data.X, fixation.Data.Y);
                    break;

                case FixationDataEventType.End:
                    Console.WriteLine("Fixation ended at X: {0}, Y: {1}", fixation.Data.X, fixation.Data.Y);
                    if (!Double.IsNaN(_fixationBeginTimestamp))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Fixation duration: {0}", TimeSpan.FromMilliseconds(fixation.Data.Timestamp - _fixationBeginTimestamp));
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    break;
            }
        }

        private static void OnNextHeadPose(object sender, StreamData<HeadPoseData> headPose)
        {
            var timestamp = headPose.Data.Timestamp;
            var hasHeadPosition = headPose.Data.HasHeadPosition;
            var headPosition = headPose.Data.HeadPosition;
            var hasRotation = headPose.Data.HasRotation;
            var headRotation = headPose.Data.HeadRotation;

            Console.WriteLine($"Head pose timestamp  : {timestamp}");
            Console.WriteLine($"Has head position    : {hasHeadPosition}");
            Console.WriteLine($"Has rotation  (X,Y,Z): ({hasRotation.HasRotationX},{hasRotation.HasRotationY},{hasRotation.HasRotationZ})");
            Console.WriteLine($"Head position (X,Y,Z): ({headPosition.X},{headPosition.Y},{headPosition.Z})");
            Console.WriteLine($"Head rotation (X,Y,Z): ({headRotation.X},{headRotation.Y},{headRotation.Z})");
            Console.WriteLine("-----------------------------------------------------------------");
        }
    }
}
