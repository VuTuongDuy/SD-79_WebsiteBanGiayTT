﻿using AppData.IRepositories;
using AppData.Models;
using AppData.Repositories;
using AppView.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AppView.Controllers
{

    public class VoucherController : Controller
    {
        private readonly IAllRepositories<Voucher> repos;
        private ShopDBContext context = new ShopDBContext();
        private DbSet<Voucher> voucher;


        public VoucherController()
        {
            voucher = context.Vouchers;
            AllRepositories<Voucher> all = new AllRepositories<Voucher>(context, voucher);
            repos = all;
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
        private string GenerateVoucherCode()
        {
            var last = context.Vouchers.OrderByDescending(c => c.VoucherCode).FirstOrDefault();
            if (last != null)
            {
                var lastNumber = int.Parse(last.VoucherCode.Substring(2)); // Lấy phần số cuối cùng từ ColorCode
                var nextNumber = lastNumber + 1; // Tăng giá trị cuối cùng
                var newCode = "VC" + nextNumber.ToString("D3"); // Tạo ColorCode mới
                return newCode;
            }
            return "VC001"; // Trường hợp không có ColorCode trong cơ sở dữ liệu, trả về giá trị mặc định "CL001"
        }

        public async Task<IActionResult> YourAction()
        {
            // Lấy mã voucher bằng cách sử dụng phương thức private
            var voucherCode = GenerateVoucherCode();

            // Truyền mã voucher đến view
            ViewData["VoucherCode"] = voucherCode;

            return View();
        }
        public async Task<IActionResult> GetAllVouchers()
        {
            if (CheckUserRole() == false)
            {
                return RedirectToAction("Forbidden", "Home");
            }
            string apiUrl = "https://localhost:7036/api/Voucher/get-voucher";
            var httpClient = new HttpClient(); // tạo ra để callApi
            var response = await httpClient.GetAsync(apiUrl);// Lấy dữ liệu ra
            string apiData = await response.Content.ReadAsStringAsync();
            var styles = JsonConvert.DeserializeObject<List<VoucherViewModel>>(apiData);
            /*ShoppingCartViewModel s = new ShoppingCartViewModel();
            s.Vouchers = styles.ToList();*/


            return View(styles);

        }
        public async Task<IActionResult> GetAllVoucherss()
        {
            if (CheckUserRole() == false)
            {
                return RedirectToAction("Forbidden", "Home");
            }
            // Lấy username từ session
            var username = HttpContext.Session.GetString("UserName");

            // Kiểm tra nếu username có giá trị
            if (!string.IsNullOrEmpty(username))
            {
                // Gọi API và truyền username
                string apiUrl = $"https://localhost:7036/api/Voucher/get-voucher-for-username?username={username}";
                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(apiUrl);
                string apiData = await response.Content.ReadAsStringAsync();
                var vouchers = JsonConvert.DeserializeObject<List<VoucherViewModel>>(apiData);

                return View(vouchers);
            }
            else
            {
                // Nếu username không có giá trị, gọi API mà không truyền thêm thông tin
                string apiUrl = "https://localhost:7036/api/Voucher/get-voucher1";
                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(apiUrl);
                string apiData = await response.Content.ReadAsStringAsync();
                var vouchers = JsonConvert.DeserializeObject<List<VoucherViewModel>>(apiData);

                return View(vouchers);
            }
        }



        public async Task<IActionResult> CreateVouchers()
        {
            if (CheckUserRole() == false)
            {
                return RedirectToAction("Forbidden", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateVouchers(Voucher voucher)
        {
            var httpClient = new HttpClient();
            string apiUrl = $"https://localhost:7036/api/Voucher/create-voucher?code={GenerateVoucherCode()}&exclusiveright={voucher.Exclusiveright}&status={voucher.Status}&total={voucher.Total}&value={voucher.VoucherValue}&maxUse=0&remainUse={voucher.RemainingUsage}&expireDate={voucher.ExpirationDate.ToString("yyyy-MM-ddTHH:mm:ss")}&DateCreated={voucher.DateCreated.ToString("yyyy-MM-ddTHH:mm:ss")}&Type={voucher.Type}&CreateDate={DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")}&IsDel={voucher.IsDel}&UserNameCustomer={voucher.UserNameCustomer}";
            var response = await httpClient.PostAsync(apiUrl, null);
            return RedirectToAction("GetAllVouchers");
        }

        [HttpGet]
        public async Task<IActionResult> EditVouchers(Guid id) // Khi ấn vào Create thì hiển thị View
        {
            if (CheckUserRole() == false)
            {
                return RedirectToAction("Forbidden", "Home");
            }
            // Lấy Product từ database dựa theo id truyền vào từ route
            Voucher voucher = repos.GetAll().FirstOrDefault(c => c.VoucherID == id);
            return View(voucher);
        }



        // Hàm giả định để lấy giá trị usernamecustomer từ Session

        public async Task<IActionResult> EditVouchers(Voucher voucher)
        {
            var httpClient = new HttpClient();
            string apiUrl = $"https://localhost:7036/api/Voucher/update-voucher?id={voucher.VoucherID}&exclusiveright={voucher.Exclusiveright}&total={voucher.Total}&code={voucher.VoucherCode}&status={voucher.Status}&value={voucher.VoucherValue}&maxUse={voucher.MaxUsage}&remainUse={voucher.RemainingUsage}&dateTime={voucher.ExpirationDate.ToString("yyyy-MM-ddTHH:mm:ss")}&DateCreated={voucher.DateCreated.ToString("yyyy-MM-ddTHH:mm:ss")}&Type={voucher.Type}&CreateDate={DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")}&IsDel ={voucher.IsDel}";
            var response = await httpClient.PutAsync(apiUrl, null);
            return RedirectToAction("GetAllVouchers");
        }

        public async Task<IActionResult> DeleteVouchers(Guid id)
        {
            if (CheckUserRole() == false)
            {
                return RedirectToAction("Forbidden", "Home");
            }
            var voucher = repos.GetAll().First(c => c.VoucherID == id);
            var httpClient = new HttpClient();
            string apiUrl = $"https://localhost:7036/api/Voucher/delete-voucher?id={id}";
            var response = await httpClient.DeleteAsync(apiUrl);
            return RedirectToAction("GetAllVouchers");
        }
        public async Task<IActionResult> FindVouchers(string searchQuery)
        {
            if (CheckUserRole() == false)
            {
                return RedirectToAction("Forbidden", "Home");
            }
            var color = repos.GetAll().Where(c => c.VoucherCode.ToLower().Contains(searchQuery.ToLower()));
            return View(color);
        }




        public IActionResult UseVoucher(string voucherCode)
        {
            var voucher = context.Vouchers.FirstOrDefault(v => v.VoucherCode == voucherCode);

            if (voucher == null)
            {
                // Voucher không tồn tại
                return RedirectToAction("InvalidVoucher");
            }

            if (voucher.ExpirationDate < DateTime.Now)
            {
                // Voucher đã hết hạn
                return RedirectToAction("ExpiredVoucher");
            }

            if (voucher.RemainingUsage <= 0)
            {
                // Voucher đã hết lượt sử dụng
                return RedirectToAction("MaxUsageReached");
            }

            // Giảm số lượt sử dụng còn lại của voucher
            voucher.RemainingUsage -= 1;

            // Lưu các thay đổi vào cơ sở dữ liệu
            context.SaveChanges();

            // Chuyển hướng tới trang thành công
            return RedirectToAction("VoucherUsed");
        }

        public IActionResult InvalidVoucher()
        {
            // Xử lý khi voucher không tồn tại
            return View();
        }

        public IActionResult ExpiredVoucher()
        {
            // Xử lý khi voucher đã hết hạn
            return View();
        }

        public IActionResult MaxUsageReached()
        {
            // Xử lý khi voucher đã hết lượt sử dụng
            return View();
        }

        public IActionResult VoucherUsed()
        {
            // Xử lý khi voucher đã được sử dụng thành công
            return View();
        }

        public async Task<IActionResult> Details(Guid Id)
        {
            var vc = repos.GetAll().FirstOrDefault(c => c.VoucherID == Id);
            return View(vc);
        }
        public async Task<IActionResult> Details1(Guid Id)
        {
            var vc = repos.GetAll().FirstOrDefault(c => c.VoucherID == Id);
            return View(vc);
        }


        public ActionResult LockVoucher(Guid id)
        {
            // Lấy voucher từ cơ sở dữ liệu dựa trên id
            var voucher = context.Vouchers.Find(id);

            if (voucher != null)
            {
                // Thực hiện logic để khóa voucher
                voucher.Status = 1; // Đặt Status thành 1 để chỉ định đã khóa

                // Cập nhật cơ sở dữ liệu
                context.SaveChanges();
            }

            // Chuyển hướng lại danh sách voucher
            return RedirectToAction("GetAllVouchers");
        }

        public ActionResult UnlockVoucher(Guid id)
        {
            // Lấy voucher từ cơ sở dữ liệu dựa trên id
            var voucher = context.Vouchers.Find(id);

            if (voucher != null)
            {
                // Thực hiện logic để mở khóa voucher
                voucher.Status = 0; // Đặt Status thành 0 để chỉ định đã mở khóa

                // Cập nhật cơ sở dữ liệu
                context.SaveChanges();
            }

            // Chuyển hướng lại danh sách voucher
            return RedirectToAction("GetAllVouchers");
        }
    }
}