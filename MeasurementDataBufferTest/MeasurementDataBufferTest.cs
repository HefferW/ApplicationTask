using MeasurementDataBuffer;

namespace MeasurementDataBufferT
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
        public async Task AddMeasurement_SingleMeasurement_ShouldBeSaved()
        {
            // Arrange
            var dao = new DaoTest();
            using var buffer = new MeasurementDataBuffer.MeasurementDataBuffer(dao);

            var temp = new Temperature(25.0);

            // Act
            buffer.AddMeasurement(temp);
            await Task.Delay(200); // Wait for processing

            // Assert
            Assert.Single(dao.storage);
            Assert.Equal(temp.Value, dao.storage[0].Value);
        }

        [Fact]
        public async Task AddMeasurements_Batch_ShouldSaveAll()
        {
            // Arrange
            var dao = new DaoTest();
            using var buffer = new MeasurementDataBuffer.MeasurementDataBuffer(dao);

            var batch = new List<IMeasurementData>
            {
                new Temperature(10.1),
                new Temperature(20.2),
                new Temperature(30.3)
            };

            // Act
            buffer.AddMeasurements(batch);
            await Task.Delay(500); // Wait for processing

            // Assert
            Assert.Equal(3, dao.storage.Count);
        }

        [Fact]
        public async Task ClearBuffer_ShouldPreventPendingMeasurements()
        {
            // Arrange
            var dao = new DaoTest();
            using var buffer = new MeasurementDataBuffer.MeasurementDataBuffer(dao);

            buffer.AddMeasurement(new Temperature(10.0));
            buffer.ClearBuffer();
            buffer.AddMeasurement(new Temperature(99.9));

            await Task.Delay(300); // Wait for processing

            // Assert
            Assert.Single(dao.storage);
            Assert.Equal(99.9, dao.storage[0].Value);
        }

        [Fact]
        public void AddMeasurement_Null_ShouldThrow()
        {
            // Arrange
            var dao = new DaoTest();
            using var buffer = new MeasurementDataBuffer.MeasurementDataBuffer(dao);

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => buffer.AddMeasurement(null));
        }

        [Fact]
        public void AddMeasurements_Null_ShouldThrow()
        {
            // Arrange
            var dao = new DaoTest();
            using var buffer = new MeasurementDataBuffer.MeasurementDataBuffer(dao);

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => buffer.AddMeasurements(null));
        }

        [Fact]
        public void AddMeasurement_AfterDispose_ShouldThrow()
        {
            // Arrange
            var dao = new DaoTest();
            var buffer = new MeasurementDataBuffer.MeasurementDataBuffer(dao);
            buffer.Dispose();

            // Act + Assert
            Assert.Throws<ObjectDisposedException>(() => buffer.AddMeasurement(new Temperature(12.3)));
        }
    }
}
