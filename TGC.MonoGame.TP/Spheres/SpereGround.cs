namespace TGC.MonoGame.TP.Spheres {

    public class SphereGround : SphereType {
        
        public string folder(){
            return "ground/";
        }

        public float jump() {
            return 1500;
        }

        public float speed() {
            return 100f;
        }


    }

}