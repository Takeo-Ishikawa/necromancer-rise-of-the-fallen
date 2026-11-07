using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Necromancer
{
    /// <summary>
    /// The Cripta is available at the conquered cave sanctuary:
    /// store and retrieve existing servants, perform permanent rituals and save progress.
    /// </summary>
    public partial class NecroPrototype
    {
        readonly List<NecroActorRecord> vault=new List<NecroActorRecord>();
        bool cryptOpen,restoring;
        int vaultPage,lifeRitual,vaultRitual,spiritRitual;
        float autoSaveClock;
        // Save snapshots of regions not currently loaded in this refuge-first
        // preview. Attribute/wardrobe purchases must not erase old cave data.
        NecroStageRecord[] previousStageSnapshots;
        int previousCheckpointStage;

        int VaultCapacity() {return 8+vaultRitual*4;}

        void ShowCryptAltar()
        {
            if(stage!=0 || current==null || !current.claimed || current.cryptAltar!=null)return;
            Vector2 at=NecroLevels.At(level,level.sealX,level.sealY);
            SpriteRenderer altar=Visual("Cripta - Altar de Almas",
                at+new Vector2(1.25f,.25f),NecroArt.Get("crypt_altar"),-2,current.root.transform);
            Fit(altar,1.6f,1.6f);
            current.cryptAltar=altar.transform;
        }

        bool CanAccessCrypt()
        {
            return !inSanctuary && stage==0 && stages!=null && stages.Length>0 &&
                stages[0]!=null && stages[0].claimed && NearSeal();
        }

        bool OpenCrypt()
        {
            if(gameOver || !CanAccessCrypt())return false;
            cryptOpen=true;upgradeOpen=false;
            touchMove=Vector2.zero;
            Announce("Cripta: organize suas criaturas e realize rituais.");
            return true;
        }
        void CloseCrypt()
        {
            cryptOpen=false;
            SaveProgress();
        }

        bool StoreServant()
        {
            if(!cryptOpen || !CanAccessCrypt())return false;
            if(vault.Count>=VaultCapacity())
            {
                Announce("Cripta cheia. Aprimore seus nichos.");return false;
            }
            Actor selected=null;float nearest=4.2f;
            foreach(Actor servant in servants)
            {
                float distance=Vector2.Distance(servant.body.position,player.body.position);
                if(distance<nearest){nearest=distance;selected=servant;}
            }
            if(selected==null){Announce("Nenhum servo próximo para guardar.");return false;}
            NecroActorRecord record=RecordActor(selected);
            vault.Add(record);
            servants.Remove(selected);
            Destroy(selected.body.gameObject);
            Announce("Servo "+selected.kind+" guardado na Cripta.");
            SaveProgress();
            return true;
        }

        bool WithdrawServant(int index)
        {
            if(!cryptOpen || !CanAccessCrypt() || index<0 || index>=vault.Count)return false;
            NecroActorRecord record=vault[index];
            if(record==null || string.IsNullOrEmpty(record.kind))return false;
            if(CommandUsed()+CommandCost(record.kind)>CommandLimit)
            {
                Announce("Comando insuficiente. Guarde outro servo.");return false;
            }
            Actor actor=Create(record.kind,(Vector2)player.body.position+new Vector2(.8f,0),
                true,persistentRoot.transform);
            RestoreActorStats(actor,record);
            actor.hp=Mathf.Max(1,actor.hp);
            servants.Add(actor);
            vault.RemoveAt(index);
            vaultPage=Mathf.Max(0,Mathf.Min(vaultPage,Mathf.Max(0,vault.Count-1)));
            Announce("Servo "+actor.kind+" retirado da Cripta.");
            SaveProgress();
            return true;
        }

        int VitalityCost() {return 6+lifeRitual*4;}
        int VaultCost() {return 8+vaultRitual*5;}
        int SpiritCost() {return 6+spiritRitual*3;}

        bool RitualVitality()
        {
            if(!cryptOpen || lifeRitual>=5 || shards<VitalityCost())return false;
            shards-=VitalityCost();lifeRitual++;
            player.maxHp+=15;player.hp=Mathf.Min(player.maxHp,player.hp+15);
            maxMana+=5;mana=Mathf.Min(maxMana,mana+5);
            Announce("Ritual vital: +15 vida, +5 ânima permanente.");
            SaveProgress();return true;
        }
        bool RitualVault()
        {
            if(!cryptOpen || vaultRitual>=2 || shards<VaultCost())return false;
            shards-=VaultCost();vaultRitual++;
            Announce("Cripta expandida para "+VaultCapacity()+" nichos.");
            SaveProgress();return true;
        }
        bool RitualSpirit()
        {
            if(!cryptOpen || spiritRitual>=3 || shards<SpiritCost())return false;
            shards-=SpiritCost();spiritRitual++;
            Announce("Ritual de fluxo: regen. de ânima aprimorada.");
            SaveProgress();return true;
        }

        NecroActorRecord RecordActor(Actor actor)
        {
            Vector3 at=actor.body.position;
            return new NecroActorRecord{
                kind=actor.kind,x=at.x,y=at.y,
                hp=actor.hp,maxHp=actor.maxHp,attack=actor.attack,cooldown=actor.cooldown
            };
        }
        void RestoreActorStats(Actor actor,NecroActorRecord record)
        {
            if(record.maxHp>0 && record.maxHp<100000)actor.maxHp=record.maxHp;
            if(record.attack>0 && record.attack<10000)actor.attack=record.attack;
            actor.hp=Mathf.Clamp(record.hp,1,actor.maxHp);
            actor.cooldown=Mathf.Clamp(record.cooldown,0,10);
        }

        NecroStageRecord RecordStage(StageInstance instance)
        {
            NecroStageRecord state=new NecroStageRecord();
            if(instance==null)return state;
            state.visited=true;
            state.claimed=instance.claimed;
            state.bossDefeated=instance.bossDefeated;
            state.bossBound=instance.bossBound;
            List<NecroActorRecord> monsters=new List<NecroActorRecord>();
            foreach(Actor enemy in instance.enemies)
                if(enemy!=null && enemy.hp>0 && enemy.body!=null)
                    monsters.Add(RecordActor(enemy));
            state.enemies=monsters.ToArray();
            List<NecroCorpseRecord> corpses=new List<NecroCorpseRecord>();
            foreach(Corpse body in instance.corpses)
                if(body!=null && body.body!=null && body.time>0)
                {
                    Vector3 at=body.body.position;
                    corpses.Add(new NecroCorpseRecord{
                        kind=body.kind,x=at.x,y=at.y,time=body.time
                    });
                }
            state.corpses=corpses.ToArray();
            List<NecroItemRecord> items=new List<NecroItemRecord>();
            foreach(Drop item in instance.drops)
                if(item!=null && item.body!=null)
                {
                    Vector3 at=item.body.position;
                    items.Add(new NecroItemRecord{
                        x=at.x,y=at.y,slot=item.slot,tier=item.tier
                    });
                }
            state.drops=items.ToArray();
            List<NecroResourceRecord> resources=new List<NecroResourceRecord>();
            foreach(ResourceNode node in instance.resources)
                if(node!=null && node.body!=null)
                {
                    Vector3 at=node.body.position;
                    resources.Add(new NecroResourceRecord{type=node.type,x=at.x,y=at.y});
                }
            state.resources=resources.ToArray();
            return state;
        }

        // The preview always begins at the refuge. The old snapshot is used
        // to restore permanent hero progression without jumping to the cave;
        // companion placement still follows PlayerPrefs as before.
        bool RestoreSanctuaryProfile()
        {
            NecroSaveData saved;
            if(!NecroSaveStore.TryLoad(out saved))return false;
            previousStageSnapshots=saved.stages;
            previousCheckpointStage=Mathf.Clamp(saved.stage,0,levels.Length-1);
            playerLevel=Mathf.Clamp(saved.level,1,100);
            experience=Mathf.Clamp(saved.experience,0,100000);
            maxMana=Mathf.Clamp(saved.maxMana,55,3000);
            mana=Mathf.Clamp(saved.mana,0,maxMana);
            shards=Mathf.Clamp(saved.shards,0,100000);
            kills=Mathf.Clamp(saved.kills,0,100000);
            gathered=Mathf.Clamp(saved.gathered,0,100000);
            staffTier=Mathf.Clamp(saved.staffTier,0,3);
            mantleTier=Mathf.Clamp(saved.mantleTier,0,3);
            lifeRitual=Mathf.Clamp(saved.lifeRitual,0,5);
            vaultRitual=Mathf.Clamp(saved.vaultRitual,0,2);
            spiritRitual=Mathf.Clamp(saved.spiritRitual,0,3);
            huntOrder=saved.huntOrder;
            completed=saved.completed;
            spellUnlocked=spellUnlocked || saved.spellUnlocked;
            vault.Clear();
            if(saved.vault!=null)foreach(NecroActorRecord item in saved.vault)
                if(item!=null && !string.IsNullOrEmpty(item.kind) &&
                   vault.Count<VaultCapacity())vault.Add(item);
            player.maxHp=Mathf.Clamp(saved.maxHp,115f,10000f);
            player.hp=Mathf.Clamp(saved.hp,1f,player.maxHp);
            RestoreMeditationAllocations(saved);
            speciesRanks.Clear();
            if(saved.rankKinds!=null && saved.rankValues!=null)
            {
                for(int i=0;i<Mathf.Min(saved.rankKinds.Length,saved.rankValues.Length);i++)
                    if(!string.IsNullOrEmpty(saved.rankKinds[i]))
                        speciesRanks[saved.rankKinds[i]]=Mathf.Clamp(saved.rankValues[i],0,3);
            }
            Announce("Progresso da necromante restaurado.");
            return true;
        }

        bool SaveProgress()
        {
            if(restoring || player==null || current==null || gameOver)return false;
            // Permanent changes can be made inside the refuge. Preserve the
            // last cave checkpoint and snapshots of as-yet-unloaded regions.
            if(inSanctuary)SaveCompanions();
            int checkpointStage=inSanctuary ? previousCheckpointStage : stage;
            checkpointStage=Mathf.Clamp(checkpointStage,0,levels.Length-1);
            Vector2 checkpoint = inSanctuary
                ? NecroLevels.At(levels[checkpointStage],
                    levels[checkpointStage].startX,levels[checkpointStage].startY)
                : (Vector2)player.body.position;
            NecroSaveData save=new NecroSaveData {
                version=NecroSaveStore.Version,
                stage=checkpointStage,level=playerLevel,experience=experience,
                mana=mana,maxMana=maxMana,shards=shards,kills=kills,
                gathered=gathered,staffTier=staffTier,mantleTier=mantleTier,
                vitalityPoints=vitalityPoints,powerPoints=powerPoints,
                agilityPoints=agilityPoints,
                lifeRitual=lifeRitual,vaultRitual=vaultRitual,spiritRitual=spiritRitual,
                spellUnlocked=spellUnlocked,huntOrder=huntOrder,completed=completed,
                hp=player.hp,maxHp=player.maxHp,
                playerX=checkpoint.x,playerY=checkpoint.y
            };
            List<string> kinds=new List<string>(speciesRanks.Keys);
            kinds.Sort(StringComparer.Ordinal);
            save.rankKinds=kinds.ToArray();
            save.rankValues=new int[kinds.Count];
            for(int i=0;i<kinds.Count;i++)save.rankValues[i]=speciesRanks[kinds[i]];
            List<NecroActorRecord> active=new List<NecroActorRecord>();
            foreach(Actor ally in servants)if(ally!=null && ally.hp>0 && ally.body!=null)
                active.Add(RecordActor(ally));
            save.army=active.ToArray();
            save.vault=vault.ToArray();
            save.stages=new NecroStageRecord[stages.Length];
            for(int i=0;i<stages.Length;i++)
                save.stages[i]=stages[i]!=null ? RecordStage(stages[i]) :
                    (previousStageSnapshots!=null && i<previousStageSnapshots.Length &&
                     previousStageSnapshots[i]!=null ?
                     previousStageSnapshots[i] : RecordStage(null));
            bool written=NecroSaveStore.Write(save);
            if(written)
            {
                previousStageSnapshots=save.stages;
                previousCheckpointStage=save.stage;
            }
            return written;
        }

        private bool RestoreProgress()
        {
            if(!NecroSaveStore.TryLoad(out NecroSaveData save))return false;
            if(save.stage<0 || save.stage>=levels.Length || save.stages.Length==0)return false;
            previousStageSnapshots=save.stages;
            previousCheckpointStage=save.stage;
            restoring=true;
            try
            {
                playerLevel=Mathf.Clamp(save.level,1,100);
                experience=Mathf.Clamp(save.experience,0,100000);
                maxMana=Mathf.Clamp(save.maxMana,55,3000);
                mana=Mathf.Clamp(save.mana,0,maxMana);
                shards=Mathf.Clamp(save.shards,0,100000);
                kills=Mathf.Clamp(save.kills,0,100000);
                gathered=Mathf.Clamp(save.gathered,0,100000);
                staffTier=Mathf.Clamp(save.staffTier,0,3);
                mantleTier=Mathf.Clamp(save.mantleTier,0,3);
                RestoreMeditationAllocations(save);
                lifeRitual=Mathf.Clamp(save.lifeRitual,0,5);
                vaultRitual=Mathf.Clamp(save.vaultRitual,0,2);
                spiritRitual=Mathf.Clamp(save.spiritRitual,0,3);
                spellUnlocked=save.spellUnlocked;
                huntOrder=save.huntOrder;
                completed=save.completed;
                player.maxHp=Mathf.Clamp(save.maxHp,115,10000);
                player.hp=Mathf.Clamp(save.hp,1,player.maxHp);
                speciesRanks.Clear();
                if(save.rankKinds!=null && save.rankValues!=null)
                {
                    for(int i=0;i<Mathf.Min(save.rankKinds.Length,save.rankValues.Length);i++)
                        if(!string.IsNullOrEmpty(save.rankKinds[i]))
                            speciesRanks[save.rankKinds[i]]=Mathf.Clamp(save.rankValues[i],0,3);
                }
                vault.Clear();
                foreach(NecroActorRecord item in save.vault)
                    if(item!=null && !string.IsNullOrEmpty(item.kind) &&
                        vault.Count<VaultCapacity())vault.Add(item);

                // Create only regions that were already visited. Unvisited areas
                // remain fresh until the player discovers them.
                for(int i=0;i<Mathf.Min(save.stages.Length,levels.Length);i++)
                {
                    NecroStageRecord savedStage=save.stages[i];
                    if(savedStage==null || !savedStage.visited)continue;
                    LoadStage(i);
                    RestoreStage(savedStage);
                }
                LoadStage(save.stage);
                Vector2 target=new Vector2(save.playerX,save.playerY);
                if(!Blocked(target,player.radius))
                    player.body.position=target;
                foreach(Actor servant in servants)Destroy(servant.body.gameObject);
                servants.Clear();
                int budget=0;
                foreach(NecroActorRecord item in save.army)
                {
                    if(item==null || string.IsNullOrEmpty(item.kind))continue;
                    int cost=CommandCost(item.kind);
                    if(budget+cost>CommandLimit)continue;
                    Actor restored=Create(item.kind,player.body.position+
                        new Vector3(.6f+.12f*servants.Count,0,0),
                        true,persistentRoot.transform);
                    RestoreActorStats(restored,item);
                    servants.Add(restored);
                    budget+=cost;
                }
                gameOver=false;upgradeOpen=false;cryptOpen=false;
                Announce("Progresso restaurado. Bem-vindo de volta.");
                return true;
            }
            catch(Exception exception)
            {
                Debug.LogError("Necromancer: could not restore save: "+exception);
                return false;
            }
            finally {restoring=false;}
        }

        private void RestoreStage(NecroStageRecord saved)
        {
            foreach(Actor actor in current.enemies)if(actor.body!=null)Destroy(actor.body.gameObject);
            current.enemies.Clear();
            if(saved.enemies!=null)foreach(NecroActorRecord record in saved.enemies)
            {
                if(record==null || string.IsNullOrEmpty(record.kind))continue;
                Actor enemy=Create(record.kind,new Vector2(record.x,record.y),
                    false,current.root.transform);
                RestoreActorStats(enemy,record);
                current.enemies.Add(enemy);
            }
            if(saved.corpses!=null)foreach(NecroCorpseRecord body in saved.corpses)
            {
                if(body==null || string.IsNullOrEmpty(body.kind) || body.time<=0)continue;
                SpriteRenderer marker=Visual("Cadaver: "+body.kind,
                    new Vector2(body.x,body.y),NecroArt.Effect(),9,current.root.transform);
                Fit(marker,.48f,.48f);
                marker.color=new Color(.82f,.72f,.9f,.75f);
                current.corpses.Add(new Corpse{
                    kind=body.kind,body=marker.transform,time=body.time
                });
            }
            if(saved.drops!=null)foreach(NecroItemRecord item in saved.drops)
            {
                if(item!=null)SpawnDrop(new Vector2(item.x,item.y),
                    Mathf.Clamp(item.slot,0,1),Mathf.Clamp(item.tier,1,3));
            }
            foreach(ResourceNode node in current.resources)
                if(node.body!=null)Destroy(node.body.gameObject);
            current.resources.Clear();
            if(saved.resources!=null)foreach(NecroResourceRecord node in saved.resources)
            {
                if(node!=null && !string.IsNullOrEmpty(node.type))
                    SpawnResource(node.type,new Vector2(node.x,node.y));
            }
            current.claimed=saved.claimed;
            current.bossDefeated=saved.bossDefeated;
            current.bossBound=saved.bossBound;
            if(saved.claimed)
            {
                if(!string.IsNullOrEmpty(level.baseBackground))
                {
                    current.backdrop.sprite=NecroArt.Environment(level.baseBackground);
                    Fit(current.backdrop,level.width,level.height);
                }
                current.seal.localScale=Vector3.one*3.2f;
                if(stage==0)ShowCryptAltar();
            }
        }

        private void Autosave(float dt)
        {
            if(restoring || gameOver || upgradeOpen || cryptOpen)return;
            autoSaveClock+=dt;
            if(autoSaveClock<15f)return;
            autoSaveClock=0;
            SaveProgress();
        }

        void RecoverAfterDeath()
        {
            gameOver=false;
            shards=Mathf.Max(0,shards-2);
            if(stages.Length>0 && stages[0]!=null && stages[0].claimed)
                LoadStage(0);
            else LoadStage(stage);
            player.hp=Mathf.Max(1,player.maxHp*.65f);
            mana=maxMana;
            Announce("Voce retornou ao refugio. Perdeu ate 2 fragmentos.");
            SaveProgress();
        }
    }
}
