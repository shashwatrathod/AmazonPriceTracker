using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Toolkit.Uwp;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Net.Http;
using HtmlAgilityPack;
using System.Web;
using System.Diagnostics;
using Windows.Storage;
using Json;
using Newtonsoft.Json;
using System.Security.Cryptography;
using Windows.UI.Popups;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.Toolkit.Uwp.Helpers;
using System.Threading.Tasks;
using AmazonPriceTrackerBackground;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;

// -br- is the delimeter

namespace AmazonPriceTracker
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private ObservableCollection<AmazonItem> AmazonItems = new ObservableCollection<AmazonItem>();

        private Library lib = new Library();
        public MainPage()
        {
            this.InitializeComponent();
            
            lib.Init();
            lib.Toggle();
        }

        private async Task<object> getAmazonItemsAsync()
        {
            try
            {
                var helper = new LocalObjectStorageHelper();

                if (await helper.FileExistsAsync("AmazonItems"))
                {
                    AmazonItems = await helper.ReadFileAsync<ObservableCollection<AmazonItem>>("AmazonItems");
                }

                dataGrid.ItemsSource = AmazonItems;
                return null;
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.StackTrace);
                AmazonItems = new ObservableCollection<AmazonItem>();
                Console.WriteLine(e.StackTrace);
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

        private async void add_button_Click(object sender, RoutedEventArgs e)
        {
            progressRing.IsActive = true;
            String url = url_text_box.Text;
            String desired_price = desired_price_text_box.Text;

            List<String> itemProperties = await getAmazonItemFromURLAsync(url);

            
            if(itemProperties != null)
            {
                try
                {
                    AmazonItem item = new AmazonItem(itemProperties[0], itemProperties[1], double.Parse(desired_price.Trim()), itemProperties[2], itemProperties[3]);
                    if (item.productPrice <= item.desiredPrice)
                    {
                        createWindowsToast(item);
                    }
                    _ = AppendItemDataAsync(item);
                    _ = StoreURLDataAsync(url, desired_price);
                    AmazonItems.Add(item);
                    await storeAmazonItemsAsync();
                    await lib.Toggle();
                }
                catch(Exception er)
                {
                    Debug.WriteLine(er.StackTrace);
                    MessageDialog message = new MessageDialog("An error occured! Please try again.");
                    message.Title = "Error";
                    await message.ShowAsync();
                }
                finally
                {
                    progressRing.IsActive = false;
                    url_text_box.Text = "";
                    desired_price_text_box.Text = "";
                }
            }
            
        }

        private async System.Threading.Tasks.Task StoreURLDataAsync(String url, String price)
        {
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile file = await storageFolder.CreateFileAsync("urls.txt", Windows.Storage.CreationCollisionOption.OpenIfExists);

            await Windows.Storage.FileIO.AppendTextAsync(file, url + " -br- " + price);
        }

        private async System.Threading.Tasks.Task AppendItemDataAsync(AmazonItem item)
        {
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile file = await storageFolder.CreateFileAsync("items.txt", Windows.Storage.CreationCollisionOption.OpenIfExists);
            string[] line = { item.ToString() };
            await Windows.Storage.FileIO.AppendLinesAsync(file,line);
        }

        private async System.Threading.Tasks.Task<List<String>> getAmazonItemFromURLAsync(String url)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                try {
                    HttpResponseMessage responseMessage = await httpClient.GetAsync(url);
                    HttpContent content = responseMessage.Content;

                    HtmlDocument htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(await content.ReadAsStringAsync());

                    var title = htmlDocument.DocumentNode.SelectSingleNode("//span[@id='productTitle']");
                    var priceOne = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='priceBlockStrikePriceString a - text - strike']");
                    var priceTwo = htmlDocument.DocumentNode.SelectSingleNode("//span[@id = 'priceblock_ourprice']");
                    var priceThree = htmlDocument.DocumentNode.SelectSingleNode("//span[@id = 'priceblock_dealprice']");
                    var priceFour= htmlDocument.DocumentNode.SelectSingleNode("//span[@id = 'priceblock_saleprice']");

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
                        Debug.WriteLine("Price3");
                        price = price.Replace("₹ ", "");
                        price = price.Replace("$ ", "");
                        price = price.Replace(",", "");
                        status = "Available";
                    }
                    else if (priceFour != null)
                    {
                        Debug.WriteLine("Price4");
                        price = HttpUtility.HtmlDecode(priceFour.InnerText).Trim();
                        price = price.Replace("₹ ", "");
                        price = price.Replace("$ ", "");
                        price = price.Replace(",", "");
                        status = "Available";
                    }
                    else if (priceTwo != null)
                    {
                        Debug.WriteLine("Price2");
                        price = HttpUtility.HtmlDecode(priceTwo.InnerText).Trim();
                        price = price.Replace("₹ ", "");
                        price = price.Replace("$ ", "");
                        price = price.Replace(",", "");
                        status = "Available";
                    } 
                    else if (priceOne != null)
                    {
                        Debug.WriteLine("Price1");
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
                
                }catch(Exception e)
                {
                    Debug.WriteLine(e.StackTrace);
                    MessageDialog message = new MessageDialog("Please check the URL and try again!");
                    message.Title = "Error";
                    await message.ShowAsync();
                    progressRing.IsActive = false;
                    return null;
                }
             }
        }

        private async void VisitContextMenuClick(object sender, RoutedEventArgs e)
        {
            var b = sender as FrameworkElement;
            AmazonItem item = b.DataContext as AmazonItem;
            var uri = new Uri(@""+item.productURL);

            var success = await Windows.System.Launcher.LaunchUriAsync(uri);

            if (success)
            {
                Debug.WriteLine("URI: Success");
            }
            else
            {
                Debug.WriteLine("URI: failed");
            }
        }

        private void CopyURLContextMenuClick(object sender, RoutedEventArgs e)
        {
            var b = sender as FrameworkElement;
            AmazonItem item = b.DataContext as AmazonItem;

            DataPackage dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText(item.productURL);
            Clipboard.SetContent(dataPackage);
        }

        private void RemoveContextMenuClick(object sender, RoutedEventArgs e)
        {
            var b = sender as FrameworkElement;
            AmazonItem item = b.DataContext as AmazonItem;
            AmazonItems.Remove(item);
            storeAmazonItemsAsync();
        }

        private async void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            progressRing.IsActive = true;
           await getAmazonItemsAsync();
           await updateAmazonItemsList();
            progressRing.IsActive = false;
        }

        private async Task<object> updateAmazonItemsList()
        {
            ObservableCollection<AmazonItem> dummList = new ObservableCollection<AmazonItem>();

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
                dataGrid.ItemsSource = AmazonItems;
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

        private async void refresh_button_click(object sender, RoutedEventArgs e)
        {
            progressRing.IsActive = true;           
            _ = await updateAmazonItemsList();
            progressRing.IsActive = false;
        }
    }
}

