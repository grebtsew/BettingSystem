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
        public double START_VALUE = 1;
        public int selected_ticks = 0;
        public double full_change = 0;
        public bool bet_started = false;
        public double bet_start_value = 0;
        public double flow_value = 0;
        public bool plus = false;
        public double bet_approx = 0;
        public double d1 = 0;
        public double d2 = 0;
        public double delta_change = 0;
        public Random rnd = new Random();
        public double change_value = 0;

        public const int SLEEP_TIME = 200;
        public const double VALUE_BALANCE = SLEEP_TIME / 2.5;
        public const int DERIVATE_NUMBER_AMOUNT = 3;
        public const double FIFTY_PERCENT = 0.5;


        public Form1()
        {
            InitializeComponent();


            comboBox1.SelectedIndex = 0;

            chartUpdateThread = new Thread(new ThreadStart(this.run));
            chartUpdateThread.IsBackground = true;
            chartUpdateThread.Start();
            
        }

        private void bet_is_over()
        {
            if ((bet_start_value - flow_value < 0 && plus) || (bet_start_value - flow_value > 0 && !plus))
            {

                if (this.label3.InvokeRequired)
                {
                    this.label3.BeginInvoke((MethodInvoker)delegate () { this.label3.Text = "Won! Diff: " + (-(bet_start_value - flow_value)).ToString(); });
                }
                else
                {
                    this.label3.Text = "Won! Diff: " + (-(bet_start_value - flow_value)).ToString();
                }
            }
            else
            {

                if (this.label3.InvokeRequired)
                {
                    this.label3.BeginInvoke((MethodInvoker)delegate () { this.label3.Text = "Lost! Diff: " + (-(bet_start_value - flow_value)).ToString(); });
                }
                else
                {
                    this.label3.Text = "Lost! Diff: " + (-(bet_start_value - flow_value)).ToString();
                }
            }
        }

        private void approx_position()
        {
            if (selected_ticks > 0)
            {
                full_change = flow_value;
                d1 = flow_value;
                d2 = valueArray[valueArray.Length - DERIVATE_NUMBER_AMOUNT];
                delta_change = d2 - d1;



                for (int i = 0; i < selected_ticks; i++)
                {
                    full_change += delta_change;
                }


            }

        }

        private void generate_data_flow()
        {
            if (rnd.NextDouble() < FIFTY_PERCENT)
            {
                change_value = -rnd.NextDouble() / VALUE_BALANCE;
            }
            else
            {
                change_value = rnd.NextDouble() / VALUE_BALANCE;
            }


            flow_value += change_value;
            valueArray[valueArray.Length - 1] = flow_value;
            Array.Copy(valueArray, 1, valueArray, 0, valueArray.Length - 1);
        }


        private void run()
        {

            flow_value = START_VALUE;

            while (true)
            {

                if(selected_ticks <= 0)
                {
                    if (bet_started)
                    {
                        bet_is_over();
                        bet_started = false;
                    }
                }

                // count down for bet
                if (bet_started)
                {
                    selected_ticks--;

                    if (this.numericUpDown1.InvokeRequired)
                    {
                        this.numericUpDown1.BeginInvoke((MethodInvoker)delegate () { this.numericUpDown1.Value = selected_ticks; });
                    }
                    else
                    {
                        this.numericUpDown1.Value = selected_ticks;
                    }
                }

                // Generate a flow of data, something to bet on
                generate_data_flow();

                // Approx curve
                approx_position();
 

                // update chart
                if (chart1.IsHandleCreated)
                {
                    this.Invoke((MethodInvoker)delegate { UpdateChart(); });
                } else
                {
                    // ....
                }

                // update value label
                if (this.label1.InvokeRequired)
                {
                    this.label1.BeginInvoke((MethodInvoker)delegate () { this.label1.Text = flow_value.ToString();  });
                }
                else
                {
                    this.label1.Text = flow_value.ToString(); 
                }

                // rest a while
                Thread.Sleep(SLEEP_TIME);
            }

        }


        private void UpdateChart()
        {
            chart1.Series["Series1"].Points.Clear();
            chart1.Series["Series2"].Points.Clear();
            chart1.Series["Series3"].Points.Clear();

            foreach (double d in valueArray)
            {

                chart1.Series["Series1"].Points.AddY(d);

            }

            if (selected_ticks > 0)
            {
                chart1.Series["Series2"].Points.AddXY(valueArray.Length, valueArray[valueArray.Length-1]);
                chart1.Series["Series2"].Points.AddXY(valueArray.Length + selected_ticks, full_change);
            }

            if (bet_started)
            {
                chart1.Series["Series3"].Points.AddXY(valueArray.Length + selected_ticks, bet_approx);
                
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
            // plus
            if (!bet_started)
            {
                plus = true;
                bet_started = true;
                bet_start_value = flow_value;
                bet_approx = full_change;
                label3.Text = "Positive bet started please wait...";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

            // negative
            if (!bet_started) {
            plus = false;
            bet_started = true;
            bet_start_value = flow_value;
            bet_approx = full_change;

            label3.Text = "Negative bet started please wait...";
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            selected_ticks =(int) numericUpDown1.Value;
           
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
