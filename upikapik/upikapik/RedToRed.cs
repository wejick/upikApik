﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text;
using System.IO;
using Db4objects.Db4o;
using Db4objects.Db4o.Linq;

namespace upikapik
{
    class AsynchRedServ : IDisposable
    {
        private string FILE_DIR = "music/";
        private const int MAX_THREAD = 5;
        private const int MSG_LENGTH_BYTE = 40; //masih ngasal, panjang dari request message
        private int threadCount = 0;
        private TcpListener server;
        public bool enable = true;
        private IPEndPoint ipServer;
        private ManualResetEvent allDone = new ManualResetEvent(false); // to track thread state

        public AsynchRedServ()
        {
            ipServer = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1337);
        }
        public AsynchRedServ(string ipAddress, int port)
        {
            ipServer = new IPEndPoint(IPAddress.Parse(ipAddress), port);
        }
        public void startListening()
        {
            try
            {
                server = new TcpListener(ipServer);
                server.Server.ReceiveTimeout = 3000;
                server.Start();
                while (enable)
                {
                    allDone.Reset();
                    server.BeginAcceptTcpClient(acceptCallback, server);
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private void acceptCallback(IAsyncResult result)
        {
            allDone.Set();

            string command;
            string[] parsedCommand;
            string filename;
            byte[] buffRead = new byte[MSG_LENGTH_BYTE];
            byte[] buffSend;
            int startPost;
            int size;
            TcpListener server = (TcpListener)result.AsyncState;
            TcpClient client = null;
            try
            {
                client = server.EndAcceptTcpClient(result);
            }
            catch (ObjectDisposedException ex)
            {
                return;
            }
            NetworkStream clientStream = client.GetStream();
            if (threadCount >= 5)
            {
                byte[] busy = System.Text.Encoding.UTF8.GetBytes("BUSY");
                clientStream.Write(busy, 0, busy.Length);
            }
            else
            {
                clientStream.Read(buffRead, 0, MSG_LENGTH_BYTE);
                command = System.Text.Encoding.UTF8.GetString(buffRead);

                //GET;filename;block_start,size
                parsedCommand = command.Split(';');

                filename = parsedCommand[1];
                startPost = Convert.ToInt16(parsedCommand[2]);
                size = Convert.ToInt16(parsedCommand[3]);
                buffSend = new byte[size];

                buffSend = getblocks(filename, startPost, size);
                //send
                clientStream.Write(buffSend, 0, size);
            }

            clientStream.Close();
            client.Close();
        }
        // get blocks from files
        private byte[] getblocks(string filename, int startPost, int size)
        {
            byte[] blocks;
            blocks = new byte[size];
            FileStream file = null;
            try
            {
                file = new FileStream(FILE_DIR + filename, FileMode.Open, FileAccess.Read);
                file.Seek(startPost, 0);
                file.Read(blocks, 0, size);
            }
            catch (FileLoadException ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                ((IDisposable)file).Dispose();
            }

            return blocks;
        }
        public void Dispose()
        {
            enable = false;
            this.server.Stop();
            allDone.Set();
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
    class AsynchRedStream
    {
        IObjectContainer db;
        file_list fileinfo;

        private const int MAX_REQUEST = 4;
        private int startpost;
        int id_file;

        private ManualResetEvent allDone = new ManualResetEvent(false);
        private object createReqLocker = new object();
        private bool enable = false ;

        Timer startTimer;
        private Queue<RequestProp> writeQueue = new Queue<RequestProp>(MAX_REQUEST);
        private Queue<RequestProp> failedRequestQueue = new Queue<RequestProp>();
        private Queue<Hosts> hosts;
 
        public AsynchRedStream(string dbName)
        {
            db = Db4oEmbedded.OpenFile(dbName);
        }
        public void startStream(int id_file, ref byte[] bassBuffer)
        {
            this.id_file = id_file;
            getFileInfo(id_file);
            string filename = getFilename();
            int blocksize = getBlockSize();
            int filesize = getFileSize();

            enable = true;
            startTimer = new Timer(x => { startTimerCallback(filename,blocksize,filesize); }, null, 0, 100); // is it better than forever while?
        }
        public void startTimerCallback(string filename, int blocksize, int filesize)
        {
            if(enable)
            {
                if (writeQueue.Count != 4)
                {
                    if ((startpost + blocksize) >= filesize)
                    {
                        stopStream();
                        startTimer.Dispose();
                    }
                    else if(failedRequestQueue.Count!=0)
                        startConnect(failedRequestQueue.Dequeue());
                    else
                        startConnect(createReq(filename,blocksize,filesize));
                }
                else
                    Thread.Sleep(50);
            }
        }
        public void stopStream()
        {
            enable = false;
        }
        private void startConnect(RequestProp req)
        {
            req.client = new TcpClient();

            allDone.Reset();
            try
            {
                req.client.BeginConnect(req.peer.Address, req.peer.Port, connectCallback, req);
            }
            catch (SocketException ex)
            {
                enqueueFailedRequest(req);
            }
            allDone.WaitOne();
        }
        private void connectCallback(IAsyncResult result)
        {
            allDone.Set();
            RequestProp req = (RequestProp)result.AsyncState;
            req.client.EndConnect(result);
            req.stream = req.client.GetStream();

            byte[] sendBuffer = Encoding.UTF8.GetBytes("GET;" + req.filename + ";" + req.startPost + ";" + req.blockSize + ";");
            req.receiveBuffer = new byte[req.blockSize];
            try
            {
                req.stream.Write(sendBuffer, 0, sendBuffer.Length);
            }
            catch (IOException ex)
            {
                enqueueFailedRequest(req);
            }
            Thread.Sleep(100);
            try
            {
                req.stream.BeginRead(req.receiveBuffer, 0, req.blockSize, readCallback, req);
            }
            catch (IOException ex)
            {
                enqueueFailedRequest(req);
            }
            printBlocks(req.receiveBuffer);
        }
        // at this 
        private void readCallback(IAsyncResult result)
        {
            RequestProp req = (RequestProp)result.AsyncState;
            req.stream.EndRead(result);
            byte[] receiveBuffer = new byte[req.blockSize];
            receiveBuffer = req.receiveBuffer;
            writeQueue.Enqueue(req);
        }
        private void printBlocks(byte[] blocks)
        {
            Console.WriteLine(System.Text.Encoding.UTF8.GetString(blocks));
        }
        private RequestProp createReq(string filename, int blocksize, int filesize)
        {
            RequestProp req = new RequestProp();
            req.blockSize = blocksize;
            req.filename = filename;
            req.startPost = startpost;
            req.peer.Port = 1337; // hard coded port

            lock (createReqLocker)
            {
                if ((startpost + blocksize) > filesize)
                    blocksize = filesize - startpost + 1;
                else
                    startpost = startpost + blocksize + 1;
                
                // get the address then enqueue it again
                // if block available from host smaller than requested, get another host
                while (true)
                {
                    Hosts peer = hosts.Dequeue();
                    hosts.Enqueue(peer);
                    if (peer.blockAvail >= startpost + blocksize)
                        break;
                }
            }
            return req;
        }
        private void getFileInfo(int id_file)
        {
            dynamic file = from file_list f in db where f.id_file.Equals(id_file) select f;
            foreach (var item in file)
            {
                fileinfo = item;
            }
        }
        private string getFilename()
        {
            return fileinfo.nama;
        }
        private int getFileSize()
        {
            return fileinfo.size;
        }
        private int getBlockSize()
        {
            int frameSize = (144 * fileinfo.bitrate * 1000) / fileinfo.samplerate;
            return (1500 / frameSize) * frameSize; 
        }
        private void getHostsAvail(int id_file)
        {
            hosts.Clear();
            dynamic H = from file_host_rel f in db where f.id_file.Equals(id_file) select f;
            foreach (var item in H)
            {
                hosts.Enqueue(item);
            }
        }
        public void updateHosts()
        {
            getHostsAvail(id_file);
        }
        private void enqueueFailedRequest(RequestProp req)
        {
            Hosts host = hosts.Dequeue(); 
            req.peer = host.peer;
            hosts.Enqueue(host);
            failedRequestQueue.Enqueue(req);
        }
    }
}
