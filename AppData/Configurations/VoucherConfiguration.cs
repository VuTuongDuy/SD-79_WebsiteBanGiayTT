﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppData.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace AppData.Configurations
{
    public class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
    {
        public void Configure(EntityTypeBuilder<Voucher> builder)
        {
            builder.ToTable("Voucher");
            builder.HasKey(c => c.VoucherID);
            builder.Property(c => c.VoucherCode).HasColumnType("nvarchar(100)");
            builder.Property(c => c.VoucherValue).HasColumnType("decimal(18, 2)");
			builder.Property(c => c.Total).HasColumnType("decimal(18, 2)");
			builder.Property(c => c.Exclusiveright).HasColumnType("nvarchar(100)").IsRequired(false);
			builder.Property(c => c.ExpirationDate).HasColumnType("Datetime");
            builder.Property(c => c.MaxUsage).HasColumnType("int");
            builder.Property(c => c.RemainingUsage).HasColumnType("int");
            builder.Property(c => c.Status).HasColumnType("int");
			builder.Property(c => c.Type).HasColumnType("int").IsRequired(false);
			builder.Property(c => c.CreateDate).HasColumnType("Datetime");
			//builder.Property(c => c.IsDel).HasColumnType("bool").IsRequired(false);
			builder.Property(c => c.DateCreated).HasColumnType("Datetime");
			builder.Property(c => c.UserNameCustomer).HasColumnType("nvarchar(100)").IsRequired(false);
		}
    }
}
