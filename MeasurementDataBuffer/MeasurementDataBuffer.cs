using System;
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
    }
}
