using E_learning.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace E_learning.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionsVisioconferenceController : ControllerBase
    {
        private readonly ElearningDbContext _context;

        public SessionsVisioconferenceController(ElearningDbContext context)
        {
            _context = context;
        }

        // GET: api/SessionsVisioconference
        /// <summary>
        /// Récupère la liste de toutes les sessions de visioconférence.
        /// </summary>
        /// <returns>Une liste d'objets SessionVisioconference.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SessionVisioconference>>> GetSessionsVisioconference()
        {
            return await _context.SessionsVisioconference.ToListAsync();
        }

        // GET: api/SessionsVisioconference/5
        /// <summary>
        /// Récupère une session par son identifiant.
        /// </summary>
        /// <param name="id">L'identifiant de la session.</param>
        /// <returns>L'objet SessionVisioconference correspondant.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<SessionVisioconference>> GetSessionVisioconference(string id)
        {
            var session = await _context.SessionsVisioconference.FindAsync(id);

            if (session == null)
            {
                return NotFound();
            }

            return session;
        }

        // POST: api/SessionsVisioconference
        /// <summary>
        /// Crée une nouvelle session de visioconférence.
        /// </summary>
        /// <param name="session">L'objet SessionVisioconference à créer.</param>
        /// <returns>Le nouvel objet SessionVisioconference créé.</returns>
        [HttpPost]
        public async Task<ActionResult<SessionVisioconference>> PostSessionVisioconference(SessionVisioconference session)
        {
            _context.SessionsVisioconference.Add(session);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSessionVisioconference), new { id = session.IdSession }, session);
        }

        // PUT: api/SessionsVisioconference/5
        /// <summary>
        /// Met à jour les informations d'une session existante.
        /// </summary>
        /// <param name="id">L'identifiant de la session à mettre à jour.</param>
        /// <param name="session">L'objet SessionVisioconference avec les nouvelles données.</param>
        /// <returns>Un résultat sans contenu si la mise à jour réussit.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSessionVisioconference(string id, SessionVisioconference session)
        {
            if (id != session.IdSession)
            {
                return BadRequest();
            }

            _context.Entry(session).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SessionVisioconferenceExists(id))
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

        // DELETE: api/SessionsVisioconference/5
        /// <summary>
        /// Supprime une session de visioconférence par son identifiant.
        /// </summary>
        /// <param name="id">L'identifiant de la session à supprimer.</param>
        /// <returns>Un résultat sans contenu si la suppression réussit.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSessionVisioconference(string id)
        {
            var session = await _context.SessionsVisioconference.FindAsync(id);
            if (session == null)
            {
                return NotFound();
            }

            _context.SessionsVisioconference.Remove(session);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SessionVisioconferenceExists(string id)
        {
            return _context.SessionsVisioconference.Any(e => e.IdSession == id);
        }
    }
}
