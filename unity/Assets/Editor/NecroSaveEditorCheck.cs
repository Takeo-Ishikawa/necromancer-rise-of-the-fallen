using System;
using UnityEditor.Build;
using UnityEngine;

namespace Necromancer.EditorTools
{
    /// <summary>Small editor-time serialization regression check, run as part of APK build.</summary>
    public static class NecroSaveEditorCheck
    {
        public static void Verify()
        {
            NecroSaveData snapshot=new NecroSaveData {
                version=NecroSaveStore.Version,
                stage=2,level=9,experience=17,shards=41,maxMana=94,
                hp=73,maxHp=138,
                rankKinds=new[]{"rat","wisp"},rankValues=new[]{2,1},
                army=new[]{new NecroActorRecord {
                    kind="wisp",hp=27,maxHp=62,attack=15,x=10,y=12
                }},
                vault=new[]{new NecroActorRecord {
                    kind="grave_lord",hp=171,maxHp=290,attack=34,x=20,y=5
                }},
                stages=new[]{new NecroStageRecord {
                    visited=true,claimed=true,bossBound=true,
                    resourcesInitialized=true,
                    enemies=new[]{new NecroActorRecord{kind="skeleton",hp=20,maxHp=75,x=14,y=8}},
                    resources=new[]{new NecroResourceRecord{type="ossos",x=12,y=7}},
                    drops=new[]{new NecroItemRecord{slot=1,tier=2,x=9,y=10}}
                }}
            };
            string json=JsonUtility.ToJson(snapshot);
            NecroSaveData again=JsonUtility.FromJson<NecroSaveData>(json);
            if(again==null || again.version!=NecroSaveStore.Version ||
                again.level!=9 || again.stage!=2 || again.shards!=41 ||
                again.army==null || again.army.Length!=1 || again.army[0].kind!="wisp" ||
                again.vault==null || again.vault.Length!=1 || again.vault[0].hp!=171 ||
                again.rankKinds==null || again.rankKinds.Length!=2 ||
                again.rankValues==null || again.rankValues[0]!=2 ||
                again.stages==null || again.stages.Length!=1 ||
                !again.stages[0].claimed || !again.stages[0].bossBound ||
                again.stages[0].resources.Length!=1 ||
                !again.stages[0].resourcesInitialized ||
                again.stages[0].drops[0].tier!=2 ||
                again.stages[0].enemies[0].kind!="skeleton")
                throw new BuildFailedException("Necromancer save JSON serialization regression.");
            // A pre-0.9.15 v1 stage lacks this optional flag and must be
            // distinguishable from a new stage with zero remaining pickups.
            NecroStageRecord legacy=JsonUtility.FromJson<NecroStageRecord>(
                "{\"visited\":true,\"resources\":[]}");
            if(legacy==null || !legacy.visited || legacy.resourcesInitialized)
                throw new BuildFailedException(
                    "Legacy resource migration marker regression.");
            Debug.Log("[Necromancer] save DTO JSON serialization round-trip: OK");
        }
    }
}
