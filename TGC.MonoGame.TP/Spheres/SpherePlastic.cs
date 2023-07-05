using Microsoft.Xna.Framework.Graphics;
namespace TGC.MonoGame.TP.Spheres {

    public class SpherePlastic : SphereType {
        public Texture2D Ao {get;set;}
        public Texture2D Color {get;set;}
        public Texture2D Metalness {get;set;}
        public Texture2D Normal {get;set;}
        public Texture2D Roughness {get;set;}
        public string Name {get;} = "PLASTIC BALL"; 
        public string folder(){
            return "plastic/";
        }

        public float jump() {
            return 1200;
        }

        public float speed() {
            return 70;
        }


    }

}