using System;
using System.IO;
using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// Versioned snapshot DTOs. Unity JsonUtility serializes public fields,
    /// so arrays and plain classes are used rather than dictionaries.
    /// </summary>
    [Serializable] public sealed class NecroSaveData
    {
        public int version=1;
        // Missing/zero in old saves: world map snapshot is incompatible, hero progression is not.
        public int worldLayoutRevision;
        public int stage,level,experience,mana,maxMana,shards,kills,gathered;
        public int staffTier,mantleTier,lifeRitual,vaultRitual,spiritRitual;
        // Optional 0.9.0 attributes: prior v1 save JSON loads these as zero.
        public int vitalityPoints,powerPoints,agilityPoints;
        // 0.9.1: six unlocks, each stored as a bit; missing in older saves.
        public int learnedSkills;
        public bool spellUnlocked,huntOrder,completed;
        public float hp,maxHp,playerX,playerY;
        public string[] rankKinds=new string[0];
        public int[] rankValues=new int[0];
        public NecroActorRecord[] army=new NecroActorRecord[0];
        public NecroActorRecord[] vault=new NecroActorRecord[0];
        public NecroStageRecord[] stages=new NecroStageRecord[0];
    }

    [Serializable] public sealed class NecroActorRecord
    {
        public string kind;
        public float x,y,hp,maxHp,attack,cooldown;
    }
    [Serializable] public sealed class NecroCorpseRecord
    {
        public string kind;
        public float x,y,time;
    }
    [Serializable] public sealed class NecroItemRecord
    {
        public float x,y;
        public int slot,tier;
    }
    [Serializable] public sealed class NecroResourceRecord
    {
        public string type;
        public float x,y;
    }
    [Serializable] public sealed class NecroStageRecord
    {
        public bool visited,claimed,bossDefeated,bossBound;
        // Optional v1 field: false for snapshots from before resource spawning
        // was enabled; true means an empty resources array is intentional.
        public bool resourcesInitialized;
        // Optional discovery mask: older saves safely begin with fresh fog.
        public string exploration;
        public NecroActorRecord[] enemies=new NecroActorRecord[0];
        public NecroCorpseRecord[] corpses=new NecroCorpseRecord[0];
        public NecroItemRecord[] drops=new NecroItemRecord[0];
        public NecroResourceRecord[] resources=new NecroResourceRecord[0];
    }

    /// <summary>
    /// Local Android save with a recovery copy. A temporary file is written first,
    /// then the previous save is backed up before replacement.
    /// File operations are kept outside gameplay so they can be exercised separately.
    /// </summary>
    public static class NecroSaveStore
    {
        public const int Version=1;
        public const string FileName="necro-save-v1.json";
        public static string SavePath {
            get { return Path.Combine(Application.persistentDataPath,FileName); }
        }
        public static string BackupPath { get { return SavePath+".bak"; } }

        public static bool TryLoad(out NecroSaveData data)
        {
            if(ReadFile(SavePath,out data))return true;
            if(ReadFile(BackupPath,out data))
            {
                Debug.LogWarning("Necromancer: primary save unavailable; using backup.");
                return true;
            }
            data=null;
            return false;
        }

        private static bool ReadFile(string path,out NecroSaveData data)
        {
            data=null;
            if(!File.Exists(path))return false;
            try
            {
                string json=File.ReadAllText(path);
                if(json.Length<20 || json.Length>4_000_000)return false;
                data=JsonUtility.FromJson<NecroSaveData>(json);
                if(data==null || data.version!=Version || data.level<1 ||
                    data.stages==null || data.stages.Length==0 ||
                    data.army==null || data.vault==null)
                {
                    data=null;return false;
                }
                return true;
            }
            catch(Exception e)
            {
                Debug.LogWarning("Necromancer: save read failed: "+e.Message);
                data=null;return false;
            }
        }

        public static bool Write(NecroSaveData data)
        {
            if(data==null)return false;
            try
            {
                string path=SavePath;
                string parent=Path.GetDirectoryName(path);
                if(!string.IsNullOrEmpty(parent))Directory.CreateDirectory(parent);
                string temp=path+".tmp";
                string json=JsonUtility.ToJson(data,false);
                File.WriteAllText(temp,json);
                if(File.Exists(path))File.Copy(path,BackupPath,true);
                File.Copy(temp,path,true);
                File.Delete(temp);
                return true;
            }
            catch(Exception e)
            {
                Debug.LogError("Necromancer: save write failed: "+e.Message);
                return false;
            }
        }
    }
}
