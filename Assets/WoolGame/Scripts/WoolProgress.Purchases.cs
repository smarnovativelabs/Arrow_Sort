using System;
using System.Collections.Generic;
using UnityEngine;
namespace WoolGame
{
    public static partial class WoolProgress
    {
        public static bool PermanentNoAds {get{EnsureLoaded();return data.permanentNoAds;}}
        public static bool OwnsProduct(string id){EnsureLoaded();var product=WoolIapCatalog.Find(id);return product!=null&&product.purchaseOnce&&(id==WoolIapCatalog.RemoveAds&&data.permanentNoAds||data.ownedProducts!=null&&data.ownedProducts.Contains(id));}
        // Called only after a store receipt has been validated. Save the grant and token together.
        internal static bool DeliverPurchase(string productId,string transactionKey,bool restored=false)
        {
            var p=WoolIapCatalog.Find(productId);
            if(p==null||string.IsNullOrWhiteSpace(transactionKey))throw new ArgumentException("Unknown purchase or missing transaction token.");
            EnsureLoaded();if(data.purchaseTransactions==null)data.purchaseTransactions=new List<string>();if(data.ownedProducts==null)data.ownedProducts=new List<string>();
            if(data.purchaseTransactions.Contains(transactionKey))return false;
            string snapshot=JsonUtility.ToJson(data);
            try
            {
                bool grant=!restored&&(!p.purchaseOnce||!OwnsProduct(productId))&&(!p.removeAds||!data.removeAdsBonusGranted);
                if(grant)
                {
                    if(data.coins>int.MaxValue-p.coins||data.removePowers>int.MaxValue-p.powers||data.sortPowers>int.MaxValue-p.powers||data.boostPowers>int.MaxValue-p.powers||UnlockPowerCount>int.MaxValue-p.powers)
                        throw new InvalidOperationException("Wallet or inventory limit reached.");
                    data.coins+=p.coins;data.collectedCoins+=p.coins;RecordDailyCoins(p.coins);
                    if(p.powers>0){if(data.spin==null)data.spin=new WoolSpinState();data.spin.unlockPowers+=p.powers;data.removePowers+=p.powers;data.sortPowers+=p.powers;data.boostPowers+=p.powers;}
                }
                if(p.removeAds){data.permanentNoAds=true;if(grant)data.removeAdsBonusGranted=true;}
                if(p.purchaseOnce&&!data.ownedProducts.Contains(productId))data.ownedProducts.Add(productId);
                data.purchaseTransactions.Add(transactionKey);Save();return grant;
            }
            catch{data=Read(snapshot);Normalize();throw;}
        }
    }
}
