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

        //randowmizer for new quiz
        int Randomizer(int min, int max)
        {
            Random rdm = new Random();
            return rdm.Next(min, max);
        }

        //start new quizz
        void NewQuiz()
        {
            var value = resultName.Count();
            var randomed = Randomizer(0, value);
            lblResult.Text = resultName[randomed] ;
            lblScoreRes.Text = $"Your Result : {score}";
        }

        //get result to randomize
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
                //img = mainForm.Preprocess(img);
                pictBoxResult.Image = img;
            }
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            Bitmap img = new Bitmap(pictBoxResult.Image);
            img = mainForm.Preprocess(img);
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
        
    }
}
