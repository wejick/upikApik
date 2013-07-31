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
        string fileId { get; set; }
        string nodeIP { get; set; }
        int block_avail { get; set; }
    }
    class file_list
    {
        string fileId { get; set; }
        string filename { get; set; }
        int bitrate { get; set; }
        int samplerate { get; set; }
        int size { get; set; }
    }
    class file_available
    {
        string name { get; set; }
        int bitrate { get; set; }
        int samplerate { get; set; }
        int size { get; set; }
        int block_avail { get; set; }
        private bool full { get; set; }
    }
    class config
    {
        string trackerIp { get; set; }
    }
}
