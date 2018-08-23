using Accord.Imaging.Converters;
using Accord.Imaging.Filters;
using Accord.Neuro;
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
    public partial class quizForm : Form
    {
        int score = 0;
        ActivationNetwork an;
        List<string> resultName = new List<string>();
        public static string savedDirectoryName = "assets\\";
        public quizForm(ActivationNetwork anNetwork)
        {
            InitializeComponent();
            this.an = anNetwork;
            getResult();
            NewQuiz();
        }

        int Randomizer(int min, int max)
        {
            Random rdm = new Random();
            return rdm.Next(min, max);
        }

        void NewQuiz()
        {
            Console.WriteLine($"Result Name: {resultName.Count()}");
            lblResult.Text = resultName[Randomizer(0, resultName.Count() - 1)] ;
            lblScoreRes.Text = $"Your Result : {score}";
        }

        void getResult()
        {
            
            foreach (var folder in Directory.GetDirectories(savedDirectoryName))
            {
                string label = new DirectoryInfo(folder).Name;
                if(resultName.Any())
                {
                    resultName.Add(label);
                }
                else if (!resultName.Contains(label)) {
                    resultName.Add(label);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Filter = "Image File|*.jpg;*.jpeg;*.bmp";
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                Bitmap img = new Bitmap(ofd.FileName);
                pictBoxResult.Image = img;
            }
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            Bitmap img = new Bitmap(pictBoxResult.Image);
            img = Preprocess(img);
            ImageToArray imageToArray = new ImageToArray();
            double[] input;
            imageToArray.Convert(img, out input);
            var res = an.Compute(input);
            var highest = 0; 
            for(var i  = 0; i < res.Length; i++)
            {
                if (res[i] > res[highest])
                {
                    highest = i;
                }
            }
            if (resultName[highest] == lblResult.Text)
            {
                MessageBox.Show("True");
                score = score + 10;
                NewQuiz();
            }
            else
            {
                MessageBox.Show($"Wrong Answer, You Uploaded {resultName[highest]}");
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
