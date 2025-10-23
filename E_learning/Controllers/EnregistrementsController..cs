using E_learning.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration; // Required to access configuration settings

namespace E_learning.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnregistrementsController : ControllerBase
    {
        private readonly ElearningDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _dailyCoApiKey;
        private const string DailyCoBaseUrl = "https://api.daily.co/v1/";

        public EnregistrementsController(ElearningDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _dailyCoApiKey = _configuration["DailyCoApiKey"]; // Retrieve the API key from configuration

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_dailyCoApiKey}");
        }

        // GET: api/Enregistrements
        /// <summary>
        /// Récupère la liste de tous les enregistrements.
        /// </summary>
        /// <returns>Une liste d'objets Enregistrement.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Enregistrement>>> GetEnregistrements()
        {
            return await _context.Enregistrements.ToListAsync();
        }

        // GET: api/Enregistrements/5
        /// <summary>
        /// Récupère un enregistrement par son identifiant.
        /// </summary>
        /// <param name="id">L'identifiant de l'enregistrement.</param>
        /// <returns>L'objet Enregistrement correspondant.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Enregistrement>> GetEnregistrement(string id)
        {
            var enregistrement = await _context.Enregistrements.FindAsync(id);

            if (enregistrement == null)
            {
                return NotFound();
            }

            return enregistrement;
        }

        // GET: api/Enregistrements/by-cours/5
        /// <summary>
        /// Récupère la liste des enregistrements pour un cours spécifique.
        /// </summary>
        /// <param name="coursId">L'identifiant du cours.</param>
        /// <returns>Une liste d'objets Enregistrement.</returns>
        [HttpGet("by-cours/{coursId}")]
        public async Task<ActionResult<IEnumerable<Enregistrement>>> GetEnregistrementsForCourse(string coursId)
        {
            var enregistrements = await _context.Enregistrements
                .Where(e => e.CoursId == coursId)
                .ToListAsync();

            if (enregistrements == null || enregistrements.Count == 0)
            {
                return NotFound($"No recordings found for course with ID: {coursId}");
            }

            return enregistrements;
        }

        // POST: api/Enregistrements
        /// <summary>
        /// Crée un nouvel enregistrement de session de cours.
        /// </summary>
        /// <param name="enregistrement">L'objet Enregistrement à créer.</param>
        /// <returns>Le nouvel objet Enregistrement créé.</returns>
        [HttpPost]
        public async Task<ActionResult<Enregistrement>> PostEnregistrement(Enregistrement enregistrement)
        {
            _context.Enregistrements.Add(enregistrement);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEnregistrement), new { id = enregistrement.IdEnregistrement }, enregistrement);
        }

        // PUT: api/Enregistrements/5
        /// <summary>
        /// Met à jour un enregistrement existant.
        /// </summary>
        /// <param name="id">L'identifiant de l'enregistrement à mettre à jour.</param>
        /// <param name="enregistrement">L'objet Enregistrement avec les nouvelles données.</param>
        /// <returns>Un résultat sans contenu si la mise à jour réussit.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEnregistrement(string id, Enregistrement enregistrement)
        {
            if (id != enregistrement.IdEnregistrement)
            {
                return BadRequest();
            }

            _context.Entry(enregistrement).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EnregistrementExists(id))
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

        // DELETE: api/Enregistrements/5
        /// <summary>
        /// Supprime un enregistrement par son identifiant.
        /// </summary>
        /// <param name="id">L'identifiant de l'enregistrement à supprimer.</param>
        /// <returns>Un résultat sans contenu si la suppression réussit.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnregistrement(string id)
        {
            var enregistrement = await _context.Enregistrements.FindAsync(id);
            if (enregistrement == null)
            {
                return NotFound();
            }

            _context.Enregistrements.Remove(enregistrement);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Enregistrements/create-session/{coursId}
        /// <summary>
        /// Crée une session Daily.co pour un cours avec enregistrement automatique.
        /// </summary>
        /// <param name="coursId">L'identifiant du cours.</param>
        /// <returns>L'URL de la session créée.</returns>
        [HttpPost("create-session/{coursId}")]
        public async Task<ActionResult<string>> CreateDailyCoSession(string coursId)
        {
            var session = new
            {
                name = $"session_cours_{coursId}",
                privacy = "private",
                properties = new
                {
                    enable_prejoin_ui = false,
                    start_video_off = true,
                    start_audio_off = true,
                    enable_chat = true,
                    enable_recordings = "cloud"
                }
            };

            var json = JsonConvert.SerializeObject(session);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{DailyCoBaseUrl}rooms", content);
            if (!response.IsSuccessStatusCode)
            {
                return BadRequest("Failed to create Daily.co session.");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var dailySession = JsonConvert.DeserializeObject<DailySessionResponse>(responseContent);

            // Créer une nouvelle SessionVisioconference dans notre base de données
            var newSession = new SessionVisioconference
            {
                IdSession = dailySession.Name,
                NomSession = $"Session pour le cours {coursId}",
                UrlSession = dailySession.Url,
                DateHeureDebut = DateTime.Now,
                CoursId = coursId
            };
            _context.SessionsVisioconference.Add(newSession);
            await _context.SaveChangesAsync();

            return Ok(dailySession.Url);
        }

        private bool EnregistrementExists(string id)
        {
            return _context.Enregistrements.Any(e => e.IdEnregistrement == id);
        }
    }

    // Classe pour désérialiser la réponse de l'API Daily.co
    public class DailySessionResponse
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public bool Privacy { get; set; }
    }
}
