/**
 * < Classes for database >
 * 
 * < -- >
 * < -- >
 * 
 **/
using System;

namespace upikapik
{
    class file_host_rel
    {
        public string id_file { get; set; }
        public string ip { get; set; }
        public int block_avail { get; set; }
    }
    class file_list
    {
        public string id_file { get; set; }
        public string filename { get; set; }
        public int bitrate { get; set; }
        public int samplerate { get; set; }
        public int size { get; set; }
    }
    class file_available
    {
        public int id_file;
        public string nama { get; set; }
        public int bitrate { get; set; }
        public int samplerate { get; set; }
        public int size { get; set; }
        public int block_avail { get; set; }
        public bool full { get; set; }
        public bool isFull()
        {
            // if framelength*block available + header == size this is true
            return true;
        }
        public int frameSize()
        {
            // the formula is 144 * bitrate * 1000 / samplingRate + paddingSize;
            return 0;
        }
    }
    class config
    {
        string trackerIp { get; set; }
    }
}
