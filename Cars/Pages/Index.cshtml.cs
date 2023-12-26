using Cars.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.JSInterop;
using System.Text;
using System.Text.Json;

namespace Cars.Pages
{
    public class IndexModel : PageModel
    {

        private readonly IConfiguration _configuration;
        [BindProperty]
        public Cars.Models.User User { get; set; }

        [BindProperty]
        public Cars.Models.Login login { get; set; }
        public IndexModel (IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet()
        {

        }
        public async Task<IActionResult> OnPostLogin(Login login)
        {
            try
            {
                var apiUrl = $"{_configuration["App:SelfUrl"]}/api/LogIn";
                using var httpClient = new HttpClient();

                // Serialize the user object to JSON and send it in the request body
                var json = JsonSerializer.Serialize(login);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    using JsonDocument document = JsonDocument.Parse(jsonString);

                    // Navigate to the "value" property and extract the "token" property
                    JsonElement root = document.RootElement;
                    string token = root.GetProperty("value").GetProperty("token").GetString();
                    TempData["UserToken"] = token;
                    //Response.Cookies.Append("Usertoken", token, new CookieOptions
                    //{
                    //    Expires = DateTimeOffset.Now.AddDays(1), // Set cookie expiration
                    //    Secure = true, // Make the cookie secure (HTTPS only)
                    //    HttpOnly = true, // Prevent client-side script access
                    //    SameSite = SameSiteMode.None // Adjust SameSite attribute as needed
                    //});
                    return RedirectToPage("/Car/Index");
                }
                else
                {

                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return RedirectToPage("/Index");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
                return RedirectToPage("/Index");
            }
        }

        public async Task<IActionResult> OnPostRegister(Cars.Models.User user)
        {
            try
            {
                var apiUrl = $"{_configuration["App:SelfUrl"]}/api/User/register";
                using var httpClient = new HttpClient();

                // Serialize the user object to JSON and send it in the request body
                var response = await httpClient.PostAsJsonAsync(apiUrl, user);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                }
                else
                {

                    var errorMessage = await response.Content.ReadAsStringAsync();
                }
                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
                return RedirectToPage("/Index");
            }

            // Redirect to home page or any other page after registration
        }
    }
}
