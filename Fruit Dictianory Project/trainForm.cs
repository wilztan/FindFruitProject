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
using Accord.Statistics.Analysis;

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
        List<string> inputLabel = new List<string>();
        List<double[]> outputImgArray = new List<double[]>();

        /*
         * For Activation Network
         */
        ActivationNetwork an;
        BackPropagationLearning bpn;
        DistanceNetwork dn;
        PrincipalComponentAnalysis pca;
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
                    imageToArray.Convert(mainForm.Preprocess(img), out imageAsArray);
                    var output = new double[totalData];
                    inputLabel.Add(label);
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
                    Console.WriteLine($"Report error BPN {i} : {error}");
                }
            }
            an.Save(savedANNetwork);
        }

        void Clustering()
        {
            pca = new PrincipalComponentAnalysis();
            //pca
            pca.Learn(inputImgArray.ToArray());
            var pcaRes = pca.Transform(inputImgArray.ToArray());


            dn = new DistanceNetwork(pcaRes[0].Length, mainForm.closestSquareNumber(totalData));
            som = new SOMLearning(dn);
            Console.WriteLine("Learning");

            for( var i = 0; i < maxEpoch; i++)
            {
                var error = som.RunEpoch(pcaRes);
                if(error< errorGoal)
                {
                    break;
                }
                if (i % 10 == 0)
                {
                    Console.WriteLine($"Report Cluster error {i} : {error}");
                }
            }
            dn.Save(savedDNNetwork);
        }

        

    }
}
