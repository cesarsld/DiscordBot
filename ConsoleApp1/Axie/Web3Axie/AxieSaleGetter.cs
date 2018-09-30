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
        private static string AxieLabContractAddress = "0x99ff9f4257D5b6aF1400C994174EbB56336BB79F";
        private static string auctionABI = @"[
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
        private static string labABI = @"[
  {
    'constant': true,
    'inputs': [],
    'name': 'eggCoinContract',
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
    'constant': true,
    'inputs': [],
    'name': 'lastSaleDate',
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
    'constant': true,
    'inputs': [],
    'name': 'genePoolContract',
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
        'name': 'account',
        'type': 'address'
      }
    ],
    'name': 'isPauser',
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
    'inputs': [
      {
        'name': '',
        'type': 'address'
      }
    ],
    'name': 'numPendingAxie',
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
    'constant': true,
    'inputs': [
      {
        'name': '',
        'type': 'uint256'
      }
    ],
    'name': 'pendingGeneHashOwner',
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
    'constant': false,
    'inputs': [],
    'name': 'renouncePauser',
    'outputs': [],
    'payable': false,
    'stateMutability': 'nonpayable',
    'type': 'function'
  },
  {
    'constant': true,
    'inputs': [],
    'name': 'coreExtraContract',
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
    'constant': true,
    'inputs': [],
    'name': 'defaultReferralPercentage',
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
    'name': 'renounceOwnership',
    'outputs': [],
    'payable': false,
    'stateMutability': 'nonpayable',
    'type': 'function'
  },
  {
    'constant': false,
    'inputs': [],
    'name': 'withdrawEther',
    'outputs': [],
    'payable': false,
    'stateMutability': 'nonpayable',
    'type': 'function'
  },
  {
    'constant': true,
    'inputs': [],
    'name': 'nextPricePercentage',
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
        'name': 'account',
        'type': 'address'
      }
    ],
    'name': 'addPauser',
    'outputs': [],
    'payable': false,
    'stateMutability': 'nonpayable',
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
    'constant': true,
    'inputs': [],
    'name': 'isOwner',
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
    'constant': false,
    'inputs': [
      {
        'name': '_percentage',
        'type': 'uint256'
      }
    ],
    'name': 'setDefaultReferralPercentage',
    'outputs': [],
    'payable': false,
    'stateMutability': 'nonpayable',
    'type': 'function'
  },
  {
    'constant': false,
    'inputs': [
      {
        'name': 'account',
        'type': 'address'
      }
    ],
    'name': 'addMinter',
    'outputs': [],
    'payable': false,
    'stateMutability': 'nonpayable',
    'type': 'function'
  },
  {
    'constant': false,
    'inputs': [],
    'name': 'renounceMinter',
    'outputs': [],
    'payable': false,
    'stateMutability': 'nonpayable',
    'type': 'function'
  },
  {
    'constant': true,
    'inputs': [
      {
        'name': 'account',
        'type': 'address'
      }
    ],
    'name': 'isMinter',
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
    'name': 'NUM_EGG_COINS_PER_AXIE',
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
    'constant': true,
    'inputs': [
      {
        'name': '',
        'type': 'address'
      }
    ],
    'name': 'referralPercentage',
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
    'constant': true,
    'inputs': [],
    'name': 'startingPrice',
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
        'name': '_referrers',
        'type': 'address[]'
      },
      {
        'name': '_percentage',
        'type': 'uint256'
      }
    ],
    'name': 'setReferralPercentage',
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
    'constant': true,
    'inputs': [],
    'name': 'minDAIPrice',
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
    'inputs': [
      {
        'name': '_coreExtraContract',
        'type': 'address'
      },
      {
        'name': '_genePoolContract',
        'type': 'address'
      },
      {
        'name': '_eggCoinContract',
        'type': 'address'
      },
      {
        'name': '_minDAIPrice',
        'type': 'uint256'
      },
      {
        'name': '_nextPricePercentage',
        'type': 'uint256'
      },
      {
        'name': '_defaultReferralPercentage',
        'type': 'uint256'
      }
    ],
    'payable': false,
    'stateMutability': 'nonpayable',
    'type': 'constructor'
  },
  {
    'anonymous': false,
    'inputs': [
      {
        'indexed': true,
        'name': '_buyer',
        'type': 'address'
      },
      {
        'indexed': true,
        'name': '_referrer',
        'type': 'address'
      },
      {
        'indexed': false,
        'name': '_amount',
        'type': 'uint8'
      },
      {
        'indexed': false,
        'name': '_price',
        'type': 'uint256'
      },
      {
        'indexed': false,
        'name': '_referralReward',
        'type': 'uint256'
      }
    ],
    'name': 'AxieBought',
    'type': 'event'
  },
  {
    'anonymous': false,
    'inputs': [
      {
        'indexed': true,
        'name': '_receiver',
        'type': 'address'
      }
    ],
    'name': 'AxieRedeemed',
    'type': 'event'
  },
  {
    'anonymous': false,
    'inputs': [
      {
        'indexed': true,
        'name': '_owner',
        'type': 'address'
      },
      {
        'indexed': false,
        'name': '_geneHash',
        'type': 'uint256'
      }
    ],
    'name': 'AxieClaimed',
    'type': 'event'
  },
  {
    'anonymous': false,
    'inputs': [
      {
        'indexed': true,
        'name': '_referrer',
        'type': 'address'
      },
      {
        'indexed': false,
        'name': '_percentage',
        'type': 'uint256'
      }
    ],
    'name': 'ReferralPercentageUpdated',
    'type': 'event'
  },
  {
    'anonymous': false,
    'inputs': [
      {
        'indexed': true,
        'name': 'previousOwner',
        'type': 'address'
      }
    ],
    'name': 'OwnershipRenounced',
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
    'anonymous': false,
    'inputs': [],
    'name': 'Paused',
    'type': 'event'
  },
  {
    'anonymous': false,
    'inputs': [],
    'name': 'Unpaused',
    'type': 'event'
  },
  {
    'anonymous': false,
    'inputs': [
      {
        'indexed': true,
        'name': 'account',
        'type': 'address'
      }
    ],
    'name': 'PauserAdded',
    'type': 'event'
  },
  {
    'anonymous': false,
    'inputs': [
      {
        'indexed': true,
        'name': 'account',
        'type': 'address'
      }
    ],
    'name': 'PauserRemoved',
    'type': 'event'
  },
  {
    'anonymous': false,
    'inputs': [
      {
        'indexed': true,
        'name': 'account',
        'type': 'address'
      }
    ],
    'name': 'MinterAdded',
    'type': 'event'
  },
  {
    'anonymous': false,
    'inputs': [
      {
        'indexed': true,
        'name': 'account',
        'type': 'address'
      }
    ],
    'name': 'MinterRemoved',
    'type': 'event'
  },
  {
    'constant': true,
    'inputs': [
      {
        'name': '_amount',
        'type': 'uint8'
      }
    ],
    'name': 'getPrice',
    'outputs': [
      {
        'name': '_price',
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
        'name': '_amount',
        'type': 'uint8'
      }
    ],
    'name': 'getNextPrice',
    'outputs': [
      {
        'name': '_price',
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
        'name': '_amount',
        'type': 'uint8'
      },
      {
        'name': '_referrer',
        'type': 'address'
      }
    ],
    'name': 'buyAxie',
    'outputs': [
      {
        'name': '_totalPrice',
        'type': 'uint256'
      }
    ],
    'payable': true,
    'stateMutability': 'payable',
    'type': 'function'
  },
  {
    'constant': false,
    'inputs': [
      {
        'name': '_from',
        'type': 'address'
      },
      {
        'name': '',
        'type': 'uint256'
      },
      {
        'name': '_tokenAddress',
        'type': 'address'
      },
      {
        'name': '',
        'type': 'bytes'
      }
    ],
    'name': 'receiveApproval',
    'outputs': [],
    'payable': false,
    'stateMutability': 'nonpayable',
    'type': 'function'
  },
  {
    'constant': false,
    'inputs': [
      {
        'name': '_genes',
        'type': 'uint256'
      },
      {
        'name': '_geneHash',
        'type': 'uint256'
      }
    ],
    'name': 'unlockAxie',
    'outputs': [
      {
        'name': '_axieId',
        'type': 'uint256'
      }
    ],
    'payable': false,
    'stateMutability': 'nonpayable',
    'type': 'function'
  },
  {
    'constant': false,
    'inputs': [
      {
        'name': '_minDAIPrice',
        'type': 'uint256'
      }
    ],
    'name': 'setMinDAIPrice',
    'outputs': [],
    'payable': false,
    'stateMutability': 'nonpayable',
    'type': 'function'
  },
  {
    'constant': false,
    'inputs': [
      {
        'name': '_nextPricePercentage',
        'type': 'uint256'
      }
    ],
    'name': 'setNextPricePercentage',
    'outputs': [],
    'payable': false,
    'stateMutability': 'nonpayable',
    'type': 'function'
  }
]
";
        #endregion
        private static ulong marketPlaceChannelId = 423343101428498435;
        private static ulong botCommandChannelId = 487932149354463232;
        private static BigInteger lastBlockChecked = 6379721;
        private static float initialAxieMeoS2Price = 1.0001f;
        public static bool IsServiceOn= true;
        public AxieSaleGetter()
        {
        }

        public static async Task GetData()
        {
            IsServiceOn = true;
            var web3 = new Web3("https://mainnet.infura.io");
            var auctionContract = web3.Eth.GetContract(auctionABI, AxieCoreContractAddress);
            var labContract = web3.Eth.GetContract(labABI, AxieLabContractAddress);
            var auctionSuccesfulEvent = auctionContract.GetEvent("AuctionSuccessful");
            var axieBoughtEvent = labContract.GetEvent("AxieBought");
            var lastBlock = await GetLastBlockCheckpoint(web3);
            var firstBlock = GetInitialBlockCheckpoint(lastBlock.BlockNumber);
            while (true)
            {
                try
                {
                    var auctionFilterAll = auctionSuccesfulEvent.CreateFilterInput(firstBlock, lastBlock);
                    var labFilterAll = axieBoughtEvent.CreateFilterInput(firstBlock, lastBlock);
                    var auctionLogs = await auctionSuccesfulEvent.GetAllChanges<AuctionSuccessfulEvent>(auctionFilterAll);
                    var labLogs = await axieBoughtEvent.GetAllChanges<AxieBoughtEvent>(labFilterAll);
                    firstBlock = lastBlock;
                    lastBlock = await GetLastBlockCheckpoint(web3);
                    if (auctionLogs != null && auctionLogs.Count > 0)
                    {
                        foreach (var log in auctionLogs)
                        {
                            int axieId = Convert.ToInt32(log.Event.tokenId.ToString());
                            float priceinEth = Convert.ToSingle(Nethereum.Util.UnitConversion.Convert.FromWei(log.Event.totalPrice).ToString());
                            await PostAxieToMarketplace(axieId, priceinEth);
                            await Task.Delay(5000);
                        };
                        Console.WriteLine("End of batch");
                    }
                    if (labLogs != null && labLogs.Count > 0)
                    {
                        foreach (var log in labLogs)
                        {
                            float priceinEth = Convert.ToSingle(Nethereum.Util.UnitConversion.Convert.FromWei(log.Event.price).ToString());
                            int amount = log.Event.amount;
                            await PostLabSaleToBotCommand(amount, priceinEth);
                            await Task.Delay(5000);
                        };
                        Console.WriteLine("End of batch");
                    }
                    await Task.Delay(60000);

                }
                catch (Exception ex)
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
            SocketChannel channel = Bot.GetChannelContext(marketPlaceChannelId);  //479664564061995019
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
            if(price >= 1 || axieData.hasMystic) await msgChannel.SendMessageAsync("", false, axieData.EmbedAxieSaleData( price));

        }

        private static async Task PostLabSaleToBotCommand(int count, float price)
        {
            SocketChannel channel = Bot.GetChannelContext(botCommandChannelId);  //479664564061995019 botCommandChannelId
            IMessageChannel msgChannel = channel as IMessageChannel;
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("Lab alert!!!");
            embed.WithUrl("https://axieinfinity.com/axie-lab");
            embed.WithDescription($"{count} " + (count > 1? "eggs have" : "egg has") + $" been bought for {price.ToString("F4")} ether"+ (count > 1 ? " each" : "") + "!");
            embed.WithColor(Color.Orange);
            await msgChannel.SendMessageAsync("", false , embed.Build());

        }

        private static async Task<BlockParameter> GetLastBlockCheckpoint(Web3 web3)
        {
            var lastBlock = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            var blockNumber = lastBlock.Value - 7;
            return new BlockParameter(new HexBigInteger(blockNumber));
        }

        private static BlockParameter GetInitialBlockCheckpoint(HexBigInteger blockNumber)
        {
            var firstBlock = blockNumber.Value - 10;
            return new BlockParameter(new HexBigInteger(firstBlock));
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

    public class AxieBoughtEvent
    {
        [Parameter("address", "_buyer", 1, true)]
        public string buyer { get; set; }

        [Parameter("address", "_referrer", 2, true)]
        public string referrer { get; set; }

        [Parameter("int8", "_amount", 3)]
        public int amount { get; set; }

        [Parameter("uint256", "_price", 4)]
        public BigInteger price { get; set; }

        [Parameter("uint256", "_referralReward", 4)]
        public BigInteger referralReward { get; set; }
    }
}

