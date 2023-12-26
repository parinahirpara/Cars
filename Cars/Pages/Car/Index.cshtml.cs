using Cars.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Text;

namespace Cars.Pages.Car
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;
        [BindProperty]
        public List<Cars.Models.Car> Cars { get; set; } = new List<Cars.Models.Car>();

        [BindProperty]
        public string Token { get; private set; }
        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
       
        public async Task<IActionResult> OnGet()
        {
            try
            {
                if (!string.IsNullOrEmpty(Token))
                {
                    string apiUrl = $"{_configuration["App:SelfUrl"]}/api/user/current";
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

                        HttpResponseMessage response = await client.GetAsync(apiUrl);

                        if (response.IsSuccessStatusCode)
                        {
                            string jsonString = await response.Content.ReadAsStringAsync();

                            var user = JsonConvert.DeserializeObject<User>(jsonString);

                            if (user == null || user.UserId == 0)
                            {
                                return RedirectToPage("/Error");
                            }

                            await LoadCars(user.UserId);

                            return Page();
                        }
                        else
                        {
                            return RedirectToPage("/Error");
                        }
                    }
                }
                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return RedirectToPage("/Error");
            }
           
        }
        public async Task<IActionResult> OnPostDelete(int id)
        {
           // await DeleteCar(id);
            return RedirectToPage();
        }
        public async Task<IActionResult> OnPostAddCar([Bind("Brand,Model,Year,Price,New")] Cars.Models.Car car)
        {
            if (ModelState.IsValid)
            {
                var apiUrl = $"{_configuration["App:SelfUrl"]}/api/Cars";
                using var httpClient = new HttpClient();

                var content = new StringContent(JsonConvert.SerializeObject(car), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage();
                }
                else
                {
                    // Handle API error response
                    ModelState.AddModelError(string.Empty, "Failed to add a new car.");
                }
            }

            // Return to the page if there are model errors
            return RedirectToPage();
        }
        public async Task<IActionResult> OnPostEditCar(int id, [Bind("Id,Brand,Model,Year,Price,New")] Cars.Models.Car car)
        {
            if (ModelState.IsValid)
            {
                var apiUrl = $"{_configuration["App:SelfUrl"]}/api/Cars";
                using var httpClient = new HttpClient();
                var content = new StringContent(JsonConvert.SerializeObject(car), Encoding.UTF8, "application/json");
                var response = await httpClient.PutAsync($"{apiUrl}/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage();
                }
                else
                {
                    // Handle API error response
                    ModelState.AddModelError(string.Empty, "Failed to update the car details.");
                }
            }

            // Return to the page if there are model errors
            return RedirectToPage();
        }
        private async Task LoadCars(int userId)
        {
            var apiUrl = $"{_configuration["App:SelfUrl"]}/api/Cars/ByUserId/{userId}";
            using var httpClient = new HttpClient();

            // Serialize the user object to JSON and send it in the request body
            var response = await httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Cars = JsonConvert.DeserializeObject<List<Cars.Models.Car>>(content);
            }
        }
        private async Task DeleteCar(int id,int userId)
        {
            var apiUrl = $"{_configuration["App:SelfUrl"]}/api/Cars";
            using var httpClient = new HttpClient();
            var response = await httpClient.DeleteAsync($"{apiUrl}/{id}?userId={userId}");
            response.EnsureSuccessStatusCode();
        }

    }
}
