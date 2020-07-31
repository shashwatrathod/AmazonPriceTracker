# Amazon Price Tracker

This is a GUI Universal Windows Platform(UWP) application which uses **HtmlAgilityPack** to keep track of the prices of given amazon items. 


# How to install?

1. Download the latest version from [here](https://drive.google.com/drive/folders/16asGShhYpCkbYWIAfX3qNv7jKwlpc6my?usp=sharing).
2.  Install via .appx or by running the "Install" script using Powershell.

You're good to go!
## Instructions

1. Copy the URL of the amazon item you so desperately want.
2. Paste the URL in the textbox.
![URL box](https://i.imgur.com/SQiqeF4.png)
3. Enter the desired price of the item. The application will notify you if the price of the product falls to or below your desired price.
![desired price box](https://imgur.com/oWKrpAQ.png)
4. Hit "ADD" !
The application runs in background. So even if you close the UI, the application will refresh the item list every 15 minutes and notify you if there is a favourable change in price! 
(You can change this behaviour by disabling "Background Apps" in App Settings of windows.)
Other than that, you can hit refresh to force the app to refresh the list at anytime.

## Features

 - You just have to paste the URL once, the app will take care of the rest.
 - You can mention the price at which you want to buy the item at.
 - Get notifications when the price of a desired item falls below or to the desired price.
 - Right click on any list item to Visit link, copy link, or to remove item from the list.
 - GUI instead of CLI !


I am also planning to include an option where the application starts at startup so you don't have to manually fire up the application, so stay tuned for that!
 
