# Jellyfin Force Password Change

Ein Plugin, mit dem Administratoren für einzelne lokale Jellyfin-Konten einen Passwortwechsel anfordern können. Das Plugin stellt sowohl eine Verwaltungsseite im Dashboard als auch eine eigenständige, benutzerfreundliche Passwortwechsel-Seite bereit. Offene Anforderungen bleiben nach einem Serverneustart erhalten.

## Deutsch

### Kompatibilität und Status

- Ausschließlich für Jellyfin Server **12.x** vorgesehen.
- Gegen Jellyfin API **12.2.0** gebaut; die Manifest-ABI ist `12.0.0.0`.
- Nicht mit Jellyfin 10.x oder 11.x verwenden.
- Das Repository ist für private Tests gedacht. Es ist noch kein GitHub-Release mit installierbarem Plugin-ZIP veröffentlicht; deshalb ist die Installation aus dem Jellyfin-Katalog derzeit noch nicht aktiv.

### Funktionen im Überblick

- Administratoren wählen Benutzer im Jellyfin-Dashboard aus und verlangen oder entfernen einen Passwortwechsel.
- Die Anforderungen werden im Jellyfin-Plugin-Konfigurationsverzeichnis in einer JSON-Datei gespeichert.
- Benutzer ändern ihr Passwort auf einer eigenen Plugin-Seite. Die Seite funktioniert unabhängig davon, ob der verwendete Jellyfin-Client selbst eine Plugin-Oberfläche anbietet.
- Die Seite verwendet Jellyfin-Benutzernamen und aktuelles Passwort zur Anmeldung, prüft, ob ein Wechsel erforderlich ist, und zeigt dann Eingabefelder für das neue Passwort an.
- Die Seite ist Deutsch oder Englisch, abhängig von der Spracheinstellung des Browsers.
- Die Mindestlänge beträgt standardmäßig 12 Zeichen und lässt sich auf einen Wert zwischen 8 und 128 Zeichen einstellen.
- Optional kann das Plugin neu gestartete Jellyfin-Sitzungen eines betroffenen Benutzers beenden. Diese Einstellung ist standardmäßig deaktiviert. Für die integrierte Passwortwechsel-Seite muss sie ausgeschaltet sein, sonst beendet das Plugin schon die Sitzung bei der Anmeldung.
- Nach einem erfolgreichen Wechsel entfernt das Plugin die Anforderung und widerruft die übrigen Anmeldetokens des Benutzers. Das Token, mit dem die Passwortseite gerade angemeldet ist, bleibt ausgenommen.

Das Plugin unterstützt lokale Jellyfin-Konten, die von Jellyfin verwaltet werden. Es setzt nicht selbstständig Passwörter für externe Authentifizierungsdienste zurück.

### Installation

Das Plugin kann manuell installiert oder nach Veröffentlichung über ein eigenes Jellyfin-Plugin-Repository hinzugefügt werden. Jellyfin 12.x ist erforderlich. Vor dem Einspielen auf dem privaten Server ein Backup der Jellyfin-Konfiguration anlegen.

#### Weg 1: Manuelle Installation (für private Tests empfohlen)

1. Öffne die [GitHub-Actions-Seite des Projekts](https://github.com/losbaerchenos/jellyfin-plugin-force-password-change/actions) und wähle den letzten erfolgreichen **Build and test**-Workflow auf dem Branch `main`.
2. Lade unter **Artifacts** das Archiv `jellyfin-plugin-force-password-change` herunter. Es ist ein ZIP-Archiv mit `Jellyfin.Plugin.ForcePasswordChange.dll` und den zugehörigen Build-Dateien.
3. Ermittle den Plugin-Ordner deiner Jellyfin-Installation:

   | Installation | Plugin-Ordner |
   |---|---|
   | Windows-Dienst / Installer | `C:\ProgramData\Jellyfin\Server\plugins` |
   | Windows, direkter Benutzerstart | `%LOCALAPPDATA%\jellyfin\plugins` |
   | Debian/Ubuntu Linux | `/var/lib/jellyfin/plugins` |
   | Docker/Container | `/config/plugins` **im Container**; auf dem Host entspricht das dem eingebundenen `/config`-Volume |

   Linux-Paketpfade können je nach Distribution abweichen. Maßgeblich ist das `plugins`-Verzeichnis unter Jellyfins Datenverzeichnis.
4. Beende den Jellyfin-Server, damit keine Plugin-Dateien verwendet werden (Linux-Paketinstallation: `sudo systemctl stop jellyfin`; Docker: den Container stoppen).
5. Erstelle darin den Versionsordner `Force Password Change_1.0.0.0`, zum Beispiel `C:\ProgramData\Jellyfin\Server\plugins\Force Password Change_1.0.0.0` oder `/var/lib/jellyfin/plugins/Force Password Change_1.0.0.0`.
6. Entpacke das Actions-Artefakt-ZIP direkt in diesen Versionsordner. Die Plugin-DLL muss direkt darin liegen und darf nicht noch in einem zusätzlichen Unterordner stecken. Wenn du das Archiv zuerst in einem anderen Ordner entpackt hast, kopiere dessen Dateien in den Versionsordner und behalte die Verzeichnisstruktur bei.
7. Stelle bei Linux- und Docker-Installationen sicher, dass der Jellyfin-Prozess die Dateien lesen kann.
8. Starte Jellyfin wieder (Linux-Paketinstallation: `sudo systemctl start jellyfin`; Docker: den Container starten). Öffne **Dashboard → Plugins → My Plugins** und prüfe, dass **Force Password Change** geladen und unterstützt ist. Falls es fehlt oder als nicht unterstützt angezeigt wird, prüfe Server-Version, Ordnerstruktur, Dateirechte und Jellyfin-Logs.

Für einen lokalen Build sind .NET 10 SDK und Zugriff auf NuGet erforderlich:

```powershell
dotnet test tests/Jellyfin.Plugin.ForcePasswordChange.Tests/Jellyfin.Plugin.ForcePasswordChange.Tests.csproj --configuration Release
dotnet publish src/Jellyfin.Plugin.ForcePasswordChange/Jellyfin.Plugin.ForcePasswordChange.csproj --configuration Release --output output
Compress-Archive -Path output\* -DestinationPath Jellyfin.Plugin.ForcePasswordChange_1.0.0.0.zip -Force
```

Installiere nur, wenn alle Tests erfolgreich sind. Für ein Update zuerst Jellyfin beenden, dann den Inhalt des Plugin-Versionsordners durch die neue Version ersetzen und Jellyfin erneut starten. **Nicht** die Plugin-Konfiguration löschen: Die offenen Passwortwechsel-Anforderungen liegen separat unter `plugins/configurations/Jellyfin.Plugin.ForcePasswordChange.pending-users.json`. Auch andere Plugin-Einstellungen liegen im Konfigurationsverzeichnis.

#### Weg 2: Über ein eigenes Plugin-Repository

Dieser Weg ist möglich, sobald eine getestete Version als GitHub-Release veröffentlicht wurde. Die Plugin-Repository-Liste und die Release-Datei müssen vom Jellyfin-Server ohne GitHub-Anmeldung erreichbar sein. Bei einem privaten GitHub-Repository kann Jellyfin das Manifest und das Paket nicht mit GitHub-Zugangsdaten abrufen; solange das Projekt privat bleiben soll, verwende Weg 1.

1. Stelle sicher, dass das GitHub-Repository und seine Release-Dateien öffentlich und ohne Anmeldung erreichbar sind. Falls das Repository privat bleiben soll, verwende Weg 1; Jellyfin kann keine privaten GitHub-Dateien mit deinen GitHub-Zugangsdaten abrufen.
2. Erstelle für die Version `1.0.0.0` ein GitHub-Release mit dem Tag `v1.0.0.0`. Der Release-Workflow prüft die Tests, baut das Plugin und hängt `Jellyfin.Plugin.ForcePasswordChange_1.0.0.0.zip` an das Release an.
3. Prüfe, dass die Manifest-Datei unter folgender URL öffentlich abrufbar ist:

   `https://raw.githubusercontent.com/losbaerchenos/jellyfin-plugin-force-password-change/main/manifest.json`

   Das Manifest verweist auf das ZIP im Release. Sowohl diese URL als auch das Release-Paket müssen ohne Anmeldung erreichbar sein. Solange das Release nicht veröffentlicht wurde, funktioniert die Katalog-Installation noch nicht.
4. Melde dich als Jellyfin-Administrator im Web-Dashboard an und öffne **Dashboard → Plugins → Repositories**. Je nach Dashboard-Version erreichst du die Verwaltung über das Repository-/Zahnrad-Symbol auf der Plugins-Seite.
5. Wähle **+ / Add repository** und trage ein:

   - **Repository name:** `Force Password Change`
   - **Repository URL:** `https://raw.githubusercontent.com/losbaerchenos/jellyfin-plugin-force-password-change/main/manifest.json`

   Speichere den Eintrag.
6. Öffne **Dashboard → Plugins → Catalog**, aktualisiere bei Bedarf den Katalog und suche nach **Force Password Change**.
7. Wähle Version `1.0.0.0`, installiere das Plugin und starte Jellyfin neu, wenn Jellyfin dazu auffordert.
8. Kontrolliere unter **Dashboard → Plugins → My Plugins**, dass die Version installiert und unterstützt ist.

Für eine spätere Version muss ein neues Release-Paket hochgeladen und der Versions-Eintrag in `manifest.json` mit neuer Versionsnummer, ABI, Änderungsprotokoll, Zeitstempel und direkter ZIP-Download-URL ergänzt werden. Die Repository-URL selbst bleibt gleich; Jellyfin kann den Katalog anschließend aktualisieren.

#### Nach der Installation: Passwortwechsel einrichten

1. Öffne **Dashboard → Plugins → Force Password Change**.
2. Stelle die gewünschte Mindestlänge ein (8 bis 128 Zeichen, Standard: 12) und speichere die Einstellungen. **Anmeldesitzungen blockieren** muss für die integrierte Passwortwechsel-Seite ausgeschaltet bleiben.
3. Wähle unter **Benutzer** das lokale Jellyfin-Konto aus und klicke auf **Passwortwechsel verlangen**.
4. Kopiere den Link **Passwortwechsel-Seite öffnen** und sende ihn dem Benutzer über einen sicheren Kanal.
5. Der Benutzer öffnet den Link, meldet sich mit seinem Jellyfin-Benutzernamen und bisherigen Passwort an und gibt das neue Passwort zweimal ein.
6. Nach der Erfolgsmeldung kann der Benutzer sich mit dem neuen Passwort anmelden. Andere aktive Anmeldetokens dieses Kontos werden widerrufen.

Der Link enthält weder Benutzername noch Passwort oder Anmeldetoken. Er kann grundsätzlich von jedem aufgerufen werden; für den Passwortwechsel ist jedoch die erfolgreiche Anmeldung mit einem Jellyfin-Konto erforderlich, für das eine offene Anforderung besteht.

### HTTPS und Umgang mit Zugangsdaten

Die Passwortwechsel-Seite fordert den Benutzer auf, sein aktuelles Passwort einzugeben. Daher den Link ausschließlich über **HTTPS** aufrufen und teilen; bei einer Reverse-Proxy-Installation muss HTTPS auch am Proxy korrekt eingerichtet sein. Zugangsdaten nicht in URLs, Tickets, Chat-Protokolle oder Screenshots einfügen.

Die Seite verwendet das aktuelle Passwort nur für Jellyfins normale Anmeldung über `POST /Users/AuthenticateByName`. Das daraus erhaltene Zugriffstoken wird nur im Arbeitsspeicher der geöffneten Seite gehalten und weder in `localStorage` noch `sessionStorage` abgelegt. Nach erfolgreichem Wechsel wird es aus dem Speicher der Seite entfernt. Die Plugin-API und die Seite geben das neue Passwort nicht in Logs aus. Der Server speichert das Passwort ausschließlich über Jellyfins `IUserManager`.

Die eigenständige HTML-Seite selbst ist ohne Anmeldung abrufbar, damit sie im Browser direkt geöffnet werden kann. Die Status- und Passwortwechsel-API-Endpunkte erfordern weiterhin eine gültige Jellyfin-Authentifizierung. Der Plugin-Endpunkt darf daher nicht als Ersatz für HTTPS oder eine sichere Serverfreigabe betrachtet werden.

### Verwaltungsseite und Benutzerablauf

Die Verwaltungsseite befindet sich im Jellyfin-Dashboard unter **Plugins → Force Password Change**. Dort können Administratoren:

- eine Mindestlänge von 8 bis 128 Zeichen speichern;
- die optionale Blockierung neu gestarteter Sitzungen aktivieren oder deaktivieren;
- einen Jellyfin-Benutzer auswählen;
- den Passwortwechsel verlangen oder eine bestehende Anforderung aufheben;
- den Link zur selbstständigen Passwortänderung öffnen und kopieren.

Der Benutzerablauf ist bewusst eigenständig: Der Administrator teilt den Link, der Benutzer meldet sich dort mit dem bisherigen Jellyfin-Passwort an, die Seite fragt den persönlichen Anforderungsstatus ab und zeigt bei offener Anforderung das Formular an. Der Benutzer muss das neue Passwort zweimal eingeben. Nach der serverseitigen Mindestlängenprüfung wird das Passwort geändert, die Anforderung gelöscht und Jellyfin widerruft die anderen Tokens dieses Benutzers. Die Seite zeigt anschließend die Erfolgsmeldung; die Anmeldung bei Jellyfin erfolgt mit dem neuen Passwort.

Die Seite verwendet den Jellyfin-Server und dessen Basis-Pfad automatisch. Bei einem Server hinter einem Reverse Proxy mit Präfix (beispielsweise `/jellyfin`) sollte der Administrator den im Dashboard erzeugten vollständigen Link teilen, statt den Plugin-Pfad manuell zusammenzusetzen.

### Plugin-Endpunkte

Alle Plugin-Routen beginnen mit `/Plugin/ForcePasswordChange`.

| Methode | Pfad | Zugriff | Beschreibung |
|---|---|---|---|
| `GET` | `/PasswordChangePage` | Seite öffentlich abrufbar; Passwortaktionen benötigen Anmeldung | Liefert die eigenständige, lokalisierte HTML-Oberfläche. Der Pfad bleibt auch mit einem Jellyfin-Basis-Pfad verwendbar. |
| `POST` | `/User/{userId}/Require` | Administrator | Verlangt den Passwortwechsel für einen existierenden Benutzer. |
| `POST` | `/User/{userId}/Clear` | Administrator | Hebt die Anforderung auf. |
| `GET` | `/RequiredUsers` | Administrator | Gibt die IDs aller Benutzer mit offener Anforderung zurück. |
| `GET` | `/Status` | Angemeldeter Benutzer | Gibt den eigenen Status und die konfigurierte Mindestlänge zurück. |
| `POST` | `/ChangePassword` | Angemeldeter Benutzer | Ändert das eigene Passwort nur, wenn für das Konto eine Anforderung besteht. |

`GET /Status` antwortet beispielsweise mit:

```json
{
  "passwordChangeRequired": true,
  "minimumPasswordLength": 12
}
```

Für `POST /ChangePassword` wird ein Jellyfin-Anmeldetoken im normalen `X-Emby-Token`-Header und folgender JSON-Body verwendet:

```json
{
  "newPassword": "ein-neues-passwort-mit-mindestens-12-zeichen"
}
```

Die Authentifizierung erfolgt pro Benutzerkonto. Ein Benutzer kann nicht über diesen Endpunkt das Passwort eines anderen Kontos setzen. Bei ungültigem Anmeldetoken, fehlender Anforderung oder zu kurzem Passwort wird die Änderung abgewiesen.

### Sitzungsblockierung

Die Option **Anmeldesitzungen blockieren** beendet Sitzungen, sobald eine neue Sitzung für ein Konto mit offener Anforderung beginnt. Sie ist standardmäßig aus. Für die integrierte Passwortwechsel-Seite muss sie ausgeschaltet bleiben, weil sie sonst direkt die Anmeldung des Benutzers beendet. Die Option ist nur für eine eigene Client-Anwendung mit eigenem Authentifizierungs- und Passwortwechsel-Ablauf geeignet. Wenn der Benutzer bereits ausgesperrt ist, muss ein Administrator die Blockierung im Dashboard deaktivieren oder die Anforderung aufheben.

### Tests, CI und Freigabe

Die Tests überprüfen das dauerhafte Speichern und Löschen offener Anforderungen sowie Standardkonfiguration und ungültige Benutzer-IDs:

```powershell
dotnet test tests/Jellyfin.Plugin.ForcePasswordChange.Tests/Jellyfin.Plugin.ForcePasswordChange.Tests.csproj --configuration Release
```

GitHub Actions führt Tests und Build bei Pull Requests und Pushes auf `main` aus. Das veröffentlichte Artefakt ist ein Build-Artefakt des Workflows und noch keine öffentliche Plugin-Veröffentlichung. Vor einer öffentlichen Freigabe zusätzlich Installation, Dashboard-Konfiguration, Passwortänderung, widerrufene Sitzungen, Reverse-Proxy-Basis-Pfad und Wiederherstellung der JSON-Anforderungen auf der privaten Jellyfin-12.x-Instanz testen.

## English

### Compatibility and status

- Intended exclusively for Jellyfin Server **12.x**.
- Built against Jellyfin API **12.2.0**; the manifest target ABI is `12.0.0.0`.
- Do not use with Jellyfin 10.x or 11.x.
- This repository is intended for private testing. No GitHub release containing an installable plugin ZIP has been published yet, so installation from the Jellyfin catalog is not active yet.

### Features at a glance

- Administrators select users in the Jellyfin dashboard and require or clear a password change.
- Requirements are stored in a JSON file in Jellyfin's plugin configuration directory.
- Users change their passwords on a dedicated plugin page. The page works even when their Jellyfin client does not provide a plugin-specific UI.
- The page signs in with the user's Jellyfin username and current password, checks whether a change is required, and then displays the new-password form.
- The page is displayed in German or English according to the browser language.
- The default minimum length is 12 characters and can be configured from 8 to 128.
- Optionally, the plugin can log out newly started Jellyfin sessions for a user with a pending requirement. This setting is disabled by default. It must stay disabled when using the built-in password-change page, otherwise the plugin logs the user out as soon as they sign in.
- After a successful change, the requirement is cleared and the user's other authentication tokens are revoked. The token used by the password page remains active.

The plugin supports local Jellyfin accounts managed by Jellyfin. It does not reset passwords at external authentication providers.

### Installation

You can install the plugin manually or add its own Jellyfin plugin repository after a release has been published. Jellyfin 12.x is required. Back up the Jellyfin configuration before installing on your private server.

#### Method 1: Manual installation (recommended for private testing)

1. Open the [project's GitHub Actions page](https://github.com/losbaerchenos/jellyfin-plugin-force-password-change/actions) and select the latest successful **Build and test** workflow run on the `main` branch.
2. Download the `jellyfin-plugin-force-password-change` archive under **Artifacts** and extract it. It contains `Jellyfin.Plugin.ForcePasswordChange.dll` and its build files.
3. Find the plugin directory for your Jellyfin installation:

   | Installation | Plugin directory |
   |---|---|
   | Windows service / installer | `C:\ProgramData\Jellyfin\Server\plugins` |
   | Windows, direct user launch | `%LOCALAPPDATA%\jellyfin\plugins` |
   | Debian/Ubuntu Linux | `/var/lib/jellyfin/plugins` |
   | Docker/container | `/config/plugins` **inside the container**; on the host this is under the mounted `/config` volume |

   Linux package paths may vary by distribution. Use the `plugins` directory beneath Jellyfin's data directory.
4. Stop the Jellyfin server so no plugin files are in use.
5. Create a version directory named `Force Password Change_1.0.0.0`, for example `C:\ProgramData\Jellyfin\Server\plugins\Force Password Change_1.0.0.0` or `/var/lib/jellyfin/plugins/Force Password Change_1.0.0.0`.
6. Copy the extracted archive files directly into that version directory. The plugin DLL must be directly inside it, not inside another nested directory. Preserve the archive's directory structure.
7. On Linux and Docker, make sure the Jellyfin process can read the files.
8. Start Jellyfin again. Open **Dashboard → Plugins → My Plugins** and verify **Force Password Change** is loaded and supported. If it is missing or unsupported, check the server version, directory structure, file permissions, and Jellyfin logs.

To build locally, you need the .NET 10 SDK and access to NuGet:

```powershell
dotnet test tests/Jellyfin.Plugin.ForcePasswordChange.Tests/Jellyfin.Plugin.ForcePasswordChange.Tests.csproj --configuration Release
dotnet publish src/Jellyfin.Plugin.ForcePasswordChange/Jellyfin.Plugin.ForcePasswordChange.csproj --configuration Release --output output
Compress-Archive -Path output\* -DestinationPath Jellyfin.Plugin.ForcePasswordChange_1.0.0.0.zip -Force
```

Install only if all tests pass. To update, stop Jellyfin, replace the files in the plugin version directory with the new version, and start Jellyfin again. **Do not** delete the plugin configuration: pending password-change requirements are stored separately in `plugins/configurations/Jellyfin.Plugin.ForcePasswordChange.pending-users.json`. Other plugin settings are also in the configuration directory.

#### Method 2: Install from a custom plugin repository

This method becomes available once a tested version has been published as a GitHub release. The Jellyfin server must be able to fetch the repository manifest and release archive without signing in to GitHub. Jellyfin cannot fetch files from a private GitHub repository using your GitHub credentials, so use Method 1 while the project is private.

1. Make sure the GitHub repository and its release files are public and accessible without signing in. If you want to keep the repository private, use Method 1 because Jellyfin cannot fetch private GitHub files with your GitHub credentials.
2. Create a GitHub release for version `1.0.0.0` with the tag `v1.0.0.0`. The release workflow tests and builds the plugin, then attaches `Jellyfin.Plugin.ForcePasswordChange_1.0.0.0.zip` to the release.
3. Verify that the manifest is publicly available at:

   `https://raw.githubusercontent.com/losbaerchenos/jellyfin-plugin-force-password-change/main/manifest.json`

   The manifest points to the ZIP attached to the release. Both the manifest URL and release package must be reachable without authentication. Catalog installation will not work until the release has been published.
4. Sign in to the Jellyfin web dashboard as an administrator and open **Dashboard → Plugins → Repositories**. Depending on the dashboard version, repository management may be available using the repository/gear icon on the Plugins page.
5. Select **+ / Add repository** and enter:

   - **Repository name:** `Force Password Change`
   - **Repository URL:** `https://raw.githubusercontent.com/losbaerchenos/jellyfin-plugin-force-password-change/main/manifest.json`

   Save the entry.
6. Open **Dashboard → Plugins → Catalog**, refresh the catalog if needed, and search for **Force Password Change**.
7. Select version `1.0.0.0`, install the plugin, and restart Jellyfin if prompted.
8. Under **Dashboard → Plugins → My Plugins**, verify that the version is installed and supported.

For a later version, publish a new release package and add a version entry to `manifest.json` with the new version, ABI, changelog, timestamp, and direct ZIP download URL. The repository URL remains unchanged; Jellyfin can refresh the catalog afterward.

#### After installation: set up a password change

1. Open **Dashboard → Plugins → Force Password Change**.
2. Set and save the desired minimum length (8 to 128 characters, default: 12). **Block sessions until changed** must remain disabled for the built-in password-change page.
3. Select the local Jellyfin account in the **Users** section and click **Require password change**.
4. Copy the **Passwortwechsel-Seite öffnen** link and send it to the user through a secure channel.
5. The user opens the link, signs in with their Jellyfin username and current password, and enters the new password twice.
6. After the success message, the user can sign in with the new password. Other active authentication tokens for this account are revoked.

The link contains no username, password, or authentication token. Anyone can open it, but changing a password requires successful authentication as a Jellyfin account with a pending requirement.

### HTTPS and credential handling

The password-change page asks users to enter their current password. Open and share it **only over HTTPS**; with a reverse proxy, HTTPS must also be correctly configured at the proxy. Do not put credentials in URLs, tickets, chat logs, or screenshots.

The page uses the current password only for Jellyfin's normal `POST /Users/AuthenticateByName` sign-in. The resulting access token stays only in the open page's memory and is not stored in `localStorage` or `sessionStorage`. It is removed from the page after a successful change. The plugin API and page do not log the new password. Jellyfin's `IUserManager` is used to store the password.

The standalone HTML page itself can be fetched without authentication so that it can be opened directly in a browser. The status and password-change API endpoints still require valid Jellyfin authentication. The plugin endpoint is not a substitute for HTTPS or safe server exposure.

### Dashboard and user workflow

The management page is available in the Jellyfin dashboard under **Plugins → Force Password Change**. Administrators can:

- save a minimum password length from 8 to 128 characters;
- enable or disable the optional blocking of newly started sessions;
- select a Jellyfin user;
- require a password change or clear an existing requirement;
- open and copy the self-service password-change link.

The user workflow is self-contained: the administrator shares the link, the user signs in there with their current Jellyfin password, and the page checks that user's requirement status. When a change is pending, the user must enter the new password twice. After the server validates the configured minimum length, it changes the password, clears the requirement, and Jellyfin revokes the user's other tokens. The page displays a success message; the user can then sign in to Jellyfin with the new password.

The page automatically uses the Jellyfin server and its base path. If the server is behind a reverse proxy with a URL prefix (for example, `/jellyfin`), share the complete link generated in the dashboard rather than constructing the plugin path manually.

### Plugin endpoints

All plugin routes are under `/Plugin/ForcePasswordChange`.

| Method | Path | Access | Description |
|---|---|---|---|
| `GET` | `/PasswordChangePage` | Page is publicly fetchable; password actions require sign-in | Serves the standalone, localized HTML page. The route also works with a Jellyfin base path. |
| `POST` | `/User/{userId}/Require` | Administrator | Require a password change for an existing user. |
| `POST` | `/User/{userId}/Clear` | Administrator | Clear a pending requirement. |
| `GET` | `/RequiredUsers` | Administrator | Return IDs of users with a pending requirement. |
| `GET` | `/Status` | Authenticated user | Return the current user's status and configured minimum length. |
| `POST` | `/ChangePassword` | Authenticated user | Change the current user's password only when a requirement is pending. |

For example, `GET /Status` returns:

```json
{
  "passwordChangeRequired": true,
  "minimumPasswordLength": 12
}
```

`POST /ChangePassword` requires a Jellyfin user access token in the normal `X-Emby-Token` header and this JSON body:

```json
{
  "newPassword": "a-new-password-at-least-12-characters-long"
}
```

Authentication is scoped to the signed-in user. A user cannot use this endpoint to set another account's password. Requests are rejected for invalid authentication, no pending requirement, or a password shorter than the configured minimum.

### Session blocking

The **Block sessions until changed** option logs out sessions when a new session starts for an account with a pending requirement. It is disabled by default. It must remain disabled for the built-in password-change page because otherwise it ends the user's session immediately after sign-in. This option is only suitable for a custom client with its own authentication and password-change flow. If a user is locked out, an administrator must disable blocking in the dashboard or clear the requirement.

### Tests, CI, and release

Tests cover persistence and clearing of pending requirements, default configuration, and invalid user IDs:

```powershell
dotnet test tests/Jellyfin.Plugin.ForcePasswordChange.Tests/Jellyfin.Plugin.ForcePasswordChange.Tests.csproj --configuration Release
```

GitHub Actions runs tests and builds for pull requests and pushes to `main`. Its published output is a workflow build artifact, not a public plugin release. Before a public release, also test installation, dashboard configuration, password changes, token revocation, reverse-proxy base paths, and restoring the JSON requirements on the private Jellyfin 12.x instance.
