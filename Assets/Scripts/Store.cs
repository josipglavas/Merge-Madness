using System.Linq;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Store : MonoBehaviour, IDetailedStoreListener {

    private IStoreController StoreController;
    private IExtensionProvider ExtensionProvider;

    [SerializeField] private GameObject loadingOverlay;
    [SerializeField] private GameObject removeAdsGameobject;
    [SerializeField] private Transform settingsScreen;

    private async void Start() {
        InitializationOptions options = new InitializationOptions()
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            .SetEnvironmentName("test");
#else
            .SetEnvironmentName("production");
#endif
        await UnityServices.InitializeAsync(options);
        ResourceRequest operation = Resources.LoadAsync<TextAsset>("IAPProductCatalog");
        operation.completed += HandleIAPCatalogLoaded;
    }

    private void HandleIAPCatalogLoaded(AsyncOperation operation) {

        ResourceRequest request = operation as ResourceRequest;
        Debug.Log($"Asset: {request.asset}");
        ProductCatalog catalog = JsonUtility.FromJson<ProductCatalog>((request.asset as TextAsset).text);
        Debug.Log($"Loaded catalog with: {catalog.allProducts.Count} items");

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        StandardPurchasingModule.Instance().useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
        StandardPurchasingModule.Instance().useFakeStoreAlways = true;
#endif

#if UNITY_IOS
        ConfigurationBuilder builder = ConfigurationBuilder.Instance(
            StandardPurchasingModule.Instance(AppStore.AppleAppStore)
            );
#elif UNITY_ANDROID
 ConfigurationBuilder builder = ConfigurationBuilder.Instance(
            StandardPurchasingModule.Instance(AppStore.GooglePlay)
            );
#else
ConfigurationBuilder builder = ConfigurationBuilder.Instance(
           StandardPurchasingModule.Instance(AppStore.NotSpecified)
           );
#endif

        foreach (ProductCatalogItem item in catalog.allProducts) {
            builder.AddProduct(item.id, item.type);
        }

        UnityPurchasing.Initialize(this, builder);
    }


    private void HandlePurchase(Product Product) {
        loadingOverlay.SetActive(true);
        StoreController.InitiatePurchase(Product);
    }
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions) {
        StoreController = controller;
        ExtensionProvider = extensions;
        List<Product> sortedProducts = StoreController.products.all.ToList();
        foreach (Product product in sortedProducts) {
            if (FileManager.Instance.AdsRemoved)
                return;
            GameObject button = Instantiate(removeAdsGameobject, settingsScreen);
            button.GetComponent<Button>().onClick.AddListener(() => {
                HandlePurchase(product);
            });
            button.GetComponentInChildren<TextMeshProUGUI>().text = ($"{product.metadata.localizedPriceString} " +
            $"{product.metadata.isoCurrencyCode}");
        }

    }

    public void OnInitializeFailed(InitializationFailureReason error) {
        Debug.Log($"Store initialization failed: {error}");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message) {
        throw new System.NotImplementedException();
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription) {
        Debug.Log($"Failed to purchase {product.definition.id} because {failureDescription}");
        loadingOverlay.SetActive(false);
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason) {
        throw new System.NotImplementedException();
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent) {
        FileManager.Instance.SetRemoveAds(true);
        Debug.Log("purchase complete!");
        loadingOverlay.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name.ToString());
        return PurchaseProcessingResult.Complete;
    }

}
