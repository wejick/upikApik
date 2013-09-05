using System;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace upikapik
{
    public partial class mainForm : Form
    {
        private const int MAX_FILE_AVAIL = 10;
        BassPlayer _player;
        private OpenFileDialog _opnFile;
        string _appDir;
        bool local = false;

        // streaming things
        byte[] buffer;
        GCHandle BufferHandler;

        // bass player things
        file_list _current_file;
        int _timeTotal;
        int _timeCurrent;
        int _indexOfPlayedFile;
        bool _shuffle = false;
        List<file_list> playList = new List<file_list>();
        Random _rand = new Random();

        int frame_size = 0;
        int lastStartpost = 0;
        int available_sec = 0;
        int block_size = 0;

        // timer
        Timer _timerPlayer;
        Timer _timerRed; // May can be if implemented in redToHub
        int intervalOne;

        // another HMR component
        RedToHub _toHub = new RedToHub("RedDb.db4o", "192.168.0.33", 1337); // need to be esier to change
        AsynchRedServ _server = new AsynchRedServ("192.168.0.7", 1338);// need to be esier to change
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

            _timerPlayer.Interval = 100;
            _timerPlayer.Tick += new EventHandler(onTimerPlayer);
            _timerRed.Interval = 540000; // 9 minutes
            _timerRed.Tick += new EventHandler(onTimerRed);
            _timerRed.Start();

            _toHub.command("NA");

            tRedServ = new System.Threading.Thread(() => { _server.startListening(); });
            tRedServ.Start();

            _appDir = Directory.GetCurrentDirectory();
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
                        System.IO.File.Copy(path, Path.Combine(_appDir, "music", filename));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Failed to copy file");
                        fail = true;
                    }
                    if (!fail)
                        _toHub.addFileAvail(path, 0);
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
            if (listPlay.Items.Count != 0)
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
            intervalOne++;

            _timeCurrent = _player.getPosSec();
            lblStatus.Text = "Time : " + s2t(_timeTotal) + " / " + s2t(_player.getPosSec());

            lastStartpost = _redStream.getLastValueOfBuffer();
            if (lastStartpost != 0)
            {
                if (_timeTotal != 0)
                    barProgress.Value = (int) ((float) lastStartpost / (float) _current_file.size * 100) ;
            }
            if (_timeCurrent != -1)
                barSeek.Value = _timeCurrent;

            if (intervalOne == 10)
            {
                if (!local)
                {
                    _player.play_buffer(BufferHandler.AddrOfPinnedObject(), _current_file.size);
                    _timeTotal = _player.getLenSec();
                    barSeek.SetRange(0, _timeTotal);
                }
            }

            //write to buffer
            if(!local)
                _redStream.writeToStream(BufferHandler.AddrOfPinnedObject());
            
            // play next song
            /*if ((listPlay.Items.Count-1 > _indexOfPlayedFile) && !(_player.isActive()) && !_shuffle)
            {
                _indexOfPlayedFile++;
                play();
            }
            else if (!(_player.isActive()) && _shuffle)
            {
                _indexOfPlayedFile = _rand.Next(0, listPlay.Items.Count);
                play();
            }*/
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
            _player.stop();
            if (_toHub.isFull(_current_file.nama))
            {
                _player.play_local("music\\" + _current_file.nama);
                _timeTotal = _player.getLenSec();
                barSeek.SetRange(0, _timeTotal);
                local = true;
            }
            else
            {
                local = false;
                try
                {
                    BufferHandler.Free();
                }
                catch (Exception ex)
                {

                }
                //clean the stream properties
                _redStream.stopStream();
                _redStream.closeFile();

                //clean buffer
                try
                {
                    BufferHandler.Free();
                }
                catch (Exception ex)
                {

                }
                //get host info before playing
                _toHub.command("FD;" + _current_file.id_file);
                System.Threading.Thread.Sleep(200);
                _redStream.getHostsAvail(_toHub.getAvailableHost(_current_file.nama));

                //start stream
                _redStream.startStream(_current_file.id_file, _current_file);

                //get streaming properties
                block_size = _redStream.getBlockSize();
                frame_size = _redStream.getFrameSize();                
                //_timeTotal = (int)((_current_file.size / frame_size) * 0.026);

                //set buffer
                buffer = new byte[_current_file.size];
                BufferHandler = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            }            
            //barProgress.Value = 0;
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
