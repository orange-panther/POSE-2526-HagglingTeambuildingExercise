using haggling_interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CustomerOffer : IOffer
{
    public OfferStatus Status { get; set; }
    public IProduct Product { get; set; }
    public decimal Price { get; set; }
    public PersonType OfferedBy { get; set; }
}
