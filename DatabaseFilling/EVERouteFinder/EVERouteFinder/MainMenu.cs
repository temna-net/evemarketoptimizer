﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using EVERouteFinder.Classes;
using System.IO;
using System.Globalization;

namespace EVERouteFinder
{
    public partial class MainMenu : Form
    {
        public MainMenu()
        {
            InitializeComponent();
        }


        private List<Node> getSolarSystems()
        {
            EVEDBoperations nodeOperations = new EVEDBoperations();
            nodeOperations.startEVEDBConnection();
            nodeOperations.openEVEDBConnection();
            nodeOperations.setEVEDBQuery(nodeOperations.premadeQuery_getSolarSystemsList());
            List<Node> solarSystems = new List<Node>();
            while (nodeOperations.eveDBQueryRead())
            {
                solarSystems.Add(new Node((int)nodeOperations.eveDBReader[0]));
            }
            nodeOperations.eveDBQueryClose();
            nodeOperations.closeEVEDBConnection();
            return solarSystems;
        }

        private void loopNodes()
        {
            List<Node> myList = getSolarSystems();
            ParallelOptions po = new ParallelOptions();
            po.MaxDegreeOfParallelism = Environment.ProcessorCount;
            //workload is around 55% program 35% database. probably could be highly optimized by saving all queried nodes to a single structure and working from there
            Parallel.ForEach(myList, po, n =>
            {
                List<Node> mylist2 = getSolarSystems();
                foreach (Node n1 in mylist2)
                {
                    loop(n, n1);
                }
            }
            );

            this.textBoxResult.Text = Settings.SEVEDBSettings.factor.ToString();
        }

        private void loop(Node n, Node n1)
        {
            if (n.ID != n1.ID)
            {
                int tid = Thread.CurrentThread.ManagedThreadId;
                PathOperations pop = new PathOperations(n, n1);
                pop.nofactor = false;
                pop.start.nofactor = false;
                pop.goal.nofactor = false;
                List<Node> route = new List<Node>();

                route = pop.Evaluate();
                string systems = "";
                int i = 0;
                foreach (Node n2 in route)
                {
                    systems += "System: " + " " + n2.Name + " " + n2.Security.ToString() + " " + n2.Region.ToString() + " " + "\r\n"; //n2.f_score.ToString() +
                    i++;
                }
                pop = new PathOperations(n, n1);
                pop.nofactor = true;
                pop.start.nofactor = true;
                pop.goal.nofactor = true;
                route = new List<Node>();

                route = pop.Evaluate();
                string systems1 = "";
                int a = 0;
                foreach (Node n2 in route)
                {
                    systems1 += "System: " + " " + n2.Name + " " + n2.Security.ToString() + " " + n2.Region.ToString() + " " + "\r\n"; //n2.f_score.ToString() +
                    a++;
                }
                if (systems != systems1)
                {
                    SetText(n.Name + " " + n1.Name + " " + "Not qualified " + a.ToString() + ", " + (i - a).ToString() + "\r\n", 2);
                    if (i - a > 0)
                    {
                        Settings.SEVEDBSettings.factor -= 0.01;
                        SetText(Settings.SEVEDBSettings.factor.ToString() + " /// " + DateTime.Now.ToLongTimeString() + "\r\n", 3);
                    }
                }
                else
                {
                    SetText(n.Name + " " + n1.Name + " " + "Qualified " + a.ToString() + "\r\n", 1);
                }
            }
        }

        private void SetText(string text, int thread)
        {
            if (this.textBox2.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text, thread });
            }
            else
            {
                switch (thread % 3)
                {
                    case 0:
                        this.textBoxResult.AppendText(text);
                        this.textBoxResult.Focus();
                        break;
                    case 1:
                        this.textBox2.AppendText(text);
                        this.textBox2.Focus();
                        break;
                    case 2:
                        this.textBox1.AppendText(text);
                        this.textBox1.Focus();
                        break;
                }
            }
        }

        delegate void SetTextCallback(string text, int thread);

        delegate void StartDoingWork();

        private void searchFactor()
        {
            StartDoingWork s = new StartDoingWork(loopNodes);
            Thread myNewThread = new Thread(s.Invoke);
            myNewThread.Start();
            this.button1.Enabled = false;
        }

        private void inputMarketDatabaseDump()
        {
            StartDoingWork s = new StartDoingWork(loopMarketDatabase);
            Thread myNewThread = new Thread(s.Invoke);
            myNewThread.Start();
            this.button1.Enabled = false;
        }

        private void loopMarketDatabase()
        {
            DirectoryInfo di = new DirectoryInfo(@"C:\Users\Greitone\EVEMarketDumps");
            foreach(FileInfo fi in di.GetFiles())
            {
                IEnumerable<string> stringList = File.ReadLines(fi.FullName);
                ParallelOptions po = new ParallelOptions();
                po.MaxDegreeOfParallelism = Environment.ProcessorCount;
                string mys = stringList.ElementAt(0);
                bool isEveLog = false;
                if (mys.StartsWith("price,volRemaining,typeID,range,orderID,volEntered,minVolume,bid,issueDate,duration,stationID,regionID,solarSystemID,jumps,"))
                {
                    isEveLog = true;
                }
                Parallel.ForEach(stringList, po, n =>
                {
                    string[] s = formatOrderString(n, isEveLog);
                    EVEOrder o = new EVEOrder(s);
                    o.InsertToDB();
                }
            );
            }
        }

        private string[] formatOrderString(string n, bool isEveLog)
        {
            string[] s = n.Split(new string[] { "\",\"", "\t", "," }, StringSplitOptions.RemoveEmptyEntries);
            if (s[0].Contains('\"'))
            {
                for (int i = 0; i < s.Count(); i++)
                {
                    s[i] = s[i].Replace("\",\"", string.Empty);
                    s[i] = s[i].Replace('\"', ' ');
                }
            }
            return s;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            inputMarketDatabaseDump();
        }

        private void MainMenu_Load(object sender, EventArgs e)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        }


    }
}