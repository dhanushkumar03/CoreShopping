using Core.Models;
using Core.Models.ProductModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Core.Controllers
{
    [Authorize(Policy = "adminpolicy")]
    public class AdminController : Controller
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        public AdminController(IWebHostEnvironment hostEnvironment)
        {
            webHostEnvironment = hostEnvironment;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7246/");
                var getTask = await client.GetAsync("api/Admin/index");
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
        public IActionResult Create()
        {
            return View(new ProductViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductViewModel product)
        {
            if (product != null)
            {
                using (var client = new HttpClient())
                {
                    string path = ProcessUploadedFile(product);
                    Product p = new Product
                    {
                        Name = product.Name,
                        Price = product.Price,
                        Quantity = product.Quantity,
                        Description = product.Description,
                        ProductImage = path
                    };
                    client.BaseAddress = new Uri("https://localhost:7246/");
                    var postTask = await client.PostAsJsonAsync<Product>("api/Admin/create", p);
                    if (postTask.IsSuccessStatusCode)
                    {
                        var userjsonString = postTask.Content.ReadAsStringAsync();
                        var de = Newtonsoft.Json.JsonConvert.DeserializeObject<ValidationModel>(userjsonString.Result);

                        return RedirectToAction("Index", "Admin");
                    }
                    ModelState.AddModelError("", "Invalid Details");
                }
            }
            return View(product);
        }

        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            Product p = await GetById(id);
            ProductViewModel product = new ProductViewModel
            {
                Id = p.Id,
                Name= p.Name,
                Price= p.Price,
                Quantity= p.Quantity,
                Description= p.Description,
                ExistingImage = p.ProductImage
            };
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ProductViewModel model)
        {
            if(model != null)
            {
                using(var client = new HttpClient())
                {
                    int id = model.Id;
                    if (model.Image != null)
                    {
                        string path = ProcessUploadedFile(model);
                        model.ExistingImage = path;
                        
                    }
                    Product product = new Product
                    {
                        Name = model.Name,
                        Price = model.Price,
                        Quantity= model.Quantity,
                        Description= model.Description,
                        ProductImage = model.ExistingImage
                    };
                    client.BaseAddress = new Uri("https://localhost:7246/");
                    var putTask = await client.PutAsJsonAsync("api/Admin/edit?id=" + id, product);
                    if (putTask.IsSuccessStatusCode)
                    {
                        var userjsonString = putTask.Content.ReadAsStringAsync();
                        var de = Newtonsoft.Json.JsonConvert.DeserializeObject<Product>(userjsonString.Result);
                        return RedirectToAction("Index", "Admin");
                    }
                    ModelState.AddModelError("", "Invalid Details");
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
                Product p = await GetById(id);
                return View(p);
        }

        [HttpGet]
        public async Task<ActionResult> Delete(int id)
        {
            Product p = await GetById(id);
            return View(p);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            using(var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7246/");
                var deleteTask = await client.DeleteAsync("api/Admin/delete?id=" + id);
                if (deleteTask.IsSuccessStatusCode)
                {
                    var userjsonString = deleteTask.Content.ReadAsStringAsync();
                    var de = Newtonsoft.Json.JsonConvert.DeserializeObject<Product>(userjsonString.Result);
                    return RedirectToAction("Index", "Admin");
                }
            }
            return RedirectToAction("Index", "Admin");
        }

        [HttpGet]
        public async Task<Product> GetById(int id)
        {
            Product reqProduct = new Product();
            using(var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7246/");
                var getTask = await client.GetAsync("api/Admin/getbyid?id=" + id);
                if(getTask.IsSuccessStatusCode)
                {
                    var userjsonString = getTask.Content.ReadAsStringAsync();
                    var de = Newtonsoft.Json.JsonConvert.DeserializeObject<Product>(userjsonString.Result);
                    reqProduct = de;
                }
            }
            return reqProduct;
        }
        private string ProcessUploadedFile(ProductViewModel model)
        {
            string uniqueFileName = null;

            if (model.Image != null)
            {
                string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "Uploads");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Image.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.Image.CopyTo(fileStream);
                }
            }
            string path = "/Uploads/" + uniqueFileName;
            return path;
        }
    }
}
