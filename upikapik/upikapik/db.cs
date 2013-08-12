using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace upikapik
{
    class file_host_rel
    {
        public int id_file { get; set; }
        public string ip { get; set; }
        public int block_avail { get; set; }
    }
    class file_list
    {
        public int id_file { get; set; }
        public string nama { get; set; }
        public int bitrate { get; set; }
        public int samplerate { get; set; }
        public int size { get; set; }
    }
    class file_available
    {
        public int id_file; // different with hub numbering
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
    }
    class config
    {
        string trackerIp { get; set; }
    }
}
