using System;
using System.Collections.Generic;
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
        public RedToHub(string dbName)
        {
            db = Db4oEmbedded.OpenFile(dbName);
        }
        // get complete file list available at hub
        public void getFileList()
        {
            string fileList = "[{\"id_file\":0,\"nama\":\"merindu.mp3\",\"bitrate\":\"128\",\"samplerate\":\"444000\",\"size\":6000},{\"id_file\":1,\"nama\":\"Menunggu.mp3\",\"bitrate\":\"128\",\"samplerate\":\"444000\",\"size\":6000},{\"id_file\":2,\"nama\":\"Menanti.mp3\",\"bitrate\":\"128\",\"samplerate\":\"444000\",\"size\":6000}]";
            dynamic objects = JsonConvert.DeserializeObject(fileList);
            foreach (var item in objects)
            {
                db.Store(item);
            }
        }
        // get file detail including available peer
        public void getFd(string fileId)
        {
            string fileDetail = "[{\"id_file\":1,\"ip\":\"192.168.0.1\",\"block_avail\":20},{\"id_file\":1,\"ip\":\"192.168.0.3\",\"block_avail\":20},{\"id_file\":1,\"ip\":\"192.168.0.23\",\"block_avail\":20}]";
            List<file_host_rel> update = new List<file_host_rel>();
            dynamic objects = JsonConvert.DeserializeObject(fileDetail);
            dynamic obj;
            // if exist update, else insert
            // not yet tested
            foreach (var item in objects)
            {
                int id = item.id;
                string ip = item.ip;
                obj = from file_host_rel f in db
                      where f.id_file.Equals(id) && f.ip.Equals(ip)
                      select f;
                if (obj != null)
                {
                    obj.block_avail = item.block_avail;
                    db.Store(obj);
                } else
                    db.Store(item);
            }

        }
        // update information of your file at hub
        public void update(int fileId, int block_avail)
        {
            string command;
            dynamic fileInfo = from file_available f in db
                               where f.id_file == fileId
                               select f;
            command = "UP;;"+fileInfo.id_file+";;ipku;;"+fileInfo.block_avail;
        }
    }
}
