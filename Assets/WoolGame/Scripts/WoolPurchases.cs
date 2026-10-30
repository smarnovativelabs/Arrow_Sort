using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Purchasing;
namespace WoolGame
{
    public sealed class WoolPurchases : MonoBehaviour
    {
        static WoolPurchases instance;
        public static WoolPurchases Current=>instance;
        public static WoolPurchases Ensure(){if(instance)return instance;return new GameObject("Google Play purchases").AddComponent<WoolPurchases>();}
        public event Action<string,string> Message;
        StoreController store;WoolIapConfiguration configuration;
        bool connecting,connected,busy,restoring;string buying;
        public bool Ready=>connected&&configuration&&configuration.productsPublished;
        void Awake(){if(instance&&instance!=this){Destroy(gameObject);return;}instance=this;DontDestroyOnLoad(gameObject);configuration=Resources.Load<WoolIapConfiguration>("ArrowSortPurchases");Initialize();}
#if UNITY_ANDROID || UNITY_EDITOR
        async void Initialize()
#else
        void Initialize()
#endif
        {
#if !UNITY_ANDROID && !UNITY_EDITOR
            return; // Unsupported player platform.
#else
            if(connecting||connected||!configuration||!configuration.productsPublished)return;
            try{
                if(store==null){
                    store=UnityIAPServices.StoreController();
                    store.OnStoreConnected+=Connected;store.OnStoreDisconnected+=Disconnected;
                    store.OnProductsFetched+=ProductsFetched;store.OnProductsFetchFailed+=ProductsFailed;
                    store.OnPurchasePending+=Pending;store.OnPurchaseConfirmed+=Confirmed;store.OnPurchaseFailed+=Failed;
                    store.OnPurchaseDeferred+=Deferred;store.OnPurchasesFetched+=PurchasesFetched;
                    store.OnPurchasesFetchFailed+=PurchasesFailed;store.OnAuthAccountChanged+=AccountChanged;
                    store.ProcessPendingOrdersOnPurchasesFetched(true);
                }
                connecting=true;await store.Connect();
            }catch(Exception e){connecting=false;Debug.LogWarning("Google Play billing setup: "+e.GetType().Name);}
#endif
        }
        List<ProductDefinition> Definitions(){var result=new List<ProductDefinition>();foreach(var p in WoolIapCatalog.Products)result.Add(new ProductDefinition(p.id,p.purchaseOnce?ProductType.NonConsumable:ProductType.Consumable));return result;}
        void Connected(){connecting=false;connected=true;store.FetchProducts(Definitions());}
        void Disconnected(StoreConnectionFailureDescription failure){connected=connecting=busy=false;buying=null;Debug.LogWarning("Google Play billing disconnected: "+failure.Message);}
        void ProductsFetched(List<Product> products){store.FetchPurchases();}
        void ProductsFailed(ProductFetchFailed failure){busy=false;Debug.LogWarning("Google Play products unavailable: "+failure.FailureReason);}
        void AccountChanged(){busy=false;buying=null;store.FetchProducts(Definitions());}
        public string Price(string id){return Ready?store.GetProductById(id)?.metadata?.localizedPriceString:null;}
        public bool CanBuy(string id){return Ready&&!busy&&!WoolProgress.OwnsProduct(id)&&store.GetProductById(id)?.availableToPurchase==true;}
        public void Buy(string id)
        {
            if(PurchasesBlockedByPreview){Tell("PURCHASE UNAVAILABLE","Purchases are disabled during a level preview.");return;}
            if(!CanBuy(id)){Initialize();Tell("STORE UNAVAILABLE","This item is unavailable right now. Please try again later.");return;}
            busy=true;buying=id;
            try{store.PurchaseProduct(id);}catch(Exception){busy=false;buying=null;Tell("PURCHASE UNAVAILABLE","The store could not open. Please try again.");}
        }
        public void Restore()
        {
            if(PurchasesBlockedByPreview||!Ready){Initialize();Tell("STORE UNAVAILABLE","The store is unavailable right now. Please try again later.");return;}
            if(busy)return;busy=restoring=true;store.FetchPurchases();
        }
        [Serializable] sealed class StoreReceipt { public string Store,Payload; }
        [Serializable] sealed class GooglePayload { public string json; }
        [Serializable] sealed class GooglePurchase { public string productId,packageName,purchaseToken; public int purchaseState=-1; }
        // Trust the Google Play Billing SDK callback; no local RSA signature validation.
        // Keep product/state/package checks and the same token hash for duplicate delivery protection.
        internal static string StoreTransactionKey(string receipt,string id,string packageName) {
            if(string.IsNullOrWhiteSpace(receipt)||WoolIapCatalog.Find(id)==null)throw new InvalidOperationException("Missing receipt or unknown product.");
            var envelope=JsonUtility.FromJson<StoreReceipt>(receipt);
            if(envelope==null||envelope.Store!="GooglePlay"||string.IsNullOrWhiteSpace(envelope.Payload))throw new InvalidOperationException("Expected Google Play receipt.");
            var payload=JsonUtility.FromJson<GooglePayload>(envelope.Payload);
            if(payload==null||string.IsNullOrWhiteSpace(payload.json))throw new InvalidOperationException("Missing Google Play purchase data.");
            var purchase=JsonUtility.FromJson<GooglePurchase>(payload.json);
            if(purchase==null||purchase.productId!=id||purchase.packageName!=packageName||purchase.purchaseState!=0||string.IsNullOrWhiteSpace(purchase.purchaseToken))throw new InvalidOperationException("Receipt does not match a completed purchase.");
            using(var hash=SHA256.Create())return Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(id+":"+purchase.purchaseToken)));
        }
        bool PurchasesBlockedByPreview { get {
#if UNITY_EDITOR
            return false; // Fake purchases are allowed in disposable preview saves.
#else
            return WoolProgress.IsPreview;
#endif
        }}
        string Validate(Order order,string id) {
            if(PurchasesBlockedByPreview)throw new InvalidOperationException("Purchases unavailable during preview.");
#if UNITY_EDITOR
            var receipt=JsonUtility.FromJson<StoreReceipt>(order.Info.Receipt);
            if(receipt==null||receipt.Store!="fake"||string.IsNullOrWhiteSpace(order.Info.TransactionID))throw new InvalidOperationException("Expected Editor Fake Store transaction.");
            return "editor-fake:"+id+":"+order.Info.TransactionID;
#else
            return StoreTransactionKey(order.Info.Receipt,id,Application.identifier);
#endif
        }
        string ProductId(Order order){var items=order.CartOrdered.Items();if(items.Count!=1||items[0].Quantity!=1)throw new InvalidOperationException("Unexpected purchase cart.");var id=items[0].Product.definition.id;if(WoolIapCatalog.Find(id)==null)throw new InvalidOperationException("Unknown product.");return id;}
        void Pending(PendingOrder order)
        {
            try{
                string id=ProductId(order),token=Validate(order,id);WoolProgress.DeliverPurchase(id,token);
                // Confirm only after the durable reward/transaction save succeeds.
                store.ConfirmPurchase(order);
            }catch(Exception e){busy=false;buying=null;Debug.LogWarning("Purchase left pending: "+e.GetType().Name);Tell("PURCHASE PENDING","Your purchase could not be verified yet. Reopen the game to retry. No unverified reward was granted.");}
        }
        void Confirmed(Order order){if(order is FailedOrder failed){Failed(failed);return;}bool notify=buying!=null;busy=false;buying=null;ApplyNoAds();if(notify){
#if UNITY_EDITOR
            Tell("TEST PURCHASE COMPLETE","Editor Fake Store: test items added. No real payment was made.");
#else
            Tell("PURCHASE COMPLETE","Your items have been added. Thank you!");
#endif
            }}
        void Failed(FailedOrder failure){busy=false;buying=null;Tell("PURCHASE NOT COMPLETED","The purchase was cancelled or could not be completed. No items were granted.");}
        void Deferred(DeferredOrder order){busy=false;buying=null;Tell("PURCHASE AWAITING APPROVAL","Your purchase is awaiting store approval. Items will arrive after approval.");}
        void PurchasesFetched(Orders orders)
        {
            bool ok=true;
            foreach(var order in orders.ConfirmedOrders)try{string id=ProductId(order);if(WoolIapCatalog.Find(id).purchaseOnce)WoolProgress.DeliverPurchase(id,Validate(order,id),true);}catch(Exception e){ok=false;Debug.LogWarning("Purchase restore: "+e.GetType().Name);}
            ApplyNoAds();if(restoring){restoring=busy=false;Tell(ok?"PURCHASES RESTORED":"RESTORE INCOMPLETE",ok?"Your owned purchases have been restored. Previously spent coins cannot be restored.":"Some purchases could not be verified. Please try again later.");}
        }
        void PurchasesFailed(PurchasesFetchFailureDescription failure){bool notify=restoring;restoring=busy=false;if(notify)Tell("RESTORE UNAVAILABLE","The store could not retrieve your purchases. Please try again.");}
        void ApplyNoAds(){if(WoolProgress.PermanentNoAds&&AdsMediation.AdsMediationManager.instance)AdsMediation.AdsMediationManager.instance.OnRemoveAds();}
        void Tell(string title,string body){Message?.Invoke(title,body);}
        void OnDestroy(){if(instance!=this)return;instance=null;if(store==null)return;store.OnStoreConnected-=Connected;store.OnStoreDisconnected-=Disconnected;store.OnProductsFetched-=ProductsFetched;store.OnProductsFetchFailed-=ProductsFailed;store.OnPurchasePending-=Pending;store.OnPurchaseConfirmed-=Confirmed;store.OnPurchaseFailed-=Failed;store.OnPurchaseDeferred-=Deferred;store.OnPurchasesFetched-=PurchasesFetched;store.OnPurchasesFetchFailed-=PurchasesFailed;store.OnAuthAccountChanged-=AccountChanged;}
    }
}
