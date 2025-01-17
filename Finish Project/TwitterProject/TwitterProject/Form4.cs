﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TweetSharp;

namespace TwitterProject
{
    public partial class Form4 : Form
    {
        public Form4()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Apı Key doldurulmalıdır.");
            }

            if (string.IsNullOrEmpty(textBox2.Text))
            {
                MessageBox.Show("Api Secret doldurulmalıdır.");
            }
            if (string.IsNullOrEmpty(textBox3.Text))
            {
                MessageBox.Show("Access Token doldurulmalıdır.");
            }
            if (string.IsNullOrEmpty(textBox4.Text))
            {
                MessageBox.Show("Access Token Secret doldurulmalıdır.");
            }

            TwitterService a = new TwitterService(textBox1.Text, textBox2.Text, textBox3.Text, textBox4.Text);

            Form5 frm = new Form5();

            frm.t1 = textBox1.Text;
            frm.t2 = textBox2.Text;
            frm.t3 = textBox3.Text;
            frm.t4 = textBox4.Text;

            frm.Show();
            this.Close();
            
        }
    }
}
