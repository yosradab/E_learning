using E_learning.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;

namespace E_learning.Controllers
{
    /// <summary>
    /// Represents the absence data received from the API request.
    /// This is a Data Transfer Object (DTO) to prevent model validation errors.
    /// </summary>
    public class AbsenceDto
    {
        public string UtilisateurId { get; set; }
        public bool Justifiee { get; set; }
        public string Motif { get; set; }
        public int PeriodeAbsence { get; set; }
        public DateTime DateAbsence { get; set; }
    }

    /// <summary>
    /// API controller for managing absences.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AbsenceController : ControllerBase
    {
        private readonly ElearningDbContext _context;

        public AbsenceController(ElearningDbContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Retrieves all absence records.
        /// </summary>
        /// <returns>A list of all absences.</returns>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Absence>>> GetAbsences()
        {
            return await _context.Absences.ToListAsync();
        }

        /// <summary>
        /// Retrieves a single absence by its ID.
        /// </summary>
        /// <param name="id">The ID of the absence.</param>
        /// <returns>The absence object or a NotFound result.</returns>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Absence>> GetAbsence(string id)
        {
            var absence = await _context.Absences.FindAsync(id);

            if (absence == null)
            {
                return NotFound();
            }

            return absence;
        }
        /// <summary>
        /// Updates an existing absence record.
        /// Only the user who registered the absence or an administrator can update it.
        /// </summary>
        /// <param name="id">The ID of the absence to update.</param>
        /// <param name="absenceData">The updated absence data.</param>
        /// <returns>A confirmation message or an error.</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrateur, Formateur")]
        public async Task<IActionResult> UpdateAbsence(string id, [FromBody] AbsenceDto absenceData)
        {
            var existingAbsence = await _context.Absences.FindAsync(id);
            if (existingAbsence == null)
            {
                return NotFound();
            }

            var enregistreurId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (existingAbsence.EnregistreurId != enregistreurId && userRole != "Administrateur")
            {
                return Forbid("You are not authorized to update this absence.");
            }

            existingAbsence.Statut = absenceData.Justifiee ? StatutAbsence.Justifiee : StatutAbsence.NonJustifiee;
            existingAbsence.Motif = absenceData.Motif;
            existingAbsence.PeriodeAbsence = absenceData.PeriodeAbsence;
            existingAbsence.DateAbsence = absenceData.DateAbsence;


            _context.Entry(existingAbsence).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Absences.Any(e => e.IdAbsence == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { message = "Absence updated successfully." });
        }
        /// <summary>
        /// Deletes an absence record by its ID.
        /// Only the user who registered the absence or an administrator can delete it.
        /// </summary>
        /// <param name="id">The ID of the absence to delete.</param>
        /// <returns>A confirmation message or a NotFound result.</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrateur, Formateur")]
        public async Task<IActionResult> DeleteAbsence(string id)
        {
            var absence = await _context.Absences.FindAsync(id);
            if (absence == null)
            {
                return NotFound();
            }

            var enregistreurId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (absence.EnregistreurId != enregistreurId && userRole != "Administrateur")
            {
                return Forbid("You are not authorized to delete this absence.");
            }

            _context.Absences.Remove(absence);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Absence deleted successfully." });
        }


        /// <summary>
        /// Registers an absence for a student.
        /// Only trainers and administrators can use this endpoint.
        /// </summary>
        /// <param name="absenceData">The absence data sent from the client.</param>
        /// <returns>A confirmation message or an error.</returns>
        [HttpPost("apprenant")]
        [Authorize(Roles = "Formateur, Administrateur")]
        public async Task<IActionResult> EnregistrerAbsenceApprenant([FromBody] AbsenceDto absenceData)
        {
            // Retrieve the ID of the user who is registering the absence.
            var enregistreurId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(enregistreurId))
            {
                return Unauthorized("User ID not found.");
            }

            // Check if the provided user ID corresponds to a student.
            var apprenant = await _context.Utilisateurs.FindAsync(absenceData.UtilisateurId);
            if (apprenant == null || apprenant.TypeUtilisateur != TypeUtilisateur.Apprenant)
            {
                return BadRequest("The provided user ID does not correspond to a valid student.");
            }

            // Create a new instance of the absence and populate the fields,
            // converting the boolean to the enum.
            var absence = new Absence
            {
                IdAbsence = Guid.NewGuid().ToString(),
                DateAbsence = absenceData.DateAbsence,
                Statut = absenceData.Justifiee ? StatutAbsence.Justifiee : StatutAbsence.NonJustifiee,
                Motif = absenceData.Motif,
                PeriodeAbsence = absenceData.PeriodeAbsence,
                UtilisateurId = absenceData.UtilisateurId,
                EnregistreurId = enregistreurId
            };

            // Add the absence to the database.
            _context.Absences.Add(absence);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Student absence successfully registered." });
        }

        /// <summary>
        /// Registers an absence for a trainer.
        /// Only administrators can use this endpoint.
        /// </summary>
        /// <param name="absenceData">The absence data sent from the client.</param>
        /// <returns>A confirmation message or an error.</returns>
        [HttpPost("formateur")]
        [Authorize(Roles = "Administrateur")]
        public async Task<IActionResult> EnregistrerAbsenceFormateur([FromBody] AbsenceDto absenceData)
        {
            // Retrieve the ID of the user who is registering the absence.
            var enregistreurId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(enregistreurId))
            {
                return Unauthorized("User ID not found.");
            }

            // Check if the provided user ID corresponds to a trainer.
            var formateur = await _context.Utilisateurs.FindAsync(absenceData.UtilisateurId);
            if (formateur == null || formateur.TypeUtilisateur != TypeUtilisateur.Formateur)
            {
                return BadRequest("The provided user ID does not correspond to a valid trainer.");
            }

            // Create a new instance of the absence and populate the fields,
            // converting the boolean to the enum.
            var absence = new Absence
            {
                IdAbsence = Guid.NewGuid().ToString(),
                DateAbsence = absenceData.DateAbsence,
                Statut = absenceData.Justifiee ? StatutAbsence.Justifiee : StatutAbsence.NonJustifiee,
                Motif = absenceData.Motif,
                PeriodeAbsence = absenceData.PeriodeAbsence,
                UtilisateurId = absenceData.UtilisateurId,
                EnregistreurId = enregistreurId
            };

            // Add the absence to the database.
            _context.Absences.Add(absence);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Trainer absence successfully registered." });
        }
    }
}
