using System;
using System.Collections.Generic;
using Nethereum.Web3;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RPC.Eth.DTOs;
using System.Threading.Tasks;
using System.Numerics;
using Nethereum.Contracts.CQS;
using Nethereum.Util;
using Nethereum.Web3.Accounts;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
namespace DiscordBot.Axie.Web3Axie
{
    public class AxieSaleGetter
    {
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
        public AxieSaleGetter()
        {
        }

        public static async Task GetData()
        {

            var web3 = new Web3("https://mainnet.infura.io");
            var contract = web3.Eth.GetContract(ABI, AxieCoreContractAddress);
            var auctionSuccesfulEvent = contract.GetEvent("AuctionSuccessful");
            var firstBlock = new BlockParameter(new HexBigInteger(6357537));
            //var lastBlock = new BlockParameter(new HexBigInteger(6357958));
            var lastBlock = BlockParameter.CreateLatest();

            try
            {
                //var filterAll = await auctionSuccesfulEvent.CreateFilterBlockRangeAsync(firstBlock, lastBlock); //error here
                var filterAll = auctionSuccesfulEvent.CreateFilterInput(firstBlock, lastBlock); //error here

                var logs = await auctionSuccesfulEvent.GetAllChanges<AuctionSuccessfulEvent>(filterAll);

            }
            catch (Nethereum.JsonRpc.Client.RpcClientUnknownException ex)
            {
                Console.WriteLine(ex.Message);
            }

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
