# Jellyfin Force Password Change

Ein Plugin, mit dem Administratoren für einzelne lokale Jellyfin-Konten einen Passwortwechsel anfordern können. Das Plugin stellt sowohl eine Verwaltungsseite im Dashboard als auch eine eigenständige, benutzerfreundliche Passwortwechsel-Seite bereit. Offene Anforderungen bleiben nach einem Serverneustart erhalten.

## Deutsch

### Kompatibilität und Status

- Ausschließlich für Jellyfin Server **12.x** vorgesehen.
- Gegen Jellyfin API **12.2.0** gebaut; die Manifest-ABI ist `12.0.0.0`.
- Nicht mit Jellyfin 10.x verwenden.
- Das Repository ist für private Tests gedacht. Es gibt noch keine öffentliche Plugin-Version und keine Plugin-Repository-URL.

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

### Installation für private Tests

Vor der Installation auf einem Produktivserver zuerst auf einer privaten Jellyfin-12.x-Testinstanz prüfen und Server- sowie Plugin-Konfiguration sichern.

Voraussetzungen zum Erstellen: .NET 10 SDK und Zugriff auf NuGet.

```powershell
dotnet restore tests/Jellyfin.Plugin.ForcePasswordChange.Tests/Jellyfin.Plugin.ForcePasswordChange.Tests.csproj
dotnet test tests/Jellyfin.Plugin.ForcePasswordChange.Tests/Jellyfin.Plugin.ForcePasswordChange.Tests.csproj --configuration Release
dotnet publish src/Jellyfin.Plugin.ForcePasswordChange/Jellyfin.Plugin.ForcePasswordChange.csproj --configuration Release --output output
```

Der CI-Workflow führt ebenfalls Restore, Tests und Veröffentlichung aus und stellt das Build-Artefakt bereit. Erst installieren, wenn der Build und alle Tests erfolgreich waren:

1. Das veröffentlichte Plugin-Artefakt in ein eigenes Plugin-Verzeichnis auf dem Jellyfin-Server installieren.
2. Jellyfin neu starten.
3. Im Dashboard kontrollieren, dass **Force Password Change** geladen und unterstützt angezeigt wird. Bei einem ABI- oder Ladefehler die Installation abbrechen und die Server-Logs prüfen.
4. **Plugins → Force Password Change** öffnen.
5. Mindestlänge einstellen und speichern. **Anmeldesitzungen blockieren** ausgeschaltet lassen, damit der Benutzer sich auf der Passwortwechsel-Seite anmelden kann.
6. Im Bereich **Benutzer** ein Konto auswählen und **Passwortwechsel verlangen** betätigen.
7. Den angezeigten Link **Passwortwechsel-Seite öffnen** kopieren und dem Benutzer über einen sicheren Kanal schicken.
8. Den Benutzer bitten, den Link zu öffnen, sich mit seinem Jellyfin-Benutzernamen und bisherigen Passwort anzumelden und anschließend ein neues Passwort einzugeben und zu bestätigen.
9. Nach der Erfolgsmeldung mit dem neuen Passwort bei Jellyfin anmelden. Andere aktive Anmeldetokens dieses Benutzers sind nach dem Wechsel widerrufen.

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
- This repository is intended for private testing. There is no public plugin release or plugin repository URL yet.

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

### Installation for private testing

Test on a private Jellyfin 12.x instance before installing on a production server. Back up the server and plugin configuration first.

Build requirements: .NET 10 SDK and access to NuGet.

```powershell
dotnet restore tests/Jellyfin.Plugin.ForcePasswordChange.Tests/Jellyfin.Plugin.ForcePasswordChange.Tests.csproj
dotnet test tests/Jellyfin.Plugin.ForcePasswordChange.Tests/Jellyfin.Plugin.ForcePasswordChange.Tests.csproj --configuration Release
dotnet publish src/Jellyfin.Plugin.ForcePasswordChange/Jellyfin.Plugin.ForcePasswordChange.csproj --configuration Release --output output
```

The CI workflow also restores dependencies, runs tests, publishes the plugin, and uploads the build artifact. Install only after the build and all tests pass:

1. Install the published plugin artifact into its own plugin directory on the Jellyfin server.
2. Restart Jellyfin.
3. Verify in the dashboard that **Force Password Change** is loaded and supported. If there is an ABI or loading error, stop and check the server logs.
4. Open **Plugins → Force Password Change**.
5. Set and save the minimum password length. Leave **Block sessions until changed** disabled so the user can sign in to the password-change page.
6. Select an account in the **Users** section and choose **Require password change**.
7. Copy the displayed **Passwortwechsel-Seite öffnen** link and send it to the user through a secure channel.
8. Ask the user to open the link, sign in with their Jellyfin username and current password, and then enter and confirm a new password.
9. After the success message, sign in to Jellyfin with the new password. Other active authentication tokens for the user have been revoked.

The link contains no username, password, or authentication token. It can be opened by anyone, but changing a password requires successful authentication as a Jellyfin account with a pending requirement.

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
