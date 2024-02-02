using System.Collections.Concurrent;
using FakeXrmEasy.Core.CommercialLicense;

namespace FakeXrmEasy.Core.Tests.CommercialLicense
{
    public class FakeEnvironmentReader : IEnvironmentReader
    {
        private readonly ConcurrentDictionary<string, string> _variables;

        public FakeEnvironmentReader()
        {
            _variables = new ConcurrentDictionary<string, string>();
        }
        
        public string GetEnvironmentVariable(string variableName)
        {
            string variableValue = "";
            var exists = _variables.TryGetValue(variableName, out variableValue);
            if (!exists)
            {
                return null;
            }

            return variableValue;
        }

        public void SetEnvironmentVariable(string variableName, string variableValue)
        {
            _variables.AddOrUpdate(variableName, variableValue, (key, oldValue) => variableValue);
        }
    }
}