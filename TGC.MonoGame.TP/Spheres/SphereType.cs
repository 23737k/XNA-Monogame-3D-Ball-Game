using Microsoft.Xna.Framework.Graphics;
namespace TGC.MonoGame.TP {

    public interface SphereType {
        public Texture2D Ao {get;set;}
        public Texture2D Color {get;set;}
        public Texture2D Metalness {get;set;}
        public Texture2D Normal {get;set;}
        public Texture2D Roughness {get;set;}
        public string Name {get;}

        public string folder();

        public float jump();

        public float speed();
    }

}