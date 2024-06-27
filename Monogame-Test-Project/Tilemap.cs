using ECS;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using viewStuff;



namespace tilemap
{



    /*
    File requirements
    1. must have all layers accounted for with layer names being as listed
    2. must export as csv file
    */

    public class Tilemap
    {
        public int tileDim = 32;
        public int atlasNumTilesPerRow = 2;

        // tilemap base filename (each layer is just an extension of thisname)
        string baseMapFilename;
        const string filepathPrefix = "../../../Content/MapData/";

        // tilemap layers loaded from file
        private Dictionary<LayerType, Dictionary<Vector2, int>> layers;
        //public List<Dictionary<Vector2, int>> layers;

        public string textureAtlasId; // string name of texture stored in resourcemanager
        public string normalAtlasId; // probably going to change this

        private Dictionary<LayerType, string> layerNames = new()
        {
            { LayerType.background, "background" },
            { LayerType.midground, "midground" },
            //{ LayerType.midground_normal, "midground_normal" },
            { LayerType.collision, "collision" },
            { LayerType.foreground, "foreground" }
        };

        // this enum provides a more accesible way to concretely define which indecies within the layer
        // list correspond to which layer in the map, functionally.
        // currently used
        public enum LayerType // to be adjusted, I am not a huge fan of the way this is organized.
        {
            // background is like the floor (walking paths, stone, grass, etc)
            background,
            // midground is things that the player is level with (standing stones, trees, chests, etc)
            midground,
            // represents the normal maps that are ascribed to each texture in the midground layer
            // instead of this, simply use another texture with all the normal map stuff on it (might be expensive)
            //midground_normal, 
            // foreground is something that is between the player and the camera.
            foreground,
            // defines the collidable tiles in the tilemap. Right now I have this so that I can check if
            // a tile is collidable in O(1) time instead of checking against every tile. I think in the future I will have
            // the tilemap system hold onto this data(?) and make it available to the physics system upon load.
            collision,
            // number of layers in the tilemap
            Count
        }

        public Tilemap(string textureAtlasId, string normalAtlasId, string baseMapFilename)
        {
            this.textureAtlasId = textureAtlasId;
            this.normalAtlasId = normalAtlasId;

            //this.baseMapFilename = baseMapFilename;
            this.baseMapFilename = "map-dev";

            layers = new Dictionary<LayerType, Dictionary<Vector2, int>>((int)LayerType.Count);
        }   

        // load in all layers from map folder
        public void Load()
        {
            for (LayerType layer = 0; layer < LayerType.Count; layer++)
            {
                LoadLayer(layer, filepathPrefix + baseMapFilename + "/" + baseMapFilename + "_" + GetLayerName(layer) + ".csv");
            }
        }
        

        public Dictionary<Vector2, int> GetLayer(LayerType layerType)
        {
            if (layers.ContainsKey(layerType))
            {
                return layers[layerType];
            }
            throw new Exception("Tilemap:GetLayer(LayerType layerType) -> layer is null");
        }


        private void LoadLayer(LayerType layerType, string filepath)
        {
            if (!layers.ContainsKey(layerType))
            {
                layers.Add(layerType, LoadLayerFile(filepath));
            }
        }

        
        private string GetLayerName(LayerType layerType)
        {
            if (layerNames.ContainsKey(layerType)) {
                return layerNames[layerType];
            }
            throw new Exception("Tilemap:GetLayerName(LayerType layerType) -> layerNames Dictionary does not contains given LayerType");
        }


        private Dictionary<Vector2, int> LoadLayerFile(string filepath)
        {
            Dictionary<Vector2, int> result = new();
            StreamReader reader = new(filepath);
            int y = 0;
            string line;

            while((line = reader.ReadLine()) != null)
            {
                string[] items = line.Split(',');
                for (int x = 0; x < items.Length; x++)
                {
                    if (int.TryParse(items[x], out int value)) 
                    {
                        if (value > -1)
                        {
                            result[new Vector2(x, y)] = value;
                        }
                    }
                }
                y++;
            }
            return result;
        }


        


        



    }
}


