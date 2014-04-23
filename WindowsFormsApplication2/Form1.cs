using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Collections;

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        class User
        {
            //User data structure
            public String NAME;
            public String CAR_NUMBER;
            public String PHONE_NUMBER;
            public String TIME;
        }
        bool isListening = false;

        public IPEndPoint ipep;
        public Socket serverSocket;
        public Socket clientSocket;
        public IPEndPoint clientep;
        public Socket senderSocket;

        public Hashtable hshTable = new Hashtable();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form_1_Load(object sender, EventArgs e)
        {
            //only allow one application to run
            if (CheckIsRun())
                Application.Exit();
            //create data.xml
            if (!File.Exists("data.xml"))          
            {
                XmlDocument xmldoc;
                XmlNode xmlnode;
                XmlElement xmlelem;
                xmldoc = new XmlDocument();
                XmlDeclaration xmldecl;
                xmldecl = xmldoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                xmldoc.AppendChild(xmldecl);

                //加入一个根元素
                xmlelem = xmldoc.CreateElement("", "GarageInfo", "");
                xmldoc.AppendChild(xmlelem);
                xmldoc.Save("data.xml");
            }

            //parse xml file
            User [] usr=parse_xml("data.xml");


            listView1.GridLines = true;//表格是否显示网格线
            listView1.FullRowSelect = true;//是否选中整行

            listView1.View = View.Details;//设置显示方式
            listView1.Scrollable = true;//是否自动显示滚动条
            listView1.MultiSelect = false;//是否可以选择多行

            //添加表头（列）
            listView1.Columns.Add("NAME", "车主姓名");
            listView1.Columns.Add("CAR_NUMBER", "车牌号");
            listView1.Columns.Add("PHONE_NUMBER", "电话号码");
            listView1.Columns.Add("TIME", "入库时间");

            //add stuff in listview
            for (int i = 0; i < usr.Length; i++)
            {
                add_listitem(usr[i].NAME, usr[i].CAR_NUMBER, usr[i].PHONE_NUMBER, usr[i].TIME);
            }


            listView1.Columns["NAME"].Width = -2;//根据内容设置宽度
            listView1.Columns["CAR_NUMBER"].Width = -2;//根据标题设置宽度

            listView1.Columns["PHONE_NUMBER"].Width = -2;
            listView1.Columns["TIME"].Width = -2;
        }

        
        private void add_listitem(String NAME, String CAR_NUMBER, String PHONE_NUMBER, String TIME)
        {
            //添加表格内容 add a row into listview
                ListViewItem item = new ListViewItem();
                item.SubItems.Clear();

                item.SubItems[0].Text = NAME;
                item.SubItems.Add(CAR_NUMBER);
                item.SubItems.Add(PHONE_NUMBER);
                item.SubItems.Add(TIME);
                listView1.Items.Add(item);
        }

        //write a user's info into xml file as a child node
        private void write_xml(String NAME, String CAR_NUMBER, String PHONE_NUMBER, String TIME)
        {

            //加入另外一个元素
            XmlDocument doc = new XmlDocument();
            doc.Load("data.xml");
            XmlNode root = doc.SelectSingleNode("GarageInfo");//查找<Employees> 
            XmlElement xe1 = doc.CreateElement("User");//创建一个<Node>节点 
            xe1.SetAttribute("name", NAME);//设置该节点genre属性 
            xe1.SetAttribute("carnum", CAR_NUMBER);//设置该节点ISBN属性
            xe1.SetAttribute("phonenum", PHONE_NUMBER);
            xe1.SetAttribute("TIME", TIME);
            root.AppendChild(xe1);//添加到<Employees>节点中 
            doc.Save("data.xml");
        }

        //read existing xml data
        private User[] parse_xml(String filename) {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            XmlNode node = doc.SelectSingleNode("/GarageInfo");
            XmlNodeList nodeList = node.ChildNodes;
            int i = nodeList.Count;
            User[] all_usr = new User[i];

            //read all data into User[] array
            for(int j=0;j<i;j++)
            {
                XmlNode internal_node = nodeList.Item(j);
                if (internal_node != null)
                {
                    User user = new User();
                    user.NAME = internal_node.Attributes["name"].Value;
                    user.CAR_NUMBER = internal_node.Attributes["carnum"].Value;
                    user.PHONE_NUMBER = internal_node.Attributes["phonenum"].Value;
                    user.TIME = internal_node.Attributes["TIME"].Value;
                    all_usr[j] = user;
                }
            }
            return all_usr;
        }

        //Listen button onclick
        private void button1_Click(object sender, EventArgs e)
        {
            Thread listenerThread = new Thread(new ThreadStart(listener));
            listenerThread.IsBackground = true;
            if (isListening == false)
            {
                //set connection
                ipep = new IPEndPoint(IPAddress.Any, 7631);
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(ipep);
                serverSocket.Listen(10);

                //start a listener thread in order to avoid jamming the main thread 
                listenerThread.Start();
                MessageBox.Show("Start Listening on Port" + " 7631");
                isListening = true;
            }
            else {
                MessageBox.Show("Already Listening");
            }
 
        }

        //lintener thread operation
        private void listener()
        {

            while (true)
            {
                try
                {
                    //create a new thread when a tcp connection accepted to recive data
                    clientSocket = serverSocket.Accept();
                    Thread clientThread = new Thread(new ThreadStart(ReceiveData));
                    clientThread.IsBackground = true;
                    clientThread.Start();
                 
                }
                catch (Exception ex)
                {
                    MessageBox.Show("listening Error: " + ex.Message);
                }
            }
        }

        private void ReceiveData()
        {
            bool keepalive = true;  
            Socket s = clientSocket;  
            Byte[] buffer = new Byte[1024];

            clientep = (IPEndPoint)s.RemoteEndPoint;
            while (keepalive)
            { 
                int bufLen = 0;             
                //client socket recive
                try 
                { 
                    bufLen = s.Available;
                    s.Receive(buffer, 0, bufLen, SocketFlags.None); 
                    if (bufLen == 0) 
                        continue; 
                }catch (Exception ex) 
                { 
                    MessageBox.Show("Receive Error:" + ex.Message);                   
                    return; 
                } 
                //get client ip
                clientep = (IPEndPoint)s.RemoteEndPoint;
                
                //get stuff from buffer
                string clientcommand = System.Text.Encoding.UTF8.GetString(buffer).Substring(0, bufLen);
                string[] stringSeparators = new string[] { "EOF" };
                string[] split = clientcommand.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);


                //store username ipAddress pair so i can send info to corresponding client
                hshTable.Add(split[0],clientep.Address);

                //write recived info in listview and xml file through safe thread operation
                Invoke(new MethodInvoker(delegate()
                {
                    add_listitem(split[0], split[1], split[2], split[3]);
                    write_xml(split[0], split[1], split[2], split[3].Trim());
                }));
                keepalive = false;
            }
            s.Shutdown(SocketShutdown.Receive);
            s.Close();
            Thread.CurrentThread.Abort();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        //Car Goes Out button onclick
        private void button3_Click(object sender, EventArgs e)
        {
            IPEndPoint mobilePoint;
            int count = listView1.SelectedItems.Count;
            if (count != 0)
            {
                for (int n = 0; n < count; n++)
                {
                    //delete item in xml file
                    String name = listView1.SelectedItems[0].Text;
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load("data.xml");
                    XmlNode root = xmlDoc.SelectSingleNode("GarageInfo");
                    XmlNodeList xnl = xmlDoc.SelectSingleNode("GarageInfo").ChildNodes;
                    for (int i = 0; i < xnl.Count; i++)
                    {
                        XmlElement xe = (XmlElement)xnl.Item(i);
                        if (xe.GetAttribute("name") == name)
                        {
                            root.RemoveChild(xe);
                            if (i < xnl.Count) i = i - 1;
                        }
                    }
                    xmlDoc.Save("data.xml");

                    //calculate fee
                    string TIME = listView1.SelectedItems[0].SubItems[3].Text.Substring(0, 6);
                    int duration;
                    //f**k this stupid c# time function
                    if (DateTime.Now.Minute.ToString().Length == 1)
                    {
                        duration = convertTime(DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + "0"+DateTime.Now.Minute.ToString()) - convertTime(TIME);
                    }
                    else
                    {
                        duration = convertTime(DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) - convertTime(TIME);
                    }
                    if (duration <= 60)
                    {
                        double fee = duration * 0.1;
                        MessageBox.Show("您目前已停" + duration.ToString()
                                + "分钟，需要交" + fee.ToString() + "元");
                    }
                    else if (duration <= 120)
                    {
                        double fee = 60 * 0.1 + (duration - 60) * 0.2;
                        MessageBox.Show("您目前已停" + duration.ToString()
                                + "分钟，需要交" + fee.ToString() + "元");
                    }
                    else
                    {
                        double fee = 60 * 0.1 + 60 * 0.2 + (duration - 120) * 0.3;
                        MessageBox.Show("您目前已停" + duration.ToString()
                                + "分钟，需要交" + fee.ToString() + "元");
                    }

                    //send delete info to corresponding user using ip address in hshTable 
                    IPAddress reciveIP = (IPAddress)hshTable[name];

                    //I just broadcast delete info if reciveIP is null(when user's info is loaded from xml)
                    if (reciveIP != null)
                    {
                        mobilePoint = new IPEndPoint(reciveIP, 24358);
                        senderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        try
                        {
                            senderSocket.Connect(mobilePoint);
                        }
                        catch (SocketException ex)
                        {
                            MessageBox.Show("connect error: " + ex.Message);
                            listView1.Items.Remove(listView1.SelectedItems[0]);
                            return;
                        }
                        byte[] data = new byte[1024];
                        data = Encoding.UTF8.GetBytes(listView1.SelectedItems[0].Text);
                        senderSocket.Send(data, data.Length, SocketFlags.None);
                        senderSocket.Shutdown(SocketShutdown.Both);
                        senderSocket.Close();
                    }
                    else { 
                        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);//初始化一个Scoket实习,采用UDP传输
                        IPEndPoint iep = new IPEndPoint(IPAddress.Broadcast, 24358);
                        sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                        byte[] data = new byte[1024];
                        data = Encoding.UTF8.GetBytes(listView1.SelectedItems[0].Text);
                        sock.SendTo(data,iep);
                        sock.Close();
                    }
                    //remove list item
                    listView1.Items.Remove(listView1.SelectedItems[0]);
                }
            }
        }

        //convert current time
        private int convertTime(string time)
        {
            char[] c = new char[time.Length];
            c = time.ToCharArray();
            int dd = (c[0] - '0') * 10 + (c[1] - '0');
            int hh = (c[2] - '0') * 10 + (c[3] - '0');
            int mm = (c[4] - '0') * 10 + (c[5] - '0');
            return mm + hh * 60 + dd * 60 * 24;
        }

        private bool CheckIsRun() 
        {
            string procName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            if (System.Diagnostics.Process.GetProcessesByName(procName).GetUpperBound(0) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
