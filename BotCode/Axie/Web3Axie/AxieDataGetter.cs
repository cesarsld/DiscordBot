﻿using System;
using System.Collections.Generic;
using Nethereum.Web3;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RPC.Eth.DTOs;
using System.Threading.Tasks;
using System.Numerics;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Discord;
using Discord.WebSocket;
using DiscordBot.Axie.SubscriptionServices;
using DiscordBot.Axie.SubscriptionServices.PremiumServices;
using Newtonsoft.Json.Linq;
using DiscordBot.Axie;

using Nethereum.Util;
using System.Linq;
namespace DiscordBot.Axie.Web3Axie
{
    public class AxieDataGetter
    {

        #region ABI & contract declaration
        private static string AxieCoreContractAddress = "0xF4985070Ce32b6B1994329DF787D1aCc9a2dd9e2";
        private static string NftAddress = "0xf5b0a3efb8e8e4c201e2a935f110eaaf3ffecb8d";
        private static string AxieLabContractAddress = "0x99ff9f4257D5b6aF1400C994174EbB56336BB79F";
        private static string AxieExtraDataContract = "0x10e304a53351b272dc415ad049ad06565ebdfe34";
        private static string ExpCheckpointContract = "0x71FfC95Ca3BcEbF26024f689F40006182916167f";
        private static string AxieLandPresaleContract = "0x2299a91cc0bffd8c7f71349da8ab03527b79724f";
        private static string landSaleABI = @"[{'constant':true,'inputs':[{'name':'','type':'uint8'}],'name':'chestCap','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'initialDiscountPercentage','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'cashbackPercentage','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_chestType','type':'uint8'},{'name':'_chestAmount','type':'uint256'},{'name':'_tokenAddress','type':'address'},{'name':'_maxTokenAmount','type':'uint256'},{'name':'_minConversionRate','type':'uint256'},{'name':'_owner','type':'address'}],'name':'purchaseFor','outputs':[],'payable':true,'stateMutability':'payable','type':'function'},{'constant':true,'inputs':[],'name':'daiAddress','outputs':[{'name':'','type':'address'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_chestCap','type':'uint256[4]'}],'name':'setChestCap','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'loomAddress','outputs':[{'name':'','type':'address'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'endedAt','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[],'name':'unpause','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'ethAddress','outputs':[{'name':'','type':'address'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'lunaBankAddress','outputs':[{'name':'','type':'address'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'paused','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'defaultReferralPercentage','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[],'name':'withdrawEther','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_referrers','type':'address[]'},{'name':'_percentage','type':'uint256[]'}],'name':'setReferralPercentages','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'savannahChestPrice','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'initialDiscountDays','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[],'name':'pause','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_chestType','type':'uint8'},{'name':'_chestAmount','type':'uint256'},{'name':'_tokenAddress','type':'address'},{'name':'_maxTokenAmount','type':'uint256'},{'name':'_minConversionRate','type':'uint256'},{'name':'_referrer','type':'address'}],'name':'purchase','outputs':[],'payable':true,'stateMutability':'payable','type':'function'},{'constant':true,'inputs':[{'name':'','type':'address'}],'name':'customTokenRate','outputs':[{'name':'quote','type':'address'},{'name':'value','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_token','type':'address'}],'name':'withdrawToken','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'mysticChestPrice','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_newAdmin','type':'address'}],'name':'changeAdmin','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_from','type':'address'},{'name':'_value','type':'uint256'},{'name':'_tokenAddress','type':'address'},{'name':'','type':'bytes'}],'name':'receiveApproval','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_tokenAddresses','type':'address[]'},{'components':[{'name':'quote','type':'address'},{'name':'value','type':'uint256'}],'name':'_rates','type':'tuple[]'}],'name':'setCustomTokenRates','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[],'name':'removeAdmin','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'kyber','outputs':[{'name':'','type':'address'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[{'name':'_chestType','type':'uint8'},{'name':'_chestAmount','type':'uint256'},{'name':'_tokenAddress','type':'address'}],'name':'getPrice','outputs':[{'name':'_tokenAmount','type':'uint256'},{'name':'_minConversionRate','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'lunaContract','outputs':[{'name':'','type':'address'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[{'name':'','type':'address'}],'name':'referralPercentage','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'arcticChestPrice','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'startedAt','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'admin','outputs':[{'name':'','type':'address'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'forestChestPrice','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'inputs':[{'name':'_lunaContract','type':'address'},{'name':'_lunaBankAddress','type':'address'}],'payable':false,'stateMutability':'nonpayable','type':'constructor'},{'payable':true,'stateMutability':'payable','type':'fallback'},{'anonymous':false,'inputs':[{'indexed':true,'name':'_chestType','type':'uint8'},{'indexed':false,'name':'_chestAmount','type':'uint256'},{'indexed':true,'name':'_tokenAddress','type':'address'},{'indexed':false,'name':'_tokenAmount','type':'uint256'},{'indexed':false,'name':'_totalPrice','type':'uint256'},{'indexed':false,'name':'_lunaCashbackAmount','type':'uint256'},{'indexed':false,'name':'_buyer','type':'address'},{'indexed':true,'name':'_owner','type':'address'}],'name':'ChestPurchased','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'name':'_referrer','type':'address'},{'indexed':false,'name':'_referralReward','type':'uint256'}],'name':'ReferralRewarded','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'name':'_referrer','type':'address'},{'indexed':false,'name':'_percentage','type':'uint256'}],'name':'ReferralPercentageUpdated','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'name':'_tokenAddress','type':'address'},{'indexed':true,'name':'_quoteTokenAddress','type':'address'},{'indexed':false,'name':'_rate','type':'uint256'}],'name':'CustomTokenRateUpdated','type':'event'},{'anonymous':false,'inputs':[],'name':'Paused','type':'event'},{'anonymous':false,'inputs':[],'name':'Unpaused','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'name':'_oldAdmin','type':'address'},{'indexed':true,'name':'_newAdmin','type':'address'}],'name':'AdminChanged','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'name':'_oldAdmin','type':'address'}],'name':'AdminRemoved','type':'event'}]";
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
        private static string extraABI = @"[
  {
    'constant': true,
    'inputs': [
      {
        'name': '_axieId',
        'type': 'uint256'
      }
    ],
    'name': 'getExtra',
    'outputs': [
      {
        'name': '',
        'type': 'uint256'
      },
      {
        'name': '',
        'type': 'uint256'
      },
      {
        'name': '',
        'type': 'uint256'
      },
      {
        'name': '',
        'type': 'uint256'
      }
    ],
    'payable': false,
    'stateMutability': 'view',
    'type': 'function'
  }
]";
        private static string expABI = @"[
{
    'constant': true,
    'inputs': [{
        'name': '_axieId',
        'type': 'uint256'
    }],
    'name': 'getCheckpoint',
    'outputs': [{
        'name': '_exp',
        'type': 'uint256'
    }, {
        'name': '_createdAt',
        'type': 'uint256'
    }],
    'payable': false,
    'stateMutability': 'view',
    'type': 'function'
}
]";

        #endregion
        private static ulong marketPlaceChannelId = 423343101428498435;
        private static ulong botCommandChannelId = 487932149354463232;
        private static BigInteger lastBlockChecked = 6379721;
        public static double eggLabPrice = 0.6f; //change to double

        //public static Queue<Task<IUserMessage>> messageQueue = new Queue<Task<IUserMessage>>();

        public static bool IsServiceOn = false;
        public AxieDataGetter()
        {
        }

        public static async Task StartService()
        {
            if(!IsServiceOn)
            {
                IsServiceOn = true;
                await GetData();
            }
        }

        public static async Task<AxieExtraData> GetExtraData(int axieId)
        {
            var web3 = new Web3("https://mainnet.infura.io");
            var extraDataContract = web3.Eth.GetContract(extraABI, AxieExtraDataContract);
            var getExtraFunction = extraDataContract.GetFunction("getExtra");
            try
            {
                var result = await getExtraFunction.CallDeserializingToObjectAsync<AxieExtraData>(new BigInteger(axieId));
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.ReadLine();
            }
            return null;
        }

        public static async Task<int> GetSyncedExp(int axieId)
        {
            var web3 = new Web3("https://mainnet.infura.io");
            var expContract = web3.Eth.GetContract(expABI, ExpCheckpointContract);
            var getExpFunction = expContract.GetFunction("getCheckpoint");
            try
            {
                var result = await getExpFunction.CallDeserializingToObjectAsync<AxieExpCheckPoint>(new BigInteger(axieId));
                return Convert.ToInt32(result.exp.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.ReadLine();
            }
            return 0;
        }

        public static async Task<AxieExtraData> test(int axieId)
        {
            var web3 = new Web3("https://mainnet.infura.io");
            var auctionContract = web3.Eth.GetContract(auctionABI, AxieCoreContractAddress);
            var getSellerInfoFunction = auctionContract.GetFunction("getAuction");
            try
            {
                var lastBlock = await GetLastBlockCheckpoint(web3);
                var firstBlock = GetInitialBlockCheckpoint(lastBlock.BlockNumber);
                object[] input = new object[2];
                input[0] = NftAddress;
                input[1] = new BigInteger(axieId);
                var result = await getSellerInfoFunction.CallDeserializingToObjectAsync<SellerInfo>(firstBlock, input);
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.ReadLine();
            }
            return null;
        }

        public static void ShutDownLoop() => IsServiceOn = false;

        public static async Task GetData()
        {
            IsServiceOn = true;
            TaskHandler.IsOn = true;
            _ = TaskHandler.UpdateServiceCheckLoop();
            var web3 = new Web3("https://mainnet.infura.io");
            //get contracts
            var auctionContract = web3.Eth.GetContract(auctionABI, AxieCoreContractAddress);
            var getSellerInfoFunction = auctionContract.GetFunction("getAuction");
            var labContract = web3.Eth.GetContract(labABI, AxieLabContractAddress);
            var landPresaleContract = web3.Eth.GetContract(landSaleABI, AxieLandPresaleContract);
            //get events
            var auctionSuccesfulEvent = auctionContract.GetEvent("AuctionSuccessful");
            var auctionCreatedEvent = auctionContract.GetEvent("AuctionCreated");
            //var axieBoughtEvent = labContract.GetEvent("AxieBought");
            var auctionCancelled = auctionContract.GetEvent("AuctionCancelled");
            var chestPurchasedEvent = landPresaleContract.GetEvent("ChestPurchased");

            //set block range search
            var lastBlock = await GetLastBlockCheckpoint(web3);
            var firstBlock = GetInitialBlockCheckpoint(lastBlock.BlockNumber);
            while (IsServiceOn)
            {
                try
                {
                    //prepare filters
                    var auctionFilterAll = auctionSuccesfulEvent.CreateFilterInput(firstBlock, lastBlock);
                    var auctionCancelledFilterAll = auctionCancelled.CreateFilterInput(firstBlock, lastBlock);
                    var auctionCreationFilterAll = auctionCreatedEvent.CreateFilterInput(firstBlock, lastBlock);
                    var landSaleFilterAll = chestPurchasedEvent.CreateFilterInput(firstBlock, lastBlock);
                    //var labFilterAll = axieBoughtEvent.CreateFilterInput(firstBlock, lastBlock);

                    //get logs from blockchain
                    var auctionLogs = await auctionSuccesfulEvent.GetAllChanges<AuctionSuccessfulEvent>(auctionFilterAll);
                    var auctionCancelledLogs = await auctionSuccesfulEvent.GetAllChanges<AuctionCancelledEvent>(auctionFilterAll);
                    //var labLogs = await axieBoughtEvent.GetAllChanges<AxieBoughtEvent>(labFilterAll);
                    var auctionCreationLogs = await auctionCreatedEvent.GetAllChanges<AuctionCreatedEvent>(auctionCreationFilterAll);
                    var landLogs = await chestPurchasedEvent.GetAllChanges<ChestPurchasedEvent>(landSaleFilterAll);


                    BigInteger latestLogBlock = 0;
                    //read logs
                    if (auctionCancelledLogs != null && auctionCancelledLogs.Count > 0) _ = HandleAuctionCancelTriggers(auctionCancelledLogs);

                    if (auctionCreationLogs != null && auctionCreationLogs.Count > 0)
                    {

                        foreach (var log in auctionCreationLogs)
                        {
                            var axie = await AxieObject.GetAxieFromApi(Convert.ToInt32(log.Event.tokenId.ToString()));
                            var price = log.Event.startingPrice;
                            //await axie.GetTrueAuctionData();
                            _ = CheckForSnipeFilters(axie, price);
                        }
                    }

                    foreach (var log in landLogs)
                    {
                        if (log.Event.totalPrice >= BigInteger.Parse("1000000000000000000"))
                        {
                            float priceinEth = Convert.ToSingle(Nethereum.Util.UnitConversion.Convert.FromWei(log.Event.totalPrice).ToString());
                            await PostLandSaleTOMarketPlace(priceinEth, log.Event.chestType, Convert.ToInt32(log.Event.chestAmount.ToString()));
                        }
                    }

                    if (auctionLogs != null && auctionLogs.Count > 0)
                    {
                        foreach (var log in auctionLogs)
                        {
                            latestLogBlock = log.Log.BlockNumber.Value;
                            int axieId = Convert.ToInt32(log.Event.tokenId.ToString());
                            float priceinEth = Convert.ToSingle(Nethereum.Util.UnitConversion.Convert.FromWei(log.Event.totalPrice).ToString());
                            _ = CheckForExistingMarketTriggers(axieId);
                            object[] input = new object[2];
                            input[0] = NftAddress;
                            input[1] = log.Event.tokenId;
                            var sellerInfo = await getSellerInfoFunction.CallDeserializingToObjectAsync<SellerInfo>(
                                new BlockParameter(new HexBigInteger(log.Log.BlockNumber.Value - 1)), input);
                            await AlertSeller(axieId, priceinEth, sellerInfo.seller);
                            await PostAxieToMarketplace(axieId, priceinEth);
                            await Task.Delay(5000);
                        };
                        Console.WriteLine("End of batch");
                    }
                    //if (labLogs != null && labLogs.Count > 0)
                    //{
                    //    foreach (var log in labLogs)
                    //    {
                    //        latestLogBlock = log.Log.BlockNumber.Value;
                    //        float priceinEth = Convert.ToSingle(Nethereum.Util.UnitConversion.Convert.FromWei(log.Event.price).ToString());
                    //        eggLabPrice = priceinEth * 1.07;
                    //        int amount = log.Event.amount;
                    //        await PostLabSaleToBotCommand(amount, priceinEth);
                    //        await Task.Delay(5000);
                    //    };
                    //    Console.WriteLine("End of batch");
                    //}
                    await Task.Delay(60000);
                    if (latestLogBlock > lastBlock.BlockNumber.Value) firstBlock = new BlockParameter(new HexBigInteger(latestLogBlock + 1));
                    else firstBlock = new BlockParameter(new HexBigInteger(lastBlock.BlockNumber.Value + 1));
                    lastBlock = await GetLastBlockCheckpoint(web3);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Logger.Log(ex.ToString());
                    IsServiceOn = false;
                    await PostToMarketplace("Something went wrong while fetching data from the blockchain... Please retart this service using this command `>axie rebootSales`.");
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
            var axieData = await AxieObject.GetAxieFromApi(index);

            if (axieData.stage == 4)
            {
                if (price >= 1 || axieData.hasMystic) await msgChannel.SendMessageAsync("", false, axieData.EmbedAxieSaleData(price).Build());
            }
            else if(price >= 1) await msgChannel.SendMessageAsync("", false, axieData.EmbedAxieSaleData(price).Build());
            //else if (price >= 1 || axieData.hasMystic) await msgChannel.SendMessageAsync("", false, axieData.EmbedAxieSaleData(price));

        }

        private static async Task PostLandSaleTOMarketPlace(float price, int chestType, int count)
        {
            SocketChannel channel = Bot.GetChannelContext(marketPlaceChannelId);  //479664564061995019
            IMessageChannel msgChannel = channel as IMessageChannel;
            var embed = new EmbedBuilder();
            embed.WithTitle("Land sold!");
            embed.WithDescription($"{price.ToString("F4")} eth worth of land have been sold!");
            var chest = "";
            var chestImage = "";
            Color color = Color.Default;
            switch (chestType)
            {
                case 0:
                    chest = "Savannah";
                    chestImage = "https://cdn.axieinfinity.com/assets/images/e5610a54886f5a93e053914b6980c96c.png";
                    color = Color.Gold;
                    break;
                case 1:
                    chest = "Forest";
                    chestImage = "https://cdn.axieinfinity.com/assets/images/10b76293695edefb48846a47a721a11b.png";
                    color = Color.Green;
                    break;
                case 2:
                    chest = "Arctic";
                    chestImage = "https://cdn.axieinfinity.com/assets/images/1e339cb5ff522af62158c7f0807646be.png";
                    color = new Color(255, 255, 255);
                    break;
                case 3:
                    chest = "Mystic";
                    chestImage = "https://cdn.axieinfinity.com/assets/images/85ca5805f0d454f4abdacb01e86263dc.png";
                    color = Color.Purple;
                    break;
            }
            embed.AddField(chest + " chests count", count, true);
            embed.WithColor(color);
            embed.WithUrl("https://land.axieinfinity.com/purchase");
            embed.WithThumbnailUrl(chestImage);
            await msgChannel.SendMessageAsync("", embed: embed.Build());
        }

        private static async Task PostLabSaleToBotCommand(int count, float price)
        {
            SocketChannel channel = Bot.GetChannelContext(botCommandChannelId);  //479664564061995019 botCommandChannelId
            IMessageChannel msgChannel = channel as IMessageChannel;
            EmbedBuilder embed = new EmbedBuilder();
            embed.WithTitle("Lab alert!!!");
            embed.WithUrl("https://axieinfinity.com/axie-lab");
            embed.WithDescription($"{count} " + (count > 1 ? "pods have" : "pod has") + $" been bought for {price.ToString("F4")} ether" + (count > 1 ? " each" : "") + "!");
            embed.WithColor(Color.Orange);
            await msgChannel.SendMessageAsync("", false, embed.Build());

        }

        private static async Task<BlockParameter> GetLastBlockCheckpoint(Web3 web3)
        {
            var lastBlock = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            var blockNumber = lastBlock.Value - 12;
            return new BlockParameter(new HexBigInteger(blockNumber));
        }

        private static BlockParameter GetInitialBlockCheckpoint(HexBigInteger blockNumber)
        {
            var firstBlock = blockNumber.Value - 10;
            return new BlockParameter(new HexBigInteger(firstBlock));
        }

        private static async Task CheckForExistingMarketTriggers(int axieID)
        {
            var subList = await SubscriptionServicesHandler.GetSubList();
            bool hasTriggered = false;
            if (subList != null)
            {
                foreach (var user in subList)
                {
                    var marketService = user.GetServiceList().FirstOrDefault(s => s.name == ServiceEnum.MarketPlace) as MarketplaceService;
                    if (marketService != null)
                    {
                        var trigger = marketService.GetTriggerFromId(axieID);
                        if (trigger != null)
                        {
                            hasTriggered = true;
                            await Bot.GetUser(user.GetId()).SendMessageAsync("", false, trigger.GetMissedTriggerMessage().Build());
                            marketService.RemoveTrigger(trigger);
                            await Task.Delay(5000);
                        }
                    }
                }
            }
            if (hasTriggered)
            {
                _ = SubscriptionServicesHandler.SetSubList();
                hasTriggered = false;
                
            }
        }

        private static async Task CheckForExistingAuctionWatchTriggers(int axieID, BigInteger price)
        {
            var subList = await SubscriptionServicesHandler.GetSubList();
            bool hasTriggered = false;
            var axieData = await AxieObject.GetAxieFromApi(axieID);
            if (subList != null)
            {
                foreach (var user in subList)
                {

                    var auctionWatchService = user.GetServiceList().FirstOrDefault(s => s.name == ServiceEnum.AuctionWatch) as AuctionWatchService;
                    if (auctionWatchService != null)
                    {
                        var list = auctionWatchService.GetList();
                        foreach (var filter in list)
                        {
                            if (filter.Match(axieData, price))
                            {
                                await Bot.GetUser(user.GetId()).SendMessageAsync("", embed: (await filter.GetTriggerMessage(axieID, price)).Build());
                            }
                        }
                    }
                }
            }
            if (hasTriggered)
            {
                _ = SubscriptionServicesHandler.SetSubList();
                hasTriggered = false;

            }
        }


        public static async Task AlertSeller(int axieID, float price, string address)
        {
            var axieData = await AxieObject.GetAxieFromApi(axieID);
            var subList = await SubscriptionServicesHandler.GetSubList();
            if (subList != null)
            {
                foreach (var user in subList)
                {
                    var marketService = user.GetServiceList().FirstOrDefault(s => s.name == ServiceEnum.MarketPlace) as MarketplaceService;
                    if (marketService != null)
                    {
                        if (marketService.GetNotifStatus())
                        {
                            if (await AxieHolderListHandler.DoesUserHaveAddress(user.GetId(), address))
                            {
                                await Bot.GetUser(user.GetId()).SendMessageAsync("", false, axieData.EmbedAxieSaleData(price).Build());
                            }
                        }
                    }
                }
            }
        }

        private static async Task CheckForSnipeFilters(AxieObject axie, BigInteger price)
        {
            var subList = await SubscriptionServicesHandler.GetSubList();
            foreach (var user in subList)
            {
                var marketService = user.GetServiceList().FirstOrDefault(s => s.name == ServiceEnum.AuctionWatch) as AuctionWatchService;
                if (marketService != null)
                {
                    var message = await marketService.IsFilterMatch(axie, price);
                    if (message != null)
                    {
                        await Bot.GetUser(user.GetId()).SendMessageAsync("", embed: message.Build());
                    }
                }
            }
        }

        private static async Task HandleAuctionCancelTriggers(List<EventLog<AuctionCancelledEvent>> auctionCancelledLogs)
        {
            if (auctionCancelledLogs != null && auctionCancelledLogs.Count > 0)
            {
                foreach (var log in auctionCancelledLogs)
                {
                    int axieId = Convert.ToInt32(log.Event.tokenId.ToString());
                    await CheckForExistingMarketTriggers(axieId);
                }
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


    public class AuctionCreatedEvent
    {
        [Parameter("address", "_nftAddress", 1, true)]
        public string nftAddress { get; set; }

        [Parameter("uint256", "_tokenId", 2, true)]
        public BigInteger tokenId { get; set; }

        [Parameter("uint256", "_startingPrice", 3)]
        public BigInteger startingPrice { get; set; }

        [Parameter("uint256", "_endingPrice", 4)]
        public BigInteger endingPrice { get; set; }

        [Parameter("uint256", "_duration", 5)]
        public BigInteger duration { get; set; }

        [Parameter("address", "_seller", 6)]
        public string seller { get; set; }

    }

    public class AuctionCancelledEvent
    {
        [Parameter("address", "_nftAddress", 1, true)]
        public string nftAddress { get; set; }

        [Parameter("uint256", "_tokenId", 2, true)]
        public BigInteger tokenId { get; set; }
    }

    [FunctionOutput]
    public class AxieExtraData 
    { 
        [Parameter("uint256", "_sireId", 1)]
        public BigInteger sireId { get; set; }

        [Parameter("uint256", "_matronId", 2)]
        public BigInteger matronId { get; set; }

        [Parameter("uint256", "_exp", 3)]
        public BigInteger exp { get; set; }

        [Parameter("uint256", "_numBreeding", 4)]
        public BigInteger numBreeding { get; set; }
    }


    [FunctionOutput]
    public class AxieExpCheckPoint
    {
        [Parameter("uint256", "_exp", 3)]
        public BigInteger exp { get; set; }

        [Parameter("uint256", "_createdAt", 4)]
        public BigInteger createdAt { get; set; }
    }

    [FunctionOutput]
    public class SellerInfo
    {
        [Parameter("address", "seller", 1)]
        public string seller { get; set; }

        [Parameter("uint256", "startingPrice", 2)]
        public BigInteger startingPrice { get; set; }

        [Parameter("uint256", "endingPrice", 3)]
        public BigInteger endingPrice { get; set; }

        [Parameter("uint256", "duration", 4)]
        public BigInteger duration { get; set; }

        [Parameter("uint256", "startedAt", 5)]
        public BigInteger startedAt { get; set; }
    }

    [Function("getExtra")]
    public class AxieExtraFunction 
    {
        [Parameter("uint256", "_axieId", 1)]
        public BigInteger axieId { get; set; }

    }
    public class ChestPurchasedEvent
    {
        [Parameter("uint8", "_chestType", 1, true)]
        public int chestType { get; set; }

        [Parameter("uint256", "_chestAmount", 2)]
        public BigInteger chestAmount { get; set; }

        [Parameter("address", "_tokenAddress", 3, true)]
        public string tokenAddress { get; set; }

        [Parameter("uint256", "_tokenAmount", 4)]
        public BigInteger tokenAmount { get; set; }

        [Parameter("uint256", "_totalPrice", 5)]
        public BigInteger totalPrice { get; set; }

        [Parameter("uint256", "_lunaCashbackAmount", 6)]
        public BigInteger lunaCashbackAmount { get; set; }

        [Parameter("address", "_buyer", 7)]
        public string buyer { get; set; }

        [Parameter("address", "_owner", 8, true)]
        public string owner { get; set; }
    }
}
