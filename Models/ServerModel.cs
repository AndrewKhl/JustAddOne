using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JustAddOne.Models
{
    public sealed class ServerModel : IDisposable
    {
        private const int FilterDataDelay = 200;
        private const int CalculationDataDelay = 200;
        private const int MaxStringLength = 100;

        private static readonly ServerModel _serverModel = new ServerModel();
        private readonly ConcurrentQueue<string> _rawData;
        private readonly ConcurrentQueue<string> _filteredData;

        private readonly LongArithetic _LA;

        private bool _serverRun;

        public static ServerModel Instance => _serverModel;

        public string Number => _LA.GetNumber();


        private ServerModel()
        {
            _LA = new LongArithetic(MaxStringLength);
            _rawData = new ConcurrentQueue<string>();
            _filteredData = new ConcurrentQueue<string>();

            _serverRun = true;

            ThreadPool.QueueUserWorkItem(FilterUserData);
            ThreadPool.QueueUserWorkItem(CalculationUserData);
        }

        public void AddValue(string value)
        {
            _rawData.Enqueue(value);
        }

        private async void FilterUserData(object obj)
        {
            while (_serverRun)
            {
                if (_rawData.Count > 0 && _rawData.TryDequeue(out var rawItem))
                {
                    if (string.IsNullOrEmpty(rawItem) || rawItem.Length > MaxStringLength || !rawItem.All(char.IsNumber))
                        continue;

                    _filteredData.Enqueue(rawItem);
                }
                else
                    await Task.Delay(FilterDataDelay);
            }
        }

        private async void CalculationUserData(object obj)
        {
            while (_serverRun)
            {
                if (_filteredData.Count > 0 && _filteredData.TryDequeue(out var data))
                    _LA.AddNumber(data);
                else
                    await Task.Delay(CalculationDataDelay);
            }
        }

        public void Dispose()
        {
            _serverRun = false;
        }


        private sealed class LongArithetic
        {
            private readonly List<int> _totalNumber;
            private readonly int _baseLength;
            public LongArithetic(int baseStringLength)
            {
                _baseLength = baseStringLength;
                _totalNumber = new List<int>(baseStringLength * 10);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AddNumber(string str)
            {
                if (str.Length > _baseLength || str.Length == 0)
                    return;

                int carry = 0;

                for (int i = 0; i < _totalNumber.Capacity; ++i)
                {
                    if (_totalNumber.Count - 1 < i)
                        _totalNumber.Add(0);

                    _totalNumber[i] += carry + (i < str.Length ? (str[str.Length - i - 1] - '0') : 0);
                    carry = _totalNumber[i] / 10;
                    _totalNumber[i] %= 10;

                    if (carry == 0 && i >= str.Length - 1)
                        break;
                }

                if (_totalNumber.Count >= _totalNumber.Capacity - 2)
                    _totalNumber.AddRange(Enumerable.Repeat(0, _baseLength));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public string GetNumber()
            {
                if (_totalNumber.Count != 0)
                {
                    var answer = new StringBuilder(new string('0', _totalNumber.Count));

                    for (int i = 0; i < _totalNumber.Count; ++i)
                        answer[i] = (char)(_totalNumber[_totalNumber.Count - i - 1] + '0');

                    return answer.ToString();
                }
                return "0";
            }
        }
    }
}
