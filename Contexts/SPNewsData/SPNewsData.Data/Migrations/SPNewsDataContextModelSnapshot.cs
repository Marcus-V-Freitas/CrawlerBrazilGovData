﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SPNewsData.Data.Context;

namespace SPNewsData.Data.Migrations
{
    [DbContext(typeof(SPNewsDataContext))]
    partial class SPNewsDataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.11");

            modelBuilder.Entity("SPNewsData.Domain.Entities.GovNews", b =>
                {
                    b.Property<int?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime?>("CaptureDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime(6)")
                        .HasDefaultValueSql("getdate()");

                    b.Property<DateTime?>("PublicationDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Source")
                        .HasColumnType("longtext");

                    b.Property<string>("Subtitle")
                        .HasColumnType("longtext");

                    b.Property<string>("Title")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("GovNews");
                });

            modelBuilder.Entity("SPNewsData.Domain.Entities.Subject", b =>
                {
                    b.Property<int?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int?>("GovNewsId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("GovNewsId");

                    b.ToTable("Subjects");
                });

            modelBuilder.Entity("SPNewsData.Domain.Entities.UrlExtracted", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Search")
                        .HasColumnType("longtext");

                    b.Property<string>("Title")
                        .HasColumnType("longtext");

                    b.Property<string>("Url")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("UrlExtracteds");
                });

            modelBuilder.Entity("SPNewsData.Domain.Entities.Subject", b =>
                {
                    b.HasOne("SPNewsData.Domain.Entities.GovNews", "GovNews")
                        .WithMany("Subjects")
                        .HasForeignKey("GovNewsId");

                    b.Navigation("GovNews");
                });

            modelBuilder.Entity("SPNewsData.Domain.Entities.GovNews", b =>
                {
                    b.Navigation("Subjects");
                });
#pragma warning restore 612, 618
        }
    }
}
