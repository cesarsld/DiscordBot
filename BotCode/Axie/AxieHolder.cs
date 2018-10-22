using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DiscordBot
{
    class AxieHolderOld
    {
        public ulong userId;
        public List<string> addressList;

        public AxieHolderOld(ulong _userId, List<string> _addressList)
        {
            userId = _userId;
            addressList = _addressList;
        }
        public ulong GetUserId() => userId;
        public List<string> GetAddressList() => addressList;
        public void AddAddress(string address) => addressList.Add(address);
        public void RemoveAddress(string address) => addressList.Remove(address);
    }
    class AxieHolder
    {
        public ulong userId;
        public List<string> addressList;
        public List<int> nonBuyableAxieList;
        public bool isPingable;
        [JsonConstructor]
        public AxieHolder(ulong _userId, List<string> _addressList, bool pingable, List<int> axieList)
        {
            userId = _userId;
            addressList = _addressList;
            isPingable = pingable;
            nonBuyableAxieList = axieList;
        }
        public AxieHolder(AxieHolderOld holder)
        {
            userId = holder.userId;
            addressList = holder.addressList;
            isPingable = true;
            nonBuyableAxieList = new List<int>();
        }

        public ulong GetUserId() => userId;
        public List<string> GetAddressList() => addressList;
        public void AddAddress(string address) => addressList.Add(address);
        public void RemoveAddress(string address) => addressList.Remove(address);
    }


}
