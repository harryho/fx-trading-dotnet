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


namespace fxpa
{
    [Serializable]
    public class OrderInformation
    {
        string _orderPlatformId;
        public string OrderPlatformId
        {
            get { return _orderPlatformId; }
        }

        OrderTypeEnum _orderType;
        public OrderTypeEnum OrderType
        {
            get { return _orderType; }
        }

        double _volume;
        public double Volume
        {
            get { return _volume; }
        }

        double _openPrice;
        public double OpenPrice
        {
            get { return _openPrice; }
        }

        double _closePrice;
        public double ClosePrice
        {
            get { return _closePrice; }
        }

        double _orderStopLoss;
        public double OrderStopLoss
        {
            get { return _orderStopLoss; }
        }

        double _orderTakeProfit;
        public double OrderTakeProfit
        {
            get { return _orderTakeProfit; }
        }

        /// <summary>
        /// Not including swaps or commissions.
        /// </summary>
        double _currentProfit;
        public double CurrentProfit
        {
            get { return _currentProfit; }
        }

        double _orderSwap;
        public double OrderSwap
        {
            get { return _orderSwap; }
        }

        string _orderSymbol;
        public string OrderSymbol
        {
            get { return _orderSymbol; }
        }

        DateTime _sourceCloseTime;
        public DateTime SourceCloseTime
        {
            get { return _sourceCloseTime; }
        }

        DateTime _sourceOpenTime;
        public DateTime SourceOpenTime
        {
            get { return _sourceOpenTime; }
        }

        DateTime _orderExpiration;
        public DateTime OrderExpiration
        {
            get { return _orderExpiration; }
        }

        double _orderCommission;
        public double OrderCommission
        {
            get { return _orderCommission; }
        }

        string _orderComment;
        public string OrderComment
        {
            get { return _orderComment; }
        }

        /// <summary>
        /// This is also called the order "magic number" in MT4.
        /// </summary>
        int _orderMagicNumber;
        public int OrderMagicNumber
        {
            get { return _orderMagicNumber; }
        }

        bool _isOpen;
        public bool IsOpen
        {
            get { return _isOpen; }
        }

        /// <summary>
        /// 
        /// </summary>
        public OrderInformation(string orderPlatformId, string orderSymbol, int orderType, double volume, double openPrice, double closePrice,
            double orderStopLoss, double orderTakeProfit, double currentProfit, double orderSwap, int orderPlatformOpenTime, int orderPlatformCloseTime,
            int orderExpiration, double orderCommission, string orderComment, int orderMagicNumber) 
        {
            _orderPlatformId = orderPlatformId;
            _orderSymbol = orderSymbol;
            _orderType = (OrderTypeEnum)orderType;
            _volume = volume;
            _openPrice = openPrice;
            _closePrice = closePrice;
            _orderStopLoss = orderStopLoss;
            _orderTakeProfit = orderTakeProfit;
            _currentProfit = currentProfit;
            _orderSwap = orderSwap;
            _sourceOpenTime = GeneralHelper.GenerateDateTimeSecondsFrom1970(orderPlatformOpenTime);

            //According to documentataion this is the way to establish if order is closed, see here : http://docs.mql4.com/trading/OrderSelect
            _isOpen = orderPlatformCloseTime == 0;

            _sourceCloseTime = GeneralHelper.GenerateDateTimeSecondsFrom1970(orderPlatformCloseTime);
            _orderExpiration = GeneralHelper.GenerateDateTimeSecondsFrom1970(orderExpiration);
            _orderCommission = orderCommission;
            _orderComment = orderComment;
            _orderMagicNumber = orderMagicNumber;
        }

        public string Print()
        {
            return "orderTicket:[" + _orderPlatformId + "] " +
            "orderSymbol:[" + _orderSymbol + "] " +
            "orderType:[" + _orderType + "] " +
            "volume:[" + _volume + "] " +
            "openPrice:[" + _openPrice + "] " +
            "closePrice:[" + _closePrice + "] " +
            "orderStopLoss:[" + _orderStopLoss + "] " +
            "orderTakeProfit:[" + _orderTakeProfit + "] " +
            "currentProfit:[" + _currentProfit + "] " +
            "orderSwap:[" + _orderSwap + "] " +
            "platformOpenTime :[" + _sourceOpenTime + "] " +
            "platformCloseTime :[" + _sourceCloseTime + "] " +
            "orderExpiration :[" + _orderExpiration + "] " +
            "orderCommission:[" + _orderCommission + "] " +
            "orderComment:[" + _orderComment + "] " +
            "orderMagicNumber:[" + _orderMagicNumber + "]";

        }

    }
}
