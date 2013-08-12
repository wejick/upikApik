using System;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
namespace upikapik
{
    public partial class mainForm : Form
    {
        private const int MAX_FILE_AVAIL = 10;
        BassPlayer _player;
        private OpenFileDialog _opnFile;
//        string[] _files;
        string[] _paths;
        int _timeTotal;
        int _timeCurrent;
        int _indexOfPlayedFile;
        bool _shuffle = false;
        List<file_list> playList = new List<file_list>();
        Timer _timerPlayer;
        Timer _timerRed; // May can be if implemented in redToHub
        Random _rand = new Random();
        RedToHub _toHub = new RedToHub("RedDb.db4o","192.168.0.1",1337); // need to be esier to change
        
        public mainForm()
        {
            InitializeComponent();
            _opnFile = new OpenFileDialog();
            _player = new BassPlayer();
            _timerPlayer = new Timer();
            _timerRed = new Timer();

            _opnFile.Filter = "mp3 (*.mp3)|*.mp3";
            _opnFile.Multiselect = false;

            _timerPlayer.Interval = 500;
            _timerPlayer.Tick += new EventHandler(onTimerPlayer);
            _timerRed.Interval = 540000; // 9 minutes
            _timerRed.Tick += new EventHandler(onTimerRed);

            _toHub.command("NA");
        }
                
        private void btnPlay_Click(object sender, EventArgs e)
        {
            _player.pause_resume();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (_toHub.howMuch() < MAX_FILE_AVAIL)
            {
                if (_opnFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string path = _opnFile.FileName;
                    string filename = _opnFile.SafeFileName;
                    System.IO.File.Copy(path, "music/"+filename);

                    _toHub.addFileAvail(path,0);
                }
            }
            else
                MessageBox.Show("File available in network is reached maximum limit");
        }

        private void barSeek_Scroll(object sender, EventArgs e)
        {
            _player.seek(barSeek.Value);
        }

        private void listPlay_DoubleClick(object sender, EventArgs e)
        {
            if(listPlay.Items.Count != 0 )
            {
                _indexOfPlayedFile = listPlay.SelectedIndex;
                play();
                _timerPlayer.Start();
            }
        }

        private void onTimerPlayer(object source, EventArgs e)
        {
            _timeCurrent = _player.getPosSec();
            lblStatus.Text = "Time : " + s2t(_timeTotal) + " / " + s2t(_player.getPosSec());
            if(_timeCurrent != -1)
                barSeek.Value = _timeCurrent;
            
            // play next song
            if ((listPlay.Items.Count-1 > _indexOfPlayedFile) && !(_player.isActive()) && !_shuffle)
            {
                _indexOfPlayedFile++;
                play();
            }
            else if (!(_player.isActive()) && _shuffle)
            {
                _indexOfPlayedFile = _rand.Next(0, listPlay.Items.Count);
                play();
            }
        }
        private void onTimerRed(object source, EventArgs e)
        {
            _toHub.command("FL");
            System.Threading.Thread.Sleep(500);
            _toHub.command("NA");
            refreshList();
        }
        private TimeSpan s2t(int seconds)
        {
            TimeSpan time = new TimeSpan(0, 0, seconds);
            return time;
        }

        private void barVol_Scroll(object sender, EventArgs e)
        {
            _player.setVolume(barVol.Value);
        }

        private void play()
        {
            _player.play(_paths[_indexOfPlayedFile]);
            _timeTotal = _player.getLenSec();
            barSeek.SetRange(0, _timeTotal);
            this.Text = "UpikApik : " + _player.getFileName();

            barVol.Value = _player.getVolume();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (_shuffle == false)
            {
                _shuffle = true;
            }
            else
                _shuffle = false;
        }

        private void listPlay_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            _toHub.command("FL");
            System.Threading.Thread.Sleep(100);
            refreshList();
        }
        private void refreshList()
        {
            playList = _toHub.getFileList();
            listPlay.Items.Clear();
            foreach (var item in playList)
            {
                listPlay.Items.Add(item.nama);
            }
        }
    }
}
