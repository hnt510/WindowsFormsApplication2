using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void Form_1_Load(object sender, EventArgs e)
        {

            //write a xml then parse it!
            write_xml("HUANG", "皖G45678", "18317710201", "140419");

            //parse xml
            String [] s =parse_xml("data.xml");

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
            add_listitem(s[0],s[1],s[2],s[3]);

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
            XmlDocument xmldoc;
            XmlNode xmlnode;
            XmlElement xmlelem;

            xmldoc = new XmlDocument();
            XmlDeclaration xmldecl;
            xmldecl = xmldoc.CreateXmlDeclaration("1.0", "gb2312", null);
            xmldoc.AppendChild(xmldecl);

            //加入一个根元素
            xmlelem = xmldoc.CreateElement("", "GarageInfo", "");
            xmldoc.AppendChild(xmlelem);

            //加入另外一个元素

            XmlNode root = xmldoc.SelectSingleNode("GarageInfo");//查找<Employees> 
            XmlElement xe1 = xmldoc.CreateElement("User");//创建一个<Node>节点 
            xe1.SetAttribute("name", NAME);//设置该节点genre属性 
            xe1.SetAttribute("carnum", CAR_NUMBER);//设置该节点ISBN属性
            xe1.SetAttribute("phonenum", PHONE_NUMBER);
            xe1.SetAttribute("TIME", TIME);
            root.AppendChild(xe1);//添加到<Employees>节点中 
            xmldoc.Save("data.xml");
        }

        private String[] parse_xml(String filename) {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            XmlNode node = doc.SelectSingleNode("/GarageInfo/User");
            String[] s = new String[4];
            if (node != null)
            {
                s[0] = node.Attributes["name"].Value;
                s[1] = node.Attributes["carnum"].Value;
                s[2] = node.Attributes["phonenum"].Value;
                s[3] = node.Attributes["TIME"].Value;
                node = null;
            }
            return s;
        }
        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
