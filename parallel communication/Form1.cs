using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using parallel_communication.Properties;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;

namespace parallel_communication
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            label1.Text = "0";
            byte bb =(byte) parallel.Inp(890);
            bb = (byte)((byte)bb ^ (byte)32);
            //MessageBox.Show(bb.ToString());
            //parallel.Out(890, 0);
            parallel.Out(890, bb);
            //button2.PerformClick();
        }
        Thread check;
        string data = "";
        int mul, sum,signal;
        int output = 0;
        void k()
        {
            while (true)
            {
                data = ""; mul = 1; sum = 0; signal = 0;
                if (parallel.Inp(888) == 255)
                {
                    //Thread.Sleep(600);
                    //if (parallel.Inp(888) != 255) continue;

                    //Thread.Sleep(600);
                    //if (parallel.Inp(888) != 255) continue;
                    //while (parallel.Inp(888) == 255) ;
                    //Thread.Sleep(8);
                    //if (parallel.Inp(888) == 255)
                    {
                        for (int i = 1; i < 9; i++)
                        {
                            if (parallel.Inp(889) == 255)
                            { //data += "0"; 
                                label1.Text = "0";
                            }
                            else
                            {
                                sum += mul;
                                mul *= 2;
                                label1.Text = "1";
                            }
                            if (signal == 1)
                            {
                                while (parallel.Inp(888) != 255) ;
                                signal=0;
                            }
                            else
                            {
                                while (parallel.Inp(888) == 255) ;
                                signal = 1;
                            }
                        }
                        listBox1.Items.Add(sum.ToString());
                    }
                }
            }
        }
        void show()
        {
            
            while (true)
            {
                data = "";
                int dd=(int)parallel.Inp(888);
                int mask = 1;
                mul = 1; sum = 0;
                while (mask != 0)
                {
                    if ((dd & mask) != 0)
                    {
                        data += "1";
                        sum += mul;
                        mul *= 2;
                    }
                    else
                    {
                        data += "0";
                        mul *= 2;
                    }
                    mask <<= 1;
                }
                label1.Text = sum.ToString();
                //label1.Text = data;
                Thread.Sleep(100);
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            //parallel.Out(888, 255);
            //MessageBox.Show(parallel.Inp(889).ToString());
            //return;
            check = new Thread(show);
            check.Start();           
        }
        Thread t;
        private void button2_Click(object sender, EventArgs e)
        {
            t = new Thread(doi);
            t.Start();
        }
        void doi()
        {
            int cnt = 0;
            int tot = 0;
            string tt = "";
            HiPerfTimer ht = new HiPerfTimer();
            double inter;
            bool detail = false;
            while (true)
            {
                if (parallel.Inp(889) == 255 && cnt==0)
                {
                    //MessageBox.Show("Got It Buddy!!");
                    //break;
                    ht.Stop();
                    inter = ht.Duration;
                    if (inter > 1)
                    {
                        //listBox1.Items.Clear();
                    }
                    tt = inter.ToString();
                    if (detail)
                    {
                        if (output == 0)
                            listBox1.Items.Add("-" + tt.Substring(2, 3) + " ms " + tt.Substring(5, 3) + " us");
                        else
                            listBox2.Items.Add("-" + tt.Substring(2, 3) + " ms " + tt.Substring(5, 3) + " us");
                    }
                    else listBox1.Items.Add("-" + tt);

                    ht.Start();
                    cnt = 1;
                }
                else if (cnt > 0 && parallel.Inp(889)!=255) {
                    ht.Stop();
                    //MessageBox.Show(ht.Duration.ToString());
                    inter = ht.Duration;
                    tt = inter.ToString();
                    if (detail)
                    {
                        if (output == 0)
                            listBox1.Items.Add("+" + tt.Substring(2, 3) + " ms " + tt.Substring(5, 3) + " us");
                        else
                            listBox2.Items.Add("+" + tt.Substring(2, 3) + " ms " + tt.Substring(5, 3) + " us");
                    }
                    else listBox1.Items.Add("+" + tt);
                    ht.Start();
                    cnt = 0;
                    //int.Parse(label1.Text)
                    //label1.Text =(listBox1.Items.Count-tot) .ToString();
                    //tot = listBox1.Items.Count;
                    //Thread.Sleep(1000);
                }
                //Thread.Sleep(11);

            }
        }

        bool stat = true;
        private void button3_Click(object sender, EventArgs e)
        {

            listBox1.Items.Clear();
            listBox2.Items.Clear();
            return;
            
            if (stat == true)
                parallel.Out(888, 0x00);
            else parallel.Out(888, 0x01);
            stat = !stat;
        }


        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            parallel.Out(888,4); 
            parallel.Out(889,255); 
            parallel.Out(890,204);
            if (check != null)
                check.Abort();
            if(t!=null)
                t.Abort();
            Application.Exit();
        }
        void dance()
        {
            while (true)
            {
                byte a = 0x01;
                while (a!=0x00)
                {
                    parallel.Out(888, a);
                    a <<= 1;
                    Thread.Sleep(100);
                }
            }
 
        }
        
        private void button5_Click(object sender, EventArgs e)
        {
            output = output==0?1:0;
            //check = new Thread(dance);
            //check.Start(); 
            
        }
    }
    public class parallel
    {
        [DllImport("inpout32.dll", EntryPoint = "Inp32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]

        public static extern int Inp(ushort PortAddress);
        [DllImport("inpout32.dll", EntryPoint = "Out32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]

        public static extern void Out(ushort PortAddress, short Value); 

    }
    internal class HiPerfTimer
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(
            out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(
            out long lpFrequency);

        private long startTime, stopTime;
        private long freq;

        // Constructor
        public HiPerfTimer()
        {
            startTime = 0;
            stopTime = 0;

            if (QueryPerformanceFrequency(out freq) == false)
            {
                // high-performance counter not supported
                throw new Win32Exception();
            }
            //MessageBox.Show(freq.ToString());
        }
        // Start the timer
        public void Start()
        {
            // lets do the waiting threads there work
            Thread.Sleep(0);
            //startTime = 0;
            //stopTime = 0;
            QueryPerformanceCounter(out startTime);
        }
        // Stop the timer
        public void Stop()
        {
            QueryPerformanceCounter(out stopTime);
        }
        // Returns the duration of the timer (in seconds)
        public double Duration
        {
            get
            {
                return (double)(stopTime - startTime) / (double)freq;
            }
        }
    }

}

