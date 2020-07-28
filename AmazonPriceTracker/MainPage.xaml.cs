﻿using System;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

// -br- is the delimeter

namespace AmazonPriceTracker
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private ObservableCollection<AmazonItem> AmazonItems = new ObservableCollection<AmazonItem>();
        public  MainPage()
        {
            this.InitializeComponent();

            getAmazonItemsAsync();

            
        }

        private async void getAmazonItemsAsync()
        {
            try
            {
                /*
                StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
                StorageFile file = await folder.GetFileAsync("AmazonItems.txt");
                var text = await FileIO.ReadTextAsync(file);
                AmazonItems = (ObservableCollection<AmazonItem>)JsonConvert.DeserializeObject(text);
                */
                var helper = new LocalObjectStorageHelper();

                if (await helper.FileExistsAsync("AmazonItems"))
                {
                    AmazonItems = await helper.ReadFileAsync<ObservableCollection<AmazonItem>>("AmazonItems");
                }

            }
            catch(Exception e)
            {
                Debug.WriteLine(e.StackTrace);
                MessageDialog message = new MessageDialog(e.StackTrace);
                AmazonItems = new ObservableCollection<AmazonItem>();
                Console.WriteLine(e.StackTrace);
            }
        }

        private async void storeAmazonItemsAsync()
        {
            /*
             StorageFile sampleFile = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync("AmazonItems.txt", CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteTextAsync(sampleFile, JsonConvert.SerializeObject(AmazonItems));
            */

            var helper = new LocalObjectStorageHelper();

            await helper.SaveFileAsync("AmazonItems", AmazonItems);

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
                    _ = AppendItemDataAsync(item);
                    _ = StoreURLDataAsync(url, desired_price);
                    AmazonItems.Add(item);
                    storeAmazonItemsAsync();
                    foreach (var it in AmazonItems)
                    {
                        Debug.WriteLine(it.ToString());
                    }
                }
                catch(Exception er)
                {
                    //Show an error dialogue
                    Debug.WriteLine(er.StackTrace);
                    MessageDialog message = new MessageDialog("An error occured! Please try again.");
                    await message.ShowAsync();
                }
                finally
                {
                    progressRing.IsActive = false;
                    url_text_box.Text = "";
                    desired_price_text_box.Text = "";
                }
            }
            //Update the UI
            
        }

        private async System.Threading.Tasks.Task StoreURLDataAsync(String url, String price)
        {
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            /*
            if (storageFolder == null)
            {
                storageFolder = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync("Data");
            }
            */
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
                    if (priceTwo != null)
                    {
                        price = HttpUtility.HtmlDecode(priceTwo.InnerText).Trim();
                        price = price.Replace("₹ ", "");
                        price = price.Replace("$ ", "");
                        price = price.Replace(",", "");
                        status = "Available";
                    } else if (priceOne != null)
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
                
                }catch(Exception e)
                {
                    Debug.WriteLine(e.StackTrace);
                    MessageDialog message = new MessageDialog("Please check the URL and try again!");
                    await message.ShowAsync();
                    progressRing.IsActive = false;
                    return null;
                }
             }
        }

        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

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
    }
}