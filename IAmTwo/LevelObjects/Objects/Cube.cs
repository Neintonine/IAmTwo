﻿using IAmTwo.Game;
using IAmTwo.LevelObjects.Objects.SpecialObjects;
using IAmTwo.Resources;
using OpenTK;

namespace IAmTwo.LevelObjects.Objects
{
    public class Cube : SpecialActor, IPlaceableObject
    {
        public Cube()
        {

            Name = "Cube";

            Passive = false;
            Transform.Size.Set(50);
            Transform.ZIndex.Set(20);
            Texture = Resource.RequestTexture(@".\Resources\MovingBox_d.png");

            ShaderArguments["EmissionTex"] = Resource.RequestTexture(@".\Resources\MovingBox_e.png", OpenTK.Graphics.OpenGL4.TextureMinFilter.Nearest);
            ShaderArguments["EmissionStrength"] = 1.75f;

            ChecksGrounded = true;

            Material.Blending = true;
        }

        public override void Collided(PhysicsObject obj, Vector2 mtv)
        {
            base.Collided(obj, mtv);

            switch (obj)
            {
                case SpecialObject special:
                    HandleCollision(special, mtv);
                    return;
                case Player p:
                    mtv.Y = 0;
                    break;
            }

            DefaultCollisionResolvement(obj, mtv);

            Velocity.X *= .9f;
        }

        public ScaleArgs AllowedScaling { get; } = ScaleArgs.NoScaling;
        public float AllowedRotationSteps { get; } = 0;
        public float? TriggerRotation { get; } = null;
        public string Category { get; } = "Special";
        public Vector2 StartSize { get; } = new Vector2(50);
    }
}