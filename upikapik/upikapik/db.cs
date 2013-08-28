using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text;
using System.IO;

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
    class block_req
    {
        public bool is_finnish;
        public byte[] buffer;
        public int start_post;
        public block_req(bool is_finnish, int buffer_size, int start_post)
        {
            this.is_finnish = is_finnish;
            buffer = new byte[buffer_size];
            this.start_post = start_post;
        }
    }
    class RequestProp
    {
        public IPEndPoint peer;
        public TcpClient client;
        public NetworkStream stream;
        public string filename;
        // it must be adjusted, because file seeking add 1 automatically but socket reading nope
        // Adjust by subtract it with 1 when write to file
        public int startPost;
        public int blockSize;
        public byte[] receiveBuffer;
    }
    class Hosts
    {
        public IPEndPoint peer;
        public int blockAvail;
    }
}
