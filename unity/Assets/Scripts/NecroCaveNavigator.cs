using UnityEngine;

namespace Necromancer
{
    // Transient runtime-only follower path, never serialized into saves.
    public sealed class NecroCaveRoute
    {
        public Vector2 destination;
        public Vector2[] waypoints;
        public int cursor;
    }

    /// <summary>
    /// Cached clearance-aware breadth-first cave routing. Only used by
    /// followers when their desired formation slot is behind solid rock.
    /// One 1m navigation grid per body-size class; all search buffers reused.
    /// The original direct-follow movement remains the fast path.
    /// </summary>
    public static class NecroCaveNavigator
    {
        const int RadiusBuckets=3;
        static readonly float[] Clearance={.32f,.49f,.72f};
        static LevelDefinition mapped;
        static int columns,rows,serial;
        static readonly bool[][] walkable=new bool[RadiusBuckets][];
        static int[] queue;
        static int[] seen;
        static int[] parent;

        static bool Ready(LevelDefinition level)
        {
            if(!NecroCaveNetwork.HasNetwork(level))return false;
            if(mapped==level)return true;
            mapped=level;
            columns=Mathf.RoundToInt(level.width);
            rows=Mathf.RoundToInt(level.height);
            int count=columns*rows;
            if(count<=0 || count>120000)return false;
            for(int i=0;i<walkable.Length;i++)walkable[i]=null;
            queue=new int[count];
            seen=new int[count];
            parent=new int[count];
            serial=0;
            return true;
        }

        static int Bucket(float radius)
        {
            return radius<=.28f?0:radius<=.43f?1:2;
        }

        static bool[] FloorMask(LevelDefinition level,int bucket)
        {
            if(walkable[bucket]!=null)return walkable[bucket];
            bool[] floor=new bool[columns*rows];
            float clearance=Clearance[bucket];
            for(int y=0;y<rows;y++)
                for(int x=0;x<columns;x++)
                    floor[y*columns+x]=!NecroCaveNetwork.Blocked(
                        level,new Vector2(x+.5f,y+.5f),clearance);
            walkable[bucket]=floor;
            return floor;
        }

        static Vector2 CellCenter(int node)
        {
            return new Vector2(node%columns+.5f,node/columns+.5f);
        }

        public static bool Direct(LevelDefinition level,Vector2 from,
            Vector2 to,float radius)
        {
            if(!NecroCaveNetwork.HasNetwork(level))return true;
            float distance=Vector2.Distance(from,to);
            int steps=Mathf.Max(1,Mathf.CeilToInt(distance/.30f));
            for(int i=0;i<=steps;i++)
                if(NecroCaveNetwork.Blocked(level,
                    Vector2.Lerp(from,to,(float)i/steps),radius))
                    return false;
            return true;
        }

        static int ClosestNode(LevelDefinition level,Vector2 position,
            float radius,bool[] floor)
        {
            int baseX=Mathf.FloorToInt(position.x);
            int baseY=Mathf.FloorToInt(position.y);
            int selected=-1;
            float best=float.MaxValue;
            // A character or target can stand off-center in a tunnel.
            for(int ring=0;ring<=5;ring++)
            {
                for(int dy=-ring;dy<=ring;dy++)
                    for(int dx=-ring;dx<=ring;dx++)
                    {
                        if(Mathf.Max(Mathf.Abs(dx),Mathf.Abs(dy))!=ring)
                            continue;
                        int x=baseX+dx,y=baseY+dy;
                        if(x<0 || y<0 || x>=columns || y>=rows)continue;
                        int node=y*columns+x;
                        if(!floor[node])continue;
                        Vector2 center=CellCenter(node);
                        float distance=(center-position).sqrMagnitude;
                        if(distance>=best)continue;
                        if(!Direct(level,position,center,radius))continue;
                        best=distance;
                        selected=node;
                    }
                if(selected>=0)return selected;
            }
            return -1;
        }

        public static NecroCaveRoute Build(LevelDefinition level,Vector2 from,
            Vector2 destination,float radius)
        {
            if(!Ready(level))return null;
            int bucket=Bucket(radius);
            bool[] floor=FloorMask(level,bucket);
            int origin=ClosestNode(level,from,radius,floor);
            int goal=ClosestNode(level,destination,radius,floor);
            if(origin<0 || goal<0)return null;

            if(serial==int.MaxValue)
            {
                System.Array.Clear(seen,0,seen.Length);
                serial=0;
            }
            int search=++serial;
            int head=0,tail=0;
            queue[tail++]=origin;
            seen[origin]=search;
            parent[origin]=-1;
            bool reached=origin==goal;
            // Four cardinal moves are intentional: no diagonal corner cuts.
            while(head<tail && !reached)
            {
                int node=queue[head++];
                int x=node%columns,y=node/columns;
                for(int side=0;side<4;side++)
                {
                    int nx=x+(side==0?1:side==1?-1:0);
                    int ny=y+(side==2?1:side==3?-1:0);
                    if(nx<0 || ny<0 || nx>=columns || ny>=rows)continue;
                    int adjacent=ny*columns+nx;
                    if(!floor[adjacent] || seen[adjacent]==search)continue;
                    seen[adjacent]=search;
                    parent[adjacent]=node;
                    queue[tail++]=adjacent;
                    if(adjacent==goal){reached=true;break;}
                }
            }
            if(!reached)return null;
            int length=0;
            for(int node=goal;node!=origin;node=parent[node])
            {
                if(++length>columns*rows)return null;
            }
            // The terminal target is a valid formation position, not a cell
            // center; only append it if the final segment is unobstructed.
            bool appendGoal=Direct(level,CellCenter(goal),destination,radius);
            // Include the starting grid center as a safe waypoint. The
            // character may stand off-center and a diagonal leap to the
            // *second* cell could otherwise cut a rocky corner.
            Vector2[] points=new Vector2[length+1+(appendGoal?1:0)];
            points[0]=CellCenter(origin);
            int at=length+1;
            for(int node=goal;node!=origin;node=parent[node])
                points[--at]=CellCenter(node);
            if(appendGoal)points[length+1]=destination;
            if(points.Length==0)return null;
            return new NecroCaveRoute{
                destination=destination,waypoints=points,cursor=0
            };
        }

        public static Vector2 Next(LevelDefinition level,NecroCaveRoute route,
            Vector2 from,float radius)
        {
            if(route==null || route.waypoints==null ||
                route.waypoints.Length==0)return from;
            int last=route.waypoints.Length-1;
            int index=Mathf.Clamp(route.cursor,0,last);
            while(index<last &&
                (from-route.waypoints[index]).sqrMagnitude<.20f &&
                Direct(level,from,route.waypoints[index+1],radius))
                index++;
            // Short straight shortcuts smooth out the staircase route and
            // also reduce repeated collision sampling on mobile.
            for(int i=Mathf.Min(last,index+4);i>index;i--)
                if(Direct(level,from,route.waypoints[i],radius))
                {index=i;break;}
            route.cursor=index;
            return route.waypoints[index];
        }
    }
}
