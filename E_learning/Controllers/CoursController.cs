using E_learning.Models;
using E_learning.Models.E_learning.Models;
using iText.Kernel.Pdf; // N'oubliez pas d'installer ce package NuGet
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text; // Ajouté pour l'exportation CSV
using System.Threading.Tasks;

namespace E_learning.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursController : ControllerBase
    {
        private readonly ElearningDbContext _context;

        public CoursController(ElearningDbContext context)
        {
            _context = context;
        }

        // GET: api/Cours
        /// <summary>
        /// Récupère la liste de tous les cours.
        /// </summary>
        /// <returns>Une liste d'objets Cours.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cours>>> GetCours()
        {
            // Inclure les formateurs dans la réponse
            return await _context.Cours.Include(c => c.CoursFormateurs).ThenInclude(cf => cf.Formateur).ToListAsync();
        }

        // GET: api/Cours/5
        /// <summary>
        /// Récupère un cours par son identifiant.
        /// </summary>
        /// <param name="id">L'identifiant du cours.</param>
        /// <returns>L'objet Cours correspondant.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Cours>> GetCoursById(string id)
        {
            var cours = await _context.Cours.Include(c => c.CoursFormateurs).ThenInclude(cf => cf.Formateur).FirstOrDefaultAsync(c => c.IdCours == id);

            if (cours == null)
            {
                return NotFound();
            }

            return cours;
        }

        // GET: api/Cours/niveau/A1
        /// <summary>
        /// Récupère la liste des cours par niveau (A1, A2, etc.).
        /// </summary>
        /// <param name="niveau">Le niveau de cours à rechercher.</param>
        /// <returns>Une liste de cours du niveau spécifié.</returns>
        [HttpGet("niveau/{niveau}")]
        public async Task<ActionResult<IEnumerable<Cours>>> GetCoursParNiveau(string niveau)
        {
            var cours = await _context.Cours
                .Where(c => c.Niveau == niveau)
                .ToListAsync();

            if (cours == null || cours.Count == 0)
            {
                return NotFound($"No courses found for level: {niveau}");
            }

            return cours;
        }

        // POST: api/Cours
        /// <summary>
        /// Crée un nouveau cours et gère les relations avec les formateurs et les fichiers.
        /// </summary>
        /// <param name="titre">Le titre du cours.</param>
        /// <param name="description">La description du cours.</param>
        /// <param name="niveau">Le niveau du cours.</param>
        /// <param name="formateurIds">La liste des identifiants des formateurs à associer.</param>
        /// <param name="fichiers">La liste des fichiers à télécharger.</param>
        /// <returns>Le nouvel objet Cours créé.</returns>
        [HttpPost]
        [Authorize(Roles = "Administrateur")]
        public async Task<ActionResult<Cours>> PostCours([FromForm] string titre, [FromForm] string description, [FromForm] string niveau, [FromForm] List<string> formateurIds, [FromForm] List<IFormFile> fichiers)
        {
            var nouveauxFormateurs = await _context.Utilisateurs.Where(u => formateurIds.Contains(u.IdUtilisateur) && u.TypeUtilisateur == TypeUtilisateur.Formateur).ToListAsync();
            if (nouveauxFormateurs.Count != formateurIds.Count)
            {
                return BadRequest("Un ou plusieurs identifiants de formateur sont invalides.");
            }

            var cours = new Cours
            {
                Titre = titre,
                Description = description,
                Niveau = niveau
            };

            _context.Cours.Add(cours);

            // Gérer le téléchargement des fichiers
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            foreach (var fichier in fichiers)
            {
                if (fichier.Length > 0)
                {
                    var filePath = Path.Combine(uploadsPath, fichier.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await fichier.CopyToAsync(stream);
                    }

                    // Créer un nouvel objet Fichier et l'ajouter à la liste de fichiers du cours
                    cours.ListeFichiers.Add(new Fichier
                    {
                        IdFichier = Guid.NewGuid().ToString(),
                        Nom = fichier.FileName,
                        CheminFichier = filePath,
                        TypeFichier = fichier.ContentType,
                        CoursId = cours.IdCours
                    });
                }
            }

            // Affecter les formateurs au cours
            foreach (var formateur in nouveauxFormateurs)
            {
                _context.CoursFormateurs.Add(new CoursFormateur
                {
                    IdCours = cours.IdCours,
                    IdFormateur = formateur.IdUtilisateur
                });
            }

            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCours), new { id = cours.IdCours }, cours);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCours(string id, [FromBody] CoursUpdateDto coursUpdateDto)
        {
            // Recherche du cours par l'ID fourni dans l'URL
            var cours = await _context.Cours.FindAsync(id);

            if (cours == null)
            {
                return NotFound();
            }

            // Mise à jour des propriétés
            cours.Titre = coursUpdateDto.Titre;
            cours.Description = coursUpdateDto.Description;
            cours.Niveau = coursUpdateDto.Niveau;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CoursExists(id))
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
        // Models/CoursUpdateDto.cs
        public class CoursUpdateDto
        {
            public string Titre { get; set; }
            public string Description { get; set; }
            public string Niveau { get; set; }
        }
        [HttpPost("{id}/formateurs")]
        [Authorize(Roles = "Administrateur")]
        public async Task<IActionResult> AffecterFormateurs(string id, [FromBody] List<string> formateurIds)
        {
            var cours = await _context.Cours.FindAsync(id);
            if (cours == null)
            {
                return NotFound("Cours non trouvé.");
            }

            var formateurs = await _context.Utilisateurs
                .Where(u => formateurIds.Contains(u.IdUtilisateur) && u.TypeUtilisateur == TypeUtilisateur.Formateur)
                .ToListAsync();

            if (formateurs.Count != formateurIds.Count)
            {
                return BadRequest("Un ou plusieurs identifiants de formateur sont invalides.");
            }

            // Récupérer les formateurs déjà affectés pour ce cours
            var existingFormateurIds = await _context.CoursFormateurs
                .Where(cf => cf.IdCours == id)
                .Select(cf => cf.IdFormateur)
                .ToListAsync();

            foreach (var formateur in formateurs)
            {
                // Vérifier si le formateur n'est PAS déjà affecté
                if (!existingFormateurIds.Contains(formateur.IdUtilisateur))
                {
                    _context.CoursFormateurs.Add(new CoursFormateur
                    {
                        IdCours = id,
                        IdFormateur = formateur.IdUtilisateur,
                        // IdApprenant is no longer part of this model.
                        Cours = cours,
                        Formateur = formateur
                    });
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

       
        [HttpPost("{id}/apprenants")]
        [Authorize(Roles = "Formateur, Administrateur")]
        public async Task<IActionResult> AffecterApprenants(string id, [FromBody] List<string> apprenantIds)
        {
            var cours = await _context.Cours.FindAsync(id);
            if (cours == null)
            {
                return NotFound("Cours non trouvé.");
            }

            var apprenants = await _context.Utilisateurs
                .Where(u => apprenantIds.Contains(u.IdUtilisateur) && u.TypeUtilisateur == TypeUtilisateur.Apprenant)
                .ToListAsync();

            if (apprenants.Count != apprenantIds.Count)
            {
                return BadRequest("Un ou plusieurs identifiants d'apprenant sont invalides.");
            }

            // Récupérer les apprenants déjà inscrits au cours pour éviter les doublons
            // We now query the new CoursApprenants table
            var existingApprenantIds = await _context.CoursApprenants
                .Where(ca => ca.IdCours == id)
                .Select(ca => ca.IdApprenant)
                .ToListAsync();

            foreach (var apprenant in apprenants)
            {
                // Ajouter seulement si l'apprenant n'est pas déjà inscrit
                if (!existingApprenantIds.Contains(apprenant.IdUtilisateur))
                {
                    // We now use the new CoursApprenant model
                    _context.CoursApprenants.Add(new CoursApprenant
                    {
                        IdCours = id,
                        IdApprenant = apprenant.IdUtilisateur,
                        Cours = cours,
                        Apprenant = apprenant
                    });
                }
            }

            await _context.SaveChangesAsync();
            return NoContent(); // 204 No Content on success
        }
        // NOUVEAU: Récupère la liste des formateurs pour un cours spécifique.
        // GET: api/Cours/5/formateurs
        [HttpGet("{id}/formateurs")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Utilisateur>>> GetListeFormateurs(string id)
        {
            var formateurs = await _context.CoursFormateurs
                .Where(cf => cf.IdCours == id)
                .Select(cf => cf.Formateur)
                .ToListAsync();

            if (formateurs == null || !formateurs.Any())
            {
                return NotFound("Aucun formateur trouvé pour ce cours.");
            }

            return formateurs;
        }

        // NOUVEAU: Récupère la liste des apprenants pour un cours spécifique.
        // GET: api/Cours/5/apprenants
        [HttpGet("{id}/apprenants")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Utilisateur>>> GetListeApprenants(string id)
        {
            var apprenants = await _context.CoursApprenants
                .Where(ca => ca.IdCours == id)
                .Select(ca => ca.Apprenant)
                .ToListAsync();

            if (apprenants == null || !apprenants.Any())
            {
                return NotFound("Aucun apprenant trouvé pour ce cours.");
            }

            return apprenants;
        }

        // PUT: api/Cours/{id}/formateurs
        /// <summary>
        /// Met à jour la liste des formateurs d'un cours.
        /// </summary>
        /// <param name="id">L'identifiant du cours.</param>
        /// <param name="formateurIds">La nouvelle liste d'identifiants de formateurs.</param>
        /// <returns>Un résultat sans contenu si la mise à jour réussit.</returns>
        [HttpPut("{id}/formateurs")]
        [Authorize(Roles = "Administrateur")]
        public async Task<IActionResult> ModifierFormateurs(string id, [FromBody] List<string> formateurIds)
        {
            var cours = await _context.Cours.Include(c => c.CoursFormateurs).FirstOrDefaultAsync(c => c.IdCours == id);
            if (cours == null)
            {
                return NotFound("Cours non trouvé.");
            }

            // Vérifier que tous les nouveaux formteurs existent et ont le bon rôle
            var nouveauxFormateurs = await _context.Utilisateurs.Where(u => formateurIds.Contains(u.IdUtilisateur) && u.TypeUtilisateur == TypeUtilisateur.Formateur).ToListAsync();
            if (nouveauxFormateurs.Count != formateurIds.Count)
            {
                return BadRequest("Un ou plusieurs identifiants de formateur sont invalides.");
            }

            _context.CoursFormateurs.RemoveRange(cours.CoursFormateurs);

            foreach (var formateur in nouveauxFormateurs)
            {
                _context.CoursFormateurs.Add(new CoursFormateur
                {
                    IdCours = id,
                    IdFormateur = formateur.IdUtilisateur
                });
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Cours/5
        /// <summary>
        /// Supprime un cours par son identifiant.
        /// </summary>
        /// <param name="id">L'identifiant du cours à supprimer.</param>
        /// <returns>Un résultat sans contenu si la suppression réussit.</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrateur")]
        public async Task<IActionResult> DeleteCours(string id)
        {
            // Eagerly load all related entities to ensure they are available for deletion.
            var cours = await _context.Cours
                .Include(c => c.ListeFichiers) // Files associated with the course
                .Include(c => c.CoursFormateurs) // Formateurs associated with the course
                .Include(c => c.CoursApprenants) // Apprenants associated with the course
                .FirstOrDefaultAsync(c => c.IdCours == id);

            if (cours == null)
            {
                return NotFound();
            }

            if (cours.ListeFichiers != null && cours.ListeFichiers.Any())
            {
                _context.Fichiers.RemoveRange(cours.ListeFichiers);
            }

           

            if (cours.CoursFormateurs != null && cours.CoursFormateurs.Any())
            {
                _context.CoursFormateurs.RemoveRange(cours.CoursFormateurs);
            }

            if (cours.CoursApprenants != null && cours.CoursApprenants.Any())
            {
                _context.CoursApprenants.RemoveRange(cours.CoursApprenants);
            }

            // Finally, remove the course itself.
            _context.Cours.Remove(cours);

            // Save all changes in a single transaction.
            // This ensures all removals succeed together or fail together.
            await _context.SaveChangesAsync();

            return NoContent(); // HTTP 204 No Content response for a successful deletion.
        }

        // POST: api/Cours/upload
        /// <summary>
        /// Gère le téléchargement d'un fichier et l'associe à un cours.
        /// </summary>
        /// <param name="file">Le fichier à télécharger.</param>
        /// <param name="coursId">L'identifiant du cours auquel associer le fichier.</param>
        /// <returns>Le chemin du fichier si le téléchargement est réussi.</returns>
        [HttpPost("upload/{coursId}")]
        [Authorize(Roles = "Formateur, Administrateur")]
        public async Task<IActionResult> UploadFichier(IFormFile file, string coursId)
        {
            // Vérifier si un fichier a été envoyé et si l'ID du cours est valide
            if (file == null || file.Length == 0)
            {
                return BadRequest("Aucun fichier n'a été sélectionné.");
            }

            // Vérifier si le cours existe
            var cours = await _context.Cours.FindAsync(coursId);
            if (cours == null)
            {
                return NotFound("Cours non trouvé.");
            }

            try
            {
                // Créer un nom de fichier unique pour éviter les conflits
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;

                // Définir le chemin d'enregistrement sur le serveur
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Sauvegarder le fichier sur le disque
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Créer un nouvel objet Fichier pour la base de données
                var newFichier = new Fichier
                {
                    IdFichier = Guid.NewGuid().ToString(),
                    Nom = file.FileName,
                    CheminFichier = Path.Combine("uploads", uniqueFileName),
                    TypeFichier = file.ContentType,
                    CoursId = coursId
                };

                // Ajouter le fichier à la base de données et sauvegarder les changements
                _context.Fichiers.Add(newFichier);
                await _context.SaveChangesAsync();

                return Ok(new { CheminFichier = newFichier.CheminFichier });
            }
            catch (Exception ex)
            {
                // En cas d'erreur, retourner une réponse avec le message d'erreur
                return StatusCode(500, $"Erreur lors du téléchargement du fichier: {ex.Message}");
            }
        }

        private bool CoursExists(string id)
        {
            return _context.Cours.Any(e => e.IdCours == id);
        }
    }

}
