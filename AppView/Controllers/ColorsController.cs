﻿using AppData.IRepositories;
using AppData.Models;
using AppData.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;


namespace AppView.Controllers
{
    public class ColorsController : Controller
    {
        private readonly IAllRepositories<Color> _repos;
        private ShopDBContext _dbContext = new ShopDBContext();
        private DbSet<Color> _color;
        public ColorsController()
        {
            _color = _dbContext.Colors;
            AllRepositories<Color> all = new AllRepositories<Color>(_dbContext, _color);
            _repos = all;
        }

        private bool CheckUserRole()
        {
            var CustomerRole = HttpContext.Session.GetString("UserId");
            var EmployeeNameSession = HttpContext.Session.GetString("RoleName");
            var EmployeeName = EmployeeNameSession != null ? EmployeeNameSession.Replace("\"", "") : null;
            if (CustomerRole != null || EmployeeName != "Quản lý")
            {
                return false;
            }
            return true;
        }
        private string GenerateColorCode()
        {
            var lastColor = _dbContext.Colors.OrderByDescending(c => c.ColorCode).FirstOrDefault();
            if (lastColor != null)
            {
                var lastNumber = int.Parse(lastColor.ColorCode.Substring(2)); // Lấy phần số cuối cùng từ ColorCode
                var nextNumber = lastNumber + 1; // Tăng giá trị cuối cùng
                var newColorCode = "CL" + nextNumber.ToString("D3"); // Tạo ColorCode mới
                return newColorCode;
            }
            return "CL001"; // Trường hợp không có ColorCode trong cơ sở dữ liệu, trả về giá trị mặc định "CL001"
        }
        public async Task<IActionResult> GetAllColor()
        {
            if (CheckUserRole() == false)
            {
                return RedirectToAction("Forbidden", "Home");
            }
            string apiUrl = "https://localhost:7036/api/Color/get-color";
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(apiUrl);
            string apiData = await response.Content.ReadAsStringAsync();
            // Lấy kqua trả về từ API
            // Đọc từ string Json vừa thu được sang List<T>
            var color = JsonConvert.DeserializeObject<List<Color>>(apiData);
            return View(color);
        }
        [HttpGet]
        public async Task<IActionResult> CreateColor()
        {
            if (CheckUserRole() == false)
            {
                return RedirectToAction("Forbidden", "Home");
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateColor(Color color)
        {
            var httpClient = new HttpClient();
            string apiUrl = $"https://localhost:7036/api/Color/create-color?ColorCode={GenerateColorCode()}&Name={color.Name}&Status={color.Status}&DateCreated={color.DateCreated.ToString("yyyy-MM-ddTHH:mm:ss")}";
            var response = await httpClient.PostAsync(apiUrl, null);
            return RedirectToAction("GetAllColor");
        }


        [HttpGet]
        public async Task<IActionResult> EditColor(Guid id) // Khi ấn vào Create thì hiển thị View
        {
            if (CheckUserRole() == false)
            {
                return RedirectToAction("Forbidden", "Home");
            }
            // Lấy Product từ database dựa theo id truyền vào từ route
            Color color = _repos.GetAll().FirstOrDefault(c => c.ColorID == id);
            return View(color);
        }
        public async Task<IActionResult> EditColor(Color color) // Thực hiện việc Tạo mới
        {
            var httpClient = new HttpClient();
            string apiUrl = $"https://localhost:7036/api/Color/update-color?ColorCode={color.ColorCode}&Name={color.Name}&Status={color.Status}&DateCreated={color.DateCreated}&ColorID={color.ColorID}";/*&ColorID={color.ColorID.ToString("yyyy-MM-ddTHH:mm:ss")}*/
            var response = await httpClient.PutAsync(apiUrl, null);
            return RedirectToAction("GetAllColor");
        }
        public async Task<IActionResult> DeleteColor(Guid id)
        {
            if (CheckUserRole() == false)
            {
                return RedirectToAction("Forbidden", "Home");
            }
            var cus = _repos.GetAll().First(c => c.ColorID == id);
            var httpClient = new HttpClient();
            string apiUrl = $"https://localhost:7036/api/Color/delete-color?id={id}";
            var response = await httpClient.DeleteAsync(apiUrl);
            return RedirectToAction("GetAllColor");
        }
        public async Task<IActionResult> FindColor(string searchQuery)
        {
            if (CheckUserRole() == false)
            {
                return RedirectToAction("Forbidden", "Home");
            }
            var color = _repos.GetAll().Where(c => c.ColorCode.ToLower().Contains(searchQuery.ToLower()) || c.Name.ToLower().Contains(searchQuery.ToLower()));
            return View(color);
        }
    }
}

