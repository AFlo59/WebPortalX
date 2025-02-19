✅ 3️⃣ Initialiser la solution
bash
Copier
Modifier
cd ~/WebPortalX
dotnet new sln -n WebPortalX
✅ 4️⃣ Créer chaque projet et l'ajouter à la solution
📌 Backend API en ASP.NET Core

bash
Copier
Modifier
cd ~/WebPortalX/WebPortalX.API
dotnet new webapi -n WebPortalX.API
cd ~/WebPortalX
dotnet sln add WebPortalX.API/WebPortalX.API.csproj
📌 Frontend

bash
Copier
Modifier
cd ~/WebPortalX/WebPortalX.Frontend
dotnet new webapp -n WebPortalX.Frontend
cd ~/WebPortalX
dotnet sln add WebPortalX.Frontend/WebPortalX.Frontend.csproj
📌 Infrastructure (Base de données et services)

bash
Copier
Modifier
cd ~/WebPortalX/WebPortalX.Infrastructure
dotnet new classlib -n WebPortalX.Infrastructure
cd ~/WebPortalX
dotnet sln add WebPortalX.Infrastructure/WebPortalX.Infrastructure.csproj
📌 Core (Logique métier)

bash
Copier
Modifier
cd ~/WebPortalX/WebPortalX.Core
dotnet new classlib -n WebPortalX.Core
cd ~/WebPortalX
dotnet sln add WebPortalX.Core/WebPortalX.Core.csproj
📌 Tests

bash
Copier
Modifier
cd ~/WebPortalX/WebPortalX.Tests
dotnet new xunit -n WebPortalX.Tests
cd ~/WebPortalX
dotnet sln add WebPortalX.Tests/WebPortalX.Tests.csproj
✅ 5️⃣ Vérifier la solution
📌 Liste les projets pour s'assurer qu'ils sont bien enregistrés dans la solution

bash
Copier
Modifier
dotnet sln list
Tu devrais voir :

markdown
Copier
Modifier
Projet(s)
---------
WebPortalX.API/WebPortalX.API.csproj
WebPortalX.Frontend/WebPortalX.Frontend.csproj
WebPortalX.Infrastructure/WebPortalX.Infrastructure.csproj
WebPortalX.Core/WebPortalX.Core.csproj
WebPortalX.Tests/WebPortalX.Tests.csproj


🚀 Commandes essentielles pour gérer ton projet WebPortalX
Voici toutes les commandes utiles pour build, migrer, et lancer ton projet.

✅ 1. Commandes de base pour la compilation et le build
📌 Nettoyer, restaurer et compiler

bash
Copier
Modifier
cd ~/WebPortalX  # Aller à la racine du projet
dotnet clean      # Supprimer les fichiers compilés
dotnet restore    # Restaurer les dépendances NuGet
dotnet build      # Compiler le projet
📌 Vérifier si tout est bien ajouté dans la solution

bash
Copier
Modifier
dotnet sln list
📌 Si un projet manque dans la solution, l'ajouter

bash
Copier
Modifier
dotnet sln add WebPortalX.Infrastructure/WebPortalX.Infrastructure.csproj
dotnet sln add WebPortalX.API/WebPortalX.API.csproj
dotnet sln add WebPortalX.Frontend/WebPortalX.Frontend.csproj
✅ 2. Commandes pour gérer les migrations Entity Framework
📌 Créer une nouvelle migration

bash
Copier
Modifier
cd ~/WebPortalX/WebPortalX.API
dotnet ef migrations add NomDeLaMigration --project ../WebPortalX.Infrastructure --startup-project WebPortalX.API
📌 Appliquer la migration à la base de données

bash
Copier
Modifier
dotnet ef database update --project ../WebPortalX.Infrastructure --startup-project WebPortalX.API
📌 Lister les migrations appliquées

bash
Copier
Modifier
dotnet ef migrations list --project ../WebPortalX.Infrastructure --startup-project WebPortalX.API
📌 Annuler la dernière migration (si erreur)

bash
Copier
Modifier
dotnet ef migrations remove --project ../WebPortalX.Infrastructure --startup-project WebPortalX.API
📌 Réinitialiser complètement la base de données et recréer les migrations

bash
Copier
Modifier
rm ~/WebPortalX/WebPortalX.API/webportalx.db  # Supprimer la base SQLite
rm -rf ~/WebPortalX/WebPortalX.Infrastructure/Migrations  # Supprimer les migrations existantes
dotnet ef migrations add InitialSetup --project ../WebPortalX.Infrastructure --startup-project WebPortalX.API
dotnet ef database update --project ../WebPortalX.Infrastructure --startup-project WebPortalX.API
✅ 3. Commandes pour exécuter le backend et le frontend
📌 Lancer l’API WebPortalX (Backend, Swagger, JWT, SQLite)

bash
Copier
Modifier
dotnet run --project WebPortalX.API
📌 Accéder à Swagger pour tester l’API
👉 http://localhost:5079/swagger

📌 Lancer le frontend WebPortalX.Frontend (Razor Pages)

bash
Copier
Modifier
dotnet run --project WebPortalX.Frontend
📌 Accéder au frontend
👉 http://localhost:5143

✅ 4. Commandes utiles pour SQLite (Gestion de la base de données)
📌 Lister les tables dans SQLite

bash
Copier
Modifier
sqlite3 ~/WebPortalX/WebPortalX.API/webportalx.db ".tables"
📌 Voir la structure d’une table

bash
Copier
Modifier
sqlite3 ~/WebPortalX/WebPortalX.API/webportalx.db "PRAGMA table_info('UserManager');"
📌 Afficher les 10 premiers utilisateurs

bash
Copier
Modifier
sqlite3 ~/WebPortalX/WebPortalX.API/webportalx.db "SELECT * FROM UserManager LIMIT 10;"
✅ 5. Gestion des logs et des erreurs
📌 Voir les logs de démarrage de l’API

bash
Copier
Modifier
dotnet run --project WebPortalX.API
📌 Vérifier les logs système (journalctl sur Linux, Windows Event Viewer sur Windows)

bash
Copier
Modifier
journalctl -xe | grep WebPortalX
📌 Vérifier les erreurs SQLite

bash
Copier
Modifier
sqlite3 ~/WebPortalX/WebPortalX.API/webportalx.db "PRAGMA integrity_check;"
🚀 Résumé des commandes principales
Action	Commande
Nettoyer et reconstruire le projet	dotnet clean && dotnet restore && dotnet build
Lancer le backend WebPortalX.API	dotnet run --project WebPortalX.API
Lancer le frontend WebPortalX.Frontend	dotnet run --project WebPortalX.Frontend
Créer une migration	dotnet ef migrations add NomMigration --project ../WebPortalX.Infrastructure --startup-project WebPortalX.API
Appliquer les migrations	dotnet ef database update --project ../WebPortalX.Infrastructure --startup-project WebPortalX.API
Vérifier les tables SQLite	sqlite3 ~/WebPortalX/WebPortalX.API/webportalx.db ".tables"
Supprimer la base SQLite et recréer les migrations	rm ~/WebPortalX/WebPortalX.API/webportalx.db && rm -rf ~/WebPortalX/WebPortalX.Infrastructure/Migrations && dotnet ef migrations add InitialSetup --project ../WebPortalX.Infrastructure --startup-project WebPortalX.API && dotnet ef database update --project ../WebPortalX.Infrastructure --startup-project WebPortalX.API


✅ 1️⃣ Supprimer les fichiers inutiles
📌 Supprime les fichiers par défaut créés lors de dotnet new

bash
Copier
Modifier
rm -f ~/WebPortalX/WebPortalX.Infrastructure/Class1.cs
rm -f ~/WebPortalX/WebPortalX.Core/Class1.cs
rm -f ~/WebPortalX/WebPortalX.API/WeatherForecast.cs
rm -f ~/WebPortalX/WebPortalX.API/Controllers/WeatherForecastController.cs
✅ 2️⃣ Vérifier et ajouter les références de projet
📌 Ajoute les références entre les projets pour qu'ils se trouvent entre eux

bash
Copier
Modifier
cd ~/WebPortalX

dotnet add WebPortalX.API/WebPortalX.API.csproj reference WebPortalX.Core/WebPortalX.Core.csproj
dotnet add WebPortalX.API/WebPortalX.API.csproj reference WebPortalX.Infrastructure/WebPortalX.Infrastructure.csproj
dotnet add WebPortalX.Infrastructure/WebPortalX.Infrastructure.csproj reference WebPortalX.Core/WebPortalX.Core.csproj
📌 Vérifie que les références sont bien en place :

bash
Copier
Modifier
dotnet list WebPortalX.API/WebPortalX.API.csproj reference
dotnet list WebPortalX.Infrastructure/WebPortalX.Infrastructure.csproj reference


✅ 3️⃣ Réinstaller les packages NuGet
📌 Réinstalle les dépendances manquantes

bash
Copier
Modifier
cd ~/WebPortalX

dotnet add WebPortalX.Core/WebPortalX.Core.csproj package BCrypt.Net-Next
dotnet add WebPortalX.Infrastructure/WebPortalX.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Sqlite
dotnet add WebPortalX.Infrastructure/WebPortalX.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add WebPortalX.Infrastructure/WebPortalX.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Tools
dotnet add WebPortalX.API/WebPortalX.API.csproj package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add WebPortalX.API/WebPortalX.API.csproj package Microsoft.IdentityModel.Tokens
dotnet add WebPortalX.API/WebPortalX.API.csproj package dotenv.net
dotnet add WebPortalX.API/WebPortalX.API.csproj package Microsoft.EntityFrameworkCore.Sqlite
dotnet add WebPortalX.API/WebPortalX.API.csproj package Microsoft.EntityFrameworkCore
📌 Restaurer toutes les dépendances

bash
Copier
Modifier
dotnet restore


✅ 1️⃣ Installer les packages manquants
Exécute ces commandes pour ajouter les packages requis :

bash
Copier
Modifier
cd ~/WebPortalX

# Ajouter la bibliothèque IdentityModel
dotnet add WebPortalX.API/WebPortalX.API.csproj package IdentityModel

# Ajouter le package pour l'authentification JWT
dotnet add WebPortalX.API/WebPortalX.API.csproj package Microsoft.AspNetCore.Authentication.JwtBearer

# Restaurer les dépendances
dotnet restore
✅ 2️⃣ Recompiler le projet
Vérifie que tout est en place :

bash
Copier
Modifier
dotnet clean
dotnet build
Si tout est bon, continue avec la migration et la base de données :

bash
Copier
Modifier
cd ~/WebPortalX/WebPortalX.API
dotnet ef migrations add InitialSetup --project ../WebPortalX.Infrastructure --startup-project WebPortalX.API
dotnet ef database update --project ../WebPortalX.Infrastructure --startup-project WebPortalX.API