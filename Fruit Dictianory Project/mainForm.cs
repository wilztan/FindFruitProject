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
using Accord.Neuro.Learning;

namespace Fruit_Dictianory_Project
{
    public partial class mainForm : Form
    {
        ActivationNetwork an;
        DistanceNetwork dn;
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
        
        /*
         * Going to Quiz Form
         */
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
        /*
         * Going to Find Form
         */
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
    }
}
