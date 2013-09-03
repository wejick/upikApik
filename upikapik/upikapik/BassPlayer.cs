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
        private int channel;
        private long streamLen; // stream size in byte
        private long streamPos; // stream position in byte
        private string path;
        private System.Timers.Timer mainTime;
        private bool local = false;
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
        /*
         * < Set volume>
         * */
        public void setVolume(int vol)
        {
            Bass.BASS_SetVolume(((float)vol) / 10);
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
        /*
         * < Get current volume>
         * @return volume in int
         * */
        public int getVolume()
        {
            return (int)(Bass.BASS_GetVolume() * 10);
        }
        /*
         * < Is the stream paused ?>
         * @return true if stream paused
         * */
        public bool isPause()
        {
            if (Bass.BASS_ChannelIsActive(stream) == BASSActive.BASS_ACTIVE_PAUSED)
                return true;
            else
                return false;
        }
        /*
         * < Is the stream not active ?>
         * @return true if stream paused
         * */
        public bool isActive()
        {
            if (Bass.BASS_ChannelIsActive(stream) == BASSActive.BASS_ACTIVE_STOPPED)
                return false;
            else
                return true;
        }
        // control the stream
        /*
         * < Play the stream from file>
         * @param path of source file
         * */
        public void play_local(string path)
        {
            this.setPath(path);
            local = true;
            BASSActive status;
            status = Bass.BASS_ChannelIsActive(stream);

            if (status == BASSActive.BASS_ACTIVE_PLAYING)
            {
                Bass.BASS_StreamFree(stream);
            }
            if ((stream = Bass.BASS_StreamCreateFile(path, 0L, 0L, BASSFlag.BASS_DEFAULT)) != 0)
            {
                Bass.BASS_ChannelPlay(stream, false);
                streamLen = Bass.BASS_ChannelGetLength(stream);
            }
            else
                throw new System.InvalidOperationException("Can't open file to play");
        }
        /*
         * < Play the stream from buffer>
         * @param buffer of played file
         * */
        public void play_buffer(ref byte[] buffer)
        {
            local = false;
            if (Bass.BASS_ChannelIsActive(stream) == BASSActive.BASS_ACTIVE_PLAYING || 
                Bass.BASS_ChannelIsActive(stream) == BASSActive.BASS_ACTIVE_PLAYING)
            {
                Bass.BASS_StreamFree(stream);
                Bass.BASS_StreamFree(channel);
            }
            if ((stream = Bass.BASS_SampleLoad(buffer,0,buffer.Length,1,BASSFlag.BASS_DEFAULT)) != 0)
            {
                channel = Bass.BASS_SampleGetChannel(stream, false);
                Bass.BASS_ChannelPlay(channel, false);
                streamLen = Bass.BASS_ChannelGetLength(channel);
            }
        }
        /*
         * < Play the stream from buffer>
         * @param intptr of buffer of played file
         * */
        public void play_buffer(IntPtr buffer, int length)
        {
            local = false;
            if (Bass.BASS_ChannelIsActive(stream) == BASSActive.BASS_ACTIVE_PLAYING ||
                Bass.BASS_ChannelIsActive(stream) == BASSActive.BASS_ACTIVE_PLAYING)
            {
                Bass.BASS_StreamFree(stream);
                Bass.BASS_StreamFree(channel);
            }
            if ((stream = Bass.BASS_SampleLoad(buffer,0,length,1,BASSFlag.BASS_DEFAULT)) != 0)
            {
                channel = Bass.BASS_SampleGetChannel(stream, false);
                Bass.BASS_ChannelPlay(channel, false);
                streamLen = Bass.BASS_ChannelGetLength(channel);
            }
        }
        /*
         * < Pause or resume the stream according to current state of stream >
         * 
         * */
        public void pause_resume()
        {
            if (local)
            {
                if (stream != 0)
                {
                    // if stream playing, pause it
                    if (Bass.BASS_ChannelIsActive(stream) == BASSActive.BASS_ACTIVE_PLAYING)
                    {
                        Bass.BASS_ChannelPause(stream);
                        mainTime.Stop();
                    }
                    // if stream paused, play it
                    else if (Bass.BASS_ChannelIsActive(stream) == BASSActive.BASS_ACTIVE_PAUSED)
                    {
                        mainTime.Start();
                        Bass.BASS_ChannelPlay(stream, false);
                    }
                }
            }
            else
            {
                if (channel != 0)
                {
                    // if stream playing, pause it
                    if (Bass.BASS_ChannelIsActive(channel) == BASSActive.BASS_ACTIVE_PLAYING)
                    {
                        Bass.BASS_ChannelPause(channel);
                        mainTime.Stop();
                    }
                    // if stream paused, play it
                    else if (Bass.BASS_ChannelIsActive(channel) == BASSActive.BASS_ACTIVE_PAUSED)
                    {
                        mainTime.Start();
                        Bass.BASS_ChannelPlay(channel, false);
                    }
                }
            }
        }
        /*
         * < Stop the stream >
         * */
        public void stop()
        {
            if(local)
                Bass.BASS_ChannelStop(stream);
            else
                Bass.BASS_ChannelStop(channel);
        }
        /*
         * < Seek the stream to certain position >
         * @param pos is desired position in seconds
         * */
        public void seek(int pos)
        {
            if(local)
                Bass.BASS_ChannelSetPosition(stream, (double)pos);
            else
                Bass.BASS_ChannelSetPosition(stream, (double)pos);
        }

        // bass spesific good routine
        /*
         * < Free resource used by bass >
         * */
        public void free()
        {
            Bass.BASS_Free();
        }
        /*
         * < Set the maximum limit to play, the limit is from available block >
         * < supposed to update by runtime method (what is it?) >
         * < time conversion is from available_block * second per block >
         * @param limit to play, from available block. In time
         * */
        public void setLimit(int limit)
        {

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
            if(local)
                streamPos = Bass.BASS_ChannelGetPosition(stream);
            else
                streamPos = Bass.BASS_ChannelGetPosition(channel);
        }
    }
}
