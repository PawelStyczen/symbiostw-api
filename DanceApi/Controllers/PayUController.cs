using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DanceApi.Interface;
using DanceApi.Model;
using Newtonsoft.Json;
using System;
using System.Text;

[Route("api/[controller]")]
[ApiController]
public class PayUController : ControllerBase
{
    private readonly IPayURepository _payURepository;
    private readonly IMeetingRepository _meetingRepository;
    private readonly IUserRepository _userRepository;

    public PayUController(IPayURepository payURepository, IMeetingRepository meetingRepository,
        IUserRepository userRepository)
    {
        _payURepository = payURepository;
        _meetingRepository = meetingRepository;
        _userRepository = userRepository;
    }

    [HttpGet("get-token")]
    public async Task<IActionResult> GetToken()
    {
        try
        {
            var token = await _payURepository.GetAccessTokenAsync();
            return Ok(new { access_token = token });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("create-order")]
    public async Task<IActionResult> CreateOrder([FromBody] PayUOrder order)
    {
        try
        {
            order.continueUrl ??= "http://localhost:3000/payment-success";
            var redirectUrl = await _payURepository.CreateOrderAsync(order);

            return Ok(new { url = redirectUrl });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Failed to create PayU order." });
        }
    }

[HttpPost("webhook")]
public async Task<IActionResult> PaymentWebhook()
{
    try
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var requestBody = await reader.ReadToEndAsync();


        Console.WriteLine(requestBody);

        var webhookData = JsonConvert.DeserializeObject<dynamic>(requestBody);
        if (webhookData == null || webhookData.order == null)
        {
    
            return BadRequest("Invalid webhook data.");
        }

   
        var order = webhookData.order;
        string status = order.status;
        string orderId = order.orderId;
        

        if (status == "COMPLETED")
        {
            string extOrderId = order.extOrderId.ToString();
            string[] parts = extOrderId.Split('-'); 
            int meetingId = int.Parse(parts[0]); 
            string email = order.buyer.email.ToString();

            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
        
                return NotFound($"User with email {email} not found.");
            }

            var success = await _meetingRepository.AddParticipantToMeetingAsync(meetingId, user.Id);
            if (success)
            {
                
                return Ok(new
                {
                    message = "User added to meeting successfully.",
                    redirectUrl = "http://localhost:3000/dashboard"
                });
            }
            else
            {
             
                return BadRequest("Failed to add participant to the meeting.");
            }
        }
        else
        {
          
            return BadRequest("Payment not completed.");
        }
    }
    catch (Exception ex)
    {
        
        return BadRequest("Webhook processing error.");
    }

}


}