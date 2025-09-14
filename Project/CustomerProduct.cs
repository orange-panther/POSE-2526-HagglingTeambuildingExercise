using haggling_interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class CustomerProduct : IProduct
{
    public string Name { get; init; }
    public ProductType Type { get; init; }
    public Percentage Rarity { get; set; }
}
