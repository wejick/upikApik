using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace upikapik
{
    // declare it and use startstream
    // and then stopstream
    class AsynchRedStream
    {
        file_list fileinfo;

        private const int MAX_REQUEST = 10;
        private int startpost;
        int id_file;
        byte[] bassBuffer;

        private ManualResetEvent allDone = new ManualResetEvent(false);
        private object createReqLocker = new object();
        private object writeQueueLocker = new object();
        private object writeFileLocker = new object();
        private bool enable = false;
        private bool enStream = false;
        private bool enWrite = false;

        private Timer startTimer;
        private Queue<RequestProp> writeQueue = new Queue<RequestProp>(MAX_REQUEST);
        private Queue<RequestProp> bassBufferQueue = new Queue<RequestProp>();
        private Queue<RequestProp> failedRequestQueue = new Queue<RequestProp>();
        private Queue<int> starpostQueue = new Queue<int>();
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
            bassBuffer = new byte[filesize];

            file = new FileStream("music/" + filename, FileMode.OpenOrCreate, FileAccess.Write);

            enable = true;
            enStream = true;
            enWrite = true;
            createStartPostQueue(blocksize, filesize);
            startTimer = new Timer(x => { startTimerCallback(filename, blocksize, filesize); }, null, 0, 500); // is it better than forever while?
        }
        private void startTimerCallback(string filename, int blocksize, int filesize)
        {
            // keep timer always alive
            startTimer.Change(0, 500);
            if (enable)
            {
                if(enStream)
                {
                    if (!(isWriteQueueFull()))
                    {                       
                        if (failedRequestQueue.Count != 0)
                            startConnect(failedRequestQueue.Dequeue());
                        else if(starpostQueue.Count != 0)
                            startConnect(createRequest(filename, blocksize, filesize));
                    }
                }
                lock (writeQueueLocker)
                {
                    if (writeQueue.Count != 0)
                    {
                        writeToFile(writeQueue.Dequeue());
                    }

                    else if (writeQueue.Count == 0 && enStream == false)
                    {
                        stopStream();
                        startTimer.Dispose();
                    }
                }
                writeToBuffer(); // write to internal bassBuffer
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

            try
            {
                req.stream.BeginRead(req.receiveBuffer, 0, req.blockSize, readCallback, req);
            }
            catch (IOException ex)
            {
                enqueueFailedRequest(req);
            }

        }
        private void readCallback(IAsyncResult result) // read response from another peer and write it to file
        {
            RequestProp req = (RequestProp)result.AsyncState;
            req.stream.EndRead(result);
            byte[] receiveBuffer = new byte[req.blockSize];
            receiveBuffer = req.receiveBuffer;
            lock (writeQueueLocker)
            {
                writeQueue.Enqueue(req);
                bassBufferQueue.Enqueue(req);
            }
        }
        private void writeToFile(RequestProp req) // write to file
        {
            if (req == null)
                return;
            try
            {
                lock (writeFileLocker)
                {
                    if (req.startPost == 0)
                        file.Seek(req.startPost, SeekOrigin.Begin);
                    else
                        file.Seek(req.startPost, SeekOrigin.Begin);
                    file.BeginWrite(req.receiveBuffer, 0, req.receiveBuffer.Length, writeCallback, req);
                }
            }
            catch (IOException ex)
            {

            }
        }
        private void writeCallback(IAsyncResult result)
        {
            //RequestProp req = (RequestProp)result;
            file.EndWrite(result);
            if (writeQueue.Count == 0 && enStream == false)
            {
                    //endEverything();
            }
        }

        // tools methods
        private void printBlocks(byte[] blocks)
        {
            Console.WriteLine(System.Text.Encoding.UTF8.GetString(blocks));
        }
        private void createStartPostQueue(int blocksize, int filesize)
        {
            int startpost = 0;
            starpostQueue.Enqueue(startpost);
            while (true)
            {
                if ((startpost + blocksize) > filesize)
                    blocksize = filesize - startpost + 1;
                else
                    startpost = startpost + blocksize + 1;
                starpostQueue.Enqueue(startpost);
                if (startpost+blocksize >= filesize)
                    break;
            }
        }
        private RequestProp createRequest(string filename, int blocksize, int filesize)
        {
            RequestProp req = new RequestProp();
            req.blockSize = blocksize;
            req.filename = filename;
            req.startPost = starpostQueue.Dequeue();
            req.peer = new IPEndPoint(IPAddress.Any, 1338);
            
            if ((startpost + blocksize) > filesize)
                req.blockSize = filesize - req.startPost + 1;
            
            lock (createReqLocker)
            {
                // get the address then enqueue it again
                // if block available from host smaller than requested, get another host
                while (true)
                {
                    Hosts peer = hosts.Dequeue();
                    req.peer = peer.peer;
                    hosts.Enqueue(peer);
                    if ((peer.blockAvail * blocksize) >= startpost + req.blockSize)
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
            //return (1500 / frameSize) * frameSize;
            return (12000 / frameSize) * frameSize;
        }            
        public void endEverything()
        {
            if(file != null)
                file.Close();
        }
        private void enqueueFailedRequest(RequestProp req)
        {
            Hosts host = hosts.Dequeue();
            req.peer = host.peer;
            hosts.Enqueue(host);
            failedRequestQueue.Enqueue(req);
        }
        private void writeToBuffer()
        {
            RequestProp req;
            while (bassBufferQueue.Count != 0)
            {
                req = bassBufferQueue.Dequeue();
                int y = 0;
                for (int i = req.startPost; i < req.blockSize - 1 + req.startPost; i++)
                {
                    if (i == bassBuffer.Length)
                        break;
                    bassBuffer[i] = req.receiveBuffer[y];
                    y++;
                }
            }
        }
        public void byteToBuffer(IntPtr buff, byte[] buffer)
        {
            Marshal.Copy(buffer, 0, buff, buffer.Length);
        }
        // public method are intented to invoked by external event
        public void writeToBuffer(ref byte[] buffer)
        {
            RequestProp req;
            while (bassBufferQueue.Count != 0)
            {
                req = bassBufferQueue.Dequeue();
                int y = 0;
                for (int i = req.startPost; i < req.blockSize - 1 + req.startPost; i++)
                {
                    if (i == buffer.Length)
                        break;
                    buffer[i] = req.receiveBuffer[y];
                    y++;
                }
            }
        }
        public byte[] getBuffer()
        {
            return bassBuffer;
        }
        public void getHostsAvail(Queue<Hosts> hosts)
        {
            //if (this.hosts.Count != 0)
            //    this.hosts.Clear();
            this.hosts = hosts;
        }
        public void stopStream()
        {
            enable = false;
        }
        public bool isWriteQueueFull()
        {
            if (writeQueue.Count == MAX_REQUEST)
                return true;
            return false;
        }

    }
}
