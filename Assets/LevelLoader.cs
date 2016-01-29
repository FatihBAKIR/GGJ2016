using Newtonsoft.Json;
using UnityEngine;

class LevelLoader
{

    public class LevelInfo
    {
        class LevelMeta
        {
            [JsonProperty("width")]
            public int Width;

            [JsonProperty("height")]
            public int Height;
        }

        class TileInfo
        {
            [JsonProperty("material")]
            private string _mat;
        }

        class SingleTileInfo
        {
            [JsonProperty("coord.x")]
            private int x;

            [JsonProperty("coord.y")]
            private int y;

            [JsonProperty("tileinfo")]
            private int infoNo;

            [JsonProperty("elevate")]
            private float elevation;
        }

        [JsonProperty("meta")]
        private LevelMeta Meta;

        [JsonProperty("tileinfo")]
        private TileInfo[] infos;

        [JsonProperty("tiles")]
        private Tile[] tiles;
    }

    public static Level LoadLevel(string level)
    {
        string file = Resources.Load<TextAsset>(level).text;
        var info = JsonConvert.DeserializeObject<LevelInfo>(file);

        return null;
    }
}