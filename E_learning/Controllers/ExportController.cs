using E_learning.Models;
using E_learning.Models.E_learning.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using OfficeOpenXml;
using System.Security.Claims;

namespace E_learning.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ExportController : ControllerBase
    {
        private readonly ElearningDbContext _context;

        public ExportController(ElearningDbContext context)
        {
            _context = context;
            // Définir le contexte de la licence pour EPPlus.
            // La propriété ExcelPackage.License est en lecture seule.
            // Vous devez appeler l'une de ses méthodes (SetCommercial, SetNonCommercialPersonal, etc.).
            // J'ai mis "E-learning App" comme nom, vous pouvez le changer.
            ExcelPackage.License.SetNonCommercialPersonal("E-learning App");
        }

        // GET: api/Export/pdf/{idFichier}
        /// <summary>
        /// Exporte le contenu d'un fichier en tant que PDF.
        /// </summary>
        /// <param name="idFichier">L'identifiant du fichier à exporter.</param>
        /// <returns>Le fichier PDF à télécharger.</returns>
        [HttpGet("pdf/{idFichier}")]
        [Authorize(Roles = "Administrateur, Formateur, Apprenant")]
        public async Task<IActionResult> ExportPdf(string idFichier)
        {
            var fichier = await _context.Fichiers.FirstOrDefaultAsync(f => f.IdFichier == idFichier);
            if (fichier == null)
            {
                return NotFound("Fichier non trouvé.");
            }

            // Récupérer le résultat associé et inclure les données de l'apprenant pour le PDF
            var resultat = await _context.Resultats
                                         .Include(r => r.Apprenant) // Utiliser Include pour charger les données de l'apprenant
                                         .FirstOrDefaultAsync(r => r.FichierId == idFichier);

            // --- MISE À JOUR DU CONTRÔLE D'ACCÈS ---
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isAdministrator = User.IsInRole("Administrateur");
            bool isFormateur = User.IsInRole("Formateur");

            if (!isAdministrator && !isFormateur && (resultat == null || resultat.IdApprenant != currentUserId))
            {
                return Forbid();
            }
            // --- FIN DU NOUVEAU CONTRÔLE ---

            // Créer un nouveau document PDF
            PdfDocument document = new PdfDocument();
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            XFont font = new XFont("Verdana", 12, XFontStyle.Regular);

            int yPoint = 50;
            gfx.DrawString($"Rapport de résultat", font, XBrushes.Black, new XRect(0, yPoint, page.Width, page.Height), XStringFormats.TopCenter);
            yPoint += 30;

            if (resultat != null && resultat.Apprenant != null)
            {
                // Utiliser le nom et d'autres données de l'apprenant au lieu de l'ID
                gfx.DrawString($"Apprenant: {resultat.Apprenant.Nom} {resultat.Apprenant.Prenom}", font, XBrushes.Black, new XRect(40, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                yPoint += 20;
                gfx.DrawString($"Email: {resultat.Apprenant.Email}", font, XBrushes.Black, new XRect(40, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                yPoint += 20;
                gfx.DrawString($"Note: {resultat.Note}", font, XBrushes.Black, new XRect(40, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                yPoint += 20;
                gfx.DrawString($"Résultat Final: {resultat.ResultatFinal}", font, XBrushes.Black, new XRect(40, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                yPoint += 20;
                gfx.DrawString($"Remarque: {resultat.Remarque}", font, XBrushes.Black, new XRect(40, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                yPoint += 20;
            }
            else if (resultat != null)
            {
                // Gérer le cas où le résultat existe, mais l'apprenant n'est pas trouvé
                gfx.DrawString($"Apprenant ID: {resultat.IdApprenant}", font, XBrushes.Black, new XRect(40, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                yPoint += 20;
                gfx.DrawString($"Note: {resultat.Note}", font, XBrushes.Black, new XRect(40, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                yPoint += 20;
                gfx.DrawString($"Résultat Final: {resultat.ResultatFinal}", font, XBrushes.Black, new XRect(40, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                yPoint += 20;
                gfx.DrawString($"Remarque: {resultat.Remarque}", font, XBrushes.Black, new XRect(40, yPoint, page.Width, page.Height), XStringFormats.TopLeft);
                yPoint += 20;
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fichier.CheminFichier);

            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    var fileContent = System.IO.File.ReadAllText(filePath);
                    gfx.DrawString("Contenu du fichier:\n\n" + fileContent, font, XBrushes.Black, new XRect(40, yPoint, page.Width - 80, page.Height), XStringFormats.TopLeft);
                }
                catch (Exception ex)
                {
                    gfx.DrawString("Erreur: Impossible de lire le contenu du fichier. Il ne s'agit pas d'un fichier texte.", font, XBrushes.Red, new XRect(40, yPoint, page.Width - 80, page.Height), XStringFormats.TopLeft);
                }
            }
            else
            {
                return NotFound("Fichier physique non trouvé sur le serveur.");
            }

            using (MemoryStream stream = new MemoryStream())
            {
                document.Save(stream);
                return File(stream.ToArray(), "application/pdf", $"Resultat_{resultat?.Apprenant?.Nom} {resultat?.Apprenant?.Prenom}.pdf");
            }
        }

        // GET: api/Export/excel/{idFichier}
        /// <summary>
        /// Exporte le contenu d'un fichier en tant que fichier Excel.
        /// </summary>
        /// <param name="idFichier">L'identifiant du fichier à exporter.</param>
        /// <returns>Le fichier Excel à télécharger.</returns>
        [HttpGet("excel/{idFichier}")]
        [Authorize(Roles = "Administrateur, Formateur, Apprenant")]
        public async Task<IActionResult> ExportExcel(string idFichier)
        {
            var fichier = await _context.Fichiers.FirstOrDefaultAsync(f => f.IdFichier == idFichier);
            if (fichier == null)
            {
                return NotFound("Fichier non trouvé.");
            }

            // Récupérer le résultat associé et inclure les données de l'apprenant pour l'Excel
            var resultat = await _context.Resultats
                                         .Include(r => r.Apprenant) // Utiliser Include pour charger les données de l'apprenant
                                         .FirstOrDefaultAsync(r => r.FichierId == idFichier);

            // --- MISE À JOUR DU CONTRÔLE D'ACCÈS ---
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isAdministrator = User.IsInRole("Administrateur");
            bool isFormateur = User.IsInRole("Formateur");

            if (!isAdministrator && !isFormateur && (resultat == null || resultat.IdApprenant != currentUserId))
            {
                return Forbid();
            }
            // --- FIN DU NOUVEAU CONTRÔLE ---

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Résultats");

                // Ajouter des en-têtes de colonne
                worksheet.Cells[1, 1].Value = "Nom Apprenant";
                worksheet.Cells[1, 2].Value = "Prénom Apprenant";
                worksheet.Cells[1, 3].Value = "Email Apprenant";
                worksheet.Cells[1, 4].Value = "Note";
                worksheet.Cells[1, 5].Value = "Remarque";
                worksheet.Cells[1, 6].Value = "Resultat Final";
                worksheet.Cells[1, 7].Value = "Nom Fichier";
                worksheet.Cells[1, 8].Value = "Chemin Fichier";

                if (resultat != null && resultat.Apprenant != null)
                {
                    // Utiliser le nom et d'autres données de l'apprenant
                    worksheet.Cells[2, 1].Value = resultat.Apprenant.Nom;
                    worksheet.Cells[2, 2].Value = resultat.Apprenant.Prenom;
                    worksheet.Cells[2, 3].Value = resultat.Apprenant.Email;
                    worksheet.Cells[2, 4].Value = resultat.Note;
                    worksheet.Cells[2, 5].Value = resultat.Remarque;
                    worksheet.Cells[2, 6].Value = resultat.ResultatFinal;
                    worksheet.Cells[2, 7].Value = fichier.Nom;
                    worksheet.Cells[2, 8].Value = fichier.CheminFichier;
                }
                else if (resultat != null)
                {
                    // Gérer le cas où l'apprenant n'est pas trouvé
                    worksheet.Cells[2, 1].Value = resultat.IdApprenant;
                    worksheet.Cells[2, 4].Value = resultat.Note;
                    worksheet.Cells[2, 5].Value = resultat.Remarque;
                    worksheet.Cells[2, 6].Value = resultat.ResultatFinal;
                    worksheet.Cells[2, 7].Value = fichier.Nom;
                    worksheet.Cells[2, 8].Value = fichier.CheminFichier;
                }

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fichier.CheminFichier);
                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        var fileContent = System.IO.File.ReadAllText(filePath);
                        worksheet.Cells[3, 1].Value = "Contenu du Fichier";
                        worksheet.Cells[3, 2].Value = fileContent;
                    }
                    catch (Exception ex)
                    {
                        worksheet.Cells[3, 1].Value = "Contenu du Fichier";
                        worksheet.Cells[3, 2].Value = "Erreur: Impossible de lire le contenu. Il ne s'agit pas d'un fichier texte.";
                    }
                }
                else
                {
                    return NotFound("Fichier physique non trouvé sur le serveur.");
                }

                // Convertir le package en un tableau d'octets
                var stream = new MemoryStream(package.GetAsByteArray());

                // Retourner le fichier
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Resultat_{resultat?.Apprenant?.Nom} {resultat?.Apprenant?.Prenom}.xlsx");
            }
        }
    }
}
