using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Intelligent.OTC.WebApi.Core
{
    public class ODataMetadata<T> where T : class
    {
        private readonly long? _count;
        private IEnumerable<T> _result;

        public ODataMetadata(IEnumerable<T> result, long? count)
        {
            _count = count;
            _result = result;
        }

        public IEnumerable<T> Results
        {
            get { return _result; }
        }

        public long? Count
        {
            get { return _count; }
        }
    }

    public class ODataMetadata
    {
        private readonly long? _count;
        private IEnumerable _result;

        public ODataMetadata(IEnumerable result, long? count)
        {
            _count = count;
            _result = result;
        }

        public IEnumerable Results
        {
            get { return _result; }
        }

        public long? Count
        {
            get { return _count; }
        }
    }
}
