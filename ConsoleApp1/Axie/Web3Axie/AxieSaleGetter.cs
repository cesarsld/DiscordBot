using System;
using System.Collections.Generic;
using Nethereum.Web3;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RPC.Eth.DTOs;
using System.Threading.Tasks;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Discord;
using Discord.WebSocket;

using Newtonsoft.Json.Linq;
namespace DiscordBot.Axie.Web3Axie
{
    public class AxieSaleGetter
    {
        #region ABI & contract declaration
        private static string AxieCoreContractAddress = "0xF4985070Ce32b6B1994329DF787D1aCc9a2dd9e2";
        private static string ABI = @"[
  {
    'constant': false,
    'inputs': [
      {
        'name': 'token',
        'type': 'address'
      }
    ],
    'name': 'reclaimToken',
    'outputs': [],
    'payable': false,
    'stateMutability': 'nonpayable',
    'type': 'function'
  },
  {
    'constant': false,
    'inputs': [
      {
        'name': 'contractAddr',
        'type': 'address'
      }
    ],
    'name': 'reclaimContract',
    'outputs': [],
    'payable': false,
    'stateMutability': 'nonpayable',
    'type': 'function'
  },
  {
    'constant': false,
    'inputs': [],
    'name': 'unpause',
    'outputs': [],
    'payable': false,
    'stateMutability': 'nonpayable',
    'type': 'function'
  },
  {
    'constant': true,
    'inputs': [
      {
        'name': '',
        'type': 'address'
      },
      {
        'name': '',
        'type': 'uint256'
      }
    ],
    'name': 'auctions',
    'outputs': [
      {
        'name': 'seller',
        'type': 'address'
      },
      {
        'name': 'startingPrice',
        'type': 'uint128'
      },
      {
        'name': 'endingPrice',
        'type': 'uint128'
      },
      {
        'name': 'duration',
        'type': 'uint64'
      },
      {
        'name': 'startedAt',
        'type': 'uint64'
      }
    ],
    'payable': false,
    'stateMutability': 'view',
    'type': 'function'
  },
  {
    'constant': true,
    'inputs': [],
    'name': 'paused',
    'outputs': [
      {
        'name': '',
        'type': 'bool'
      }
    ],
    'payable': false,
    'stateMutability': 'view',
    'type': 'function'
  },
  {
    'constant': true,
    'inputs': [],
    'name': 'ownerCut',
    'outputs': [
      {
        'name': '',
        'type': 'uint256'
      }
    ],
    'payable': false,
    'stateMutability': 'view',
    'type': 'function'
  },
  {
    'constant': false,
    'inputs': [],
    'name': 'pause',
    'outputs': [],
    'payable': false,
    'stateMutability': 'nonpayable',
    'type': 'function'
  },
  {
    'constant': true,
    'inputs': [],
    'name': 'owner',
    'outputs': [
      {
        'name': '',
        'type': 'address'
      }
    ],
    'payable': false,
    'stateMutability': 'view',
    'type': 'function'
  },
  {
    'constant': false,
    'inputs': [],
    'name': 'reclaimEther',
    'outputs': [],
    'payable': false,
    'stateMutability': 'nonpayable',
    'type': 'function'
  },
  {
    'constant': false,
    'inputs': [
      {
        'name': 'from_',
        'type': 'address'
      },
      {
        'name': 'value_',
        'type': 'uint256'
      },
      {
        'name': 'data_',
        'type': 'bytes'
      }
    ],
    'name': 'tokenFallback',
    'outputs': [],
    'payable': false,
    'stateMutability': 'nonpayable',
    'type': 'function'
  },
  {
    'constant': false,
    'inputs': [
      {
        'name': 'newOwner',
        'type': 'address'
      }
    ],
    'name': 'transferOwnership',
    'outputs': [],
    'payable': false,
    'stateMutability': 'nonpayable',
    'type': 'function'
  },
  {
    'inputs': [
      {
        'name': '_ownerCut',
        'type': 'uint256'
      }
    ],
    'payable': false,
    'stateMutability': 'nonpayable',
    'type': 'constructor'
  },
  {
    'payable': false,
    'stateMutability': 'nonpayable',
    'type': 'fallback'
  },
  {
    'anonymous': false,
    'inputs': [
      {
        'indexed': true,
        'name': '_nftAddress',
        'type': 'address'
      },
      {
        'indexed': true,
        'name': '_tokenId',
        'type': 'uint256'
      },
      {
        'indexed': false,
        'name': '_startingPrice',
        'type': 'uint256'
      },
      {
        'indexed': false,
        'name': '_endingPrice',
        'type': 'uint256'
      },
      {
        'indexed': false,
        'name': '_duration',
        'type': 'uint256'
      },
      {
        'indexed': false,
        'name': '_seller',
        'type': 'address'
      }
    ],
    'name': 'AuctionCreated',
    'type': 'event'
  },
  {
    'anonymous': false,
    'inputs': [
      {
        'indexed': true,
        'name': '_nftAddress',
        'type': 'address'
      },
      {
        'indexed': true,
        'name': '_tokenId',
        'type': 'uint256'
      },
      {
        'indexed': false,
        'name': '_totalPrice',
        'type': 'uint256'
      },
      {
        'indexed': false,
        'name': '_winner',
        'type': 'address'
      }
    ],
    'name': 'AuctionSuccessful',
    'type': 'event'
  },
  {
    'anonymous': false,
    'inputs': [
      {
        'indexed': true,
        'name': '_nftAddress',
        'type': 'address'
      },
      {
        'indexed': true,
        'name': '_tokenId',
        'type': 'uint256'
      }
    ],
    'name': 'AuctionCancelled',
    'type': 'event'
  },
  {
    'anonymous': false,
    'inputs': [],
    'name': 'Pause',
    'type': 'event'
  },
  {
    'anonymous': false,
    'inputs': [],
    'name': 'Unpause',
    'type': 'event'
  },
  {
    'anonymous': false,
    'inputs': [
      {
        'indexed': true,
        'name': 'previousOwner',
        'type': 'address'
      },
      {
        'indexed': true,
        'name': 'newOwner',
        'type': 'address'
      }
    ],
    'name': 'OwnershipTransferred',
    'type': 'event'
  },
  {
    'constant': true,
    'inputs': [
      {
        'name': '_nftAddress',
        'type': 'address'
      },
      {
        'name': '_tokenId',
        'type': 'uint256'
      }
    ],
    'name': 'getAuction',
    'outputs': [
      {
        'name': 'seller',
        'type': 'address'
      },
      {
        'name': 'startingPrice',
        'type': 'uint256'
      },
      {
        'name': 'endingPrice',
        'type': 'uint256'
      },
      {
        'name': 'duration',
        'type': 'uint256'
      },
      {
        'name': 'startedAt',
        'type': 'uint256'
      }
    ],
    'payable': false,
    'stateMutability': 'view',
    'type': 'function'
  },
  {
    'constant': true,
    'inputs': [
      {
        'name': '_nftAddress',
        'type': 'address'
      },
      {
        'name': '_tokenId',
        'type': 'uint256'
      }
    ],
    'name': 'getCurrentPrice',
    'outputs': [
      {
        'name': '',
        'type': 'uint256'
      }
    ],
    'payable': false,
    'stateMutability': 'view',
    'type': 'function'
  },
  {
    'constant': false,
    'inputs': [
      {
        'name': '_nftAddress',
        'type': 'address'
      },
      {
        'name': '_tokenId',
        'type': 'uint256'
      },
      {
        'name': '_startingPrice',
        'type': 'uint256'
      },
      {
        'name': '_endingPrice',
        'type': 'uint256'
      },
      {
        'name': '_duration',
        'type': 'uint256'
      }
    ],
    'name': 'createAuction',
    'outputs': [],
    'payable': false,
    'stateMutability': 'nonpayable',
    'type': 'function'
  },
  {
    'constant': false,
    'inputs': [
      {
        'name': '_nftAddress',
        'type': 'address'
      },
      {
        'name': '_tokenId',
        'type': 'uint256'
      }
    ],
    'name': 'bid',
    'outputs': [],
    'payable': true,
    'stateMutability': 'payable',
    'type': 'function'
  },
  {
    'constant': false,
    'inputs': [
      {
        'name': '_nftAddress',
        'type': 'address'
      },
      {
        'name': '_tokenId',
        'type': 'uint256'
      }
    ],
    'name': 'cancelAuction',
    'outputs': [],
    'payable': false,
    'stateMutability': 'nonpayable',
    'type': 'function'
  },
  {
    'constant': false,
    'inputs': [
      {
        'name': '_nftAddress',
        'type': 'address'
      },
      {
        'name': '_tokenId',
        'type': 'uint256'
      }
    ],
    'name': 'cancelAuctionWhenPaused',
    'outputs': [],
    'payable': false,
    'stateMutability': 'nonpayable',
    'type': 'function'
  }
]";
        #endregion
        private static ulong marketPlaceChannelId = 423343101428498435;
        private static BigInteger lastBlockChecked = 6357537;
        public static bool IsServiceOn= true;
        public AxieSaleGetter()
        {
        }

        public static async Task GetData()
        {
            IsServiceOn = true;
            var web3 = new Web3("https://mainnet.infura.io");
            var contract = web3.Eth.GetContract(ABI, AxieCoreContractAddress);
            var auctionSuccesfulEvent = contract.GetEvent("AuctionSuccessful");
            var penultimateBlockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            var firstBlock = new BlockParameter(penultimateBlockNumber);
            var lastBlock = BlockParameter.CreateLatest();
            while (true)
            {
                try
                {
                    var filterAll = auctionSuccesfulEvent.CreateFilterInput(firstBlock, lastBlock);
                    var logs = await auctionSuccesfulEvent.GetAllChanges<AuctionSuccessfulEvent>(filterAll);
                    if (logs != null && logs.Count > 0)
                    {
                        foreach (var log in logs)
                        {
                            int axieId = Convert.ToInt32(log.Event.tokenId.ToString());
                            float priceinEth = Convert.ToSingle(Nethereum.Util.UnitConversion.Convert.FromWei(log.Event.totalPrice).ToString());
                            await PostAxieToMarketplace(axieId, priceinEth);
                            await Task.Delay(5000);
                        };
                        Console.WriteLine("End of batch");
                    }
                    var lastBlockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
                    lastBlockChecked = lastBlockNumber.Value;
                    firstBlock = new BlockParameter(new HexBigInteger(lastBlockChecked));
                    lastBlock = BlockParameter.CreateLatest();
                    await Task.Delay(60000);

                }
                catch (Nethereum.JsonRpc.Client.RpcClientUnknownException ex)
                {
                    Console.WriteLine(ex.Message);
                    IsServiceOn = false;
                    await PostToMarketplace("Something went wrong... Please this service using this command `>axie rebootSales`.");
                    break;
                }
            }
        }

        private static async Task PostToMarketplace(string text)
        {
            SocketChannel channel = Bot.GetChannelContext(marketPlaceChannelId);
            IMessageChannel msgChannel = channel as IMessageChannel;
            await msgChannel.SendMessageAsync(text);
        }

            private static async Task PostAxieToMarketplace(int index, float price)
        {
            SocketChannel channel = Bot.GetChannelContext(479664564061995019);
            IMessageChannel msgChannel = channel as IMessageChannel;
            string json = "";
            int safetyNet = 0;
            while (safetyNet < 10)
            {
                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                    try
                    {
                        json = await wc.DownloadStringTaskAsync("https://api.axieinfinity.com/v1/axies/" + index.ToString()); //https://axieinfinity.com/api/axies/
                        break;
                    }

                    catch (Exception ex)
                    {
                        safetyNet++;
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
            if (safetyNet == 10) await msgChannel.SendMessageAsync("Error. Axie data could not be retrieved.");
            JObject axieJson = JObject.Parse(json);
            AxieData axieData = axieJson.ToObject<AxieData>();
            axieData.jsonData = axieJson;
            if(price > 1 || axieData.hasMystic) await msgChannel.SendMessageAsync("", false, axieData.EmbedAxieSaleData( price));

        }

    }

    public class AuctionSuccessfulEvent
    {
        [Parameter("address", "_nftAddress", 1, true)]
        public string nftAddress { get; set; }

        [Parameter("uint256", "_tokenId", 2, true)]
        public BigInteger tokenId { get; set; }

        [Parameter("uint256", "_totalPrice", 3)]
        public BigInteger totalPrice { get; set; }

        [Parameter("address", "_winner", 4)]
        public string winner { get; set; }
    }
}
