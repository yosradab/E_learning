using E_learning.Models.E_learning.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_learning.Models
{
    public class Resultat
    {
        /// <summary>
        /// Identifiant unique du résultat.
        /// </summary>
        [Key]
        public string IdResultat { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Note obtenue par l'apprenant.
        /// </summary>
        [Required]
        [Range(0, 20)]
        public double Note { get; set; }

        /// <summary>
        /// Remarque ou commentaire du formateur/administrateur.
        /// </summary>
        public string Remarque { get; set; }

        /// <summary>
        /// Résultat final (ex: "Admis", "Refusé", "En attente").
        /// </summary>
        public string ResultatFinal { get; set; }

        /// <summary>
        /// Date et heure de la publication du résultat.
        /// </summary>
        public DateTime DatePublication { get; set; } = DateTime.Now;

        /// <summary>
        /// Clé étrangère vers l'utilisateur (Apprenant) concerné par ce résultat.
        /// </summary>
        [Required]
        [ForeignKey("Apprenant")]
        public string IdApprenant { get; set; }
        public Utilisateur Apprenant { get; set; }

        /// <summary>
        /// Clé étrangère vers l'utilisateur (Formateur ou Admin) qui a posté le résultat.
        /// </summary>
        [Required]
        [ForeignKey("Auteur")]
        public string IdAuteur { get; set; }
        public Utilisateur Auteur { get; set; }

        /// <summary>
        /// Clé étrangère vers le cours auquel le résultat est lié (optionnel).
        /// </summary>
        public string? CoursId { get; set; }
        public Cours? Cours { get; set; }

        /// <summary>
        /// Clé étrangère vers le fichier associé (optionnel).
        /// </summary>
        public string? FichierId { get; set; }
        public Fichier? Fichier { get; set; }
    }
}
