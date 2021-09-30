using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Definit.Common
{
    public class EventDebouncer
    {
        private readonly Timer _timer;
        private readonly Action _actionToRun;
        private readonly TimeSpan _debounceTime;

        public EventDebouncer(Action actionToRun, TimeSpan debounceTime)
        {
            this._actionToRun = actionToRun;
            this._debounceTime = debounceTime;
            _timer = new Timer(TimerCallback, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        public void HandleEvent()
        {
            //Only trigger once (no periodic repeat)
            _timer.Change(_debounceTime, Timeout.InfiniteTimeSpan);
        }

        public void HandleEvent(object sender, EventArgs eventArgs)
        {
            HandleEvent();
        }

        private void TimerCallback(object state)
        {
            //Stop timer, will be started when action is received
            _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            _actionToRun();
        }
    }
}
