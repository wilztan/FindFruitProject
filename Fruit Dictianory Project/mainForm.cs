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
using Accord.Imaging;
using Accord.Neuro.Learning;
using Accord.Imaging.Filters;
using Accord.Statistics.Analysis;

namespace Fruit_Dictianory_Project
{
    public partial class mainForm : Form
    {
        ActivationNetwork an;
        DistanceNetwork dn;
        PrincipalComponentAnalysis pca;
        static string savedANNetwork = "ActivationNetwork.bin";
        static string savedDNNetwork = "DistanceNetwork.bin";

        public mainForm()
        {
            InitializeComponent();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnTrain_Click(object sender, EventArgs e)
        {
            var formTrain = new trainForm();
            formTrain.ShowDialog();
        }

        private void mainForm_Load(object sender, EventArgs e)
        {

        }
        
        //Going to Quiz Form
        private void btnQuiz_Click(object sender, EventArgs e)
        {
            //validate existence of files
            if (System.IO.File.Exists(savedANNetwork))
            {
                //load Networks
                an = (ActivationNetwork)ActivationNetwork.Load(savedANNetwork);
                //go to form
                var form = new quizForm(an);
                form.ShowDialog();
            }
            else
            {
                MessageBox.Show($"{savedANNetwork} not found");
            }
        }
        
        //Going to Find Form
        private void btnFind_Click(object sender, EventArgs e)
        {
            //validate existence of files
            if (System.IO.File.Exists(savedDNNetwork))
            {
                //Load Networks
                dn = (DistanceNetwork)DistanceNetwork.Load(savedDNNetwork);
                //Go to Form
                var form = new findForm(dn);
                form.ShowDialog();
            }
            else
            {
                MessageBox.Show($"{savedDNNetwork} not Found");
            }
        }


        /*
         * General Function for usage in any form
         */
         //Preprocess
        public static Bitmap Preprocess(Bitmap img)
        {
            Bitmap clone = (Bitmap)img.Clone();
            //Grayscale
            clone = Grayscale.CommonAlgorithms.BT709.Apply(clone);
            //Treshold
            clone = (new Threshold(127)).Apply(clone);
            //Edge Detector
            clone = (new HomogenityEdgeDetector()).Apply(clone);
            //noise Reduction
            clone = preprocessNoise(clone);
            //resize
            clone = new ResizeBilinear(25, 25).Apply(clone);
            return clone;

        }

        //Noise Reduction
        public static Bitmap preprocessNoise(Bitmap img)
        {
            var clone = (Bitmap)img.Clone();
            int xMin = clone.Width, xMax = 0;
            int yMin = clone.Height, yMax = 0;
            for (int i = 0; i < clone.Height; i++)
            {
                for (int j = 0; j < clone.Width; j++)
                {
                    if (clone.GetPixel(j, i).R > 127)
                    {
                        xMin = Math.Min(xMin, j);
                        xMax = Math.Max(xMax, j);
                        yMin = Math.Min(yMin, i);
                        yMax = Math.Max(yMax, i);
                    }
                }
            }
            if (xMin == clone.Width) xMin = 0;
            if (xMax == 0) xMax = clone.Width;
            if (yMin == clone.Height) yMin = 0;
            if (yMax == 0) yMax = clone.Width;

            clone = (Bitmap)clone.Clone(
                new Rectangle(
                    xMin, 
                    yMin, 
                    xMax - xMin, 
                    yMax - yMin
                    ),
                clone.PixelFormat
                );

            return clone;
        }
        //closestMeanSquare
        public static int closestSquareNumber(int n)
        {
            return (int)Math.Pow(Math.Round(Math.Sqrt(n)), 2);

        }
    }
}
