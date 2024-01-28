using Compunet.YoloV8;
using Compunet.YoloV8.Data;
using OpenCvSharp;
using Size = OpenCvSharp.Size;

namespace YoloTest
{
    public class ObjectDetectionInfo
    {
        public string ClassName { get; set; }
        public int ConsecutiveCount { get; set; }
    }

    public class DetectionResultCustom
    {
        public IReadOnlyList<IBoundingBox> Boxes { get; set; }
    }

    public partial class MainPage : ContentPage
    {
        int count = 0;
        Image videoDisplay; // UI element for displaying video frames

        public MainPage()
        {
            InitializeComponent();

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

        }

        private void SetupUI()
        {
            // Create and configure the Image control for video display
            videoDisplay = new Image
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                WidthRequest = 320,
                HeightRequest = 240
            };

            // Add the Image control to the page's layout
            this.Content = new StackLayout
            {
                Children = { videoDisplay }
            };
        }

        private async void OnCounterClicked(object sender, EventArgs e)
        {
            SetupUI();

            //videoDisplay.Source = ConvertPngToImageSource("dotnet_bot.png");

            await Task.Run(() =>
            {
                // Process video from webcam
                ProcessVideoFromWebcam("C:\\Users\\Tushar\\Downloads\\webcam_video.mp4");

            });
        }

        public ImageSource ConvertPngToImageSource(string filePath)
        {
            ImageSource imageSource = null;

            if (File.Exists(filePath))
            {
                byte[] imageData = File.ReadAllBytes(filePath);

                imageSource = ImageSource.FromStream(() => new MemoryStream(imageData));
            }

            return imageSource;
        }


        public void ProcessVideoFromWebcam(string outputFilePath)
        {
            int frameCounter = 0;
            int skipFrames = 2;
            int detectionThreshold = 5; // Number of consecutive frames an object must be detected
            Dictionary<string, ObjectDetectionInfo> detectionCounts = new Dictionary<string, ObjectDetectionInfo>();

            using var predictor = new YoloV8(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models", "yolov8n.onnx"));

            using (var capture = new VideoCapture(0))
            {
                capture.Open(0);
                if (!capture.IsOpened())
                {
                    throw new Exception("Camera not found!");
                }

                using (var writer = new VideoWriter(outputFilePath, FourCC.MJPG, 15, new Size(640, 480)))
                {
                    Mat frame = new Mat();

                    // Start timer
                    var startTime = DateTime.Now;

                    while ((DateTime.Now - startTime).TotalSeconds < 10)
                    {
                        bool hasFrame = capture.Read(frame);
                        if (!hasFrame || frame.Empty())
                            break;

                        if (frameCounter++ % skipFrames != 0) continue;
                        Mat frameClone = new Mat();
                        Cv2.Resize(frame, frameClone, new Size(320, 240));

                        // Calculate elapsed time and format it
                        //var elapsed = DateTime.Now - startTime;
                        //var timerText = $"{elapsed.Minutes:D2}:{elapsed.Seconds:D2}.{elapsed.Milliseconds / 100:D1}";
                        //Cv2.PutText(frame, timerText, new OpenCvSharp.Point(10, 30), HersheyFonts.HersheySimplex, 1, Scalar.White, 2);

                        // Object detection and drawing code (commented out)
                        var result = predictor.Detect(frameClone.ToBytes());
                        UpdateDetectionCounts(new DetectionResultCustom()
                        {
                            Boxes = result.Boxes,
                        }, detectionCounts, detectionThreshold);
                        //foreach (var v in result.Boxes)
                        //{
                        //    Cv2.PutText(frame, v.Class.Name, new OpenCvSharp.Point(v.Bounds.X, v.Bounds.Y), HersheyFonts.HersheyPlain, 1, Scalar.White, 3, LineTypes.Link8);
                        //    Cv2.Rectangle(frame, new OpenCvSharp.Rect(v.Bounds.X, v.Bounds.Y, v.Bounds.Width, v.Bounds.Height), Scalar.DarkRed, 1, LineTypes.Link8, 0);
                        //}

                        // Convert Mat to Stream and display
                        //using (var memoryStream = new MemoryStream())
                        //{
                        //    Cv2.ImEncode(".png", frameClone, out var imageData);
                        //    //memoryStream.Write(imageData, 0, imageData.Length);
                        //    //memoryStream.Position = 0;

                        //    MainThread.BeginInvokeOnMainThread(() =>
                        //    {
                        //        videoDisplay.Source = ImageSource.FromStream(() => new MemoryStream(imageData));
                        //        //SaveImageToFileAsync(new MemoryStream(imageData), "C:\\Users\\Tushar\\Downloads\\test_image.png");
                        //    });
                        //}

                        // Check which objects meet the detection threshold
                        foreach (var item in detectionCounts)
                        {
                            if (item.Value.ConsecutiveCount >= detectionThreshold)
                            {
                                // Object detected consistently
                                Console.WriteLine($"Object '{item.Key}' consistently detected for {item.Value.ConsecutiveCount} frames.");
                            }
                        }

                        writer.Write(frame);
                    }

                    // Performance metrics
                    var endTime = DateTime.Now;
                    var actualDuration = (endTime - startTime).TotalSeconds;
                    var actualFPS = frameCounter / actualDuration;

                    Console.WriteLine($"Actual Duration: {actualDuration} seconds");
                    Console.WriteLine($"Actual FPS: {actualFPS}");
                }
            }
        }


        public void SaveImageToFileAsync(MemoryStream imageStream, string filePath)
        {
            try
            {
                // Reset the position of MemoryStream to the beginning
                imageStream.Position = 0;

                // Create a new file stream to write the data
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    // Copy the MemoryStream to the FileStream
                    imageStream.CopyTo(fileStream);
                }

                // The file has been saved to the specified path
                Console.WriteLine("Image saved to " + filePath);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine("Error saving image: " + ex.Message);
            }
        }

        public void UpdateDetectionCounts(DetectionResultCustom detectionResult, Dictionary<string, ObjectDetectionInfo> detectionCounts, int detectionThreshold)
        {
            IBoundingBox a = null;
            // Reset or increment counts based on current detections
            foreach (var detection in detectionResult.Boxes)
            {
                if (detectionCounts.TryGetValue(detection.Class.Name, out var info))
                {
                    info.ConsecutiveCount++;
                }
                else
                {
                    detectionCounts[detection.Class.Name] = new ObjectDetectionInfo { ClassName = detection.Class.Name, ConsecutiveCount = 1 };
                }
            }

            // Decrease count for objects not detected in this frame
            foreach (var key in detectionCounts.Keys.ToList())
            {
                if (!detectionResult.Boxes.Any(box => box.Class.Name == key))
                {
                    detectionCounts[key].ConsecutiveCount--;
                    if (detectionCounts[key].ConsecutiveCount < 0)
                        detectionCounts[key].ConsecutiveCount = 0;
                }
            }
        }
    }

    
}


