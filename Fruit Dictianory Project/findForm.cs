using Accord.Imaging.Converters;
using Accord.Imaging.Filters;
using Accord.Neuro;
using Accord.Neuro.Learning;
using Accord.Statistics.Analysis;
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
        //public variable
        PrincipalComponentAnalysis pca;
        DistanceNetwork dn;
        SOMLearning som;
        List<string> resultName = new List<string>();
        public static string savedDirectoryName = "assets\\";
        ImageList imgList = new ImageList();
        List<double[]> trainedData = new List<double[]>();
        List<List<string>> classMap = new List<List<string>>();
        List<string> inputLabel = new List<string>();
        int totalData;

        //initialize
        public findForm(DistanceNetwork dn)
        {
            InitializeComponent();
            this.dn = dn;
            listView1.LargeImageList = imgList;

            //Predicting Method
            som = new SOMLearning(this.dn);
            init();
            getTrainedDataArray();
            getClassMap();
            classMapChecker();
        }

        //back
        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //here is where the magic starts
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
                img = mainForm.Preprocess(img);
                ImageToArray imageToArray = new ImageToArray();
                double[] input;
                imageToArray.Convert(img, out input);
                var pcares = pca.Transform(input);
                dn.Compute(pcares);
                var index = dn.GetWinner();
                var similarIndex = classMap[index];
                foreach(var item in similarIndex)
                {
                    showItem(item);
                }

            }
        }

        //clearance for item in imagelist and listview
        void clearItem()
        {
            imgList.Images.Clear();
            listView1.Clear();
        }

        //initialize item
        void init()
        {
            pca = new PrincipalComponentAnalysis();
            totalData = Directory.GetDirectories(savedDirectoryName).Length;
            Console.WriteLine($"Total Data : {totalData}");
        }

        //valuing class of item
        void getClassMap()
        {
            for (int i = 0; i < mainForm.closestSquareNumber(totalData); i++)
            {
                classMap.Add(new List<string>());
            }
            for (int a = 0; a < inputLabel.Count; a++)
            {
                var data = trainedData[a];
                var label = inputLabel[a];
                pca.Learn(trainedData.ToArray());
                var pcaOutput = pca.Transform(data);
                dn.Compute(pcaOutput);
                var index = dn.GetWinner();
                Console.WriteLine($"{label} winner {index}");
                if (!classMap[index].Contains(label))
                {
                    classMap[index].Add(label);
                }
            }
        }
        
        //setting up trained data
        void getTrainedDataArray()
        {
            foreach (var subFolder in Directory.GetDirectories(savedDirectoryName))
            {
                var label = new DirectoryInfo(subFolder).Name;
                ImageToArray imageToArray = new ImageToArray();
                foreach (var files in Directory.GetFiles(subFolder))
                {
                    var img = new Bitmap(files);
                    double[] imageAsArray;
                    imageToArray.Convert(mainForm.Preprocess(img), out imageAsArray);
                    trainedData.Add(imageAsArray);
                    inputLabel.Add(label);
                }
            }
        }

        //show every item met the requirement
        void showItem(string index)
        {
            foreach (var subFolder in Directory.GetDirectories(savedDirectoryName))
            {
                foreach (var files in Directory.GetFiles(subFolder))
                {
                    var label = new DirectoryInfo(subFolder).Name;
                    if (label == index)
                    {
                        Bitmap imgs = new Bitmap(files);

                        imgList.Images.Add(files, imgs);
                        listView1.Items.Add(label, files);
                    }
                }
            }
        }

        // just for checking purpose
        void classMapChecker()
        {
            Console.WriteLine($"class Map Count {classMap.Count()}");
            var c = 0;
            var b = 0;
            foreach (var item in classMap)
            {
                b = 0;
                foreach (var a in item)
                {
                    b++;
                    Console.WriteLine($"hi item {a} is on Winner value {b} on itteration {c}");
                }
                c++;
            }
        }
    }
}
