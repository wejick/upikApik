using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text;
using System.IO;

namespace upikapik
{
    // declare it and use startstream
    // and then stopstream
    class AsynchRedStream
    {
        file_list fileinfo;

        private const int MAX_REQUEST = 4;
        private int startpost;
        int id_file;

        //byte[] bassBuffer;

        private ManualResetEvent allDone = new ManualResetEvent(false);
        private object createReqLocker = new object();
        private object writeByteLocker = new object();
        private object writeFileLocker = new object();
        private bool enable = false;

        Timer startTimer;
        private Queue<RequestProp> writeQueue = new Queue<RequestProp>(MAX_REQUEST);
        private Queue<RequestProp> bassBufferQueue = new Queue<RequestProp>();
        private Queue<RequestProp> failedRequestQueue = new Queue<RequestProp>();
        private Queue<Hosts> hosts;

        FileStream file = null;
        ManualResetEvent manualEvent = new ManualResetEvent(false);

        public AsynchRedStream()
        {
        }
        public void startStream(int id_file, file_list fileinfo)
        {
            this.fileinfo = fileinfo;
            this.id_file = id_file;
            string filename = getFilename();
            int blocksize = getBlockSize();
            int filesize = getFileSize();
            //this.bassBuffer = bassBuffer;

            file = new FileStream("music/" + filename, FileMode.OpenOrCreate, FileAccess.Write);

            enable = true;
            startTimer = new Timer(x => { startTimerCallback(filename, blocksize, filesize); }, null, 0, 100); // is it better than forever while?
        }
        public void startTimerCallback(string filename, int blocksize, int filesize)
        {
            if (enable)
            {
                if (writeQueue.Count != 0)
                {
                    writeToFile(writeQueue.Dequeue());
                }
                if (writeQueue.Count != 4)
                {
                    if ((startpost + blocksize) >= filesize)
                    {
                        stopStream();
                        startTimer.Dispose();
                    }
                    else if (failedRequestQueue.Count != 0)
                        startConnect(failedRequestQueue.Dequeue());
                    else
                        startConnect(createReq(filename, blocksize, filesize));
                }
                //else
                    //Thread.Sleep(50);
            }
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
        private void connectCallback(IAsyncResult result) // connect to another peer and send message to get block
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
            //Thread.Sleep(100);
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
        private void readCallback(IAsyncResult result) // read response from another peer and write it to file
        {
            RequestProp req = (RequestProp)result.AsyncState;
            req.stream.EndRead(result);
            byte[] receiveBuffer = new byte[req.blockSize];
            receiveBuffer = req.receiveBuffer;
            writeQueue.Enqueue(req);
            bassBufferQueue.Enqueue(req);
        }
        private void writeToFile(RequestProp req) // write to file
        {
            try
            {
                lock (writeFileLocker)
                {
                    file.Seek(req.startPost - 1, 0);
                    file.BeginWrite(req.receiveBuffer, 0, req.receiveBuffer.Length, writeCallback, null);
                }
            }
            catch (IOException ex)
            {

            }
        }
        private void writeCallback(IAsyncResult result)
        {
            RequestProp req = (RequestProp)result;
            file.EndWrite(result);
        }

        // tools methods
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
            req.peer = new IPEndPoint(IPAddress.Any, 1337);

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
        private void endEverything()
        {
            file.Close();
        }
        private void enqueueFailedRequest(RequestProp req)
        {
            Hosts host = hosts.Dequeue();
            req.peer = host.peer;
            hosts.Enqueue(host);
            failedRequestQueue.Enqueue(req);
        }

        // public method are intented to invoked by external event
        public void writeToBuffer(ref byte[] buffer)
        {
            while (bassBufferQueue.Count != 0)
            {
                RequestProp req = bassBufferQueue.Dequeue();
                int y = 0;
                for (int i = req.startPost; i < req.receiveBuffer.Length; i++)
                {
                    buffer[i] = req.receiveBuffer[y];
                    y++;
                }
            }
        }
        public void getHostsAvail(Queue<Hosts> hosts)
        {
            if (hosts.Count != 0)
                this.hosts.Clear();
            this.hosts = hosts;
        }
        public void stopStream()
        {
            enable = false;
            endEverything();
        }
    }
}
