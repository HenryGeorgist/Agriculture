namespace Agriculture.Crops
{
    public class CropLocation : Interfaces.ICropLocation
    {
        private double _X;
        private double _Y;
        public int CropID { get; set; }
        public string CropName { get; set; }
        public double X
        {
            get
            {
               return _X;
            }

            set
            {
               _X = value;
            }
        }

        public double Y
        {
            get
            {
                return _Y;
            }

            set
            {
                _Y = value;
            }
        }
    }
}
