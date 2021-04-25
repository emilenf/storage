﻿#region License
// Copyright (c) 2021 Jake Fowler
//
// Permission is hereby granted, free of charge, to any person 
// obtaining a copy of this software and associated documentation 
// files (the "Software"), to deal in the Software without 
// restriction, including without limitation the rights to use, 
// copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following 
// conditions:
//
// The above copyright notice and this permission notice shall be 
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR 
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cmdty.Storage.Excel
{
    public enum CalcStatus
    {
        Running,
        Error,
        Success,
        Cancelled
    }
    public abstract class ExcelCalcWrapper
    {
        public event Action<double> OnProgressUpdate;
        public double Progress { get; protected set; }
        public Task CalcTask { get; protected set; }
        public object Results { get; protected set; } // TODO use generic Task<TResult>?
        public CalcStatus Status { get; protected set; }
        protected CancellationTokenSource _cancellationTokenSource;

        protected ExcelCalcWrapper()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        protected void UpdateProgress(double progress)
        {
            // TODO some sort of synchonisation needed? Look online.
            Progress = progress;
            OnProgressUpdate?.Invoke(progress);
        }

        public void Cancel()
            => _cancellationTokenSource.Cancel();


        public abstract bool CancellationSupported();

        protected void UpdateStatus(Task task)
        {
            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    Status = CalcStatus.Success;
                    break;
                case TaskStatus.Canceled:
                    Status = CalcStatus.Cancelled;
                    break;
                case TaskStatus.Faulted:
                    Status = CalcStatus.Error;
                    break;
                default:
                    throw new ApplicationException($"Task status {task.Status} not supported.");
            }
        }

    }

}
