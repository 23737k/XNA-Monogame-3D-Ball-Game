namespace TGC.MonoGame.TP.Spheres {

    public class SpherePlastic : SphereType {
        
        public string folder(){
            return "plastic/";
        }

        public float jump() {
            return 1500;
        }

        public float speed() {
            return 70;
        }


    }

}