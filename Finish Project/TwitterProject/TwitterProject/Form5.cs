using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TweetSharp;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;



namespace TwitterProject
{
    public partial class Form5 : Form
    {
        public Form5()
        {
            InitializeComponent();

        }

        public string t1 { get; set; }
        public string t2 { get; set; }
        public string t3 { get; set; }
        public string t4 { get; set; }



        private void button1_Click(object sender, EventArgs e)
        {
            TwitterService a = new TwitterService(t1, t2, t3, t4);

            listBox1.Items.Clear();
            var veriler = a.ListTweetsOnHomeTimeline(new ListTweetsOnHomeTimelineOptions(){   });
            foreach (var item in veriler)
            {
                listBox1.Items.Add(item.User.Name + "  " + item.Text);
                tweetid.Items.Add(item.Id.ToString());
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            TwitterService a = new TwitterService(t1, t2, t3, t4);
            a.SendTweet(new SendTweetOptions() {Status = textBox1.Text });
            MessageBox.Show("Tweetlendi.");
        }

        private void button2_Click(object sender, EventArgs e)
        {

            TwitterService a = new TwitterService(t1, t2, t3, t4);
            listBox1.Items.Clear();
            tweetid.Items.Clear();
            var veriler = a.ListTweetsOnUserTimeline( new ListTweetsOnUserTimelineOptions(){ Count = 10 });
            foreach (var item in veriler)
            {
                listBox1.Items.Add(item.User.Name + "  " + item.Text);

            }

        }

        private void button5_Click(object sender, EventArgs e)
        {

        }
        ListBox tweetid = new ListBox();
        private void button3_Click(object sender, EventArgs e)
        {
            TwitterService b = new TwitterService(t1, t2, t3, t4);
            b.Retweet(new RetweetOptions() {Id=long.Parse(tweetid.Items[listBox1.SelectedIndex].ToString()) } );
            MessageBox.Show("Retweetlendi.");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            TwitterService a = new TwitterService(t1, t2, t3, t4);
            listBox1.Items.Clear();
            tweetid.Items.Clear();
            var veriler = a.ListTweetsMentioningMe(new ListTweetsMentioningMeOptions() { Count = 10 });
            foreach (var item in veriler)
            {
                listBox1.Items.Add(item.User.Name + "  " + item.Text);

            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form5_Load(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }
    }
}
