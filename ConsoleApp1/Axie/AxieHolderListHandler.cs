using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Data;
using System.IO;
using Discord;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DiscordBot
{
    public class AxieHolder
    {
        private ulong userId;
        private List<string> addressList;

        public AxieHolder(ulong _userId, List<string> _addressList)
        {
            userId = _userId;
            addressList = _addressList;
        }
        public ulong GetUserId() => userId;
        public List<string> GetAddressList() => addressList;
        public void AddAddress(string address) => addressList.Add(address);
    }
    class AxieHolderListHandler
    {
        private String strFilename = "Logger/AxieHolderList.XML";
        DataTable dt;

        public AxieHolderListHandler()
        {

        }

        public List<AxieHolder> GetAxieHolders()
        {
            List<AxieHolder> list = new List<AxieHolder>();
            if(File.Exists(strFilename))
            {
                string fileData = "";
                using (StreamReader sr = new StreamReader(strFilename))
                {
                    fileData = sr.ReadToEnd();
                }
                string[] jsonFiles = fileData.Split(',');
                foreach(var json in jsonFiles)
                {
                    list.Add(JObject.Parse(json).ToObject<AxieHolder>());
                }
            }
            return list;
        }

        public Dictionary<ulong, string> GetAxieHolderList()
        {
            DataSet ds = new DataSet();
            Dictionary<ulong, string> holderDictionary = new Dictionary<ulong, string>();

            if (File.Exists(strFilename))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(DataSet));
                FileStream readStream = new FileStream(strFilename, FileMode.Open);
                ds = (DataSet)xmlSerializer.Deserialize(readStream);
                readStream.Close();

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    //list.Add((ulong)ds.Tables[0].Rows[i]["UserID"]);
                    holderDictionary.Add((ulong)ds.Tables[0].Rows[i][0], (string)ds.Tables[0].Rows[i][1]);
                }
            }

            return holderDictionary;
        }

        private void AddUserAddress(ulong userId, string address, IMessageChannel channel)
        {
            List<AxieHolder> axieHolders = GetAxieHolders();
            foreach(var holder in axieHolders)
            {
                if (holder.GetAddressList().Contains(address))
                {
                    channel.SendMessageAsync("Error. Address already used.");
                    return;
                }
            }
            AxieHolder axieHolder = axieHolders.First(holder => holder.GetUserId() == userId);
            if (axieHolder == null)
            {
                List<string> addressList = new List<string>() { address };
                axieHolders.Add(new AxieHolder(userId,addressList));
            }
            else 
            {
                axieHolder.AddAddress(address);
            }
            SetAddressList(axieHolders);
        }

        private void RemoveUser(ulong userId, IMessageChannel channel)
        {
            List<AxieHolder> axieHolders = GetAxieHolders();

            AxieHolder axieHolder = axieHolders.First(holder => holder.GetUserId() == userId);
            if (axieHolder == null)
            {
                channel.SendMessageAsync("User does not exist, fool!");
            }
            else
            {
                axieHolders.Remove(axieHolder);
                SetAddressList(axieHolders);
            }
        }


        private void RemoveAddress(ulong userId, string address, IMessageChannel channel)
        {
            List<AxieHolder> axieHolders = GetAxieHolders();
            AxieHolder axieHolder = axieHolders.First(holder => holder.GetUserId() == userId);
            if (axieHolder == null)
            {
                channel.SendMessageAsync("This address doesn't belong to you :(");
            }
            else
            {
                try 
                {
                    axieHolder.AddAddress(address);
                }
                catch (Exception e)
                {
                    channel.SendMessageAsync("Error, address does not exist in your address list.");
                }
            }
            SetAddressList(axieHolders);
        }

        private void SetAddressList(List<AxieHolder> axieHolderList)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach(var holder in axieHolderList)
            {
                stringBuilder.Append(JsonConvert.SerializeObject(holder) + ",");
                stringBuilder.AppendLine();
            }
            using(StreamWriter sw = new StreamWriter(strFilename))
            {
                sw.Write(stringBuilder.ToString());
            }
        }


        private void SetHolderList(Dictionary<ulong, string> banList)
        {
            DataSet ds = new DataSet();
            dt = new DataTable();
            dt.Columns.Add(new DataColumn("UserID", typeof(ulong)));
            dt.Columns.Add(new DataColumn("UserAddress", typeof(string)));
            foreach (KeyValuePair<ulong, string>user in banList)
            {
                dt.Rows.Add(user.Key, user.Value);
            }

            ds.Tables.Add(dt);
            ds.Tables[0].TableName = "AxieHolderList";

            StreamWriter serialWriter;
            serialWriter = new StreamWriter(strFilename);
            XmlSerializer xmlWriter = new XmlSerializer(ds.GetType());
            xmlWriter.Serialize(serialWriter, ds);
            serialWriter.Close();
            ds.Clear();
        }

        private void fillRow(ulong userID, string address)
        {
            DataRow dr;
            dr = dt.NewRow();
            //dr.
            dt.Rows.Add(dr);
        }
        public void AddUserToHolderList(ulong userID, string address)
        {
            Dictionary<ulong, string> list = GetAxieHolderList();
            //List<ulong> list = new List<ulong>();

            if (!list.ContainsValue(address))
            {
                list.Add(userID, address);
            }

            SetHolderList(list);
        }
        public void RemoveUserFromHolderList(ulong userID)
        {
            Dictionary<ulong, string> list = GetAxieHolderList();

            if (list.Contains(userID) == true)
            {
                list.Remove(userID);
            }

            SetHolderList(list);
        }
    }
}
