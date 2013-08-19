using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text;
using System.IO;

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
        public TcpClient client;
        public NetworkStream stream;
        public string filename;
        public int startPost;
        public int blockSize;
        public byte[] receiveBuffer;
    }
    class AsynchRedStream
    {
        private IPEndPoint ipServer;
        private int port;
        private int MAX_REQUEST = 4;
        private int numOfRequest = 0;
        private int startpost;
        private ManualResetEvent allDone = new ManualResetEvent(false);
        public AsynchRedStream()
        {
            ipServer = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1337);
        }
        public AsynchRedStream(string ipAddress, int port)
        {
            ipServer = new IPEndPoint(IPAddress.Parse(ipAddress), port);
        }
        public void startStream(string id_file)
        {
            IPEndPoint host = new IPEndPoint(IPAddress.Parse("192.168.0.7"), 1337);
            startConnect(host, "Kalimba.mp3", 0, 400);
            startConnect(host, "Kalimba.mp3", 401, 400);
        }
        private void startConnect(IPEndPoint peer, string filename, int startpost, int blockSize)
        {
            RequestProp req = new RequestProp();
            req.client = new TcpClient();
            req.filename = filename;
            req.startPost = startpost;
            req.blockSize = blockSize;

            allDone.Reset();
            req.client.BeginConnect(peer.Address, peer.Port, connectCallback, req);
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
            req.stream.Write(sendBuffer, 0, sendBuffer.Length);
            Thread.Sleep(100);
            req.stream.BeginRead(req.receiveBuffer, 0, req.blockSize, readCallback, req);
            printBlocks(req.receiveBuffer);
        }
        // at this 
        private void readCallback(IAsyncResult result)
        {
            RequestProp req = (RequestProp)result.AsyncState;
            req.stream.EndRead(result);
            byte[] receiveBuffer = new byte[req.blockSize];
            receiveBuffer = req.receiveBuffer;
            printBlocks(receiveBuffer);
        }
        private void printBlocks(byte[] blocks)
        {
            Console.WriteLine(System.Text.Encoding.UTF8.GetString(blocks));
        }
    }
}
