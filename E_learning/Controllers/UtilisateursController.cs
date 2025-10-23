using E_learning.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace E_learning.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Protects the entire controller
    public class UtilisateursController : ControllerBase
    {
        private readonly ElearningDbContext _context;


        public UtilisateursController(ElearningDbContext context)
        {
            _context = context;
        }

        public class ForgotPasswordDTO
        {
            public string Email { get; set; }
        }

        public class ChangePasswordDTO
        {
            public string OldPassword { get; set; }
            public string NewPassword { get; set; }
        }

        // GET: api/Utilisateurs
        /// <summary>
        /// Récupère la liste de tous les utilisateurs.
        /// </summary>
        /// <returns>Une liste d'objets Utilisateur.</returns>
        [HttpGet]
        [Authorize(Roles = "Administrateur, Formateur")]
        public async Task<ActionResult<IEnumerable<Utilisateur>>> GetUtilisateurs()
        {
            return await _context.Utilisateurs.ToListAsync();
        }

        // GET: api/Utilisateurs/5
        /// <summary>
        /// Récupère un utilisateur par son identifiant.
        /// </summary>
        /// <param name="id">L'identifiant de l'utilisateur.</param>
        /// <returns>L'objet Utilisateur correspondant.</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Apprenant, Formateur, Administrateur")]
        public async Task<ActionResult<Utilisateur>> GetUtilisateur(string id)
        {
            var utilisateur = await _context.Utilisateurs.FindAsync(id);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (utilisateur == null)
            {
                return NotFound();
            }

            // Un apprenant ne peut voir que son propre profil
            if (User.IsInRole("Apprenant") && utilisateur.IdUtilisateur != userId)
            {
                return Forbid();
            }

            return utilisateur;
        }

        // PUT: api/Utilisateurs/5
        /// <summary>
        /// Met à jour les informations d'un utilisateur existant.
        /// </summary>
        /// <param name="id">L'identifiant de l'utilisateur à mettre à jour.</param>
        /// <param name="utilisateur">L'objet Utilisateur avec les nouvelles données.</param>
        /// <returns>Un résultat sans contenu si la mise à jour réussit.</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Apprenant, Formateur, Administrateur")]
        public async Task<IActionResult> PutUtilisateur(string id, Utilisateur utilisateur)
        {
            if (id != utilisateur.IdUtilisateur)
            {
                return BadRequest();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            // Seul un administrateur peut modifier le rôle d'un utilisateur
            if (utilisateur.TypeUtilisateur.ToString() != userRole && userRole != "Administrateur")
            {
                return Forbid();
            }

            // Un utilisateur non-admin ne peut mettre à jour que son propre profil
            if (userRole != "Administrateur" && utilisateur.IdUtilisateur != userId)
            {
                return Forbid();
            }

            _context.Entry(utilisateur).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UtilisateurExists(id))
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

        // DELETE: api/Utilisateurs/5
        /// <summary>
        /// Supprime un utilisateur par son identifiant.
        /// </summary>
        /// <param name="id">L'identifiant de l'utilisateur à supprimer.</param>
        /// <returns>Un résultat sans contenu si la suppression réussit.</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrateur")]
        public async Task<IActionResult> DeleteUtilisateur(string id)
        {
            var utilisateur = await _context.Utilisateurs.FindAsync(id);
            if (utilisateur == null)
            {
                return NotFound();
            }

            _context.Utilisateurs.Remove(utilisateur);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        // GET: api/Utilisateurs/cours-by-formateur
        /// <summary>
        /// Récupère la liste des cours affectés au formateur authentifié.
        /// </summary>
        /// <returns>Une liste d'objets Cours.</returns>
        [HttpGet("cours-by-formateur")]
        [Authorize(Roles = "Formateur")]
        public async Task<ActionResult<IEnumerable<Cours>>> GetCoursForFormateur()
        {
            var formateurId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Utilise la table de jointure pour trouver tous les cours où ce formateur est lié.
            var coursIds = await _context.CoursFormateurs
        .Where(cf => cf.IdFormateur == formateurId)
        .Select(cf => cf.IdCours)
        .ToListAsync();

            // Récupère les objets Cours complets en utilisant les IDs.
            var cours = await _context.Cours
        .Where(c => coursIds.Contains(c.IdCours))
        .ToListAsync();

            if (cours == null || !cours.Any())
            {
                return NotFound("Aucun cours trouvé pour ce formateur.");
            }

            return cours;
        }


        // GET: api/Utilisateurs/apprenants-by-formateur
        /// <summary>
        /// Récupère la liste des apprenants affectés aux cours du formateur authentifié.
        /// </summary>
        /// <returns>Une liste d'objets Utilisateur de type Apprenant.</returns>
        [HttpGet("apprenants-by-formateur")]
        [Authorize(Roles = "Administrateur, Formateur")]
        public async Task<ActionResult<IEnumerable<Utilisateur>>> GetApprenantsForFormateur()
        {
            var formateurId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 1. Récupère les IDs de tous les cours affectés à ce formateur.
            var coursIds = await _context.CoursFormateurs
        .Where(cf => cf.IdFormateur == formateurId)
        .Select(cf => cf.IdCours)
        .ToListAsync();

            if (!coursIds.Any())
            {
                return NotFound("Aucun cours n'est assigné à ce formateur.");
            }

            // 2. Utilise la nouvelle table de jointure CoursApprenant pour récupérer les IDs d'apprenants uniques
            // affectés aux cours du formateur.
            var apprenantIds = await _context.CoursApprenants
        .Where(ca => coursIds.Contains(ca.IdCours))
        .Select(ca => ca.IdApprenant)
        .Distinct()
        .ToListAsync();

            if (!apprenantIds.Any())
            {
                return NotFound("Aucun apprenant n'est trouvé dans les cours assignés.");
            }

            // 3. Récupère les objets Utilisateur pour ces IDs.
            var apprenants = await _context.Utilisateurs
        .Where(u => apprenantIds.Contains(u.IdUtilisateur))
        .ToListAsync();

            return apprenants;
        }

        // GET: api/Utilisateurs/getAllFormateurs
        /// <summary>
        /// Récupère la liste de tous les utilisateurs de type Formateur.
        /// </summary>
        /// <returns>Une liste d'objets Utilisateur de type Formateur.</returns>
        [HttpGet("getAllFormateurs")]
        [Authorize(Roles = "Administrateur, Formateur")]
        public async Task<ActionResult<IEnumerable<Utilisateur>>> GetAllFormateurs()
        {
            var formateurs = await _context.Utilisateurs.Where(u => u.TypeUtilisateur == TypeUtilisateur.Formateur).ToListAsync();
            return formateurs;
        }

        // GET: api/Utilisateurs/getAllApprenants
        /// <summary>
        /// Récupère la liste de tous les utilisateurs de type Apprenant.
        /// </summary>
        /// <returns>Une liste d'objets Utilisateur de type Apprenant.</returns>
        [HttpGet("getAllApprenants")]
        [Authorize(Roles = "Administrateur, Formateur")]
        public async Task<ActionResult<IEnumerable<Utilisateur>>> GetAllApprenants()
        {
            var apprenants = await _context.Utilisateurs.Where(u => u.TypeUtilisateur == TypeUtilisateur.Apprenant).ToListAsync();
            return apprenants;
        }

        // POST: api/Utilisateurs/register
        /// <summary>
        /// Crée un nouvel utilisateur.
        /// </summary>
        /// <param name="utilisateur">L'objet Utilisateur à créer.</param>
        /// <returns>Le nouvel utilisateur créé.</returns>
        [HttpPost("register")]
        [AllowAnonymous] // Allows user creation without authentication (e.g., for sign-up)
        public async Task<ActionResult<Utilisateur>> RegisterUser(Utilisateur utilisateur)
        {
            // Check if the user already exists by email.
            if (_context.Utilisateurs.Any(u => u.Email == utilisateur.Email))
            {
                return Conflict("Un utilisateur avec cet email existe déjà.");
            }

            // Hash the password before saving.
            var passwordHasher = new PasswordHasher<Utilisateur>();
            utilisateur.MotDePasse = passwordHasher.HashPassword(utilisateur, utilisateur.MotDePasse);
            utilisateur.IdUtilisateur = Guid.NewGuid().ToString(); // Assign a unique ID
            _context.Utilisateurs.Add(utilisateur);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUtilisateur", new { id = utilisateur.IdUtilisateur }, utilisateur);
        }

       
        [HttpPut("ban/{id}")]
        [Authorize(Roles = "Administrateur")]
        public async Task<IActionResult> BanUser(string id)
        {
            var user = await _context.Utilisateurs.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            user.EstActif = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Activates a user by setting their EstActif status to true.
        /// </summary>
        /// <param name="id">The ID of the user to activate.</param>
        /// <returns>A result indicating the success or failure of the operation.</returns>
        [HttpPut("activate/{id}")]
        [Authorize(Roles = "Administrateur")]
        public async Task<IActionResult> ActivateUser(string id)
        {
            var user = await _context.Utilisateurs.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            user.EstActif = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        // PUT: api/Utilisateurs/change-password
        /// <summary>
        /// Allows an authenticated user to change their own password.
        /// </summary>
        /// <param name="dto">The object containing the old and new passwords.</param>
        /// <returns>A no-content result if the update is successful.</returns>
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var utilisateur = await _context.Utilisateurs.FindAsync(userId);

            if (utilisateur == null)
            {
                return NotFound();
            }

            var passwordHasher = new PasswordHasher<Utilisateur>();
            var result = passwordHasher.VerifyHashedPassword(utilisateur, utilisateur.MotDePasse, dto.OldPassword);

            if (result == PasswordVerificationResult.Failed)
            {
                return BadRequest("L'ancien mot de passe est incorrect.");
            }

            utilisateur.MotDePasse = passwordHasher.HashPassword(utilisateur, dto.NewPassword);
            _context.Entry(utilisateur).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UtilisateurExists(string id)
        {
            return _context.Utilisateurs.Any(e => e.IdUtilisateur == id);
        }
    }
}
