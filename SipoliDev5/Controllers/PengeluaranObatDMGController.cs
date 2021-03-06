﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SipoliDev5.Models;
using PagedList; //paging
using PagedList.Mvc;
using System.Text; //paging
using SipoliDev5.Models.ViewModels;
using System.Data.Entity.Infrastructure;//view model
namespace SipoliDev5.Controllers
{
    [Authorize]
    public class PengeluaranObatDMGController : Controller
    {
        private EntitiesConnection db = new EntitiesConnection();
        public JsonResult GetDataStokObat()
        {
            var coba = from r in db.StokObat where r.KlinikID == 1 where r.Stok > 0 select new { Value = r.Obat.ID, Text = r.Obat.Nama, Stok = r.Stok };
            return Json(coba);
        }
        // GET: /PengeluaranObatDMG/
        public JsonResult GetDataPasien(string term)
        {
            var result = (from r in db.Orang
                          where r.Nama.ToLower().Contains(term.ToLower())
                          select new { label = r.Nama, value = r.Nama, id = r.ID });//query
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDataObat(string term)
        {
            var obat = (from r in db.StokObat
                        where r.Obat.Nama.ToLower().Contains(term.ToLower())
                        where r.KlinikID == 1
                        select new { label = r.Obat.Nama, value = r.Obat.Nama, id = r.ID });//query
            return Json(obat, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDataPasienFilter(string term)
        {
            var result = (from r in db.Orang
                          from i in db.PengeluaranObat
                          where r.ID == i.PasienID
                          where i.KlinikID == 1
                          where r.Nama.ToLower().Contains(term.ToLower())
                          select new { label = r.Nama, value = r.Nama, id = r.ID }).Distinct();//query
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // GET: /PengeluaranObat/
        public ActionResult Index(string Obat, string Pasien, string Month, DateTime? Date, int? Year, string Sortby, int? page, string Command)
        {
            var pengeluaranobat = from a in db.PengeluaranObat
                                  where a.KlinikID == 1
                                  select new PengeluaranObat_ViewModel()
                                  {
                                      ID = a.ID,
                                      Tanggal = a.Tanggal,
                                      PasienID = a.PasienID,
                                      ObatID = a.ObatID,
                                      KlinikID = a.KlinikID,
                                      Jumlah = a.Jumlah,
                                      Pasien = a.Orang.Nama,
                                      Obat = a.Obat.Nama,
                                      SatuanObat = a.Obat.SatuanObat.Nama,
                                      Klinik = a.Klinik.Nama,
                                      TujuanKlinikID = a.TujuanKlinikID,
                                      //TujuanKlinik = a.TujuanKlinik.Klinik.Nama,
                                  };

            //filtering
            var SelectListYear = ((from year in db.PengeluaranObat orderby year.Tanggal.Value.Year select year.Tanggal.Value.Year)).Distinct().ToList();
            ViewBag.Year = new SelectList(SelectListYear);

            if (!String.IsNullOrEmpty(Obat))
            {
                pengeluaranobat = pengeluaranobat.Where(a => a.Obat.Contains(Obat));
            }

            if (!String.IsNullOrEmpty(Pasien))
            {
                pengeluaranobat = pengeluaranobat.Where(b => b.Pasien.Contains(Pasien));
            }

            if (Date != null)
            {
                pengeluaranobat = pengeluaranobat.Where(c => c.Tanggal == Date);
            }

            if (Year != null)
            {
                pengeluaranobat = pengeluaranobat.Where(d => d.Tanggal.Value.Year == Year);
            }

            if (!String.IsNullOrEmpty(Month))
            {
                int MonthInt = Int32.Parse(Month);
                pengeluaranobat = pengeluaranobat.Where(e => e.Tanggal.Value.Month == MonthInt);
            }

            //sorting
            ViewBag.SortTanggalParameter = Sortby == "Tanggal" ? "Tanggal Desc" : "Tanggal";
            ViewBag.SortObatParameter = Sortby == "Obat" ? "Obat Desc" : "Obat";
            ViewBag.SortPasienParameter = Sortby == "Pasien" ? "Pasien Desc" : "Pasien";

            switch (Sortby)
            {
                case "Tanggal":
                    pengeluaranobat = pengeluaranobat.OrderBy(a => a.Tanggal);
                    break;
                case "Tanggal Desc":
                    pengeluaranobat = pengeluaranobat.OrderByDescending(a => a.Tanggal);
                    break;
                case "Obat":
                    pengeluaranobat = pengeluaranobat.OrderBy(b => b.Obat);
                    break;
                case "Obat Desc":
                    pengeluaranobat = pengeluaranobat.OrderByDescending(b => b.Obat);
                    break;
                case "Pasien":
                    pengeluaranobat = pengeluaranobat.OrderBy(c => c.Pasien);
                    break;
                case "Pasien Desc":
                    pengeluaranobat = pengeluaranobat.OrderByDescending(c => c.Pasien);
                    break;
                default:
                    pengeluaranobat = pengeluaranobat.OrderByDescending(g => g.ID);
                    break;
            }

            //export
            //Session["pengeluaranobats"] = pengeluaranobat.ToList<PengeluaranObat_ViewModel>();
            if (Command == "Export")
            {
                pengeluaranobat.ToList<PengeluaranObat_ViewModel>();
                StringBuilder sb = new StringBuilder();
                var Month1 = "";
                if (pengeluaranobat != null && pengeluaranobat.Any())
                {
                    sb.Append("<table style='font-size:20px;'>");
                    //row1
                    sb.Append("<tr style='height:15px'></tr>");
                    //row2
                    sb.Append("<tr>");
                    sb.Append("<td></td>");
                    sb.Append("<td colspan='5'style='border:1px solid black; width:120px', align='center'><b>DATA PENGELUARAN OBAT POLIKLINIK DRAMAGA</b></td>");
                    sb.Append("</tr>");
                    //row3
                    if (!string.IsNullOrEmpty(Obat) || !string.IsNullOrEmpty(Pasien) || Year != null || !string.IsNullOrEmpty(Month) || Date != null)
                    {
                        if (!string.IsNullOrEmpty(Month)) { switch (Month) { case "1":Month1 = "Januari"; break; case "2":Month1 = "Februari"; break; case "3":Month1 = "Maret"; break; case "4":Month1 = "April"; break; case "5":Month1 = "Mei"; break; case "6":Month1 = "Juni"; break; case "7":Month1 = "Juli"; break; case "8":Month1 = "Agustus"; break; case "9":Month1 = "September"; break; case "10":Month1 = "Oktober"; break; case "11":Month1 = "November"; break; case "12":Month1 = "Desember"; break; default: Month1 = "-"; break; } }
                        sb.Append("<tr>");
                        sb.Append("<td style='width:15px'></td>");
                        sb.Append("<td style='width:150px; border:1px solid black'><b>TANGGAL</b></td>");
                        sb.Append("<td style='width:300px; border:1px solid black'>" + Date + "</td>");
                        sb.Append("<td style='width:120px; border:1px solid black'><b>BULAN</b></td>");
                        sb.Append("<td colspan='2'style='width:120px; border:1px solid black'>" + Month1 + "</td>");
                        sb.Append("</tr>");
                        sb.Append("<tr>");
                        sb.Append("<td style='width:15px'></td>");
                        sb.Append("<td style='width:150px; border:1px solid black'><b>NAMA OBAT</b></td>");
                        sb.Append("<td style='width:300px; border:1px solid black'>" + Obat + "</td>");
                        sb.Append("<td style='width:120px; border:1px solid black'><b>TAHUN</b></td>");
                        sb.Append("<td colspan='2'style='width:120px; border:1px solid black;text-align: left;'>" + Year + "</td>");
                        sb.Append("</tr>");
                        sb.Append("<tr>");
                        sb.Append("<td style='width:15px'></td>");
                        sb.Append("<td style='width:150px; border:1px solid black'><b>NAMA PASIEN</b></td>");
                        sb.Append("<td style='width:120px; border:1px solid black'>" + Pasien + "</td>");
                        sb.Append("<td colspan='3' style='border:1px solid black'></td>");
                        sb.Append("</tr>");
                    }
                    //row4
                    sb.Append("<tr>");
                    sb.Append("<td style='width:15px'></td>");
                    sb.Append("<td style='width:150px; border:1px solid black; background-color: yellow;'><center><b>TANGGAL</b></center></td>");
                    sb.Append("<td style='width:300px; border:1px solid black; background-color: yellow;'><center><b>NAMA PASIEN</b></center></td>");
                    sb.Append("<td style='width:300px; border:1px solid black; background-color: yellow;'><center><b>NAMA OBAT</b></center></td>");
                    sb.Append("<td style='width:120px; border:1px solid black; background-color: yellow;'><center><b>JUMLAH</b></center></td>");
                    sb.Append("<td style='width:200px; border:1px solid black; background-color: yellow;'><center><b>SATUAN OBAT</b></center></td>");
                    sb.Append("</tr>");

                    foreach (var result in pengeluaranobat)
                    {
                        sb.Append("<tr>");
                        sb.Append("<td style='width:15px'></td>");
                        sb.Append("<td style='border:1px solid black;'>" + result.Tanggal + "</td>");
                        sb.Append("<td style='border:1px solid black;'>" + result.Pasien + "</td>");
                        sb.Append("<td style='border:1px solid black;'>" + result.Obat + "</td>");
                        sb.Append("<td style='border:1px solid black;'>" + result.Jumlah + "</td>");
                        sb.Append("<td style='border:1px solid black;'>" + result.SatuanObat + "</td>");
                        sb.Append("</tr>");
                    }

                    if (!string.IsNullOrEmpty(Obat) || !string.IsNullOrEmpty(Pasien) || Year != null || !string.IsNullOrEmpty(Month) || Date != null)
                    {
                        if (!string.IsNullOrEmpty(Obat))
                        {
                            pengeluaranobat = (from a in db.PengeluaranObat
                                               where a.KlinikID == 1
                                               where a.TujuanKlinikID == 1
                                               where a.Obat.Nama == Obat
                                               group a by new { b = Obat, c = a.Obat.SatuanObat.Nama } into g
                                               select new PengeluaranObat_ViewModel()
                                               {
                                                   Obat = g.Key.b,
                                                   SatuanObat = g.Key.c,
                                                   Total = g.Select(m => m.Jumlah).Sum()
                                               });
                        }
                        if (!string.IsNullOrEmpty(Pasien))
                        {
                            pengeluaranobat = (from a in db.PengeluaranObat
                                               where a.KlinikID == 1
                                               where a.TujuanKlinikID == 1
                                               where a.Orang.Nama == Pasien
                                               group a by new { b = a.Obat.Nama, c = a.Obat.SatuanObat.Nama } into g
                                               select new PengeluaranObat_ViewModel()
                                               {
                                                   Obat = g.Key.b,
                                                   SatuanObat = g.Key.c,
                                                   Total = g.Select(m => m.Jumlah).Sum()
                                               });
                        }
                        if (Year != null)
                        {
                            pengeluaranobat = (from a in db.PengeluaranObat
                                               where a.KlinikID == 1
                                               where a.TujuanKlinikID == 1
                                               where a.Tanggal.Value.Year == Year
                                               group a by new { b = a.Obat.Nama, c = a.Obat.SatuanObat.Nama } into g
                                               select new PengeluaranObat_ViewModel()
                                               {
                                                   Obat = g.Key.b,
                                                   SatuanObat = g.Key.c,
                                                   Total = g.Select(m => m.Jumlah).Sum()
                                               });
                        }
                        if (!string.IsNullOrEmpty(Month))
                        {
                            var intmonth = int.Parse(Month);
                            pengeluaranobat = (from a in db.PengeluaranObat
                                               where a.KlinikID == 1
                                               where a.TujuanKlinikID == 1
                                               where a.Tanggal.Value.Month == intmonth
                                               group a by new { b = a.Obat.Nama, c = a.Obat.SatuanObat.Nama } into g
                                               select new PengeluaranObat_ViewModel()
                                               {
                                                   Obat = g.Key.b,
                                                   SatuanObat = g.Key.c,
                                                   Total = g.Select(m => m.Jumlah).Sum()
                                               });
                        }
                        if (Date != null)
                        {
                            pengeluaranobat = (from a in db.PengeluaranObat
                                               where a.KlinikID == 1
                                               where a.TujuanKlinikID == 1
                                               where a.Tanggal == Date
                                               group a by new { b = a.Obat.Nama, c = a.Obat.SatuanObat.Nama } into g
                                               select new PengeluaranObat_ViewModel()
                                               {
                                                   Obat = g.Key.b,
                                                   SatuanObat = g.Key.c,
                                                   Total = g.Select(m => m.Jumlah).Sum()
                                               });
                        }
                        if (!string.IsNullOrEmpty(Obat) && !string.IsNullOrEmpty(Pasien))
                        {
                            pengeluaranobat = (from a in db.PengeluaranObat
                                               where a.KlinikID == 1
                                               where a.TujuanKlinikID == 1
                                               where a.Obat.Nama == Obat
                                               where a.Orang.Nama == Pasien
                                               group a by new { b = a.Obat.Nama, c = a.Obat.SatuanObat.Nama } into g
                                               select new PengeluaranObat_ViewModel()
                                               {
                                                   Obat = g.Key.b,
                                                   SatuanObat = g.Key.c,
                                                   Total = g.Select(m => m.Jumlah).Sum()
                                               });
                        }
                        if (!string.IsNullOrEmpty(Obat) && Year != null)
                        {
                            pengeluaranobat = (from a in db.PengeluaranObat
                                               where a.KlinikID == 1
                                               where a.TujuanKlinikID == 1
                                               where a.Obat.Nama == Obat
                                               where a.Tanggal.Value.Year == Year
                                               group a by new { b = a.Obat.Nama, c = a.Obat.SatuanObat.Nama } into g
                                               select new PengeluaranObat_ViewModel()
                                               {
                                                   Obat = g.Key.b,
                                                   SatuanObat = g.Key.c,
                                                   Total = g.Select(m => m.Jumlah).Sum()
                                               });
                        }
                        if (!string.IsNullOrEmpty(Obat) && !string.IsNullOrEmpty(Month))
                        {
                            var intmonth = int.Parse(Month);
                            pengeluaranobat = (from a in db.PengeluaranObat
                                               where a.KlinikID == 1
                                               where a.TujuanKlinikID == 1
                                               where a.Obat.Nama == Obat
                                               where a.Tanggal.Value.Month == intmonth
                                               group a by new { b = a.Obat.Nama, c = a.Obat.SatuanObat.Nama } into g
                                               select new PengeluaranObat_ViewModel()
                                               {
                                                   Obat = g.Key.b,
                                                   SatuanObat = g.Key.c,
                                                   Total = g.Select(m => m.Jumlah).Sum()
                                               });
                        }
                        if (!string.IsNullOrEmpty(Obat) && Date != null)
                        {
                            pengeluaranobat = (from a in db.PengeluaranObat
                                               where a.KlinikID == 1
                                               where a.TujuanKlinikID == 1
                                               where a.Obat.Nama == Obat
                                               where a.Tanggal == Date
                                               group a by new { b = a.Obat.Nama, c = a.Obat.SatuanObat.Nama } into g
                                               select new PengeluaranObat_ViewModel()
                                               {
                                                   Obat = g.Key.b,
                                                   SatuanObat = g.Key.c,
                                                   Total = g.Select(m => m.Jumlah).Sum()
                                               });
                        }

                        if (!string.IsNullOrEmpty(Pasien) && Year != null)
                        {
                            pengeluaranobat = (from a in db.PengeluaranObat
                                               where a.KlinikID == 1
                                               where a.TujuanKlinikID == 1
                                               where a.Tanggal.Value.Year == Year
                                               where a.Orang.Nama == Pasien
                                               group a by new { b = a.Obat.Nama, c = a.Obat.SatuanObat.Nama } into g
                                               select new PengeluaranObat_ViewModel()
                                               {
                                                   Obat = g.Key.b,
                                                   SatuanObat = g.Key.c,
                                                   Total = g.Select(m => m.Jumlah).Sum()
                                               });
                        }
                        if (!string.IsNullOrEmpty(Pasien) && !string.IsNullOrEmpty(Month))
                        {
                            var intmonth = int.Parse(Month);
                            pengeluaranobat = (from a in db.PengeluaranObat
                                               where a.KlinikID == 1
                                               where a.TujuanKlinikID == 1
                                               where a.Tanggal.Value.Month == intmonth
                                               where a.Orang.Nama == Pasien
                                               group a by new { b = a.Obat.Nama, c = a.Obat.SatuanObat.Nama } into g
                                               select new PengeluaranObat_ViewModel()
                                               {
                                                   Obat = g.Key.b,
                                                   SatuanObat = g.Key.c,
                                                   Total = g.Select(m => m.Jumlah).Sum()
                                               });
                        }
                        if (!string.IsNullOrEmpty(Pasien) && Date != null)
                        {
                            pengeluaranobat = (from a in db.PengeluaranObat
                                               where a.KlinikID == 1
                                               where a.TujuanKlinikID == 1
                                               where a.Tanggal == Date
                                               where a.Orang.Nama == Pasien
                                               group a by new { b = a.Obat.Nama, c = a.Obat.SatuanObat.Nama } into g
                                               select new PengeluaranObat_ViewModel()
                                               {
                                                   Obat = g.Key.b,
                                                   SatuanObat = g.Key.c,
                                                   Total = g.Select(m => m.Jumlah).Sum()
                                               });
                        }
                        if (Year != null && !string.IsNullOrEmpty(Month))
                        {
                            var intmonth = int.Parse(Month);
                            pengeluaranobat = (from a in db.PengeluaranObat
                                               where a.KlinikID == 1
                                               where a.TujuanKlinikID == 1
                                               where a.Tanggal.Value.Month == intmonth
                                               where a.Tanggal.Value.Year == Year
                                               group a by new { b = a.Obat.Nama, c = a.Obat.SatuanObat.Nama } into g
                                               select new PengeluaranObat_ViewModel()
                                               {
                                                   Obat = g.Key.b,
                                                   SatuanObat = g.Key.c,
                                                   Total = g.Select(m => m.Jumlah).Sum()
                                               });
                        }

                        if (!string.IsNullOrEmpty(Obat) && !string.IsNullOrEmpty(Pasien) && Year != null)
                        {
                            pengeluaranobat = (from a in db.PengeluaranObat
                                               where a.KlinikID == 1
                                               where a.TujuanKlinikID == 1
                                               where a.Obat.Nama == Obat
                                               where a.Orang.Nama == Pasien
                                               where a.Tanggal.Value.Year == Year
                                               group a by new { b = a.Obat.Nama, c = a.Obat.SatuanObat.Nama } into g
                                               select new PengeluaranObat_ViewModel()
                                               {
                                                   Obat = g.Key.b,
                                                   SatuanObat = g.Key.c,
                                                   Total = g.Select(m => m.Jumlah).Sum()
                                               });
                        }
                        if (!string.IsNullOrEmpty(Obat) && !string.IsNullOrEmpty(Pasien) && !string.IsNullOrEmpty(Month))
                        {
                            var intmonth = int.Parse(Month);
                            pengeluaranobat = (from a in db.PengeluaranObat
                                               where a.KlinikID == 1
                                               where a.TujuanKlinikID == 1
                                               where a.Obat.Nama == Obat
                                               where a.Orang.Nama == Pasien
                                               where a.Tanggal.Value.Month == intmonth
                                               group a by new { b = a.Obat.Nama, c = a.Obat.SatuanObat.Nama } into g
                                               select new PengeluaranObat_ViewModel()
                                               {
                                                   Obat = g.Key.b,
                                                   SatuanObat = g.Key.c,
                                                   Total = g.Select(m => m.Jumlah).Sum()
                                               });
                        }
                        if (!string.IsNullOrEmpty(Obat) && !string.IsNullOrEmpty(Pasien) && Date != null)
                        {
                            pengeluaranobat = (from a in db.PengeluaranObat
                                               where a.KlinikID == 1
                                               where a.TujuanKlinikID == 1
                                               where a.Obat.Nama == Obat
                                               where a.Orang.Nama == Pasien
                                               where a.Tanggal == Date
                                               group a by new { b = a.Obat.Nama, c = a.Obat.SatuanObat.Nama } into g
                                               select new PengeluaranObat_ViewModel()
                                               {
                                                   Obat = g.Key.b,
                                                   SatuanObat = g.Key.c,
                                                   Total = g.Select(m => m.Jumlah).Sum()
                                               });
                        }
                        if (!string.IsNullOrEmpty(Obat) && Year != null && !string.IsNullOrEmpty(Month))
                        {
                            var intmonth = int.Parse(Month);
                            pengeluaranobat = (from a in db.PengeluaranObat
                                               where a.KlinikID == 1
                                               where a.TujuanKlinikID == 1
                                               where a.Obat.Nama == Obat
                                               where a.Tanggal.Value.Month == intmonth
                                               where a.Tanggal.Value.Year == Year
                                               group a by new { b = a.Obat.Nama, c = a.Obat.SatuanObat.Nama } into g
                                               select new PengeluaranObat_ViewModel()
                                               {
                                                   Obat = g.Key.b,
                                                   SatuanObat = g.Key.c,
                                                   Total = g.Select(m => m.Jumlah).Sum()
                                               });
                        }
                        if (!string.IsNullOrEmpty(Pasien) && Year != null && !string.IsNullOrEmpty(Month))
                        {
                            var intmonth = int.Parse(Month);
                            pengeluaranobat = (from a in db.PengeluaranObat
                                               where a.KlinikID == 1
                                               where a.TujuanKlinikID == 1
                                               where a.Tanggal.Value.Year == Year
                                               where a.Orang.Nama == Pasien
                                               where a.Tanggal.Value.Month == intmonth
                                               group a by new { b = a.Obat.Nama, c = a.Obat.SatuanObat.Nama } into g
                                               select new PengeluaranObat_ViewModel()
                                               {
                                                   Obat = g.Key.b,
                                                   SatuanObat = g.Key.c,
                                                   Total = g.Select(m => m.Jumlah).Sum()
                                               });
                        }
                        if (!string.IsNullOrEmpty(Obat) && !string.IsNullOrEmpty(Pasien) && Year != null && !string.IsNullOrEmpty(Month))
                        {
                            var intmonth = int.Parse(Month);
                            pengeluaranobat = (from a in db.PengeluaranObat
                                               where a.KlinikID == 1
                                               where a.TujuanKlinikID == 1
                                               where a.Obat.Nama == Obat
                                               where a.Orang.Nama == Pasien
                                               where a.Tanggal.Value.Year == Year
                                               where a.Tanggal.Value.Month == intmonth
                                               group a by new { b = a.Obat.Nama, c = a.Obat.SatuanObat.Nama } into g
                                               select new PengeluaranObat_ViewModel()
                                               {
                                                   Obat = g.Key.b,
                                                   SatuanObat = g.Key.c,
                                                   Total = g.Select(m => m.Jumlah).Sum()
                                               });
                        }
                        sb.Append("<tr></tr>");
                        sb.Append("<tr>");
                        sb.Append("<td></td>");
                        sb.Append("<td></td>");
                        sb.Append("<td></td>");
                        sb.Append("<td style='border:1px solid black;background-color: yellow;'><center><b>NAMA OBAT</b></center></td>");
                        sb.Append("<td style='border:1px solid black;background-color: yellow;'><center><b>TOTAL</b></center></td>");
                        sb.Append("<td style='border:1px solid black;background-color: yellow;'><center><b>SATUAN OBAT</b></center></td>");
                        sb.Append("</tr>");
                        foreach (var result in pengeluaranobat)
                        {
                            sb.Append("<tr>");
                            sb.Append("<td></td>");
                            sb.Append("<td></td>");
                            sb.Append("<td></td>");
                            sb.Append("<td style='border:1px solid black;'><b>" + result.Obat + "</b></td>");
                            sb.Append("<td style='border:1px solid black;'>" + result.Total + "</td>");
                            sb.Append("<td style='border:1px solid black;'>" + result.SatuanObat + "</td>");
                            sb.Append("</tr>");
                        }
                    }


                }
                string sFileName = "[" + DateTime.Now + "] DATA PENGELUARAN OBAT POLIKLINIK BARANANGSIANG.xls";
                HttpContext.Response.AddHeader("content-disposition", "attachment; filename=" + sFileName);

                Response.ContentType = "application/ms-excel";
                Response.Charset = "";

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
                return File(buffer, "application/vnd.ms-excel");


                //var pengeluaranobat = (List<PengeluaranObat_ViewModel>)Session["pengeluaranobats"];
                //pengeluaranobat.ToList();
                /*pengeluaranobat.ToList<PengeluaranObat_ViewModel>();
                StringBuilder sb = new StringBuilder();
                if (pengeluaranobat != null && pengeluaranobat.Any())
                {
                    sb.Append("<table style='1px solid black; font-size:12px;'>");
                    sb.Append("<tr>");
                    sb.Append("<td colspan='5' style='width:120px', align='center'><b>DATA PERJALANAN HARIAN OBAT POLIKLINIK DMG</b></td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td style='width:120px;'><center><b>TANGGAL</b></center></td>");
                    sb.Append("<td style='width:300px;'><center><b>NAMA PASIEN</b></center></td>");
                    sb.Append("<td style='width:300px;'><center><b>NAMA OBAT</b></center></td>");
                    sb.Append("<td style='width:120px;'><center><b>JUMLAH</b></center></td>");
                    sb.Append("<td style='width:120px;'><center><b>SATUAN OBAT</b></center></td>");
                    sb.Append("</tr>");

                    foreach (var result in pengeluaranobat)
                    {
                        sb.Append("<tr>");
                        sb.Append("<td>" + result.Tanggal + "</td>");
                        sb.Append("<td>" + result.Pasien + "</td>");
                        sb.Append("<td>" + result.Obat + "</td>");
                        sb.Append("<td>" + result.Jumlah + "</td>");
                        sb.Append("<td>" + result.SatuanObat + "</td>");
                        sb.Append("</tr>");
                    }
                }
                string sFileName = "DATA PERJALANAN HARIAN OBAT POLIKLINIK DMG.xls";
                HttpContext.Response.AddHeader("content-disposition", "attachment; filename=" + sFileName);

                Response.ContentType = "application/ms-excel";
                Response.Charset = "";

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
                return File(buffer, "application/vnd.ms-excel");*/
            }

            return View(pengeluaranobat.ToList().ToPagedList(page ?? 1, 20));
        }



        // GET: /PengeluaranObatDMG/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PengeluaranObat pengeluaranobat = db.PengeluaranObat.Find(id);
            if (pengeluaranobat == null)
            {
                return HttpNotFound();
            }
            return View(pengeluaranobat);
        }

        // GET: /PengeluaranObatDMG/Create
        [Authorize(Roles = "Admin,Staf,StafDramaga")]
        public ActionResult Create()
        {
            ViewBag.KlinikID = new SelectList(db.Klinik, "ID", "Nama");
            ViewBag.PasienID = new SelectList(db.Orang, "ID", "Nama");

            ViewBag.count = 1;
            string[] temp = new string[100];
            ViewBag.Jumlah = temp;
            ViewBag.JumlahError = "";
            ViewBag.JumlahError2 = "";
            ViewBag.Obat = temp;
            ViewBag.ONama = temp;
            ViewBag.ObatError = "";

            return View();
        }

        // POST: /PengeluaranObatDMG/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin,Staf,StafDramaga")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PengeluaranObat pengeluaranobat)
        {
            if (ModelState.IsValid)
            {
                /*
                int count = Int32.Parse(Request["count"]);
                for (int kep = 1; kep <= count; kep++)
                {
                    var Jumlah = Request["Jumlah" + kep + ""].ToString();
                    var ObatID = Request["ObatID" + kep + ""].ToString();
                    if (Jumlah == null)
                        Int32.Parse(Jumlah);
                    hispengeluaranobat.Jumlah= Int32.Parse(Jumlah);
                    hispengeluaranobat.ObatID = Int32.Parse(ObatID);
                    db.PengeluaranObat.Add(hispengeluaranobat);
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
                */

                int count = Int32.Parse(Request["count"]);
                ViewBag.count = count;
                var arrayJumlah = new string[100];
                var arrayObat = new string[100];
                var arrayONama = new string[100];
                int[] arrayObatIDInt = new int[100];
                bool adaEN = false;
                bool adaEJ = false;
                bool adaEO = false;
                bool adaEJ2 = false;
                string baris1 = "", baris2 = "", baris3 = "";

                var Nama = Request["PasienID"];
                if (!String.IsNullOrEmpty(Nama))
                {
                    ViewBag.NamaError = "";
                    ViewBag.Nama = Nama;
                }
                else
                {
                    ViewBag.Nama = "";
                    adaEN = true;
                }

                for (int kepo = 1; kepo <= count; kepo++)
                {
                    var Jumlah = Request["Jumlah" + kepo + ""];
                    if (!String.IsNullOrEmpty(Jumlah))
                    {
                        ViewBag.JumlahError = "";
                        arrayJumlah[kepo] = Jumlah;
                    }
                    else
                    {
                        arrayJumlah[kepo] = "";
                        adaEJ = true;
                        if (baris1 == "")
                            baris1 += kepo.ToString();
                        else
                            baris1 += ", " + kepo.ToString();
                    }

                    var ObatID = Request["ObatID" + kepo + ""];
                    if (!String.IsNullOrEmpty(ObatID) && ObatID != "--Pilih Obat--")
                    {
                        //@ViewBag.JumlahError = "";
                        string[] words = ObatID.Split('&');
                        arrayObatIDInt[kepo] = int.Parse(words[0]);
                        //ViewBag.word = ObatID;
                        arrayONama[kepo] = words[1];
                        arrayObat[kepo] = ObatID;

                        string[] stok = words[1].Split(':', '[', ']');
                        //ViewBag.Coba = stok[2];
                        int now = 0;
                        if (!String.IsNullOrEmpty(Jumlah))
                        {
                            now = int.Parse(Jumlah);
                        }
                        if (now > int.Parse(stok[2]))
                        {
                            adaEJ2 = true;
                            if (baris3 == "")
                                baris3 += kepo.ToString();
                            else
                                baris3 += ", " + kepo.ToString();
                        }
                    }
                    else
                    {
                        //ViewBag.JumlahError = "Form nama obat tidak boleh kosong";
                        arrayObat[kepo] = "";
                        adaEO = true;
                        if (baris2 == "")
                            baris2 += kepo.ToString();
                        else
                            baris2 += ", " + kepo.ToString();
                    }
                }

                if (adaEJ) ViewBag.JumlahError = "Jumlah obat harus diisi pada baris " + baris1 + ".";
                if (adaEO) ViewBag.ObatError = "Nama obat harus diisi pada baris " + baris2 + ".";
                if (adaEJ2) ViewBag.JumlahError2 = "Jumlah pengeluaran obat pada baris " + baris3 + " melebihi stok yang tersedia.";
                if (adaEN) ViewBag.NamaError = "Nama pasien harus diisi.";

                if (!adaEJ && !adaEO && !adaEJ2 && !adaEN)
                {
                    for (int kepo = 1; kepo <= count; kepo++)
                    {
                        pengeluaranobat.Jumlah = Int32.Parse(arrayJumlah[kepo]);
                        pengeluaranobat.ObatID = arrayObatIDInt[kepo];
                        db.PengeluaranObat.Add(pengeluaranobat);
                        db.SaveChanges();
                    }
                    return RedirectToAction("Index");
                }
                ViewBag.Jumlah = arrayJumlah;
                ViewBag.ONama = arrayONama;
                ViewBag.Obat = arrayObat;

                ViewBag.KlinikID = new SelectList(db.Klinik, "ID", "Nama", pengeluaranobat.KlinikID);

                return View(pengeluaranobat);
            }
            else
            {
                int count = Int32.Parse(Request["count"]);
                ViewBag.count = count;
                var arrayJumlah = new string[100];
                var arrayObat = new string[100];
                var arrayONama = new string[100];
                int[] arrayObatIDInt = new int[100];
                bool adaEN = false;
                bool adaEJ = false;
                bool adaEO = false;
                bool adaEJ2 = false;
                string baris1 = "", baris2 = "", baris3 = "";

                var Nama = Request["PasienID"];
                if (!String.IsNullOrEmpty(Nama))
                {
                    ViewBag.NamaError = "";
                    ViewBag.Nama = Nama;
                }
                else
                {
                    ViewBag.Nama = "";
                    adaEN = true;
                }

                for (int kepo = 1; kepo <= count; kepo++)
                {
                    var Jumlah = Request["Jumlah" + kepo + ""];
                    if (!String.IsNullOrEmpty(Jumlah))
                    {
                        ViewBag.JumlahError = "";
                        arrayJumlah[kepo] = Jumlah;
                    }
                    else
                    {
                        arrayJumlah[kepo] = "";
                        adaEJ = true;
                        if (baris1 == "")
                            baris1 += kepo.ToString();
                        else
                            baris1 += ", " + kepo.ToString();
                    }

                    var ObatID = Request["ObatID" + kepo + ""];
                    if (!String.IsNullOrEmpty(ObatID) && ObatID != "--Pilih Obat--")
                    {
                        //@ViewBag.JumlahError = "";
                        string[] words = ObatID.Split('&');
                        arrayObatIDInt[kepo] = int.Parse(words[0]);
                        //ViewBag.word = ObatID;
                        arrayONama[kepo] = words[1];
                        arrayObat[kepo] = ObatID;

                        string[] stok = words[1].Split(':', '[', ']');
                        //ViewBag.Coba = stok[2];
                        int now = 0;
                        if (!String.IsNullOrEmpty(Jumlah))
                        {
                            now = int.Parse(Jumlah);
                        }
                        if (now > int.Parse(stok[2]))
                        {
                            adaEJ2 = true;
                            if (baris3 == "")
                                baris3 += kepo.ToString();
                            else
                                baris3 += ", " + kepo.ToString();
                        }
                    }
                    else
                    {
                        //ViewBag.JumlahError = "Form nama obat tidak boleh kosong";
                        arrayObat[kepo] = "";
                        adaEO = true;
                        if (baris2 == "")
                            baris2 += kepo.ToString();
                        else
                            baris2 += ", " + kepo.ToString();
                    }
                }

                if (adaEJ) ViewBag.JumlahError = "Jumlah obat harus diisi pada baris " + baris1 + ".";
                if (adaEO) ViewBag.ObatError = "Nama obat harus diisi pada baris " + baris2 + ".";
                if (adaEJ2) ViewBag.JumlahError2 = "Jumlah pengeluaran obat pada baris " + baris3 + " melebihi stok yang tersedia.";
                if (adaEN) ViewBag.NamaError = "Nama pasien harus diisi.";

                ViewBag.Jumlah = arrayJumlah;
                ViewBag.ONama = arrayONama;
                ViewBag.Obat = arrayObat;

                ViewBag.KlinikID = new SelectList(db.Klinik, "ID", "Nama", pengeluaranobat.KlinikID);

                //return RedirectToAction("Index");
                return View(pengeluaranobat);
            }
        }

        // GET: /PengeluaranObatDMG/Edit/5
        [Authorize(Roles = "Admin,Staf,StafDramaga")]
        public ActionResult Edit(int? id, bool? E, bool? E1, bool? E2, bool? E3, bool? E4, string S)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PengeluaranObat pengeluaranobat = db.PengeluaranObat.Find(id);
            if (pengeluaranobat == null)
            {
                return HttpNotFound();
            }
            var pasien = (from r in db.Orang
                          from i in db.RekamMedik
                          where r.ID == i.PasienID
                          select new { Nama = r.Nama, ID = r.ID }).Distinct();
            var obat = from r in db.StokObat where r.KlinikID == 1 where r.Stok > 0 select new { ID = r.Obat.ID, Nama = r.Obat.Nama, Stok = r.Stok };
            ViewBag.ObatID = new SelectList(obat, "ID", "Nama", pengeluaranobat.ObatID);
            ViewBag.PasienID = new SelectList(pasien, "ID", "Nama", pengeluaranobat.PasienID);

            //simpan jumlah stok sblm diedit
            ViewBag.jmlsblmedit = pengeluaranobat.Jumlah;

            //error handling
            if (E == true)
            {
                ViewBag.E = true;
            }
            if (E1 == true)
            {
                ViewBag.E1 = true;
            }
            if (E2 == true)
            {
                ViewBag.E2 = true;
            }
            if (E3 == true)
            {
                ViewBag.E3 = true;
            }
            if (E4 == true)
            {
                ViewBag.stoksaat = S;
                ViewBag.E4 = true;
            }

            return View(pengeluaranobat);
        }

        // POST: /PengeluaranObatDMG/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin,Staf,StafDramaga")]
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(int? id, PengeluaranObat pengeluaranobat, string jmlsblmedit)
        {
            ViewBag.E = false;

            //tanggal tdk blh kosong
            if (pengeluaranobat.Tanggal == null)
            {
                ViewBag.E = true;
                ViewBag.E1 = true;
            }

            //jumlah obat tdk blh kosong
            if (pengeluaranobat.Jumlah == null)
            {
                ViewBag.E = true;
                ViewBag.E2 = true;
            }

            //jumlah obat harus bilangan positif
            string jumlah = pengeluaranobat.Jumlah.ToString();
            if (pengeluaranobat.Jumlah != null)
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(jumlah, "^[0-9]+$"))
                {
                    ViewBag.E = true;
                    ViewBag.E3 = true;
                }
            }

            //jumlah obat tdk blh lebih dari stok saat edit
            var stoksblmedit = (from i in db.StokObat
                                where i.ObatID == pengeluaranobat.ObatID
                                where i.KlinikID == 1
                                select i.Stok).FirstOrDefault().ToString();
            var intstoksblmedit = int.Parse(stoksblmedit);
            var intjmlsblmedit = int.Parse(jmlsblmedit);
            var stoksaatedit = intstoksblmedit + intjmlsblmedit;
            if (pengeluaranobat.Jumlah != null)
            {
                var jmlsaatedit = pengeluaranobat.Jumlah.ToString();
                var intjmlsaatedit = int.Parse(jmlsaatedit);

                if (intjmlsaatedit > stoksaatedit)
                {
                    ViewBag.E = true;
                    ViewBag.E4 = true;
                    ViewBag.stoksaatedit = stoksaatedit;
                }
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var pengeluaranupdate = db.PengeluaranObat.Where(i => i.ID == id).Single();
            if (TryUpdateModel(pengeluaranupdate, "",
                new string[] { "ID", "ObatID", "Jumlah", "Tanggal", "HET", "HargaAktual", "PenyediaObatID" }) && !ViewBag.E)
            {
                try
                {
                    db.Entry(pengeluaranupdate).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException)
                {
                    //log the error, uncommend dex and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try Again, and if the problem persists, see your system administrator.");
                }
            }
            //kalau gagal, kembali ke action edit
            return RedirectToAction("Edit", new { E = ViewBag.E, E1 = ViewBag.E1, E2 = ViewBag.E2, E3 = ViewBag.E3, E4 = ViewBag.E4, S = ViewBag.stoksaatedit });
        }

        // GET: /PengeluaranObatDMG/Delete/5
        [Authorize(Roles = "Admin,Staf,StafDramaga")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PengeluaranObat pengeluaranobat = db.PengeluaranObat.Find(id);
            if (pengeluaranobat == null)
            {
                return HttpNotFound();
            }
            return PartialView("_Delete", pengeluaranobat);
        }

        // POST: /PengeluaranObatDMG/Delete/5
        [Authorize(Roles = "Admin,Staf,StafDramaga")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PengeluaranObat pengeluaranobat = db.PengeluaranObat.Find(id);
            db.PengeluaranObat.Remove(pengeluaranobat);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
/*[HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PengeluaranObat pengeluaranobat)
        {
            if (ModelState.IsValid)
            {
                db.Entry(pengeluaranobat).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.KlinikID = new SelectList(db.Klinik, "ID", "Nama", pengeluaranobat.KlinikID);
            ViewBag.ObatID = new SelectList(db.Obat, "ID", "Nama", pengeluaranobat.ObatID);
            ViewBag.PasienID = new SelectList(db.Orang, "ID", "Nama", pengeluaranobat.PasienID);
            return View(pengeluaranobat);
        }*/
/*public ActionResult MultipleCommand(string Obat, string Pasien, string Month, DateTime? Date, int? Year, string Sortby, int? page, string Command)
        {
            var pengeluaranobat = from a in db.PengeluaranObat.Include(h => h.Orang).Include(h => h.Obat).Include(h => h.Klinik)
                                  where a.KlinikID == 1
                                  select new PengeluaranObat_ViewModel()
                                  {
                                      ID = a.ID,
                                      Tanggal = a.Tanggal,
                                      PasienID = a.PasienID,
                                      ObatID = a.ObatID,
                                      KlinikID = a.KlinikID,
                                      Jumlah = a.Jumlah,
                                      Pasien = a.Orang.Nama,
                                      Obat = a.Obat.Nama,
                                      SatuanObat = a.Obat.SatuanObat.Nama,
                                      Klinik = a.Klinik.Nama
                                  };

            //filtering
            var SelectListYear = ((from year in db.PengeluaranObat orderby year.Tanggal.Value.Year select year.Tanggal.Value.Year)).Distinct().ToList();
            ViewBag.Year = new SelectList(SelectListYear);

            if (!String.IsNullOrEmpty(Obat))
            {
                pengeluaranobat = pengeluaranobat.Where(a => a.Obat.Contains(Obat));
            }

            if (!String.IsNullOrEmpty(Pasien))
            {
                pengeluaranobat = pengeluaranobat.Where(b => b.Pasien.Contains(Pasien));
            }

            if (Date != null)
            {
                pengeluaranobat = pengeluaranobat.Where(c => c.Tanggal == Date);
            }

            if (Year != null)
            {
                pengeluaranobat = pengeluaranobat.Where(d => d.Tanggal.Value.Year == Year);
            }

            if (!String.IsNullOrEmpty(Month))
            {
                int MonthInt;
                switch (Month)
                {
                    case "Januari":
                        MonthInt = 1;
                        break;
                    case "Februari":
                        MonthInt = 2;
                        break;
                    case "Maret":
                        MonthInt = 3;
                        break;
                    case "April":
                        MonthInt = 4;
                        break;
                    case "Mei":
                        MonthInt = 5;
                        break;
                    case "Juni":
                        MonthInt = 6;
                        break;
                    case "Juli":
                        MonthInt = 7;
                        break;
                    case "Agustus":
                        MonthInt = 8;
                        break;
                    case "September":
                        MonthInt = 9;
                        break;
                    case "Oktober":
                        MonthInt = 10;
                        break;
                    case "November":
                        MonthInt = 11;
                        break;
                    case "Desember":
                        MonthInt = 12;
                        break;
                    default:
                        MonthInt = 0;
                        break;
                }
                pengeluaranobat = pengeluaranobat.Where(e => e.Tanggal.Value.Month == MonthInt);
            }

            if (Command == "Filter")
            {
                //sorting
                ViewBag.SortTanggalParameter = Sortby == "Tanggal" ? "Tanggal Desc" : "Tanggal";
                ViewBag.SortObatParameter = Sortby == "Obat" ? "Obat Desc" : "Obat";
                ViewBag.SortPasienParameter = Sortby == "Pasien" ? "Pasien Desc" : "Pasien";

                switch (Sortby)
                {
                    case "Tanggal":
                        pengeluaranobat = pengeluaranobat.OrderBy(a => a.Tanggal);
                        break;
                    case "Tanggal Desc":
                        pengeluaranobat = pengeluaranobat.OrderByDescending(a => a.Tanggal);
                        break;
                    case "Obat":
                        pengeluaranobat = pengeluaranobat.OrderBy(b => b.Obat);
                        break;
                    case "Obat Desc":
                        pengeluaranobat = pengeluaranobat.OrderByDescending(b => b.Obat);
                        break;
                    case "Pasien":
                        pengeluaranobat = pengeluaranobat.OrderBy(c => c.Pasien);
                        break;
                    case "Pasien Desc":
                        pengeluaranobat = pengeluaranobat.OrderByDescending(c => c.Pasien);
                        break;
                    default:
                        pengeluaranobat = pengeluaranobat.OrderByDescending(g => g.ID);
                        break;
                }

                //export
                Session["pengeluaranobats"] = pengeluaranobat.ToList<PengeluaranObat_ViewModel>();

                return View(pengeluaranobat.ToList().ToPagedList(page ?? 1, 2));

            }
            if (Command == "Export")
            {
                //var pengeluaranobat = (List<PengeluaranObat_ViewModel>)Session["pengeluaranobats"];
                //pengeluaranobat.ToList();
                pengeluaranobat.ToList<PengeluaranObat_ViewModel>();
                StringBuilder sb = new StringBuilder();
                if (pengeluaranobat != null && pengeluaranobat.Any())
                {
                    sb.Append("<table style='1px solid black; font-size:12px;'>");
                    sb.Append("<tr>");
                    sb.Append("<td colspan='5' style='width:120px', align='center'><b>DATA PERJALANAN HARIAN OBAT POLIKLINIK DMG</b></td>");
                    sb.Append("</tr>");
                    sb.Append("<tr>");
                    sb.Append("<td style='width:120px;'><center><b>TANGGAL</b></center></td>");
                    sb.Append("<td style='width:300px;'><center><b>NAMA PASIEN</b></center></td>");
                    sb.Append("<td style='width:300px;'><center><b>NAMA OBAT</b></center></td>");
                    sb.Append("<td style='width:120px;'><center><b>JUMLAH</b></center></td>");
                    sb.Append("<td style='width:120px;'><center><b>SATUAN OBAT</b></center></td>");
                    sb.Append("</tr>");

                    foreach (var result in pengeluaranobat)
                    {
                        sb.Append("<tr>");
                        sb.Append("<td>" + result.Tanggal + "</td>");
                        sb.Append("<td>" + result.Pasien + "</td>");
                        sb.Append("<td>" + result.Obat + "</td>");
                        sb.Append("<td>" + result.Jumlah + "</td>");
                        sb.Append("<td>" + result.SatuanObat + "</td>");
                        sb.Append("</tr>");
                    }
                }
                string sFileName = "DATA PERJALANAN HARIAN OBAT POLIKLINIK DMG.xls";
                HttpContext.Response.AddHeader("content-disposition", "attachment; filename=" + sFileName);

                Response.ContentType = "application/ms-excel";
                Response.Charset = "";

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
                return File(buffer, "application/vnd.ms-excel");
            }
            return RedirectToAction("Index");
        }*/
