namespace DanceApi.Dto;

public class CreatePaymentRequest
{
    public int MeetingId { get; set; }
    public string BuyerEmail { get; set; }
}