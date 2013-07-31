using System;
using Db4objects.Db4o;
using Db4objects.Db4o.Linq;
using System.Text;
using Newtonsoft.Json;
namespace upikapik
{
    class RedToHub
    {
        IObjectContainer db;
        private string trackerIP {get; set;}
        public void start()
        {
        }
        public void setDb(string db_filename)
        {
            db = Db4oEmbedded.OpenFile(db_filename);
        }
        // get complete file list available at hub
        public void getFileList()
        {
            string fileList = "[{\"id_file\":0,\"nama\":\"merindu.mp3\",\"bitrate\":\"128\",\"samplerate\":\"444000\",\"size\":6000},{\"id_file\":1,\"nama\":\"Menunggu.mp3\",\"bitrate\":\"128\",\"samplerate\":\"444000\",\"size\":6000},{\"id_file\":2,\"nama\":\"Menanti.mp3\",\"bitrate\":\"128\",\"samplerate\":\"444000\",\"size\":6000}]";

            
        }
        // get file detail including available peer
        public void getFd(string fileId)
        {

        }
        // update information of your file at hub
        public void update(string fileId, int block_avail)
        {
        }
    }
}
