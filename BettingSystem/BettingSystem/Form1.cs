using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


using System.Threading;
using System.Diagnostics;

namespace BettingSystem
{
    public partial class Form1 : Form
    {

        public Thread chartUpdateThread;
        public double[] valueArray = new double[30];
        public double[] tempArray = new double[40];
        public double START_VALUE = 1;
        public int selected_ticks = 0;


        public Form1()
        {
            InitializeComponent();

            chartUpdateThread = new Thread(new ThreadStart(this.run));
            chartUpdateThread.IsBackground = true;
            chartUpdateThread.Start();
            
        }

        private void run()
        {
            double value = START_VALUE;
            double change_value = 0;
            double d1 = 0;
            double d2 = 0;
            double delta_change = 0;



            Random rnd = new Random();

            while (true)
            {
                if (rnd.NextDouble() < 0.5) { 
                change_value = - rnd.NextDouble() / 100;
                } else
                {
                 change_value = rnd.NextDouble() / 100;
                }
                


                    value = value + change_value;

                valueArray[valueArray.Length - 1] = value;
                Array.Copy(valueArray, 1, valueArray, 0, valueArray.Length - 1);

                // pressumption

                if(selected_ticks > 0)
                {
                    Array.Clear(tempArray, 0, tempArray.Length);
                    
                    
                 d1 = value;
                 d2 = valueArray[valueArray.Length - 2];
                delta_change = d2 - d1;
                    Console.WriteLine(1000*delta_change);
                for(int i = 0; i < selected_ticks; i++)
                    {
                        tempArray[tempArray.Length -2- 10+selected_ticks] = value + delta_change;
                        Array.Copy(tempArray, 1, tempArray, 0, tempArray.Length - 1);
                    }

                }


                // chart
                if (chart1.IsHandleCreated)
                {
                    this.Invoke((MethodInvoker)delegate { UpdateChart(); });
                } else
                {
                    // ....
                }

                // labels
                if (this.label1.InvokeRequired)
                {
                    this.label1.BeginInvoke((MethodInvoker)delegate () { this.label1.Text = value.ToString();  });
                }
                else
                {
                    this.label1.Text = value.ToString(); 
                }


                Thread.Sleep(500);
            }

        }


        private void UpdateChart()
        {
            chart1.Series["Series1"].Points.Clear();
            chart1.Series["Series2"].Points.Clear();

            foreach (double d in valueArray)
            {

                chart1.Series["Series1"].Points.AddY(d);

                

            }

            if (selected_ticks > 0)
            {

                foreach (double j in tempArray)
                {
                  
                    chart1.Series["Series2"].Points.AddY(j);
                }
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
         
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            // pluss

        }

        private void button2_Click(object sender, EventArgs e)
        {
            // negative

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            selected_ticks =(int) numericUpDown1.Value;
        }
    }
}
