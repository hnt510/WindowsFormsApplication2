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

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        class User
        {
            public String NAME;
            public String CAR_NUMBER;
            public String PHONE_NUMBER;
            public String TIME;
        }
        bool isStop = false;

        IPEndPoint ipep;
        Socket serverSocket;
        Socket clientSocket;
        IPEndPoint clientep;
        Socket senderSocket;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form_1_Load(object sender, EventArgs e)
        {
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

            //add stuff in list
            for (int i = 0; i < usr.Length; i++)
            {
                add_listitem(usr[i].NAME, usr[i].CAR_NUMBER, usr[i].PHONE_NUMBER, usr[i].TIME);
            }
                

            listView1.Columns["ProductName"].Width = -1;//根据内容设置宽度
            listView1.Columns["SN"].Width = -2;//根据标题设置宽度

            listView1.Columns["Price"].Width = -2;
            listView1.Columns["Number"].Width = -2;
        }
        private void add_listitem(String NAME, String CAR_NUMBER, String PHONE_NUMBER, String TIME)
        {
            //添加表格内容
                ListViewItem item = new ListViewItem();
                item.SubItems.Clear();

                item.SubItems[0].Text = NAME;
                item.SubItems.Add(CAR_NUMBER);
                item.SubItems.Add(PHONE_NUMBER);
                item.SubItems.Add(TIME);
                listView1.Items.Add(item);
        }

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

        private User[] parse_xml(String filename) {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            XmlNode node = doc.SelectSingleNode("/GarageInfo");
            XmlNodeList nodeList = node.ChildNodes;
            int i = nodeList.Count;
            User[] all_usr = new User[i];
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
        private void button1_Click(object sender, EventArgs e)
        {
            Thread listenerThread = new Thread(new ThreadStart(listener));
            if (isStop == true)
            {
                stopListen();
                button1.Text = "Listen";
                isStop = false;
                listenerThread.Abort();
                
            }
            else
            {
                button1.Text = "Stop";
                ipep = new IPEndPoint(IPAddress.Any, 7631);
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(ipep);
                serverSocket.Listen(10);
                listenerThread.Start();
                MessageBox.Show("Start Listening on Port" + " 7631");
                isStop = true;
            }
        }

        private void stopListen()
        {
            //throw new NotImplementedException();
            serverSocket.Shutdown(SocketShutdown.Receive);
        }

        private void listener()
        {
            //throw new NotImplementedException();

            while (true)
            {
                try
                {
                    clientSocket = serverSocket.Accept();
                    Thread clientThread = new Thread(new ThreadStart(ReceiveData));
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
            //throw new NotImplementedException();
            bool keepalive = true;  
            Socket s = clientSocket;  
            Byte[] buffer = new Byte[1024];

            clientep = (IPEndPoint)s.RemoteEndPoint;
            while (keepalive)
            { 
                int bufLen = 0;                 
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
                clientep = (IPEndPoint)s.RemoteEndPoint; 
                string clientcommand = System.Text.Encoding.UTF8.GetString(buffer).Substring(0, bufLen);
                string [] split=clientcommand.Split(' ');
                                    Invoke(new MethodInvoker(delegate()
                    {
                        add_listitem(split[0], split[1], split[2], split[3]);
                        write_xml(split[0], split[1], split[2], split[3]);
                    }));
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            int count = listView1.SelectedItems.Count;
            if (count != 0)
            {
                for (int n = 0; n < count; n++)
                {
                    String s = listView1.SelectedItems[0].Text;
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load("data.xml");
                    XmlNode root = xmlDoc.SelectSingleNode("GarageInfo");
                    XmlNodeList xnl = xmlDoc.SelectSingleNode("GarageInfo").ChildNodes;
                    for (int i = 0; i < xnl.Count; i++)
                    {
                        XmlElement xe = (XmlElement)xnl.Item(i);
                        if (xe.GetAttribute("name") == s)
                        {
                            root.RemoveChild(xe);
                            if (i < xnl.Count) i = i - 1;
                        }
                    }
                    xmlDoc.Save("data.xml");
                    listView1.Items.Remove(listView1.SelectedItems[0]);

                    //send delete info
                    IPEndPoint mobileIP = new IPEndPoint(clientep.Address, 6001);
                    senderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    try  
                    {
                        senderSocket.Connect(ipep);              
                    }catch (SocketException ex)  
                    { 
                        MessageBox.Show("connect error: " + ex.Message);                  
                        return;
                    }
                    byte[] data = new byte[1024];
                    data = Encoding.UTF8.GetBytes(listView1.SelectedItems[0].Text);
                    senderSocket.Send(data, data.Length, SocketFlags.None);
                }
            }
        }


    }
}
