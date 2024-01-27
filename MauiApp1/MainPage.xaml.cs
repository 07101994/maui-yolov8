using Compunet.YoloV8;
using Compunet.YoloV8.Plotting;

//using static Android.Graphics.ColorSpace;
using SixLabors.ImageSharp;

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
            
            using var predictor = new YoloV8(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models", "yolov8n-cls.onnx"));

            var result = predictor.Detect("C:\\Users\\Tushar\\Pictures\\IMG_20220310_000632_Bokeh.jpg");
            // or
            var test_image = SixLabors.ImageSharp.Image.Load("C:\\Users\\Tushar\\Pictures\\Test image.jpg");

            var ploted = result.PlotImage(test_image);
            ploted.Save("C:\\Users\\Tushar\\Pictures\\test2.jpg");
            var classify_result = predictor.Classify("C:\\Users\\Tushar\\Pictures\\IMG_20220310_000632_Bokeh.jpg");

            classify_result.PlotImage(test_image);
            Console.WriteLine(result);
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
    }

}
