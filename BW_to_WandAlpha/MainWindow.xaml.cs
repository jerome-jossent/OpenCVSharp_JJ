using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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
        Mat source;
        Dictionary<string, Mat> mats;

        #region BINDING
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public string _folder_IN
        {
            get { return folder_IN; }
            set
            {
                if (folder_IN != value)
                {
                    folder_IN = value;
                    OnPropertyChanged("_folder_IN");
                }
            }
        }
        string folder_IN = @"D:\C#\OpenCVSharpJJ\data";

        public string _folder_OUT
        {
            get { return folder_OUT; }
            set
            {
                if (folder_OUT != value)
                {
                    folder_OUT = value;
                    OnPropertyChanged("_folder_OUT");
                }
            }
        }
        string folder_OUT = @"D:\C#\OpenCVSharpJJ\data2";
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            sld_b.Value = 255;
            sld_g.Value = 0;
            sld_r.Value = 255;
        }

        void btn_ReadDirectory_click(object sender, RoutedEventArgs e)
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
                Title = lb.Items.Count + " / " + fichiers.Length;
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

            int seuil1 = (int)filter.LowerValue;
            int seuil2 = (int)filter.HigherValue;

            //if (seuil1 > 0 || seuil2 < 255)
            //{//passe bande : on ne garde que ce qui est au dessus de seuil1 et en dessous de seuil2:
            //    //en travaux
            //    gray = gray.Threshold(seuil1, seuil2, ThresholdTypes.Otsu | ThresholdTypes.Binary);
            //}

            Mat gray_not = new Mat();
            Cv2.BitwiseNot(gray, gray_not);

            Mat[] bgra = new Mat[] { Mat.Ones(src.Size(),MatType.CV_8UC1) * sld_b.Value,
                                     Mat.Ones(src.Size(),MatType.CV_8UC1) * sld_g.Value,
                                     Mat.Ones(src.Size(),MatType.CV_8UC1) * sld_r.Value,
                                     (ckb_not_a.IsChecked==true)? gray_not:gray};

            //Mat[] bgra = new Mat[] { gray_not,
            //                         gray_not,
            //                         gray_not,
            //                         gray_not};

            //Mat[] bgra = new Mat[] { gray,
            //                         gray,
            //                         gray,
            //                         gray_not};

            Mat newmat = new Mat();
            Cv2.Merge(bgra, newmat);
            return newmat;
        }

        void slds_management(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            tb_b.Text = "Blue  : " + sld_b.Value;
            tb_g.Text = "Green : " + sld_g.Value;
            tb_r.Text = "Red   : " + sld_r.Value;
            if (source != null)
                ProcessOnSelectedItem();
        }

        private void ckb_management(object sender, RoutedEventArgs e)
        {
            if (source != null)
                ProcessOnSelectedItem();
        }

        void filter_values_Changed(object sender, RoutedEventArgs e)
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

        void btn_SaveSelected_click(object sender, RoutedEventArgs e)
        {
            ListBoxItem item = (ListBoxItem)lb.SelectedItem;
            string path = item.ToolTip.ToString();
            Mat mat = mats[path];
            Mat mat_out = ImageProcessing(mat);

            string filename = System.IO.Path.GetFileNameWithoutExtension(path);

            mat_out.SaveImage(folder_OUT + "\\" + filename + ".png");
        }


    }
}