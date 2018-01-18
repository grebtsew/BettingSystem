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
        public bool add_once = false;
        public bool move_to_start = false;
        public int x = 0;
        public int start_ticks = 0;
        public float loses = 0;
        public float wins = 0;


        public const int SLEEP_TIME = 200;
        public const double VALUE_BALANCE = SLEEP_TIME / 2.5;
        public const int DERIVATE_NUMBER_AMOUNT = 20 ;
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
                delegate_write_label(label3, "Won! Diff: " + (-(bet_start_value - flow_value)).ToString());
                wins++;
                delegate_write_label(label9, wins.ToString());
            }
            else
            {
                delegate_write_label(label3, "Lost! Diff: " + (-(bet_start_value - flow_value)).ToString() );
                loses++;
                delegate_write_label(label11, loses.ToString());
            }
           
           delegate_write_label(label12, Math.Round((wins / (wins + loses)) * 100).ToString() + " %");
        }

        private void approx_position()
        {
            if (selected_ticks > 0)
            {
                full_change = flow_value;
                d1 = flow_value;
                

                 double[] change_array = new double[DERIVATE_NUMBER_AMOUNT];

                for(int i = 0; i < DERIVATE_NUMBER_AMOUNT; i++)
                {
                    d2 = valueArray[valueArray.Length - (i+2)]; // skip first

                    change_array[i] = d1 - d2;
                }

                // medel of change_array
                delta_change = change_array.Average();


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
                delegate_write_label(label1, flow_value.ToString());

                // show win/lose status
                bet_status();

                 
                // rest a while
                Thread.Sleep(SLEEP_TIME);
            }

        }

        private void bet_status()
        {
            if (bet_started) { 
            if (plus)
            {
                if (flow_value > bet_start_value)
                {
                    Winning();
                }
                else
                {
                    Losing();
                }

            }
            else
            {
                if (flow_value < bet_start_value)
                {
                    Winning();
                }
                else
                {
                    Losing();
                }
            }
            }
        }

        private void Losing()
        {
            delegate_write_label(label7, "Losing");
        }
        private void Winning()
        {
            delegate_write_label(label7, "Winning");
        }

        private void delegate_write_label(Label l, string text)
        {
            if (l.InvokeRequired)
            {
                l.BeginInvoke((MethodInvoker)delegate () { l.Text = text; });
            }
            else
            {
                l.Text = text;
            }
        }
 

        private void UpdateChart()
        {
            chart1.Series["Series1"].Points.Clear();
            chart1.Series["Series2"].Points.Clear();
            chart1.Series["Series3"].Points.Clear();
            chart1.Series["Series4"].Points.Clear();
            chart1.Series["Series5"].Points.Clear();

            // dataflow
            foreach (double d in valueArray)
            {
                chart1.Series["Series1"].Points.AddY(d);
            }

            // Approx
            if (selected_ticks > 0)
            {
                chart1.Series["Series2"].Points.AddXY(valueArray.Length, valueArray[valueArray.Length-1]);
                chart1.Series["Series2"].Points.AddXY(valueArray.Length + selected_ticks, full_change);
            }

            // Target approx
            if (bet_started)
            {
                chart1.Series["Series3"].Points.AddXY(valueArray.Length + selected_ticks, bet_approx);
                
            }

            // Follow Approx
            
            if (move_to_start)
            {
                chart1.Series["Series4"].Points.AddXY(valueArray.Length-x, bet_start_value);
                chart1.Series["Series4"].Points.AddXY(valueArray.Length-x + start_ticks, bet_approx);
                chart1.Series["Series5"].Points.AddXY(valueArray.Length-x, bet_start_value);
                chart1.Series["Series5"].Points.AddXY(valueArray.Length-x + start_ticks, bet_start_value);
                x++;

                if (  start_ticks -x< 0)
                {
                    move_to_start = false;
                    x = 0;
                }

            }
            

            if (add_once)
            {
                move_to_start = true;
                add_once = false;
                start_ticks = selected_ticks;
                chart1.Series["Series4"].Points.AddXY(valueArray.Length, bet_start_value);
                chart1.Series["Series4"].Points.AddXY(valueArray.Length + start_ticks, full_change);
                chart1.Series["Series5"].Points.AddXY(valueArray.Length, bet_start_value);
                chart1.Series["Series5"].Points.AddXY(valueArray.Length + start_ticks, bet_start_value);
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
            if (!bet_started && selected_ticks > 0)
            {
                add_once = true;
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
            if (!bet_started && selected_ticks > 0) {
                add_once = true;
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

        private void label12_Click(object sender, EventArgs e)
        {

        }
    }
}
