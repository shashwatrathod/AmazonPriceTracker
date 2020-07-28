using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazonPriceTracker
{
    class AmazonItem
    {
        public String productTitle { get; set; }
        public double productPrice { get; set; }
        public String productStatus { get; set; }
        public String productURL { get; set; }
        public double desiredPrice { get; set; }

        public AmazonItem(String productTitle, String productPrice, double expectedPrice, String productStatus, String productURL)
        {
            this.productTitle = productTitle;
            this.productStatus = productStatus;
            this.productURL = productURL;
            this.desiredPrice = expectedPrice;

            productPrice = productPrice.Replace(",", "").Replace(" ", "");
            this.productPrice = double.Parse(productPrice);
        }

        public String getStringPrice()
        {
            return this.productPrice.ToString();
        }

        override
        public String ToString()
        {
            return productTitle + " -br- " + productStatus + " -br- " + productPrice + " -br- " + productURL;
        }

        public List<String> getItemProperties()
        {
            List<String> itemList = new List<string>();
            itemList.Append(productTitle);
            itemList.Append(productStatus);
            itemList.Append(productPrice.ToString());
            itemList.Append(productURL);
            return itemList;
        }
    }
}
