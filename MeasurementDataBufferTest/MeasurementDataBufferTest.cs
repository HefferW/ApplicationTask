using MeasurementDataBuffer;

namespace MeasurementDataBufferTest
{
    public class MeasurementDataBufferTest
    {
        /// <summary>
        /// Example implementation of IMeasurementData for xUnit tests
        /// </summary>
        internal class Temperature : IMeasurementData
        {
            public DateTime Timestamp { get; }
            public double Value { get; }

            public Temperature(double value)
            {
                Timestamp = DateTime.Now;
                Value = value;
            }

            public override string ToString()
            {
                return $"[{Timestamp:HH:mm:ss.fff}] Value: {Value}";
            }
        }

        /// <summary>
        /// Example implementation of IDao for xUnit tests to verify
        /// data transmission from MeasurementDataBuffer to storage
        /// </summary>
        public class DaoTest : IDao
        {
            public List<IMeasurementData> storage = new List<IMeasurementData>();
            public bool SaveMeasurementData(IMeasurementData measurement)
            {
                if (measurement == null) throw new ArgumentNullException(nameof(measurement));
                storage.Add(measurement);
                Thread.Sleep(100); // Simulate slow database
                return true;
            }
        }

        [Fact]
        public void Test1()
        {

        }
    }
}