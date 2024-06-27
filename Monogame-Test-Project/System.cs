using bitmask;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using tilemap;
using viewStuff;
using resource;
using Microsoft.Xna.Framework.Content;
using System.Security.Cryptography;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using static tilemap.Tilemap;


namespace ECS.Systems
{
    // base class for systems which are Constantly Updated
    public abstract class UpdateSystem
    {
        public abstract void Update(GameTime gameTime);
        
    }



    /*
    Manages animated entities by switching between their animations appropriately
    */
    public class AnimationSystem : UpdateSystem
    {
        private EntityManager eMan;
        private Bitmask signature;
        private float tickRate = 0.2f;
        private float lastTick = 0.0f;

        public AnimationSystem(EntityManager eMan)
        {
            this.eMan = eMan;
            signature = new Bitmask((int)ComponentType.Count);
            signature[ComponentType.CAnimation] = true;
            signature[ComponentType.CTransform] = true;
        }

        public override void Update(GameTime gameTime)
        {
            float curTime = (float)gameTime.TotalGameTime.TotalSeconds;
            if (curTime - lastTick > tickRate)
            {
                lastTick = curTime;
                List<Entity> ents = eMan.GetEntities(signature).ToList();
                foreach(var e in ents)
                {
                    CAnimation anim = (CAnimation)eMan.GetComponent<CAnimation>(e.id);
                    anim.frame += 1;
                    if (anim.frame > anim.numFrames)
                    {
                        anim.frame = 0;
                    }
                }
            }

        }
    }


    /*
    
    Lighting System

    * the lighting  system will basically allow for seamless connection of the lights
      in the ecs to the shader present in the renderer
    * the tasks it will complete include 
        1. gathering the correct light information from the ecs
        2. setting the shader parameters with those values
        3. handle light count overflow and underflow for the renderer
           * i.e., because the shader uses static values for array sizes, 
             only a const amount of lights are allowed to be sent to the shader.
             This system will take care of ensuring correct array values
             are set.
    * in layman's terms, the lighting system will only be in charge of sending lighting
      information from the ECS to the shader, as the task is different from the purpose
      of the renderer itself and requires a seperate, decoupled system to manage it.
    * for now the lighting system has hard coded values for the shader and what not, I believe this will
      probably stay this way for the forseeable future.
    */


    public class LightingSystem
    {
        private EntityManager eMan;
        private Bitmask signature;
        private int maxNumLights;

        private EffectParameter effectParamPointLightPositions;
        private EffectParameter effectParamPointLightColors;
        private EffectParameter effectParamPointLightRadii;
        private EffectParameter effectParamAmbientLightColor;

        private Vector3[] pointLightPositions;
        private Vector3[] pointLightColors;
        private float[] pointLightRadii;
        private Vector3 ambientLightColor;
        
        public LightingSystem(EntityManager eMan, int maxNumLights, Effect pixelShader)
        {
            this.eMan = eMan;

            signature = new Bitmask((int)ComponentType.Count);
            signature[ComponentType.CPointLight] = true;
            signature[ComponentType.CTransform] = true;

            this.maxNumLights = maxNumLights;
            
            effectParamPointLightPositions = pixelShader.Parameters["PointLightPositions"];
            effectParamPointLightColors = pixelShader.Parameters["PointLightColors"];
            effectParamPointLightRadii = pixelShader.Parameters["PointLightRadii"];
            effectParamAmbientLightColor = pixelShader.Parameters["AmbientLightColor"];

            pointLightPositions = new Vector3[maxNumLights];
            pointLightColors = new Vector3[maxNumLights];
            pointLightRadii = new float[maxNumLights];
            ambientLightColor = new Vector3(0.3f);
        }

        public void SetShaderParameters(Camera2D cam)
        {
            List<Entity> ents = eMan.GetEntities(signature).ToList();
            CPointLight thisLight = null;
            CTransform thisTrans = null;
            // gather component values
            for (int i = 0; i < ents.Count && i < maxNumLights; i++)
            {
                thisTrans = (CTransform)eMan.GetComponent<CTransform>(ents[i].id);
                thisLight = (CPointLight)eMan.GetComponent<CPointLight>(ents[i].id);

                pointLightPositions[i] = new Vector3(cam.worldToScreen(thisTrans.position + thisLight.offset), 10);
                pointLightColors[i] = thisLight.color;
                pointLightRadii[i] = thisLight.radius * cam.Zoom;
            }
            // set shader parameters
            effectParamPointLightPositions.SetValue(pointLightPositions);
            effectParamPointLightColors.SetValue(pointLightColors);
            effectParamPointLightRadii.SetValue(pointLightRadii);
            effectParamAmbientLightColor.SetValue(ambientLightColor);
        }
    }


    /*
    The tilemap system handles bridging the gap between 
    entities and the tilemap. This will handle collisions with 
    collidable objects stored in the tilemap, detecting and responding
    to the different types of tiles that the entity might stand on,
    and whether or not an entity is in lava or water for example.
    */
    public class TilemapManager
    {
        private EntityManager eMan;
        private Bitmask signature;

        public Tilemap tilemap;

        private Dictionary<Vector2, int> collisionLayer;

        public TilemapManager(EntityManager eMan, Tilemap tilemap)
        {
            this.eMan = eMan;
            this.tilemap = tilemap;
            this.tilemap.Load();
            
            this.collisionLayer = tilemap.GetLayer(LayerType.collision);
        }

        
        // takes a world position and gets the tile it resides in
        public Rectangle GetTileRect(Vector2 pos)
        {
            Vector2 tileCoord = WorldToTile(pos);

            return new Rectangle((int)tileCoord.X * tilemap.tileDim, (int)tileCoord.Y * tilemap.tileDim, tilemap.tileDim, tilemap.tileDim);
        }


        // takes a tilemap and manages transferring the midground into the ECS
        public void ProcessMap()
        {
            throw new NotImplementedException();

            //var layer = tilemap.GetLayer(Tilemap.LayerType.midground);
            //if (layer == null)
            //{
            //    throw new Exception("TilemapSystem:Init(Tilemap tilemap) -> tilemap.GetLayer(Tilemap.LayerType.midground) is null");
            //}

            //foreach (var item in layer) {
            //    if (item.Value > 0)
            //    {
            //        Entity e = eMan.CreateEntity();
            //        eMan.AddComponent<CTransform>(e, new CTransform(item.Key));
            //        eMan.AddComponent<CCollider>(e, new CRectCollider(new Vector2(32f, 32f)));
            //        eMan.AddComponent<CTexture>(e, new CTexture("brick"));
            //        eMan.AddComponent<CRigidBody>(e, new CRigidBody(10f));
            //    }
            //}
        }


        public Vector2 WorldToTile(Vector2 pos)
        {
            return new Vector2((int)(pos.X / tilemap.tileDim),
                (int)(pos.Y / tilemap.tileDim));
        }


        // checks if an object in world space is colliding with a tile
        public bool IsSolidAt(Vector2 pos)
        {
            if (tilemap == null)
            {
                throw new Exception("TilemapManager:IsSolidAt(Vector2 pos) -> TilemapManager.tilemap is null");
            }
            Vector2 tilePos = WorldToTile(pos);
            if (collisionLayer.ContainsKey(tilePos))
            {
                if (collisionLayer[tilePos] > 0)
                {
                    return true;
                }
            }
            return false;
        }


    }



    // (Still not sure if this class is the best idea, might be over doing it system wise here)
    /**
     * Action System
     * 
     * Uses information in controller components and translates it
     * into actions for entites.
     */
    public class ActionSystem : UpdateSystem
    {
        private EntityManager eMan;
        private Bitmask signature;

        public ActionSystem(EntityManager eMan)
        {
            this.eMan = eMan;
            signature = new Bitmask((int)ComponentType.Count);
            signature[ComponentType.CController] = true;
            signature[ComponentType.CTransform] = true;
            signature[ComponentType.CRigidBody] = true;
        }


        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            List<Entity> entities = eMan.GetEntities(signature).ToList();
            foreach (Entity e in entities)
            {
                CController cont = (CController)eMan.GetComponent<CController>(e.id);
                CRigidBody rig = (CRigidBody)eMan.GetComponent<CRigidBody>(e.id);
                CTransform trans = (CTransform)eMan.GetComponent<CTransform>(e.id);

                UpdatePlayer(cont, rig, trans);
            }
        }

        private void UpdatePlayer(CController cont, CRigidBody rig, CTransform trans)
        {
            if (rig == null)
            {
                throw new Exception("MovementSystem.Update: rigidbody null");
            }

            // for now, all players have the same movement speed
            float moveSpeed = 20f; // 20f
                                   //rig.velocity += cont.movement * moveSpeed;

            rig.acceleration += cont.leftStick * moveSpeed;

            // limit player speed gained by input this way
            //rig.velocity += cont.movement * moveSpeed * dt;
            rig.acceleration += rig.velocity * -0.1f; // -0.2f // -0.06f;

            if (cont.rightStick.Length() > 0.5f)
            {
                float theta = (float)Math.Atan2(cont.rightStick.Y, cont.rightStick.X);
                trans.rotation = theta;
            }
            
            
        }

    }

}

