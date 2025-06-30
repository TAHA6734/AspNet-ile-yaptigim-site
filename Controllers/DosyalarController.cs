using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http; // IFormFile için
using Oracle.ManagedDataAccess.Client;
using ClosedXML.Excel;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.IO;

// iText7 için gerekli usingler:
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;

namespace Site.Controllers
{
    public class DosyalarController : Controller
    {
        private string conStr = "User Id=system;Password=1234;Data Source=localhost:1521/XEPDB1";

        // Sayfayı açar, filtreli listeyi getirir
        [HttpGet]
        public IActionResult Yukle(string isim = null, string soyisim = null, int? yil_sayisi = null, int? maas = null)
        {
            ViewBag.Tablo = VeriGetir(isim, soyisim, yil_sayisi, maas);

            ViewBag.Isim = isim;
            ViewBag.Soyisim = soyisim;
            ViewBag.YilSayisi = yil_sayisi;
            ViewBag.Maas = maas;

            return View();
        }

        // Excel dosyasından toplu veri yükler
        [HttpPost]
        public IActionResult Yukle(IFormFile excelDosyasi)
        {
            if (excelDosyasi != null && excelDosyasi.Length > 0)
            {
                try
                {
                    using var workbook = new XLWorkbook(excelDosyasi.OpenReadStream());
                    var worksheet = workbook.Worksheet(1);
                    DataTable table = new DataTable();

                    var firstRow = worksheet.FirstRowUsed();
                    if (firstRow == null)
                    {
                        ViewBag.Mesaj = "Excel dosyasında veri bulunamadı.";
                        ViewBag.Tablo = VeriGetir();
                        return View();
                    }

                    var expectedHeaders = new[] { "isim", "soyisim", "yil_sayisi", "maas" };
                    var actualHeaders = firstRow.Cells().Select(c => c.GetString().Trim().ToLower()).ToList();

                    if (!expectedHeaders.SequenceEqual(actualHeaders))
                    {
                        ViewBag.Mesaj = "Excel başlıkları hatalı. Sırasıyla 'isim, soyisim, yil_sayisi, maas' olmalı.";
                        ViewBag.Tablo = VeriGetir();
                        return View();
                    }

                    foreach (var cell in firstRow.Cells())
                        table.Columns.Add(cell.GetString());

                    foreach (var row in worksheet.RowsUsed().Skip(1))
                    {
                        var dataRow = table.NewRow();
                        int i = 0;
                        foreach (var cell in row.Cells(1, table.Columns.Count))
                            dataRow[i++] = cell.GetString();
                        table.Rows.Add(dataRow);
                    }

                    using var con = new OracleConnection(conStr);
                    con.Open();

                    foreach (DataRow row in table.Rows)
                    {
                        string isim = row[0]?.ToString()?.Trim() ?? "";
                        string soyisim = row[1]?.ToString()?.Trim() ?? "";

                        if (string.IsNullOrWhiteSpace(isim) || string.IsNullOrWhiteSpace(soyisim))
                        {
                            ViewBag.Mesaj = "İsim ve Soyisim boş olamaz.";
                            ViewBag.Tablo = VeriGetir();
                            return View();
                        }

                        if (!int.TryParse(row[2]?.ToString(), out int yil) || yil < 0)
                        {
                            ViewBag.Mesaj = $"'{isim} {soyisim}' kaydı için yıl değeri geçersiz.";
                            ViewBag.Tablo = VeriGetir();
                            return View();
                        }

                        if (!int.TryParse(row[3]?.ToString(), out int maas) || maas < 0)
                        {
                            ViewBag.Mesaj = $"'{isim} {soyisim}' kaydı için maaş değeri geçersiz.";
                            ViewBag.Tablo = VeriGetir();
                            return View();
                        }

                        string query = @"INSERT INTO calisan_bilgileri (isim, soyisim, yil_sayisi, maas)
                                         VALUES (:isim, :soyisim, :yil, :maas)";
                        using var cmd = new OracleCommand(query, con);
                        cmd.Parameters.Add(new OracleParameter("isim", isim));
                        cmd.Parameters.Add(new OracleParameter("soyisim", soyisim));
                        cmd.Parameters.Add(new OracleParameter("yil", yil));
                        cmd.Parameters.Add(new OracleParameter("maas", maas));
                        cmd.ExecuteNonQuery();
                    }

                    ViewBag.Mesaj = "Başarıyla eklendi!";
                }
                catch (System.Exception ex)
                {
                    ViewBag.Mesaj = "Hata: " + ex.Message;
                }
            }
            else
            {
                ViewBag.Mesaj = "Lütfen bir dosya seçin.";
            }

            ViewBag.Tablo = VeriGetir();
            return View();
        }

        // Tekil çalışan ekleme
        [HttpPost]
        public IActionResult Ekle(string isim, string soyisim, int yil_sayisi, int maas)
        {
            if (string.IsNullOrWhiteSpace(isim) || string.IsNullOrWhiteSpace(soyisim))
            {
                ViewBag.Mesaj = "İsim ve Soyisim boş olamaz.";
                ViewBag.Tablo = VeriGetir();
                return View("Yukle");
            }
            if (yil_sayisi < 0 || maas < 0)
            {
                ViewBag.Mesaj = "Yıl sayısı ve maaş negatif olamaz.";
                ViewBag.Tablo = VeriGetir();
                return View("Yukle");
            }

            try
            {
                using var con = new OracleConnection(conStr);
                con.Open();

                string query = @"INSERT INTO calisan_bilgileri (isim, soyisim, yil_sayisi, maas) 
                                 VALUES (:isim, :soyisim, :yil, :maas)";
                using var cmd = new OracleCommand(query, con);
                cmd.Parameters.Add(new OracleParameter("isim", isim));
                cmd.Parameters.Add(new OracleParameter("soyisim", soyisim));
                cmd.Parameters.Add(new OracleParameter("yil", yil_sayisi));
                cmd.Parameters.Add(new OracleParameter("maas", maas));
                cmd.ExecuteNonQuery();

                ViewBag.Mesaj = "Yeni çalışan başarıyla eklendi.";
            }
            catch (System.Exception ex)
            {
                ViewBag.Mesaj = "Ekleme hatası: " + ex.Message;
            }

            ViewBag.Tablo = VeriGetir();
            return View("Yukle");
        }

        // Çalışan silme
        [HttpPost]
        public IActionResult Sil(int id)
        {
            try
            {
                using var con = new OracleConnection(conStr);
                con.Open();
                string query = "DELETE FROM calisan_bilgileri WHERE id = :id";
                using var cmd = new OracleCommand(query, con);
                cmd.Parameters.Add(new OracleParameter("id", id));
                int etkilenen = cmd.ExecuteNonQuery();

                ViewBag.Mesaj = etkilenen > 0 ? "Kayıt başarıyla silindi." : "Kayıt bulunamadı.";
            }
            catch (System.Exception ex)
            {
                ViewBag.Mesaj = "Silme işlemi sırasında hata: " + ex.Message;
            }

            ViewBag.Tablo = VeriGetir();
            return View("Yukle");
        }

        // Çalışan güncelleme
        [HttpPost]
        public IActionResult Guncelle(int id, string isim, string soyisim, int yil_sayisi, int maas)
        {
            if (string.IsNullOrWhiteSpace(isim) || string.IsNullOrWhiteSpace(soyisim))
            {
                ViewBag.Mesaj = "İsim ve soyisim boş olamaz.";
                ViewBag.Tablo = VeriGetir();
                return View("Yukle");
            }
            if (yil_sayisi < 0 || maas < 0)
            {
                ViewBag.Mesaj = "Yıl sayısı ve maaş negatif olamaz.";
                ViewBag.Tablo = VeriGetir();
                return View("Yukle");
            }

            try
            {
                using var con = new OracleConnection(conStr);
                con.Open();
                string query = @"UPDATE calisan_bilgileri 
                                 SET isim = :isim, soyisim = :soyisim, yil_sayisi = :yil, maas = :maas 
                                 WHERE id = :id";
                using var cmd = new OracleCommand(query, con);
                cmd.Parameters.Add(new OracleParameter("isim", isim));
                cmd.Parameters.Add(new OracleParameter("soyisim", soyisim));
                cmd.Parameters.Add(new OracleParameter("yil", yil_sayisi));
                cmd.Parameters.Add(new OracleParameter("maas", maas));
                cmd.Parameters.Add(new OracleParameter("id", id));

                int etkilenen = cmd.ExecuteNonQuery();

                ViewBag.Mesaj = etkilenen > 0 ? "Kayıt başarıyla güncellendi." : "Kayıt bulunamadı.";
            }
            catch (System.Exception ex)
            {
                ViewBag.Mesaj = "Güncelleme sırasında hata: " + ex.Message;
            }

            ViewBag.Tablo = VeriGetir();
            return View("Yukle");
        }

        // Veritabanından filtreli liste çekme
        private DataTable VeriGetir(string isim = null, string soyisim = null, int? yil_sayisi = null, int? maas = null)
        {
            DataTable dt = new DataTable();
            try
            {
                using var con = new OracleConnection(conStr);
                con.Open();

                string query = "SELECT * FROM calisan_bilgileri WHERE 1=1";
                var parameters = new List<OracleParameter>();

                if (!string.IsNullOrWhiteSpace(isim))
                {
                    query += " AND LOWER(isim) LIKE :isim";
                    parameters.Add(new OracleParameter("isim", "%" + isim.ToLower() + "%"));
                }
                if (!string.IsNullOrWhiteSpace(soyisim))
                {
                    query += " AND LOWER(soyisim) LIKE :soyisim";
                    parameters.Add(new OracleParameter("soyisim", "%" + soyisim.ToLower() + "%"));
                }
                if (yil_sayisi.HasValue)
                {
                    query += " AND yil_sayisi = :yil_sayisi";
                    parameters.Add(new OracleParameter("yil_sayisi", yil_sayisi.Value));
                }
                if (maas.HasValue)
                {
                    query += " AND maas = :maas";
                    parameters.Add(new OracleParameter("maas", maas.Value));
                }

                using var cmd = new OracleCommand(query, con);
                cmd.Parameters.AddRange(parameters.ToArray());

                using var da = new OracleDataAdapter(cmd);
                da.Fill(dt);
            }
            catch
            {
                // Hata olursa boş DataTable döner
            }
            return dt;
        }

        // PDF oluşturup indirir (filtreli)
        [HttpGet]
        public IActionResult PdfCikar(string isim = null, string soyisim = null, int? yil_sayisi = null, int? maas = null)
        {
            var dt = VeriGetir(isim, soyisim, yil_sayisi, maas);

            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Font ayarlama (iText7’de SetBold() yerine font atama önerilir)
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            document.Add(new Paragraph("Çalışanlar Listesi")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFont(boldFont)
                .SetFontSize(18)
                .SetMarginBottom(20));

            // Tablo başlat (sütun sayısı kadar)
            Table table = new Table(dt.Columns.Count, true);

            // Başlıklar
            foreach (DataColumn col in dt.Columns)
            {
                table.AddHeaderCell(new Cell().Add(new Paragraph(col.ColumnName.ToUpper()).SetFont(boldFont)));
            }

            // Satırlar
            foreach (DataRow row in dt.Rows)
            {
                foreach (var cell in row.ItemArray)
                {
                    table.AddCell(new Cell().Add(new Paragraph(cell?.ToString() ?? "").SetFont(font)));
                }
            }

            document.Add(table);
            document.Close();

            byte[] pdfBytes = ms.ToArray();

            return File(pdfBytes, "application/pdf", "Calisanlar.pdf");
        }
    }
}
