# Jellyfin Force Password Change

Ein Plugin, mit dem Administratoren für einzelne lokale Jellyfin-Konten einen Passwortwechsel anfordern können. Das Plugin stellt sowohl eine Verwaltungsseite im Dashboard als auch eine eigenständige, benutzerfreundliche Passwortwechsel-Seite bereit. Offene Anforderungen bleiben nach einem Serverneustart erhalten.

## Deutsch

### Kompatibilität und Status

- Ausschließlich für Jellyfin Server **12.x** vorgesehen.
- Gegen Jellyfin API **12.2.0** gebaut; die Manifest-ABI ist `12.0.0.0`.
- Nicht mit Jellyfin 10.x oder 11.x verwenden.
- Für die Installation über den Jellyfin-Katalog muss dieses GitHub-Repository öffentlich sein. Solange es privat ist, können nur Personen mit GitHub-Zugriff das Plugin herunterladen.

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

Voraussetzung: Jellyfin Server **12.x**. Vor der Installation auf dem eigenen Server ein Backup der Jellyfin-Konfiguration anlegen.

#### Weg 1: Manuelle Installation

1. Lade das neueste Plugin-ZIP von der [Release-Seite](https://github.com/losbaerchenos/jellyfin-plugin-force-password-change/releases/latest) herunter. Für Tests einer noch nicht veröffentlichten Version kannst du stattdessen das `jellyfin-plugin-force-password-change`-Artefakt des letzten erfolgreichen [GitHub-Actions-Builds](https://github.com/losbaerchenos/jellyfin-plugin-force-password-change/actions/workflows/build.yml?query=branch%3Amain) herunterladen.
2. Stoppe Jellyfin. Bei einer Linux-Paketinstallation: `sudo systemctl stop jellyfin`; bei Docker den Container stoppen.
3. Öffne den Plugin-Ordner deiner Installation:

   - Windows-Dienst/Installer: `C:\ProgramData\Jellyfin\Server\plugins`
   - Windows, direkter Benutzerstart: `%LOCALAPPDATA%\jellyfin\plugins`
   - Debian/Ubuntu: `/var/lib/jellyfin/plugins`
   - Docker: `/config/plugins` im Container (das eingebundene `/config`-Volume auf dem Host)

   Linux-Pfade können distributionsabhängig abweichen; verwende den `plugins`-Ordner unter Jellyfins Datenverzeichnis.
4. Erstelle darin den Versionsordner `Force Password Change_1.0.0.0` und entpacke den Inhalt des ZIP-Archivs direkt hinein. Die Datei `Jellyfin.Plugin.ForcePasswordChange.dll` muss direkt im Versionsordner liegen, nicht in einem zusätzlichen Unterordner. Bei einer neueren Plugin-Version den Ordnernamen entsprechend anpassen.
5. Starte Jellyfin wieder. Im Dashboard unter **Plugins → My Plugins** sollte **Force Password Change** als installiert und unterstützt erscheinen.

Das ZIP benötigt keinen Build und kein zusätzliches Entpack- oder Installationswerkzeug. Bei Updates Jellyfin stoppen, den Plugin-Dateiordner durch die neue Version ersetzen und Jellyfin neu starten. Die Konfiguration unter `plugins/configurations` nicht löschen; dort liegen auch die persistenten Passwortwechsel-Anforderungen.

#### Weg 2: Installation über das Plugin-Repository

Das Repository muss öffentlich sein, damit Jellyfin Manifest und ZIP ohne GitHub-Anmeldung laden kann. Solange das GitHub-Repository privat ist, ist nur die manuelle Installation für Personen mit Repository-Zugriff möglich.

1. Melde dich als Administrator im Jellyfin-Web-Dashboard an und öffne **Dashboard → Plugins → Repositories** (in manchen Dashboard-Versionen über das Repository-/Zahnrad-Symbol auf der Plugins-Seite).
2. Wähle **+ / Add repository** und trage ein:

   - **Repository name:** `Force Password Change`
   - **Repository URL:** [`https://github.com/losbaerchenos/jellyfin-plugin-force-password-change/releases/latest/download/manifest.json`](https://github.com/losbaerchenos/jellyfin-plugin-force-password-change/releases/latest/download/manifest.json)

3. Speichere den Eintrag und öffne **Dashboard → Plugins → Catalog**. Aktualisiere den Katalog bei Bedarf, suche **Force Password Change** und installiere die angebotene Version.
4. Starte Jellyfin neu, falls dazu aufgefordert, und prüfe die Installation unter **Dashboard → Plugins → My Plugins**.

Das Manifest und das Plugin-ZIP werden automatisch für jedes Release erzeugt. Die Repository-URL bleibt bei Updates gleich; nach Veröffentlichung eines neuen Releases den Plugin-Katalog in Jellyfin aktualisieren.
Bereits veröffentlichte Plugin-Versionen bleiben im Manifest erhalten und können daher weiterhin im Katalog für Neuinstallationen oder Rollbacks ausgewählt werden.

#### Releases veröffentlichen

Änderungen vor dem Merge auf einer privaten Jellyfin-12.x-Instanz testen und anschließend nach `main` mergen. Bei Änderungen an `src/` oder `tests/` startet der Workflow automatisch: Er führt die Tests aus, erhöht die vierteilige Plugin-Version, erstellt den passenden Tag und veröffentlicht ZIP sowie Manifest als GitHub-Release. Automatische Releases setzen die höchste vorhandene kanonische Version fort; da aktuell `v1.0.0.0` veröffentlicht ist, ist die nächste automatische Version `1.0.0.1`, sofern kein höherer Tag dazukommt. Nichtkanonische Tags (zum Beispiel mit führenden Nullen), veraltete Main-Branch-Läufe und manuelle Tags, die nicht neuer als die höchste veröffentlichte Version sind, werden abgelehnt. Unvollständige Entwürfe und Tags werden bei einem späteren Main-Lauf wiederverwendet oder bereinigt. Ein fehlgeschlagener Build oder Test veröffentlicht kein Release. Ein direkt gepushter Versionstag wird ebenfalls geprüft und nur veröffentlicht, wenn er auf einen bereits in `main` enthaltenen Commit zeigt und die neueste veröffentlichte Version übertrifft.

GitHub Actions muss aktiviert sein. Der Workflow fordert für den isolierten Release-Schritt `contents: write` an; Organisationsrichtlinien dürfen diese Berechtigung nicht blockieren. Nach erfolgreicher Veröffentlichung den Katalog in Jellyfin aktualisieren.

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

GitHub Actions führt Tests und Build bei Pull Requests und Pushes auf `main` aus; diese Builds stellen ein Artefakt für private Tests bereit. Ein Versions-Tag wie `v1.0.0.0` startet den Release-Workflow, der das Plugin testet und baut und anschließend ein versioniertes ZIP sowie das generierte Manifest an ein GitHub-Release hängt. Vor der Veröffentlichung zusätzlich Installation, Dashboard-Konfiguration, Passwortänderung, widerrufene Sitzungen, Reverse-Proxy-Basis-Pfad und Wiederherstellung der JSON-Anforderungen auf der privaten Jellyfin-12.x-Instanz testen.

## English

### Compatibility and status

- Intended exclusively for Jellyfin Server **12.x**.
- Built against Jellyfin API **12.2.0**; the manifest target ABI is `12.0.0.0`.
- Do not use with Jellyfin 10.x or 11.x.
- The GitHub repository must be public for Jellyfin catalog installation. While it is private, only people with GitHub access can download the plugin.

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

Jellyfin Server **12.x** is required. Back up your Jellyfin configuration before installing on your server.

#### Method 1: Manual installation

1. Download the latest plugin ZIP from the [GitHub Releases page](https://github.com/losbaerchenos/jellyfin-plugin-force-password-change/releases/latest). To test a version that has not been released yet, download the `jellyfin-plugin-force-password-change` artifact from the latest successful [GitHub Actions build](https://github.com/losbaerchenos/jellyfin-plugin-force-password-change/actions/workflows/build.yml?query=branch%3Amain).
2. Stop Jellyfin. For a Linux package installation run `sudo systemctl stop jellyfin`; for Docker, stop the container.
3. Open the plugin directory for your installation:

   - Windows service/installer: `C:\ProgramData\Jellyfin\Server\plugins`
   - Windows, direct user launch: `%LOCALAPPDATA%\jellyfin\plugins`
   - Debian/Ubuntu: `/var/lib/jellyfin/plugins`
   - Docker: `/config/plugins` inside the container (the mounted `/config` volume on the host)

   Linux paths may vary by distribution; use the `plugins` folder beneath Jellyfin's data directory.
4. Create a version folder named `Force Password Change_1.0.0.0` and extract the ZIP contents directly into it. `Jellyfin.Plugin.ForcePasswordChange.dll` must be directly inside the version folder, not an additional nested directory. Adjust the folder version for a newer plugin release.
5. Start Jellyfin again. **Dashboard → Plugins → My Plugins** should show **Force Password Change** as installed and supported.

The ZIP does not require a build or a separate installer. To update, stop Jellyfin, replace the plugin files with the new version, and restart the server. Do not delete `plugins/configurations`; it contains plugin settings and persisted password-change requirements.

#### Method 2: Install from the plugin repository

The GitHub repository must be public so Jellyfin can download the manifest and ZIP without GitHub authentication. While the repository is private, only people with GitHub access can install it manually.

1. Sign in to the Jellyfin web dashboard as an administrator and open **Dashboard → Plugins → Repositories** (in some dashboard versions, use the repository/gear icon on the Plugins page).
2. Select **+ / Add repository** and enter:

   - **Repository name:** `Force Password Change`
   - **Repository URL:** [`https://github.com/losbaerchenos/jellyfin-plugin-force-password-change/releases/latest/download/manifest.json`](https://github.com/losbaerchenos/jellyfin-plugin-force-password-change/releases/latest/download/manifest.json)

3. Save the entry and open **Dashboard → Plugins → Catalog**. Refresh the catalog if needed, search for **Force Password Change**, and install the offered version.
4. Restart Jellyfin if prompted, then verify the installation under **Dashboard → Plugins → My Plugins**.

The release workflow automatically generates the manifest and plugin ZIP for each release. The repository URL stays the same; refresh the plugin catalog in Jellyfin after a new release is published.
Previously released plugin versions remain in the manifest and stay available in the catalog for new installations or rollbacks.

#### Publishing a release

Test changes on a private Jellyfin 12.x instance before merging them into `main`. A push to `main` that changes `src/` or `tests/` automatically starts the release workflow: it runs the tests, increments the four-part plugin version, creates the corresponding tag, and publishes the ZIP and manifest as a GitHub release. Automatic releases continue from the highest existing canonical version; because `v1.0.0.0` is currently published, the next automatic version is `v1.0.0.1` unless a higher tag is added first. Noncanonical tags (for example, components with leading zeroes), stale main-branch runs, and manual tags that are not newer than the highest published version are rejected. Incomplete drafts and tags are reused or cleaned up on a later main-branch run. A failed build or test does not publish a release. Pushing a version tag directly also runs the tests and publishes it only if the tagged commit is already contained in `main` and its version exceeds the latest published version.

GitHub Actions must be enabled. The workflow requests `contents: write` only for its isolated release job; organization policies must not block that permission. After the release succeeds, refresh the plugin catalog in Jellyfin.

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

GitHub Actions runs tests and builds for pull requests and pushes to `main`; these builds provide an artifact for private testing. A version tag such as `v1.0.0.0` starts the release workflow, which tests and builds the plugin, then attaches a versioned ZIP and generated manifest to a GitHub release. Before publishing, also test installation, dashboard configuration, password changes, token revocation, reverse-proxy base paths, and restoring the JSON requirements on the private Jellyfin 12.x instance.
