using System;
using Nethereum.Web3;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Hex.HexConvertors.Extensions;
namespace DiscordBot.Axie.Web3Axie
{
    public class AxieSaleGetter
    {
        private string AxieCoreContractAddress = "0xF5b0A3eFB8e8E4c201e2A935F110eAaF3FFEcb8d";
        private string ABI = "";
        public AxieSaleGetter()
        {
        }

        public void GetData()
        {

            var web3 = new Web3();
            var contract = web3.Eth.GetContract(ABI, AxieCoreContractAddress);
            var eventAuctionSuccesful = contract.GetEvent("AuctionSuccessful");
        }
    }
    [FunctionOutput]
    public class AuctionSuccessfulEvent
    {
        [Parameter("address", "_nftAddress", 1, true)]
        public int nftAddress { get; set; }

        [Parameter("uint256", "_tokenId", 2, true)]
        public string tokenId { get; set; }

        [Parameter("uint256", "_totalPrice", 3, false)]
        public int totalPrice { get; set; }

        [Parameter("address", "_winner", 1, true)]
        public int winner { get; set; }
    }
}
