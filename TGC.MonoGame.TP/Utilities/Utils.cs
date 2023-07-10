using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Geometries;
using System.Collections.Generic;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using TGC.MonoGame.TP.Physics.Bepu;
using TGC.MonoGame.TP.MapObjects;
using TGC.MonoGame.TP.Spheres;
using NumericVector3 = System.Numerics.Vector3;
using TGC.MonoGame.TP.PBR;
using TGC.MonoGame.TP.Checkpoints;
using System.Linq;

namespace TGC.MonoGame.TP
{
    public class Utils
    {
        public static NumericVector3 ToNumericVector3(Vector3 v)
        {
            return new NumericVector3(v.X, v.Y, v.Z);
        }
        public static System.Numerics.Quaternion ToSysNumQuaternion(Quaternion v)
        {
            return new System.Numerics.Quaternion(v.X, v.Y, v.Z,v.W);
        }


        public static void SetEffect(Camera camera, Effect effect, Matrix world)
        {
            effect.Parameters["eyePosition"]?.SetValue(camera.Position);
            effect.Parameters["Tiling"]?.SetValue(Vector2.One);
            effect.Parameters["World"]?.SetValue(world);
            effect.Parameters["WorldViewProjection"].SetValue(world  * camera.View * camera.Projection);
            effect.Parameters["InverseTransposeWorld"]?.SetValue(Matrix.Transpose(Matrix.Invert(world)));
        }
        public static void SetEffect(Camera camera, Effect effect, Matrix world, Texture2D texture, Texture2D normal)
        {
            effect.Parameters["ModelTexture"].SetValue(texture);
            effect.Parameters["NormalTexture"].SetValue(normal);
            effect.Parameters["eyePosition"]?.SetValue(camera.Position);
            effect.Parameters["Tiling"]?.SetValue(Vector2.One);
            effect.Parameters["World"].SetValue(world);
            effect.Parameters["WorldViewProjection"].SetValue(world  * camera.View* camera.Projection);
            effect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Transpose(Matrix.Invert(world)));
        }
    }
}