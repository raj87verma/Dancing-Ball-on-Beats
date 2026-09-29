# How to Build a .apk You Can Install on Your Phone

This repo has a GitHub Actions workflow that builds an Android `.apk` file
for you automatically — Unity runs entirely on GitHub's servers. You don't
need to install Unity, Android Studio, or anything else on your own
computer.

There is a **one-time setup** step (about 10 minutes) that requires
installing the free **Unity Hub** app just once, to generate a small
license file. After that, every future build is 100% automatic on GitHub.

---

## One-time setup

### Step 1 — Create a free Unity account (skip if you already have one)
Go to https://id.unity.com/ and sign up with an email + password.

### Step 2 — Install Unity Hub (only Unity Hub, not the full Editor)
1. Download Unity Hub: https://unity.com/download
2. Install it and open it.
3. Sign in with the Unity account from Step 1.
4. Go to **Preferences (or Settings) → Licenses → Add** → choose
   **"Get a free personal license"** and confirm.
   - Even if a license already shows up in the list, click through
     "Add" again to make sure a license *file* gets created on disk
     (this is what the next step needs).

### Step 3 — Find the license file
Unity Hub creates a file called `Unity_lic.ulf` on your computer:
- **Windows:** `C:\ProgramData\Unity\Unity_lic.ulf`
- **Mac:** `/Library/Application Support/Unity/Unity_lic.ulf`
- **Linux:** `~/.local/share/unity3d/Unity/Unity_lic.ulf`

(These folders can be hidden — you may need to enable "show hidden
files" in your file explorer.)

Open this file in Notepad (or any text editor) and copy its **entire
contents**.

### Step 4 — Add 3 secrets to your GitHub repo
1. Go to https://github.com/raj87verma/Dancing-Ball-on-Beats
2. **Settings → Secrets and variables → Actions → New repository secret**
3. Create these three secrets:

| Secret name | Value |
|---|---|
| `UNITY_LICENSE` | Paste the entire contents of the `Unity_lic.ulf` file from Step 3 |
| `UNITY_EMAIL` | Your Unity account email |
| `UNITY_PASSWORD` | Your Unity account password |

That's it — setup is done. You can uninstall Unity Hub now if you want;
it was only needed to generate the license file once.

---

## Building the APK (every time you want a new build)

1. Go to https://github.com/raj87verma/Dancing-Ball-on-Beats/actions
2. Click **"Build Android APK"** in the left sidebar.
3. Click **Run workflow** → **Run workflow** (green button).
4. Wait roughly 20–40 minutes (first run is slower; Unity has to
   download and cache on GitHub's server).
5. When the run finishes with a green checkmark, click into it and
   scroll to the **Artifacts** section at the bottom.
6. Download `DancingBallOnBeats-apk` — it's a `.zip`, and inside it is
   your `.apk` file.

## Installing on your phone

1. Transfer the `.apk` file to your Android phone (email, WhatsApp,
   Google Drive, USB cable — any method works).
2. Open the file on your phone. Android will ask permission to
   "install from unknown sources" the first time — allow it.
3. Tap Install, then open the app and play.

This is a **debug build**, meant for testing on your own device. It is
not signed for the Play Store — that's a separate step for later, once
you're ready to publish.

## If the build fails

Open the failed run, click the red ❌ step to expand its log, and copy
the error text. Common causes:
- **License activation error / "no valid Unity license"** → double check
  the `UNITY_LICENSE` secret has the *entire* file contents (including
  the first and last lines), and that `UNITY_EMAIL`/`UNITY_PASSWORD` are
  correct.
- **A compile error mentioning a `.cs` file** → a script has a bug;
  share the exact error text so it can be fixed.
- **Personal license expired** → Personal licenses need reactivating
  every so often; repeat Steps 2–4 to get a fresh `Unity_lic.ulf`.
