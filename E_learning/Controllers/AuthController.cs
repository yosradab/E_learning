using BCrypt.Net; // N'oubliez pas d'installer ce package NuGet pour le hachage des mots de passe.
using E_learning.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace E_learning.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ElearningDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ElearningDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Crée un nouvel utilisateur.
        /// </summary>
        /// <param name="request">Les informations d'enregistrement de l'utilisateur.</param>
        /// <returns>Un résultat HTTP indiquant la réussite ou l'échec de la création.</returns>
        [HttpPost("create-user")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUser([FromBody] RegisterRequest request)
        {
            // Vérifie si un utilisateur avec cet email existe déjà.
            if (await _context.Utilisateurs.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest("Un utilisateur avec cet email existe déjà.");
            }

            var nouvelUtilisateur = new Utilisateur
            {
                IdUtilisateur = Guid.NewGuid().ToString(),
                Email = request.Email,
                MotDePasse = BCrypt.Net.BCrypt.HashPassword(request.MotDePasse),
                Nom = request.Nom,
                Prenom = request.Prenom,
                TypeUtilisateur = request.TypeUtilisateur,
                DateDeNaissance = request.DateDeNaissance,
                Adresse = request.Adresse,
                NumTel = request.NumTel,
                Identifiant = request.Identifiant,
                EstActif = true 
            };

            _context.Utilisateurs.Add(nouvelUtilisateur);
            await _context.SaveChangesAsync();

            return Ok(nouvelUtilisateur);
        }

        /// <summary>
        /// Gère la connexion des utilisateurs existants.
        /// </summary>
        /// <param name="request">L'objet de requête de connexion avec l'email et le mot de passe.</param>
        /// <returns>Un résultat HTTP avec un token d'authentification ou un message d'erreur.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var utilisateur = await _context.Utilisateurs.SingleOrDefaultAsync(u => u.Email == request.Email);

            if (utilisateur == null)
            {
                return Unauthorized("Email ou mot de passe incorrect.");
            }

            // Vérifie le mot de passe haché
            if (!BCrypt.Net.BCrypt.Verify(request.MotDePasse, utilisateur.MotDePasse))
            {
                return Unauthorized("Email ou mot de passe incorrect.");
            }

            // Génère un JWT pour l'authentification
            var token = GenerateJwtToken(utilisateur);

            // Ajoute le type d'utilisateur à la réponse
            var userType = utilisateur.TypeUtilisateur.ToString();

            return Ok(new { token, userType });
        }

        /// <summary>
        /// Génère un jeton JWT pour un utilisateur donné.
        /// </summary>
        /// <param name="utilisateur">L'objet Utilisateur pour lequel générer le jeton.</param>
        /// <returns>Le jeton JWT sous forme de chaîne.</returns>
        private string GenerateJwtToken(Utilisateur utilisateur)
        {
            var key = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(key))
            {
                throw new System.InvalidOperationException("Clé JWT non configurée.");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, utilisateur.IdUtilisateur),
                new Claim(ClaimTypes.Email, utilisateur.Email),
                new Claim(ClaimTypes.Role, utilisateur.TypeUtilisateur.ToString())
            };

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Gère le téléchargement d'une photo de profil et la lie à un utilisateur existant.
        /// </summary>
        /// <param name="file">Le fichier photo à télécharger.</param>
        /// <returns>Le chemin du fichier si le téléchargement est réussi.</returns>
        [HttpPost("upload-profile-photo")]
        [Authorize(Roles = "Formateur, Administrateur, Etudiant")]
        public async Task<IActionResult> UploadPhotoDeprofil(IFormFile file)
        {
            // Récupérer l'ID de l'utilisateur connecté depuis le token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Utilisateur non authentifié.");
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest("Aucun fichier n'a été sélectionné.");
            }

            var utilisateur = await _context.Utilisateurs.FindAsync(userId);
            if (utilisateur == null)
            {
                return NotFound("Utilisateur non trouvé.");
            }

            try
            {
                var photoPath = await SaveFileAsync(file);

                // Mettre à jour la propriété Photo de l'utilisateur
                utilisateur.Photo = photoPath;

                await _context.SaveChangesAsync();

                return Ok(new { PhotoPath = photoPath });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors du téléchargement de la photo : {ex.Message}");
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
    public class LoginRequest
    {
        public string Email { get; set; }
        public string MotDePasse { get; set; }
    }

    public class RegisterRequest
    {
        public string Email { get; set; }
        public string MotDePasse { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public TypeUtilisateur TypeUtilisateur { get; set; }
        public DateTime DateDeNaissance { get; set; }
        public string Adresse { get; set; }
        public string NumTel { get; set; }
        public string Identifiant { get; set; }
    }
}
