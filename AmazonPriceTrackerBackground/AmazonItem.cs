using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazonPriceTrackerBackground
{
    public sealed class AmazonItem
    {
        public String productTitle { get; set; }
        public double productPrice { get; set; }
        public String productStatus { get; set; }
        public String productURL { get; set; }
        public double desiredPrice { get; set; }

        public double previousPrice { get; set; }

        public AmazonItem(String productTitle, String productPrice, double expectedPrice, String productStatus, String productURL)
        {
            this.productTitle = productTitle;
            this.productStatus = productStatus;
            this.productURL = productURL;
            this.desiredPrice = expectedPrice;
           

            productPrice = productPrice.Replace(",", "").Replace(" ", "");
            this.productPrice = double.Parse(productPrice);
            this.previousPrice = 0;
        }

        public String getStringPrice()
        {
            return this.productPrice.ToString();
        }

        override
        public sealed String ToString()
        {
            return productTitle + " -br- " + productStatus + " -br- " + productPrice + " -br- " + desiredPrice + " -br- " + productURL;
        }

        
    }
}
