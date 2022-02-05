using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
//using System.Drawing;
using System.Linq;
using System.Text;
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
    public partial class MainWindow : System.Windows.Window, INotifyPropertyChanged
    {
        const string titre = "Black and White → White and Alpha (Transparent)";
        Mat source;
        Dictionary<string, Mat> mats;
        System.Windows.Media.Color color = Color.FromRgb(0, 0, 0);
        Mat colors;

        #region BINDING
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public string _folder_IN
        {
            get { return Properties.Settings.Default["folder_IN"].ToString(); }
            set
            {
                Properties.Settings.Default["folder_IN"] = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged("_folder_IN");
            }
        }

        public string _folder_OUT
        {
            get { return Properties.Settings.Default["folder_OUT"].ToString(); }
            set
            {
                Properties.Settings.Default["folder_OUT"] = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged("_folder_OUT");
            }
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            colors = new Mat("Images\\colorpicker.png");
            _colorNew += MainWindow__colorNew; ;
        }

        private void btn_ReadDirectory_click(object sender, MouseButtonEventArgs e)
        {
            mats = new Dictionary<string, Mat>();

            if (!System.IO.Directory.Exists(_folder_IN))
                return;
            string[] fichiers = System.IO.Directory.GetFiles(_folder_IN);
            lb.Items.Clear();
            foreach (string fichier in fichiers)
            {
                Mat mat = new Mat(fichier);
                source = mat;
                mats.Add(fichier, mat);
                lb.Items.Add(newItem(mat, fichier));
                //Application.Current.
                //Dispatcher.BeginInvoke(new Action(() =>                    {
                Title = titre + " - " + lb.Items.Count + " / " + fichiers.Length;
                //}));
                System.Threading.Thread.Sleep(1);
            }
        }

        ListBoxItem newItem(Mat mat, string fichier)
        {
            ListBoxItem item = new ListBoxItem();
            System.Windows.Controls.Image img = new System.Windows.Controls.Image();
            img.Source = conversion.ToImageSource(mat);
            img.Height = 100;
            img.Stretch = Stretch.Uniform;
            item.Content = img;
            item.ToolTip = fichier;
            return item;
        }

        Mat ImageProcessing(Mat src)
        {
            Mat gray = new Mat();
            Cv2.CvtColor(src, gray, ColorConversionCodes.RGB2GRAY);

            Mat gray_not = new Mat();
            if (ckb_not_a.IsChecked == true)
                Cv2.BitwiseNot(gray, gray_not);

            Mat[] bgra = new Mat[] { Mat.Ones(src.Size(),MatType.CV_8UC1) * color.B,
                                     Mat.Ones(src.Size(),MatType.CV_8UC1) * color.G,
                                     Mat.Ones(src.Size(),MatType.CV_8UC1) * color.R,
                                     (ckb_not_a.IsChecked==true)? gray_not:gray};

            Mat newmat = new Mat();
            Cv2.Merge(bgra, newmat);
            return newmat;
        }

        private void ckb_management(object sender, RoutedEventArgs e)
        {
            if (source != null)
                ProcessOnSelectedItem();
        }

        void lb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProcessOnSelectedItem();
        }

        void ProcessOnSelectedItem()
        {
            if (lb.SelectedItem == null) return;
            ListBoxItem item = (ListBoxItem)lb.SelectedItem;
            Mat mat = mats[item.ToolTip.ToString()];
            img_before.Source = conversion.ToImageSource(mat);
            Mat mat_out = ImageProcessing(mat);
            img_after.Source = conversion.ToImageSource(mat_out);
        }

        private void btn_SaveSelected_click(object sender, MouseButtonEventArgs e)
        {
            string msg;
            try
            {
                ListBoxItem item = (ListBoxItem)lb.SelectedItem;
                string path = item.ToolTip.ToString();
                Mat mat = mats[path];
                Mat mat_out = ImageProcessing(mat);

                string filename = System.IO.Path.GetFileNameWithoutExtension(path);
                string fullfilename = _folder_OUT + "\\" + filename + ".png";
                mat_out.SaveImage(fullfilename);
                msg = "File created : \n\n" + fullfilename;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            MessageBox.Show(msg);
        }

        void Colors_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MouseMove += Colors_MouseMove;
            MouseLeftButtonUp += Colors_MouseLeftButtonUp;
        }

        void Colors_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MouseMove -= Colors_MouseMove;
            MouseLeftButtonUp -= Colors_MouseLeftButtonUp;
        }

        public class NewColorEventArgs : EventArgs
        {
            public Color color { get; private set; }
            public NewColorEventArgs(Color color)
            {
                this.color = color;
            }
        }

        public event EventHandler<NewColorEventArgs> _colorNew;

        void Colors_MouseMove(object sender, MouseEventArgs e)
        {
            //relative position
            System.Windows.Point ui_pos = e.GetPosition(_Colors);
            int x = (int)(ui_pos.X / _ColorsGrid.ActualWidth * colors.Width);
            int y = (int)(ui_pos.Y / _ColorsGrid.ActualHeight * colors.Height);

            //get pixel
            Vec3b px = colors.Get<Vec3b>(y, x);

            //read pixel to make color
            Color color = Color.FromArgb(255, px[2], px[1], px[0]);
            _SetTarget(ui_pos);
            this.color = color;
            _colorNew?.Invoke(null, new NewColorEventArgs(color));
        }

        private void _SetTarget(System.Windows.Point ui_pos)
        {
            int d = 10;
            double x = ui_pos.X;
            double y = ui_pos.Y;
            _lineNO.Points = new PointCollection(new List<System.Windows.Point> { new System.Windows.Point(x - d, y - 1), new System.Windows.Point(x - 1, y - 1), new System.Windows.Point(x - 1, y - d) });
            _lineNE.Points = new PointCollection(new List<System.Windows.Point> { new System.Windows.Point(x + d, y - 1), new System.Windows.Point(x + 1, y - 1), new System.Windows.Point(x + 1, y - d) });
            _lineSE.Points = new PointCollection(new List<System.Windows.Point> { new System.Windows.Point(x - d, y + 1), new System.Windows.Point(x - 1, y + 1), new System.Windows.Point(x - 1, y + d) });
            _lineSO.Points = new PointCollection(new List<System.Windows.Point> { new System.Windows.Point(x + d, y + 1), new System.Windows.Point(x + 1, y + 1), new System.Windows.Point(x + 1, y + d) });
        }

        private void MainWindow__colorNew(object sender, NewColorEventArgs e)
        {         
            _colorRGB.Content = "RGB (" + e.color.R + ", " + e.color.G + ", " + e.color.B + ")";
            _ColorPicker.Background = new SolidColorBrush(e.color);
            ProcessOnSelectedItem();
        }
    }
}