using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasurementDataBuffer
{
    public interface IMeasurementData
    {
        // Example properties not mentioned in the task
        DateTime Timestamp { get; }
        double Value { get; }
    }

    public interface IDao
    {
        bool SaveMeasurementData(IMeasurementData measurement);
    }

    internal class MeasurementDataBuffer
    {
        private readonly IDao? _dao;
        private readonly BlockingCollection<IMeasurementData> _buffer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dao"></param>
        public MeasurementDataBuffer(IDao dao)
        {
            _dao = dao ?? throw new ArgumentNullException(nameof(dao));
            _buffer = new BlockingCollection<IMeasurementData>(new ConcurrentQueue<IMeasurementData>());
        }

        /// <summary>
        /// Add a single measurement to the buffer
        /// </summary>
        /// <param name="measurement"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddMeasurement(IMeasurementData measurement)
        {
            if (measurement == null) throw new ArgumentNullException(nameof(measurement));

            _buffer.Add(measurement);
        }

        /// <summary>
        /// Add multiple measurements to the buffer
        /// </summary>
        /// <param name="measurements"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddMeasurements(List<IMeasurementData> measurements)
        {
            if (measurements == null) throw new ArgumentNullException(nameof(measurements));

            foreach (var measurement in measurements)
            {
                _buffer.Add(measurement);
            }
        }

        /// <summary>
        /// Clear all buffered data
        /// </summary>
        public void ClearBuffer()
        {
            while (_buffer.TryTake(out _)) { }
        }

        public void Dispose()
        {
            // ToDo: implement IDisposable interface
        }

        private async Task PersistBuffer()
        {
            // ToDo: implement logic to move measurements to "storage"
        }
    }
}
