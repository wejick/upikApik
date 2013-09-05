using System;
using System.Collections.Generic;
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
        private const int MSG_LENGTH_BYTE = 100; //masih ngasal, panjang dari request message
        private int threadCount = 0;
        private TcpListener server;
        public bool enable = true;
        private IPEndPoint ipServer;
        private ManualResetEvent allDone = new ManualResetEvent(false); // to track thread state

        public AsynchRedServ()
        {
            ipServer = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1338);
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
                server.Server.ReceiveTimeout = 8000;
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
                try
                {
                    clientStream.Read(buffRead, 0, MSG_LENGTH_BYTE);
                    command = System.Text.Encoding.UTF8.GetString(buffRead);

                    //GET;filename;block_start,size
                    parsedCommand = command.Split(';');

                    filename = parsedCommand[1];
                    startPost = Convert.ToInt32(parsedCommand[2]);
                    size = Convert.ToInt16(parsedCommand[3]);
                    buffSend = new byte[size];

                    buffSend = getblocks(filename, startPost, size);
                    //send
                    clientStream.Write(buffSend, 0, size);
                }
                catch (TimeoutException ex)
                {
                    clientStream.Close();
                    client.Close();
                }
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

}
