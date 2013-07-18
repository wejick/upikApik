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
         * < initialize everything to get bass set and ready >         * 
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
            return path2FileName(path);
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
        /*
         * < Play the stream >
         * @param path of source file
         * */
        public void play(string path)
        {
            this.setPath(path);
            if ((stream = Bass.BASS_StreamCreateFile(path, 0L, 0L, BASSFlag.BASS_DEFAULT)) != 0)
            {
                Bass.BASS_ChannelPlay(stream, false);
                streamLen = Bass.BASS_ChannelGetLength(stream);
            }
            else
                throw new System.InvalidOperationException("Can't open file to play");
        }
        /*
         * < Pause or resume the stream according to current state of stream >
         * 
         * */
        public void pause_resume()
        {
            if (stream != 0)
            {
                // if stream playing, pause it
                if (Bass.BASS_ChannelIsActive(stream) == BASSActive.BASS_ACTIVE_PLAYING)
                    Bass.BASS_ChannelPause(stream);
                // if stream paused, play it
                else if (Bass.BASS_ChannelIsActive(stream) == BASSActive.BASS_ACTIVE_PAUSED)
                    Bass.BASS_ChannelPlay(stream, false);
            }
        }
        /*
         * < Stop the stream >
         * */
        public void stop()
        {
            Bass.BASS_ChannelStop(stream);
        }
        /*
         * < Seek the stream to certain position >
         * @param pos is desired position in seconds
         * */
        public void seek(int pos)
        {
            Bass.BASS_ChannelSetPosition(stream, pos);
        }

        // bass spesific good routine
        /*
         * < Free resource used by bass >
         * */
        public void free()
        {
            Bass.BASS_Free();
        }

        // utility
        /*
         * < Take filename from complete path >
         * @param a string of path
         * */
        private string path2FileName(string path)
        {
            return System.IO.Path.GetFileName(path);
        }
        /*
         * < timer to maintain dirty job :p >
         * */
        private void onMainTime(object source, ElapsedEventArgs e)
        {
            streamPos = Bass.BASS_ChannelGetPosition(stream);
        }
    }
}
