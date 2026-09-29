using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DancingBallOnBeats.Data
{
    /// <summary>Serializable snapshot of everything we persist between sessions.</summary>
    [Serializable]
    public class SaveData
    {
        public int totalCoins;
        public bool musicMuted;
        public bool sfxMuted;
        public float musicVolume = 0.8f;
        public float sfxVolume = 1f;
        public List<string> unlockedSongIds = new List<string>();
        public List<HighScoreEntry> highScores = new List<HighScoreEntry>();

        public int GetHighScore(string songId)
        {
            foreach (var entry in highScores)
            {
                if (entry.songId == songId) return entry.score;
            }
            return 0;
        }
    }

    [Serializable]
    public class HighScoreEntry
    {
        public string songId;
        public int score;
        public int bestCombo;
    }

    /// <summary>
    /// Lightweight JSON-file save/load system. Writes to Application.persistentDataPath so it
    /// survives app updates on Android, with a PlayerPrefs fallback if file IO fails
    /// (e.g. restricted storage on some OEM skins).
    /// </summary>
    public static class SaveSystem
    {
        private const string FileName = "dbob_save.json";
        private const string PlayerPrefsFallbackKey = "DBOB_SAVE_FALLBACK";

        private static SaveData _cache;

        public static SaveData Current
        {
            get
            {
                if (_cache == null) _cache = Load();
                return _cache;
            }
        }

        private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        public static SaveData Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    string json = File.ReadAllText(FilePath);
                    var data = JsonUtility.FromJson<SaveData>(json);
                    if (data != null) return data;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] File load failed, trying PlayerPrefs fallback: {e.Message}");
            }

            try
            {
                if (PlayerPrefs.HasKey(PlayerPrefsFallbackKey))
                {
                    string json = PlayerPrefs.GetString(PlayerPrefsFallbackKey);
                    var data = JsonUtility.FromJson<SaveData>(json);
                    if (data != null) return data;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] PlayerPrefs fallback load failed: {e.Message}");
            }

            return new SaveData();
        }

        public static void Save(SaveData data)
        {
            _cache = data;
            string json = JsonUtility.ToJson(data);

            try
            {
                File.WriteAllText(FilePath, json);
                return;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] File save failed, using PlayerPrefs fallback: {e.Message}");
            }

            try
            {
                PlayerPrefs.SetString(PlayerPrefsFallbackKey, json);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] PlayerPrefs fallback save also failed: {e.Message}");
            }
        }

        public static void SubmitScore(string songId, int score, int bestCombo)
        {
            var data = Current;
            var entry = data.highScores.Find(h => h.songId == songId);
            if (entry == null)
            {
                entry = new HighScoreEntry { songId = songId, score = score, bestCombo = bestCombo };
                data.highScores.Add(entry);
            }
            else
            {
                if (score > entry.score) entry.score = score;
                if (bestCombo > entry.bestCombo) entry.bestCombo = bestCombo;
            }
            Save(data);
        }

        public static void UnlockSong(string songId)
        {
            var data = Current;
            if (!data.unlockedSongIds.Contains(songId))
            {
                data.unlockedSongIds.Add(songId);
                Save(data);
            }
        }

        public static bool IsUnlocked(string songId, bool unlockedByDefault)
        {
            if (unlockedByDefault) return true;
            return Current.unlockedSongIds.Contains(songId);
        }

        public static void SetVolumes(float music, float sfx)
        {
            var data = Current;
            data.musicVolume = Mathf.Clamp01(music);
            data.sfxVolume = Mathf.Clamp01(sfx);
            Save(data);
        }

        public static void SetMuted(bool musicMuted, bool sfxMuted)
        {
            var data = Current;
            data.musicMuted = musicMuted;
            data.sfxMuted = sfxMuted;
            Save(data);
        }
    }
}
