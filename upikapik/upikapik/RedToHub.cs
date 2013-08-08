using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Db4objects.Db4o;
using Db4objects.Db4o.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace upikapik
{
    class RedToHub
    {
        IObjectContainer db;
        private string trackerIP { get; set; }
        private int port { get; set; }
        private IPEndPoint server;
        private string comToHub;
        private string response;
        private string[] parsedCommand;

        public RedToHub(string dbName, string trackerIP, int port)
        {
            db = Db4oEmbedded.OpenFile(dbName);
            this.trackerIP = trackerIP;
            this.port = port;
            server = new IPEndPoint(IPAddress.Parse(trackerIP), port);
        }

        // this is where the socket client and thread live :-D
        public void connect(string command)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(connectionHandler));
            this.comToHub = command;
            parsedCommand = comToHub.Split(';');
        }
        private void connectionHandler(object pStateObj)
        {
            //TcpClient red = new TcpClient(server);
            TcpClient red = new TcpClient();
            red.Connect(server);
            // create buffer and set encoding to UTF8
            //byte[] buffer = System.Text.Encoding.UTF8.GetBytes(comToHub);
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(comToHub);
            // prepare stream to write or read
            NetworkStream stream = red.GetStream();
            // send command to HUB
            if (parsedCommand[0] == "FL")
            {
                buffer = System.Text.Encoding.UTF8.GetBytes(parsedCommand[0]);
            }
            if (parsedCommand[0] == "FD")
            {
                buffer = System.Text.Encoding.UTF8.GetBytes(parsedCommand[0]+";;"+parsedCommand[1]);
            }
            if (parsedCommand[0] == "UP")
            {
                // command;;id_file;;block_avail
                buffer = System.Text.Encoding.UTF8.GetBytes(parsedCommand[0] + ";;" + parsedCommand[1] + ";;" + parsedCommand[2]);
            }
            if (parsedCommand[0] == "NA")
            {
                buffer = System.Text.Encoding.UTF8.GetBytes(parsedCommand[0]);
            }
            stream.Write(buffer, 0, buffer.Length);

            // clear buffer
            buffer = new Byte[1024];
            int byteCnt = stream.Read(buffer, 0, buffer.Length);
            if(parsedCommand[0] == "FL"||parsedCommand[0] == "FD")
                response = System.Text.Encoding.UTF8.GetString(buffer);
            if (parsedCommand[0] == "FL")
            {
                getFileList(response);
            }
            if (parsedCommand[0] == "FD")
            {
                getFileDetail(response,Convert.ToInt16(parsedCommand[1]));
            }
            stream.Close();
        }
        public void printResponse()
        {
            Console.WriteLine("Isi response : {0}", response);
        }
        private void getFileList(string response)
        {
            List<file_list> list = JsonConvert.DeserializeObject<List<file_list>>(response);
            dynamic all = from file_list f in db select f;
            // delete all
            foreach (var item in all)
            {
                db.Delete(item);
            }
            // write all
            foreach (var item in list)
            {
                db.Store(item);
            }
            Console.WriteLine(response);
        }
        private void getFileDetail(string response, int id)
        {
            string fileDetail = response;
            List<file_host_rel> update = JsonConvert.DeserializeObject<List<file_host_rel>>(response);
            dynamic all_ok = from file_host_rel f in db where f.id_file.Equals(id) select f;
            // delete all apropriate
            foreach (var item in all_ok)
            {
                db.Delete(item);
            }
            // insert all from response
            foreach (var item in update)
            {
                Console.WriteLine(item.ip);
                db.Store(item);
            }
            all_ok = from file_host_rel f in db where f.id_file.Equals(id) select f;
            foreach (var item in all_ok)
            {
                Console.WriteLine(item.ip);
            }
            Console.WriteLine(response);
        }
    }
}
