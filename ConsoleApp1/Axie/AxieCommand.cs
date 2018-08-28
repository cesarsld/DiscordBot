﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;
namespace DiscordBot
{
    [Group("axie")]
    public class AxieCommand : ModuleBase
    {
        [Command("help"), Summary("register ETH wallet address to user ID")]
        public async Task PostHelp()
        {
            await Context.Channel.SendMessageAsync("Commands that you can use :\n"
                                                 + "- `>axie addaddress _input_address` : register an ETH wallet address to user ID. You can add several addresses if you own more than 1 . \n"
                                                 + "- `>axie removeaddress _input_address` : remove an ETH wallet address from user.\n"
                                                 + "- `>axie removeuser` : remove all data from user.\n"
                                                 + "- `>axie buy _input_id` : Ping the owner of this axie.\n"
                                                 + "- `>axie show` : Show user's addresses");
        }
        [Command("addaddress"), Summary("register ETH wallet address to user ID")]
        public async Task AddAddress(string address)
        {
            await AxieHolderListHandler.AddUserAddress(Context.Message.Author.Id, address, Context.Channel);
        }

        [Command("removeaddress"), Summary("remove ETH wallet address to user ID")]
        public async Task RemoveAddress(string address)
        {
            await AxieHolderListHandler.RemoveAddress(Context.Message.Author.Id, address, Context.Channel);
        }
        [Command("removeuser"), Summary("remove user from DB")]
        public async Task RemoveUser()
        {
            await AxieHolderListHandler.RemoveUser(Context.Message.Author.Id, Context.Channel);
        }

        [Command("buy"), Summary("notify buyer than you want to buy axie")]
        public async Task BuyAxie(int axieNumber)
        {
            string json = "";
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                try
                {
                    json = await wc.DownloadStringTaskAsync("https://axieinfinity.com/api/axies/" + axieNumber.ToString());
                }
                catch (Exception ex)
                {
                    await Context.Channel.SendMessageAsync("Error. Axie could not be found.");
                    return;
                }
            }
            JObject axieJson = JObject.Parse(json);
            AxieData axieData = axieJson.ToObject<AxieData>();
            await AxieHolderListHandler.GetHolderId(axieData.owner, Context);
            //if (axieData.stage <= 2) await Context.Channel.SendMessageAsync("Axie is still an egg! I can't check what it's going to be >:O ");
        }

        [Command("show"), Summary("show you addresses")]
        public async Task ShowAddresses()
        {
            await AxieHolderListHandler.GetUserAddressList(Context.Message.Author.Id, Context.Channel);
        }
    }
}
