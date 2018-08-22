using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Data;
using System.IO;

namespace DiscordBot
{
    class BanListHandler
    {
        private String strFilename = "Logger/BanList.XML";
        DataTable dt;

        public BanListHandler()
        {

        }

        public List<ulong> GetBanList()
        {
            DataSet ds = new DataSet();
            List<ulong> list = new List<ulong>();

            if (File.Exists(strFilename))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(DataSet));
                FileStream readStream = new FileStream(strFilename, FileMode.Open);
                ds = (DataSet)xmlSerializer.Deserialize(readStream);
                readStream.Close();

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    list.Add((ulong)ds.Tables[0].Rows[i]["UserID"]);
                }
            }

            return list;
        }

        private void SetBanList(List<ulong> banList)
        {
            DataSet ds = new DataSet();
            dt = new DataTable();
            dt.Columns.Add(new DataColumn("UserID", typeof(ulong)));
            foreach (ulong userID in banList)
            {
                fillRow(userID);
            }

            ds.Tables.Add(dt);
            ds.Tables[0].TableName = "BanList";

            StreamWriter serialWriter;
            serialWriter = new StreamWriter(strFilename);
            XmlSerializer xmlWriter = new XmlSerializer(ds.GetType());
            xmlWriter.Serialize(serialWriter, ds);
            serialWriter.Close();
            ds.Clear();
        }

        private void fillRow(ulong userID)
        {
            DataRow dr;
            dr = dt.NewRow();
            dr["UserID"] = userID;
            dt.Rows.Add(dr);
        }
        public void AddUserToBannedList(ulong userID)
        {
            List<ulong> list = GetBanList();
            //List<ulong> list = new List<ulong>();

            if (list.Contains(userID) == false)
            {
                list.Add(userID);
            }

            SetBanList(list);
        }
        public void UnbanUserFromBannedList(ulong userID)
        {
            List<ulong> list = GetBanList();

            if (list.Contains(userID) == true)
            {
                list.Remove(userID);
            }

            SetBanList(list);
        }

        public void HandleGameInit(List<ulong> playerList)
        {

        }
    }
}
