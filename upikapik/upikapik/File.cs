using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace upikapik
{
    class File
    {
        public File(string path)
        {
            this.path = path;
            // create file if none
            // set filename
            // get bitrate
            // get padding
            // get samplerate
            // get size
            // calculate frame length
        }
        private string path { get; set; }
        private string filename { get; set;}
        private int frameLength;
        private int bitrate;
        private int samplerate;
        private int size;
        private bool padding;
    }
}
