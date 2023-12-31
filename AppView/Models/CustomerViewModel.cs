﻿namespace AppView.Models
{
	public class CustomerViewModel
	{
		public Guid CumstomerID { get; set; }
		public string? FullName { get; set; }
		public string? UserName { get; set; }
		public string? Password { get; set; }
		public string? Email { get; set; }
		public int Sex { get; set; }
		public string? ResetPassword { get; set; }//
		public string? PhoneNumber { get; set; }
		public int Status { get; set; }
		public DateTime DateCreated { get; set; }
		public string? RankName { get; set; }
	}
}
