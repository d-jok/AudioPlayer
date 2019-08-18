using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WMPLib;
using TagLib;

namespace AudioPlayer
{
    public struct AudioInfo
    {
        public string Url;
        public int TimeSec;

        public AudioInfo(string link, int time)
        {
            Url = link;
            TimeSec = time;
        }
    }

    public partial class Form1 : Form
    {
        WindowsMediaPlayer Player;
        private List<AudioInfo> PlayList = new List<AudioInfo>();

        private int CurrentTrack = 0;
        private bool isOpen = false;
        private bool isPlaying = false;      

        public Form1()
        {
            InitializeComponent();
            trackBar2.Maximum = 100;
            trackBar2.Value = 50;
            label3.Text = trackBar2.Value.ToString();
        }

        ~Form1()
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Interval = 1000;
            textBox2.Text = "00:00";
        }

        private void PlayFile(String url)
        {
            textBox1.Text = Path.GetFileName(url);
            trackBar1.Maximum = PlayList[CurrentTrack].TimeSec;

            Player = new WMPLib.WindowsMediaPlayer();

            Player.URL = url;           
            Player.controls.play();

            timer1.Start();
            isPlaying = true;
        }

        private void Stop()
        {
            if (isOpen == true)
            {
                isPlaying = false;

                Player.controls.stop();
                timer1.Stop();
                textBox2.Text = "00:00";
                trackBar1.Value = 0;
            }
        }

        private void Next()
        {
            if (isOpen == true)
            {
                Player.controls.stop();
                timer1.Stop();
                trackBar1.Value = 0;

                if (CurrentTrack + 1 < PlayList.Count)
                {
                    CurrentTrack++;
                    string url = PlayList[CurrentTrack].Url;
                    PlayFile(url);
                }
                else
                {
                    CurrentTrack = 0;
                    string url = PlayList[CurrentTrack].Url;
                    PlayFile(url);
                }
            }
        }

        private void Previous()
        {
            if (isOpen == true)
            {
                Player.controls.stop();
                timer1.Stop();
                trackBar1.Value = 0;

                if (CurrentTrack - 1 >= 0)
                {
                    CurrentTrack--;
                    string url = PlayList[CurrentTrack].Url;
                    PlayFile(url);
                }
                else
                {
                    CurrentTrack = PlayList.Count - 1;
                    string url = PlayList[CurrentTrack].Url;
                    PlayFile(url);
                }
            }
        }

        private void move()
        {
            Player.controls.pause();
            Player.controls.currentPosition = trackBar1.Value;
            Player.controls.play();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            textBox2.Text = Player.controls.currentPositionString;
            trackBar1.Value = (int)Player.controls.currentPosition;
        }       

        private void Player_PlayStateChange(int NewState)
        {
            if ((WMPLib.WMPPlayState)NewState == WMPLib.WMPPlayState.wmppsStopped)
            {
                this.Close();
            }
        }

        private void Player_MediaError(object pMediaObject)
        {
            MessageBox.Show("Cannot play media file.");
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)  //Play
        {
            try
            {
                if (isOpen == true && isPlaying == false)
                {
                    PlayFile(PlayList[CurrentTrack].Url);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)  //Stop
        {
            Stop();
        }

        private void button3_Click(object sender, EventArgs e)  //Next
        {
            Next();
        }

        private void button4_Click(object sender, EventArgs e)  //Previous
        {
            Previous();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFiles();            
        }

        private void OpenFiles()
        {
            OpenFileDialog file = new OpenFileDialog()
            {
                Multiselect = true,
                ValidateNames = true
            };

            TagLib.File File;    //for track (int)time in seconds (using TagLib.dll) 
            AudioInfo[] Open; 

            if (file.ShowDialog() == DialogResult.OK)
            {
                Open = new AudioInfo[file.FileNames.Length];
                int i = 0;

                try
                {
                    foreach (string obj in file.FileNames)
                    {
                        File = TagLib.File.Create(obj);
                        //PlayListAdd(new AudioInfo[] { new AudioInfo(obj, (int)File.Properties.Duration.TotalSeconds) });
                        Open[i] = new AudioInfo(obj, (int)File.Properties.Duration.TotalSeconds);
                        i++;
                    }

                    PlayListAdd(Open);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void PlayListAdd(params AudioInfo[] input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                PlayList.Add(input[i]);
                listBox1.Items.Add(Path.GetFileName(input[i].Url));
            }
            isOpen = true;
            PlayFile(PlayList[CurrentTrack].Url);
            //PlayFile(PlayList[CurrentTrack].Url);   //Add first track to Player.URL;
            //PlayList.Add(input[0]);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)       //CHECK!!!!
        {
            bool clicked = true;

            trackBar1.MouseDown += (s, x) =>
            {
                timer1.Stop();
                clicked = true;
            };

            trackBar1.MouseUp += (s, x) =>
            {
                if (!clicked)
                    return;

                clicked = false;
                timer1.Start();
                move();
            };

        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            try
            {
                if (isOpen == true)
                {
                    Player.settings.volume = trackBar2.Value;
                    label3.Text = trackBar2.Value.ToString();
                }
                else
                    MessageBox.Show("Choose track", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch(Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Player.controls.stop();

            CurrentTrack = listBox1.SelectedIndex;
            PlayFile(PlayList[listBox1.SelectedIndex].Url);
        }
    }
}