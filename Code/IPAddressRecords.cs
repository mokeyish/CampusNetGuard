using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CampusNetGuard.Code
{
    class IPAddressRecords
    {
        private List<string> _list = new List<string>();
        private readonly string filename="IPAddressRecords.log";
        private readonly string[] titles = new string[] {
            "最近IP记录表",
            "Ip地址:\t",
            "掩码:\t",
            "网关:\t",
            "Dns:\t" };
        public IPAddressRecords()
        {
            filename = RunTime.Directory.AppData + filename;
        }
        public string this[int index]
        {
            get
            {
                return _list[0].Substring(20);
            }
        }
        private void setTitles(int i,string y)
        {
            string[] t = titles[i].Split('\t');
            isChanged |=  t[1] != y;
            if (isChanged) titles[i] = string.Format("{0}\t{1}",t[0],y);

        }
        public void Add(NetCardInterface Interface)
        {
            setTitles(1, Interface.IPAddress.ToString());
            setTitles(2, Interface.Netmask.ToString());
            setTitles(3, Interface.GatewayAddress.ToString());
            setTitles(4, string.Format("{0} , {1}", Interface.Dns[0], Interface.Dns[1]));
            if (isChanged)
            {
                _list.Insert(0, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss\t") + Interface.IPAddress);
            }
            Save();
        }
        public void Read()
        {
            try
            {
                if (File.Exists(filename))
                {
                    using (StringReader sr = new StringReader(File.ReadAllText(filename, RunTime.Encode)))
                    {
                        for (int i = 0; i < titles.Length; i++)
                        {
                            titles[i] = sr.ReadLine();
                        }
                        for (int i = 0; i < 10; i++)
                        {
                            _list.Add(sr.ReadLine());

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                RunTime.Logger.Error("读取历史ip出错" + ex.ToString());
            }
        }
        private bool isChanged = false;
        public void Save()
        {
            if (isChanged)
            {
                try
                {

                    using (StringWriter sw = new StringWriter())
                    {
                        foreach (var item in titles)
                        {
                            sw.WriteLine(item);
                        }
                        foreach (var item in _list)
                        {
                            sw.WriteLine(item);
                        }
                        File.WriteAllText(filename, sw.ToString(), RunTime.Encode);
                    }
                }
                catch (Exception ex)
                {

                    RunTime.Logger.Error("保存历史ip出错" + ex.ToString());
                }
            }
        }
    }
}
