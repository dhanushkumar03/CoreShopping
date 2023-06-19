using Core.Models.ProductModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Core.Controllers
{
    [Authorize(Policy ="userpolicy")]
    public class CustomerController : Controller
    {
        private Uri url = new Uri("https://localhost:7246/");
        public async Task<IActionResult> Index()
        {
            using(var client = new HttpClient())
            {
                client.BaseAddress = url;
                var getTask = await client.GetAsync("api/Customer/index?sortOrder=&currentFilter=&searchString=&page=");
                if (getTask.IsSuccessStatusCode)
                {
                    var userjsonString = getTask.Content.ReadAsStringAsync();
                    var de = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<Product>>(userjsonString.Result);
                    return View(de);
                }
            }
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> AddToCart(int id)
        {
            using (var client = new HttpClient())
            {
                var di = HttpContext.Session.GetString("userid");
                Product product = await GetById(id);
                client.BaseAddress = url;
                var postTask = await client.PostAsJsonAsync("api/Customer/addtocart?id=" + di, product);
                if(postTask.IsSuccessStatusCode)
                {
                    var userjsonString = postTask.Content.ReadAsStringAsync();
                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            List<CartReference> list = new List<CartReference>();
            using (var client = new HttpClient())
            {
                var id = HttpContext.Session.GetString("userid");
                client.BaseAddress = url;
                var getTask = await client.GetAsync("api/Customer/cart?id=" + id);
                if(getTask.IsSuccessStatusCode)
                {
                    var userjsonString = getTask.Content.ReadAsStringAsync();
                    var de = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<CartReference>>(userjsonString.Result);
                    list.AddRange(de);
                    int totalp = 0;
                    int totalq = 0;
                    foreach (var item in list)
                    {
                        totalq = totalq + item.ProductQuantity;
                        totalp = totalp + (item.ProductPrice * item.ProductQuantity);
                    }
                    ViewBag.TotalPrice = totalp;
                    ViewBag.TotalQuantity = totalq;
                }
            }
            return View(list);
        }

        public async Task<IActionResult> Plus(int pId, string cId)
        {
            using(var client = new HttpClient())
            {
                client.BaseAddress=url;
                var getTask = await client.GetAsync("api/Customer/quantchange?pId=" + pId + "&cId=" + cId + "&op=" + 1);
            }
            return RedirectToAction("Cart");
        }

        public async Task<IActionResult> Minus(int pId, string cId)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = url;
                var getTask = await client.GetAsync("api/Customer/quantchange?pId=" + pId + "&cId=" + cId + "&op=" + 0);
            }
            return RedirectToAction("Cart");
        }

        public async Task<IActionResult> CheckOut()
        {
            using( var client = new HttpClient())
            {
                var id = HttpContext.Session.GetString("userid");
                client.BaseAddress = url;
                var getTask = await client.GetAsync("api/Customer/checkout?id=" + id);
                if (getTask.IsSuccessStatusCode)
                {
                    return RedirectToAction("Cart");
                }
            }
            return RedirectToAction("Cart");
        }

        [HttpGet]
        public async Task<Product> GetById(int id)
        {
            Product reqProduct = new Product();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7246/");
                var getTask = await client.GetAsync("api/Customer/getbyid?id=" + id);
                if (getTask.IsSuccessStatusCode)
                {
                    var userjsonString = getTask.Content.ReadAsStringAsync();
                    var de = Newtonsoft.Json.JsonConvert.DeserializeObject<Product>(userjsonString.Result);
                    reqProduct = de;
                }
            }
            return reqProduct;
        }
    }
}
