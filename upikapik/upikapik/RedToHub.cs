﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
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
        private bool connect = true; // potential race condition
        private int fileCnt;
        public RedToHub(string dbName, string trackerIP, int port)
        {
            db = Db4oEmbedded.OpenFile(dbName);
            this.trackerIP = trackerIP;
            this.port = port;
            server = new IPEndPoint(IPAddress.Parse(trackerIP), port);
        }
        // is connect
        public bool isConnect()
        {
            return connect;
        }
        // how much file in network
        public int howMuch()
        {
            return fileCnt;
        }
        // this is where the socket client and thread live :-D
        public void command(string command)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(connectionHandler));
            this.comToHub = command;
            parsedCommand = comToHub.Split(';');
        }
        // get file list from db
        public List<file_list> getFileList()
        {
            List<file_list> fileList = new List<file_list>();
            dynamic files = from file_list f in db select f;
            //fileCnt = files.count();
            fileCnt = 0;
            foreach(var item in files)
            {
                fileList.Add(item);
                fileCnt++;
            }
            return fileList;
        }
        // parse command and choose suitable command to execute
        private void connectionHandler(object pStateObj)
        {
            //TcpClient red = new TcpClient(server);
            TcpClient red = new TcpClient();
            red.SendTimeout = 3000;
            red.ReceiveTimeout = 3000;
            try
            {
                red.Connect(server);
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.ErrorCode + " " + ex.InnerException);
                connect = false;
            }
            if (connect == true)
            {
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
                    buffer = System.Text.Encoding.UTF8.GetBytes(parsedCommand[0] + ";;" + parsedCommand[1]);
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
                if (parsedCommand[0] == "ADD")
                {
                    // command;;file_name;;bitrate;;samplerate;;size
                    buffer = System.Text.Encoding.UTF8.GetBytes(parsedCommand[0] + ";;" + parsedCommand[1] + ";;" + parsedCommand[2] + ";;" + parsedCommand[3] + ";;" + parsedCommand[4]);
                }
                stream.Write(buffer, 0, buffer.Length);

                // clear buffer
                buffer = new Byte[1024];
                int byteCnt = stream.Read(buffer, 0, buffer.Length);
                if (parsedCommand[0] == "FL" || parsedCommand[0] == "FD")
                    response = System.Text.Encoding.UTF8.GetString(buffer);
                if (parsedCommand[0] == "FL")
                {
                    comFileList(response);
                }
                if (parsedCommand[0] == "FD")
                {
                    comFileDetail(response, Convert.ToInt16(parsedCommand[1]));
                }
                stream.Close();
                red.Close();
            }
        }
        // send command and retrive file list from hub
        private void comFileList(string response)
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
        // send command and retrieve file detail from hub
        private void comFileDetail(string response, int id)
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

        // Add entry to file_available db
        public void addFileAvail(string filePath, int block_avail = 0)
        {
            storeFileAvailable(getInfo(filePath, block_avail));
        }
        // block_avail = 0 is full
        public file_available getInfo(string filePath,int block_avail=0)
        {
            MP3Header reader = new MP3Header();
            file_available file = new file_available();
            
            reader.ReadMP3Information(filePath);
            file.nama = Path.GetFileName(Uri.UnescapeDataString(filePath).Replace("/", "\\"));
            file.bitrate = reader.intBitRate;
            file.samplerate = reader.intFrequency;
            file.size = Convert.ToInt32(reader.lngFileSize);
            // there are ambuguity between block and frame, actually this is same thing
            if (block_avail == 0)
            {
                file.block_avail = reader.intTotalFrame;
                file.full = true;
            }
            else
            {
                file.block_avail = block_avail;
                file.full = false;
            }            

            return file;
        }
        public void storeFileAvailable(file_available file)
        {
            int fileIdInHub = 0;
            db.Store(file);
            this.command("ADD;"+file.nama+";"+file.bitrate+";"+file.samplerate+";"+file.size);
            Thread.Sleep(500);
            this.command("FL");
            Thread.Sleep(5000);
            dynamic t = from file_list sip in db select sip;
            //dynamic f = from file_list sip in db where sip.nama.Equals(file.nama) select sip;
            foreach (var item in t)
            {
                if(item.nama == file.nama)
                    fileIdInHub = item.id_file;
            }
            this.command("UP;" + fileIdInHub + ";" + file.block_avail);
        }
        public Queue<Hosts> getAvailableHost(int id_file)
        {
            Hosts host = new Hosts();
            Queue<Hosts> hosts = new Queue<Hosts>();

            String strHostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
            IPAddress[] addr = ipEntry.AddressList;

            dynamic obj = from file_host_rel f in db where f.id_file.Equals(id_file) select f;
            foreach(var item in obj)
            {
                foreach (IPAddress address in addr)
                {
                    if (address.Address == item.ip)
                    {
                        break;
                    }
                    else
                    {
                        host.blockAvail = item.block_avail;
                        host.peer.Address = IPAddress.Parse(item.ip);
                        host.peer.Port = 1337;

                        hosts.Enqueue(host);
                    }
                }
            }
            return new Queue<Hosts>();
        }
        public void updateFileInfo(int id_file, int blockAvailable)
        {
            file_available file = new file_available();
            dynamic f = from file_list g in db where g.id_file.Equals(id_file) select g;
            foreach (var item in f)
            {
                db.Delete(item);

                file.id_file = id_file;
                file.nama = item.nama;
                file.samplerate = item.samplerate;
                file.bitrate = item.bitrate;
                file.size = item.size;
                if (blockAvailable == item.size)
                {
                    file.full = true;
                }
                else
                    file.full = false;
                db.Store(item);
            }
        }
    }
}
