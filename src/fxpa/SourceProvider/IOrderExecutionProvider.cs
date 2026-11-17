// -----
// MIT License
//
// Copyright (c) 2005-2026 Harry Ho
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// -----

using System;
using System.Collections.Generic;
using System.Text;

using System.Collections.ObjectModel;

namespace fxpa
{
    public interface IOrderExecutionProvider 
    {
        string Symbol { get; }

        /// <summary>
        /// Orders are expected to be provided in chronological order.
        /// </summary>
        ReadOnlyCollection<Order> Orders { get; }

        //OrderExecutionAccount Account { get; }

        //AccountInfo GetAccountInfo();

        /// <summary>
        /// This is where time control how be exercised on a provider. 
        /// If provider does not support time control, member is null.
        /// </summary>
        ITimeControl TimeControl { get; }

        event GeneralHelper.GenericDelegate<Order> OrderAddedEvent;
        event GeneralHelper.GenericDelegate<Order> OrderUpdatedEvent;
        event GeneralHelper.GenericDelegate<Order> OrderRemovedEvent;

        bool OpenOrder(Order order, OrderTypeEnum orderType, double volume, double allowedSlippage, double desiredPrice,
            double takeProfit, double stopLoss, string comment, out double openingPrice, out DateTime openingTime, out string operationResultMessage);

        /// <summary>
        /// Data Provider needed, to initialize the newly obtained orders with it.
        /// </summary>
        bool ObtainAllOrders(out string operationResultMessage);

        bool DecreaseOrderVolume(Order order, double volumeDecreasal, double allowedSlippage, double desiredPrice, out double decreasalPrice, out string operationResultMessage);

        bool CloseOrder(Order order, double allowedSlippage, double desiredPrice, out double closingPrice, out DateTime closingTime, out string operationResultMessage);

        bool Initialize(AnalyzerSession session);

        void UnInitialize();
    }
}
