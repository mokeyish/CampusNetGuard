using System;
using System.IO;
using CampusNetGuard.Libraries;

namespace CampusNetGuard.Code
{
    public class ConfigManager
    {
        private readonly string path;
        private Configurations config;
        public ConfigManager(string path)
        {
            this.path = path;
        }
        public Configurations Config
        {
            get
            {
                if (config == null)
                {
                    Read();
                }
                return config;
            }
        }
        public void Read()
        {

            lock (this)
            {
                if (File.Exists(path))
                {
                    try
                    {
                        try
                        {
                            config = fastJSON.JSON.ToObject<Configurations>(File.ReadAllText(path, RunTime.Encode));
                        }
                        catch (Exception ex)
                        {
                            config = new Configurations();
                            RunTime.Logger.Error("json配置转换出错，" + ex.ToString());
                        }
                        Base64Crypt bc = new Base64Crypt();
                        config.Password = bc.Decode(config.Password);
                        config.WebPassword = bc.Decode(config.WebPassword);
                        if (config.ver != 1)
                        {
                            Configurations m = new Configurations();
                            config.ExampleHosts = m.ExampleHosts;
                            config.ver = 1;
                            Save();
                        }
                    }
                    catch (Exception ex)
                    {
                        RunTime.Logger.Error("从本地读取Config出错" + ex.Message);
                        config = new Configurations();
                    }
                }
                else
                {
                    config = new Configurations();
                }
            }
        }
        public void Save()
        {
            lock (this)
            {

                string Password = Config.Password;
                string WebPassword = Config.WebPassword;
                try
                {
                    Base64Crypt bc = new Base64Crypt();
                    Config.Password = bc.Encode(Config.Password);
                    Config.WebPassword = bc.Encode(Config.WebPassword);
                    File.WriteAllText(path, Config.ToString(), RunTime.Encode);
                }
                catch (Exception ex)
                {
                    RunTime.Logger.Error("保存Config到本地出错" + ex.Message);

                }
                finally
                {
                    Config.Password = Password;
                    Config.WebPassword = WebPassword;

                }
            }
        }
        
        public override string ToString()
        {
            return config.ToString();
        }
    }

}
