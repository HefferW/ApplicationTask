using System.Collections.Concurrent;

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

    internal class MeasurementDataBuffer : IDisposable
    {
        private readonly IDao? _dao;
        private readonly BlockingCollection<IMeasurementData> _buffer;
        private readonly CancellationTokenSource _cts;
        private readonly Task _processingTask;
        private bool _disposed = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dao"></param>
        public MeasurementDataBuffer(IDao dao)
        {
            _dao = dao ?? throw new ArgumentNullException(nameof(dao));
            _buffer = new BlockingCollection<IMeasurementData>(new ConcurrentQueue<IMeasurementData>());
            _cts = new CancellationTokenSource();
            _processingTask = Task.Run((PersistBuffer));
        }

        /// <summary>
        /// Add a single measurement to the buffer
        /// </summary>
        /// <param name="measurement"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddMeasurement(IMeasurementData measurement)
        {
            if (measurement == null) throw new ArgumentNullException(nameof(measurement));
            if (_disposed) throw new ObjectDisposedException(nameof(MeasurementDataBuffer));

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
            if (_disposed) throw new ObjectDisposedException(nameof(MeasurementDataBuffer));

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
            if (_disposed) throw new ObjectDisposedException(nameof(MeasurementDataBuffer));

            while (_buffer.TryTake(out _)) { }
        }

        /// <summary>
        /// Dispose ressources
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _cts.Cancel(); // Signal cancellation
            _buffer.CompleteAdding(); // Mark buffer as complete

            try
            {
                _processingTask.Wait(); // Wait for background task to finish
            }
            catch (AggregateException ex) when (ex.InnerException is OperationCanceledException)
            {
                // Expected cancellation
            }

            _buffer.Dispose();
            _cts.Dispose();
            _disposed = true;
        }

        /// <summary>
        /// Write buffer to storage
        /// </summary>
        /// <returns>Task</returns>
        private async Task PersistBuffer()
        {
            try
            {
                foreach (var data in _buffer.GetConsumingEnumerable(_cts.Token))
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            _dao.SaveMeasurementData(data);
                        }
                        catch
                        {
                            // Optional: handle or log individual save failures
                        }
                    }, _cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // Proper shutdown
            }
        }
    }
}
