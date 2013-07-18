/**
 * < Play and control audio stream from many source >
 * 
 * < Play the audio stream and control it, you can use it to play, pause, resume or seek the stream. >
 * < The source stream can be from file or another source we implement later. >
 * 
 **/
using System;
using System.Collections.Generic;
using System.Timers;
using System.Text;
using Un4seen.Bass;

namespace upikapik
{
    class BassPlayer
    {
        private int stream;
        private long streamLen; // stream size in byte
        private long streamPos; // stream position in byte
        private string path;
        private string filename;
        private System.Timers.Timer mainTime;

        /*
         * < initialize everything to get bass set and ready >
         * 
         * */
        public BassPlayer()
        {
            // initialize bass device
            if (!(Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero)))
            {
                throw new System.InvalidOperationException("Can't initialize bass device");
            }
            // setup timer
            mainTime = new System.Timers.Timer(500);
            mainTime.Elapsed += new ElapsedEventHandler(onMainTime);
            mainTime.AutoReset = true;
            mainTime.Enabled = true;
        }
        // set some properties
        /*
         * < set path from active stream source>
         * @param path of source (string)
         * */
        private void setPath(string path)
        {
            this.path = path;
        }        
        // get info from stream
        /*
         * < Get path from active stream source>
         * @return string file path from stream source
         * */
        public string getPath()
        {
            return path;
        }
        /*
         * < Get filename from active stream source>
         * @return string filename from stream source
         * */
        public string getFileName()
        {
            return System.IO.Path.GetFileName(path);
        }
        /*
         * < Get the length from stream source in byte>
         * @return stream source size in int
         * */
        public long getLenByte()
        {
            return streamLen;
        }
        /*
         * < Get the length from stream source in seconds>
         * @return stream source size in seconds
         * */
        public int getLenSec()
        {
            return (int)Bass.BASS_ChannelBytes2Seconds(stream, streamLen);
        }
        /*
         * < Get current playback position from stream in byte>
         * @return stream position size in int
         * */
        public long getPosByte()
        {
            return streamPos;
        }
        /*
         * < Get current playback position from stream in second>
         * @return stream position time in int
         * */
        public int getPosSec()
        {
            return (int)Bass.BASS_ChannelBytes2Seconds(stream, streamPos);
        }

        // control the stream
        public void play(string path);
        public void pause_resume();
        public void stop();
        public void seek(int pos);

        // bass spesific good routine
        public void free();

        // utility
        private string path2FileName(string path);
        private static void onMainTime(object source, ElapsedEventArgs e)
        {

        }
    }
}
