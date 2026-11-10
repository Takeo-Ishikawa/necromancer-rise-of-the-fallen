using UnityEngine;

namespace Necromancer
{
    public static class NecroDownedRules
    {
        public const float OrdinaryReach=1.30f;
        public const float PupReach=1.35f;
        public const float RotboarReach=1.80f;
        public const float DownedDuration=14f;

        public static bool WithinReach(float squared,float reach)
        {
            return squared<=reach*reach;
        }

        public static int FragmentsFor(string kind)
        {
            if(kind=="warden" || kind=="grave_lord" ||
               kind=="marsh_lord")return 8;
            if(kind=="rotboar")return 5;
            if(kind=="grave_pup" || kind=="skeleton" ||
               kind=="wisp")return 3;
            return 2;
        }

        public static bool CanCapture(int used,int cost,int maximum)
        {
            return used>=0 && cost>0 && used+cost<=maximum;
        }

        public static int CaptureMana(bool mage,bool ordinary,int cost)
        {
            return mage && ordinary?Mathf.Max(0,cost):0;
        }
    }

    public partial class NecroPrototype
    {
        struct DownedChoice
        {
            public Corpse corpse;
            public NecroCreatureView pup;
            public Actor boar;
            public Vector2 at;
            public string kind;
            public bool Valid => corpse!=null || pup!=null || boar!=null;
        }

        DownedChoice FindNearbyDowned()
        {
            DownedChoice result=new DownedChoice();
            if(inSanctuary || gameOver || player==null || current==null ||
               inventoryOpen || wardrobeOpen || meditationOpen || cryptOpen ||
               upgradeOpen || armyInspect || gearInspect)return result;
            Vector2 from=player.body.position;
            float nearest=float.PositiveInfinity;
            foreach(Corpse entry in current.corpses)
            {
                if(entry==null || entry.body==null || entry.time<=0f)continue;
                Vector2 at=entry.body.position;
                float d=(at-from).sqrMagnitude;
                if(d>=nearest || !NecroDownedRules.WithinReach(
                    d,NecroDownedRules.OrdinaryReach) || !Visible(from,at))continue;
                nearest=d;
                result=new DownedChoice{corpse=entry,at=at,kind=entry.kind};
            }
            foreach(NecroCreatureView pup in current.fallenPups)
            {
                if(pup==null || !pup.CanBind)continue;
                Vector2 at=pup.transform.position;
                float d=(at-from).sqrMagnitude;
                if(d>=nearest || !NecroDownedRules.WithinReach(
                    d,NecroDownedRules.PupReach) || !Visible(from,at))continue;
                nearest=d;
                result=new DownedChoice{pup=pup,at=at,kind="grave_pup"};
            }
            foreach(Actor boar in current.fallenBoars)
            {
                if(boar==null || boar.body==null ||
                   boar.bindUntil<=Time.time)continue;
                Vector2 at=boar.body.position;
                float d=(at-from).sqrMagnitude;
                if(d>=nearest || !NecroDownedRules.WithinReach(
                    d,NecroDownedRules.RotboarReach) || !Visible(from,at))continue;
                nearest=d;
                result=new DownedChoice{boar=boar,at=at,kind=boar.kind};
            }
            return result;
        }

        // Removing from the owning list before granting rewards makes
        // extraction and capture exclusive even with simultaneous taps.
        bool ConsumeDowned(DownedChoice choice)
        {
            if(!choice.Valid || current==null)return false;
            if(choice.corpse!=null)
            {
                if(choice.corpse.body==null || choice.corpse.time<=0f ||
                   !current.corpses.Remove(choice.corpse))return false;
                Destroy(choice.corpse.body.gameObject);
                return true;
            }
            if(choice.pup!=null)
            {
                if(!choice.pup.CanBind ||
                   !current.fallenPups.Remove(choice.pup))return false;
                Destroy(choice.pup.gameObject);
                return true;
            }
            if(choice.boar!=null)
            {
                if(choice.boar.body==null ||
                   choice.boar.bindUntil<=Time.time ||
                   !current.fallenBoars.Remove(choice.boar))return false;
                Destroy(choice.boar.body.gameObject);
                return true;
            }
            return false;
        }

        void HarvestDowned(DownedChoice choice)
        {
            if(!choice.Valid || inSanctuary || player==null ||
               player.hp<=0f || gameOver)return;
            int gain=Mathf.Min(NecroDownedRules.FragmentsFor(choice.kind),
                Mathf.Max(0,100000-shards));
            if(gain<=0){Announce("Fragmentos no limite.");return;}
            if(!ConsumeDowned(choice))return;
            shards+=gain;
            gathered++;
            NecroCombatFx.Emit(choice.at,current.root.transform,9);
            Announce("Recursos extraidos: +"+gain+" fragmentos.");
            SaveProgress();
        }

        int DownedCaptureMana(DownedChoice choice)
        {
            return NecroDownedRules.CaptureMana(HeroCanUseMagic,
                choice.corpse!=null,SkillRaiseManaCost(choice.kind));
        }

        void CaptureDowned(DownedChoice choice)
        {
            if(!choice.Valid || inSanctuary || player==null ||
               player.hp<=0f || gameOver)return;
            if(!NecroDownedRules.CanCapture(CommandUsed(),
                    CommandCost(choice.kind),CommandLimit))
            {
                Announce("Comando cheio. Recolha um servo.");
                return;
            }
            int cost=DownedCaptureMana(choice);
            if(mana<cost){Announce("Anima insuficiente.");return;}
            if(!ConsumeDowned(choice))return;
            mana-=cost;
            Actor servant=CreateRestoredActor(choice.kind,choice.at,true,
                persistentRoot.transform);
            servants.Add(servant);
            if(IsBoss(choice.kind))current.bossBound=true;
            if(HeroCanUseMagic)AnimateReanimation(servant,choice.at);
            else
            {
                // Physical characters capture without casting magic.
                servant.reanimationUntil=Time.time+.45f;
                if(servant.view!=null)servant.view.Reanimate();
            }
            Announce("Criatura capturada: "+choice.kind+".");
            SaveCompanions();
            SaveProgress();
        }

        Rect DownedHarvestRect(float scale)
        {
            float width=Screen.width/scale,height=Screen.height/scale;
            Vector3 screen=cam.WorldToScreenPoint(player.body.position);
            float left=Mathf.Clamp(screen.x/scale-110f,10f,
                Mathf.Max(10f,width-220f));
            float top=Mathf.Clamp((Screen.height-screen.y)/scale+68f,
                108f,Mathf.Max(108f,height-76f));
            return new Rect(left,top,104f,51f);
        }

        Rect DownedCaptureRect(float scale)
        {
            Rect first=DownedHarvestRect(scale);
            return new Rect(first.x+112f,first.y,104f,51f);
        }

        void DrawDownedChoices(float scale)
        {
            DownedChoice choice=FindNearbyDowned();
            if(!choice.Valid)return;
            Rect left=DownedHarvestRect(scale),right=DownedCaptureRect(scale);
            float remaining=choice.corpse!=null?choice.corpse.time:
                choice.pup!=null?choice.pup.FallenRemaining:
                Mathf.Max(0f,choice.boar.bindUntil-Time.time);
            GUI.Label(new Rect(left.x-9f,left.y-26f,234f,23f),
                "CRIATURA CAIDA  "+Mathf.CeilToInt(remaining)+"s",hudAction);
            DrawWardrobeSurface(left,
                new Color(.12f,.12f,.11f,.96f),
                new Color(.91f,.79f,.52f,.96f));
            GUI.Label(new Rect(left.x+2f,left.y+3f,100f,21f),
                "EXTRAIR",hudAction);
            GUI.Label(new Rect(left.x+2f,left.y+26f,100f,19f),
                "+"+NecroDownedRules.FragmentsFor(choice.kind)+" FRAG",hudSmall);
            int manaCost=DownedCaptureMana(choice);
            bool canCapture=NecroDownedRules.CanCapture(CommandUsed(),
                CommandCost(choice.kind),CommandLimit) && mana>=manaCost;
            DrawWardrobeSurface(right,
                canCapture?new Color(.09f,.17f,.17f,.97f):
                    new Color(.09f,.09f,.11f,.87f),
                canCapture?new Color(.58f,.91f,.82f,.97f):
                    new Color(.40f,.42f,.43f,.80f));
            GUI.Label(new Rect(right.x+2f,right.y+3f,100f,21f),
                "CAPTURAR",hudAction);
            GUI.Label(new Rect(right.x+2f,right.y+26f,100f,19f),
                !canCapture?"INDISPONIVEL":
                manaCost>0?manaCost+" ANIMA":"NOVO SERVO",hudSmall);
        }
    }
}
