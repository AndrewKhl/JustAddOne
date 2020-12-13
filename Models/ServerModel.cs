using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JustAddOne.Models
{
    public sealed class ServerModel : IDisposable
    {
        private const int FilterDataDelay = 50;
        private const int CalculationDataDelay = 50;
        private const int MaxStringLength = 1000;

        private static readonly ServerModel _serverModel = new ServerModel();
        private readonly ConcurrentQueue<string> _rawData;
        private readonly ConcurrentQueue<string> _filteredData;
        private readonly List<double> _countsRequests;

        private readonly LongArithetic _LA;
        private readonly Stopwatch _st;

        private int _currentSec = -1;
        private int _currentCount = 0;
        private bool _serverRun;

        public static ServerModel Instance => _serverModel;

        public string Number => _LA.GetNumber();


        private ServerModel()
        {
            _st = new Stopwatch();
            _LA = new LongArithetic(MaxStringLength);
            _rawData = new ConcurrentQueue<string>();
            _filteredData = new ConcurrentQueue<string>();
            _countsRequests = new List<double>(2000);

            _serverRun = true;

            ThreadPool.QueueUserWorkItem(FilterUserData);
            ThreadPool.QueueUserWorkItem(CalculationUserData);
        }


        public void AddValue(string value)
        {
        //    if (_currentSec != -1 && _currentSec != DateTime.UtcNow.Second)
        //    {
        //        _countsRequests.Add(_currentCount);
        //        _currentCount = 1;
        //    }
        //    else
        //        _currentCount++;

            _currentSec = DateTime.UtcNow.Second;
            _rawData.Enqueue(value);

            //if (_countsRequests.Count > 25)
            //    StorageModels.SaveResults(_countsRequests, "Metric2");
        }

        private async void FilterUserData(object obj)
        {
            while (_serverRun)
            {
                //_st.Restart();

                if (_rawData.Count > 0 && _rawData.TryDequeue(out var rawItem))
                {
                    if (string.IsNullOrEmpty(rawItem) || rawItem.Length > MaxStringLength || !rawItem.All(char.IsNumber))
                        continue;

                    _filteredData.Enqueue(rawItem);
                }
                else
                    await Task.Delay(FilterDataDelay);

                //_st.Stop();
                //_countsRequests.Add(_st.ElapsedMilliseconds);

                //if (_countsRequests.Count == 10000)
                //    StorageModels.SaveResults(_countsRequests, "Metric3");
            }
        }

        private async void CalculationUserData(object obj)
        {
            while (_serverRun)
            {
                //_st.Restart();

                if (_filteredData.Count > 0 && _filteredData.TryDequeue(out var data))
                    _LA.AddNumber(data);
                else
                    await Task.Delay(CalculationDataDelay);

                //_st.Stop();
                //_countsRequests.Add(_st.ElapsedMilliseconds);

                //if (_countsRequests.Count == 1000)
                //    StorageModels.SaveResults(_countsRequests, "Metric4");
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
