using Accord;
using Compunet.YoloV8;
using Compunet.YoloV8.Plotting;
using OpenCvSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using System.Drawing;
using Color = Microsoft.Maui.Graphics.Color;
using Size = OpenCvSharp.Size; // Or your chosen video library


namespace YoloTest
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
            //using var predictor = new YoloV8(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models", "yolov8n.onnx"));

            //var result = predictor.Classify("C:\\Users\\Tushar\\Pictures\\download.bmp");
            // or
            //var test_image = SixLabors.ImageSharp.Image.Load("C:\\Users\\Tushar\\Pictures\\Test image.jpg");


            //result.PlotImage(test_image);
            ProcessVideo("C:\\Users\\Tushar\\Downloads\\120495896_291600253812769_6977585485950531814_n.mp4", "C:\\Users\\Tushar\\Downloads\\test_video.mp4");

            //Console.WriteLine(result);

        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }

        public void ProcessVideo(string inputFilePath, string outputFilePath)
        {
            using var predictor = new YoloV8(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models", "yolov8n.onnx"));

            using (var capture = new VideoCapture(inputFilePath))
            using (var writer = new VideoWriter(outputFilePath, FourCC.MJPG, capture.Fps, new Size(capture.FrameWidth, capture.FrameHeight)))
            {
                int FPS = 0;
                Mat frame = new Mat();
                while (true)
                {
                    bool hasFrame = capture.Read(frame);
                    if (!hasFrame || frame.Empty())
                        break;

                    if (++FPS > 5)
                    {
                        FPS = FPS % 5;

                        //var image = frame.To<SixLabors.ImageSharp.Image>();
                        var result = predictor.Detect(frame.ToBytes());
                        // Optional: Process the frame here

                        //result.PlotImage();
                        foreach (var v in result.Boxes)
                        {
                            Cv2.PutText(frame, v.Class.Name, new OpenCvSharp.Point(v.Bounds.X, v.Bounds.Y), HersheyFonts.HersheyPlain, 1, Scalar.White, 3, LineTypes.Link8);
                            Cv2.Rectangle(frame, new OpenCvSharp.Rect(v.Bounds.X, v.Bounds.Y, v.Bounds.Width, v.Bounds.Height), Scalar.DarkRed, 1, LineTypes.Link8, 0);
                        }

                        writer.Write(frame);
                    }
                    else
                    {
                        writer.Write(frame);
                        continue;
                    }
                }
            }
        }
    }
}
