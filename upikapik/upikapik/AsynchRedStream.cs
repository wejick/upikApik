using System;
using System.Collections;
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
        IPAddress own = IPAddress.Parse("192.168.0.7");

        file_list fileinfo;

        private const int MAX_REQUEST = 10;
        int id_file;
        byte[] bassBuffer;

        private ManualResetEvent allDone = new ManualResetEvent(false);
        private object createReqLocker = new object();
        private object writeBufferLocker = new object();
        private object writeFileLocker = new object();
        private bool enable = false;
        private bool enStream = false;
        private int startpost = 0;

        private Timer startTimer;
        private Queue<RequestProp> writeQueue = new Queue<RequestProp>(MAX_REQUEST);
        private SortedList bufferList = new SortedList();
        private SortedList writeList = new SortedList();
        private Queue<RequestProp> bassBufferQueue = new Queue<RequestProp>();
        private Queue<RequestProp> failedRequestQueue = new Queue<RequestProp>();
        private Queue<int> starpostQueue = new Queue<int>();
        private Queue<Hosts> hosts;
        private SortedList writedStartpost = new SortedList();

        private static int lastAdjacentKey = 0;
        private static int bufferTurn = 0;
        private static int writeTurn = 0;

        int blocksize;

        FileStream file = null;
        ManualResetEvent manualEvent = new ManualResetEvent(false);

        // variable to hold test value
        static int ignoredConnection;
        List<string> requestAndHost = new List<string>();

        public AsynchRedStream()
        {
        }
        public void startStream(int id_file, file_list fileinfo)
        {            
            this.fileinfo = fileinfo;
            this.id_file = id_file;
            string filename = getFilename();
            blocksize = getBlockSize();
            int filesize = getFileSize();
            bassBuffer = new byte[filesize];

            file = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "music/" + filename, FileMode.OpenOrCreate, FileAccess.Write,FileShare.ReadWrite);

            enable = true;
            enStream = true;
            createStartPostQueue(blocksize, filesize);
            startTimer = new Timer(x => { startTimerCallback(filename, blocksize, filesize); }, null, 0, 500); // is it better than forever while?
        }
        private void startTimerCallback(string filename, int blocksize, int filesize)
        {
            // keep timer always alive
            startTimer.Change(0, 500);
            if (enable)
            {
                if(writeList.Count != MAX_REQUEST)
                {
                    if (enStream)
                    {
                        if (failedRequestQueue.Count != 0)
                            startConnect(failedRequestQueue.Dequeue());
                        else if (starpostQueue.Count != 0)
                            startConnect(createRequest(filename, blocksize, filesize));
                    }
                    lock (writeBufferLocker)
                    {
                        if (writeList.Count != 0)
                            writeToFile();
                        else if (writeList.Count == 0 && enStream == false)
                        {
                            stopStream();
                            startTimer.Dispose();
                        }
                        writeToBuffer();
                    }
                }
            }
            else
                reset();
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
            lock (writeBufferLocker)
            {
                //writeQueue.Enqueue(req);
                writeList.Add(req.startPost, req);
                bufferList.Add(req.startPost, req);
                if(enable == false)
                {
                    reset();
                }
            }
        }
        private void writeToFile() // write to file
        {
            RequestProp req = (RequestProp)writeList.GetByIndex(0);
            requestAndHost.Add(req.peer.Address.ToString());
            if (req.startPost == writeTurn)
            {
                try
                {
                    lock (writeFileLocker)
                    {
                        if (req.startPost == 0)
                            file.Seek(req.startPost, SeekOrigin.Begin);
                        else
                            file.Seek(req.startPost+1, SeekOrigin.Begin);
                        //file.BeginWrite(req.receiveBuffer, 0, req.receiveBuffer.Length, writeCallback, req);
                        file.Write(req.receiveBuffer, 0, req.receiveBuffer.Length);
                        writedStartpost.Add(req.startPost, req.startPost);
                        writeList.Remove(req.startPost);
                        writeTurn = writeTurn + blocksize + 1;
                    }
                }
                catch (IOException ex)
                {

                }
            }
        }
        private void writeToBuffer()
        {
            if (bufferList.Count != 0)
            {
                RequestProp tmp = (RequestProp)bufferList.GetByIndex(0);
                if (tmp.startPost == bufferTurn)
                {
                    int y = 0;
                    for (int i = tmp.startPost; i < tmp.blockSize - 1 + tmp.startPost; i++)
                    {
                        if (i == bassBuffer.Length)
                            break;
                        bassBuffer[i] = tmp.receiveBuffer[y];
                        y++;
                    }
                    bufferList.Remove(tmp.startPost);
                    bufferTurn = bufferTurn + blocksize + 1;
                }
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
                if (startpost + blocksize >= filesize)
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
                // if the address is same as our address, continue
                while (true)
                {
                    Hosts peer = hosts.Dequeue();
                    hosts.Enqueue(peer);
                    if (own.ToString() == peer.peer.Address.ToString() )
                        continue;
                    else
                    {
                        req.peer = peer.peer;                        
                        if ((peer.blockAvail * blocksize) >= startpost + req.blockSize)
                            break;
                    }
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
        private void enqueueFailedRequest(RequestProp req)
        {
            ignoredConnection++;
            Hosts host = hosts.Dequeue();
            req.peer = host.peer;
            hosts.Enqueue(host);
            failedRequestQueue.Enqueue(req);
        }
        private void reset()
        {
            bufferTurn = 0;
            writeTurn = 0;
            writedStartpost.Clear();
            writeList.Clear();
            bufferList.Clear();
        }
        // public method are intended to invoked by external event
        public void closeFile()
        {
            if (file != null)
                file.Close();
        }
        public int getBlockSize()
        {
            int frameSize = (144 * fileinfo.bitrate * 1000) / fileinfo.samplerate;
            //return (1500 / frameSize) * frameSize;
            return (12000 / frameSize) * frameSize;
        }
        public byte[] getBuffer()
        {
            return bassBuffer;
        }
        public int getFrameSize()
        {
            int frameSize = (144 * fileinfo.bitrate * 1000) / fileinfo.samplerate;
            return frameSize;
        }
        public void getHostsAvail(Queue<Hosts> hosts)
        {
            this.hosts = hosts;
        }
        public int getLastAdjacentValue()
        {
            int next;
            int current;
            int blockSize = getBlockSize();
            int last = 0;
            for (int i = lastAdjacentKey; i < writedStartpost.Count - 1; i++)
            {
                current = (int)writedStartpost.GetByIndex(i) + 1;
                next = (int)writedStartpost.GetByIndex(i + 1);
                if ((current + blockSize) == next)
                    last = next;
                else
                {
                    lastAdjacentKey = i - 1;
                }
            }
            return last;
        }
        public int getLastAdjacentBufferValue()
        {
            RequestProp next;
            RequestProp current;
            int blockSize = getBlockSize();
            int last = 0;
            for (int i = 0; i < bufferList.Count - 1; i++)
            {
                current = (RequestProp)bufferList.GetByIndex(i);
                next = (RequestProp)bufferList.GetByIndex(i + 1);
                if ((current.startPost + 1 + blockSize) == next.startPost)
                    last = next.startPost;
            }
            return last;
        }
        public int getLastValueOfBuffer()
        {
            if(bufferList.Count != 0){
                RequestProp tmp = (RequestProp)bufferList.GetByIndex(bufferList.Count - 1);
                return tmp.startPost + blocksize;
            }
            return 0;
        }
        public int getTestIgnoredSession()
        {
            return ignoredConnection;
        }
        public List<string> getTestRequestAndHost()
        {
            return requestAndHost;
        }
        public void stopStream()
        {
            ignoredConnection = 0;            
            enable = false;
            reset();
        }
        public void writeToStream(IntPtr streamBuffer)
        {
            Marshal.Copy(bassBuffer, 0, streamBuffer, bassBuffer.Length);
        }
    }
}
