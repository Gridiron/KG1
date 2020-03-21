using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing.IndexedProperties;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Globalization;
using System.Windows.Controls.Primitives;
using Color = System.Windows.Media.Color;

namespace KG1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public static int[] ConvertCmykToRgb(double c, double m, double y, double k)
        {
            var r = Convert.ToInt32(255 * (1 - c) * (1 - k));
            var g = Convert.ToInt32(255 * (1 - m) * (1 - k));
            var b = Convert.ToInt32(255 * (1 - y) * (1 - k));

            return new []{r,g,b};
        }


        public static double[] ConvertRgbToCmyk(int r, int g, int b)
        {
            double rf = r / 255F;
            double gf = g / 255F;
            double bf = b / 255F;

            var k = ClampCmyk(1 - Math.Max(Math.Max(rf, gf), bf));
            var c = ClampCmyk((1 - rf - k) / (1 - k));
            var m = ClampCmyk((1 - gf - k) / (1 - k));
            var y = ClampCmyk((1 - bf - k) / (1 - k));

            return new[]{ c, m, y, k };
        }

        private static double ClampCmyk(double value)
        {
            if (value < 0 || double.IsNaN(value))
            {
                value = 0;
            }

            return value;
        }



        // Convert an RGB value into an HLS value.
        public static double[] RgbToHls(int r, int g, int b)
        {
            double h;
            double l;
            double s;
            // Convert RGB to a 0.0 to 1.0 range.
            var doubleR = r / 255.0;
            var doubleG = g / 255.0;
            var doubleB = b / 255.0;

            // Get the maximum and minimum RGB components.
            var max = doubleR;
            if (max < doubleG) max = doubleG;
            if (max < doubleB) max = doubleB;

            var min = doubleR;
            if (min > doubleG) min = doubleG;
            if (min > doubleB) min = doubleB;

            var diff = max - min;
            l = (max + min) / 2;
            if (Math.Abs(diff) < 0.00001)
            {
                s = 0;
                h = 0;  // H is really undefined.
            }
            else
            {
                if (l <= 0.5) s = diff / (max + min);
                else s = diff / (2 - max - min);

                var rDist = (max - doubleR) / diff;
                var gDist = (max - doubleG) / diff;
                var bDist = (max - doubleB) / diff;

                if (doubleR == max) h = bDist - gDist;
                else if (doubleG == max) h = 2 + rDist - bDist;
                else h = 4 + gDist - rDist;

                h = h * 60;
                if (h < 0) h += 360;
            }

            return new[] { h,l,s };
        }

        // Convert an HLS value into an RGB value.
        public static int[] HlsToRgb(double h, double l, double s)
        {
            int r;
            int g;
            int b;
            double p2;
            if (l <= 0.5) p2 = l * (1 + s);
            else p2 = l + s - l * s;

            double p1 = 2 * l - p2;
            double double_r, double_g, double_b;
            if (s == 0)
            {
                double_r = l;
                double_g = l;
                double_b = l;
            }
            else
            {
                double_r = QqhToRgb(p1, p2, h + 120);
                double_g = QqhToRgb(p1, p2, h);
                double_b = QqhToRgb(p1, p2, h - 120);
            }

            // Convert RGB to the 0 to 255 range.
            r = (int)(double_r * 255.0);
            g = (int)(double_g * 255.0);
            b = (int)(double_b * 255.0);

            return new[] {r, g, b};
        }

        private static double QqhToRgb(double q1, double q2, double hue)
        {
            if (hue > 360) hue -= 360;
            else if (hue < 0) hue += 360;

            if (hue < 60) return q1 + (q2 - q1) * hue / 60;
            if (hue < 180) return q2;
            if (hue < 240) return q1 + (q2 - q1) * (240 - hue) / 60;
            return q1;
        }

        private void DisplayHsl()
        {
            var rgb = HlsToRgb(HSL1Slider.Value, HSL2Slider.Value/100, HSL3Slider.Value/100);
            ColorPanel.Background = new SolidColorBrush(Color.FromRgb((byte)RGB1Slider.Value, (byte)RGB2Slider.Value, (byte)RGB3Slider.Value));
            var cmyk = ConvertRgbToCmyk((byte)RGB1Slider.Value, (byte)RGB2Slider.Value, (byte)RGB3Slider.Value);
            CMYK1Slider.Value = cmyk[0] * 100;
            CMYK2Slider.Value = cmyk[1] * 100;
            CMYK3Slider.Value = cmyk[2] * 100;
            CMYK4Slider.Value = cmyk[3] * 100;
            CLabel.Content = Math.Round(CMYK1Slider.Value, 2).ToString(CultureInfo.InvariantCulture);
            MLabel.Content = Math.Round(CMYK2Slider.Value, 2).ToString(CultureInfo.InvariantCulture);
            YLabel.Content = Math.Round(CMYK3Slider.Value, 2).ToString(CultureInfo.InvariantCulture);
            KLabel.Content = Math.Round(CMYK4Slider.Value, 2).ToString(CultureInfo.InvariantCulture);
            RGB1Slider.Value = rgb[0];
            RGB2Slider.Value = rgb[1];
            RGB3Slider.Value = rgb[2];
            RLabel.Content = rgb[0].ToString(CultureInfo.InvariantCulture);
            GLabel.Content = rgb[1].ToString(CultureInfo.InvariantCulture);
            BLabel.Content = rgb[2].ToString(CultureInfo.InvariantCulture);

            HLabel.Content = Math.Floor(HSL1Slider.Value).ToString(CultureInfo.InvariantCulture);
            LabelLabel.Content = Math.Round(HSL2Slider.Value, 2).ToString(CultureInfo.InvariantCulture);
            SLabel.Content = Math.Round(HSL3Slider.Value, 2).ToString(CultureInfo.InvariantCulture);
        }

        private void DisplayRgb()
        {
            ColorPanel.Background = new SolidColorBrush(Color.FromRgb((byte)RGB1Slider.Value, (byte)RGB2Slider.Value, (byte)RGB3Slider.Value));
            RLabel.Content = ((byte)RGB1Slider.Value).ToString(CultureInfo.InvariantCulture);
            GLabel.Content = ((byte)RGB2Slider.Value).ToString(CultureInfo.InvariantCulture);
            BLabel.Content = ((byte)RGB3Slider.Value).ToString(CultureInfo.InvariantCulture);
            var hls = RgbToHls((byte)RGB1Slider.Value, (byte)RGB2Slider.Value, (byte)RGB3Slider.Value);
            HSL1Slider.Value = hls[0];
            HSL2Slider.Value = hls[1] * 100;
            HSL3Slider.Value = hls[2] * 100;
            HLabel.Content = Math.Floor(hls[0]).ToString(CultureInfo.InvariantCulture);
            LabelLabel.Content = Math.Round(hls[1], 2).ToString(CultureInfo.InvariantCulture);
            SLabel.Content = Math.Round(hls[2], 2).ToString(CultureInfo.InvariantCulture);

            var cmyk = ConvertRgbToCmyk((byte)RGB1Slider.Value, (byte)RGB2Slider.Value, (byte)RGB3Slider.Value);
            CMYK1Slider.Value = cmyk[0] * 100;
            CMYK2Slider.Value = cmyk[1] * 100;
            CMYK3Slider.Value = cmyk[2] * 100;
            CMYK4Slider.Value = cmyk[3] * 100;
            CLabel.Content = Math.Round(cmyk[0] * 100, 2).ToString(CultureInfo.InvariantCulture);
            MLabel.Content = Math.Round(cmyk[1] * 100, 2).ToString(CultureInfo.InvariantCulture);
            YLabel.Content = Math.Round(cmyk[2] * 100, 2).ToString(CultureInfo.InvariantCulture);
            KLabel.Content = Math.Round(cmyk[3] * 100, 2).ToString(CultureInfo.InvariantCulture);
        }

        private void DisplayCmyk()
        {
            var rgb = ConvertCmykToRgb(CMYK1Slider.Value/100, CMYK2Slider.Value/100, CMYK3Slider.Value/100, CMYK4Slider.Value/100);
            ColorPanel.Background = new SolidColorBrush(Color.FromRgb((byte)RGB1Slider.Value, (byte)RGB2Slider.Value, (byte)RGB3Slider.Value));
            CLabel.Content = Math.Round(CMYK1Slider.Value, 2).ToString(CultureInfo.InvariantCulture);
            MLabel.Content = Math.Round(CMYK2Slider.Value, 2).ToString(CultureInfo.InvariantCulture);
            YLabel.Content = Math.Round(CMYK3Slider.Value, 2).ToString(CultureInfo.InvariantCulture);
            KLabel.Content = Math.Round(CMYK4Slider.Value, 2).ToString(CultureInfo.InvariantCulture);
            RGB1Slider.Value = rgb[0];
            RGB2Slider.Value = rgb[1];
            RGB3Slider.Value = rgb[2];
            RLabel.Content = rgb[0].ToString(CultureInfo.InvariantCulture);
            GLabel.Content = rgb[1].ToString(CultureInfo.InvariantCulture);
            BLabel.Content = rgb[2].ToString(CultureInfo.InvariantCulture);
            var hls = RgbToHls(rgb[0], rgb[1], rgb[2]);
            HSL1Slider.Value = hls[0];
            HSL2Slider.Value = hls[1] * 100;
            HSL3Slider.Value = hls[2] * 100;
            HLabel.Content = Math.Floor(hls[0]).ToString(CultureInfo.InvariantCulture);
            LabelLabel.Content = Math.Round(hls[1], 2).ToString(CultureInfo.InvariantCulture);
            SLabel.Content = Math.Round(hls[2], 2).ToString(CultureInfo.InvariantCulture);

        }
        private void RGB1Slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            DisplayRgb();
        }
        private void RGB2Slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            DisplayRgb();
        }

        private void RGB3Slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            DisplayRgb();
        }

        private void HSL2Slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            DisplayHsl();
        }

        private void HSL3Slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            DisplayHsl();
        }

        private void HSL1Slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            DisplayHsl();
        }

        private void CMYK1Slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            DisplayCmyk();
        }

        private void CMYK2Slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            DisplayCmyk();
        }

        private void CMYK3Slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            DisplayCmyk();
        }

        private void CMYK4Slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            DisplayCmyk();
        }


    }
}
