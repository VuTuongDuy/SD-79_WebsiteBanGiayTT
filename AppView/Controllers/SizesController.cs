﻿using AppData.IRepositories;
using AppData.Models;
using AppData.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AppView.Controllers
{
    public class SizesController : Controller
    {
        private readonly IAllRepositories<Size> _repos;
        private ShopDBContext _dbcontext = new ShopDBContext();
        private DbSet<Size> _size;

        public SizesController()
        {
            _size = _dbcontext.Sizes;
            AllRepositories<Size> all = new AllRepositories<Size>(_dbcontext, _size);
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
        private string GenerateSizeCode()
        {
            var lastSize = _dbcontext.Sizes.OrderByDescending(c => c.SizeCode).FirstOrDefault();
            if (lastSize != null)
            {
                var lastNumber = int.Parse(lastSize.SizeCode.Substring(2)); // Lấy phần số cuối cùng từ ColorCode
                var nextNumber = lastNumber + 1; // Tăng giá trị cuối cùng
                var newSizeCode = "SZ" + nextNumber.ToString("D3"); // Tạo ColorCode mới
                return newSizeCode;
            }
            return "SZ001"; // Trường hợp không có ColorCode trong cơ sở dữ liệu, trả về giá trị mặc định "CL001"
        }
        public async Task<IActionResult> GetAllSize()
        {
            if (CheckUserRole() == false)
            {
                return RedirectToAction("Forbidden", "Home");
            }
            string apiUrl = "https://localhost:7036/api/Size/get-size";
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(apiUrl);
            string apiData = await response.Content.ReadAsStringAsync();
            var size = JsonConvert.DeserializeObject<List<Size>>(apiData);
            return View(size);
        }
        [HttpGet]
        public async Task<IActionResult> CreateSize()
        {
            if (CheckUserRole() == false)
            {
                return RedirectToAction("Forbidden", "Home");
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateSize(Size size)
        {
            var httpClient = new HttpClient();
            string apiUrl = $"https://localhost:7036/api/Size/create-size?SizeCode={GenerateSizeCode()}&Name={size.Name}&Status={size.Status}&DateCreated={size.DateCreated}";
            var response = await httpClient.PostAsync(apiUrl, null);
            return RedirectToAction("GetAllSize");
        }
        [HttpGet]
        public async Task<IActionResult> EditSize(Guid id) // Khi ấn vào Create thì hiển thị View
        {
            if (CheckUserRole() == false)
            {
                return RedirectToAction("Forbidden", "Home");
            }
            // Lấy Product từ database dựa theo id truyền vào từ route
            Size size = _repos.GetAll().FirstOrDefault(c => c.SizeID == id);
            return View(size);
        }
        public async Task<IActionResult> EditSize(Size size) // Thực hiện việc Tạo mới
        {
            var httpClient = new HttpClient();
            string apiUrl = $"https://localhost:7036/api/Size/update-size?sizeID={size.SizeID}&SizeCode={size.SizeCode}&Name={size.Name}&Status={size.Status}&DateCreated={size.DateCreated}";
            var response = await httpClient.PutAsync(apiUrl, null);
            return RedirectToAction("GetAllSize");
        }
        public async Task<IActionResult> DeleteSize(Guid id)
        {
            if (CheckUserRole() == false)
            {
                return RedirectToAction("Forbidden", "Home");
            }
            var sz = _repos.GetAll().First(c => c.SizeID == id);
            var httpClient = new HttpClient();
            string apiUrl = $"https://localhost:7036/api/Size/delete-size?sizeID={id}";
            var response = await httpClient.DeleteAsync(apiUrl);
            return RedirectToAction("GetAllSize");

        }
        public async Task<IActionResult> FindSize(string searchQuery)
        {
            if (CheckUserRole() == false)
            {
                return RedirectToAction("Forbidden", "Home");
            }
            var size = _repos.GetAll().Where(c => c.SizeCode.ToLower().Contains(searchQuery.ToLower()) || c.Name.ToLower().Contains(searchQuery.ToLower()));
            return View(size);
        }
    }
}
