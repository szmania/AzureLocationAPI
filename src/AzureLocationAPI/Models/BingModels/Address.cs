using System;
using System.Collections.Generic;
using System.Text;

namespace AzureLocationAPI.Models.BingModels
{
   public class Address
    {
        public string AddressLine { get; set; }
        public string AdminDistrict { get; set; }
        public string CountryRegion { get; set; }
        public string Locality { get; set; }
        public string PostalCode { get; set; }
        public string FormattedAddress { get; set; }
    }
}
