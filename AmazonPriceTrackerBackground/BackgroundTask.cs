using HtmlAgilityPack;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications; // Notifications library
using System.Diagnostics;

namespace AmazonPriceTrackerBackground 
{

    public sealed class BackgroundTask : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral;
        private ObservableCollection<AmazonItem> AmazonItems;
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();

            try
            {
                _ = await updateAmazonItemsList();
                foreach (AmazonItem item in AmazonItems)
                {
                    if (item.productPrice <= item.desiredPrice)
                    {
                        createWindowsToast(item);
                    }
                }

                Debug.WriteLine("Executed BG Task");
            }
            catch(Exception e) 
            {
                Debug.WriteLine(e.StackTrace);
            }

            _deferral.Complete();
        }

        private async Task<object> getAmazonItemsAsync()
        {
            try
            {
                var helper = new LocalObjectStorageHelper();

                if (await helper.FileExistsAsync("AmazonItems"))
                {
                    AmazonItems = await helper.ReadFileAsync<ObservableCollection<AmazonItem>>("AmazonItems");
                    return null;
                }

                return null;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private async Task<object> storeAmazonItemsAsync()
        {
            /*
             StorageFile sampleFile = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync("AmazonItems.txt", CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteTextAsync(sampleFile, JsonConvert.SerializeObject(AmazonItems));
            */

            var helper = new LocalObjectStorageHelper();

            await helper.SaveFileAsync("AmazonItems", AmazonItems);
            return null;

        }

        private async System.Threading.Tasks.Task<List<String>> getAmazonItemFromURLAsync(String url)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    HttpResponseMessage responseMessage = await httpClient.GetAsync(url);
                    HttpContent content = responseMessage.Content;

                    HtmlDocument htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(await content.ReadAsStringAsync());

                    var title = htmlDocument.DocumentNode.SelectSingleNode("//span[@id='productTitle']");
                    var priceOne = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='priceBlockStrikePriceString a - text - strike']");
                    var priceTwo = htmlDocument.DocumentNode.SelectSingleNode("//span[@id = 'priceblock_ourprice']");
                    var priceThree = htmlDocument.DocumentNode.SelectSingleNode("//span[@id = 'priceblock_dealprice']");
                    var priceFour = htmlDocument.DocumentNode.SelectSingleNode("//span[@id = 'priceblock_saleprice']");

                    String price;
                    String stringTitle;
                    String status;

                    if (title != null)
                    {
                        stringTitle = HttpUtility.HtmlDecode(title.InnerText).Trim();
                    }
                    else
                    {
                        return null;
                    }

                    if (priceThree != null)
                    {
                        price = HttpUtility.HtmlDecode(priceThree.InnerText).Trim();
                        price = price.Replace("₹ ", "");
                        price = price.Replace("$ ", "");
                        price = price.Replace(",", "");
                        status = "Available";
                    }
                    else if (priceFour != null)
                    {
                        price = HttpUtility.HtmlDecode(priceFour.InnerText).Trim();
                        price = price.Replace("₹ ", "");
                        price = price.Replace("$ ", "");
                        price = price.Replace(",", "");
                        status = "Available";
                    }
                    else if (priceTwo != null)
                    {
                        price = HttpUtility.HtmlDecode(priceTwo.InnerText).Trim();
                        price = price.Replace("₹ ", "");
                        price = price.Replace("$ ", "");
                        price = price.Replace(",", "");
                        status = "Available";
                    }
                    else if (priceOne != null)
                    {
                        price = HttpUtility.HtmlDecode(priceOne.InnerText).Trim();
                        price = price.Replace("₹ ", "");
                        price = price.Replace("$ ", "");
                        price = price.Replace(",", "");
                        status = "Available";
                    }
                    else
                    {
                        price = "0";
                        status = "Unavailable";
                    }
                    List<String> itemProperties = new List<string>();
                    itemProperties.Add(stringTitle);
                    itemProperties.Add(price);
                    itemProperties.Add(status);
                    itemProperties.Add(url);
                    return itemProperties;

                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        private async Task<object> updateAmazonItemsList()
        {
            ObservableCollection<AmazonItem> dummList = new ObservableCollection<AmazonItem>();

            _ = await getAmazonItemsAsync();
            if (AmazonItems != null)
            {
                foreach (AmazonItem item in AmazonItems)
                {
                    List<String> itemProperties = await getAmazonItemFromURLAsync(item.productURL);
                    AmazonItem newItem;
                    if (itemProperties != null)
                    {
                        newItem = new AmazonItem(itemProperties[0], itemProperties[1], item.desiredPrice, itemProperties[2], itemProperties[3]);
                        dummList.Add(newItem);
                    }
                }

                AmazonItems = dummList;
                await storeAmazonItemsAsync();
            }
            return null;
        }



        public void createWindowsToast(AmazonItem item)
        {
            ToastContent content = new ToastContent()
            {
                Launch = item.productURL,
                ActivationType = ToastActivationType.Protocol,
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = item.productTitle
                            },

                            new AdaptiveText()
                            {
                                Text = "The item is available at " + item.productPrice + ", at or lower than the desired price " + item.desiredPrice + "."
                            }
                        },

                        Attribution = new ToastGenericAttributionText()
                        {
                            Text = "Via Amazon Price Tracker"
                        },
                    }
                },
            };

            var toast = new ToastNotification(content.GetXml());
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

    }
}
