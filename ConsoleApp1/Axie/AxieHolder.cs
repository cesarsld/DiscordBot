using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot
{
    class AxieHolder
    {
        public ulong userId;
        public List<string> addressList;

        public AxieHolder(ulong _userId, List<string> _addressList)
        {
            userId = _userId;
            addressList = _addressList;
        }
        public ulong GetUserId() => userId;
        public List<string> GetAddressList() => addressList;
        public void AddAddress(string address) => addressList.Add(address);
        public void RemoveAddress(string address) => addressList.Remove(address);
    }
}
