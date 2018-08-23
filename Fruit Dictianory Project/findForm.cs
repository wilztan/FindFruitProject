using Accord.Imaging.Converters;
using Accord.Imaging.Filters;
using Accord.Neuro;
using Accord.Neuro.Learning;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fruit_Dictianory_Project
{
    public partial class findForm : Form
    {
        DistanceNetwork dn;
        SOMLearning som;
        List<string> resultName = new List<string>();
        public static string savedDirectoryName = "assets\\";
        ImageList imgList = new ImageList();
        

        public findForm(DistanceNetwork dn)
        {
            InitializeComponent();
            this.dn = dn;
            som = new SOMLearning(this.dn);
            getResult();
            listView1.LargeImageList = imgList;
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image File|*.jpg;*.bmp;*.jpeg";
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                clearItem();
                Bitmap img = new Bitmap(ofd.FileName);
                pictureBrowsed.Image = img;
                img = Preprocess(img);
                ImageToArray imageToArray = new ImageToArray();
                double[] input;
                imageToArray.Convert(img, out input);
                dn.Compute(input);
                var index = dn.GetWinner();
                MessageBox.Show(resultName[index]);

                foreach (var subFolder in Directory.GetDirectories(savedDirectoryName))
                {
                    foreach (var files in Directory.GetFiles(subFolder))
                    {
                        var label = new DirectoryInfo(subFolder).Name;
                        if (label == resultName[index])
                        {
                            Bitmap imgs = new Bitmap(files);

                            imgList.Images.Add(files, imgs);
                            listView1.Items.Add(label,files);
                        }
                    }
                }
            }
        }

        void clearItem()
        {
            imgList.Images.Clear();
            listView1.Clear();
        }

        void getResult()
        {

            foreach (var folder in Directory.GetDirectories(savedDirectoryName))
            {
                string label = new DirectoryInfo(folder).Name;
                if (resultName.Any())
                {
                    resultName.Add(label);
                }
                else if (!resultName.Contains(label))
                {
                    resultName.Add(label);
                }
            }
        }


        Bitmap Preprocess(Bitmap img)
        {
            Bitmap clone = (Bitmap)img.Clone();
            //Grayscale
            clone = Grayscale.CommonAlgorithms.BT709.Apply(clone);
            //Treshold
            clone = (new Threshold(127)).Apply(clone);
            //Edge Detector
            clone = (new HomogenityEdgeDetector()).Apply(clone);
            //resize
            clone = new ResizeBilinear(25, 25).Apply(clone);
            return clone;
        }
    }
}
