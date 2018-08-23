using Accord.Neuro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Accord;
using Accord.Statistics;
using Accord.Neuro.Learning;
using Accord.Math;
using Accord.Imaging;
using Accord.Imaging.Converters;
using Accord.Imaging.Filters;
using System.IO;

namespace Fruit_Dictianory_Project
{
    public partial class trainForm : Form
    {

        static string savedANNetwork = "ActivationNetwork.bin";
        static string savedDNNetwork = "DistanceNetwork.bin";

        ImageList imgList = new ImageList();
        string savedDirectoryName ="assets\\";

        /**
         * Image Files
         */
        List<double[]> inputImgArray = new List<double[]>();
        List<double[]> outputImgArray = new List<double[]>();

        /*
         * For Activation Network
         */
        ActivationNetwork an;
        BackPropagationLearning bpn;
        DistanceNetwork dn;
        SOMLearning som;
        int inputCount = 25 * 25;
        int outputCount = 0; //changeable
        int totalData;

        /*
         * For General Purpose of Training
         */
        int hidden = 3;
        double errorGoal = 0.0001;
        double maxEpoch = 100;
        


        public trainForm()
        {
            InitializeComponent();
            btnDone.Enabled = false;
            lvTrain.LargeImageList = imgList;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void trainForm_Load(object sender, EventArgs e)
        {

        }

        /***
         * Exit Function
         */
        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /*
         * Browse The Files
         */
        private void btnTrainBrowse_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            //filter Images
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.bmp;";
            //Multiselect
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                //for every images
                foreach (var images in ofd.FileNames)
                {
                    imgList.Images.Add(images, new Bitmap(images));
                    var label = System.IO.Path.GetFileName(images);
                    lvTrain.Items.Add(label, images);
                }
            }
        }

        // Still can submit without images 
        // the text doesn't say anything about it
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            var label = textFruitName.Text;
            //validation
            if (label == "")
            {
                MessageBox.Show("Please Fill The Name");
            }
            else
            {
                //save to directory
                for (var i = 0; i < imgList.Images.Count; i++)
                {
                    string fileName = Path.GetFileName(imgList.Images.Keys[i].ToString());
                    string dir = $"{savedDirectoryName}{label}";
                    Directory.CreateDirectory(dir); 
                    imgList.Images[i].Save($"{dir}\\{fileName}");
                }

                // enable everything
                textFruitName.Text = "";
                clear();
                btnDone.Enabled = true;
                MessageBox.Show("Item Saved");
            }
        }

        void clear()
        {
            imgList.Images.Clear();
            lvTrain.Clear();
        }

        // The text doesn't say anything about the clear button
        private void btnClear_Click(object sender, EventArgs e)
        {
            clear();
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            totalData = Directory.GetDirectories(savedDirectoryName).Length;
            Console.WriteLine($"Total Data : {totalData}");
            var counter = 0;
            foreach (var subFolder in Directory.GetDirectories(savedDirectoryName))
            {
                var label = new DirectoryInfo(subFolder).Name;
                ImageToArray imageToArray = new ImageToArray();
                foreach(var files in Directory.GetFiles(subFolder))
                {
                    var img = new Bitmap(files);
                    double[] imageAsArray;
                    imageToArray.Convert(Preprocess(img), out imageAsArray);
                    var output = new double[totalData];
                    Array.Clear(output, 0, totalData);
                    output[counter] = 1;
                    inputImgArray.Add(imageAsArray);
                    outputImgArray.Add(output);
                    Console.WriteLine("Result Output");
                    output.ToList().ForEach(Console.WriteLine);
                }
                Console.WriteLine("Item Done");
                counter++;
                outputCount++;
            }
            BackPropagationMethod();
            Clustering();
            MessageBox.Show("Trained Succesfully");
            this.Close();
        }

        void BackPropagationMethod()
        {
            
            an = new ActivationNetwork(
                new SigmoidFunction(),
                inputCount,
                hidden,
                outputCount
                );
            bpn = new BackPropagationLearning(an);
            new NguyenWidrow(an).Randomize();

            Console.WriteLine("Learning");
            for (var i = 0; i < maxEpoch; i++)
            {
                var error = bpn.RunEpoch(inputImgArray.ToArray(),outputImgArray.ToArray());
                if (error < errorGoal)
                {
                    break;
                }
                if (i % 10 == 0)
                {
                    Console.WriteLine($"Report error {i} : {error}");
                }
            }
            an.Save(savedANNetwork);
        }

        void Clustering()
        {
            dn = new DistanceNetwork(inputCount, closestSquareNumber(totalData));
            som = new SOMLearning(dn);
            Console.WriteLine("Learning");

            for( var i = 0; i < maxEpoch; i++)
            {
                var error = som.RunEpoch(inputImgArray.ToArray());
                if(error< errorGoal)
                {
                    break;
                }
                if (i % 10 == 0)
                {
                    Console.WriteLine($"Report error {i} : {error}");
                }
            }
            dn.Save(savedDNNetwork);
        }

        int closestSquareNumber(int n)
        {
            return (int) Math.Pow(Math.Round(Math.Sqrt(n)), 2);

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
