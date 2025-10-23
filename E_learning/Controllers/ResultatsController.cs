using E_learning.Models;
using E_learning.Models.E_learning.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace E_learning.Controllers
{
    // DTO pour la création d'un résultat avec un fichier
    public class ResultatCreateDTO
    {
        public string IdApprenant { get; set; }
        public double Note { get; set; }
        public string Remarque { get; set; }
        public string ResultatFinal { get; set; }
        public string? CoursId { get; set; }
        public IFormFile? Fichier { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Protège le contrôleur entier
    public class ResultatsController : ControllerBase
    {
        private readonly ElearningDbContext _context;

        public ResultatsController(ElearningDbContext context)
        {
            _context = context;
        }

        // GET: api/Resultats
        /// <summary>
        /// Récupère tous les résultats.
        /// </summary>
        /// <returns>Une liste de tous les résultats.</returns>
        [HttpGet]
        [Authorize(Roles = "Administrateur, Formateur")]
        public async Task<ActionResult<IEnumerable<Resultat>>> GetResultats()
        {
            return await _context.Resultats.ToListAsync();
        }

        // GET: api/Resultats/5
        /// <summary>
        /// Récupère un résultat spécifique par son identifiant.
        /// </summary>
        /// <param name="id">L'identifiant du résultat.</param>
        /// <returns>Le résultat correspondant.</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrateur, Formateur, Apprenant")]
        public async Task<ActionResult<Resultat>> GetResultat(string id)
        {
            var resultat = await _context.Resultats
                .Include(r => r.Apprenant)
                .Include(r => r.Auteur)
                .Include(r => r.Fichier)
                .FirstOrDefaultAsync(r => r.IdResultat == id);

            if (resultat == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            // Seul l'apprenant concerné, le formateur ou l'administrateur peut voir le résultat
            if (userRole == "Apprenant" && resultat.IdApprenant != userId)
            {
                return Forbid();
            }

            return resultat;
        }

        // GET: api/Resultats/byApprenant/{idApprenant}
        /// <summary>
        /// Récupère tous les résultats pour un apprenant spécifique.
        /// </summary>
        /// <param name="idApprenant">L'identifiant de l'apprenant.</param>
        /// <returns>Une liste des résultats de l'apprenant.</returns>
        [HttpGet("byApprenant/{idApprenant}")]
        [Authorize(Roles = "Administrateur, Formateur, Apprenant")]
        public async Task<ActionResult<IEnumerable<Resultat>>> GetResultatsByApprenant(string idApprenant)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            // Un apprenant peut seulement voir ses propres résultats
            if (userRole == "Apprenant" && idApprenant != userId)
            {
                return Forbid();
            }

            var resultats = await _context.Resultats
                .Where(r => r.IdApprenant == idApprenant)
                .Include(r => r.Auteur)
                .Include(r => r.Fichier)
                .ToListAsync();

            if (!resultats.Any())
            {
                return NotFound("Aucun résultat trouvé pour cet apprenant.");
            }

            return resultats;
        }

        // POST: api/Resultats
        /// <summary>
        /// Crée un nouveau résultat.
        /// </summary>
        /// <param name="resultatDto">L'objet DTO contenant les données du résultat et le fichier.</param>
        /// <returns>Le résultat nouvellement créé.</returns>
        [HttpPost]
        [Authorize(Roles = "Administrateur, Formateur")]
        public async Task<ActionResult<Resultat>> PostResultat([FromForm] ResultatCreateDTO resultatDto)
        {
            // Vérifie que l'apprenant existe
            var apprenantExists = await _context.Utilisateurs
                .AnyAsync(u => u.IdUtilisateur == resultatDto.IdApprenant && u.TypeUtilisateur == TypeUtilisateur.Apprenant);

            if (!apprenantExists)
            {
                return BadRequest("L'apprenant spécifié n'existe pas.");
            }

            // Vérifie si un CoursId est fourni et s'il existe
            if (!string.IsNullOrEmpty(resultatDto.CoursId))
            {
                var coursExists = await _context.Cours.AnyAsync(c => c.IdCours == resultatDto.CoursId);
                if (!coursExists)
                {
                    return BadRequest("Le cours spécifié n'existe pas.");
                }
            }

            // Gérer le téléchargement du fichier
            string? fichierId = null;
            if (resultatDto.Fichier != null)
            {
                // Créer le dossier "uploads" s'il n'existe pas
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(resultatDto.Fichier.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await resultatDto.Fichier.CopyToAsync(stream);
                }

                var fichier = new Fichier
                {
                    IdFichier = Guid.NewGuid().ToString(),
                    Nom = resultatDto.Fichier.FileName,
                    CheminFichier = uniqueFileName,
                    TypeFichier = resultatDto.Fichier.ContentType,
                    CoursId = resultatDto.CoursId
                };
                _context.Fichiers.Add(fichier);
                await _context.SaveChangesAsync();
                fichierId = fichier.IdFichier;
            }

            // Créer le nouvel objet Resultat
            var resultat = new Resultat
            {
                IdResultat = Guid.NewGuid().ToString(),
                Note = resultatDto.Note,
                Remarque = resultatDto.Remarque,
                ResultatFinal = resultatDto.ResultatFinal,
                IdApprenant = resultatDto.IdApprenant,
                CoursId = resultatDto.CoursId,
                FichierId = fichierId,
                DatePublication = DateTime.Now,
                IdAuteur = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };

            _context.Resultats.Add(resultat);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetResultat), new { id = resultat.IdResultat }, resultat);
        }

        // PUT: api/Resultats/5
        /// <summary>
        /// Met à jour un résultat existant.
        /// </summary>
        /// <param name="id">L'identifiant du résultat à mettre à jour.</param>
        /// <param name="resultat">L'objet Resultat avec les nouvelles données.</param>
        /// <returns>Un résultat sans contenu si la mise à jour réussit.</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrateur, Formateur")]
        public async Task<IActionResult> PutResultat(string id, Resultat resultat)
        {
            if (id != resultat.IdResultat)
            {
                return BadRequest();
            }

            var auteurId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Vérifie que seul l'auteur ou un administrateur peut modifier le résultat
            var existingResultat = await _context.Resultats.AsNoTracking().FirstOrDefaultAsync(r => r.IdResultat == id);
            if (existingResultat == null)
            {
                return NotFound();
            }
            if (existingResultat.IdAuteur != auteurId && !User.IsInRole("Administrateur"))
            {
                return Forbid();
            }

            _context.Entry(resultat).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ResultatExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Resultats/5
        /// <summary>
        /// Supprime un résultat par son identifiant.
        /// </summary>
        /// <param name="id">L'identifiant du résultat à supprimer.</param>
        /// <returns>Un résultat sans contenu si la suppression réussit.</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrateur, Formateur")]
        public async Task<IActionResult> DeleteResultat(string id)
        {
            var resultat = await _context.Resultats.Include(r => r.Fichier).FirstOrDefaultAsync(r => r.IdResultat == id);
            if (resultat == null)
            {
                return NotFound();
            }

            var auteurId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Vérifie que seul l'auteur ou un administrateur peut supprimer le résultat
            if (resultat.IdAuteur != auteurId && !User.IsInRole("Administrateur"))
            {
                return Forbid();
            }

            // Supprimer le fichier associé si un existe
            if (resultat.Fichier != null)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", resultat.Fichier.CheminFichier);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                _context.Fichiers.Remove(resultat.Fichier);
            }

            _context.Resultats.Remove(resultat);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ResultatExists(string id)
        {
            return _context.Resultats.Any(e => e.IdResultat == id);
        }
    }
}
