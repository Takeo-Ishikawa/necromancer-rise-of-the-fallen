using UnityEngine;

namespace Necromancer
{
    /// <summary>
    /// 0.9.6: stable marching ranks behind the heroine.
    /// Works in caves and the refuge, avoids occupied or blocked positions,
    /// and recovers only extreme, safe stragglers after several seconds.
    /// </summary>
    public partial class NecroPrototype
    {
        const float FormationFollowStart=.82f;
        const float FormationFollowStop=.28f;
        const float FormationRecoveryDistance=12f;
        const float FormationRecoveryDelay=5.5f;

        // Deterministic slots by companion index: 4 in the first rank, 6
        // in the second, 8 in the next, then 10. Turn with the heroine.
        Vector2 PartyPosition(int slot,float bodyRadius=.30f)
        {
            int index=Mathf.Max(0,slot);
            int row=0;
            int capacity=4;
            while(index>=capacity && row<16)
            {
                index-=capacity;
                row++;
                capacity=4+2*row;
            }

            Vector2 forward=facing.sqrMagnitude>.01f?
                facing.normalized:Vector2.right;
            Vector2 lateral=new Vector2(-forward.y,forward.x);
            float sizeAdjust=Mathf.Max(0f,bodyRadius-.30f);
            // First defensive rank flanks slightly ahead of the heroine;
            // the sanctuary and hunting ranks keep their old marching slots.
            float backward=(!inSanctuary && !huntOrder && row==0)
                ? -.28f+sizeAdjust*.7f
                : 1.28f+row*1.38f+sizeAdjust*.7f;
            int column=index/2;
            float side=(column+.5f)*(1.16f+sizeAdjust*.7f);
            if((index&1)==0)side=-side;
            return (Vector2)player.body.position-forward*backward+
                lateral*side;
        }

        bool FormationSpotOpen(Actor ally,Vector2 target)
        {
            if(ally==null || ally.body==null || player==null ||
                player.body==null || Blocked(target,ally.radius))return false;
            // With equal start and end this checks occupancy without moving.
            return ActorAt(ally,target,target)==null;
        }

        bool TryFindFormationSpot(Actor ally,int slot,out Vector2 target)
        {
            target=PartyPosition(slot,ally.radius);
            if(FormationSpotOpen(ally,target))return true;

            // Golden-angle offsets produce consistent alternatives at
            // doorways, without per-frame collections or allocations.
            Vector2 preferred=target;
            for(int i=0;i<12;i++)
            {
                float angle=(i+slot%3)*2.39996323f;
                float distance=.65f+.38f*(i/4);
                Vector2 candidate=preferred+
                    new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*distance;
                if(FormationSpotOpen(ally,candidate))
                {
                    target=candidate;
                    return true;
                }
            }
            // A narrow tunnel may have no free lateral position at all.
            // Re-form into an orderly single-file trail behind the heroine.
            Vector2 forward=facing.sqrMagnitude>.01f?
                facing.normalized:Vector2.right;
            float trailStep=Mathf.Max(.70f,ally.radius*1.85f);
            for(int depth=0;depth<12;depth++)
            {
                float back=1.05f+(slot+depth)*trailStep;
                Vector2 candidate=(Vector2)player.body.position-
                    forward*back;
                if(FormationSpotOpen(ally,candidate))
                {
                    target=candidate;
                    return true;
                }
            }
            target=preferred;
            return false;
        }

        bool TryCaveRallySpot(Actor ally,int slot,out Vector2 target)
        {
            // The heroine's exact position is never a valid follower target.
            // Deterministic rings avoid conflicting with the player's body,
            // current creatures or the solid cave mask.
            Vector2 origin=player.body.position;
            Vector2 heading=facing.sqrMagnitude>.01f?
                facing.normalized:Vector2.right;
            for(int ring=0;ring<3;ring++)
            {
                float distance=Mathf.Max(player.radius+ally.radius+.35f,
                    1f+ring*.55f);
                for(int i=0;i<12;i++)
                {
                    float angle=(i+slot*3)%12*Mathf.PI/6f;
                    Vector2 direction=Quaternion.Euler(0,0,angle*Mathf.Rad2Deg)*
                        (-heading);
                    Vector2 candidate=origin+direction*distance;
                    if(FormationSpotOpen(ally,candidate))
                    {
                        target=candidate;
                        return true;
                    }
                }
            }
            target=ally.body.position;
            return false;
        }

        bool FormationRecoverySafe(Actor ally)
        {
            if(inSanctuary)return true;
            if(current==null || player==null)return false;
            Vector2 at=ally.body.position;
            Vector2 leader=player.body.position;
            foreach(Actor enemy in current.enemies)
            {
                if(enemy==null || enemy.hp<=0f || enemy.body==null)continue;
                Vector2 hostile=enemy.body.position;
                if((hostile-at).sqrMagnitude<25f ||
                   (hostile-leader).sqrMagnitude<25f)return false;
            }
            return true;
        }

        void FollowNecromancer(Actor ally,float dt)
        {
            if(ally==null || ally.body==null || player==null ||
                player.body==null)return;
            int slot=servants.IndexOf(ally);
            if(slot<0)return;
            Vector2 target;
            bool free=TryFindFormationSpot(ally,slot,out target);
            bool cave=!inSanctuary && NecroCaveNetwork.HasNetwork(level);
            RecordCaveHeroTrail();
            if(cave && (NarrowCaveAroundHero() || !free) &&
                TryCaveTrailSpot(ally,slot,out Vector2 trailSpot))
            {
                // A train of non-overlapping positions follows the route the
                // heroine actually used, especially around blind corners.
                target=trailSpot;
                free=true;
            }
            // In a crowded dead end, a goal on the player causes a collision
            // loop. Use a valid spot near the leader instead.
            if(cave && !free)
                free=TryCaveRallySpot(ally,slot,out target);
            if(cave && !free)
                target=ally.body.position;
            bool behindRock=cave && !NecroCaveNavigator.Direct(level,
                ally.body.position,target,ally.radius);
            float leaderDistance=Vector2.Distance(ally.body.position,
                player.body.position);

            if(leaderDistance>FormationRecoveryDistance)
                ally.followStragglerSeconds+=dt;
            else
                ally.followStragglerSeconds=0f;
            // No recall near hostiles, through occupied slots, or on
            // short separation: normal movement handles those cases.
            // Never teleport through a winding cave wall. On Bonehollow,
            // real routes handle long separations; recall stays for the refuge
            // and legacy open maps only.
            if(free && (!cave) &&
                ally.followStragglerSeconds>=FormationRecoveryDelay &&
                FormationRecoverySafe(ally))
            {
                ally.body.position=target;
                ally.followStragglerSeconds=0f;
                ally.catchingUp=false;
                return;
            }

            float distance=Vector2.Distance(ally.body.position,target);
            if(cave && !free)
            {
                ally.catchingUp=false;
                ally.caveRoute=null;
                return;
            }
            if(distance>FormationFollowStart || behindRock)
                ally.catchingUp=true;
            if(distance<FormationFollowStop && !behindRock)
                ally.catchingUp=false;
            if(!ally.catchingUp)return;

            float catchup=leaderDistance>7f?2f:
                leaderDistance>4f?1.55f:1f;
            if(cave && behindRock)
            {
                NecroCaveRoute route=ally.caveRoute;
                bool destinationMoved=route!=null &&
                    (route.destination-target).sqrMagnitude>1.44f;
                bool exhausted=route!=null &&
                    route.cursor>=route.waypoints.Length-1 &&
                    ((Vector2)ally.body.position-route.waypoints[
                        route.waypoints.Length-1]).sqrMagnitude<.36f;
                bool needsRoute=route==null || destinationMoved || exhausted;

                // Keep the path while its end point stays relevant. This
                // avoids rerunning breadth-first search every second while
                // the leader stands still on the other side of a wall.
                if(needsRoute && Time.time>=ally.caveRepathAt)
                {
                    route=NecroCaveNavigator.Build(level,
                        ally.body.position,target,ally.radius);
                    ally.caveRoute=route;
                    ally.caveRepathAt=Time.time+1.15f+(slot%4)*.13f;
                }
                if(route!=null)
                {
                    Vector2 waypoint=NecroCaveNavigator.Next(level,route,
                        ally.body.position,ally.radius);
                    if(NecroCaveNavigator.Direct(level,ally.body.position,
                        waypoint,ally.radius))
                    {
                        MoveToward(ally,waypoint,dt,catchup);
                        return;
                    }
                    // A cached route is invalid after forced movement or a
                    // changed start cell: request a safe rebuild next tick.
                    ally.caveRoute=null;
                    ally.caveRepathAt=Time.time;
                }
            }
            else ally.caveRoute=null;
            MoveToward(ally,target,dt,catchup);
        }
    }
}
