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
    // DTO (Data Transfer Object) unique pour gérer les données du formulaire et les fichiers.
    // La liste de fichiers est optionnelle.
    public class ExerciceCreateDTO
    {
        public TypeExercice Type { get; set; }
        public string Contenu { get; set; }
        public string? ReponseCorrecte { get; set; }
        public string? CoursId { get; set; } // Le CoursId est maintenant optionnel
        public DateTime? DateLimite { get; set; }
        public List<IFormFile>? Fichiers { get; set; }
    }


    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ExercicesController : ControllerBase
    {
        private readonly ElearningDbContext _context;

        public ExercicesController(ElearningDbContext context)
        {
            _context = context;
        }

        // Méthode utilitaire pour vérifier si l'utilisateur est un formateur ou un administrateur
        private bool IsFormateurOrAdmin()
        {
            return User.IsInRole("Formateur") || User.IsInRole("Administrateur");
        }

        // GET: api/Exercices
        /// <summary>
        /// Récupère la liste de tous les exercices.
        /// </summary>
        /// <returns>Une liste d'objets Exercice.</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Exercice>>> GetExercices()
        {
            return await _context.Exercices.Include(e => e.ListeFichiers).ToListAsync();
        }

        // GET: api/Exercices/5
        /// <summary>
        /// Récupère un exercice par son identifiant.
        /// </summary>
        /// <param name="id">L'identifiant de l'exercice.</param>
        /// <returns>L'objet Exercice correspondant.</returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Exercice>> GetExercice(string id)
        {
            var exercice = await _context.Exercices.Include(e => e.ListeFichiers).FirstOrDefaultAsync(e => e.IdExercice == id);

            if (exercice == null)
            {
                return NotFound();
            }

            return exercice;
        }

        // GET: api/Exercices/by-cours/5
        /// <summary>
        /// Récupère la liste des exercices pour un cours spécifique.
        /// </summary>
        /// <param name="coursId">L'identifiant du cours.</param>
        /// <returns>Une liste d'objets Exercice.</returns>
        [HttpGet("by-cours/{coursId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Exercice>>> GetExercicesForCourse(string coursId)
        {
            var exercices = await _context.Exercices
                .Where(e => e.CoursId == coursId)
                .Include(e => e.ListeFichiers)
                .ToListAsync();

            if (exercices == null || exercices.Count == 0)
            {
                return NotFound($"No exercises found for course with ID: {coursId}");
            }

            return exercices;
        }

        // POST: api/Exercices
        /// <summary>
        /// Crée un nouvel exercice, avec ou sans fichiers attachés.
        /// </summary>
        /// <param name="dto">L'objet de transfert de données contenant les informations de l'exercice et les fichiers.</param>
        /// <returns>Le nouvel objet Exercice créé.</returns>
        [HttpPost]
        [Authorize(Roles = "Formateur, Administrateur")]
        public async Task<ActionResult<Exercice>> PostExercice([FromForm] ExerciceCreateDTO dto)
        {
            // Vérifier si le CoursId existe seulement s'il est fourni
            if (!string.IsNullOrEmpty(dto.CoursId))
            {
                var cours = await _context.Cours.FindAsync(dto.CoursId);
                if (cours == null)
                {
                    return BadRequest("Cours non valide.");
                }
            }

            // Créer le nouvel objet Exercice
            var exercice = new Exercice
            {
                IdExercice = Guid.NewGuid().ToString(),
                Type = dto.Type,
                Contenu = dto.Contenu,
                ReponseCorrecte = dto.ReponseCorrecte,
                CoursId = dto.CoursId,
                DateLimite = dto.DateLimite // Attribution de la nouvelle propriété
            };

            // Ajoute l'exercice au contexte et le sauvegarde pour obtenir un Id
            _context.Exercices.Add(exercice);
            await _context.SaveChangesAsync();

            // Gérer le téléchargement des fichiers si la liste n'est pas nulle et contient des éléments
            if (dto.Fichiers != null && dto.Fichiers.Any())
            {
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                foreach (var fichier in dto.Fichiers)
                {
                    if (fichier.Length > 0)
                    {
                        var filePath = Path.Combine(uploadsPath, fichier.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await fichier.CopyToAsync(stream);
                        }

                        // Créer un nouvel objet Fichier en utilisant l'Id de l'exercice maintenant disponible
                        exercice.ListeFichiers.Add(new Fichier
                        {
                            IdFichier = Guid.NewGuid().ToString(),
                            Nom = fichier.FileName,
                            CheminFichier = filePath,
                            TypeFichier = fichier.ContentType,
                            ExerciceId = exercice.IdExercice
                        });
                    }
                }
                // Sauvegarde les fichiers dans la base de données
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetExercice), new { id = exercice.IdExercice }, exercice);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Formateur, Administrateur")]
        public async Task<IActionResult> PutExercice(string id, Exercice updatedExercice)
        {
            // 1. Check if the ID in the URL matches the one in the request body.
            if (id != updatedExercice.IdExercice)
            {
                return BadRequest("L'ID dans l'URL ne correspond pas à l'ID dans le corps de la requête.");
            }

            // 2. Fetch the existing Exercice from the database.
            var existingExercice = await _context.Exercices.FindAsync(id);
            if (existingExercice == null)
            {
                return NotFound("Exercice non trouvé.");
            }

            // Check if CoursId is valid if it's not null or empty
            if (!string.IsNullOrEmpty(updatedExercice.CoursId))
            {
                var cours = await _context.Cours.FindAsync(updatedExercice.CoursId);
                if (cours == null)
                {
                    return BadRequest("Cours non valide.");
                }
            }


            // 3. Update the properties of the EXISTING entity with the values from the request.
            // This is the key step to bypass the model validation on navigation properties.
            existingExercice.Type = updatedExercice.Type;
            existingExercice.Contenu = updatedExercice.Contenu;
            existingExercice.ReponseCorrecte = updatedExercice.ReponseCorrecte;
            existingExercice.DateLimite = updatedExercice.DateLimite;
            existingExercice.CoursId = updatedExercice.CoursId;

            try
            {
                // 4. Save the changes. Entity Framework will now update the existing entity.
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExerciceExists(id))
                {
                    return NotFound("Exercice non trouvé lors de la mise à jour.");
                }
                else
                {
                    throw; // Re-throw the exception for global error handling.
                }
            }

            // 5. Return a successful, no-content response.
            return NoContent();
        }

        // DELETE: api/Exercices/5
        /// <summary>
        /// Supprime un exercice par son identifiant.
        /// </summary>
        /// <param name="id">L'identifiant de l'exercice à supprimer.</param>
        /// <returns>Un résultat sans contenu si la suppression réussit.</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Formateur, Administrateur")]
        public async Task<IActionResult> DeleteExercice(string id)
        {
            var exercice = await _context.Exercices
                .Include(e => e.ListeFichiers)
                .FirstOrDefaultAsync(e => e.IdExercice == id);

            if (exercice == null)
            {
                return NotFound();
            }

            // Supprime d'abord les fichiers associés à l'exercice
            if (exercice.ListeFichiers != null && exercice.ListeFichiers.Any())
            {
                _context.Fichiers.RemoveRange(exercice.ListeFichiers);
            }

            _context.Exercices.Remove(exercice);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Exercices/upload/{exerciceId}
        /// <summary>
        /// Gère le téléchargement d'un fichier et l'associe à un exercice existant.
        /// </summary>
        /// <param name="file">Le fichier à télécharger.</param>
        /// <param name="exerciceId">L'identifiant de l'exercice auquel associer le fichier.</param>
        /// <returns>Le chemin du fichier si le téléchargement est réussi.</returns>
        [HttpPost("upload/{exerciceId}")]
        public async Task<IActionResult> UploadFichier(IFormFile file, string exerciceId)
        {
            // Vérifier si un fichier a été envoyé et si l'ID de l'exercice est valide
            if (file == null || file.Length == 0)
            {
                return BadRequest("Aucun fichier n'a été sélectionné.");
            }

            // Vérifier si l'exercice existe
            var exercice = await _context.Exercices.FindAsync(exerciceId);
            if (exercice == null)
            {
                return NotFound("Exercice non trouvé.");
            }

            try
            {
                // Sauvegarder le fichier et obtenir son chemin
                var cheminFichier = await SaveFileAsync(file);

                // Créer un nouvel objet Fichier pour la base de données
                var newFichier = new Fichier
                {
                    IdFichier = Guid.NewGuid().ToString(),
                    Nom = file.FileName,
                    CheminFichier = cheminFichier,
                    TypeFichier = file.ContentType,
                    ExerciceId = exerciceId
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

     
        

        // Méthode utilitaire pour sauvegarder un fichier sur le serveur
        private async Task<string> SaveFileAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return Path.Combine("uploads", uniqueFileName);
        }

        private bool ExerciceExists(string id)
        {
            return _context.Exercices.Any(e => e.IdExercice == id);
        }
    }
}
