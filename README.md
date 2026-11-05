# E-learning — API de plateforme de formation en ligne (ASP.NET Core)

API REST d'une **plateforme e-learning** : gestion des cours, des formateurs et des apprenants, exercices, résultats, absences et **classes virtuelles** en visioconférence avec enregistrement.

## Rôles

| Rôle | Accès |
|---|---|
| **Administrateur** | Gestion des comptes (création, bannissement, réactivation), des cours et des affectations |
| **Formateur** | Ses cours, ses apprenants, exercices, résultats, absences, sessions visio |
| **Apprenant** | Ses cours, exercices, résultats |

## Fonctionnalités

- **Authentification JWT**, mots de passe hachés avec **BCrypt**, changement de mot de passe, photo de profil
- **Gestion des utilisateurs** : inscription, liste des formateurs et des apprenants, bannissement / activation
- **Cours** : CRUD, filtrage par niveau, affectation de formateurs et d'apprenants, upload de supports
- **Exercices** par cours, avec dépôt de fichiers
- **Résultats** des apprenants
- **Absences** des apprenants et des formateurs
- **Visioconférence** : création de salles via l'**API Daily.co** et gestion des **enregistrements** par cours
- **Export** des fichiers en **PDF** et en **Excel** (`/api/Export/pdf/{id}`, `/api/Export/excel/{id}`)
- Documentation **Swagger**

## Endpoints principaux

| Contrôleur | Préfixe |
|---|---|
| Authentification | `/api/Auth` (`login`, `create-user`, `upload-profile-photo`) |
| Utilisateurs | `/api/Utilisateurs` |
| Cours | `/api/Cours` |
| Exercices | `/api/Exercices` |
| Résultats | `/api/Resultats` |
| Absences | `/api/Absence` |
| Sessions visio | `/api/SessionsVisioconference` |
| Enregistrements | `/api/Enregistrements` |
| Export | `/api/Export` |

## Stack technique

| Élément | Technologie |
|---|---|
| Framework | ASP.NET Core Web API (.NET 8) |
| ORM | Entity Framework Core 9 (migrations) |
| Base de données | SQL Server |
| Sécurité | JWT Bearer, BCrypt.Net |
| Visioconférence | API Daily.co |
| Export | iText 7, PdfSharpCore, EPPlus |
| Documentation | Swashbuckle (Swagger) |

## Lancer le projet

Prérequis : SDK .NET 8, SQL Server (ou LocalDB).

1. Vérifier la chaîne de connexion et la clé JWT dans `E_learning/appsettings.json` :
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=ElearningDB;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```
   Ajouter aussi la clé `DailyCoApiKey` pour la visioconférence.
2. Appliquer les migrations puis lancer l'API :
   ```bash
   cd E_learning
   dotnet ef database update
   dotnet run
   ```
3. Swagger : http://localhost:5110/swagger

Le projet s'ouvre aussi directement dans Visual Studio via `E_learning.sln`.

## Auteur

**Yosra Dab** — Ingénieure logiciel Full Stack · [Portfolio](https://yosradab.vercel.app) · [GitHub](https://github.com/yosradab) · [LinkedIn](https://linkedin.com/in/yosradab)
