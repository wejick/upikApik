using System;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
//using System.Threading;
namespace upikapik
{
    public partial class mainForm : Form
    {
        private const int MAX_FILE_AVAIL = 10;
        BassPlayer _player;
        private OpenFileDialog _opnFile;

        // bass player things
        string[] _paths;
        file_list _current_file;
        int _timeTotal;
        int _timeCurrent;
        int _indexOfPlayedFile;
        bool _shuffle = false;
        List<file_list> playList = new List<file_list>();
        Random _rand = new Random();
        bool local = false;
        byte[] buffer = null;
        // timer
        Timer _timerPlayer;
        Timer _timerRed; // May can be if implemented in redToHub
        int intervalCounter = 0;

        // another HMR component
        RedToHub _toHub = new RedToHub("RedDb.db4o","192.168.0.33",1337); // need to be esier to change
        //RedServ _server = new RedServ("127.0.0.1", 1337);
        AsynchRedServ _server = new AsynchRedServ("192.168.0.7", 1337);// need to be esier to change
        System.Threading.Thread tRedServ;
        AsynchRedStream _redStream = new AsynchRedStream();

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
            _timerRed.Start();

            _toHub.command("NA");
            
            tRedServ = new System.Threading.Thread(() => { _server.startListening(); });
            tRedServ.Start();
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
                    bool fail = false;
                    string path = _opnFile.FileName;
                    string filename = _opnFile.SafeFileName;
                    try
                    {
                        System.IO.File.Copy(path, Path.Combine(Directory.GetCurrentDirectory(),"music",filename));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Failed to copy file");
                        fail = true;
                    }
                    if(!fail)
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
                // set current file
                _current_file = playList.Find(p => p.nama.Equals(listPlay.SelectedItem));
                play();
                _timerPlayer.Start();
            }
        }

        private void onTimerPlayer(object source, EventArgs e)
        {
            intervalCounter++;

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

            // pause when buffering
            int available_block = _toHub.getBlockAvailableSize(_current_file.nama);
            int block_size = ((144 * _current_file.bitrate * 1000) / _current_file.samplerate);
            int available_sec = (int)(available_block * 0.026);
            barProgress.Value = (_current_file.size / ((_current_file.size / block_size) * block_size)) * 100;
            if (!local)
            {
                if ((available_sec) < (_timeCurrent - 2) && (!_player.isPause()))
                {
                    _player.pause_resume();
                }
                else if ((available_sec) < (_timeCurrent - 2) && (_player.isPause()))
                {

                }
                else if (_player.isPause())
                {
                    _player.pause_resume();
                }

                if (intervalCounter == 8) // update available block after 4 second
                {
                    if (!local)
                    {
                        _toHub.command("UP;" + _current_file.id_file + ";" + _toHub.getBlockAvailableSize(_current_file.nama));
                        _toHub.command("FD;" + _current_file.id_file);
                    }
                    intervalCounter = 0;
                }
                if (intervalCounter == 4 || intervalCounter == 7) // every 2 and 1,5 second
                {
                    _redStream.writeToBuffer(ref buffer);
                    _redStream.getHostsAvail(_toHub.getAvailableHost(_current_file.id_file));
                }
            }
        }
        private void onTimerRed(object source, EventArgs e)
        {
            _toHub.command("FL");
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
            if (_toHub.isFull(_current_file.nama))
            {
                _player.play_local("music\\" + _current_file.nama);
                _timeTotal = _player.getLenSec();
                local = true;
            }
            else
            {
                _redStream.startStream(_current_file.id_file, _current_file);
                buffer = new byte[_current_file.size];
                _redStream.writeToBuffer(ref buffer);
                _player.play_buffer(ref buffer);
            }
            barSeek.SetRange(0, _timeTotal);
            barProgress.Value = 0;
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

        private void mainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _server.enable = false;
            ((IDisposable)_server).Dispose();
        }
    }
}
