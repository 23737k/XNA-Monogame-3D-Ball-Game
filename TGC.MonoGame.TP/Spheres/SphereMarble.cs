
using Microsoft.Xna.Framework.Graphics;
namespace TGC.MonoGame.TP {

    public class SphereMarble : SphereType {
        public Texture2D Ao {get;set;}
        public Texture2D Color {get;set;}
        public Texture2D Metalness {get;set;}
        public Texture2D Normal {get;set;}
        public Texture2D Roughness {get;set;}
        public string Name {get;} = "MARBLE BALL"; 

        public string folder(){
            return "marble/";
        }

        public float jump () {
            return 1000f;
        }

        public float speed() {
            return 80f;
        }

    }

}