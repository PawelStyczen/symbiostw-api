using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using DanceApi.Interface;
using DanceApi.Model;
using Exception = System.Exception;

public class PayURepository : IPayURepository
{
    private readonly HttpClient _httpClient;
    private readonly string _authUrl;
    private readonly string _apiUrl;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _posId;

    public PayURepository(IOptions<PayUSettings> payUSettings)
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = false 
        };

        _httpClient = new HttpClient(handler);
        _authUrl = payUSettings.Value.AuthUrl;
        _apiUrl = payUSettings.Value.ApiUrl;
        _clientId = payUSettings.Value.ClientId;
        _clientSecret = payUSettings.Value.ClientSecret;
        _posId = payUSettings.Value.PosId;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        var authRequest = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", _clientId },
            { "client_secret", _clientSecret }
        };

        var requestContent = new FormUrlEncodedContent(authRequest);
        var response = await _httpClient.PostAsync(_authUrl, requestContent);
    
        var responseString = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"🔑 Token Response: {responseString}");

        response.EnsureSuccessStatusCode();

        var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseString);
        return jsonResponse.access_token;
    }

    public async Task<string> CreateOrderAsync(PayUOrder order)
    {
        var token = await GetAccessTokenAsync();

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        _httpClient.DefaultRequestHeaders.Add("X-OpenPayu-Sandbox", "true");
        _httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
        _httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

        order.merchantPosId = _posId;
        order.continueUrl ??= "http://localhost:3000/payment-success";
        order.extOrderId = $"{order.extOrderId}-{Guid.NewGuid().ToString().Substring(0, 8)}";

        order.totalAmount = order.totalAmount.ToString();
        foreach (var product in order.products)
        {
            product.unitPrice = product.unitPrice.ToString();
        }

        var orderRequest = JsonConvert.SerializeObject(order);
        var content = new StringContent(orderRequest, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(_apiUrl, content);
        var responseString = await response.Content.ReadAsStringAsync();
        

        var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseString);

        
        return jsonResponse.redirectUri;
    }
}