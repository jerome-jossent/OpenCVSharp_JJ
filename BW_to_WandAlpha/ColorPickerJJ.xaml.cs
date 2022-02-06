using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace BW_to_WandAlpha
{
    public partial class ColorPickerJJ : UserControl
    {
        Mat colors;
        bool colorpickerON = false;

        public int _Target_Length { get; set; }

        public Color _Color { get => color; set => color = value; }
        private Color color = Color.FromRgb(0, 0, 0);

        public class NewColorEventArgs : EventArgs
        {
            public Color color { get; private set; }
            public NewColorEventArgs(Color color)
            {
                this.color = color;
            }
        }
        public event EventHandler<NewColorEventArgs> _ColorNew;

        SolidColorBrush white = new SolidColorBrush(Colors.White);
        SolidColorBrush black = new SolidColorBrush(Colors.Black);

        public ColorPickerJJ()
        {
            InitializeComponent();

            ComputeMatColors();
        }

        void ComputeMatColors()
        {
            //HSV, hue range is [0, 359], saturation range is [0, 1], and value range is [0, 1]
            int hue = 360;
            int sat = 256; // intensité de couleur 0=blanc, 1=couleur max
            int val = 256; // intensité lumineuse  0=noir, 1=blanc
            colors = new Mat(size: new OpenCvSharp.Size(hue, sat + val), type: MatType.CV_8UC3);
            for (int x = 0; x < hue; x++)
            {
                //saturation
                for (int y = 0; y < sat; y++)
                {
                    HSV hsv = new HSV(h: x,
                                      s: (double)y / sat,
                                      v: 1
                                      );

                    RGB rgb = HSVToRGB(hsv);
                    Vec3b px = new Vec3b(rgb.B, rgb.G, rgb.R);

                    colors.Set(y, x, px);
                }

                //value
                for (int y = 0; y < val; y++)
                {
                    HSV hsv = new HSV(h: x,
                                      s: 1,
                                      v: (double)(val - y) / val
                                      );

                    RGB rgb = HSVToRGB(hsv);
                    Vec3b px = new Vec3b(rgb.B, rgb.G, rgb.R);

                    colors.Set(y + 256, x, px);//offset spatial ici !!
                }
            }

            _Colors.Source = conversion.ToImageSource(colors);
        }

        #region HSV & RGB
        public struct RGB
        {
            private byte _r;
            private byte _g;
            private byte _b;

            public RGB(byte r, byte g, byte b)
            {
                this._r = r;
                this._g = g;
                this._b = b;
            }

            public byte R
            {
                get { return this._r; }
                set { this._r = value; }
            }

            public byte G
            {
                get { return this._g; }
                set { this._g = value; }
            }

            public byte B
            {
                get { return this._b; }
                set { this._b = value; }
            }

            public bool Equals(RGB rgb)
            {
                return (this.R == rgb.R) && (this.G == rgb.G) && (this.B == rgb.B);
            }
        }

        public struct HSV
        {
            private double _h;
            private double _s;
            private double _v;

            public HSV(double h, double s, double v)
            {
                this._h = h;
                this._s = s;
                this._v = v;
            }

            public double H
            {
                get { return this._h; }
                set { this._h = value; }
            }

            public double S
            {
                get { return this._s; }
                set { this._s = value; }
            }

            public double V
            {
                get { return this._v; }
                set { this._v = value; }
            }

            public bool Equals(HSV hsv)
            {
                return (this.H == hsv.H) && (this.S == hsv.S) && (this.V == hsv.V);
            }
        }

        public static RGB HSVToRGB(HSV hsv)
        {
            double r = 0, g = 0, b = 0;

            if (hsv.S == 0)
            {
                r = hsv.V;
                g = hsv.V;
                b = hsv.V;
            }
            else
            {
                int i;
                double f, p, q, t;

                if (hsv.H == 360)
                    hsv.H = 0;
                else
                    hsv.H = hsv.H / 60;

                i = (int)Math.Truncate(hsv.H);
                f = hsv.H - i;

                p = hsv.V * (1.0 - hsv.S);
                q = hsv.V * (1.0 - (hsv.S * f));
                t = hsv.V * (1.0 - (hsv.S * (1.0 - f)));

                switch (i)
                {
                    case 0:
                        r = hsv.V;
                        g = t;
                        b = p;
                        break;

                    case 1:
                        r = q;
                        g = hsv.V;
                        b = p;
                        break;

                    case 2:
                        r = p;
                        g = hsv.V;
                        b = t;
                        break;

                    case 3:
                        r = p;
                        g = q;
                        b = hsv.V;
                        break;

                    case 4:
                        r = t;
                        g = p;
                        b = hsv.V;
                        break;

                    default:
                        r = hsv.V;
                        g = p;
                        b = q;
                        break;
                }
            }
            return new RGB((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }
        #endregion

        void Colors_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            colorpickerON = true;
        }

        void Colors_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            colorpickerON = false;
        }

        void Colors_MouseMove(object sender, MouseEventArgs e)
        {
            if (!colorpickerON) return;

            System.Windows.Point ui_pos = e.GetPosition(_Colors);

            double prop_x = ui_pos.X / _ColorsGrid.ActualWidth;
            double prop_y = ui_pos.Y / _ColorsGrid.ActualHeight;

            _SetMouseSelection(prop_x, prop_y);
        }

        public void _SetMouseSelection(double prop_x, double prop_y)
        {
            //convert to colors Mat dimensions
            int x = (int)(prop_x * colors.Width);
            int y = (int)(prop_y * colors.Height);
            // read pixel 
            Vec3b px = colors.Get<Vec3b>(y, x);
            // make color
            color = Color.FromArgb(255, px[2], px[1], px[0]);
            //raise event new color
            _ColorNew?.Invoke(null, new NewColorEventArgs(color));
            if (_Target_Length > 0)
                _SetTarget(prop_x, prop_y);
        }

        void _SetTarget(double prop_x, double prop_y)
        {
            double x = prop_x * _ColorsGrid.ActualWidth;
            double y = prop_y * _ColorsGrid.ActualHeight;
            //positions
            _lineNO.Points = new PointCollection(new List<System.Windows.Point> { new System.Windows.Point(x - _Target_Length, y - 1), new System.Windows.Point(x - 1, y - 1), new System.Windows.Point(x - 1, y - _Target_Length) });
            _lineNE.Points = new PointCollection(new List<System.Windows.Point> { new System.Windows.Point(x + _Target_Length, y - 1), new System.Windows.Point(x + 1, y - 1), new System.Windows.Point(x + 1, y - _Target_Length) });
            _lineSE.Points = new PointCollection(new List<System.Windows.Point> { new System.Windows.Point(x - _Target_Length, y + 1), new System.Windows.Point(x - 1, y + 1), new System.Windows.Point(x - 1, y + _Target_Length) });
            _lineSO.Points = new PointCollection(new List<System.Windows.Point> { new System.Windows.Point(x + _Target_Length, y + 1), new System.Windows.Point(x + 1, y + 1), new System.Windows.Point(x + 1, y + _Target_Length) });

            //color
            bool isDark = color.R < 120 && color.G < 120 && color.B < 120;
            if (isDark)
                _lineNO.Stroke = white;
            else
                _lineNO.Stroke = black;

            _lineSO.Stroke = _lineSE.Stroke = _lineNE.Stroke = _lineNO.Stroke;
        }

        public void _SetColor(Color color)
        {
            System.Windows.Point point = FindColor(color);
            _SetMouseSelection(point.X / colors.Width, point.Y / colors.Height);
        }

        System.Windows.Point FindColor(Color color)
        {
            System.Windows.Point point = new System.Windows.Point();
            for (int x = 0; x < colors.Width; x++)
                for (int y = 0; y < colors.Height; y++)
                {
                    // read pixel 
                    Vec3b px = colors.Get<Vec3b>(y, x);
                    if (px.Item0 == color.B &&
                        px.Item1 == color.G &&
                        px.Item2 == color.R)
                        return new System.Windows.Point(x, y);
                }
            return point;
        }
    }
}