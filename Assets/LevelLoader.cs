﻿using System;
using Newtonsoft.Json;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

class LevelLoader
{
    public class LevelInfo
    {
        public class LevelMeta
        {
            [JsonProperty("width")]
            public int Width;

            [JsonProperty("height")]
            public int Height;

            [JsonProperty("next")]
            public string Next;
        }

        public class TilePrefInfo
        {
            [JsonProperty("material")]
            public string Mat;

            [JsonProperty("canwalk")] 
            public bool CanWalk;
        }

        public class AgentPrefInfo
        {
            [JsonProperty("prefab")]
            public string Prefab;
        }

        public class TileInstanceInfo
        {
            [JsonProperty("coord")]
            public int[] Coord;

            [JsonProperty("tileinfo")]
            public int InfoNo;

            [JsonProperty("elevate")]
            public float Elevation;
        }

        public class AgentInstanceInfo
        {
            [JsonProperty("prot")]
            public int PrefId;

            [JsonProperty("coord")]
            public int[] Coords;

            [JsonProperty("target")]
            public string Target;

            [JsonProperty("id")]
            public string Id;
        }

        [JsonProperty("meta")]
        public LevelMeta Meta;

        [JsonProperty("tileinfo")]
        public TilePrefInfo[] TileInfos;

        [JsonProperty("agentinfo")]
        public AgentPrefInfo[] AgentInfos;

        [JsonProperty("tiles")]
        public TileInstanceInfo[] Tiles;

        [JsonProperty("agents")]
        public AgentInstanceInfo[] Agents;
    }

    public static Level LoadLevel(string level)
    {
        string filename = string.Format("Levels/{0}", level);

        string contents = Resources.Load<TextAsset>(filename).text;
        var info = JsonConvert.DeserializeObject<LevelInfo>(contents);

        TileInfo[] tinfos = new TileInfo[info.TileInfos.Length];
        AgentInfo[] agents = new AgentInfo[info.AgentInfos.Length];

        Dictionary<string, int> agentmap = new Dictionary<string, int>();

        for (int i = 0; i < info.TileInfos.Length; i++)
        {
            tinfos[i] = new TileInfo(Resources.Load<Material>(string.Format("Tiles/{0}", info.TileInfos[i].Mat)), info.TileInfos[i].CanWalk);
        }

        for (int i = 0; i < info.AgentInfos.Length; i++)
        {
            agents[i] = new AgentInfo
            {
                Prefab = Resources.Load<GameObject>(string.Format("Agents/{0}", info.AgentInfos[i].Prefab))
            };

            agentmap.Add(info.AgentInfos[i].Prefab, i);
        }

        Grid g = new Grid(info.Meta.Width, info.Meta.Height);

        foreach (var t in info.Tiles)
        {
            g.Set(new Tile(tinfos[t.InfoNo], new Coord(t.Coord[0], t.Coord[1]), t.Elevation));
        }

        Func<Agent>[] agentCreators = new Func<Agent>[info.Agents.Length];
        for (int i = 0; i < info.Agents.Length; i++)
        {
            LevelInfo.AgentInstanceInfo inf = info.Agents[i];
            agentCreators[i] = delegate
            {
                var go = UnityEngine.Object.Instantiate(agents[inf.PrefId].Prefab);
                go.GetComponent<Agent>().Position = new Coord(inf.Coords[0], inf.Coords[1]);
                go.GetComponent<Agent>().Target = inf.Target;
                go.GetComponent<Agent>().Id = inf.Id;
                return go.GetComponent<Agent>();
            };
        }

        return new Level(g, agents, tinfos, agentCreators, agentmap) { NextLevel = info.Meta.Next };
    }
}