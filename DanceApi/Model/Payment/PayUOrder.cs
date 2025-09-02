namespace DanceApi.Model;

using System.Collections.Generic;

public class PayUOrder
{
    public string notifyUrl { get; set; }
    public string customerIp { get; set; } = "127.0.0.1";
    public string merchantPosId { get; set; }
    public string description { get; set; }
    public string currencyCode { get; set; } = "PLN";
    public string totalAmount { get; set; }  
    public string extOrderId { get; set; }   
    public string continueUrl { get; set; }  
    public PayUBuyer buyer { get; set; }
    public List<PayUProduct> products { get; set; }
}

public class PayUBuyer
{
    public string email { get; set; }
    public string phone { get; set; } 
    public string firstName { get; set; }
    public string lastName { get; set; }
    public string language { get; set; } = "pl";  
}

public class PayUProduct
{
    public string name { get; set; }
    public string unitPrice { get; set; } 
    public int quantity { get; set; }
}