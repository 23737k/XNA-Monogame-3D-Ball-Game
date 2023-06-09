namespace TGC.MonoGame.TP {

    public class SphereMarble : SphereType {
        
        public string folder(){
            return "marble/";
        }

        public float jump () {
            return 800f;
        }

        public float speed() {
            return 120f;
        }

    }

}