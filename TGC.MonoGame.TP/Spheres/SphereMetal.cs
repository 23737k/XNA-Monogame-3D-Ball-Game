using Microsoft.Xna.Framework.Graphics;
namespace TGC.MonoGame.TP {

    public class SphereMetal : SphereType {
        public Texture2D Ao {get;set;}
        public Texture2D Color {get;set;}
        public Texture2D Metalness {get;set;}
        public Texture2D Normal {get;set;}
        public Texture2D Roughness {get;set;}
        public string Name {get;} = "METAL BALL"; 
        public string folder(){
            return "metal1/";
        }

        public float jump() {
            return 950f;
        }

        public float speed() {
            return 110f;
        }


    }

}