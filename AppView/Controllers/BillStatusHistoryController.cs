﻿using AppData.IRepositories;
using AppData.Models;
using AppData.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using AppView.Models;

namespace AppView.Controllers
{
    public class BillStatusHistoryController : Controller
    {
        private readonly IAllRepositories<BillStatusHistory> _repos;
        private readonly IAllRepositories<Bill> _billRepos;
        private readonly IAllRepositories<Employee> _employeeRepos;
        private ShopDBContext _dbContext = new ShopDBContext();
        private DbSet<BillStatusHistory> _billStatusHistories;
        private DbSet<Bill> _bill;
        private DbSet<Employee> _employee;
        public BillStatusHistoryController()
        {
            _billStatusHistories = _dbContext.BillStatusHistories;
            AllRepositories<BillStatusHistory> all = new AllRepositories<BillStatusHistory>(_dbContext, _billStatusHistories);
            _repos = all;

            _bill = _dbContext.Bills;
            AllRepositories<Bill> BillAll = new AllRepositories<Bill>(_dbContext, _bill);
            _billRepos = BillAll;

            _employee = _dbContext.Employees;
            AllRepositories<Employee> EmployeeAll = new AllRepositories<Employee>(_dbContext, _employee);
            _employeeRepos = EmployeeAll;
        }
        public async Task<IActionResult> GetAllBillStatusHistory()
        {
            string apiUrl = "https://localhost:7036/api/BillStatusHistory/get-billStatusHistory";
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(apiUrl);
            string apiData = await response.Content.ReadAsStringAsync();
            var billStatusHistory = JsonConvert.DeserializeObject<List<BillStatusHistory>>(apiData);
            var bills = _billRepos.GetAll();
            var employees = _employeeRepos.GetAll();

            // Tạo danh sách ProductViewModel với thông tin Supplier và Material
            var billStatusHistoryViewModels = billStatusHistory.Select(billStatusHistory => new BillStatusHistoryViewModel
            {
                BillStatusHistoryID = billStatusHistory.BillStatusHistoryID,
                Status = billStatusHistory.Status,
                ChangeDate = billStatusHistory.ChangeDate,
                Note = billStatusHistory.Note,
                BillCode = bills.FirstOrDefault(s => s.BillID == billStatusHistory.BillID)?.BillCode,
                EmployeeName = employees.FirstOrDefault(m => m.EmployeeID == billStatusHistory.EmployeeID)?.UserName
            }).ToList();

            return View(billStatusHistoryViewModels);
        }

        [HttpGet]
        public async Task<IActionResult> CreateBillStatusHistory()
        {
            using (ShopDBContext shopDBContext = new ShopDBContext())
            {
                var bill = shopDBContext.Bills.ToList();
                SelectList selectListBill = new SelectList(bill, "BillID", "BillCode");
                ViewBag.BillList = selectListBill;

                var employee = shopDBContext.Employees.Where(c => c.Status == 0).ToList();
                SelectList selectListEmployee = new SelectList(employee, "EmployeeID", "UserName");
                ViewBag.EmployeeList = selectListEmployee;
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateBillStatusHistory(BillStatusHistory billStatusHistory)
        {
            HttpClient httpClient = new HttpClient();
            string apiUrl = $"https://localhost:7036/api/BillStatusHistory/add-billStatusHistory?Status={billStatusHistory.Status}&ChangeDate={billStatusHistory.ChangeDate.ToString("yyyy-MM-ddTHH:mm:ss")}&Note={billStatusHistory.Note}&BillID={billStatusHistory.BillID}&EmployeeID={billStatusHistory.EmployeeID}";
            var response = await httpClient.PostAsync(apiUrl, null);
            return RedirectToAction("GetAllBillStatusHistory");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateBillStatusHistory(Guid id)
        {
            BillStatusHistory billStatusHistory = _repos.GetAll().FirstOrDefault(c => c.BillStatusHistoryID == id);
            using (ShopDBContext shopDBContext = new ShopDBContext())
            {
                var bill = shopDBContext.Bills.ToList();
                SelectList selectListBill = new SelectList(bill, "BillID", "BillCode");
                ViewBag.BillList = selectListBill;

                var employee = shopDBContext.Employees.Where(c => c.Status == 0).ToList();
                SelectList selectListEmployee = new SelectList(employee, "EmployeeID", "UserName");
                ViewBag.EmployeeList = selectListEmployee;
            }
            return View(billStatusHistory);
        }

        public async Task<IActionResult> UpdateBillStatusHistory(BillStatusHistory billStatusHistory)
        {
            HttpClient httpClient = new HttpClient();
            string apiUrl = $"https://localhost:7036/api/BillStatusHistory/update-billStatusHistory?BillStatusHistoryID={billStatusHistory.BillStatusHistoryID}&Status={billStatusHistory.Status}&ChangeDate={billStatusHistory.ChangeDate.ToString("yyyy-MM-ddTHH:mm:ss")}&Note={billStatusHistory.Note}&BillID={billStatusHistory.BillID}&EmployeeID={billStatusHistory.EmployeeID}";
            var response = await httpClient.PutAsync(apiUrl, null);
            return RedirectToAction("GetAllBillStatusHistory");
        }

        public async Task<IActionResult> DeleteBillStatusHistory(Guid id)
        {
            var billStatusHistory = _repos.GetAll().FirstOrDefault(c => c.BillStatusHistoryID == id);
            HttpClient httpClient = new HttpClient();
            string apiUrl = $"https://localhost:7036/api/BillStatusHistory/delete-billStatusHistory?BillStatusHistoryID={id}";
            var response = await httpClient.DeleteAsync(apiUrl);
            return RedirectToAction("GetAllBillStatusHistory");
        }
    }
}
