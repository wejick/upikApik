using System;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
namespace upikapik
{
    public partial class mainForm : Form
    {
        BassPlayer _player;
        private OpenFileDialog _opnFile;
        string[] _files;
        string[] _paths;
        int _timeTotal;
        int _timeCurrent;
        int _indexOfPlayedFile;
        bool _shuffle = false;
        Timer _timerPlayer;
        Timer _timerRed;
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
            _opnFile.Multiselect = true;

            _timerPlayer.Interval = 500;
            _timerPlayer.Tick += new EventHandler(onTimerPlayer);
            _timerRed.Interval = 18000; // 3 minutes
        }
                
        private void btnPlay_Click(object sender, EventArgs e)
        {
            _player.pause_resume();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (_opnFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _files = _opnFile.SafeFileNames;
                _paths = _opnFile.FileNames;
                listPlay.Items.Clear();
                for (int i = 0; i < _files.Length; i++)
                {
                    listPlay.Items.Add(_files[i]);
                }
            }
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
    }
}
