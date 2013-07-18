﻿using System;
using System.Text;
using System.Windows.Forms;

namespace upikapik
{
    public partial class mainForm : Form
    {
        BassPlayer player;
        private OpenFileDialog opnFile;
        string[] files;
        string[] paths;
        int timeTotal;
        int timeCurrent;
        int indexOfPlayedFile;
        bool shuffle = false;
        Timer timerForm;
        Random rand = new Random();

        public mainForm()
        {
            InitializeComponent();
            opnFile = new OpenFileDialog();
            player = new BassPlayer();
            timerForm = new Timer();

            opnFile.Filter = "mp3 (*.mp3)|*.mp3";
            opnFile.Multiselect = true;

            timerForm.Interval = 500;
            timerForm.Tick += new EventHandler(onTimerForm);            
        }
                
        private void btnPlay_Click(object sender, EventArgs e)
        {
            player.pause_resume();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (opnFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                files = opnFile.SafeFileNames;
                paths = opnFile.FileNames;
                listPlay.Items.Clear();
                for (int i = 0; i < files.Length; i++)
                {
                    listPlay.Items.Add(files[i]);
                }
            }
        }

        private void barSeek_Scroll(object sender, EventArgs e)
        {
            player.seek(barSeek.Value);
        }

        private void listPlay_DoubleClick(object sender, EventArgs e)
        {
            if(listPlay.Items.Count != 0 )
            {
                indexOfPlayedFile = listPlay.SelectedIndex;
                play();
                timerForm.Start();
            }
        }

        private void onTimerForm(object source, EventArgs e)
        {
            timeCurrent = player.getPosSec();
            lblStatus.Text = "Time : " + s2t(timeTotal) + " / " + s2t(player.getPosSec());
            if(timeCurrent != -1)
                barSeek.Value = timeCurrent;
            
            // play next song
            if ((listPlay.Items.Count-1 > indexOfPlayedFile) && !(player.isActive()) && !shuffle)
            {
                indexOfPlayedFile++;
                play();
            }
            else if (!(player.isActive()) && shuffle)
            {
                indexOfPlayedFile = rand.Next(0, listPlay.Items.Count);
                play();
            }
        }

        private TimeSpan s2t(int seconds)
        {
            TimeSpan time = new TimeSpan(0, 0, seconds);
            return time;
        }

        private void barVol_Scroll(object sender, EventArgs e)
        {
            player.setVolume(barVol.Value);
        }

        private void play()
        {
            player.play(paths[indexOfPlayedFile]);
            timeTotal = player.getLenSec();
            barSeek.SetRange(0, timeTotal);
            this.Text = "UpikApik : " + player.getFileName();

            barVol.Value = player.getVolume();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (shuffle == false)
            {
                shuffle = true;
            }
            else
                shuffle = false;
        }
    }
}
