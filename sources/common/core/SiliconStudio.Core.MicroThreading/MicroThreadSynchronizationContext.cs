﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading;

namespace SiliconStudio.Core.MicroThreading
{
    public class MicroThreadSynchronizationContext : SynchronizationContext
    {
        internal MicroThread MicroThread;

        public MicroThreadSynchronizationContext(MicroThread microThread)
        {
            this.MicroThread = microThread;
        }

        public override SynchronizationContext CreateCopy()
        {
            return this;
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            // There is two case:
            // 1/ We are either in normal MicroThread inside Scheduler.Step() (CurrentThread test),
            // in which case we will directly execute the callback to avoid further processing from scheduler.
            // Also, note that Wait() sends us event that are supposed to come back into scheduler.
            // Note: As it will end up on the callstack, it might be better to Schedule it instead (to avoid overflow)?
            // 2/ Otherwise, we just received an external task continuation (i.e. TaskEx.Sleep()), or a microthread triggering another,
            // so schedule it so that it comes back in our regular scheduler.
            if (MicroThread.Scheduler.RunningMicroThread == MicroThread)
            {
                d(state);
            }
            else if (MicroThread.State == MicroThreadState.Completed)
            {
                throw new InvalidOperationException("MicroThread is already completed but still posting continuations.");
            }
            else
            {
                MicroThread.ScheduleContinuation(MicroThread.ScheduleMode, d, state);
            }
        }
    }
}