namespace GameDomainModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    #region data mocks

    public static class Game
    {
        private const int MaximumRacesLimit = 10;
        private static int _currentRaceCount = 3;

        public static object Player
        {
            get { return Game._player; }
        }
        private static object _player = null;

        public static string RaceTitle { get; set; } = "What a race!";
    }

    public class Sedan
        : Vehicle
    {
        public Sedan(int maxSpeedMph)
            : base(maxSpeedMph)
        {

        }

        public enum SedanType
        {
            RaceCar,
            PassengerCar,
            ServiceCar,
        }

        public SedanType Type { get; set; } = SedanType.PassengerCar;

        public static string VehicleType { get; } = nameof(Sedan);

    }

    /// <summary>
    /// Abstract Vehicle base class.
    /// </summary>
    public abstract class Vehicle
    {
        private Vehicle()
        {

        }

        protected Vehicle(int maxSpeedMph)
        {
            this._maxSpeedMph = maxSpeedMph;
        }

        private const int AGE_LIMIT_YEARS = 10;
        private readonly int _maxSpeedMph = 0;

        // 3
        public object Tag
        {
            get { return this._tag; }
        }
        private object _tag = null;

        public string Brand { get; set; } = "Vehicle Brand";
        public string Model { get; set; } = "Vehicle Model";

        public void Start()
        {
            //let's simulate an exception:
            throw new Exception("Engine overheated!");
        }

    }

    #endregion data mocks
}
